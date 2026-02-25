// 库存系统模块
// 管理物品的添加、删除、使用和查询

// 后备箱升级配置：等级 → { materials, bonusWeight }（升级不消耗金币）
const TRUNK_UPGRADES = {
  2: { materials: [{ id: "废金属", qty: 2 }], bonusWeight: 5 },
  3: { materials: [{ id: "废金属", qty: 4 }, { id: "铜线", qty: 2 }], bonusWeight: 5 },
  4: { materials: [{ id: "废金属", qty: 6 }, { id: "橡胶", qty: 3 }], bonusWeight: 5 },
  5: { materials: [{ id: "废金属", qty: 8 }, { id: "电池", qty: 2 }, { id: "铜线", qty: 4 }], bonusWeight: 10 },
};
const TRUNK_MAX_LEVEL = 5;

// 检查是否可以升级后备箱
function canUpgradeTrunk() {
  const level = inventoryState.trunkLevel || 1;
  if (level >= TRUNK_MAX_LEVEL) return false;
  const next = TRUNK_UPGRADES[level + 1];
  if (!next) return false;
  for (const mat of next.materials) {
    if (!hasItem(mat.id, mat.qty)) return false;
  }
  return true;
}

// 执行后备箱升级
function upgradeTrunk() {
  const level = inventoryState.trunkLevel || 1;
  if (level >= TRUNK_MAX_LEVEL) return false;
  const next = TRUNK_UPGRADES[level + 1];
  if (!next) return false;
  for (const mat of next.materials) {
    if (!hasItem(mat.id, mat.qty)) return false;
  }
  // 扣除材料消耗
  for (const mat of next.materials) {
    removeItem(mat.id, mat.qty);
  }
  // 提升等级与载重
  inventoryState.trunkLevel = level + 1;
  inventoryState.maxWeight += next.bonusWeight;
  // 刷新UI
  updateWeightDisplay();
  updateInventoryDisplay();
  if (typeof saveGame === "function") saveGame();
  // 文本区提示
  const textArea = document.getElementById("textArea");
  if (textArea) {
    const p = document.createElement("p");
    p.innerHTML = `<span style="color:#fbbf24">🔧 后备箱升级至 Lv.${inventoryState.trunkLevel}！最大载重 +${next.bonusWeight}kg → ${inventoryState.maxWeight}kg</span>`;
    textArea.appendChild(p);
    if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
  }
  return true;
}

// 获取当前库存总重量
function getInventoryWeight() {
  let totalWeight = 0;
  for (const slot of inventoryState.items) {
    const config = ITEMS_CONFIG[slot.id];
    if (config) {
      totalWeight += config.weight * slot.quantity;
    }
  }
  return totalWeight;
}

// 检查是否可以添加物品（重量检查）
function canAddItem(itemId, quantity = 1) {
  const config = ITEMS_CONFIG[itemId];
  if (!config) return false;
  const additionalWeight = config.weight * quantity;
  return getInventoryWeight() + additionalWeight <= inventoryState.maxWeight;
}

// 添加物品到库存（允许超载，超载判定在事件结束后统一进行）
function addItem(itemId, quantity = 1) {
  const config = ITEMS_CONFIG[itemId];
  if (!config) return false;

  const existing = inventoryState.items.find((slot) => slot.id === itemId);

  if (existing && config.stackable) {
    existing.quantity += quantity;
  } else if (existing && !config.stackable) {
    return false; // 不可堆叠且已存在
  } else {
    inventoryState.items.push({ id: itemId, quantity: quantity });
  }

  // 追踪「一次性管理员权限」历史峰值数量（用于程序故障惩罚触发条件）
  if (itemId === "一次性管理员权限" && typeof gameState !== "undefined") {
    const currentQty = getItemQuantity("一次性管理员权限");
    if (currentQty > (gameState.adminPermitPeakCount || 0)) {
      gameState.adminPermitPeakCount = currentQty;
    }
  }

  updateInventoryDisplay();
  return true;
}

// 移除物品
function removeItem(itemId, quantity = 1) {
  const index = inventoryState.items.findIndex((slot) => slot.id === itemId);
  if (index === -1) return false;

  const slot = inventoryState.items[index];
  if (slot.quantity < quantity) return false;

  slot.quantity -= quantity;
  if (slot.quantity <= 0) {
    inventoryState.items.splice(index, 1);
  }

  updateInventoryDisplay();
  return true;
}

// 检查是否拥有物品
function hasItem(itemId, quantity = 1) {
  const slot = inventoryState.items.find((s) => s.id === itemId);
  return slot ? slot.quantity >= quantity : false;
}

// 获取物品数量
function getItemQuantity(itemId) {
  const slot = inventoryState.items.find((s) => s.id === itemId);
  return slot ? slot.quantity : 0;
}

// 使用物品（消耗品效果）
function useItem(itemId) {
  const config = ITEMS_CONFIG[itemId];
  if (!config || (config.category !== "consumable" && !config.usable)) return false;
  if (!hasItem(itemId)) return false;

  // 特殊 useAction 处理（如时间存折：弹出时间银行取款弹窗）
  if (config.usable && config.useAction === "time_bank_withdraw") {
    if (typeof useTimeBankPassbook === "function") {
      useTimeBankPassbook(document.getElementById("textArea"));
    }
    return true;
  }

  // 应急信号弹：根据当前最短板属性定向投送高级补给
  if (config.usable && config.useAction === "emergency_flare") {
    const textArea = document.getElementById("textArea");

    // 获取三属性当前值
    const stats = [
      { key: "fuel",       value: truckState.fuel       ?? 100 },
      { key: "durability", value: truckState.durability  ?? 100 },
      { key: "comfort",    value: truckState.comfort     ?? 100 },
    ];
    // 从小到大排序
    stats.sort((a, b) => a.value - b.value);
    const worst  = stats[0].key; // 最短板
    const second = stats[1].key; // 次短板

    // 最短板 → 最强单属性补给
    const primaryMap = {
      fuel:       { id: "燃油混合液",  label: "燃油混合液（燃油+80%）" },
      durability: { id: "超级修理包",  label: "超级修理包（耐久+60%）" },
      comfort:    { id: "舒适组合包",  label: "舒适组合包（舒适+50%）" },
    };
    // 次短板 → 辅助补给
    const secondaryMap = {
      fuel:       { id: "高级燃油",    label: "高级燃油（燃油+55%）" },
      durability: { id: "医疗补给包",  label: "医疗补给包（耐久+25%/舒适+35%）" },
      comfort:    { id: "医疗补给包",  label: "医疗补给包（耐久+25%/舒适+35%）" },
    };

    const primary   = primaryMap[worst];
    const secondary = secondaryMap[second];

    // 消耗信号弹
    removeItem(itemId, 1);

    // 投送补给（背包满时给出提示）
    const got = [];
    const failed = [];

    if (addItem(primary.id, 1))   { got.push(primary.label);   }
    else                           { failed.push(primary.label);  }
    // 若主次补给相同（如最短板和次短板都需要医疗补给包）则只发一件避免重复
    if (secondary.id !== primary.id) {
      if (addItem(secondary.id, 1)) { got.push(secondary.label); }
      else                           { failed.push(secondary.label); }
    }

    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

    if (textArea) {
      textArea.innerHTML += `<p style="color:#ef4444">🚨 应急信号弹升空——烈红的光柱划破天际！</p>`;
      if (got.length > 0) {
        textArea.innerHTML += `<p style="color:#4ade80">📦 援救补给投送到位：${got.join("、")}</p>`;
        got.forEach(label => {
          const id = [...Object.values(primaryMap), ...Object.values(secondaryMap)].find(m => m.label === label)?.id;
          if (!id) return;
          const cfg = ITEMS_CONFIG[id];
          if (cfg) textArea.innerHTML += `<p style="color:${cfg.color}">✦ 获得 ${cfg.name} ×1</p>`;
        });
      }
      if (failed.length > 0) {
        textArea.innerHTML += `<p style="color:#6b7280">⚠️ 背包已满，以下补给无法接收：${failed.join("、")}</p>`;
      }
      if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
    }

    if (typeof recordItemUsed === "function") recordItemUsed();
    if (typeof recordJourneyEvent === "function") recordJourneyEvent("item", { itemId, itemName: config.name });
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    return true;
  }

  // 鹰眼地图：随机获得一件玩家尚未持有的珍品，地图消耗
  if (config.usable && config.useAction === "eagle_map") {
    const textArea = document.getElementById("textArea");

    // 排除名单：衍生降级品 + 流程绑定珍品 + 鹰眼地图自身
    const _eagleMapExclude = new Set([
      "黯淡的猎人徽章", "破损的雕塑", "空白书",
      "密钥", "一次性管理员权限", "鹰眼地图",
    ]);

    // 从所有珍品中筛选可获得的候选
    const candidates = Object.keys(ITEMS_CONFIG).filter(id => {
      const c = ITEMS_CONFIG[id];
      if (c.category !== "treasure") return false;       // 必须是珍品
      if (_eagleMapExclude.has(id)) return false;        // 排除名单
      if (c.stackable === false && hasItem(id)) return false; // 已持有不可叠加珍品
      return true;
    });

    removeItem(itemId, 1); // 先消耗地图

    if (candidates.length === 0) {
      // 极端情况：所有珍品都已持有，退而求其次给金币补偿
      const goldCompensation = 30;
      addGold(goldCompensation);
      if (textArea) {
        textArea.innerHTML += `<p style="color:#7c3aed">🗺️ 你展开了鹰眼地图，标注的每一处宝藏……你竟然全都收集过了。</p>`;
        textArea.innerHTML += `<p style="color:#facc15">✦ 地图化为碎片，留下了 ${goldCompensation} 金币作为补偿。</p>`;
        if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
      }
    } else {
      const rewardId = candidates[Math.floor(Math.random() * candidates.length)];
      const rewardCfg = ITEMS_CONFIG[rewardId];
      const added = addItem(rewardId, 1);

      if (textArea) {
        textArea.innerHTML += `<p style="color:#7c3aed">🗺️ 你展开鹰眼地图，古老的墨迹引导你目光投向某处……</p>`;
        if (added) {
          textArea.innerHTML += `<p style="color:#ff00ff">🌟 珍品出现！获得了 <span style="color:${rewardCfg.color};font-weight:bold;">💎 ${rewardCfg.name}</span>！</p>`;
          // 触发珍品获得全屏特效
          if (typeof showTreasureAcquireEffect === "function") showTreasureAcquireEffect(rewardCfg.name, rewardCfg.color);
          if (typeof showTreasureRedDot === "function") showTreasureRedDot();
        } else {
          textArea.innerHTML += `<p style="color:#6b7280">⚠️ 珍品 <span style="color:${rewardCfg.color}">${rewardCfg.name}</span> 已无法放入背包（载重已满）。</p>`;
        }
        if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
      }
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
    }

    if (typeof recordItemUsed === "function") recordItemUsed();
    if (typeof recordJourneyEvent === "function") recordJourneyEvent("item", { itemId, itemName: config.name });
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    return true;
  }

  // 特殊可使用珍品（如呓语之书：设置效果倒转标记 + 转化）
  if (config.usable && config.onUse) {    // 设置游戏状态标记
    if (config.onUse.setFlag) {
      for (const [key, val] of Object.entries(config.onUse.setFlag)) {
        gameState[key] = val;
      }
    }
    // onUse 内嵌属性效果（如猎人徽章：恢复满耐久）
    if (config.onUse.useEffect) {
      const ue = config.onUse.useEffect;
      if (ue.fuel) truckState.fuel = Math.min(100, truckState.fuel + ue.fuel);
      if (ue.durability) truckState.durability = Math.min(100, truckState.durability + ue.durability);
      if (ue.comfort) truckState.comfort = Math.min(100, truckState.comfort + ue.comfort);
      if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
    }
    // 转化为另一个物品
    if (config.onUse.transformTo) {
      removeItem(itemId, 1);
      const existing = inventoryState.items.find(s => s.id === config.onUse.transformTo);
      const toCfg = ITEMS_CONFIG[config.onUse.transformTo];
      if (existing && toCfg && toCfg.stackable) {
        existing.quantity += 1;
      } else {
        inventoryState.items.push({ id: config.onUse.transformTo, quantity: 1 });
      }
      updateInventoryDisplay();
    } else {
      removeItem(itemId, 1);
    }
    if (typeof recordItemUsed === "function") recordItemUsed();
    if (typeof recordJourneyEvent === "function") recordJourneyEvent("item", { itemId, itemName: config.name });
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    return true;
  }

  // 随机金币类物品（如马年红包）
  if (config.randomGold) {
    const [min, max] = config.randomGold;
    const gold = Math.floor(Math.random() * (max - min + 1)) + min;
    addGold(gold);
    removeItem(itemId, 1);
    // 标记已打开红包（用于成就检查）
    if (itemId === "马年红包") {
      gameState.hasOpenedRedPacket = true;
    }
    const textArea = document.getElementById("textArea");
    if (textArea) {
      textArea.innerHTML += `<p>🧧 打开了${config.name}，获得了 ${gold} 金币！</p>`;
      if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
    }
    if (typeof recordItemUsed === "function") recordItemUsed();
    if (typeof recordJourneyEvent === "function") recordJourneyEvent("item", { itemId, itemName: config.name });
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    return true;
  }

  // 小丑盲盒：98%随机非珍品物品，2%随机珍品
  if (config.clownBox) {
    removeItem(itemId, 1);
    const textArea = document.getElementById("textArea");
    const roll = Math.random();
    let rewardId = null;
    let rewardQty = 1;

    if (roll < 0.02) {
      // 2% 珍品
      const treasures = Object.keys(ITEMS_CONFIG).filter(id => ITEMS_CONFIG[id].category === "treasure");
      if (treasures.length > 0) {
        rewardId = treasures[Math.floor(Math.random() * treasures.length)];
      }
    }
    if (!rewardId) {
      // 98% 非珍品
      const nonTreasures = Object.keys(ITEMS_CONFIG).filter(id => {
        const c = ITEMS_CONFIG[id];
        return c.category !== "treasure" && id !== "小丑盲盒";
      });
      if (nonTreasures.length > 0) {
        rewardId = nonTreasures[Math.floor(Math.random() * nonTreasures.length)];
      }
    }

    if (rewardId) {
      const rewardCfg = ITEMS_CONFIG[rewardId];
      const isTreasure = rewardCfg && rewardCfg.category === "treasure";
      if (addItem(rewardId, rewardQty)) {
        if (textArea) {
          const colorTag = rewardCfg ? rewardCfg.color : "#fff";
          const prefix = isTreasure ? "🌟✨" : "🤡";
          textArea.innerHTML += `<p style="color:${isTreasure ? '#ff00ff' : '#c084fc'}">${prefix} 打开小丑盲盒，获得了${isTreasure ? '珍品' : ''} <span style="color:${colorTag};font-weight:bold;">${rewardCfg ? rewardCfg.name : rewardId}</span> ×${rewardQty}！${isTreasure ? ' 🎉太幸运了！' : ''}</p>`;
          if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
        }
      } else {
        if (textArea) {
          textArea.innerHTML += `<p style="color:#6b7280">🤡 打开小丑盲盒……但背包满了，物品消失在彩色烟雾中。</p>`;
          if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
        }
      }
    }
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
    if (typeof recordItemUsed === "function") recordItemUsed();
    if (typeof recordJourneyEvent === "function") recordJourneyEvent("item", { itemId, itemName: config.name });
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    return true;
  }
  if (config.useEffect) {
    let fuelDelta = config.useEffect.fuel || 0;
    let durabilityDelta = config.useEffect.durability || 0;
    let comfortDelta = config.useEffect.comfort || 0;
    // 流浪艺人特性：使用物品时表演助兴，舒适恢复 +3
    if (comfortDelta > 0 && truckState.passengers && truckState.passengers.includes("流浪艺人")) {
      comfortDelta += 3;
    }
    if (fuelDelta) {
      truckState.fuel = Math.min(100, truckState.fuel + fuelDelta);
    }
    if (durabilityDelta) {
      truckState.durability = Math.min(
        100,
        truckState.durability + durabilityDelta,
      );
    }
    if (comfortDelta) {
      truckState.comfort = Math.min(
        100,
        truckState.comfort + comfortDelta,
      );
    }
  }

  removeItem(itemId, 1);
  updateTruckStatusDisplay();
  // 记录物品使用到历程日志
  if (typeof recordJourneyEvent === "function") {
    recordJourneyEvent("item", {
      itemId,
      itemName: config ? config.name : itemId,
    });
  }
  // 记录物品使用（用于成就检查）
  if (typeof recordItemUsed === "function") {
    recordItemUsed();
  }
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }
  return true;
}

// 添加金币
function addGold(amount) {
  inventoryState.gold += amount;
  updateGoldDisplay();
  // 获得金币时播放"叮！"音效
  if (amount > 0 && typeof playGoldCoin === "function") {
    playGoldCoin();
  }
}

// 消耗金币
function spendGold(amount) {
  if (inventoryState.gold < amount) return false;
  inventoryState.gold -= amount;
  updateGoldDisplay();
  return true;
}

// 检查合成配方是否可用
function canCraft(recipeId) {
  const recipe = CRAFTING_RECIPES[recipeId];
  if (!recipe) return false;

  // 检查材料
  for (const mat of recipe.materials) {
    if (!hasItem(mat.itemId, mat.quantity)) return false;
  }

  // 检查结果是否能放进库存（需扣除材料释放的重量）
  const resultConfig = ITEMS_CONFIG[recipe.result.itemId];
  if (!resultConfig) return false;

  // 检查非堆叠物品是否已存在
  if (!resultConfig.stackable && inventoryState.items.some(s => s.id === recipe.result.itemId)) return false;

  const resultWeight = resultConfig.weight * recipe.result.quantity;

  // 计算消耗材料后释放的重量
  let freedWeight = 0;
  for (const mat of recipe.materials) {
    const matConfig = ITEMS_CONFIG[mat.itemId];
    if (matConfig) freedWeight += matConfig.weight * mat.quantity;
  }

  const afterWeight = getInventoryWeight() - freedWeight + resultWeight;

  // 如果合成后能减轻重量（净重量变少），即使超载也允许合成
  if (resultWeight <= freedWeight) return true;

  // 允许超载合成（离开制作台时统一超载判定）
  return true;
}

// 获取合成不可用的原因（用于 tooltip）
function getCraftUnavailableReason(recipeId) {
  const recipe = CRAFTING_RECIPES[recipeId];
  if (!recipe) return "";
  for (const mat of recipe.materials) {
    if (!hasItem(mat.itemId, mat.quantity)) return "材料不足";
  }
  // 非堆叠物品已存在
  const resultConfig = ITEMS_CONFIG[recipe.result.itemId];
  if (!resultConfig) return "未知物品";
  if (!resultConfig.stackable && inventoryState.items.some(s => s.id === recipe.result.itemId)) return "已拥有该物品";
  // 扣除材料释放的重量后检查
  const resultWeight = resultConfig.weight * recipe.result.quantity;
  let freedWeight = 0;
  for (const mat of recipe.materials) {
    const matConfig = ITEMS_CONFIG[mat.itemId];
    if (matConfig) freedWeight += matConfig.weight * mat.quantity;
  }
  // 允许超载合成（离开制作台时统一超载判定）
  return "";
}

// 执行合成
function craftItem(recipeId) {
  if (!canCraft(recipeId)) return false;

  const recipe = CRAFTING_RECIPES[recipeId];

  // 消耗材料
  for (const mat of recipe.materials) {
    removeItem(mat.itemId, mat.quantity);
  }

  // 获得成品（直接操作库存，避免 addItem 的内置重量检查在边界情况下阻止合成）
  const existing = inventoryState.items.find((slot) => slot.id === recipe.result.itemId);
  const resultConfig = ITEMS_CONFIG[recipe.result.itemId];
  if (existing && resultConfig && resultConfig.stackable) {
    existing.quantity += recipe.result.quantity;
  } else {
    inventoryState.items.push({ id: recipe.result.itemId, quantity: recipe.result.quantity });
  }
  updateInventoryDisplay();

  // 记录物品合成（用于成就检查）
  if (typeof recordItemCrafted === "function") {
    recordItemCrafted();
  }
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }
  return true;
}

// 乘客特性：年迈妇人在车时商人给优惠（购买 -15%，出售 +15%）
function getEffectiveBuyPrice(merchantId, itemId) {
  const merchant = MERCHANT_CONFIG[merchantId];
  if (!merchant) return 0;
  const listing = merchant.items.find((i) => i.itemId === itemId);
  if (!listing) return 0;
  let price = listing.buyPrice;
  // 年迈妇人乘客：购买价 -15%
  if (typeof truckState !== "undefined" && truckState.passengers && truckState.passengers.includes("年迈妇人")) {
    price = Math.floor(price * 0.85);
  }
  // 繁荣时代的金属碎片：购买价 -25%
  if (typeof inventoryState !== "undefined" && inventoryState.items && inventoryState.items.some(s => s.id === "繁荣时代的金属碎片")) {
    price = Math.floor(price * 0.75);
  }
  // 会员卡加成：购买价 -20%
  if (typeof gameState !== "undefined" && Array.isArray(gameState.hardModeBonuses) && gameState.hardModeBonuses.includes("member_card")) {
    price = Math.floor(price * 0.80);
  }
  return Math.max(1, price);
}

function getEffectiveSellPrice(merchantId, itemId) {
  const merchant = MERCHANT_CONFIG[merchantId];
  if (!merchant) return 0;
  const listing = merchant.items.find((i) => i.itemId === itemId);
  if (!listing) return 0;
  const base = listing.sellPrice;
  if (typeof truckState !== "undefined" && truckState.passengers && truckState.passengers.includes("年迈妇人")) {
    return Math.floor(base * 1.15);
  }
  return base;
}

// 购买物品
function buyItem(merchantId, itemId) {
  const merchant = MERCHANT_CONFIG[merchantId];
  if (!merchant) return false;

  const listing = merchant.items.find((i) => i.itemId === itemId);
  if (!listing) return false;

  const price = getEffectiveBuyPrice(merchantId, itemId);
  if (inventoryState.gold < price) return false;

  // 允许超载购买：直接添加物品，不做重量检查（离开商人时统一超载判定）
  const config = ITEMS_CONFIG[itemId];
  if (!config) return false;
  // 不可堆叠且已存在则不能购买
  if (!config.stackable && inventoryState.items.some(s => s.id === itemId)) return false;

  spendGold(price);
  // 直接操作库存，绕过 addItem 的 canAddItem 重量检查
  const existing = inventoryState.items.find((slot) => slot.id === itemId);
  if (existing && config.stackable) {
    existing.quantity += 1;
  } else {
    inventoryState.items.push({ id: itemId, quantity: 1 });
  }
  updateInventoryDisplay();
  // 记录商人交易（用于成就检查）
  if (typeof recordMerchantTrade === "function") {
    recordMerchantTrade();
  }
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }
  return true;
}

// 出售物品
function sellItem(merchantId, itemId) {
  const merchant = MERCHANT_CONFIG[merchantId];
  if (!merchant) return false;

  const listing = merchant.items.find((i) => i.itemId === itemId);
  if (!listing) return false;

  if (!hasItem(itemId)) return false;

  const sellPrice = getEffectiveSellPrice(merchantId, itemId);
  removeItem(itemId, 1);
  addGold(sellPrice);
  // 记录商人交易（用于成就检查）
  if (typeof recordMerchantTrade === "function") {
    recordMerchantTrade();
  }
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }
  return true;
}
