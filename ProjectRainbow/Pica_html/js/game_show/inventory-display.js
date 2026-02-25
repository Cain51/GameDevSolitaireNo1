// 库存显示模块
// 管理库存面板和状态栏的UI更新

// 当前后备箱标签页："normal" 普通物品 / "treasure" 珍品
let currentInventoryTab = "normal";

// 珍品小红点状态
let _treasureRedDot = false;

// 获取物品显示名称（珍品类物品后缀加 💎）
function getItemDisplayName(config) {
  if (!config) return "";
  return config.category === "treasure" ? config.name + " 💎" : config.name;
}

// 显示珍品小红点
function showTreasureRedDot() {
  _treasureRedDot = true;
  const btn = document.getElementById("inv-tab-treasure");
  if (!btn) return;
  // 避免重复添加
  if (btn.querySelector(".treasure-red-dot")) return;
  const dot = document.createElement("span");
  dot.className = "treasure-red-dot";
  dot.style.cssText = "position:relative;display:inline-block;width:8px;height:8px;background:#ef4444;border-radius:50%;margin-left:4px;box-shadow:0 0 6px #ef4444;animation:treasureDotPulse 1.2s ease-in-out infinite;";
  btn.appendChild(dot);
}

// 隐藏珍品小红点
function hideTreasureRedDot() {
  _treasureRedDot = false;
  document.querySelectorAll(".treasure-red-dot").forEach(d => d.remove());
}

// 珍品获得全屏特效
function showTreasureAcquireEffect(itemName, itemColor) {
  // 移除可能残留的旧特效
  const old = document.getElementById("treasure-acquire-overlay");
  if (old) old.remove();

  const overlay = document.createElement("div");
  overlay.id = "treasure-acquire-overlay";
  overlay.style.cssText = `
    position:fixed;inset:0;
    display:flex;align-items:center;justify-content:center;flex-direction:column;
    pointer-events:none;z-index:99999;
    opacity:0;
    animation: treasureOverlayFadeIn 0.6s ease-out forwards;
  `;

  overlay.innerHTML = `
    <div class="treasure-acquire-backdrop" style="
      position:absolute;
      left:0;right:0;
      top:50%;transform:translateY(-50%);
      height:140px;
      background:linear-gradient(180deg, transparent 0%, rgba(0,0,0,0.65) 20%, rgba(0,0,0,0.75) 50%, rgba(0,0,0,0.65) 80%, transparent 100%);
      pointer-events:none;
    "></div>
    <div style="
      position:relative;z-index:1;
      font-size:clamp(1rem, 3vw, 1.4rem);
      color:#d4d4d8;
      margin-bottom:8px;
      text-shadow:0 0 10px rgba(0,0,0,0.8);
      animation: treasureTextFloat 0.6s ease-out forwards;
    ">✦ 获得珍品 ✦</div>
    <div style="
      position:relative;z-index:1;
      font-size:clamp(2rem, 6vw, 3.5rem);
      font-weight:900;
      color:${itemColor || "#fbbf24"};
      text-shadow:0 0 20px ${itemColor || "#fbbf24"}, 0 0 40px ${itemColor || "#fbbf24"}88, 0 2px 8px rgba(0,0,0,0.9);
      animation: treasureTextFloat 0.6s 0.1s ease-out both;
    ">${itemName} 💎</div>
  `;

  document.body.appendChild(overlay);

  // 淡入后等待 1s，然后迅速淡出
  const fadeTimer = setTimeout(() => {
    if (!document.getElementById("treasure-acquire-overlay")) return;
    overlay.style.animation = "treasureOverlayFadeOut 0.35s ease-in forwards";
    const removeTimer = setTimeout(() => overlay.remove(), 400);
    overlay.dataset.removeTimer = removeTimer;
  }, 1600); // 0.6s淡入 + 1s停留
  overlay.dataset.fadeTimer = fadeTimer;
}

// 立即清除珍品特效（用于结局/死亡时）
function clearTreasureEffect() {
  const overlay = document.getElementById("treasure-acquire-overlay");
  if (!overlay) return;
  // 清除未触发的定时器
  if (overlay.dataset.fadeTimer) clearTimeout(Number(overlay.dataset.fadeTimer));
  if (overlay.dataset.removeTimer) clearTimeout(Number(overlay.dataset.removeTimer));
  overlay.remove();
}

// 清除所有视觉特效（结局/死亡时调用）
function clearAllEffects() {
  clearTreasureEffect();
  if (typeof clearEventTheme === "function") clearEventTheme();
}

// 切换后备箱标签页
function switchInventoryTab(tab) {
  currentInventoryTab = tab;
  // 切换到珍品标签时清除小红点
  if (tab === "treasure") {
    hideTreasureRedDot();
  }
  // 更新按钮样式
  const normalBtn = document.getElementById("inv-tab-normal");
  const treasureBtn = document.getElementById("inv-tab-treasure");
  if (normalBtn && treasureBtn) {
    if (tab === "normal") {
      normalBtn.className = "px-3 py-1 text-xs font-medium rounded-md transition-all duration-200 bg-[#c41e3a] text-white shadow";
      treasureBtn.className = "px-3 py-1 text-xs font-medium rounded-md transition-all duration-200 text-gray-400 hover:text-gray-200";
    } else {
      normalBtn.className = "px-3 py-1 text-xs font-medium rounded-md transition-all duration-200 text-gray-400 hover:text-gray-200";
      treasureBtn.className = "px-3 py-1 text-xs font-medium rounded-md transition-all duration-200 bg-amber-600 text-white shadow";
    }
    // 切换样式后重新附加小红点（如果仍需显示）
    if (_treasureRedDot && tab !== "treasure") {
      showTreasureRedDot();
    }
  }
  updateInventoryDisplay();
}

// 耐久变动时触发屏幕抖动（游戏画面 #game-canvas）
function triggerScreenShake() {
  const el = document.getElementById("game-canvas");
  if (!el) return;
  el.classList.remove("screen-shake");
  void el.offsetWidth;
  el.classList.add("screen-shake");
  setTimeout(() => el.classList.remove("screen-shake"), 450);
}

// 追踪上次属性值，用于检测属性是否恢复（触发音效）
let _prevFuel       = null;
let _prevDurability = null;
let _prevComfort    = null;

// 更新皮卡状态栏（燃油、耐久、舒适）
function updateTruckStatusDisplay() {
  // ── 属性恢复音效检测 ──────────────────────────────────────────
  // 首次调用时仅初始化上次值，不触发音效
  if (_prevFuel !== null && typeof playStatRestore === "function") {
    if (truckState.fuel       > _prevFuel       + 0.01) playStatRestore("fuel");
    if (truckState.durability > _prevDurability + 0.01) playStatRestore("durability");
    if (truckState.comfort    > _prevComfort    + 0.01) playStatRestore("comfort");
  }
  _prevFuel       = truckState.fuel;
  _prevDurability = truckState.durability;
  _prevComfort    = truckState.comfort;

  // 燃油
  const fuelBar = document.getElementById("fuel-bar");
  const fuelText = document.getElementById("fuel-text");
  if (fuelBar && fuelText) {
    fuelBar.style.width = Math.max(0, truckState.fuel) + "%";
    fuelText.textContent = Math.round(Math.max(0, truckState.fuel)) + "%";
    // 低于30%变红
    if (truckState.fuel <= 30) {
      fuelBar.className =
        fuelBar.className.replace(/bg-yellow-500|bg-red-500/g, "") +
        " bg-red-500";
      fuelText.className =
        fuelText.className.replace(/text-yellow-500|text-red-500/g, "") +
        " text-red-500";
    } else {
      fuelBar.className =
        fuelBar.className.replace(/bg-yellow-500|bg-red-500/g, "") +
        " bg-yellow-500";
      fuelText.className =
        fuelText.className.replace(/text-yellow-500|text-red-500/g, "") +
        " text-yellow-500";
    }
  }

  // 耐久
  const durBar = document.getElementById("durability-bar");
  const durText = document.getElementById("durability-text");
  if (durBar && durText) {
    durBar.style.width = Math.max(0, truckState.durability) + "%";
    durText.textContent = Math.round(Math.max(0, truckState.durability)) + "%";
    if (truckState.durability <= 30) {
      durBar.className =
        durBar.className.replace(/bg-green-500|bg-red-500/g, "") +
        " bg-red-500";
      durText.className =
        durText.className.replace(/text-green-500|text-red-500/g, "") +
        " text-red-500";
    } else {
      durBar.className =
        durBar.className.replace(/bg-green-500|bg-red-500/g, "") +
        " bg-green-500";
      durText.className =
        durText.className.replace(/text-green-500|text-red-500/g, "") +
        " text-green-500";
    }
  }

  // 舒适度
  const comBar = document.getElementById("comfort-bar");
  const comText = document.getElementById("comfort-text");
  if (comBar && comText) {
    comBar.style.width = Math.max(0, truckState.comfort) + "%";
    comText.textContent = Math.round(Math.max(0, truckState.comfort)) + "%";
    if (truckState.comfort <= 30) {
      comBar.className =
        comBar.className.replace(/bg-blue-400|bg-red-500/g, "") + " bg-red-500";
      comText.className =
        comText.className.replace(/text-blue-400|text-red-500/g, "") +
        " text-red-500";
    } else {
      comBar.className =
        comBar.className.replace(/bg-blue-400|bg-red-500/g, "") +
        " bg-blue-400";
      comText.className =
        comText.className.replace(/text-blue-400|text-red-500/g, "") +
        " text-blue-400";
    }
  }

  // 行驶里程记录表
  const mileageCurrentEl = document.getElementById("mileage-current");
  const mileageBestEl = document.getElementById("mileage-best");
  if (mileageCurrentEl && typeof gameState !== "undefined") {
    mileageCurrentEl.textContent = Math.floor(gameState.mileage || 0);
  }
  if (mileageBestEl && typeof getBestMileage === "function") {
    mileageBestEl.textContent = getBestMileage();
  }
  // 经历事件次数
  const eventsCountEl = document.getElementById("events-count");
  if (eventsCountEl && typeof gameState !== "undefined") {
    eventsCountEl.textContent = gameState.totalEventsHandled || 0;
  }

  // 燃油≤5：背景暗红闪烁；耐久≤50：故障特效
  const gameCanvas = document.getElementById("game-canvas");
  if (gameCanvas) {
    if (truckState.fuel <= 20) {
      gameCanvas.classList.add("fuel-low-flash");
    } else {
      gameCanvas.classList.remove("fuel-low-flash");
    }
    if (truckState.durability <= 50) {
      gameCanvas.classList.add("durability-glitch");
    } else {
      gameCanvas.classList.remove("durability-glitch");
    }
  }
}

// 更新车上成员列表显示
function updatePassengerListDisplay() {
  const container = document.getElementById("passenger-list");
  if (!container) return;

  const passengers =
    typeof truckState !== "undefined" && Array.isArray(truckState.passengers)
      ? truckState.passengers
      : [];

  container.innerHTML = "";

  if (passengers.length === 0) {
    container.innerHTML = '<span class="text-gray-600 text-sm">暂无乘客</span>';
    return;
  }

  const getFavor =
    typeof getPassengerFavor === "function"
      ? getPassengerFavor
      : () => 50;
  const getOffMileage =
    typeof gameState !== "undefined" && gameState.passengerGetOffMileage
      ? gameState.passengerGetOffMileage
      : {};
  const mileage = typeof gameState !== "undefined" && typeof gameState.mileage === "number"
    ? gameState.mileage
    : 0;
  const permanentPassengers = typeof gameState !== "undefined" && Array.isArray(gameState.permanentPassengers)
    ? gameState.permanentPassengers
    : [];

  const newNames = (typeof gameState !== "undefined" && gameState._newPassengerNames) ? gameState._newPassengerNames : [];
  passengers.forEach((name) => {
    const cfg =
      typeof PASSENGER_CONFIG !== "undefined" && PASSENGER_CONFIG[name];
    const color = cfg && cfg.color ? cfg.color : "#94a3b8";
    const favor = getFavor(name);
    const favorColor =
      favor >= 70 ? "#22c55e" : favor >= 40 ? "#eab308" : "#ef4444";
    // 如果乘客已永久上车，不显示距离
    const isPermanent = permanentPassengers.includes(name);
    const targetMileage = isPermanent ? null : getOffMileage[name];
    const remainingKm =
      typeof targetMileage === "number" && !isNaN(targetMileage)
        ? Math.max(0, Math.ceil(targetMileage - mileage))
        : null;
    const item = document.createElement("div");
    const enterClass = newNames.indexOf(name) !== -1 ? " passenger-item-enter" : "";
    item.className =
      "flex items-center gap-2 px-2 py-1.5 rounded-lg bg-gray-800/60 border border-gray-700 flex-shrink-0" + enterClass;
    const namePart = `<span class="flex items-center gap-1.5 min-w-0"><span class="w-2 h-2 rounded-full flex-shrink-0" style="background:${color}"></span><span style="color:${color}" class="font-medium whitespace-nowrap">${name}</span>${remainingKm !== null ? `<span class="text-gray-500 text-xs flex-shrink-0" title="到达目的地剩余里程">${remainingKm}km</span>` : ""}</span>`;
    const favorPart = `<span class="flex items-center gap-0.5 flex-shrink-0 text-xs" style="color:${favorColor}" title="好感度">♥${favor}</span>`;
    item.innerHTML = namePart + favorPart;
    container.appendChild(item);
  });
  if (typeof gameState !== "undefined" && gameState._newPassengerNames && gameState._newPassengerNames.length)
    setTimeout(function () { gameState._newPassengerNames = []; }, 50);
}

// 更新金币显示
function updateGoldDisplay() {
  const goldText = document.getElementById("gold-text");
  if (goldText) {
    goldText.textContent = inventoryState.gold;
  }
}

// 更新载重条显示
function updateWeightDisplay() {
  const weightBar = document.getElementById("weight-bar");
  const weightText = document.getElementById("weight-text");
  if (weightBar && weightText) {
    const currentWeight = getInventoryWeight();
    const pct = Math.min(100, (currentWeight / inventoryState.maxWeight) * 100);
    weightBar.style.width = pct + "%";
    weightText.textContent = currentWeight + "/" + inventoryState.maxWeight;

    // 超载：红色闪烁
    if (currentWeight > inventoryState.maxWeight) {
      weightText.style.color = "#ef4444";
      weightText.style.fontWeight = "bold";
    } else {
      weightText.style.color = "";
      weightText.style.fontWeight = "";
    }

    // 超过80%变黄，满载变红
    if (pct >= 100) {
      weightBar.className =
        weightBar.className.replace(
          /bg-\[#c41e3a\]|bg-yellow-500|bg-red-500/g,
          "",
        ) + " bg-red-500";
    } else if (pct >= 80) {
      weightBar.className =
        weightBar.className.replace(
          /bg-\[#c41e3a\]|bg-yellow-500|bg-red-500/g,
          "",
        ) + " bg-yellow-500";
    } else {
      weightBar.className =
        weightBar.className.replace(
          /bg-\[#c41e3a\]|bg-yellow-500|bg-red-500/g,
          "",
        ) + " bg-[#c41e3a]";
    }
  }

  // 更新后备箱等级徽章
  const levelBadge = document.getElementById("trunk-level-badge");
  if (levelBadge) {
    const lv = inventoryState.trunkLevel || 1;
    levelBadge.textContent = "Lv." + lv;
    if (lv >= TRUNK_MAX_LEVEL) {
      levelBadge.style.color = "#22c55e";
      levelBadge.style.borderColor = "rgba(34,197,94,0.5)";
      levelBadge.style.background = "rgba(34,197,94,0.15)";
    }
  }

  // 更新升级按钮状态与 tooltip
  const upgradeBtn = document.getElementById("trunk-upgrade-btn");
  if (upgradeBtn) {
    const lv = inventoryState.trunkLevel || 1;
    if (lv >= TRUNK_MAX_LEVEL) {
      upgradeBtn.style.display = "none";
    } else {
      const next = TRUNK_UPGRADES[lv + 1];
      if (next) {
        let tipHtml = `<div style="font-weight:bold;color:#60a5fa;font-size:14px;margin-bottom:4px;">⬆ 升级至 Lv.${lv + 1}</div>`;
        tipHtml += `<div style="color:#4ade80;font-size:12px;margin-bottom:6px;border-bottom:1px solid #333;padding-bottom:4px;">载重上限 <span style="font-weight:bold;">+${next.bonusWeight}kg</span> → ${inventoryState.maxWeight + next.bonusWeight}kg</div>`;
        tipHtml += `<div style="color:#9ca3af;font-size:11px;margin-bottom:4px;">需要材料：</div>`;
        for (const mat of next.materials) {
          const cfg = ITEMS_CONFIG[mat.id];
          const owned = getItemQuantity(mat.id);
          const enough = owned >= mat.qty;
          const name = cfg ? cfg.name : mat.id;
          const color = cfg ? cfg.color : "#9ca3af";
          tipHtml += `<div style="font-size:12px;margin-bottom:2px;"><span style="color:${color}">${name} ×${mat.qty}</span> <span style="color:${enough ? "#22c55e" : "#ef4444"}">(${owned}/${mat.qty})</span></div>`;
        }
        upgradeBtn.setAttribute("data-tooltip-html", tipHtml);
        upgradeBtn.removeAttribute("data-tooltip");
      }
    }
  }
}

// 根据物品配置生成悬停 tooltip 富文本 HTML
function getItemTooltipHtml(config) {
  if (!config) return "";
  const nameColor = config.color || "#e5e5e5";
  const catLabels = { consumable: "消耗品", material: "材料", special: "特殊", treasure: "珍品" };
  const catColors = { consumable: "#4ade80", material: "#9ca3af", special: "#facc15", treasure: "#f59e0b" };
  const catLabel = catLabels[config.category] || "物品";
  const catColor = catColors[config.category] || "#9ca3af";

  let html = `<div style="font-weight:bold;color:${nameColor};font-size:14px;margin-bottom:2px;">${getItemDisplayName(config)}</div>`;
  html += `<div style="font-size:11px;color:${catColor};margin-bottom:5px;border-bottom:1px solid #333;padding-bottom:4px;">${catLabel} · ${config.weight}kg</div>`;

  // 描述
  if (config.description) {
    html += `<div style="color:#b0b0b0;font-size:12px;margin-bottom:5px;line-height:1.45;">${config.description}</div>`;
  }

  // 使用效果
  if (config.useEffect && typeof config.useEffect === "object") {
    const u = config.useEffect;
    const fx = [];
    if (typeof u.fuel === "number") fx.push(`<span style="color:#eab308">燃油 +${u.fuel}%</span>`);
    if (typeof u.durability === "number") fx.push(`<span style="color:#22c55e">耐久 +${u.durability}%</span>`);
    if (typeof u.comfort === "number") fx.push(`<span style="color:#60a5fa">舒适 +${u.comfort}%</span>`);
    if (fx.length) html += `<div style="font-size:12px;margin-bottom:3px;">📦 使用：${fx.join("，")}</div>`;
  }

  // onUse 特殊效果
  if (config.usable && config.onUse) {
    const ou = config.onUse;
    if (ou.useEffect) {
      const fx = [];
      if (ou.useEffect.fuel) fx.push(`<span style="color:#eab308">燃油 +${ou.useEffect.fuel}%</span>`);
      if (ou.useEffect.durability) fx.push(`<span style="color:#22c55e">耐久 +${ou.useEffect.durability}%</span>`);
      if (ou.useEffect.comfort) fx.push(`<span style="color:#60a5fa">舒适 +${ou.useEffect.comfort}%</span>`);
      if (fx.length) html += `<div style="font-size:12px;margin-bottom:3px;">✦ 使用：${fx.join("，")}</div>`;
    }
    if (ou.transformTo) {
      html += `<div style="font-size:11px;color:#a78bfa;">⟳ 使用后变为「${ou.transformTo}」</div>`;
    }
  }

  // 被动效果
  if (config.passive) {
    const p = config.passive;
    // lucky_charm 专属显示
    if (p.type === "lucky_charm") {
      html += `<div style="font-size:12px;color:#34d399;margin-bottom:3px;">🔮 被动：每次抉择 2% 概率获得随机物资×1<br><span style="color:#6ee7b7;font-size:11px;">（神秘事件中提升至 10%）</span></div>`;
    } else if (p.type === "music_box") {
      html += `<div style="font-size:12px;color:#f472b6;margin-bottom:3px;">🔮 被动：每次抉择 20% 概率<span style="color:#60a5fa"> 舒适 +1%</span><br><span style="color:#f9a8d4;font-size:11px;">（神秘事件中提升至 100%）</span></div>`;
    } else if (p.type === "admin_permit") {
      html += `<div style="font-size:12px;color:#22d3ee;margin-bottom:3px;">🖥️ <b>主动使用</b>：激活 Debug 控制台，获得一次使用机会<br><span style="color:#67e8f9;font-size:11px;">使用 Debug 后令牌自动销毁，控制台关闭</span></div>`;
    } else {
      const fx = [];
      if (p.fuel) fx.push(`<span style="color:#eab308">燃油 +${p.fuel}%</span>`);
      if (p.durability) fx.push(`<span style="color:#22c55e">耐久 +${p.durability}%</span>`);
      if (p.comfort) fx.push(`<span style="color:#60a5fa">舒适 +${p.comfort}%</span>`);
      if (p.goldMin && p.goldMax) fx.push(`<span style="color:#fbbf24">金币 +${p.goldMin}~${p.goldMax}</span>`);
      const chance = p.triggerChance >= 1 ? "每次" : Math.round(p.triggerChance * 100) + "%概率";
      if (fx.length) html += `<div style="font-size:12px;color:#c4b5fd;margin-bottom:3px;">🔮 被动（${chance}）：${fx.join("，")}</div>`;
    }
  }

  // 随机金币
  if (config.randomGold) {
    html += `<div style="font-size:12px;color:#fbbf24;">🧧 使用获得 ${config.randomGold[0]}~${config.randomGold[1]} 金币</div>`;
  }

  return html;
}

// 确保全局自定义 tooltip 节点存在（与当前画面风格一致）
function ensureInventoryTooltipElement() {
  let el = document.getElementById("inventory-tooltip");
  if (el) return el;
  el = document.createElement("div");
  el.id = "inventory-tooltip";
  el.setAttribute("role", "tooltip");
  el.style.cssText =
    "position:fixed;z-index:9999;max-width:280px;padding:10px 14px;border-radius:10px;border:2px solid #c41e3a;background:linear-gradient(135deg,#0d0d0d 0%,#141420 100%);color:#e5e5e5;font-size:13px;line-height:1.4;box-shadow:0 0 24px rgba(196,30,58,0.35),0 4px 12px rgba(0,0,0,0.5);pointer-events:none;opacity:0;transition:opacity 0.15s ease;visibility:hidden;";
  document.body.appendChild(el);
  return el;
}

// 显示与画面风格一致的自定义 tooltip（支持富文本 HTML）
function showInventoryTooltip(el) {
  const htmlContent = el && el.getAttribute("data-tooltip-html");
  const textContent = el && el.getAttribute("data-tooltip");
  if (!htmlContent && !textContent) return;
  const tip = ensureInventoryTooltipElement();
  if (htmlContent) {
    tip.innerHTML = htmlContent;
  } else {
    tip.textContent = textContent;
  }
  tip.style.visibility = "visible";
  tip.style.opacity = "0";
  tip.style.left = "-9999px";
  tip.style.top = "0";
  tip.offsetHeight; // 强制 reflow 以得到正确宽高
  const rect = el.getBoundingClientRect();
  const tipRect = tip.getBoundingClientRect();
  const gap = 8;
  let left = rect.left + rect.width / 2 - tipRect.width / 2;
  let top = rect.top - tipRect.height - gap;
  if (top < 12) top = rect.bottom + gap;
  if (left < 12) left = 12;
  if (left + tipRect.width > window.innerWidth - 12)
    left = window.innerWidth - tipRect.width - 12;
  tip.style.left = left + "px";
  tip.style.top = top + "px";
  tip.style.opacity = "1";

  // 时间存折：悬停时在金币区右侧显示时间银行余额徽标
  const slotIdx = el && el.getAttribute("data-slot-index");
  if (slotIdx !== null && typeof inventoryState !== "undefined" && inventoryState.items) {
    const slot = inventoryState.items[parseInt(slotIdx, 10)];
    if (slot && slot.id === "时间存折") {
      const badge = document.getElementById("time-bank-balance-badge");
      const balanceText = document.getElementById("time-bank-balance-text");
      if (badge && balanceText) {
        const bal = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;
        balanceText.textContent = bal;
        badge.style.display = "flex";
        badge.style.opacity = "1";
      }
    }
  }
}

// 隐藏自定义 tooltip
function hideInventoryTooltip() {
  const tip = document.getElementById("inventory-tooltip");
  if (tip) {
    tip.style.opacity = "0";
    tip.style.visibility = "hidden";
  }
  // 隐藏时间银行余额徽标
  const badge = document.getElementById("time-bank-balance-badge");
  if (badge) {
    badge.style.opacity = "0";
    // 短暂延迟后隐藏，让 opacity 过渡先完成
    setTimeout(() => { if (badge.style.opacity === "0") badge.style.display = "none"; }, 220);
  }
}

// 后备箱拖拽：插入位占位与缓动让位用
let inventoryDragSourceIndex = -1;
function ensureInventoryPlaceholder() {
  let el = document.getElementById("inventory-drop-placeholder");
  if (el) return el;
  el = document.createElement("div");
  el.id = "inventory-drop-placeholder";
  el.className = "inventory-drop-placeholder";
  el.setAttribute("data-placeholder", "1");
  return el;
}

// 后备箱拖拽：开始（插入占位条，其他项缓动让位）
function inventoryDragStart(e) {
  const idx = parseInt(e.currentTarget.getAttribute("data-slot-index"), 10);
  inventoryDragSourceIndex = idx;
  e.dataTransfer.setData("text/plain", String(idx));
  e.dataTransfer.effectAllowed = "move";
  e.currentTarget.classList.add("inventory-item-dragging");

  const listEl = document.getElementById("inventory-list");
  if (!listEl) return;
  const rows = listEl.querySelectorAll(".inventory-item-row");
  if (rows.length === 0) return;
  const placeholder = ensureInventoryPlaceholder();
  
  // 使用 setTimeout 延迟插入占位条，防止浏览器由于 DOM 变更而立即中断拖拽
  setTimeout(() => {
    if (placeholder.parentNode) placeholder.parentNode.removeChild(placeholder);
    const insertIndex = Math.min(idx, rows.length);
    if (insertIndex >= rows.length) listEl.appendChild(placeholder);
    else listEl.insertBefore(placeholder, rows[insertIndex]);
    placeholder.classList.add("active");
  }, 0);
}

// 后备箱拖拽：结束（移除占位条）
function inventoryDragEnd(e) {
  e.currentTarget.classList.remove("inventory-item-dragging");
  const placeholder = document.getElementById("inventory-drop-placeholder");
  if (placeholder && placeholder.parentNode) placeholder.parentNode.removeChild(placeholder);
  inventoryDragSourceIndex = -1;
}

// 后备箱拖拽：经过（更新插入位，占位条移动时其他项缓动让位）
function inventoryDragOver(e) {
  e.preventDefault();
  e.stopPropagation();
  e.dataTransfer.dropEffect = "move";
  const listEl = document.getElementById("inventory-list");
  const placeholder = document.getElementById("inventory-drop-placeholder");
  if (!listEl || !placeholder || !placeholder.parentNode) return;
  
  let targetEl = e.target;
  if (targetEl && targetEl.nodeType === 3) targetEl = targetEl.parentNode;
  const row = targetEl && targetEl.closest ? targetEl.closest(".inventory-item-row") : null;
  if (!row) return; // 在占位条或空白处仅保持 allow drop，不移动占位条
  
  const rows = listEl.querySelectorAll(".inventory-item-row");
  let insertIndex = -1;
  for (let i = 0; i < rows.length; i++) {
    if (rows[i] === row) {
      const rect = row.getBoundingClientRect();
      insertIndex = e.clientY < rect.top + rect.height / 2 ? i : i + 1;
      break;
    }
  }
  if (insertIndex < 0) return;
  insertIndex = Math.min(insertIndex, rows.length);
  const currentChildren = Array.from(listEl.children).filter((c) => c.id !== "inventory-drop-placeholder");
  if (insertIndex >= currentChildren.length) {
    listEl.appendChild(placeholder);
  } else {
    listEl.insertBefore(placeholder, currentChildren[insertIndex]);
  }
}

// 后备箱拖拽：离开（不移除占位条，保持当前插入位）
function inventoryDragLeave(e) {
  // 占位条保留，不做处理
}

// 后备箱拖拽：放下并调整顺序（按占位条位置插入，其他元素已让位）
function inventoryDrop(e) {
  e.preventDefault();
  e.stopPropagation();
  const listEl = document.getElementById("inventory-list");
  const placeholder = document.getElementById("inventory-drop-placeholder");
  if (!listEl || !placeholder || !placeholder.parentNode) {
    inventoryDragSourceIndex = -1;
    return;
  }
  const dropIndex = Array.from(listEl.children).indexOf(placeholder);
  const sourceIndex = inventoryDragSourceIndex;
  placeholder.parentNode.removeChild(placeholder);
  inventoryDragSourceIndex = -1;

  if (sourceIndex === dropIndex || isNaN(sourceIndex) || dropIndex < 0) return;
  const items = inventoryState.items;
  if (sourceIndex < 0 || sourceIndex >= items.length) return;
  const toIndex = dropIndex > sourceIndex ? dropIndex - 1 : dropIndex;
  if (toIndex < 0 || toIndex > items.length) return;
  const [moved] = items.splice(sourceIndex, 1);
  items.splice(toIndex, 0, moved);
  updateInventoryDisplay();
  if (typeof saveGame === "function") saveGame();
}

// 更新库存物品列表
function updateInventoryDisplay() {
  const listEl = document.getElementById("inventory-list");
  if (!listEl) return;

  updateWeightDisplay();
  ensureInventoryTooltipElement();

  const newIds = (typeof gameState !== "undefined" && gameState._newItemIds) ? gameState._newItemIds : [];

  // 根据当前标签页过滤物品
  const isTreasureTab = currentInventoryTab === "treasure";
  const filteredItems = [];
  for (let i = 0; i < inventoryState.items.length; i++) {
    const slot = inventoryState.items[i];
    const config = ITEMS_CONFIG[slot.id];
    if (!config) continue;
    const isTreasure = config.category === "treasure";
    if (isTreasureTab && isTreasure) filteredItems.push({ slot, index: i });
    else if (!isTreasureTab && !isTreasure) filteredItems.push({ slot, index: i });
  }

  if (filteredItems.length === 0) {
    if (isTreasureTab) {
      listEl.innerHTML =
        '<div class="text-center text-gray-500 text-sm py-8">💎 还没有收集到珍品...</div>';
    } else if (inventoryState.items.length === 0) {
      listEl.innerHTML =
        '<div class="text-center text-gray-600 text-sm py-8">后备箱空空如也...</div>';
    } else {
      listEl.innerHTML =
        '<div class="text-center text-gray-600 text-sm py-8">没有普通物品</div>';
    }
    return;
  }

  let html = "";
  for (let fi = 0; fi < filteredItems.length; fi++) {
    const { slot, index: i } = filteredItems[fi];
    const config = ITEMS_CONFIG[slot.id];
    if (!config) continue;

    const isUsable = config.category === "consumable" || config.usable;
    const categoryColors = {
      consumable: "border-green-800 bg-green-900/20",
      material: "border-gray-700 bg-gray-800/30",
      special: "border-yellow-700 bg-yellow-900/20",
      treasure: "border-amber-500 bg-amber-900/20",
    };
    const borderClass = categoryColors[config.category] || "border-gray-700";
    const tooltipHtml = getItemTooltipHtml(config).replace(/"/g, "&quot;");
    const enterClass = newIds.indexOf(slot.id) !== -1 ? " inventory-item-enter" : "";
    const displayName = getItemDisplayName(config);

    html += `
			<div class="inventory-item-row flex items-center gap-2 p-2 rounded-lg border ${borderClass} hover:bg-white/5 transition-colors group${enterClass}" data-slot-index="${i}" data-tooltip-html="${tooltipHtml}" draggable="true" onmouseenter="showInventoryTooltip(this)" onmouseleave="hideInventoryTooltip()" ondragstart="inventoryDragStart(event)" ondragend="inventoryDragEnd(event)" ondragover="inventoryDragOver(event)" ondragleave="inventoryDragLeave(event)" ondrop="inventoryDrop(event)">
				<span class="text-sm font-bold flex-shrink-0 w-5 h-5 rounded flex items-center justify-center" style="color:${config.color}; border: 1px solid ${config.color}40;">${config.name.charAt(0)}</span>
				<div class="flex-1 min-w-0">
					<div class="text-sm truncate" style="color:${config.color}">${displayName}</div>
					<div class="text-xs text-gray-600">${config.weight}kg × ${slot.quantity}</div>
				</div>
				<span class="text-xs text-gray-500 flex-shrink-0 w-10 text-right tabular-nums">×${slot.quantity}</span>
				<div class="flex gap-1 flex-shrink-0 opacity-0 group-hover:opacity-100 transition-opacity">
				<button onclick="discardItemFromInventory('${slot.id}')" class="px-2 py-0.5 text-xs bg-red-900/60 text-red-300 rounded border border-red-700/50 hover:bg-red-700 transition-all">丢弃</button>
				${isUsable ? `<button onclick="useItemFromInventory('${slot.id}')" class="px-2 py-0.5 text-xs bg-green-800/60 text-green-300 rounded border border-green-700/50 hover:bg-green-700 transition-all">使用</button>` : ""}
				</div>
			</div>`;
  }

  listEl.innerHTML = html;
  // 延迟清空，使同一批添加的多个物品都能播渐显
  if (typeof gameState !== "undefined" && gameState._newItemIds && gameState._newItemIds.length)
    setTimeout(function () { gameState._newItemIds = []; }, 50);
}

// 从库存面板使用物品
function useItemFromInventory(itemId) {
  const config = ITEMS_CONFIG[itemId];
  if (!config) return;

  if (useItem(itemId)) {
    hideInventoryTooltip();
    if (config.useEffect && config.useEffect.durability && typeof triggerScreenShake === "function")
      triggerScreenShake();
    // 添加使用反馈到文本区
    const textArea = document.getElementById("textArea");
    if (textArea) {
      // 一次性管理员权限：激活 Debug 并重置 debugUsed 标记
      if (config.onUse && config.onUse.adminPermitActivate) {
        if (typeof gameState !== "undefined") gameState.debugUsed = false;
        // fromAdmin=true：标记为管理员权限模式，禁用设置页 toggle 手动开关
        if (typeof toggleDebugMode === "function") toggleDebugMode(true, true);
        const p = document.createElement("p");
        p.innerHTML = `<span style="color:#22d3ee">🖥️ 令牌已激活！<b>Debug 控制台已开启</b>。使用一次后令牌自动销毁，设置页开关已锁定。</span>`;
        textArea.appendChild(p);
        scrollTextAreaToBottom(textArea);
      // 特殊可使用珍品（有 onUse 配置）
      } else if (config.onUse && config.onUse.useMessage) {
        const p = document.createElement("p");
        p.innerHTML = `<span style="color:${config.color}">✦ ${config.onUse.useMessage}</span>`;
        textArea.appendChild(p);
        scrollTextAreaToBottom(textArea);
      } else if (config.useEffect) {
        const effectParts = [];
        if (config.useEffect.fuel)
          effectParts.push(
            `<span style="color: #eab308;">燃</span>+${config.useEffect.fuel}`,
          );
        if (config.useEffect.durability)
          effectParts.push(
            `<span style="color: #22c55e;">耐</span>+${config.useEffect.durability}`,
          );
        if (config.useEffect.comfort)
          effectParts.push(
            `<span style="color: #60a5fa;">适</span>+${config.useEffect.comfort}`,
          );

        const p = document.createElement("p");
        p.innerHTML = `<span style="color: #4ade80;">✦ 使用了<span style="color:${config.color}">${getItemDisplayName(config)}</span>（${effectParts.join("，")}）</span>`;
        textArea.appendChild(p);
        scrollTextAreaToBottom(textArea);
      }
    }
    if (document.getElementById("crafting-modal")) showCraftingModal();
  }
}

// 从库存面板丢弃物品
function discardItemFromInventory(itemId) {
  const config = ITEMS_CONFIG[itemId];
  if (!config) return;
  if (!hasItem(itemId)) return;
  removeItem(itemId, 1);
  hideInventoryTooltip();
  if (typeof saveGame === "function") saveGame();
  const textArea = document.getElementById("textArea");
  if (textArea) {
    const p = document.createElement("p");
    p.innerHTML = `<span style="color: #94a3b8;">✦ 丢弃了 <span style="color:${config.color}">${getItemDisplayName(config)}</span> ×1</span>`;
    textArea.appendChild(p);
    scrollTextAreaToBottom(textArea);
  }
  if (document.getElementById("crafting-modal")) showCraftingModal();
}

// ===== 通用 Modal 淡出关闭工具 =====
// 给指定 id 的 modal 播放淡出动画后删除，回调在删除后执行
function closeModalWithFade(modalId, callback) {
  const modal = document.getElementById(modalId);
  if (!modal) { if (callback) callback(); return; }
  // 避免重复触发
  if (modal.dataset.closing) return;
  modal.dataset.closing = "1";
  modal.style.animation = "eventModalFadeOut 0.18s ease-in both";
  let done = false;
  const finish = () => {
    if (done) return;
    done = true;
    modal.remove();
    if (callback) callback();
  };
  modal.addEventListener("animationend", finish, { once: true });
  // 兜底：200ms 后强制执行，防止动画未触发导致卡死
  setTimeout(finish, 200);
}

// ===== 后备箱升级弹窗 =====
function showTrunkUpgradeModal() {
  const oldModal = document.getElementById("trunk-upgrade-modal");
  if (oldModal) oldModal.remove();

  const level = inventoryState.trunkLevel || 1;
  const isMax = level >= TRUNK_MAX_LEVEL;

  // 构建等级进度点
  let dotsHtml = "";
  for (let i = 1; i <= TRUNK_MAX_LEVEL; i++) {
    const active = i <= level;
    dotsHtml += `<span style="display:inline-block;width:10px;height:10px;border-radius:50%;margin:0 3px;background:${active ? "#fbbf24" : "#374151"};border:1px solid ${active ? "#f59e0b" : "#4b5563"};${i === level ? "box-shadow:0 0 6px #fbbf24;" : ""}"></span>`;
  }

  let costHtml = "";
  let canDo = false;
  if (!isMax) {
    const next = TRUNK_UPGRADES[level + 1];
    canDo = canUpgradeTrunk();
    // 材料列表（无金币消耗）
    for (const mat of next.materials) {
      const cfg = ITEMS_CONFIG[mat.id];
      const owned = getItemQuantity(mat.id);
      const enough = owned >= mat.qty;
      const name = cfg ? cfg.name : mat.id;
      const color = cfg ? cfg.color : "#9ca3af";
      costHtml += `<div style="display:flex;align-items:center;gap:6px;margin-bottom:4px;">
        <span style="color:${color}">${name} ×${mat.qty}</span>
        <span style="color:${enough ? "#22c55e" : "#ef4444"};font-size:0.75rem;">(拥有 ${owned})</span>
      </div>`;
    }
  }

  const modal = document.createElement("div");
  modal.id = "trunk-upgrade-modal";
  modal.className = "fixed inset-0 z-[9999] flex items-center justify-center";
  modal.style.animation = "eventModalFadeIn 0.25s ease-out both";
  modal.innerHTML = `
    <div class="absolute inset-0 bg-black/70" onclick="closeTrunkUpgradeModal()"></div>
    <div style="position:relative;background:#1a1a2e;border:2px solid #3b82f6;border-radius:16px;padding:24px;max-width:min(340px,94vw);width:90%;box-shadow:0 0 40px rgba(59,130,246,0.3);animation:eventInnerSlideIn 0.3s cubic-bezier(0.22,1,0.36,1) both;box-sizing:border-box;">
      <h3 style="text-align:center;font-size:1.15rem;font-weight:bold;color:#60a5fa;margin-bottom:6px;">🔧 后备箱升级</h3>
      <div style="text-align:center;margin-bottom:12px;">${dotsHtml}</div>
      <div style="text-align:center;color:#d1d5db;font-size:0.95rem;margin-bottom:4px;">
        当前等级：<span style="color:#fbbf24;font-weight:bold;">Lv.${level}</span>
        <span style="color:#6b7280;margin-left:6px;">载重上限 ${inventoryState.maxWeight}kg</span>
      </div>
      ${isMax
        ? `<div style="text-align:center;color:#22c55e;font-size:0.9rem;margin:16px 0;">✦ 已达到最高等级！</div>`
        : `<div style="border-top:1px solid #374151;margin:12px 0;padding-top:12px;">
            <div style="color:#9ca3af;font-size:0.8rem;margin-bottom:8px;">升级至 Lv.${level + 1}（载重 +${TRUNK_UPGRADES[level + 1].bonusWeight}kg）需要：</div>
            ${costHtml}
          </div>
          <button onclick="doTrunkUpgrade()" ${canDo ? "" : "disabled"}
            style="display:block;width:100%;padding:10px;border-radius:8px;font-weight:bold;font-size:0.95rem;border:1px solid ${canDo ? "#3b82f6" : "#4b5563"};background:${canDo ? "linear-gradient(to right,#1e40af,#2563eb)" : "#1f2937"};color:${canDo ? "#fff" : "#6b7280"};cursor:${canDo ? "pointer" : "not-allowed"};transition:all 0.2s;"
            ${canDo ? 'onmouseenter="this.style.background=\'linear-gradient(to right,#2563eb,#3b82f6)\'"  onmouseleave="this.style.background=\'linear-gradient(to right,#1e40af,#2563eb)\'"' : ""}>
            ${canDo ? "⬆ 升级" : "⬆ 材料不足"}
          </button>`
      }
      <button onclick="closeTrunkUpgradeModal()"
        style="display:block;width:100%;margin-top:8px;padding:8px;border-radius:8px;background:transparent;color:#9ca3af;border:1px solid #374151;cursor:pointer;font-size:0.85rem;transition:all 0.2s;"
        onmouseenter="this.style.borderColor='#6b7280';this.style.color='#d1d5db'" onmouseleave="this.style.borderColor='#374151';this.style.color='#9ca3af'">
        关闭
      </button>
    </div>
  `;
  document.body.appendChild(modal);
}

function closeTrunkUpgradeModal() {
  const modal = document.getElementById("trunk-upgrade-modal");
  if (!modal) return;
  modal.style.animation = "eventModalFadeOut 0.2s ease-in both";
  modal.addEventListener("animationend", () => {
    modal.remove();
    // 如果超载弹窗还在，刷新它
    if (document.getElementById("overweight-modal")) {
      showOverweightModal();
    }
  }, { once: true });
}

function doTrunkUpgrade() {
  if (upgradeTrunk()) {
    if (typeof playUpgrade === "function") playUpgrade();
    // 升级成功，重新打开弹窗以刷新内容
    const modal = document.getElementById("trunk-upgrade-modal");
    if (modal) modal.remove();
    showTrunkUpgradeModal();
    // 如果超载弹窗还在，也刷新它
    if (document.getElementById("overweight-modal")) {
      showOverweightModal();
    }
  }
}

// 显示商人交易界面
// 生成商店物品的富文本效果摘要（用于商人界面）
function getMerchantItemEffectHtml(config) {
  const parts = [];

  // 消耗品：useEffect
  if (config.useEffect) {
    const ue = config.useEffect;
    if (ue.fuel)       parts.push(`<span style="color:#eab308">⛽ 燃油 +${ue.fuel}%</span>`);
    if (ue.durability) parts.push(`<span style="color:#22c55e">🔧 耐久 +${ue.durability}%</span>`);
    if (ue.comfort)    parts.push(`<span style="color:#a78bfa">💺 舒适 +${ue.comfort}%</span>`);
  }

  // 随机金币（如马年红包）
  if (config.randomGold) {
    parts.push(`<span style="color:#fbbf24">🪙 随机 ${config.randomGold[0]}~${config.randomGold[1]} 金币</span>`);
  }

  // 被动效果
  if (config.passive) {
    const p = config.passive;
    if (p.type === "lucky_charm") {
      parts.push(`<span style="color:#34d399">✨ 被动：2% 随机物资（神秘事件 10%）</span>`);
    } else if (p.type === "music_box") {
      parts.push(`<span style="color:#f472b6">🎵 被动：20% 回舒适 1%（神秘事件 100%）</span>`);
    } else if (p.type === "admin_permit") {
      parts.push(`<span style="color:#22d3ee">🖥️ 使用：激活 Debug 一次，用后自毁</span>`);
    } else {
      if (p.triggerChance != null) {
        const pct = Math.round((p.ruinsTriggerChance ?? p.triggerChance) * 100);
        const basePct = Math.round(p.triggerChance * 100);
        const tagNote = p.ruinsTriggerChance ? `（废墟事件 ${pct}%）` : "";
        if (p.fuel)    parts.push(`<span style="color:#eab308">⛽ 被动${basePct}%：燃油 +${p.fuel}%${tagNote}</span>`);
        if (p.durability) parts.push(`<span style="color:#22c55e">🔧 被动${basePct}%：耐久 +${p.durability}%${tagNote}</span>`);
        if (p.comfort) parts.push(`<span style="color:#a78bfa">💺 被动${basePct}%：舒适 +${p.comfort}%${tagNote}</span>`);
        if (p.goldMin != null) parts.push(`<span style="color:#fbbf24">🪙 被动${basePct}%：金币 +${p.goldMin}~${p.goldMax}</span>`);
        if (p.addItems) {
          for (const ai of p.addItems) {
            const ic = ITEMS_CONFIG[ai.id];
            const label = ic ? `<span style="color:${ic.color}">${ic.name}</span>` : ai.id;
            parts.push(`<span style="color:#9ca3af">📦 被动${basePct}%：获得 ${label}×${ai.quantity}${tagNote}</span>`);
          }
        }
      }
      // 条件型被动（如海市蜃楼雕塑）
      if (p.type === "condition" && p.condition === "fuel_low") {
        parts.push(`<span style="color:#f97316">⚠️ 燃油危急时：回燃油 +${p.fuel}%（触发后销毁）</span>`);
      }
    }
  }

  // 可使用珍品（onUse.useEffect）
  if (config.onUse && config.onUse.useEffect) {
    const ue = config.onUse.useEffect;
    const suffix = config.onUse.transformTo ? "（使用后变形）" : "";
    if (ue.durability) parts.push(`<span style="color:#22c55e">🔧 使用：耐久 +${ue.durability}%${suffix}</span>`);
    if (ue.fuel)       parts.push(`<span style="color:#eab308">⛽ 使用：燃油 +${ue.fuel}%${suffix}</span>`);
    if (ue.comfort)    parts.push(`<span style="color:#a78bfa">💺 使用：舒适 +${ue.comfort}%${suffix}</span>`);
  }

  // 合成材料（无明确效果的材料类）
  if (parts.length === 0) {
    if (config.category === "material") {
      parts.push(`<span style="color:#6b7280">🔩 合成材料，可用于制造消耗品</span>`);
    } else if (config.category === "special") {
      parts.push(`<span style="color:#7c3aed">✦ 特殊物品，持有可解锁特殊效果</span>`);
    } else {
      // 截取 description 前 30 字作为兜底
      const short = config.description ? config.description.slice(0, 36) + (config.description.length > 36 ? "…" : "") : "";
      if (short) parts.push(`<span style="color:#6b7280">${short}</span>`);
    }
  }

  return parts.join('<span style="color:#374151"> · </span>');
}

function showMerchantModal(merchantId) {
  const merchant = MERCHANT_CONFIG[merchantId];
  if (!merchant) return;

  // 移除旧modal
  const oldModal = document.getElementById("merchant-modal");
  if (oldModal) oldModal.remove();

  const hasElderlyDiscount = typeof truckState !== "undefined" && truckState.passengers && truckState.passengers.includes("年迈妇人");
  let itemsHtml = "";
  for (const listing of merchant.items) {
    const config = ITEMS_CONFIG[listing.itemId];
    if (!config) continue;
    const owned = getItemQuantity(listing.itemId);
    // 仅可售卖（无buyPrice）的物品，玩家未持有时不显示
    if (listing.buyPrice == null && owned === 0) continue;
    const buyPrice = typeof getEffectiveBuyPrice === "function" ? getEffectiveBuyPrice(merchantId, listing.itemId) : listing.buyPrice;
    const sellPrice = typeof getEffectiveSellPrice === "function" ? getEffectiveSellPrice(merchantId, listing.itemId) : listing.sellPrice;

    // 生成购买按钮（仅限有 buyPrice 的物品）
    let buyButtonHtml = "";
    if (listing.buyPrice != null) {
      // 检查购买条件：金币足够即可购买（超载在离开商人时统一判定）
      const hasEnoughGold = inventoryState.gold >= buyPrice;
      const isOverweight = getInventoryWeight() + config.weight > inventoryState.maxWeight;

      if (hasEnoughGold) {
        buyButtonHtml = `<button onclick="merchantBuy('${merchantId}','${listing.itemId}', this)" 
          class="px-2 py-0.5 text-xs ${isOverweight ? 'bg-yellow-900/60 text-yellow-300 border-yellow-700/50 hover:bg-yellow-700' : 'bg-green-800/60 text-green-300 border-green-700/50 hover:bg-green-700'} rounded border transition-all"
          ${isOverweight ? 'title="⚠️ 购买后将超载"' : ''}>
          买 <span class="text-yellow-400">${buyPrice}🪙</span>${isOverweight ? ' ⚠️' : ''}
        </button>`;
      } else {
        buyButtonHtml = `<button disabled 
          class="px-2 py-0.5 text-xs bg-gray-700 text-gray-500 rounded border border-gray-700 cursor-not-allowed opacity-50" 
          title="金币不足">
          买 <span class="text-yellow-400/50">${buyPrice}🪙</span>
        </button>`;
      }
    }

    itemsHtml += `
			<div class="flex items-center gap-2 p-2 border border-gray-700 rounded-lg bg-gray-800/30">
				<span class="text-sm font-bold flex-shrink-0 w-6 h-6 rounded flex items-center justify-center" style="color:${config.color}; border: 1px solid ${config.color}40;">${config.name.charAt(0)}</span>
				<div class="flex-1 min-w-0">
					<div class="text-sm" style="color:${config.color}">${getItemDisplayName(config)} <span class="text-xs text-gray-600">(${config.weight}kg)</span></div>
					<div class="text-xs mt-0.5 leading-relaxed">${getMerchantItemEffectHtml(config)}</div>
					<div class="text-xs text-gray-600 mt-0.5 leading-tight" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;" title="${config.description ? config.description.replace(/"/g, '&quot;') : ''}">${config.description || ""}</div>
				</div>
				<div class="flex flex-col gap-1 items-end flex-shrink-0">
					<span class="text-xs text-gray-500">持有: ${owned}</span>
					<div class="flex gap-1">
						${buyButtonHtml}
						<button onclick="merchantSell('${merchantId}','${listing.itemId}', this)"
							class="px-2 py-0.5 text-xs bg-red-800/60 text-red-300 rounded border border-red-700/50 hover:bg-red-700 transition-all"
							${owned === 0 ? 'disabled style="opacity:0.3"' : ""}>
						卖 <span class="text-yellow-400">${sellPrice}🪙</span>
						</button>
					</div>
				</div>
			</div>`;
  }

  const modal = document.createElement("div");
  modal.id = "merchant-modal";
  modal.className = "fixed inset-0 z-50 flex items-center justify-center";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";
  modal.innerHTML = `
		<div class="absolute inset-0 bg-black/70" onclick="closeMerchantModal()"></div>
		<div class="relative bg-[#1a1a2e] border-2 border-[#c41e3a] rounded-xl p-6 max-w-md w-full mx-4 max-h-[80vh] flex flex-col">
			<h3 class="text-lg font-bold text-[#c41e3a] mb-1">${merchant.name}</h3>
			<p class="text-sm text-gray-400 mb-1 italic">"${merchant.greeting}"</p>
			${hasElderlyDiscount ? '<p class="text-xs text-green-400/90 mb-3">👵 年迈妇人同行，商人给了优惠价！</p>' : '<div class="mb-3"></div>'}
			<div class="flex items-center gap-3 mb-3 px-3 py-1.5 bg-gray-800/60 rounded-lg border border-gray-700">
				<span class="text-sm text-gray-400">持有</span>
				<span class="text-yellow-400 font-bold text-base" id="merchant-gold">${inventoryState.gold}</span>
				<span class="text-sm">🪙</span>
				<span class="ml-auto text-xs ${getInventoryWeight() > inventoryState.maxWeight ? 'text-red-400 font-bold' : getInventoryWeight() >= inventoryState.maxWeight ? 'text-yellow-400' : 'text-gray-500'}">载重 ${getInventoryWeight()}/${inventoryState.maxWeight}kg${getInventoryWeight() > inventoryState.maxWeight ? ' ⚠️' : ''}</span>
			</div>
			<div class="text-area-scroll space-y-2 overflow-y-auto flex-1 pr-1">${itemsHtml}</div>
			<button onclick="closeMerchantModal()" 
				class="mt-4 w-full py-2 bg-gray-700 text-gray-300 rounded-lg hover:bg-gray-600 transition-colors">
				离开
			</button>
		</div>`;
  document.body.appendChild(modal);
}

// 商人购买操作
function merchantBuy(merchantId, itemId, btn) {
  if (buyItem(merchantId, itemId)) {
    // 保存滚动位置后刷新界面
    const modal = document.getElementById("merchant-modal");
    const scrollEl = modal && modal.querySelector(".text-area-scroll");
    const scrollPos = scrollEl ? scrollEl.scrollTop : 0;
    if (modal) modal.remove();
    showMerchantModal(merchantId);
    // 恢复滚动位置
    const newModal = document.getElementById("merchant-modal");
    const newScrollEl = newModal && newModal.querySelector(".text-area-scroll");
    if (newScrollEl) newScrollEl.scrollTop = scrollPos;
  }
}

// 商人出售操作
function merchantSell(merchantId, itemId, btn) {
  if (sellItem(merchantId, itemId)) {
    // 保存滚动位置后刷新界面
    const modal = document.getElementById("merchant-modal");
    const scrollEl = modal && modal.querySelector(".text-area-scroll");
    const scrollPos = scrollEl ? scrollEl.scrollTop : 0;
    if (modal) modal.remove();
    showMerchantModal(merchantId);
    // 恢复滚动位置
    const newModal = document.getElementById("merchant-modal");
    const newScrollEl = newModal && newModal.querySelector(".text-area-scroll");
    if (newScrollEl) newScrollEl.scrollTop = scrollPos;
  }
}

// 主动关闭商人界面（用户点击"离开"按钮）
function closeMerchantModal() {
  closeModalWithFade("merchant-modal", () => {
    // 检查超载
    if (typeof checkOverweightAndShow === "function" && checkOverweightAndShow()) return;
    resumeGameAfterModal();
  });
}

// 制作台是否为独立打开（非事件触发）
let _craftingStandalone = false;
// 制作台是否从超载界面打开
let _craftingFromOverweight = false;

// 从后备箱面板打开制作台（独立模式，不影响游戏进程）
function openStandaloneCrafting() {
  _craftingStandalone = true;
  showCraftingModal();
}

// 显示合成界面
function showCraftingModal() {
  const oldModal = document.getElementById("crafting-modal");
  if (oldModal) oldModal.remove();

  let recipesHtml = "";
  for (const [recipeId, recipe] of Object.entries(CRAFTING_RECIPES)) {
    const resultConfig = ITEMS_CONFIG[recipe.result.itemId];
    const available = canCraft(recipeId);
    const unavailableReason = !available && typeof getCraftUnavailableReason === "function" ? getCraftUnavailableReason(recipeId) : "";
    const titleAttr = unavailableReason ? ` title="${unavailableReason}"` : "";

    let materialsHtml = recipe.materials
      .map((mat) => {
        const matConfig = ITEMS_CONFIG[mat.itemId];
        const owned = getItemQuantity(mat.itemId);
        const enough = owned >= mat.quantity;
        const nameColor = enough ? "#4ade80" : "#f87171";
        const ownedColor = enough ? "#2d5a3d" : "#5a2d2d";
        return `<span style="color:${nameColor}">${getItemDisplayName(matConfig)}×${mat.quantity}<span style="color:${ownedColor}">(${owned})</span></span>`;
      })
      .join(" + ");

    const reasonHtml = unavailableReason ? `<span class="text-xs ml-1" style="color:#783a3a">(${unavailableReason})</span>` : "";

    recipesHtml += `
			<div class="p-3 border ${available ? "border-green-700 bg-green-900/10" : "border-gray-700 bg-gray-800/20"} rounded-lg"${titleAttr}>
				<div class="flex items-center gap-2 mb-2">
					<span class="text-sm font-bold" style="color:${resultConfig.color}">${getItemDisplayName(resultConfig)}</span>
					<span class="text-xs text-gray-500">×${recipe.result.quantity}</span>
					${reasonHtml}
					<button onclick="doCraft('${recipeId}')"
						class="ml-auto px-3 py-1 text-xs rounded border transition-all ${
              available
                ? "bg-green-800/60 text-green-300 border-green-700/50 hover:bg-green-700 cursor-pointer"
                : "bg-gray-800 text-gray-600 border-gray-700 cursor-not-allowed"
            }"
						${available ? "" : "disabled"}${!available ? titleAttr : ""}>
						合成
					</button>
				</div>
				<div class="text-xs mt-0.5 mb-1.5 leading-relaxed">${getMerchantItemEffectHtml(resultConfig)}</div>
				<div class="text-xs text-gray-500 flex flex-wrap gap-1">需要：${materialsHtml}</div>
			</div>`;
  }

  // 独立模式挂在 body 上（fixed 定位），事件模式挂在 game-canvas 上（absolute 定位）
  const isStandalone = _craftingStandalone;
  const modal = document.createElement("div");
  modal.id = "crafting-modal";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";

  if (isStandalone) {
    modal.className = "fixed inset-0 z-50 flex items-center justify-center";
    modal.innerHTML = `
      <div class="absolute inset-0 bg-black/70" onclick="closeCraftingModal()"></div>
      <div class="relative bg-[#1a1a2e] border-2 border-[#facc15] rounded-xl p-6 max-w-md w-full mx-4 max-h-[80vh] flex flex-col">
        <h3 class="text-lg font-bold text-[#facc15] mb-3">🔨 制作台</h3>
        <div class="text-area-scroll space-y-2 overflow-y-auto flex-1 pr-1">${recipesHtml}</div>
        <button onclick="closeCraftingModal()" 
          class="mt-4 w-full py-2 bg-gray-700 text-gray-300 rounded-lg hover:bg-gray-600 transition-colors">
          关闭
        </button>
      </div>`;
    document.body.appendChild(modal);
  } else {
    const gameCanvas = document.getElementById("game-canvas");
    if (!gameCanvas) return;
    modal.className = "absolute inset-0 z-50 flex items-center justify-center";
    modal.innerHTML = `
      <div class="absolute inset-0 pointer-events-auto" onclick="closeCraftingModal()"></div>
      <div class="relative bg-[#1a1a2e] border-2 border-[#c41e3a] rounded-xl p-6 max-w-md w-full mx-4 max-h-[90%] flex flex-col">
        <h3 class="text-lg font-bold text-[#c41e3a] mb-3"><span style="color: #facc15;">制</span>作台</h3>
        <div class="text-area-scroll space-y-2 overflow-y-auto flex-1 pr-1">${recipesHtml}</div>
        <button onclick="closeCraftingModal()" 
          class="mt-4 w-full py-2 bg-gray-700 text-gray-300 rounded-lg hover:bg-gray-600 transition-colors">
          离开
        </button>
      </div>`;
    gameCanvas.appendChild(modal);
  }
}

// 执行合成
function doCraft(recipeId) {
  if (craftItem(recipeId)) {
    if (typeof playCraft === "function") playCraft();
    const recipe = CRAFTING_RECIPES[recipeId];
    const config = ITEMS_CONFIG[recipe.result.itemId];

    // 文本反馈
    const textArea = document.getElementById("textArea");
    if (textArea) {
      const p = document.createElement("p");
      p.innerHTML = `<span style="color: #facc15;"><span style="color: #facc15;">制</span>作了<span style="color:${config.color}">${getItemDisplayName(config)}</span>×${recipe.result.quantity}！</span>`;
      textArea.appendChild(p);
      scrollTextAreaToBottom(textArea);
    }

    // 保存滚动位置后刷新界面
    const modal = document.getElementById("crafting-modal");
    const scrollEl = modal && modal.querySelector(".text-area-scroll");
    const scrollPos = scrollEl ? scrollEl.scrollTop : 0;
    if (modal) modal.remove();
    showCraftingModal();
    // 恢复滚动位置
    const newModal = document.getElementById("crafting-modal");
    const newScrollEl = newModal && newModal.querySelector(".text-area-scroll");
    if (newScrollEl) newScrollEl.scrollTop = scrollPos;
  }
}

// 关闭合成界面
function closeCraftingModal() {
  const isStandalone = _craftingStandalone;
  const fromOverweight = _craftingFromOverweight;

  closeModalWithFade("crafting-modal", () => {
    if (isStandalone) {
      _craftingStandalone = false;
      _craftingFromOverweight = false;
      updateInventoryDisplay();
      // 从超载界面打开的制作台，关闭后刷新超载弹窗
      if (fromOverweight && document.getElementById("overweight-modal")) {
        showOverweightModal();
      }
      return;
    }
    // 事件模式：检查超载后恢复游戏
    if (typeof checkOverweightAndShow === "function" && checkOverweightAndShow()) return;
    resumeGameAfterModal();
  });
}

// 显示休息/使用物品界面
function showRestModal() {
  const oldModal = document.getElementById("rest-modal");
  if (oldModal) oldModal.remove();

  // 只显示消耗品
  const consumables = inventoryState.items.filter((slot) => {
    const config = ITEMS_CONFIG[slot.id];
    return config && config.category === "consumable";
  });

  let itemsHtml = "";
  if (consumables.length === 0) {
    itemsHtml =
      '<div class="text-center text-gray-600 text-sm py-6">没有可以使用的物品...</div>';
  } else {
    for (const slot of consumables) {
      const config = ITEMS_CONFIG[slot.id];
      const effectParts = [];
      if (config.useEffect.fuel)
        effectParts.push(
          `<span style="color: #eab308;">燃</span>+${config.useEffect.fuel}`,
        );
      if (config.useEffect.durability)
        effectParts.push(
          `<span style="color: #22c55e;">耐</span>+${config.useEffect.durability}`,
        );
      if (config.useEffect.comfort)
        effectParts.push(
          `<span style="color: #60a5fa;">适</span>+${config.useEffect.comfort}`,
        );

      itemsHtml += `
				<div class="flex items-center gap-2 p-2 border border-gray-700 rounded-lg bg-gray-800/30 hover:bg-white/5 transition-colors">
					<span class="text-sm font-bold flex-shrink-0 w-6 h-6 rounded flex items-center justify-center" style="color:${config.color}; border: 1px solid ${config.color}40;">${config.name.charAt(0)}</span>
					<div class="flex-1">
						<div class="text-sm" style="color:${config.color}">${getItemDisplayName(config)} <span class="text-xs text-gray-500">×${slot.quantity}</span></div>
						<div class="text-xs text-green-400">${effectParts.join("  ")}</div>
					</div>
					<button onclick="restUseItem('${slot.id}')"
						class="px-3 py-1 text-xs bg-green-800/60 text-green-300 rounded border border-green-700/50 hover:bg-green-700 transition-all">
						使用
					</button>
				</div>`;
    }
  }

  const modal = document.createElement("div");
  modal.id = "rest-modal";
  modal.className = "fixed inset-0 z-50 flex items-center justify-center";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";
  modal.innerHTML = `
		<div class="absolute inset-0 bg-black/70" onclick="closeRestModal()"></div>
		<div class="relative bg-[#1a1a2e] border-2 border-[#c41e3a] rounded-xl p-6 max-w-md w-full mx-4 max-h-[80vh] flex flex-col">
			<h3 class="text-lg font-bold text-[#c41e3a] mb-1">路边休息</h3>
			<p class="text-sm text-gray-400 mb-3">停下来休整一下，使用物品恢复状态。</p>
			<div class="flex gap-4 mb-3 text-xs">
				<span class="text-yellow-500"><span style="color: #eab308;">燃</span> ${Math.round(truckState.fuel)}%</span>
				<span class="text-green-500"><span style="color: #22c55e;">耐</span> ${Math.round(truckState.durability)}%</span>
				<span class="text-blue-400"><span style="color: #60a5fa;">适</span> ${Math.round(truckState.comfort)}%</span>
			</div>
			<div class="text-area-scroll space-y-2 overflow-y-auto flex-1 pr-1">${itemsHtml}</div>
			<button onclick="closeRestModal()" 
				class="mt-4 w-full py-2 bg-gray-700 text-gray-300 rounded-lg hover:bg-gray-600 transition-colors">
				继续上路
			</button>
		</div>`;
  document.body.appendChild(modal);
}

// 休息界面中使用物品
function restUseItem(itemId) {
  if (useItem(itemId)) {
    const modal = document.getElementById("rest-modal");
    if (modal) modal.remove();
    showRestModal();
  }
}

// 主动关闭休息界面
function closeRestModal() {
  closeModalWithFade("rest-modal", () => {
    // 检查超载
    if (typeof checkOverweightAndShow === "function" && checkOverweightAndShow()) return;
    resumeGameAfterModal();
  });
}

// 初始化所有显示
function initInventoryDisplay() {
  updateTruckStatusDisplay();
  updateGoldDisplay();
  updatePassengerListDisplay();
  updateInventoryDisplay();
}

// 关闭框后恢复游戏运行（委托给event-handler中的resumeGame）
function resumeGameAfterModal() {
  resumeGame();
}

// ========== 超载弹窗 ==========

// 检查是否超载并弹出超载界面
function checkOverweightAndShow() {
  if (getInventoryWeight() > inventoryState.maxWeight) {
    showOverweightModal();
    return true;
  }
  return false;
}

// 显示超载弹窗
function showOverweightModal() {
  // 保存滚动位置
  let savedScroll = 0;
  const oldModal = document.getElementById("overweight-modal");
  if (oldModal) {
    const scrollBox = oldModal.querySelector("#overweight-items-list");
    if (scrollBox) savedScroll = scrollBox.scrollTop;
    oldModal.remove();
  }

  const currentWeight = getInventoryWeight();
  const maxWeight = inventoryState.maxWeight;
  const isStillOverweight = currentWeight > maxWeight;

  // 收集所有可丢弃的物品
  let itemsHtml = "";
  const allItems = inventoryState.items.filter((slot) => {
    const config = ITEMS_CONFIG[slot.id];
    return config;
  });

  if (allItems.length === 0) {
    itemsHtml = '<div class="text-center text-gray-600 text-sm py-6">后备箱空空如也...</div>';
  } else {
    for (const slot of allItems) {
      const config = ITEMS_CONFIG[slot.id];
      if (!config) continue;
      const categoryColors = {
        consumable: "border-green-800 bg-green-900/20",
        material: "border-gray-700 bg-gray-800/30",
        special: "border-yellow-700 bg-yellow-900/20",
        treasure: "border-amber-500 bg-amber-900/20",
      };
      const borderClass = categoryColors[config.category] || "border-gray-700";
      const displayName = getItemDisplayName(config);
      const isConsumable = config.category === "consumable";
      // 消耗品效果描述
      let effectHint = "";
      if (isConsumable && config.useEffect) {
        const ep = [];
        if (config.useEffect.fuel) ep.push("燃+" + config.useEffect.fuel);
        if (config.useEffect.durability) ep.push("耐+" + config.useEffect.durability);
        if (config.useEffect.comfort) ep.push("适+" + config.useEffect.comfort);
        if (ep.length > 0) effectHint = `<div class="text-xs text-green-600">${ep.join(" / ")}</div>`;
      }
      const useBtn = isConsumable
        ? `<button onclick="overweightUseItem('${slot.id}')" 
            class="px-2 py-0.5 text-xs bg-green-900/60 text-green-300 rounded border border-green-700/50 hover:bg-green-700 transition-all flex-shrink-0">使用</button>`
        : "";
      itemsHtml += `
        <div class="flex items-center gap-2 p-2 rounded-lg border ${borderClass} hover:bg-white/5 transition-colors">
          <span class="text-sm font-bold flex-shrink-0 w-5 h-5 rounded flex items-center justify-center" style="color:${config.color}; border: 1px solid ${config.color}40;">${config.name.charAt(0)}</span>
          <div class="flex-1 min-w-0">
            <div class="text-sm truncate" style="color:${config.color}">${displayName}</div>
            <div class="text-xs text-gray-600">${config.weight}kg × ${slot.quantity}</div>
            ${effectHint}
          </div>
          <span class="text-xs text-gray-500 flex-shrink-0 w-10 text-right">×${slot.quantity}</span>
          ${useBtn}
          <button onclick="overweightDiscard('${slot.id}')" 
            class="px-2 py-0.5 text-xs bg-red-900/60 text-red-300 rounded border border-red-700/50 hover:bg-red-700 transition-all flex-shrink-0">
            丢弃
          </button>
        </div>`;
    }
  }

  const weightPercent = Math.min(100, Math.round((currentWeight / maxWeight) * 100));
  const barColor = isStillOverweight ? "#ef4444" : "#22c55e";

  const modal = document.createElement("div");
  modal.id = "overweight-modal";
  modal.className = "fixed inset-0 z-50 flex items-center justify-center";
  modal.innerHTML = `
    <div class="absolute inset-0 bg-black/80"></div>
    <div class="relative bg-[#1a1a2e] border-2 border-[#f59e0b] rounded-xl p-6 max-w-md w-full mx-4 max-h-[80vh] flex flex-col">
      <h3 class="text-lg font-bold text-[#f59e0b] mb-1">⚠️ 后备箱超载！</h3>
      <p class="text-sm text-gray-400 mb-3">皮卡不堪重负，请使用或丢弃一些物品以减轻载重。</p>
      
      <div class="mb-3">
        <div class="flex justify-between text-xs mb-1">
          <span class="text-gray-400">载重</span>
          <span id="overweight-weight-text" style="color:${isStillOverweight ? '#ef4444' : '#22c55e'}">${currentWeight}kg / ${maxWeight}kg</span>
        </div>
        <div class="w-full h-2 bg-gray-800 rounded-full overflow-hidden">
          <div id="overweight-weight-bar" class="h-full rounded-full transition-all duration-300" style="width:${weightPercent}%; background:${barColor}"></div>
        </div>
      </div>

      <div id="overweight-items-list" class="text-area-scroll space-y-2 overflow-y-auto flex-1 pr-1">${itemsHtml}</div>
      
      <div class="mt-3 flex gap-2">
        ${(inventoryState.trunkLevel || 1) < TRUNK_MAX_LEVEL ? `<button onclick="overweightOpenUpgrade()"
          class="flex-1 py-1.5 text-sm bg-blue-900/60 text-blue-300 rounded-lg border border-blue-700/50 hover:bg-blue-800 transition-all">
          ⬆ 升级后备箱
        </button>` : ""}
        <button onclick="overweightOpenCrafting()"
          class="flex-1 py-1.5 text-sm bg-yellow-900/60 text-yellow-300 rounded-lg border border-yellow-700/50 hover:bg-yellow-800 transition-all">
          🔨 制作
        </button>
      </div>

      <div class="mt-2 flex flex-col gap-2">
        <button id="overweight-continue-btn" onclick="closeOverweightModal()" 
          class="w-full py-2 rounded-lg transition-colors ${isStillOverweight ? 'bg-gray-800 text-gray-600 cursor-not-allowed' : 'bg-green-800 text-green-200 hover:bg-green-700'}"
          ${isStillOverweight ? 'disabled' : ''}>
          ${isStillOverweight ? '⛔ 载重过高，无法前进' : '✅ 继续前进'}
        </button>
        <button onclick="overweightEnding()" 
          class="w-full py-2 bg-amber-900/60 text-amber-300 rounded-lg border border-amber-700/50 hover:bg-amber-800 transition-colors text-sm">
          📦 满载而归...吗？
        </button>
      </div>
    </div>`;
  document.body.appendChild(modal);

  // 恢复滚动位置
  if (savedScroll > 0) {
    const scrollBox = modal.querySelector("#overweight-items-list");
    if (scrollBox) scrollBox.scrollTop = savedScroll;
  }
}

// 超载界面中丢弃物品
function overweightDiscard(itemId) {
  const config = ITEMS_CONFIG[itemId];
  if (!config) return;
  if (!hasItem(itemId)) return;

  removeItem(itemId, 1);
  if (typeof saveGame === "function") saveGame();

  const textArea = document.getElementById("textArea");
  if (textArea) {
    const p = document.createElement("p");
    p.innerHTML = `<span style="color: #94a3b8;">✦ 丢弃了 <span style="color:${config.color}">${getItemDisplayName(config)}</span> ×1</span>`;
    textArea.appendChild(p);
    scrollTextAreaToBottom(textArea);
  }

  // 刷新超载弹窗
  showOverweightModal();
  // 同步更新后备箱面板
  updateInventoryDisplay();
}

// 超载界面中使用消耗品
function overweightUseItem(itemId) {
  const config = ITEMS_CONFIG[itemId];
  if (!config || config.category !== "consumable") return;
  if (!hasItem(itemId)) return;

  if (useItem(itemId)) {
    if (config.useEffect && config.useEffect.durability && typeof triggerScreenShake === "function")
      triggerScreenShake();

    const textArea = document.getElementById("textArea");
    if (textArea) {
      const effectParts = [];
      if (config.useEffect && config.useEffect.fuel)
        effectParts.push(`<span style="color: #eab308;">燃</span>+${config.useEffect.fuel}`);
      if (config.useEffect && config.useEffect.durability)
        effectParts.push(`<span style="color: #22c55e;">耐</span>+${config.useEffect.durability}`);
      if (config.useEffect && config.useEffect.comfort)
        effectParts.push(`<span style="color: #60a5fa;">适</span>+${config.useEffect.comfort}`);
      const p = document.createElement("p");
      p.innerHTML = `<span style="color: #4ade80;">✦ 使用了<span style="color:${config.color}">${getItemDisplayName(config)}</span>（${effectParts.join("，")}）</span>`;
      textArea.appendChild(p);
      scrollTextAreaToBottom(textArea);
    }

    if (typeof saveGame === "function") saveGame();
  }

  // 刷新超载弹窗
  showOverweightModal();
  // 同步更新后备箱面板
  updateInventoryDisplay();
}

// 关闭超载弹窗（仅在载重合格时可调用）
function closeOverweightModal() {
  if (getInventoryWeight() > inventoryState.maxWeight) return; // 安全检查
  const modal = document.getElementById("overweight-modal");
  if (modal) modal.remove();
  resumeGameAfterModal();
}

// 满载而归结局
function overweightEnding() {
  const modal = document.getElementById("overweight-modal");
  if (modal) modal.remove();
  if (typeof showGameOver === "function") {
    showGameOver("overloaded");
  }
}

// 超载界面中打开升级后备箱
function overweightOpenUpgrade() {
  showTrunkUpgradeModal();
}

// 超载界面中打开制作台
function overweightOpenCrafting() {
  _craftingStandalone = true;
  _craftingFromOverweight = true;
  showCraftingModal();
}
