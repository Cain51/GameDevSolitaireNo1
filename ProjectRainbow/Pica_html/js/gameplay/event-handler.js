// 事件触发和处理模块
// 管理游戏中事件的检查、触发和选择处理逻辑

// 获取所有事件的合并字典
function getAllEvents() {
  const all = {};
  if (typeof GAME_EVENTS !== "undefined") {
    Object.assign(all, GAME_EVENTS);
  }
  if (typeof INVENTORY_EVENTS !== "undefined") {
    Object.assign(all, INVENTORY_EVENTS);
  }
  if (typeof EVENTS_RARE !== "undefined") {
    Object.assign(all, EVENTS_RARE);
  }
  return all;
}

// 默认可触发事件ID列表（不需要解锁）
const DEFAULT_AVAILABLE_EVENTS = [
  // ── 遭遇类 ──
  "deer",
  "rain",
  "saofurry",
  "roadblock",
  "fog",
  "lost_child",
  "vagrant",
  "exotic_traveler",
  "elderly_woman",
  // ── 停留/探索类 ──
  "rest",
  "gas_station",
  "abandoned_warehouse",
  "abandoned_mine",
  "abandoned_farm",
  "abandoned_gas_station",
  // ── 商人类 ──
  "merchant",
  "rare_merchant",
  "scavenger",
  "tech_merchant",
  // ── 特殊/随机类 ──
  "mystery_box",
  "lost_traveler",
  "stray_cat",
  "radio_tower",
  "sandstorm",
  "abandoned_village",
  "firefly_night",
  "muddy_downpour",
  "wandering_performer",
  "meteor_crater",
  "deer_nostalgia",
  "hunter_and_deer",
  "saofurry_chaos",
  // 猫猫的贴心小备注：需解锁事件（用 unlockEvents 效果激活）
  // "hunter"           邀请鹿上车后解锁
  // "ancient_monument" 探索废弃村庄后解锁
  // "supply_merchant"  搜刮废弃农场后解锁
];

// 每行文本推进时增加的里程（随机），两行合计约 3–8 km
const KM_PER_TICK_MIN = 1;
const KM_PER_TICK_MAX = 4;
// 保底昼夜休息间隔
const OVERNIGHT_REST_INTERVAL = 25;
// 最近事件去重窗口大小
const RECENT_EVENT_WINDOW = 4;
// 休息类事件ID列表
const REST_EVENT_IDS = ["rest", "craft", "gas_station"];

// 雨相关事件ID列表（雨水护符免疫范围 — 兼容旧逻辑，优先使用 tags）
const RAIN_EVENT_IDS = ["rain", "muddy_downpour"];

// 视线受阻事件ID列表（雾中人的照片免疫范围 — 兼容旧逻辑，优先使用 tags）
const VISIBILITY_EVENT_IDS = ["fog", "sandstorm"];

// 当前正在处理的事件ID（供 applyBasicEffect 判断护符免疫）
let _currentProcessingEventId = null;
// 当前正在处理的事件对象（供 applyBasicEffect 使用 tags）
let _currentProcessingEvent = null;

// ── 事件词条检测辅助函数 ──
// 优先检查事件 tags 数组，兼容旧的关键词/ID匹配
function eventHasTag(event, tagName) {
  if (!event) return false;
  if (Array.isArray(event.tags) && event.tags.includes(tagName)) return true;
  return false;
}

function isNightEvent(event) {
  if (eventHasTag(event, "夜晚")) return true;
  // 兼容：无 tags 时用关键词匹配
  const kws = ["夜", "月光", "星空", "黑夜", "夜幕", "月夜", "night"];
  return kws.some(kw =>
    (event.id && event.id.toLowerCase().includes(kw)) ||
    (event.title && event.title.includes(kw)) ||
    (event.description && event.description.includes(kw)) ||
    (event.triggerConfig && event.triggerConfig.char && event.triggerConfig.char.includes(kw))
  );
}

function isRainEvent(event) {
  if (eventHasTag(event, "雨天")) return true;
  return event.id && RAIN_EVENT_IDS.includes(event.id);
}

function isVisibilityEvent(event) {
  if (eventHasTag(event, "视线模糊")) return true;
  return event.id && VISIBILITY_EVENT_IDS.includes(event.id);
}

// 文本框滚动到底部，带 0.5s 缓动（供文本更新后统一调用）
function scrollTextAreaToBottom(textArea, durationMs) {
  if (!textArea) textArea = document.getElementById("textArea");
  if (!textArea) return;
  const target = textArea.scrollHeight - textArea.clientHeight;
  const start = textArea.scrollTop;
  if (target <= 0 || Math.abs(target - start) < 2) return;
  const dur = durationMs ?? 500;
  const startTime = performance.now();
  function step(now) {
    const t = Math.min((now - startTime) / dur, 1);
    const eased = 1 - Math.pow(1 - t, 2);
    textArea.scrollTop = start + (target - start) * eased;
    if (t < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}

// 检查是否有乘客到达目的地并让其下车
function checkPassengerGetOffAtDestination() {
  const getOff = gameState.passengerGetOffMileage;
  if (!getOff || typeof getOff !== "object") return;
  const textArea = document.getElementById("textArea");
  const toRemove = [];
  for (const name in getOff) {
    if (gameState.mileage >= getOff[name] && truckState.passengers.includes(name)) {
      toRemove.push(name);
    }
  }
  toRemove.forEach((name) => {
    // 旅行者特殊处理：第三次到达目的地时永久上车
    if (name === "旅行者") {
      if (typeof gameState.travelerDropOffCount !== "number") gameState.travelerDropOffCount = 0;
      gameState.travelerDropOffCount++;
      
      if (gameState.travelerDropOffCount >= 3) {
        // 第三次：永久上车
        if (!Array.isArray(gameState.permanentPassengers)) gameState.permanentPassengers = [];
        if (!gameState.permanentPassengers.includes("旅行者")) {
          gameState.permanentPassengers.push("旅行者");
        }
        delete getOff[name]; // 删除下车里程，不再下车
        if (textArea) {
          textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】旅行者到达了镇口，但这次他没有下车。他笑着说："我已经习惯和你一起旅行了，让我继续跟着你吧！"</p>`;
          scrollTextAreaToBottom(textArea);
        }
        if (typeof updateTruckDisplay === "function") updateTruckDisplay();
        if (typeof updatePassengerListDisplay === "function") updatePassengerListDisplay();
        return; // 不执行下车逻辑
      } else {
        // 前两次：正常下车
        const idx = truckState.passengers.indexOf(name);
        if (idx > -1) truckState.passengers.splice(idx, 1);
        delete getOff[name];
        if (textArea) {
          textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】旅行者到达了镇口，道谢后下车，并留下一个小装饰物作为纪念。</p>`;
          scrollTextAreaToBottom(textArea);
        }
        if (typeof updateTruckDisplay === "function") updateTruckDisplay();
        if (typeof updatePassengerListDisplay === "function") updatePassengerListDisplay();
        return;
      }
    }
    
    // 其他乘客正常下车
    const idx = truckState.passengers.indexOf(name);
    if (idx > -1) truckState.passengers.splice(idx, 1);
    delete getOff[name];
    if (name === "年迈妇人" && typeof addGold === "function") {
      addGold(30);
    }
    if (textArea) {
      let msg =
        name === "年迈妇人"
          ? "年迈妇人到达了镇子，感激地与你道别后下车，并留下了感谢费。"
          : `${name}到达了目的地，下车了。`;
      textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】${msg}</p>`;
      if (name === "年迈妇人")
        textArea.innerHTML += `<p style="color:#facc15;">获得 30 金币</p>`;
      scrollTextAreaToBottom(textArea);
    }
    if (typeof updateTruckDisplay === "function") updateTruckDisplay();
    if (typeof updatePassengerListDisplay === "function") updatePassengerListDisplay();
  });
}

// 每行文本推进时增加里程、扣燃油、更新显示并检查乘客下车（行驶中实时更新）
function advanceMileageForTick() {
  const kmThisTick =
    KM_PER_TICK_MIN +
    Math.floor(Math.random() * (KM_PER_TICK_MAX - KM_PER_TICK_MIN + 1));
  gameState.mileage += kmThisTick;
  const fuelCost = Math.round(
    GAME_CONFIG.fuelConsumptionPer5km * (kmThisTick / 5),
  );
  truckState.fuel = clamp(truckState.fuel - fuelCost);
  updateTruckStatusDisplay();
  if (typeof updatePassengerListDisplay === "function")
    updatePassengerListDisplay();
  checkPassengerGetOffAtDestination();
  
  // 更新成就相关状态
  if (typeof updatePerfectRunStatus === "function") {
    updatePerfectRunStatus();
  }
  if (typeof updateSurvivedLowStats === "function") {
    updateSurvivedLowStats();
  }
  // 检查成就（里程类等）
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }
}

// 检查事件触发（仅负责每 2 行是否触发节点/昼夜）
function checkEventTrigger() {
  if (
    gameState.textCount % GAME_CONFIG.triggerInterval === 0 &&
    gameState.textCount > 0
  ) {
    const totalDone = gameState.totalEventsHandled || 0;

    // ── 固定序号事件（优先级最高，在昼夜休息之前检查）──

    // 第10个事件强制触发古老神庙（仅一次，尚未触发过；用 >= 防止因昼夜扎营占位而错过精确值）
    if (totalDone >= 9 && !gameState.triggeredEvents.includes("ancient_temple")) {
      const templeEvent = getAllEvents()["ancient_temple"];
      if (templeEvent) {
        triggerEvent(templeEvent);
        return;
      }
    }

    // 第24个事件：若持有密钥且尚未触发，强制触发虚数现象
    if (totalDone >= 23 &&
        !gameState.triggeredEvents.includes("imaginary_phenomenon") &&
        typeof hasItem === "function" && hasItem("密钥", 1)) {
      const imagEvt = getAllEvents()["imaginary_phenomenon"];
      if (imagEvt) {
        triggerEvent(imagEvt);
        return;
      }
    }

    // 第48个事件：强制触发「旅途的终点...?」（仅一次）
    if (totalDone >= 47 && !gameState.triggeredEvents.includes("journey_end_question")) {
      const journeyEndEvt = getAllEvents()["journey_end_question"];
      if (journeyEndEvt) {
        if (typeof startJourneyEndEventEffect === "function") startJourneyEndEventEffect();
        triggerEvent(journeyEndEvt);
        return;
      }
    }

    // ── 昼夜强制休息（固定事件之后检查）──
    // 每N km强制触发一次昼夜休息
    const kmSinceLast = gameState.mileage - gameState.lastOvernightMileage;
    if (kmSinceLast >= OVERNIGHT_REST_INTERVAL) {
      gameState.lastOvernightMileage = gameState.mileage;
      gameState.restCountSinceOvernight = 0;
      const overnightEvent = getAllEvents()["overnight_rest"];
      if (overnightEvent) {
        triggerEvent(overnightEvent);
        return;
      }
    }

    const availableEvent = findAvailableEvent();
    if (availableEvent) {
      triggerEvent(availableEvent);
    }
  }
}

// 查找可用事件
function findAvailableEvent() {
  const allEvents = getAllEvents();
  const candidates = [];

  for (const eventId in allEvents) {
    const event = allEvents[eventId];

    // 检查是否已触发（一次性事件）
    if (event.oneTime && gameState.triggeredEvents.includes(eventId)) {
      continue;
    }
    // 保底事件（triggerWeight===0）不参与常规抽取
    if ((event.triggerWeight || 0) === 0) {
      continue;
    }
    // 最近N次内不重复
    if (
      Array.isArray(gameState.recentEvents) &&
      gameState.recentEvents.includes(eventId)
    ) {
      continue;
    }
    // 当前昼夜间隔内休息类事件已达上限
    if (
      REST_EVENT_IDS.includes(eventId) &&
      gameState.restCountSinceOvernight >= GAME_CONFIG.maxRestPerCycle
    ) {
      continue;
    }

    // 检查条件
    if (event.condition) {
      if (event.condition.requiresPassenger) {
        if (
          !truckState.passengers.includes(event.condition.requiresPassenger)
        ) {
          continue;
        }
      }
      if (event.condition.requiresItem) {
        const itemReq = event.condition.requiresItem;
        const reqId = typeof itemReq === "string" ? itemReq : itemReq.id;
        const reqQty = typeof itemReq === "string" ? 1 : itemReq.quantity || 1;

        if (!hasItem(reqId, reqQty)) {
          continue;
        }
      }
      if (event.condition.minGold) {
        if (inventoryState.gold < event.condition.minGold) {
          continue;
        }
      }
      // 若指定乘客已在车上则不触发（如流浪艺人已上车不再触发流浪艺人事件）
      if (event.condition.notPassenger) {
        const notPass = event.condition.notPassenger;
        const list = Array.isArray(notPass) ? notPass : [notPass];
        if (list.some((p) => truckState.passengers.includes(p))) continue;
      }
    }

    // 检查是否已解锁或默认可用
    if (
      !gameState.unlockedEvents.includes(eventId) &&
      !DEFAULT_AVAILABLE_EVENTS.includes(eventId)
    ) {
      continue;
    }

    candidates.push(event);
  }

  if (candidates.length === 0) return null;

  // 按权重随机选取
  const totalWeight = candidates.reduce(
    (sum, evt) => sum + (evt.triggerWeight || 10),
    0,
  );
  let roll = Math.random() * totalWeight;

  for (const event of candidates) {
    roll -= event.triggerWeight || 10;
    if (roll <= 0) {
      return event;
    }
  }

  return candidates[candidates.length - 1];
}

// 触发事件
function triggerEvent(event) {
  // 1% 概率将普通事件替换为罕见事件（休息类/保底类事件不替换）
  if (typeof EVENTS_RARE !== "undefined" && !event.rare && !event._forceNoRareReplace) {
    const noReplace = ["overnight_rest", "rest"];
    if (!noReplace.includes(event.id) && Math.random() < 0.01) {
      const rareKeys = Object.keys(EVENTS_RARE);
      if (rareKeys.length > 0) {
        const rareId = rareKeys[Math.floor(Math.random() * rareKeys.length)];
        event = EVENTS_RARE[rareId];
      }
    }
  }

  // ── 程序故障：曾同时持有 ≥2 个管理员权限时，20% 概率劫持普通事件 ──
  if (
    !event._forceNoRareReplace &&           // 不拦截强制事件
    !event._isProgramError &&               // 防止自身递归
    !event.rare &&
    typeof gameState !== "undefined" &&
    (gameState.adminPermitPeakCount || 0) >= 2 &&
    !["overnight_rest", "rest", "ancient_temple", "imaginary_phenomenon"].includes(event.id) &&
    Math.random() < 0.20
  ) {
    triggerProgramErrorEvent();
    return;
  }

  gameState.eventTriggered = true;
  pauseTextGeneration();

  // 记录事件历程
  if (typeof recordJourneyEvent === "function") {
    recordJourneyEvent("event", {
      eventId: event.id,
      title: event.title,
      image: event.image,
      rare: event.rare || false,
      tags: event.tags || [],
    });
  }

  // 应用事件背景主题特效
  if (typeof applyEventTheme === "function") {
    applyEventTheme(event);
  }

  // 萤火虫之愿：进入夜晚相关事件时，若持有该珍品则恢复耐久和金币
  const _isNightEvent = isNightEvent(event);
  const _isMysteryEvent = eventHasTag(event, "神秘");
  if (_isNightEvent && typeof inventoryState !== "undefined" && inventoryState.items) {
    const hasFireflyWish = inventoryState.items.some(s => s.id === "萤火虫之愿");
    if (hasFireflyWish && Math.random() < 0.25) {
      const durRestore = 2 + Math.floor(Math.random() * 4); // 2~5
      const goldRestore = 1 + Math.floor(Math.random() * 2); // 1~2
      truckState.durability = clamp(truckState.durability + durRestore);
      inventoryState.gold = (inventoryState.gold || 0) + goldRestore;
      if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
      // 延迟显示提示（等事件弹窗出现后显示在文本区）
      gameState._fireflyWishMessage = `✦ 萤火虫之愿 💎 在夜色中闪耀微光……（耐久+${durRestore}%，金币+${goldRestore}🪙）`;
    }

    // 天马星座的流星：进入夜晚相关事件时，若持有则获得1金币
    const hasMeteor = inventoryState.items.some(s => s.id === "天马星座的流星");
    if (hasMeteor) {
      inventoryState.gold = (inventoryState.gold || 0) + 1;
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
      gameState._meteorMessage = `✦ 天马星座的流星 💎 在夜色中泛起淡紫色微光……（金币+1🪙）`;
    }
  }

  // 天马星座的流星：神秘事件也触发（夜晚已在上方处理，避免重复）
  if (!_isNightEvent && _isMysteryEvent && typeof inventoryState !== "undefined" && inventoryState.items) {
    const hasMeteor = inventoryState.items.some(s => s.id === "天马星座的流星");
    if (hasMeteor) {
      inventoryState.gold = (inventoryState.gold || 0) + 1;
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
      gameState._meteorMessage = `✦ 天马星座的流星 💎 在神秘的气息中泛起淡紫色微光……（金币+1🪙）`;
    }
  }

  // 记录已触发（一次性事件去重）
  if (!Array.isArray(gameState.triggeredEvents)) gameState.triggeredEvents = [];
  if (!gameState.triggeredEvents.includes(event.id)) {
    gameState.triggeredEvents.push(event.id);
    // 检查成就（事件类）
    if (typeof checkAndUnlockAchievements === "function") {
      checkAndUnlockAchievements();
    }
  }

  // 维护最近事件队列
  if (event.id !== "overnight_rest") {
    if (!Array.isArray(gameState.recentEvents)) gameState.recentEvents = [];
    gameState.recentEvents.push(event.id);
    if (gameState.recentEvents.length > RECENT_EVENT_WINDOW) {
      gameState.recentEvents.shift();
    }
    // 统计休息类事件
    if (REST_EVENT_IDS.includes(event.id)) {
      gameState.restCountSinceOvernight++;
    }
  }

  // 道路减速停止，显示场景描述和触发字，弹出事件弹窗
  gradualStopRoad(GAME_CONFIG.animation.roadDeceleration)
    .then(() => {
      // 先显示场景描述文字
      if (typeof showSceneText === "function") {
        showSceneText(event).then(() => {
          // 场景描述动画结束后显示触发字
          showTriggerChar(event);
          // 停留后弹出事件选择
          setTimeout(() => {
            hideTriggerChar();
            displayEventModal(event);
          }, GAME_CONFIG.animation.charStay);
        });
      } else {
        // 如果没有场景描述功能，直接显示触发字
        showTriggerChar(event);
        setTimeout(() => {
          hideTriggerChar();
          displayEventModal(event);
        }, GAME_CONFIG.animation.charStay);
      }
    })
    .catch(() => {
      pauseRoad();
      displayEventModal(event);
    });
}

// 检查选项是否满足物品需求
function isChoiceAvailable(choice) {
  const fx = choice.result && choice.result.effects;
  if (!fx) return true;

  // 检查物品移除需求
  if (
    fx.removeItems &&
    !fx.removeItems.every((item) => hasItem(item.id, item.quantity))
  ) {
    return false;
  }

  // 检查金币花费需求
  if (fx.gold && fx.gold < 0 && inventoryState.gold < Math.abs(fx.gold)) {
    return false;
  }

  return true;
}

// ── 获取当前事件可触发的珍品 HTML ──
// 检查玩家后备箱中的珍品，如果该珍品的 triggerTags 与事件 tags 有交集，则视为可触发
function getTriggerableTreasuresHtml(event) {
  if (typeof ITEMS_CONFIG === "undefined" || typeof inventoryState === "undefined" || !inventoryState.items) return "";
  if (!Array.isArray(event.tags) || event.tags.length === 0) return "";

  const eventTagSet = new Set(event.tags);
  const matched = [];

  for (const slot of inventoryState.items) {
    const cfg = ITEMS_CONFIG[slot.id];
    if (!cfg || cfg.category !== "treasure") continue;
    if (!Array.isArray(cfg.triggerTags)) continue;
    // 珍品的 triggerTags 与事件 tags 是否有交集
    const triggers = cfg.triggerTags.some(t => eventTagSet.has(t));
    if (triggers && !matched.some(m => m.id === slot.id)) {
      matched.push({ id: slot.id, name: cfg.name, color: cfg.color });
    }
  }

  if (matched.length === 0) return "";

  const treasureSpans = matched.map(m =>
    `<span style="color:${m.color};font-weight:bold;">💎 ${m.name}</span>`
  ).join("，");

  return `<div style="position:relative;z-index:1;text-align:center;margin-top:6px;padding:4px 8px;border-top:1px dashed #4b556360;font-size:clamp(0.6rem,1.7cqw,0.72rem);color:#9ca3af;">✦ 可触发的珍品：${treasureSpans}</div>`;
}

// 显示事件弹窗（仅覆盖游戏画面 #game-canvas，其他区域保持可交互）
function displayEventModal(event) {
  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  // 主题色：优先使用事件 theme.borderColor，否则默认红色
  const themeColor = (event.theme && event.theme.borderColor) || "#c41e3a";
  // 将 hex 转为 rgb 用于 shadow/rgba
  const hexToRgb = (hex) => {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `${r},${g},${b}`;
  };
  const rgb = hexToRgb(themeColor);

  const modal = document.createElement("div");
  modal.id = "event-modal";
  modal.className =
    "event-modal text-area-scroll absolute inset-0 bg-black/80 flex items-center justify-center z-50";

  let choicesHtml = "";
  event.choices.forEach((choice) => {
    const available = isChoiceAvailable(choice);

    // 效果提示：已选过显示 hintKnown，未选过显示 hintUnknown
    let hintHtml = "";
    if (choice.hintKnown || choice.hintUnknown) {
      const remembered = typeof isChoiceRemembered === "function" && isChoiceRemembered(event.id, choice.id);
      const hintText = remembered ? (choice.hintKnown || choice.hintUnknown) : (choice.hintUnknown || "???");
      const hintColor = remembered ? "#9ca3af" : "#6b7280";
      hintHtml = `<div style="margin-top:4px;font-size:clamp(0.6rem,1.8cqw,0.75rem);color:${hintColor};font-style:italic;">${remembered ? "📋 " : "❓ "}${hintText}</div>`;
    }

    if (available) {
      choicesHtml += `
			<button onclick="handleEventChoice('${event.id}', '${choice.id}')" 
				class="event-modal-choice w-full text-white rounded-lg text-left transition-all duration-300"
				style="background:linear-gradient(to right,#374151,#1f2937);border:1px solid #4b5563;"
				onmouseenter="this.style.background='linear-gradient(to right,${themeColor},${themeColor}cc)';this.style.borderColor='${themeColor}';"
				onmouseleave="this.style.background='linear-gradient(to right,#374151,#1f2937)';this.style.borderColor='#4b5563';">
				<div class="choice-title font-bold mb-0.5">${choice.text}</div>
				<div class="choice-desc text-gray-400">${choice.description}</div>
				${hintHtml}
			</button>`;
    } else {
      // 选项不可用时灰显并标注缺少什么
      const fx = choice.result && choice.result.effects;
      const missing = [];

      // 检查缺少的物品
      if (fx && fx.removeItems) {
        fx.removeItems.forEach((item) => {
          if (!hasItem(item.id, item.quantity)) {
            const cfg = ITEMS_CONFIG[item.id];
            missing.push((cfg ? cfg.name : item.id) + "×" + item.quantity);
          }
        });
      }

      // 检查缺少的金币
      if (
        fx &&
        fx.gold &&
        fx.gold < 0 &&
        inventoryState.gold < Math.abs(fx.gold)
      ) {
        missing.push("金币×" + Math.abs(fx.gold));
      }

      const missingStr = missing.join("、");
      choicesHtml += `
			<button disabled
				class="event-modal-choice w-full bg-gray-900 text-gray-600 rounded-lg text-left border border-gray-800 cursor-not-allowed opacity-60">
				<div class="choice-title font-bold mb-0.5">${choice.text}</div>
				<div class="choice-desc text-gray-600">${choice.description}</div>
				<div class="text-red-400 mt-0.5" style="font-size: clamp(0.65rem, 1.8cqw, 0.75rem);">缺少：${missingStr}</div>
			</button>`;
    }
  });

  // 雾中人的照片：视线受阻事件中，替换描述文本并添加鬼魂粒子
  const _hasFogPhoto = isVisibilityEvent(event) &&
    typeof inventoryState !== "undefined" && inventoryState.items &&
    inventoryState.items.some(s => s.id === "雾中人的照片");

  const displayDescription = _hasFogPhoto
    ? event.description + '<br><span style="color:#94a3b8;font-style:italic;">……一个模糊的身影出现在视线尽头。你感到莫名的安心，仿佛有人在守护着你。</span>'
    : event.description;

  // 鬼魂漂浮粒子层（仅在持有雾中人的照片时注入）
  const ghostParticlesHtml = _hasFogPhoto ? `
    <div class="fog-photo-ghosts" style="position:absolute;inset:0;pointer-events:none;overflow:hidden;border-radius:inherit;z-index:0;">
      ${Array.from({length: 6}, (_, i) => {
        const left = 10 + Math.random() * 80;
        const delay = Math.random() * 4;
        const dur = 3 + Math.random() * 3;
        const size = 1.2 + Math.random() * 0.8;
        return `<span style="position:absolute;left:${left}%;bottom:-10%;font-size:${size}rem;opacity:0;animation:fogGhostFloat ${dur}s ${delay}s ease-in-out infinite;">👻</span>`;
      }).join("")}
    </div>` : "";

  // 罕见事件动态边框类名
  const rareBorderClass = event.rareClass || (event.rare ? "rare-event-border" : "");

  // ── 生成事件特性词条 HTML ──
  let tagsHtml = "";
  if (Array.isArray(event.tags) && event.tags.length > 0 && typeof EVENT_TAGS !== "undefined") {
    const tagBadges = event.tags.map(tagName => {
      const tagCfg = EVENT_TAGS[tagName];
      if (!tagCfg) return `<span style="font-size:clamp(0.55rem,1.5cqw,0.65rem);color:#94a3b8;background:#94a3b818;border:1px solid #94a3b840;border-radius:9999px;padding:1px 6px;">${tagName}</span>`;
      return `<span style="font-size:clamp(0.55rem,1.5cqw,0.65rem);color:${tagCfg.color};background:${tagCfg.bgColor};border:1px solid ${tagCfg.borderColor};border-radius:9999px;padding:1px 6px;cursor:default;" title="${tagCfg.description || ''}">${tagCfg.emoji} ${tagCfg.label}</span>`;
    }).join(" ");
    tagsHtml = `<div style="text-align:center;margin-bottom:4px;display:flex;justify-content:center;gap:4px;flex-wrap:wrap;">${tagBadges}</div>`;
  }

  modal.innerHTML = `
		<div class="event-modal-inner text-area-scroll ${rareBorderClass}" style="position:relative;background:#1a1a2e;border:2px solid ${themeColor};box-shadow:0 0 50px rgba(${rgb},0.5);border-radius:inherit;">
			${ghostParticlesHtml}
			<div class="text-center" style="position:relative;z-index:1;">
				<div class="event-modal-emoji">${event.image}</div>
				<h2 class="event-modal-title font-bold" style="color:${themeColor};">${event.title}</h2>
				${event.rare ? '<div style="text-align:center;margin-bottom:4px;"><span style="font-size:clamp(0.6rem,1.6cqw,0.7rem);color:#ff00ff;background:#ff00ff18;border:1px solid #ff00ff40;border-radius:9999px;padding:1px 8px;letter-spacing:1px;">✦ 罕见事件 ✦</span></div>' : ''}
				${tagsHtml}
				<p class="event-modal-desc text-gray-300 leading-relaxed">${displayDescription}</p>
			</div>
			<div class="space-y-1" style="position:relative;z-index:1;">
				${choicesHtml}
			</div>
			${getTriggerableTreasuresHtml(event)}
		</div>
	`;

  gameCanvas.appendChild(modal);
}

// 选择后显示乘客随机台词
function showPassengerDialogues(textArea) {
  if (!textArea || typeof PASSENGER_DIALOGUE_CONFIG === "undefined") return;
  const passengers = truckState.passengers || [];
  if (passengers.length === 0) return;

  const shuffled = [...passengers].sort(() => Math.random() - 0.5);
  let shown = 0;
  const maxShow = Math.min(3, Math.floor(passengers.length * 0.6) + 1);

  for (const name of shuffled) {
    if (shown >= maxShow) break;
    if (Math.random() > 0.5) continue;

    const cfg = PASSENGER_DIALOGUE_CONFIG[name];
    const lines = (cfg && cfg.afterChoice) || PASSENGER_DIALOGUE_CONFIG._default;
    const line = Array.isArray(lines) ? lines[Math.floor(Math.random() * lines.length)] : lines;
    if (line) {
      const color = (typeof PASSENGER_CONFIG !== "undefined" && PASSENGER_CONFIG[name] && PASSENGER_CONFIG[name].color) || "#94a3b8";
      textArea.innerHTML += `<p class="text-gray-400" style="color:${color}">「${name}」${line}</p>`;
      scrollTextAreaToBottom(textArea);
      shown++;
    }
  }
}

// 检查并触发条件剧情
function checkConditionalStories(textArea) {
  if (!textArea || typeof CONDITIONAL_STORIES_CONFIG === "undefined") return;
  const triggered = gameState.triggeredConditionalStories || [];

  for (const story of CONDITIONAL_STORIES_CONFIG) {
    if (triggered.includes(story.id)) continue;
    try {
      if (!story.condition || !story.condition()) continue;

      gameState.triggeredConditionalStories.push(story.id);
      textArea.innerHTML += `<p class="text-[#c41e3a]">【剧情】${story.message}</p>`;
      scrollTextAreaToBottom(textArea);

      if (story.rewards && story.rewards.addItems && typeof addItem === "function") {
        story.rewards.addItems.forEach((item) => {
          if (addItem(item.id, item.quantity)) {
            const cfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG[item.id];
            textArea.innerHTML += cfg
              ? `<p style="color:#4ade80">获得 <span style="color:${cfg.color}">${getItemDisplayName(cfg)}</span> ×${item.quantity}</p>`
              : `<p style="color:#4ade80">获得 ${item.id} ×${item.quantity}</p>`;
            scrollTextAreaToBottom(textArea);
            // 珍品获得：全屏特效 + 小红点
            if (cfg && cfg.category === "treasure") {
              if (typeof showTreasureAcquireEffect === "function") showTreasureAcquireEffect(cfg.name, cfg.color);
              if (typeof showTreasureRedDot === "function") showTreasureRedDot();
            }
          }
        });
        if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
      }
    } catch (e) {
      console.warn("条件剧情检查失败:", story.id, e);
    }
  }
}

// 防止事件选项被快速重复点击
let _eventChoiceLocked = false;

// 处理事件选择
function handleEventChoice(eventId, choiceId) {
  // 防重复点击：如果已经在处理中，直接忽略
  if (_eventChoiceLocked) return;
  _eventChoiceLocked = true;

  // 事件选项点击音效
  if (typeof playEventChoice === "function") playEventChoice();

  // 立即禁用所有事件选项按钮，防止在动画期间再次点击
  const eventModal = document.getElementById("event-modal");
  if (eventModal) {
    eventModal.querySelectorAll("button").forEach(btn => {
      btn.disabled = true;
      btn.style.pointerEvents = "none";
    });
  }

  // 统计事件选择次数
  gameState.totalEventsHandled = (gameState.totalEventsHandled || 0) + 1;

  // 推进背景景观（基于事件次数切换）
  if (typeof sceneryTick === "function") {
    sceneryTick();
  }

  // 记录选项到永久记忆（跨档保留）
  if (typeof recordChoiceMemory === "function") {
    recordChoiceMemory(eventId, choiceId);
  }

  const allEvents = getAllEvents();
  const event = allEvents[eventId];
  const choice = event.choices.find((c) => c.id === choiceId);

  // 记录玩家抉择到历程日志
  if (typeof recordJourneyEvent === "function") {
    recordJourneyEvent("choice", {
      eventId,
      eventTitle: event ? event.title : eventId,
      choiceId,
      choiceText: choice ? choice.text : choiceId,
    });
  }
  const textArea = document.getElementById("textArea");

  if (choice.result) {
    // 随机消息
    const rawMsg = choice.result.message;
    const msg = Array.isArray(rawMsg)
      ? rawMsg[Math.floor(Math.random() * rawMsg.length)]
      : rawMsg;

    // 显示结果消息
    textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】${msg}</p>`;
    scrollTextAreaToBottom(textArea);

    // 标记当前事件（供 applyBasicEffect 判断雨水护符/雾中人照片免疫）
    _currentProcessingEventId = eventId;
    _currentProcessingEvent = event;

    // 处理效果
    if (choice.result.effects) {
      processEffects(choice.result.effects, textArea);
    }

    // 清除当前事件标记
    _currentProcessingEventId = null;
    _currentProcessingEvent = null;

    // 萤火虫之愿触发提示
    if (gameState._fireflyWishMessage) {
      const cfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["萤火虫之愿"];
      const color = cfg ? cfg.color : "#fbbf24";
      textArea.innerHTML += `<p style="color:${color}">${gameState._fireflyWishMessage}</p>`;
      scrollTextAreaToBottom(textArea);
      delete gameState._fireflyWishMessage;
    }

    // 天马星座的流星触发提示
    if (gameState._meteorMessage) {
      const cfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["天马星座的流星"];
      const color = cfg ? cfg.color : "#c084fc";
      textArea.innerHTML += `<p style="color:${color}">${gameState._meteorMessage}</p>`;
      scrollTextAreaToBottom(textArea);
      delete gameState._meteorMessage;
    }

    // 选择后：乘客随机台词 + 条件剧情检查（若打开了二级选择则等子选择完成后再显示）
    if (!document.getElementById("sub-choice-modal")) {
      showPassengerDialogues(textArea);
      checkConditionalStories(textArea);
    }

    // 触发被动物品效果（如鹿角护符）
    triggerPassiveItemEffects(textArea);

    // 全局随机掉落：每次抉择后 0.1% 概率获得「随缘而遇的尘埃」（玩家未持有时才掉落）
    checkGlobalRandomDrop(textArea);

    // 应用困难模式 perChoice 修饰符
    applyHardModePerChoice(textArea);

    // 衰变 debuff 系统：先检查是否获得新 debuff，再应用所有已有 debuff
    checkAndApplyNewDebuff(textArea);
    applyActiveDebuffs(textArea);

    // 检查游戏结束条件
    // 优先检查旅途终点结局（由 journey_end 效果设置的 pending 标记）
    if (gameState._pendingJourneyEndEnding) {
      delete gameState._pendingJourneyEndEnding;
      _eventChoiceLocked = false;
      showGameOver("journey_end");
      return;
    }
    if (
      typeof checkGameOverConditions === "function" &&
      checkGameOverConditions()
    ) {
      _eventChoiceLocked = false;
      return;
    }

    // 保存
    saveGame();
  }

  // 关闭弹窗（淡出）
  const modal = document.getElementById("event-modal");
  if (modal) {
    modal.classList.add("modal-fade-out");
    modal.addEventListener("animationend", () => modal.remove(), { once: true });
  }

  // 检查是否有子模态框（制作台/商人/休息/二级选择/扫雷/时间银行）仍然打开
  // 如有，游戏继续暂停，等待子模态框关闭后由其自身调用 resumeGame()
  const hasOpenSubModal =
    document.getElementById("rest-modal") ||
    document.getElementById("crafting-modal") ||
    document.getElementById("merchant-modal") ||
    document.getElementById("sub-choice-modal") ||
    document.getElementById("minesweeper-modal") ||
    document.getElementById("time-bank-modal") ||
    document.getElementById("fate-roulette-modal");

  if (!hasOpenSubModal) {
    // 检查超载：若载重>=最大值，弹出超载界面暂停游戏
    if (typeof checkOverweightAndShow === "function" && checkOverweightAndShow()) {
      // 超载弹窗已打开，游戏保持暂停
      _eventChoiceLocked = false;
      return;
    }
    // 清除事件背景主题特效
    if (typeof clearEventTheme === "function") clearEventTheme();
    resumeRoad();
    resumeTextGeneration();
    gameState.eventTriggered = false;
  }

  _eventChoiceLocked = false;
}

// 递归处理事件效果
function processEffects(fx, textArea) {
  if (!fx) return;

  // 数组：按顺序执行
  if (Array.isArray(fx)) {
    fx.forEach((effect) => processEffects(effect, textArea));
    return;
  }

  // 权重随机: { type: 'weighted', options: [ {weight: 10, message: '...', effects: {...}}, ... ] }
  if (fx.type === "weighted" && Array.isArray(fx.options)) {
    const totalWeight = fx.options.reduce(
      (sum, opt) => sum + (opt.weight || 1),
      0,
    );
    let roll = Math.random() * totalWeight;
    for (const opt of fx.options) {
      roll -= opt.weight || 1;
      if (roll <= 0) {
        // 每个 option 可携带独立 message
        if (opt.message) {
          const msg = Array.isArray(opt.message)
            ? opt.message[Math.floor(Math.random() * opt.message.length)]
            : opt.message;
          textArea.innerHTML += `<p class="text-[#c41e3a]">【结果】${msg}</p>`;
          scrollTextAreaToBottom(textArea);
        }
        processEffects(opt.effects, textArea);
        return;
      }
    }
    // Fallback to last option if rounding errors
    if (fx.options.length > 0) {
      const last = fx.options[fx.options.length - 1];
      if (last.message) {
        const msg = Array.isArray(last.message)
          ? last.message[Math.floor(Math.random() * last.message.length)]
          : last.message;
        textArea.innerHTML += `<p class="text-[#c41e3a]">【结果】${msg}</p>`;
        scrollTextAreaToBottom(textArea);
      }
      processEffects(last.effects, textArea);
    }
    return;
  }

  // 概率执行: { type: 'chance', chance: 0.5, success: {...}, fail: {...} }
  if (fx.type === "chance") {
    let ch = fx.chance || 0.5;
    // 概率平衡加成：提升 chance 概率
    if (hasHardModeBonus("luck_boost")) {
      // 检测是否为珍品掉落（success 包含 addItems 且物品为 treasure）
      let isTreasureDrop = false;
      if (fx.success && fx.success.addItems && Array.isArray(fx.success.addItems)) {
        for (const ai of fx.success.addItems) {
          if (typeof ITEMS !== "undefined" && ITEMS[ai.id] && ITEMS[ai.id].category === "treasure") {
            isTreasureDrop = true;
            break;
          }
        }
      }
      if (isTreasureDrop) {
        ch = Math.min(1, ch * 1.3 + 0.15);
      } else {
        ch = Math.min(1, ch + 0.15);
      }
    }
    if (Math.random() < ch) {
      if (fx.success) processEffects(fx.success, textArea);
    } else {
      if (fx.fail) processEffects(fx.fail, textArea);
    }
    return;
  }

  // 显式序列/所有执行 { type: 'sequence', list: [...] }
  if ((fx.type === "sequence" || fx.type === "all") && Array.isArray(fx.list)) {
    fx.list.forEach((effect) => processEffects(effect, textArea));
    return;
  }

  // 二级选择: { type: 'choice', prompt: '...', options: [{text, description, message, effects}, ...] }
  if (fx.type === "choice" && Array.isArray(fx.options)) {
    showSubChoiceModal(fx, textArea);
    return;
  }

  // 旅途终点惩罚: { type: 'journey_end', stat: 'fuel'|'durability'|'comfort' }
  // 基础扣除 120%，乘客好感度越高惩罚越低（最低 60%）
  if (fx.type === "journey_end") {
    const stat = fx.stat; // 'fuel' | 'durability' | 'comfort'
    const passengers = truckState.passengers || [];
    const passengerFavor = gameState.passengerFavor || {};

    // 计算乘客减伤系数（0~1），每位乘客贡献 好感度/100，总和 cap 到 1.0
    let favorSum = 0;
    for (const name of passengers) {
      const fav = typeof passengerFavor[name] === "number" ? passengerFavor[name] : 50;
      favorSum += Math.max(0, Math.min(100, fav)) / 100;
    }
    const mitigationRatio = Math.min(1.0, favorSum); // 0~1
    // 惩罚从 120 线性减至 60（减伤系数为1时），即 penalty = 120 - 60 * mitigationRatio
    const penalty = Math.round(120 - 60 * mitigationRatio);

    // 保存触发前的属性值（用于结局判定）
    const fuelBefore       = truckState.fuel;
    const durabilityBefore = truckState.durability;
    const comfortBefore    = truckState.comfort;

    // 扣减对应属性
    truckState[stat] = Math.max(0, truckState[stat] - penalty);
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();

    // 文字反馈
    if (textArea) {
      const statLabel = { fuel: "燃油", durability: "耐久", comfort: "舒适" }[stat] || stat;
      const mitigationPct = Math.round(mitigationRatio * 100);
      textArea.innerHTML += `<p style="color:#a78bfa">🛣️ ${statLabel} <span style="color:#ef4444">-${penalty}%</span>${mitigationPct > 0 ? `<span style="color:#a3e635">（乘客同行减轻了 ${mitigationPct}% 的代价）</span>` : ""}</p>`;
      if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
    }

    // 判断是否触发终点结局：三属性均 > 0 时触发（在常规 checkGameOverConditions 之前插队）
    const survived = truckState.fuel > 0 && truckState.durability > 0 && truckState.comfort > 0;
    if (survived) {
      // 用 setTimeout 延迟触发，让当前事件流程走完（弹窗关闭、文字显示完毕）后再弹结局
      gameState._pendingJourneyEndEnding = true;
    }
    // 关闭事件弹窗的环境特效
    if (typeof stopJourneyEndEventEffect === "function") stopJourneyEndEventEffect();

    return;
  }

  // 基础效果处理
  applyBasicEffect(fx, textArea);
}

// 应用基础效果
function applyBasicEffect(fx, textArea) {
  // 效果自带消息文本
  if (fx.message && textArea) {
    const msg = Array.isArray(fx.message)
      ? fx.message[Math.floor(Math.random() * fx.message.length)]
      : fx.message;
    textArea.innerHTML += `<p class="text-[#c41e3a]">【结果】${msg}</p>`;
    scrollTextAreaToBottom(textArea);
  }
  // 乘客管理（上车时初始化该乘客好感度，并标记新上车以便仅此时播动效）
  if (fx.addPassenger && !truckState.passengers.includes(fx.addPassenger)) {
    truckState.passengers.push(fx.addPassenger);
    if (typeof gameState._newPassengerNames !== "object") gameState._newPassengerNames = [];
    gameState._newPassengerNames.push(fx.addPassenger);
    if (typeof gameState.passengerFavor !== "object")
      gameState.passengerFavor = {};
    if (
      gameState.passengerFavor[fx.addPassenger] === undefined ||
      gameState.passengerFavor[fx.addPassenger] === null
    ) {
      gameState.passengerFavor[fx.addPassenger] =
        typeof DEFAULT_FAVOR !== "undefined" ? DEFAULT_FAVOR : 50;
    }
    // 记录乘客上车（用于成就检查）
    if (typeof recordPassengerBoarded === "function") {
      recordPassengerBoarded(fx.addPassenger);
    }
    updateTruckDisplay();
    if (typeof updatePassengerListDisplay === "function")
      updatePassengerListDisplay();
    // 检查成就
    if (typeof checkAndUnlockAchievements === "function") {
      checkAndUnlockAchievements();
    }
  }
  if (fx.removePassenger) {
    const idx = truckState.passengers.indexOf(fx.removePassenger);
    if (idx > -1) {
      truckState.passengers.splice(idx, 1);
      if (typeof gameState.passengerGetOffMileage === "object")
        delete gameState.passengerGetOffMileage[fx.removePassenger];
      updateTruckDisplay();
      if (typeof updatePassengerListDisplay === "function")
        updatePassengerListDisplay();
    }
  }

  // 流浪猫喂食计数：第三次喂食时猫上车
  if (fx.strayCatFeedAndMaybeBoard) {
    if (typeof gameState.strayCatFeedCount !== "number") gameState.strayCatFeedCount = 0;
    gameState.strayCatFeedCount++;
    if (
      gameState.strayCatFeedCount >= 3 &&
      !truckState.passengers.includes("猫")
    ) {
      truckState.passengers.push("猫");
      if (typeof gameState._newPassengerNames !== "object") gameState._newPassengerNames = [];
      gameState._newPassengerNames.push("猫");
      if (typeof gameState.passengerFavor !== "object") gameState.passengerFavor = {};
      if (gameState.passengerFavor["猫"] === undefined) gameState.passengerFavor["猫"] = typeof DEFAULT_FAVOR !== "undefined" ? DEFAULT_FAVOR : 50;
      // 记录乘客上车（用于成就检查）
      if (typeof recordPassengerBoarded === "function") {
        recordPassengerBoarded("猫");
      }
      if (textArea) {
        textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】小猫跟着跳上了车，在车厢里窝成一团不走了。</p>`;
        scrollTextAreaToBottom(textArea);
      }
      if (typeof updateTruckDisplay === "function") updateTruckDisplay();
      if (typeof updatePassengerListDisplay === "function") updatePassengerListDisplay();
    }
  }

  // 设置乘客“到达目的地下车”的里程（到达该里程时自动下车）
  // 值可为数字（固定 km）或 [min, max] 数组（随机区间 km）
  if (fx.setPassengerGetOffMileage && typeof fx.setPassengerGetOffMileage === "object") {
    if (typeof gameState.passengerGetOffMileage !== "object")
      gameState.passengerGetOffMileage = {};
    for (const name in fx.setPassengerGetOffMileage) {
      // 如果旅行者已经永久上车，不再设置下车里程
      if (name === "旅行者" && Array.isArray(gameState.permanentPassengers) && gameState.permanentPassengers.includes("旅行者")) {
        continue;
      }
      const raw = fx.setPassengerGetOffMileage[name];
      let kmFromNow = 0;
      if (Array.isArray(raw) && raw.length >= 2) {
        const min = Math.max(1, Math.floor(Number(raw[0])) || 5);
        const max = Math.max(min, Math.floor(Number(raw[1])) || 15);
        kmFromNow = min + Math.floor(Math.random() * (max - min + 1));
      } else {
        kmFromNow = Number(raw);
      }
      if (!isNaN(kmFromNow) && kmFromNow > 0)
        gameState.passengerGetOffMileage[name] = gameState.mileage + kmFromNow;
    }
    if (typeof updatePassengerListDisplay === "function")
      updatePassengerListDisplay();
  }

  // 解锁事件
  if (fx.unlockEvents) {
    fx.unlockEvents.forEach((evtId) => {
      if (!gameState.unlockedEvents.includes(evtId))
        gameState.unlockedEvents.push(evtId);
    });
  }

  // 雨水护符：雨相关事件中免疫所有负面属性效果
  const _rainImmune = _currentProcessingEvent &&
    isRainEvent(_currentProcessingEvent) &&
    typeof inventoryState !== "undefined" && inventoryState.items &&
    inventoryState.items.some(s => s.id === "雨水护符");
  let _rainImmuneTriggered = false;

  // 雾中人的照片：视线受阻事件中免疫所有负面属性效果
  const _fogPhotoImmune = _currentProcessingEvent &&
    isVisibilityEvent(_currentProcessingEvent) &&
    typeof inventoryState !== "undefined" && inventoryState.items &&
    inventoryState.items.some(s => s.id === "雾中人的照片");
  let _fogPhotoImmuneTriggered = false;

  // 呓语之书：效果倒转（仅对 fuel/durability/comfort 生效）
  let _reversed = false;
  if (typeof gameState !== "undefined" && gameState._effectReversed) {
    if (fx.fuel || fx.durability || fx.comfort) {
      _reversed = true;
      if (fx.fuel) fx = Object.assign({}, fx, { fuel: -fx.fuel });
      else fx = Object.assign({}, fx);
      if (fx.durability) fx.durability = -fx.durability;
      if (fx.comfort) fx.comfort = -fx.comfort;
      gameState._effectReversed = false;
    }
  }

  // 皮卡属性
  if (fx.fuel) {
    if ((_rainImmune || _fogPhotoImmune) && fx.fuel < 0) {
      if (_rainImmune) _rainImmuneTriggered = true;
      if (_fogPhotoImmune) _fogPhotoImmuneTriggered = true;
    } else {
      let fuelDelta = fx.fuel;
      // 旅行者特性：认路，燃油消耗减少 2（休息/停车时省油）
      if (fuelDelta < 0 && truckState.passengers && truckState.passengers.includes("旅行者")) {
        fuelDelta = Math.min(0, fuelDelta + 2);
      }
      truckState.fuel = clamp(truckState.fuel + fuelDelta);
      updateTruckStatusDisplay();
    }
  }
  if (fx.durability) {
    if ((_rainImmune || _fogPhotoImmune) && fx.durability < 0) {
      if (_rainImmune) _rainImmuneTriggered = true;
      if (_fogPhotoImmune) _fogPhotoImmuneTriggered = true;
    } else {
      truckState.durability = clamp(truckState.durability + fx.durability);
      updateTruckStatusDisplay();
      if (typeof triggerScreenShake === "function") triggerScreenShake();
    }
  }
  if (fx.comfort) {
    if ((_rainImmune || _fogPhotoImmune) && fx.comfort < 0) {
      if (_rainImmune) _rainImmuneTriggered = true;
      if (_fogPhotoImmune) _fogPhotoImmuneTriggered = true;
    } else {
      let comfortDelta = fx.comfort;
      // 鹿特性：森林之灵，休息时舒适恢复 +2
      if (comfortDelta > 0 && truckState.passengers && truckState.passengers.includes("鹿")) {
        comfortDelta += 2;
      }
      truckState.comfort = clamp(truckState.comfort + comfortDelta);
      updateTruckStatusDisplay();
    }
  }

  // 雨水护符免疫提示（同一次 applyBasicEffect 只显示一次）
  if (_rainImmuneTriggered && textArea) {
    const rainCfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["雨水护符"];
    const rainColor = rainCfg ? rainCfg.color : "#60a5fa";
    textArea.innerHTML += `<p style="color:${rainColor}">✦ 雨水护符 💎 泛起涟漪，为你抵御了风雨的侵袭……</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 雾中人的照片免疫提示
  if (_fogPhotoImmuneTriggered && textArea) {
    const fogCfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["雾中人的照片"];
    const fogColor = fogCfg ? fogCfg.color : "#94a3b8";
    textArea.innerHTML += `<p style="color:${fogColor}">✦ 雾中人的照片 💎 隐约浮现一个守护的身影，为你拨开了迷雾……</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 呓语之书效果倒转提示
  if (_reversed && textArea) {
    const bookCfg = typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["空白书"];
    textArea.innerHTML += `<p style="color:#9f7aea">✦ 呓语之书的力量发动了！所有效果被倒转！书页化为空白……</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 每位乘客贡献固定值（正=舒适，负=不适）
  if (fx.comfortPerPassenger !== undefined) {
    const passengerCount = truckState.passengers
      ? truckState.passengers.length
      : 0;
    const delta = fx.comfortPerPassenger * passengerCount;
    truckState.comfort = clamp(truckState.comfort + delta);
    updateTruckStatusDisplay();
  }

  // 好感度：fx.favor 为 { "鹿": 10, "猎人": -5 }，仅对当前在车上的乘客生效
  if (fx.favor && typeof fx.favor === "object" && Array.isArray(truckState.passengers)) {
    const favorMin = typeof FAVOR_MIN !== "undefined" ? FAVOR_MIN : 0;
    const favorMax = typeof FAVOR_MAX !== "undefined" ? FAVOR_MAX : 100;
    const defaultFavor = typeof DEFAULT_FAVOR !== "undefined" ? DEFAULT_FAVOR : 50;
    if (typeof gameState.passengerFavor !== "object")
      gameState.passengerFavor = {};
    for (const name in fx.favor) {
      if (!truckState.passengers.includes(name)) continue;
      const delta = Number(fx.favor[name]);
      if (isNaN(delta)) continue;
      const cur = gameState.passengerFavor[name];
      const base = typeof cur === "number" && !isNaN(cur) ? cur : defaultFavor;
      gameState.passengerFavor[name] = Math.min(
        favorMax,
        Math.max(favorMin, base + delta),
      );
    }
    if (typeof updatePassengerListDisplay === "function")
      updatePassengerListDisplay();
    // 检查成就（好感度类）
    if (typeof checkAndUnlockAchievements === "function") {
      checkAndUnlockAchievements();
    }
  }

  // 好感度：fx.favorAll 对所有当前乘客增加/减少相同数值
  if (
    typeof fx.favorAll === "number" &&
    !isNaN(fx.favorAll) &&
    Array.isArray(truckState.passengers) &&
    truckState.passengers.length > 0
  ) {
    const favorMin = typeof FAVOR_MIN !== "undefined" ? FAVOR_MIN : 0;
    const favorMax = typeof FAVOR_MAX !== "undefined" ? FAVOR_MAX : 100;
    const defaultFavor = typeof DEFAULT_FAVOR !== "undefined" ? DEFAULT_FAVOR : 50;
    if (typeof gameState.passengerFavor !== "object")
      gameState.passengerFavor = {};
    truckState.passengers.forEach((name) => {
      const cur = gameState.passengerFavor[name];
      const base = typeof cur === "number" && !isNaN(cur) ? cur : defaultFavor;
      gameState.passengerFavor[name] = Math.min(
        favorMax,
        Math.max(favorMin, base + fx.favorAll),
      );
    });
    if (typeof updatePassengerListDisplay === "function")
      updatePassengerListDisplay();
    // 检查成就（好感度类）
    if (typeof checkAndUnlockAchievements === "function") {
      checkAndUnlockAchievements();
    }
  }

  // 金币
  if (fx.gold) {
    addGold(fx.gold);
    textArea.innerHTML += `<p style="color:#facc15;">获得 ${fx.gold} 金币</p>`;
  }

  // 打开UI模态框
  if (fx.openRestModal) showRestModal();
  if (fx.openCraftingModal) showCraftingModal();
  if (fx.openMerchantModal) showMerchantModal(fx.openMerchantModal);
  if (fx.openMinesweeper && typeof showMinesweeperModal === "function") showMinesweeperModal();

  // 神庙祈祷：随机获得3种不同物资，每种1~2个
  if (fx.templeOffering) {
    const offeringPool = ["油桶", "修理包", "坐垫", "急救箱", "零食", "废金属", "布料", "草药", "空罐", "原油", "精炼剂", "铜线", "橡胶", "电池", "皮革"];
    // 打乱后取前3个，保证不重复
    const shuffled = offeringPool.slice().sort(() => Math.random() - 0.5);
    const picked = shuffled.slice(0, 3);
    if (typeof gameState !== "undefined") gameState._newItemIds = [];
    picked.forEach(itemId => {
      const qty = 1 + Math.floor(Math.random() * 2); // 1~2
      if (addItem(itemId, qty)) {
        if (typeof gameState !== "undefined" && Array.isArray(gameState._newItemIds))
          gameState._newItemIds.push(itemId);
        const cfg = ITEMS_CONFIG[itemId];
        if (cfg) {
          textArea.innerHTML += `<p style="color:#4ade80;">神明赐予了 <span style="color:${cfg.color}">${cfg.name}</span> ×${qty}</p>`;
        }
      }
    });
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
  }

  // 物品添加（标记新物品以便后备箱渐显动效）
  if (fx.addItems) {
    if (typeof gameState !== "undefined") gameState._newItemIds = fx.addItems.map((item) => item.id);
    fx.addItems.forEach((item) => {
      if (addItem(item.id, item.quantity)) {
        const cfg = ITEMS_CONFIG[item.id];
        if (cfg) {
          textArea.innerHTML += `<p style="color:#4ade80;">获得 <span style="color:${cfg.color}">${getItemDisplayName(cfg)}</span> ×${item.quantity}</p>`;
          // 珍品获得：全屏特效 + 小红点
          if (cfg.category === "treasure") {
            if (typeof showTreasureAcquireEffect === "function") {
              showTreasureAcquireEffect(cfg.name, cfg.color);
            }
            if (typeof showTreasureRedDot === "function") {
              showTreasureRedDot();
            }
          }
        }
      }
    });
    // 检查成就（物品类）
    if (typeof checkAndUnlockAchievements === "function") {
      checkAndUnlockAchievements();
    }
  }

  // 物品移除
  if (fx.removeItems) {
    fx.removeItems.forEach((item) => {
      if (removeItem(item.id, item.quantity)) {
        const cfg = ITEMS_CONFIG[item.id];
        if (cfg) {
          textArea.innerHTML += `<p style="color:#ef4444;">消耗了 <span style="color:${cfg.color}">${cfg.name}</span> ×${item.quantity}</p>`;
          scrollTextAreaToBottom(textArea);
        }
      }
    });
  }

  // 随机拾取（标记新物品以便后备箱渐显动效）
  // fx.randomLootCount 可指定抽取次数（默认1次）
  if (fx.randomLoot) {
    const tableId = typeof fx.randomLoot === "string" ? fx.randomLoot : "默认";
    const lootTimes = (typeof fx.randomLootCount === "number" && fx.randomLootCount > 1) ? fx.randomLootCount : 1;
    if (typeof gameState !== "undefined") gameState._newItemIds = [];
    if (typeof getRandomLoot === "function") {
      for (let _lt = 0; _lt < lootTimes; _lt++) {
      for (const item of getRandomLoot(tableId)) {
        if (addItem(item.itemId, item.quantity)) {
          if (typeof gameState !== "undefined" && Array.isArray(gameState._newItemIds))
            gameState._newItemIds.push(item.itemId);
          const cfg = ITEMS_CONFIG[item.itemId];
          if (cfg) {
            textArea.innerHTML += `<p style="color:#4ade80;">找到了 <span style="color:${cfg.color}">${cfg.name}</span> ×${item.quantity}</p>`;
          }
        }
      }
      // 猎人特性：搜刮时额外找到物资
      if (truckState.passengers && truckState.passengers.includes("猎人") && typeof getRandomLoot === "function") {
        const hunterLoot = getRandomLoot(tableId);
        let hasHunterGain = false;
        for (const item of hunterLoot) {
          if (addItem(item.itemId, item.quantity)) {
            hasHunterGain = true;
            if (typeof gameState !== "undefined" && Array.isArray(gameState._newItemIds))
              gameState._newItemIds.push(item.itemId);
            const cfg = ITEMS_CONFIG[item.itemId];
            if (cfg && textArea) {
              textArea.innerHTML += `<p style="color:#94a3b8;">猎人帮你多找到了 <span style="color:${cfg.color}">${cfg.name}</span> ×${item.quantity}</p>`;
            }
          }
        }
        if (hasHunterGain && textArea) scrollTextAreaToBottom(textArea);
      }
      } // end for _lt (randomLootCount loop)
    }
  }

  // 游戏结束直接标志
  if (fx.gameOver) {
    truckState.durability = 0;
    if (typeof triggerGameOver === "function") {
      triggerGameOver("game_over_event");
    } else {
      truckState.durability = -999;
      updateTruckStatusDisplay();
    }
  }

  // ── 小丑之夜：狂欢 ── 将所有非珍品物品转为小丑盲盒
  if (fx.clownCarnival && typeof inventoryState !== "undefined" && inventoryState.items) {
    let converted = 0;
    for (let i = 0; i < inventoryState.items.length; i++) {
      const slot = inventoryState.items[i];
      const cfg = ITEMS_CONFIG[slot.id];
      if (!cfg || cfg.category === "treasure") continue;
      const qty = slot.quantity || 1;
      inventoryState.items[i] = { id: "小丑盲盒", quantity: qty };
      converted += qty;
    }
    // 合并同类盲盒
    const boxSlots = inventoryState.items.filter(s => s.id === "小丑盲盒");
    const totalBoxes = boxSlots.reduce((sum, s) => sum + (s.quantity || 1), 0);
    inventoryState.items = inventoryState.items.filter(s => s.id !== "小丑盲盒");
    if (totalBoxes > 0) {
      inventoryState.items.push({ id: "小丑盲盒", quantity: totalBoxes });
    }
    textArea.innerHTML += `<p style="color:#ff00ff">🤡 你的 ${converted} 件物品全部变成了 <span style="color:#ff00ff;font-weight:bold;">小丑盲盒</span>！</p>`;
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
  }

  // ── 小丑之夜：迷茫 ── 三项属性随机打乱
  if (fx.clownConfusion) {
    const vals = [truckState.fuel, truckState.durability, truckState.comfort];
    // Fisher-Yates 洗牌
    for (let i = vals.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [vals[i], vals[j]] = [vals[j], vals[i]];
    }
    const oldF = truckState.fuel, oldD = truckState.durability, oldC = truckState.comfort;
    truckState.fuel = vals[0];
    truckState.durability = vals[1];
    truckState.comfort = vals[2];
    textArea.innerHTML += `<p style="color:#ff00ff">🌀 属性被打乱了！燃油 ${oldF}→${vals[0]}，耐久 ${oldD}→${vals[1]}，舒适 ${oldC}→${vals[2]}</p>`;
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  }

  // ── 小丑之夜：逃跑失败 → 小丑结局 ──
  if (fx.clownEnding) {
    if (typeof showGameOver === "function") {
      showGameOver("clown_night");
    }
  }

  // ── 时间银行：存款 ── 弹出存款弹窗
  if (fx.timeBankDeposit) {
    showTimeBankDepositModal(textArea);
  }

  // ── 时间银行：取款 ── 弹出取款弹窗
  if (fx.timeBankWithdraw) {
    showTimeBankWithdrawModal(textArea);
  }

  // ── 时间银行：拒绝 → 全屏故障 + 时间乱流结局 ──
  if (fx.timeBankRefuse) {
    triggerTimeBankGlitchAndEnd(textArea);
  }

  // ── 命运轮盘：打开转盘弹窗 ──
  if (fx.fateRouletteOpen) {
    showFateRouletteModal(textArea);
  }

  // ── 虚数现象：属性强制设置（格式化空间）──
  if (typeof fx.fuelSet === "number") {
    truckState.fuel = clamp(fx.fuelSet);
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  }
  if (typeof fx.durabilitySet === "number") {
    truckState.durability = clamp(fx.durabilitySet);
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  }
  if (typeof fx.comfortSet === "number") {
    truckState.comfort = clamp(fx.comfortSet);
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  }
  // ── 虚数现象：金币强制设置 ──
  if (typeof fx.goldSet === "number") {
    inventoryState.gold = Math.max(0, fx.goldSet);
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
  }
  if (fx.fuelSet !== undefined || fx.durabilitySet !== undefined || fx.comfortSet !== undefined) {
    const fStr = fx.fuelSet !== undefined ? fx.fuelSet + "%" : "—";
    const dStr = fx.durabilitySet !== undefined ? fx.durabilitySet + "%" : "—";
    const cStr = fx.comfortSet !== undefined ? fx.comfortSet + "%" : "—";
    const gStr = fx.goldSet !== undefined ? `，金币重置为 <b style="color:#facc15">${fx.goldSet}🪙</b>` : "";
    textArea.innerHTML += `<p style="color:#22d3ee">📊 空间格式化完成：燃油 <b>${fStr}</b> / 耐久 <b>${dStr}</b> / 舒适 <b>${cStr}</b>${gStr}</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // ── 虚数现象：格式化全屏特效 ──
  if (fx.imaginaryFormatEffect) {
    triggerImaginaryFormatEffect();
  }

  // ── 虚数现象：adminPermitEnable 已废弃（改为使用物品时激活），此处保留兜底无操作 ──
  if (fx.adminPermitEnable) {
    // 仅作标记，实际激活在 useItemFromInventory 的 adminPermitActivate 分支中处理
  }
}

// ─── 虚数现象特效函数 ──────────────────────────────────────────────────

// 触发虚数格式化特效：全屏反色翻转 + 像素扫描线 + 结束时青光闪烁
function triggerImaginaryFormatEffect() {
  const DURATION = 2400; // ms，与 CSS animation 时长对齐

  // 1. 全屏反色 overlay（mix-blend-mode: difference 实现半透明反色）
  const overlay = document.createElement("div");
  overlay.id = "imaginary-format-overlay";
  document.body.appendChild(overlay);

  // 2. game-canvas 同步施加反色故障动画
  const gameCanvas = document.getElementById("game-canvas");
  if (gameCanvas) gameCanvas.classList.add("imaginary-glitch");

  // 3. 随机生成 8-14 条高亮扫描线从上扫到下
  const scanLines = [];
  const lineCount = 8 + Math.floor(Math.random() * 7);
  const colors = [
    "rgba(34,211,238,0.7)",   // 青色
    "rgba(167,139,250,0.6)",  // 紫色
    "rgba(251,191,36,0.55)",  // 金色
    "rgba(255,255,255,0.5)",  // 白色
    "rgba(239,68,68,0.5)",    // 红色
  ];
  for (let i = 0; i < lineCount; i++) {
    const line = document.createElement("div");
    line.className = "imaginary-scanline";
    const h = 2 + Math.floor(Math.random() * 6);
    const delay = (Math.random() * DURATION * 0.75 / 1000).toFixed(3);
    const dur = (0.35 + Math.random() * 0.55).toFixed(3);
    const color = colors[Math.floor(Math.random() * colors.length)];
    // 高斯辉光模拟：用 box-shadow
    line.style.cssText = `
      height: ${h}px;
      background: ${color};
      box-shadow: 0 0 ${6 + h * 2}px ${color};
      animation-duration: ${dur}s;
      animation-delay: ${delay}s;
    `;
    document.body.appendChild(line);
    scanLines.push(line);
  }

  // 4. 结束时 (DURATION * 0.75ms) 叠加一次青光闪烁
  setTimeout(() => {
    const flash = document.createElement("div");
    flash.id = "imaginary-flash";
    document.body.appendChild(flash);
    setTimeout(() => flash.remove(), 1000);
  }, DURATION * 0.7);

  // 5. 清理所有元素
  setTimeout(() => {
    overlay.remove();
    scanLines.forEach(l => l.remove());
    if (gameCanvas) gameCanvas.classList.remove("imaginary-glitch");
  }, DURATION + 100);
}

// ─── 时间银行系统函数 ────────────────────────────────────────────────

// 触发全屏故障特效（持续 durationMs 毫秒）
function triggerTimeBankGlitchEffect(durationMs, onDone) {
  const body = document.body;
  // 在整个 body 上叠加一层扭曲 overlay
  const overlay = document.createElement("div");
  overlay.id = "time-bank-glitch-overlay";
  overlay.style.cssText = `
    position: fixed; inset: 0; z-index: 99999; pointer-events: none;
    background: repeating-linear-gradient(
      0deg,
      rgba(0,229,255,0.04) 0px, rgba(0,229,255,0.04) 1px,
      transparent 1px, transparent 3px
    );
    animation: timeBankGlitchFull ${(durationMs / 1000).toFixed(2)}s ease-in-out both;
  `;
  document.body.appendChild(overlay);

  // game-canvas 同步加扭曲
  const gameCanvas = document.getElementById("game-canvas");
  if (gameCanvas) {
    gameCanvas.style.animation = `timeBankGlitchFull ${(durationMs / 1000).toFixed(2)}s ease-in-out both`;
  }

  // 随机生成若干乱码扫描线
  const scanLines = [];
  const lineCount = 6 + Math.floor(Math.random() * 6);
  for (let i = 0; i < lineCount; i++) {
    const line = document.createElement("div");
    const top = Math.random() * 100;
    const delay = Math.random() * (durationMs * 0.7);
    const h = 1 + Math.floor(Math.random() * 4);
    const color = Math.random() > 0.5 ? "rgba(0,229,255,0.6)" : "rgba(255,0,200,0.5)";
    line.style.cssText = `
      position: fixed; left: 0; right: 0; top: ${top}vh; height: ${h}px;
      background: ${color}; z-index: 100000; pointer-events: none;
      animation: timeBankGlitchFull ${(Math.random() * 0.3 + 0.15).toFixed(2)}s ease-in-out ${(delay / 1000).toFixed(2)}s both;
    `;
    document.body.appendChild(line);
    scanLines.push(line);
  }

  setTimeout(() => {
    overlay.remove();
    scanLines.forEach(l => l.remove());
    if (gameCanvas) gameCanvas.style.animation = "";
    if (onDone) onDone();
  }, durationMs);
}

// 时间银行：存款弹窗
function showTimeBankDepositModal(textArea) {
  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  const balance = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;
  const currentGold = inventoryState.gold || 0;

  const modal = document.createElement("div");
  modal.id = "time-bank-modal";
  modal.className = "text-area-scroll absolute inset-0 bg-black/75 flex items-center justify-center z-[60]";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";
  modal.style.backdropFilter = "blur(2px)";

  modal.innerHTML = `
    <div class="time-bank-event-border bg-[#000a1a] rounded-xl w-[88%] max-w-sm p-5 relative" style="animation:timeBankGlitch 3s ease-in-out infinite;">
      <div class="text-center mb-4">
        <div style="font-size:2rem;line-height:1;margin-bottom:8px;">⏳</div>
        <div class="font-bold text-lg" style="color:#00e5ff;">时间银行 — 存款</div>
        <div class="text-xs text-gray-500 mt-1">存入的金币将跨存档保存，随时取出</div>
      </div>
      <div class="bg-black/40 rounded-lg p-3 mb-4 space-y-1.5 text-sm">
        <div class="flex justify-between">
          <span class="text-gray-400">当前持有</span>
          <span class="text-yellow-400 font-bold">${currentGold} 🪙</span>
        </div>
        <div class="flex justify-between">
          <span class="text-gray-400">银行余额</span>
          <span style="color:#00e5ff;font-weight:bold;">${balance} 🪙</span>
        </div>
      </div>
      ${currentGold <= 0 ? `
        <div class="text-center text-gray-500 text-sm py-3">你目前没有金币可以存入。</div>
        <button onclick="closeTimeBankModal()" class="w-full mt-3 py-2 rounded-lg text-sm text-gray-400 border border-gray-700 hover:border-gray-500 transition-all">关闭</button>
      ` : `
        <div class="mb-3">
          <label class="text-xs text-gray-400 mb-1.5 block">存入金额（1 ~ ${currentGold}）</label>
          <div class="flex gap-2 items-center">
            <input id="time-bank-amount" type="number" min="1" max="${currentGold}" value="${currentGold}"
              class="flex-1 bg-black/60 border border-[#00e5ff]/40 rounded-lg px-3 py-2 text-white text-sm outline-none focus:border-[#00e5ff]"
              style="color:#00e5ff;">
            <button onclick="document.getElementById('time-bank-amount').value=${currentGold}"
              class="px-2 py-2 text-xs rounded-lg border border-gray-700 text-gray-400 hover:text-white transition-all">全部</button>
          </div>
        </div>
        <div class="flex gap-2">
          <button onclick="confirmTimeBankDeposit()" class="flex-1 py-2 rounded-lg text-sm font-bold transition-all"
            style="background:linear-gradient(to right,#004d7a,#00e5ff33);border:1px solid #00e5ff80;color:#00e5ff;">
            💰 确认存入
          </button>
          <button onclick="closeTimeBankModal()" class="flex-1 py-2 rounded-lg text-sm text-gray-400 border border-gray-700 hover:border-gray-500 transition-all">
            取消
          </button>
        </div>
      `}
    </div>`;

  gameCanvas.appendChild(modal);
  window._timeBankTextArea = textArea;
  window._timeBankMaxDeposit = currentGold;
  window._timeBankFromEvent = true;
  if (typeof startTimeBankMusic === "function") startTimeBankMusic();
}

// 时间银行：取款弹窗
function showTimeBankWithdrawModal(textArea) {
  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  const balance = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;

  const modal = document.createElement("div");
  modal.id = "time-bank-modal";
  modal.className = "text-area-scroll absolute inset-0 bg-black/75 flex items-center justify-center z-[60]";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";
  modal.style.backdropFilter = "blur(2px)";

  modal.innerHTML = `
    <div class="time-bank-event-border bg-[#000a1a] rounded-xl w-[88%] max-w-sm p-5 relative" style="animation:timeBankGlitch 3s ease-in-out infinite;">
      <div class="text-center mb-4">
        <div style="font-size:2rem;line-height:1;margin-bottom:8px;">🏦</div>
        <div class="font-bold text-lg" style="color:#00e5ff;">时间银行 — 取款</div>
        <div class="text-xs text-gray-500 mt-1">取回你之前存入的跨档金币</div>
      </div>
      <div class="bg-black/40 rounded-lg p-3 mb-4 space-y-1.5 text-sm">
        <div class="flex justify-between">
          <span class="text-gray-400">银行余额</span>
          <span style="color:#00e5ff;font-weight:bold;">${balance} 🪙</span>
        </div>
        <div class="flex justify-between">
          <span class="text-gray-400">当前持有</span>
          <span class="text-yellow-400 font-bold">${inventoryState.gold || 0} 🪙</span>
        </div>
      </div>
      ${balance <= 0 ? `
        <div class="text-center text-gray-500 text-sm py-3">账户余额为零，无法取款。</div>
        <button onclick="closeTimeBankModal()" class="w-full mt-3 py-2 rounded-lg text-sm text-gray-400 border border-gray-700 hover:border-gray-500 transition-all">关闭</button>
      ` : `
        <div class="mb-3">
          <label class="text-xs text-gray-400 mb-1.5 block">取出金额（1 ~ ${balance}）</label>
          <div class="flex gap-2 items-center">
            <input id="time-bank-amount" type="number" min="1" max="${balance}" value="${balance}"
              class="flex-1 bg-black/60 border border-[#00e5ff]/40 rounded-lg px-3 py-2 text-white text-sm outline-none focus:border-[#00e5ff]"
              style="color:#00e5ff;">
            <button onclick="document.getElementById('time-bank-amount').value=${balance}"
              class="px-2 py-2 text-xs rounded-lg border border-gray-700 text-gray-400 hover:text-white transition-all">全部</button>
          </div>
        </div>
        <div class="flex gap-2">
          <button onclick="confirmTimeBankWithdraw()" class="flex-1 py-2 rounded-lg text-sm font-bold transition-all"
            style="background:linear-gradient(to right,#004d7a,#00e5ff33);border:1px solid #00e5ff80;color:#00e5ff;">
            💸 确认取出
          </button>
          <button onclick="closeTimeBankModal()" class="flex-1 py-2 rounded-lg text-sm text-gray-400 border border-gray-700 hover:border-gray-500 transition-all">
            取消
          </button>
        </div>
      `}
    </div>`;

  gameCanvas.appendChild(modal);
  window._timeBankTextArea = textArea;
  window._timeBankMaxWithdraw = balance;
  window._timeBankFromEvent = true;
  if (typeof startTimeBankMusic === "function") startTimeBankMusic();
}

// 关闭时间银行弹窗（淡出）
function closeTimeBankModal(callback) {
  if (typeof stopTimeBankMusic === "function") stopTimeBankMusic();
  const modal = document.getElementById("time-bank-modal");
  if (!modal) { if (callback) callback(); return; }
  modal.style.animation = "eventModalFadeOut 0.18s ease-in both";
  const done = () => {
    modal.remove();
    if (callback) {
      callback();
    } else {
      // 取消：仅在事件中（游戏被暂停时）才恢复游戏
      if (window._timeBankFromEvent && typeof resumeGame === "function") {
        resumeGame();
      }
      window._timeBankFromEvent = false;
    }
  };
  modal.addEventListener("animationend", done, { once: true });
  setTimeout(done, 250);
}

// 确认存款
function confirmTimeBankDeposit() {
  const input = document.getElementById("time-bank-amount");
  if (!input) return;
  const max = window._timeBankMaxDeposit || 0;
  let amount = parseInt(input.value, 10);
  if (isNaN(amount) || amount < 1) amount = 1;
  if (amount > max) amount = max;
  if (amount <= 0 || inventoryState.gold < amount) {
    input.style.borderColor = "#ff4444";
    return;
  }

  const textArea = window._timeBankTextArea;
  closeTimeBankModal(() => {
    // 扣除金币
    inventoryState.gold -= amount;
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
    // 存入时间银行
    if (typeof depositToTimeBank === "function") depositToTimeBank(amount);
    const newBalance = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : amount;
    // 短暂故障特效
    triggerTimeBankGlitchEffect(800);
    // 文字反馈
    if (textArea) {
      textArea.innerHTML += `<p style="color:#00e5ff;">⏳ 时间银行：您已存入 <strong>${amount}</strong> 枚金币。银行余额：${newBalance}🪙</p>`;
      scrollTextAreaToBottom(textArea);
    }
    saveGame();
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    // 没有其他子模态框且是事件中触发，则恢复游戏
    if (window._timeBankFromEvent && !document.getElementById("time-bank-modal") && typeof resumeGame === "function") {
      resumeGame();
    }
    window._timeBankFromEvent = false;
  });
}

// 确认取款
function confirmTimeBankWithdraw() {
  const input = document.getElementById("time-bank-amount");
  if (!input) return;
  const max = window._timeBankMaxWithdraw || 0;
  let amount = parseInt(input.value, 10);
  if (isNaN(amount) || amount < 1) amount = 1;
  if (amount > max) amount = max;
  const textArea = window._timeBankTextArea;
  closeTimeBankModal(() => {
    // 从时间银行取出
    const actual = typeof withdrawFromTimeBank === "function" ? withdrawFromTimeBank(amount) : 0;
    if (actual > 0) {
      if (typeof addGold === "function") addGold(actual);
      const remaining = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;
      // 短暂故障特效
      triggerTimeBankGlitchEffect(600);
      if (textArea) {
        textArea.innerHTML += `<p style="color:#00e5ff;">⏳ 时间银行：您已取出 <strong>${actual}</strong> 枚金币。银行余额：${remaining}🪙</p>`;
        scrollTextAreaToBottom(textArea);
      }
    } else {
      if (textArea) {
        textArea.innerHTML += `<p style="color:#6b7280;">⏳ 时间银行余额不足，无法取款。</p>`;
        scrollTextAreaToBottom(textArea);
      }
    }
    saveGame();
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    // 没有其他子模态框且是事件中触发，则恢复游戏
    if (window._timeBankFromEvent && !document.getElementById("time-bank-modal") && typeof resumeGame === "function") {
      resumeGame();
    }
    window._timeBankFromEvent = false;
  });
}

// 时间银行：拒绝 → 全屏强烈故障特效 → 时间乱流结局
function triggerTimeBankGlitchAndEnd(textArea) {
  if (textArea) {
    textArea.innerHTML += `<p style="color:#00e5ff;">屏幕开始扭曲……文字化为乱码……</p>`;
    scrollTextAreaToBottom(textArea);
  }
  // 强烈故障特效 2.5 秒后触发结局
  triggerTimeBankGlitchEffect(2500, () => {
    // 存入 10 金币抚恤金到时间银行
    if (typeof setTimeBankBalance === "function") {
      const current = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;
      setTimeBankBalance(current + 10);
    }
    if (typeof showGameOver === "function") {
      showGameOver("time_rift");
    }
  });
}

// 时间存折：使用时弹出取款弹窗（由 inventory-display.js 的 useItem 调用）
function useTimeBankPassbook(textArea) {
  const balance = typeof getTimeBankBalance === "function" ? getTimeBankBalance() : 0;
  if (balance <= 0) {
    if (textArea) {
      textArea.innerHTML += `<p style="color:#6b7280;">⏳ 时间存折：银行账户余额为零，没有可取的金币。</p>`;
      scrollTextAreaToBottom(textArea);
    } else {
      // 弹出简单提示
      const gameCanvas = document.getElementById("game-canvas");
      if (gameCanvas) {
        const toast = document.createElement("div");
        toast.style.cssText = `
          position:fixed;top:30%;left:50%;transform:translateX(-50%);
          background:#000a1a;border:1px solid #00e5ff60;color:#00e5ff;
          padding:12px 24px;border-radius:8px;z-index:99990;font-size:0.875rem;
          animation:eventModalFadeIn 0.15s ease-out both;
        `;
        toast.textContent = "⏳ 时间存折：银行账户余额为零";
        document.body.appendChild(toast);
        setTimeout(() => {
          toast.style.animation = "eventModalFadeOut 0.15s ease-in both";
          setTimeout(() => toast.remove(), 200);
        }, 2000);
      }
    }
    return;
  }
  showTimeBankWithdrawModal(textArea || document.getElementById("textArea"));
  window._timeBankFromEvent = false; // 非事件触发，关闭后不恢复游戏
}

// 关闭事件弹窗
function removeEventModal() {
  const modal = document.getElementById("event-modal");
  if (modal) {
    modal.classList.add("modal-fade-out");
    modal.addEventListener("animationend", () => modal.remove(), { once: true });
  }
}

/** 带淡出的弹窗移除（通用） */
function fadeOutAndRemoveModal(modalId, callback) {
  const modal = document.getElementById(modalId);
  if (!modal) { if (callback) callback(); return; }
  modal.classList.add("modal-fade-out");
  modal.addEventListener("animationend", () => {
    modal.remove();
    if (callback) callback();
  }, { once: true });
}

// 恢复游戏运行
function resumeGame() {
  // 清除事件背景主题特效
  if (typeof clearEventTheme === "function") clearEventTheme();
  gameState.eventTriggered = false;
  resumeRoad();
  resumeTextGeneration();
  saveGame();
}

// 显示二级选择弹窗（仅覆盖游戏画面 #game-canvas，其他区域保持可交互）
function showSubChoiceModal(choiceData, textArea) {
  const oldModal = document.getElementById("sub-choice-modal");
  if (oldModal) oldModal.remove();

  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  let optionsHtml = choiceData.options
    .map((opt, i) => {
      // 检查子选项是否可用
      const fx = opt.effects;
      const hasEnoughItems =
        !fx ||
        !fx.removeItems ||
        fx.removeItems.every((item) => hasItem(item.id, item.quantity));
      const hasEnoughGold =
        !fx ||
        !fx.gold ||
        fx.gold >= 0 ||
        inventoryState.gold >= Math.abs(fx.gold);
      const available = hasEnoughItems && hasEnoughGold;

      // 生成缺少资源提示
      let missingStr = "";
      if (!available) {
        const missing = [];
        if (fx && fx.removeItems) {
          fx.removeItems.forEach((item) => {
            if (!hasItem(item.id, item.quantity)) {
              const cfg = ITEMS_CONFIG[item.id];
              missing.push((cfg ? cfg.name : item.id) + "×" + item.quantity);
            }
          });
        }
        if (
          fx &&
          fx.gold &&
          fx.gold < 0 &&
          inventoryState.gold < Math.abs(fx.gold)
        ) {
          missing.push("金币×" + Math.abs(fx.gold));
        }
        missingStr = missing.join("、");
      }

      if (available) {
        return `
		<button onclick="handleSubChoice(${i})"
			class="sub-choice-option w-full bg-gradient-to-r from-gray-700 to-gray-800 hover:from-red-800/80 hover:to-red-700/80 text-white rounded-lg text-left transition-all border border-gray-600 hover:border-red-500">
			<div class="font-bold mb-0.5">${opt.text}</div>
			${opt.description ? `<div class="text-gray-400">${opt.description}</div>` : ""}
		</button>`;
      } else {
        return `
		<button disabled
			class="sub-choice-option w-full bg-gray-900 text-gray-600 rounded-lg text-left border border-gray-800 cursor-not-allowed opacity-60">
			<div class="font-bold mb-0.5">${opt.text}</div>
			${opt.description ? `<div class="text-gray-600">${opt.description}</div>` : ""}
			<div class="text-red-400 mt-0.5">缺少：${missingStr}</div>
		</button>`;
      }
    })
    .join("");

  const modal = document.createElement("div");
  modal.id = "sub-choice-modal";
  modal.className =
    "sub-choice-modal text-area-scroll absolute inset-0 bg-black/60 flex items-center justify-center z-[60]";
  modal.innerHTML = `
		<div class="sub-choice-inner text-area-scroll bg-[#1a1a2e] border border-[#c41e3a]/60 shadow-lg w-full">
			<p class="sub-choice-prompt text-gray-300 leading-relaxed">${choiceData.prompt}</p>
			<div class="space-y-1">${optionsHtml}</div>
		</div>`;
  gameCanvas.appendChild(modal);

  // 挂载到 window 供 onclick 访问
  window._pendingSubChoiceOptions = choiceData.options;
  window._pendingSubChoiceTextArea = textArea;
  window._pendingSubChoiceEventId = _currentProcessingEventId;
  window._pendingSubChoiceEvent = _currentProcessingEvent;
}

// 处理二级选择结果
let _subChoiceLocked = false;
function handleSubChoice(index) {
  if (_subChoiceLocked) return;
  _subChoiceLocked = true;

  const modal = document.getElementById("sub-choice-modal");
  if (modal) modal.remove();

  const option = window._pendingSubChoiceOptions[index];
  const textArea = window._pendingSubChoiceTextArea;

  if (option.message) {
    const msg = Array.isArray(option.message)
      ? option.message[Math.floor(Math.random() * option.message.length)]
      : option.message;
    textArea.innerHTML += `<p class="text-[#c41e3a]">【结果】${msg}</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 恢复当前事件标记（供 applyBasicEffect 判断护符免疫）
  _currentProcessingEventId = window._pendingSubChoiceEventId || null;
  _currentProcessingEvent = window._pendingSubChoiceEvent || null;

  if (option.effects) {
    processEffects(option.effects, textArea);
  }

  _currentProcessingEventId = null;
  _currentProcessingEvent = null;

  showPassengerDialogues(textArea);
  checkConditionalStories(textArea);

  if (
    typeof checkGameOverConditions === "function" &&
    checkGameOverConditions()
  ) {
    _subChoiceLocked = false;
    return;
  }

  // 检查是否又打开了新的子模态框
  const hasOpenSubModal =
    document.getElementById("rest-modal") ||
    document.getElementById("crafting-modal") ||
    document.getElementById("merchant-modal") ||
    document.getElementById("sub-choice-modal") ||
    document.getElementById("minesweeper-modal") ||
    document.getElementById("time-bank-modal") ||
    document.getElementById("fate-roulette-modal");

  if (!hasOpenSubModal) {
    // 检查超载：若载重>=最大值，弹出超载界面暂停游戏
    if (typeof checkOverweightAndShow === "function" && checkOverweightAndShow()) {
      _subChoiceLocked = false;
      return;
    }
    resumeGame();
  }

  _subChoiceLocked = false;
}

function clamp(val) {
  return Math.min(100, val); // 不限制下限，允许负数以确保死亡判定正确
}

// 触发被动物品效果（每次做出事件选择后调用）
function triggerPassiveItemEffects(textArea) {
  if (typeof inventoryState === "undefined" || !inventoryState.items) return;
  // 收集需要转化的物品（避免在遍历中修改数组）
  const transformations = [];
  for (const slot of inventoryState.items) {
    const config = ITEMS_CONFIG[slot.id];
    if (!config || !config.passive) continue;
    const p = config.passive;

    // 一次性管理员权限：持有时监测 debugUsed，使用过一次后自毁并关闭 Debug
    if (p.type === "admin_permit") {
      if (typeof gameState !== "undefined" && gameState.debugUsed === true) {
        transformations.push({ fromId: slot.id, toId: null });
        if (textArea) {
          const pElem = document.createElement("p");
          pElem.innerHTML = `<span style="color:#6b7280">📡 「一次性管理员权限」令牌检测到 Debug 操作已执行，已自我销毁。Debug 控制台随之关闭。</span>`;
          textArea.appendChild(pElem);
          scrollTextAreaToBottom(textArea);
        }
        if (typeof toggleDebugMode === "function") toggleDebugMode(false);
      }
      continue;
    }

    // 随缘而遇的尘埃：废墟事件中10%概率获得随机基础物资×1
    if (p.type === "dust_of_fate") {
      const isRuins = _currentProcessingEvent && eventHasTag(_currentProcessingEvent, "废墟");
      if (isRuins && Math.random() < (p.ruinsTriggerChance || 0.10)) {
        const pool = ["废金属", "布料", "草药", "空罐", "原油"];
        const picked = pool[Math.floor(Math.random() * pool.length)];
        if (typeof addItem === "function") addItem(picked, 1);
        if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
        if (textArea) {
          const pickedCfg = ITEMS_CONFIG[picked];
          const itemSpan = pickedCfg ? `<span style="color:${pickedCfg.color}">${picked}</span>` : picked;
          const pElem = document.createElement("p");
          pElem.innerHTML = `<span style="color:#d4b896">✦ 随缘而遇的尘埃 💎 感知到废墟的气息，带来了 ${itemSpan}×1</span>`;
          textArea.appendChild(pElem);
          scrollTextAreaToBottom(textArea);
        }
      }
      continue;
    }

    // 幸运符：每次做出抉择时，有2%概率（神秘事件中10%）获得随机基础物资×1
    if (p.type === "lucky_charm") {
      const isMystery = _currentProcessingEvent && eventHasTag(_currentProcessingEvent, "神秘");
      const chance = isMystery ? 0.10 : 0.02;
      if (Math.random() < chance) {
        const pool = ["废金属", "布料", "草药", "空罐", "原油"];
        const picked = pool[Math.floor(Math.random() * pool.length)];
        if (typeof addItem === "function") addItem(picked, 1);
        if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
        if (textArea) {
          const pickedCfg = ITEMS_CONFIG[picked];
          const itemSpan = pickedCfg ? `<span style="color:${pickedCfg.color}">${picked}</span>` : picked;
          const pElem = document.createElement("p");
          pElem.innerHTML = `<span style="color:#34d399">✦ 幸运符 💎 带来了好运${isMystery ? "（神秘加持）" : ""}！获得 ${itemSpan}×1</span>`;
          textArea.appendChild(pElem);
          scrollTextAreaToBottom(textArea);
        }
      }
      continue;
    }

    // 八音盒：每次做出抉择时，20%概率（神秘事件100%）恢复1%舒适
    if (p.type === "music_box") {
      const isMystery = _currentProcessingEvent && eventHasTag(_currentProcessingEvent, "神秘");
      const chance = isMystery ? 1.0 : 0.20;
      if (Math.random() < chance && typeof truckState !== "undefined" && truckState.comfort < 100) {
        truckState.comfort = clamp(truckState.comfort + 1);
        if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
        if (textArea) {
          const pElem = document.createElement("p");
          pElem.innerHTML = `<span style="color:#f472b6">♪ 八音盒 💎 奏出悠扬旋律${isMystery ? "（神秘共鸣）" : ""}！舒适 +1%</span>`;
          textArea.appendChild(pElem);
          scrollTextAreaToBottom(textArea);
        }
      }
      continue;
    }

    // 条件触发型被动（如海市蜃楼雕塑：燃油≤阈值时触发）
    if (p.type === "condition") {
      let triggered = false;
      if (p.condition === "fuel_low" && typeof truckState !== "undefined" && truckState.fuel > 0 && truckState.fuel <= (p.threshold || 10)) {
        triggered = true;
        if (p.fuel && truckState.fuel > 0) {
          truckState.fuel = clamp(truckState.fuel + p.fuel);
        }
        if (p.durability && truckState.durability > 0) {
          truckState.durability = clamp(truckState.durability + p.durability);
        }
        if (p.comfort && truckState.comfort > 0) {
          truckState.comfort = clamp(truckState.comfort + p.comfort);
        }
        if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
      }
      if (triggered) {
        // 显示触发提示
        if (textArea) {
          const parts = [];
          if (p.fuel) parts.push("燃油+" + p.fuel + "%");
          if (p.durability) parts.push("耐久+" + p.durability + "%");
          if (p.comfort) parts.push("舒适+" + p.comfort + "%");
          const p2 = document.createElement("p");
          p2.innerHTML = `<span style="color:${config.color}">✦ ${getItemDisplayName(config)} 闪耀出最后的光芒！（${parts.join("，")}）</span>`;
          textArea.appendChild(p2);
          scrollTextAreaToBottom(textArea);
        }
        // 记录转化
        if (p.transformTo) {
          transformations.push({ fromId: slot.id, toId: p.transformTo });
        }
      }
      continue;
    }

    // 随机触发型被动（鹿角护符、金马雕像等）——不对 <=0 的属性生效
    // 若有 ruinsTriggerChance 且当前事件带有「废墟」tag，则使用更高概率
    const isRuinsEvent = _currentProcessingEvent && eventHasTag(_currentProcessingEvent, "废墟");
    const effectiveChance = (isRuinsEvent && p.ruinsTriggerChance != null)
      ? p.ruinsTriggerChance
      : (p.triggerChance || 0);
    if (Math.random() < effectiveChance) {
      // 应用被动效果
      if (p.comfort && typeof truckState !== "undefined" && truckState.comfort > 0) {
        truckState.comfort = clamp(truckState.comfort + p.comfort);
        if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
      }
      if (p.fuel && typeof truckState !== "undefined" && truckState.fuel > 0) {
        truckState.fuel = clamp(truckState.fuel + p.fuel);
        if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
      }
      if (p.durability && typeof truckState !== "undefined" && truckState.durability > 0) {
        truckState.durability = clamp(truckState.durability + p.durability);
        if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
      }
      // 金币被动效果
      let goldGain = 0;
      if (p.goldMin && p.goldMax) {
        goldGain = Math.floor(Math.random() * (p.goldMax - p.goldMin + 1)) + p.goldMin;
        if (typeof addGold === "function") addGold(goldGain);
      }
      // 物品被动效果（如沥青滴落装置：概率获得原油）
      const gainedItemNames = [];
      if (p.addItems && Array.isArray(p.addItems)) {
        for (const item of p.addItems) {
          if (typeof addItem === "function" && addItem(item.id, item.quantity)) {
            const itemCfg = ITEMS_CONFIG[item.id];
            if (itemCfg) {
              gainedItemNames.push(`<span style="color:${itemCfg.color}">${getItemDisplayName(itemCfg)}</span>×${item.quantity}`);
            }
          }
        }
        if (gainedItemNames.length > 0 && typeof updateInventoryDisplay === "function") {
          updateInventoryDisplay();
        }
      }
      // 显示被动触发提示
      if (textArea) {
        const parts = [];
        if (p.fuel) parts.push("燃油+" + p.fuel + "%");
        if (p.durability) parts.push("耐久+" + p.durability + "%");
        if (p.comfort) parts.push("舒适+" + p.comfort + "%");
        if (goldGain > 0) parts.push("金币+" + goldGain + "🪙");
        if (gainedItemNames.length > 0) parts.push("获得 " + gainedItemNames.join("、"));
        const p2 = document.createElement("p");
        p2.innerHTML = `<span style="color:${config.color}">✦ ${getItemDisplayName(config)} 散发微光...（${parts.join("，")}）</span>`;
        textArea.appendChild(p2);
        scrollTextAreaToBottom(textArea);
      }
    }
  }
  // 执行物品转化
  for (const t of transformations) {
    if (typeof removeItem === "function") removeItem(t.fromId, 1);
    // toId 为 null 表示纯销毁（如管理员权限令牌自毁），无需添加转化物品
    if (t.toId !== null && t.toId !== undefined) {
      // 直接添加转化后的物品到库存（不受重量限制，因为重量相同）
      const existing = inventoryState.items.find(s => s.id === t.toId);
      const tConfig = ITEMS_CONFIG[t.toId];
      if (existing && tConfig && tConfig.stackable) {
        existing.quantity += 1;
      } else {
        inventoryState.items.push({ id: t.toId, quantity: 1 });
      }
    }
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
    // 转化提示（仅在有目标物品时显示）
    if (textArea && t.toId !== null && t.toId !== undefined) {
      const fromCfg = ITEMS_CONFIG[t.fromId];
      const toCfg = ITEMS_CONFIG[t.toId];
      if (fromCfg && toCfg) {
        const p2 = document.createElement("p");
        p2.innerHTML = `<span style="color:#9ca3af">✦ <span style="color:${fromCfg.color}">${getItemDisplayName(fromCfg)}</span> 碎裂了，变成了 <span style="color:${toCfg.color}">${getItemDisplayName(toCfg)}</span></span>`;
        textArea.appendChild(p2);
        scrollTextAreaToBottom(textArea);
      }
    }
  }
}

// 每次抉择后的全局随机掉落检查（不依赖持有物品，直接触发概率）
function checkGlobalRandomDrop(textArea) {
  // 「随缘而遇的尘埃」：每次抉择 0.1% 概率随机掉落（未持有时才触发）
  if (typeof inventoryState !== "undefined" && typeof hasItem === "function") {
    if (!hasItem("随缘而遇的尘埃", 1) && Math.random() < 0.001) {
      if (typeof addItem === "function") addItem("随缘而遇的尘埃", 1);
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
      if (textArea) {
        const cfg = ITEMS_CONFIG["随缘而遇的尘埃"];
        const color = cfg ? cfg.color : "#d4b896";
        const pElem = document.createElement("p");
        pElem.innerHTML = `<span style="color:${color}">✦ 一缕尘埃随风飘落，悄悄落进了你的车窗……获得珍品「随缘而遇的尘埃」💎</span>`;
        textArea.appendChild(pElem);
        scrollTextAreaToBottom(textArea);
      }
      if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
    }
  }
}

// ─── 困难模式工具函数 ──────────────────────────────
function hasHardModeBonus(bonusId) {
  return typeof gameState !== "undefined" && Array.isArray(gameState.hardModeBonuses) && gameState.hardModeBonuses.includes(bonusId);
}

// ─── 衰变 debuff 系统 ──────────────────────────────────────

// 每隔 DECAY_DEBUFF_INTERVAL 个事件选择后，获得一个随机 debuff
function checkAndApplyNewDebuff(textArea) {
  if (gameState.easyMode) return;
  if (typeof DECAY_DEBUFFS === "undefined" || !Array.isArray(DECAY_DEBUFFS) || DECAY_DEBUFFS.length === 0) return;
  const interval = (typeof DECAY_DEBUFF_INTERVAL !== "undefined") ? DECAY_DEBUFF_INTERVAL : 10;
  const total = gameState.totalEventsHandled || 0;
  if (total <= 0 || total % interval !== 0) return;

  // 随机选择一个 debuff
  const pick = DECAY_DEBUFFS[Math.floor(Math.random() * DECAY_DEBUFFS.length)];
  if (!pick) return;

  // 查找是否已有该 debuff，叠加层数
  if (!Array.isArray(gameState.activeDebuffs)) gameState.activeDebuffs = [];
  const existing = gameState.activeDebuffs.find(d => d.id === pick.id);
  if (existing) {
    existing.stacks += 1;
  } else {
    gameState.activeDebuffs.push({ id: pick.id, stacks: 1 });
  }

  // 即时效果（如沉重负担 onApply）
  let appliedOnApply = false;
  if (pick.onApply) {
    if (pick.onApply.maxWeight) {
      inventoryState.maxWeight = Math.max(5, inventoryState.maxWeight + pick.onApply.maxWeight);
      appliedOnApply = true;
    }
  }

  // 当前生效层数（用于通知显示）
  const currentEntry = existing || gameState.activeDebuffs.find(d => d.id === pick.id);
  const currentStacks = currentEntry ? currentEntry.stacks : 1;

  // 显示获得 debuff 的通知
  if (textArea) {
    const stackInfo = currentStacks > 1 ? `（${currentStacks}层）` : "";
    let extraInfo = "";
    if (appliedOnApply && pick.onApply.maxWeight) {
      extraInfo = ` <span style="color:#fb923c">（最大载重 → ${inventoryState.maxWeight}kg）</span>`;
    }
    textArea.innerHTML += `<p style="color:#f87171">⚠ 衰变！获得 debuff：${pick.icon} <b>${pick.name}</b>${stackInfo} —— ${pick.description}${extraInfo}</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 刷新 UI：onApply 型需要刷新库存面板（载重等）
  if (appliedOnApply) {
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
  }

  // 更新 debuff 显示
  if (typeof updateDebuffBar === "function") updateDebuffBar();
}

// 每次事件选择后，应用所有已激活的 debuff
function applyActiveDebuffs(textArea) {
  if (gameState.easyMode) return;
  if (!Array.isArray(gameState.activeDebuffs) || gameState.activeDebuffs.length === 0) return;
  if (typeof DECAY_DEBUFFS === "undefined") return;

  let goldChanged = false;

  for (const debuffEntry of gameState.activeDebuffs) {
    const cfg = DECAY_DEBUFFS.find(d => d.id === debuffEntry.id);
    if (!cfg) continue;
    const stacks = debuffEntry.stacks || 1;

    // 固定 perChoice 效果（乘以层数）
    if (cfg.perChoice) {
      // 若配置了触发概率，则按 perChoiceChance * stacks（cap 到 1.0）掷骰
      if (cfg.perChoiceChance !== undefined) {
        const triggerChance = Math.min(1, cfg.perChoiceChance * stacks);
        if (Math.random() >= triggerChance) continue; // 本次未触发，跳过
      }
      if (cfg.perChoice.fuel) truckState.fuel = clamp(truckState.fuel + cfg.perChoice.fuel * stacks);
      if (cfg.perChoice.durability) truckState.durability = clamp(truckState.durability + cfg.perChoice.durability * stacks);
      if (cfg.perChoice.comfort) truckState.comfort = clamp(truckState.comfort + cfg.perChoice.comfort * stacks);
      if (cfg.perChoice.gold) {
        inventoryState.gold = Math.max(0, inventoryState.gold + cfg.perChoice.gold * stacks);
        goldChanged = true;
      }
    }

    // 概率型效果（层数叠加概率）
    if (cfg.perChoiceRandom) {
      const chance = Math.min(1, (cfg.perChoiceRandom.baseChance || 0.15) * stacks);
      if (Math.random() < chance) {
        const stats = ["fuel", "durability", "comfort"];
        const pick = stats[Math.floor(Math.random() * stats.length)];
        const delta = cfg.perChoiceRandom.amount || -3;
        truckState[pick] = clamp(truckState[pick] + delta);
        if (textArea) {
          const nameMap = { fuel: "燃油", durability: "耐久", comfort: "舒适" };
          textArea.innerHTML += `<p style="color:#fb923c">🎲 ${cfg.icon} ${cfg.name}触发！${nameMap[pick]} ${delta}%</p>`;
          scrollTextAreaToBottom(textArea);
        }
      }
    }
  }

  if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  // 金币变动后刷新库存面板（钱包漏洞等）
  if (goldChanged && typeof updateInventoryDisplay === "function") updateInventoryDisplay();
}

// ─── 困难模式 perChoice 修饰符 ──────────────────────────────
function applyHardModePerChoice(textArea) {
  if (typeof gameState === "undefined" || !Array.isArray(gameState.hardModeTags) || gameState.hardModeTags.length === 0) {
    // 即使没有难度标签，也可能有加成需要应用（但加成依赖标签分数，没有标签就没加成）
  }
  if (typeof HARD_MODE_MODIFIERS !== "undefined" && Array.isArray(gameState.hardModeTags)) {
    for (const tagId of gameState.hardModeTags) {
      const mod = HARD_MODE_MODIFIERS.find(m => m.id === tagId);
      if (!mod) continue;

      // 固定 perChoice 效果
      if (mod.perChoice) {
        if (mod.perChoice.fuel) truckState.fuel = clamp(truckState.fuel + mod.perChoice.fuel);
        if (mod.perChoice.durability) truckState.durability = clamp(truckState.durability + mod.perChoice.durability);
        if (mod.perChoice.comfort) truckState.comfort = clamp(truckState.comfort + mod.perChoice.comfort);
      }

      // 随机 perChoiceRandom 效果
      if (mod.perChoiceRandom && Math.random() < (mod.perChoiceRandom.chance || 0)) {
        const stats = ["fuel", "durability", "comfort"];
        const pick = stats[Math.floor(Math.random() * stats.length)];
        const delta = mod.perChoiceRandom.amount || -5;
        truckState[pick] = clamp(truckState[pick] + delta);
        if (textArea) {
          const nameMap = { fuel: "燃油", durability: "耐久", comfort: "舒适" };
          textArea.innerHTML += `<p style="color:#ef4444">☠ 霉运降临！${nameMap[pick]} ${delta}%</p>`;
          scrollTextAreaToBottom(textArea);
        }
      }
    }
  }

  // ─── 加成效果 ───
  if (Array.isArray(gameState.hardModeBonuses) && gameState.hardModeBonuses.length > 0) {
    // 取长补短：最高属性 -1%，最低属性 +1%（仅对 >0 的属性生效，不复活已归零属性）
    if (gameState.hardModeBonuses.includes("balance_stats")) {
      const stats = { fuel: truckState.fuel, durability: truckState.durability, comfort: truckState.comfort };
      // 只对存活属性（>0）进行平衡
      const aliveKeys = Object.keys(stats).filter(k => stats[k] > 0);
      if (aliveKeys.length >= 2) {
        let maxKey = aliveKeys[0], minKey = aliveKeys[0];
        for (const k of aliveKeys) {
          if (stats[k] > stats[maxKey]) maxKey = k;
          if (stats[k] < stats[minKey]) minKey = k;
        }
        if (maxKey !== minKey) {
          truckState[maxKey] = Math.min(100, truckState[maxKey] - 1);
          truckState[minKey] = Math.min(100, truckState[minKey] + 1);
        }
      }
    }

    // 后备箱培养仓：每次抉择后 15% 概率获得草药×1
    if (gameState.hardModeBonuses.includes("trunk_incubator")) {
      if (Math.random() < 0.15) {
        if (typeof addItem === "function") {
          addItem("草药", 1);
          if (textArea) {
            textArea.innerHTML += `<p style="color:#10b981">🌿 后备箱培养仓产出了草药×1！</p>`;
            scrollTextAreaToBottom(textArea);
          }
        }
      }
    }
  }

  if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
}

// ======================== 命运轮盘 ========================

/**
 * 显示命运轮盘子弹窗
 * 四个扇区：金钱(+88)、物质(随机物品)、死亡(fate_mockery结局)、再来一次(死亡区扩大25%)
 * 每次点击"再来一次"后，死亡扇区角度增加 25%（基础 90° × 1.25^spinCount）
 */
function showFateRouletteModal(textArea) {
  if (typeof startFateRouletteMusic === "function") startFateRouletteMusic();
  // 初始化旋转次数（第一次打开从0开始）
  if (window._fateRouletteSpinCount === undefined) window._fateRouletteSpinCount = 0;

  // ── 第一步：淡出其他 UI 元素 ──────────────────────────────
  // 淡出事件选择框（仍留在 DOM，关闭时恢复）
  const eventModal = document.getElementById("event-modal");
  if (eventModal) {
    eventModal.classList.add("fate-roulette-dim-out");
    window._fateRouletteDimmedEventModal = eventModal;
  }
  // 淡出 canvas 外层 UI：文字区 / 乘客区 / 右侧面板
  const dimSelectors = ["#textArea", "#passenger-list", ".flex-\\[3\\]"];
  // 用更稳健的方式找到这些外层元素
  window._fateRouletteDimmedEls = [];
  const textAreaEl = document.getElementById("textArea");
  if (textAreaEl) {
    // 淡出整个文字区父容器
    const textAreaParent = textAreaEl.parentElement;
    if (textAreaParent) {
      textAreaParent.classList.add("fate-roulette-dim-out");
      window._fateRouletteDimmedEls.push(textAreaParent);
    }
  }
  // 右侧面板：#game-canvas 的父级 flex 容器下第二个子元素（右侧库存面板）
  const gameCanvas = document.getElementById("game-canvas");
  if (gameCanvas) {
    const canvasWrapper = gameCanvas.parentElement; // 左侧大块
    if (canvasWrapper) {
      const outerFlex = canvasWrapper.parentElement; // 左右分栏 flex
      if (outerFlex) {
        for (const child of outerFlex.children) {
          if (!child.contains(gameCanvas)) {
            child.classList.add("fate-roulette-dim-out");
            window._fateRouletteDimmedEls.push(child);
          }
        }
      }
      // 乘客区（canvas 下方同级）
      for (const child of canvasWrapper.children) {
        if (!child.contains(gameCanvas) && child.id !== "textArea") {
          child.classList.add("fate-roulette-dim-out");
          window._fateRouletteDimmedEls.push(child);
        }
      }
    }
  }

  // ── 第二步：创建全屏遮罩弹窗（挂到 body） ─────────────────
  const modal = document.createElement("div");
  modal.id = "fate-roulette-modal";
  // 先透明，动画过程中变黑
  modal.style.cssText = "animation:fateModalBgIn 0.6s ease forwards;";

  modal.innerHTML = `
    <div id="fate-roulette-inner" class="fate-roulette-border relative text-center"
         style="animation:fateRouletteCardIn 0.55s cubic-bezier(0.34,1.56,0.64,1) both;">
      <div style="font-size:1.8rem;font-weight:900;color:#ffd700;letter-spacing:3px;margin-bottom:4px;text-shadow:0 0 20px #ffd70099;">
        🎡 命运轮盘
      </div>
      <div style="font-size:0.78rem;color:#a78b3a;margin-bottom:18px;letter-spacing:1px;">转动转盘，接受命运的裁决</div>

      <div style="position:relative;display:inline-block;">
        <div id="fate-wheel-pointer">▼</div>
        <canvas id="fate-wheel-canvas" width="300" height="300"
                style="display:block;border-radius:50%;border:3px solid #ffd70088;"></canvas>
      </div>

      <div id="fate-roulette-result"
           style="min-height:40px;margin:16px 0 10px;font-size:1rem;color:#d4a800;font-weight:bold;"></div>

      <button id="fate-roulette-btn" onclick="spinFateRoulette()">
        🎰 转动！
      </button>
    </div>`;

  document.body.appendChild(modal);

  // ── 背景变黑动画（过渡到 rgba(2,1,0,0.95)）─────────────────
  // CSS 动画只能做 opacity，用 JS 在动画结束后设置最终背景色
  setTimeout(() => {
    if (modal.parentElement) modal.style.background = "rgba(2,1,0,0.95)";
  }, 600);

  // 存储上下文
  window._fateRouletteTextArea = textArea;
  window._fateRouletteSpinning = false;
  window._fateRouletteCurrentAngle = 0;

  // 初次绘制（使用更大的 canvas 尺寸 300px）
  _drawFateWheel(window._fateRouletteCurrentAngle);
}

/**
 * 计算各扇区角度（弧度）
 * 死亡扇区基础 90°，每次再来一次增加 25%（即 × 1.25^spinCount）
 */
function _getFateWheelSectors() {
  const spinCount = window._fateRouletteSpinCount || 0;
  const FULL = Math.PI * 2;

  // 死亡扇区（弧度），不超过 210° 上限防止游戏太难
  const deathBase = Math.PI / 2; // 90°
  const deathAngle = Math.min(deathBase * Math.pow(1.25, spinCount), (Math.PI * 210) / 180);

  // 剩余角度平均分给：金钱、物质、再来一次
  const remaining = FULL - deathAngle;
  const otherAngle = remaining / 3;

  return [
    { label: "💰 金钱", color: "#b8860b", brightColor: "#ffd700", angle: otherAngle, id: "gold" },
    { label: "📦 物质", color: "#1a5c2a", brightColor: "#22c55e", angle: otherAngle, id: "stuff" },
    { label: "💀 死亡", color: "#4a0000", brightColor: "#ff2222", angle: deathAngle, id: "death" },
    { label: "🔄 再来一次", color: "#2d0a5a", brightColor: "#a855f7", angle: otherAngle, id: "again" },
  ];
}

/** 绘制转盘，offsetAngle 为当前旋转角（弧度，从正上方开始） */
function _drawFateWheel(offsetAngle) {
  const canvas = document.getElementById("fate-wheel-canvas");
  if (!canvas) return;
  const ctx = canvas.getContext("2d");
  // 动态适配 canvas 实际尺寸（初版240，放大后300）
  const size = canvas.width;
  const cx = size / 2, cy = size / 2, r = size / 2 - 4;

  ctx.clearRect(0, 0, size, size);

  const sectors = _getFateWheelSectors();
  // 起始角：指针指向正上方 = -π/2，加上 offsetAngle
  let startAngle = -Math.PI / 2 + offsetAngle;

  // 绘制各扇区
  for (const sec of sectors) {
    const endAngle = startAngle + sec.angle;

    // 扇形渐变（内深外亮）
    const grad = ctx.createRadialGradient(cx, cy, r * 0.18, cx, cy, r);
    grad.addColorStop(0, sec.color);
    grad.addColorStop(1, sec.brightColor + "99");

    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.arc(cx, cy, r, startAngle, endAngle);
    ctx.closePath();
    ctx.fillStyle = grad;
    ctx.fill();

    // 扇形边框
    ctx.strokeStyle = "#ffd70055";
    ctx.lineWidth = 1.5;
    ctx.stroke();

    // 文字
    const midAngle = startAngle + sec.angle / 2;
    const textR = r * 0.62;
    const tx = cx + Math.cos(midAngle) * textR;
    const ty = cy + Math.sin(midAngle) * textR;

    ctx.save();
    ctx.translate(tx, ty);
    ctx.rotate(midAngle + Math.PI / 2);
    ctx.fillStyle = "#fff";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";

    // 文字描边增加可读性
    ctx.shadowColor = "rgba(0,0,0,0.8)";
    ctx.shadowBlur = 4;

    // 拆分 emoji 和文字
    const parts = sec.label.split(" ");
    if (parts.length === 2) {
      const emojiSize = Math.round(size * 0.055);
      const textSize = Math.round(size * 0.042);
      ctx.font = `${emojiSize}px sans-serif`;
      ctx.fillText(parts[0], 0, -emojiSize * 0.7);
      ctx.font = `bold ${textSize}px sans-serif`;
      ctx.fillText(parts[1], 0, emojiSize * 0.6);
    } else {
      ctx.font = `bold ${Math.round(size * 0.045)}px sans-serif`;
      ctx.fillText(sec.label, 0, 0);
    }
    ctx.restore();

    startAngle = endAngle;
  }

  // 中心装饰圆
  const centerR = Math.round(size * 0.055);
  ctx.beginPath();
  ctx.arc(cx, cy, centerR, 0, Math.PI * 2);
  const centerGrad = ctx.createRadialGradient(cx, cy, 0, cx, cy, centerR);
  centerGrad.addColorStop(0, "#fff8e0");
  centerGrad.addColorStop(1, "#ffd700");
  ctx.fillStyle = centerGrad;
  ctx.fill();
  ctx.strokeStyle = "#1a0d00";
  ctx.lineWidth = 2;
  ctx.stroke();
}

/** 执行转盘旋转动画，结束后处理结果 */
function spinFateRoulette() {
  if (window._fateRouletteSpinning) return;

  if (typeof playRouletteSpinStart === "function") playRouletteSpinStart();

  const btn = document.getElementById("fate-roulette-btn");
  const resultDiv = document.getElementById("fate-roulette-result");
  if (!btn || !resultDiv) return;

  // 禁用按钮
  window._fateRouletteSpinning = true;
  btn.disabled = true;
  btn.style.opacity = "0.4";
  resultDiv.textContent = "转动中……";
  resultDiv.style.color = "#a78b3a";

  const sectors = _getFateWheelSectors();

  // 在各扇区中随机选一个落点（按角度权重）
  const totalAngle = Math.PI * 2;
  const rand = Math.random() * totalAngle;
  let acc = 0;
  let chosenSector = sectors[0];
  let chosenMidAngle = 0;
  let sectorStart = 0;
  for (const sec of sectors) {
    if (rand >= acc && rand < acc + sec.angle) {
      chosenSector = sec;
      // 在扇区内随机偏移（避免总落在正中央）
      const inOffset = (Math.random() * 0.7 + 0.15) * sec.angle;
      chosenMidAngle = acc + inOffset;
      sectorStart = acc;
      break;
    }
    acc += sec.angle;
  }

  // 计算需要旋转的总角度
  // 转盘旋转后，指针（正上方）对准 chosenMidAngle 处
  // 需要旋转的量：使 chosenMidAngle 落在 0（正上方）
  // 当前旋转偏移 + 旋转量 = -chosenMidAngle (mod 2π)
  // 加上若干整圈（4~7圈）增加动感
  const currentAngle = window._fateRouletteCurrentAngle || 0;
  const extraSpins = (4 + Math.floor(Math.random() * 4)) * Math.PI * 2;
  const targetAngle = currentAngle + extraSpins + (totalAngle - (chosenMidAngle % totalAngle));

  const duration = 3200; // 毫秒
  const startTime = performance.now();
  const startAngle = currentAngle;

  function easeOutCubic(t) {
    return 1 - Math.pow(1 - t, 3);
  }

  function animate(now) {
    const elapsed = now - startTime;
    const t = Math.min(elapsed / duration, 1);
    const angle = startAngle + (targetAngle - startAngle) * easeOutCubic(t);

    window._fateRouletteCurrentAngle = angle;
    _drawFateWheel(angle);

    if (t < 1) {
      requestAnimationFrame(animate);
    } else {
      // 动画结束，处理结果
      if (typeof playRouletteSpinStop === "function") playRouletteSpinStop();
      _handleFateRouletteResult(chosenSector);
    }
  }

  requestAnimationFrame(animate);
}

/** 处理转盘结果 */
function _handleFateRouletteResult(sector) {
  const resultDiv = document.getElementById("fate-roulette-result");
  const btn = document.getElementById("fate-roulette-btn");
  const textArea = window._fateRouletteTextArea;

  switch (sector.id) {
    case "gold": {
      // +88 金币
      if (typeof addGold === "function") addGold(88);
      else if (inventoryState) inventoryState.gold = (inventoryState.gold || 0) + 88;
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

      if (resultDiv) {
        resultDiv.innerHTML = '🪙 <span style="color:#ffd700">获得 <strong>+88</strong> 金币！</span>';
      }
      if (textArea) {
        textArea.innerHTML += `<p style="color:#ffd700;">🎡 命运轮盘：指针落在「金钱」——你赢得了 <strong>88</strong> 枚金币！</p>`;
        scrollTextAreaToBottom(textArea);
      }
      saveGame();
      if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
      // 延迟后关闭
      setTimeout(() => _closeFateRouletteModal(true), 1800);
      break;
    }

    case "stuff": {
      // 随机发放 5~8 件非宝物物品
      const itemPool = ["油桶", "修理包", "坐垫", "急救箱", "零食", "高级燃油",
                        "废金属", "布料", "草药", "空罐", "精炼剂", "轮胎", "电池", "橡胶", "皮革"];
      const count = 5 + Math.floor(Math.random() * 4);
      const gained = [];
      for (let i = 0; i < count; i++) {
        const name = itemPool[Math.floor(Math.random() * itemPool.length)];
        if (typeof addItem === "function") addItem(name, 1);
        gained.push(name);
      }
      if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

      const summary = gained.reduce((acc, n) => { acc[n] = (acc[n] || 0) + 1; return acc; }, {});
      const summaryText = Object.entries(summary).map(([n, c]) => `${n}×${c}`).join("、");

      if (resultDiv) {
        resultDiv.innerHTML = `📦 <span style="color:#22c55e">获得物资：<strong>${summaryText}</strong></span>`;
      }
      if (textArea) {
        textArea.innerHTML += `<p style="color:#22c55e;">🎡 命运轮盘：指针落在「物质」——一堆物资从天而降！获得：${summaryText}</p>`;
        scrollTextAreaToBottom(textArea);
      }
      saveGame();
      if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
      setTimeout(() => _closeFateRouletteModal(true), 2200);
      break;
    }

    case "death": {
      // 命运的愚弄结局
      if (resultDiv) {
        resultDiv.innerHTML = '💀 <span style="color:#ff2222">命运判决：<strong>死亡</strong></span>';
      }
      if (textArea) {
        textArea.innerHTML += `<p style="color:#ff2222;">🎡 命运轮盘：指针静止在那片血红的「死亡」扇区……</p>`;
        scrollTextAreaToBottom(textArea);
      }
      // 先等短暂展示结果，再触发结局
      setTimeout(() => {
        _closeFateRouletteModal(false);
        if (typeof showGameOver === "function") showGameOver("fate_mockery");
      }, 1600);
      break;
    }

    case "again": {
      // 再来一次：死亡区扩大 25%
      window._fateRouletteSpinCount = (window._fateRouletteSpinCount || 0) + 1;
      // 记录成就触发标记
      if (typeof gameState !== "undefined") gameState.fateRouletteRespan = true;
      if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();

      const spinCount = window._fateRouletteSpinCount;
      const deathPct = Math.round(Math.min(
        (Math.PI / 2) * Math.pow(1.25, spinCount) / (Math.PI * 2) * 100, 58
      ));

      if (resultDiv) {
        resultDiv.innerHTML = `🔄 <span style="color:#a855f7">再来一次！死亡区域已扩大至约 <strong>${deathPct}%</strong></span>`;
      }
      if (textArea) {
        textArea.innerHTML += `<p style="color:#a855f7;">🎡 命运轮盘：「再来一次」——但死亡的阴影正在蔓延（第 ${spinCount} 次重转）</p>`;
        scrollTextAreaToBottom(textArea);
      }

      // 重新绘制（死亡扇区已更大）并重新启用按钮
      setTimeout(() => {
        _drawFateWheel(window._fateRouletteCurrentAngle || 0);
        window._fateRouletteSpinning = false;
        if (btn) {
          btn.disabled = false;
          btn.style.opacity = "1";
          btn.textContent = "🎰 再转一次！";
        }
      }, 600);
      break;
    }
  }
}

/** 关闭命运轮盘弹窗 */
function _closeFateRouletteModal(resumeAfter) {
  if (typeof stopFateRouletteMusic === "function") stopFateRouletteMusic();
  const modal = document.getElementById("fate-roulette-modal");
  if (!modal) {
    _fateRouletteRestoreUi();
    if (resumeAfter) _onFateRouletteClose();
    return;
  }

  // 内卡缩小淡出
  const inner = document.getElementById("fate-roulette-inner");
  if (inner) inner.style.animation = "fateRouletteCardOut 0.35s ease-in both";

  // 遮罩整体淡出
  modal.style.animation = "fateModalBgOut 0.45s ease-in both";

  const done = () => {
    modal.remove();
    window._fateRouletteSpinCount = 0;
    window._fateRouletteSpinning = false;
    window._fateRouletteCurrentAngle = 0;
    // 恢复被淡出的 UI 元素
    _fateRouletteRestoreUi();
    if (resumeAfter) _onFateRouletteClose();
  };
  modal.addEventListener("animationend", done, { once: true });
  setTimeout(done, 500);
}

/** 恢复被命运轮盘淡出的 UI 元素 */
function _fateRouletteRestoreUi() {
  // 恢复事件选择框
  const eventModal = window._fateRouletteDimmedEventModal;
  if (eventModal && eventModal.parentElement) {
    eventModal.classList.remove("fate-roulette-dim-out");
    eventModal.classList.add("fate-roulette-dim-in");
    // 动画结束后清除类
    setTimeout(() => eventModal.classList.remove("fate-roulette-dim-in"), 500);
  }
  window._fateRouletteDimmedEventModal = null;

  // 恢复其他 UI
  const dimmed = window._fateRouletteDimmedEls || [];
  for (const el of dimmed) {
    if (el && el.parentElement) {
      el.classList.remove("fate-roulette-dim-out");
      el.classList.add("fate-roulette-dim-in");
      setTimeout(() => el.classList.remove("fate-roulette-dim-in"), 500);
    }
  }
  window._fateRouletteDimmedEls = [];
}

/** 轮盘关闭后恢复游戏流程 */
function _onFateRouletteClose() {
  if (typeof clearEventTheme === "function") clearEventTheme();
  if (typeof resumeRoad === "function") resumeRoad();
  if (typeof resumeTextGeneration === "function") resumeTextGeneration();
  if (typeof gameState !== "undefined") gameState.eventTriggered = false;
}

// ── 程序故障事件 ──────────────────────────────────────────────────────────────
// 曾同时持有 ≥2 个「一次性管理员权限」时，每次普通事件有 20% 概率触发此惩罚事件。
// 选项 A：接受死亡 → 权限贪婪者结局
// 选项 B：交出权限 → 移除所有管理员权限 + 关闭 debug
// 选项 C：强制重启进程 → 花屏特效后立刻返回主界面

// 乱码字符池
const _GARBLE_CHARS = "▓▒░█▄▀■□▪▫◆◇○●※†‡§¶Ω∞≈√∑∏∂∇⊕⊗⊥∧∨∩∪0x4E0F0x0xDEAD0xCAFE0x00FF0x1337ERRORWARNINGFATALOVFL";
const _GARBLE_WORDS = [
  "PERM_GREED_OVERFLOW","SEGFAULT","KERNEL_PANIC","NULL_PTR_DEREF",
  "STACK_SMASH","HEAP_CORRUPT","ACCESS_DENIED","SYS_HALT","ILLEGAL_OP",
  "ADMIN_ABUSE","REVOKE_ALL","PROCESS_KILL","0xDEADBEEF","CORE_DUMP",
  "ERR_FATAL","VIOLATION","LOCKOUT","PERMISSION_OVERFLOW","TERMINATE",
  "管理员权限溢出","系统崩溃","强制终止","违规操作","权限撤销",
];

/** 启动全屏花屏特效，duration ms 后自动移除（可传 onDone 回调） */
function _startProgramErrorOverlay(duration, onDone) {
  // 移除已有的（防重复）
  const old = document.getElementById("program-error-overlay");
  if (old) old.remove();

  const ov = document.createElement("div");
  ov.id = "program-error-overlay";

  // 主花屏层（对 body 整体应用 filter/transform）
  const glitch = document.createElement("div");
  glitch.className = "pe-glitch-layer";
  ov.appendChild(glitch);

  // RGB 分离层
  const rgbR = document.createElement("div");
  rgbR.className = "pe-rgb-r";
  ov.appendChild(rgbR);
  const rgbB = document.createElement("div");
  rgbB.className = "pe-rgb-b";
  ov.appendChild(rgbB);

  // 闪白层
  const flash = document.createElement("div");
  flash.className = "pe-flash";
  ov.appendChild(flash);

  // 扫描线（多条，错开延迟）
  for (let i = 0; i < 4; i++) {
    const sl = document.createElement("div");
    sl.className = "pe-scanline";
    sl.style.animationDelay = `${i * 0.15}s`;
    sl.style.animationDuration = `${0.45 + i * 0.12}s`;
    ov.appendChild(sl);
  }

  // 乱码文字（随机位置 + 随机内容）
  const garbleCount = 22;
  for (let i = 0; i < garbleCount; i++) {
    const el = document.createElement("div");
    el.className = "pe-garble";
    const word = _GARBLE_WORDS[Math.floor(Math.random() * _GARBLE_WORDS.length)];
    // 随机混入乱码字符
    let txt = word;
    if (Math.random() < 0.5) {
      const extra = Array.from({length: 4 + Math.floor(Math.random()*6)}, () =>
        _GARBLE_CHARS[Math.floor(Math.random() * _GARBLE_CHARS.length)]
      ).join("");
      txt = Math.random() < 0.5 ? extra + " " + word : word + " " + extra;
    }
    el.textContent = txt;
    const gx = (Math.random() * 90 - 5) + "vw";
    const gy = (Math.random() * 90 - 5) + "vh";
    el.style.left = gx;
    el.style.top = gy;
    el.style.setProperty("--gx", "0px");
    el.style.setProperty("--gy", "0px");
    el.style.setProperty("--gd", (0.4 + Math.random() * 0.8).toFixed(2) + "s");
    el.style.setProperty("--gdelay", (-Math.random() * 1.5).toFixed(2) + "s");
    el.style.fontSize = (0.65 + Math.random() * 0.7).toFixed(2) + "rem";
    el.style.color = Math.random() < 0.3 ? "#ff0060" : Math.random() < 0.5 ? "#22d3ee" : "#ffffff";
    el.style.opacity = (0.5 + Math.random() * 0.5).toFixed(2);
    ov.appendChild(el);
  }

  // 把花屏 filter 同步施加到整个 body（让 UI 全部受影响）
  document.body.style.animation = "programErrorGlitch 0.8s steps(1) infinite";

  document.body.appendChild(ov);

  if (duration > 0) {
    setTimeout(() => {
      _stopProgramErrorOverlay();
      if (typeof onDone === "function") onDone();
    }, duration);
  }
}

/** 停止并移除全屏花屏特效 */
function _stopProgramErrorOverlay() {
  const ov = document.getElementById("program-error-overlay");
  if (ov) ov.remove();
  document.body.style.animation = "";
}

function triggerProgramErrorEvent() {
  gameState.eventTriggered = true;
  pauseTextGeneration();

  // 显示触发文字到 textArea
  const textArea = document.getElementById("textArea");
  if (textArea) {
    textArea.innerHTML += `<p style="color:#22d3ee;font-family:monospace;">[ CRITICAL ERROR ] 系统检测到异常权限累积……程序故障正在蔓延。</p>`;
    scrollTextAreaToBottom(textArea);
  }

  // 先短暂触发花屏特效，再弹窗
  _startProgramErrorOverlay(0); // 0 = 持续（弹窗关闭后手动停止）

  if (typeof gradualStopRoad === "function") {
    gradualStopRoad(GAME_CONFIG.animation.roadDeceleration).then(() => {
      _showProgramErrorModal();
    });
  } else {
    _showProgramErrorModal();
  }
}

function _showProgramErrorModal() {
  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  const themeColor = "#22d3ee";
  const modal = document.createElement("div");
  modal.id = "event-modal";
  modal.className = "event-modal text-area-scroll absolute inset-0 bg-black/90 flex items-center justify-center z-50";
  modal.style.animation = "eventModalFadeIn 0.18s ease-out both";

  const choicesHtml = `
    <button onclick="_handleProgramErrorChoice('death')"
      class="event-modal-choice w-full text-white rounded-lg text-left transition-all duration-300"
      style="background:linear-gradient(to right,#374151,#1f2937);border:1px solid #4b5563;"
      onmouseenter="this.style.background='linear-gradient(to right,#7f1d1d,#450a0a)';this.style.borderColor='#ef4444';"
      onmouseleave="this.style.background='linear-gradient(to right,#374151,#1f2937)';this.style.borderColor='#4b5563';">
      <div class="choice-title font-bold mb-0.5">接受死亡</div>
      <div class="choice-desc text-gray-400">你贪婪地囤积了不该属于你的权限。系统有权收回一切。</div>
    </button>
    <button onclick="_handleProgramErrorChoice('surrender')"
      class="event-modal-choice w-full text-white rounded-lg text-left transition-all duration-300"
      style="background:linear-gradient(to right,#374151,#1f2937);border:1px solid #4b5563;"
      onmouseenter="this.style.background='linear-gradient(to right,#22d3ee33,#0e7490aa)';this.style.borderColor='#22d3ee';"
      onmouseleave="this.style.background='linear-gradient(to right,#374151,#1f2937)';this.style.borderColor='#4b5563';">
      <div class="choice-title font-bold mb-0.5">交出管理员权限</div>
      <div class="choice-desc text-gray-400">主动归还所有令牌，关闭 Debug 控制台。系统将没收后备箱全部物品，属性校准至 70%，金币清算为 6。</div>
    </button>
    <button onclick="_handleProgramErrorChoice('reboot')"
      class="event-modal-choice w-full text-white rounded-lg text-left transition-all duration-300"
      style="background:linear-gradient(to right,#374151,#1f2937);border:1px solid #4b5563;"
      onmouseenter="this.style.background='linear-gradient(to right,#1a0a2e,#0d0618aa)';this.style.borderColor='#a855f7';"
      onmouseleave="this.style.background='linear-gradient(to right,#374151,#1f2937)';this.style.borderColor='#4b5563';">
      <div class="choice-title font-bold mb-0.5">强制重启进程</div>
      <div class="choice-desc text-gray-400">抹去一切，从最初重新开始……进程将被完全终止并重载。</div>
    </button>`;

  modal.innerHTML = `
    <div class="relative bg-[#0a0f1a] border-2 rounded-xl p-6 max-w-md w-full mx-4 max-h-[90%] flex flex-col text-area-scroll overflow-y-auto"
         style="border-color:${themeColor};box-shadow:0 0 24px 4px #22d3ee44,0 0 60px 8px #22d3ee22;">
      <div class="text-center mb-1" style="font-family:monospace;color:#22d3ee;font-size:0.7rem;letter-spacing:0.1em;">SYSTEM ALERT — CRITICAL</div>
      <h3 class="text-xl font-bold mb-2 text-center" style="color:${themeColor};font-family:monospace;">⚠ 程序故障 ⚠</h3>
      <p class="text-sm text-gray-300 mb-1 text-center">检测到本次旅途中曾同时持有 <span style="color:#22d3ee;font-weight:bold;">≥2 枚管理员权限令牌</span></p>
      <p class="text-xs text-gray-500 mb-4 text-center" style="font-family:monospace;">ERROR CODE: PERMISSION_GREED_OVERFLOW</p>
      <div class="space-y-2">${choicesHtml}</div>
    </div>`;

  gameCanvas.appendChild(modal);
}

function _handleProgramErrorChoice(choice) {
  // 防重复
  if (_eventChoiceLocked) return;
  _eventChoiceLocked = true;

  const modal = document.getElementById("event-modal");
  if (modal) {
    modal.querySelectorAll("button").forEach(btn => {
      btn.disabled = true;
      btn.style.pointerEvents = "none";
    });
  }

  gameState.totalEventsHandled = (gameState.totalEventsHandled || 0) + 1;
  if (typeof sceneryTick === "function") sceneryTick();

  const textArea = document.getElementById("textArea");

  if (choice === "death") {
    // 选项A：接受死亡 → 权限贪婪者结局
    if (textArea) {
      textArea.innerHTML += `<p style="color:#ef4444;font-family:monospace;">[ PROCESS TERMINATED ] 你接受了系统的裁决……</p>`;
      scrollTextAreaToBottom(textArea);
    }
    // 加速花屏后进入结局
    _stopProgramErrorOverlay();
    _startProgramErrorOverlay(800, () => {
      if (modal) modal.remove();
      if (typeof showGameOver === "function") showGameOver("admin_permit_greed");
    });
    _eventChoiceLocked = false;
    return;
  }

  if (choice === "surrender") {
    // 选项B：交出所有管理员权限并关闭 debug
    const permitSlot = typeof inventoryState !== "undefined"
      ? inventoryState.items.find(s => s.id === "一次性管理员权限")
      : null;
    const removedCount = permitSlot ? permitSlot.quantity : 0;
    if (removedCount > 0 && typeof removeItem === "function") {
      removeItem("一次性管理员权限", removedCount);
    }
    // 关闭 debug
    if (typeof _debugFromAdminPermit !== "undefined" && _debugFromAdminPermit) {
      if (typeof _triggerAdminPermitSelfDestruct === "function") _triggerAdminPermitSelfDestruct();
    } else {
      if (typeof toggleDebugMode === "function") toggleDebugMode(false);
    }

    // 惩罚：清空背包所有物品（管理员权限已单独移除，此处清空剩余所有）
    if (typeof inventoryState !== "undefined") {
      inventoryState.items = [];
    }
    // 惩罚：三项属性重置为 70%
    if (typeof truckState !== "undefined") {
      truckState.fuel = 70;
      truckState.durability = 70;
      truckState.comfort = 70;
    }
    // 惩罚：金币变为 6
    if (typeof inventoryState !== "undefined") {
      inventoryState.gold = 6;
    }
    if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
    if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

    if (textArea) {
      const removedMsg = removedCount > 0
        ? `失去了 ${removedCount} 枚「一次性管理员权限」令牌，Debug 控制台已关闭。`
        : "Debug 控制台已关闭。";
      textArea.innerHTML += `<p style="color:#22d3ee;font-family:monospace;">[ PERMISSION REVOKED ] ${removedMsg} 系统没收了后备箱的全部物品，将三项属性强制校准至 70%，金币清算为 6🪙。系统故障暂时消退。</p>`;
      scrollTextAreaToBottom(textArea);
    }
    // 停止花屏并恢复游戏
    _stopProgramErrorOverlay();

  } else if (choice === "reboot") {
    // 选项C：强制重启进程 → 全力花屏动画后直接跳回主界面
    if (textArea) {
      textArea.innerHTML += `<p style="color:#a855f7;font-family:monospace;">[ FORCE REBOOT ] 强制终止当前进程……系统将重新载入。</p>`;
      scrollTextAreaToBottom(textArea);
    }
    // 花屏全开，1.5s 后淡出并跳转主界面（不保存存档）
    _stopProgramErrorOverlay();
    _startProgramErrorOverlay(0); // 立刻全力花屏
    if (modal) {
      modal.style.transition = "opacity 0.5s";
      modal.style.opacity = "0";
      setTimeout(() => modal.remove(), 500);
    }
    // 整页白闪 + 跳转
    setTimeout(() => {
      document.body.style.transition = "filter 0.3s, opacity 0.4s";
      document.body.style.filter = "invert(1) brightness(5) saturate(0)";
      document.body.style.opacity = "0";
    }, 1200);
    setTimeout(() => {
      _stopProgramErrorOverlay();
      if (typeof deleteSave === "function") deleteSave();
      window.location.href = "index.html";
    }, 1700);
    _eventChoiceLocked = false;
    return;
  }

  // surrender 选项：关闭弹窗并恢复游戏
  if (modal) {
    modal.classList.add("modal-fade-out");
    modal.addEventListener("animationend", () => modal.remove(), { once: true });
  }

  // 检查游戏结束
  if (typeof checkGameOverConditions === "function" && checkGameOverConditions()) {
    _eventChoiceLocked = false;
    return;
  }

  if (typeof saveGame === "function") saveGame();
  if (typeof clearEventTheme === "function") clearEventTheme();
  if (typeof resumeRoad === "function") resumeRoad();
  if (typeof resumeTextGeneration === "function") resumeTextGeneration();
  gameState.eventTriggered = false;
  _eventChoiceLocked = false;
}
