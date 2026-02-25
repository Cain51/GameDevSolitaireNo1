// 游戏结束模块
// 处理游戏结束逻辑和显示

const JOURNEY_HISTORY_KEY = "chinese_truck_adventure_journey_history";
const JOURNEY_HISTORY_MAX = 30;

// 页面淡出后跳转回主菜单
function fadeOutAndGo() {
  document.body.style.transition = 'opacity 0.5s ease-out';
  document.body.style.opacity = '0';
  setTimeout(() => { window.location.href = 'index.html'; }, 500);
}

function showGameOver(endingType) {
  // 保存本次旅途到历史历程
  try {
    const cfg =
      typeof ENDINGS_CONFIG !== "undefined" && ENDINGS_CONFIG[endingType]
        ? ENDINGS_CONFIG[endingType]
        : ENDINGS_CONFIG["game_over_event"];
    const entry = {
      endingType,
      endingTitle: cfg ? cfg.title : endingType,
      mileage: Math.floor((gameState && gameState.mileage) || 0),
      passengers: [...(gameState && gameState.passengersEverOnBoard) || []],
      sessionAchievements: [...(gameState && gameState.sessionAchievements) || []],
      timestamp: Date.now(),
    };
    let history = [];
    const saved = localStorage.getItem(JOURNEY_HISTORY_KEY);
    if (saved) {
      try {
        history = JSON.parse(saved);
      } catch (e) {}
    }
    history.unshift(entry);
    if (history.length > JOURNEY_HISTORY_MAX) history.length = JOURNEY_HISTORY_MAX;
    localStorage.setItem(JOURNEY_HISTORY_KEY, JSON.stringify(history));
  } catch (e) {}

  // 记录达成的结局
  if (typeof gameState !== "undefined") {
    if (!Array.isArray(gameState.achievedEndings)) {
      gameState.achievedEndings = [];
    }
    if (!gameState.achievedEndings.includes(endingType)) {
      gameState.achievedEndings.push(endingType);
    }
    // 保存结局数据到 localStorage（跨档保留）
    try {
      localStorage.setItem("chinese_truck_adventure_endings", JSON.stringify(gameState.achievedEndings));
    } catch (e) {}
  }

  // 更新行驶里程记录表（本次里程若破纪录则写入历史最高）
  if (typeof updateBestMileageIfNeeded === "function" && typeof gameState !== "undefined") {
    updateBestMileageIfNeeded(gameState.mileage);
  }

  // 最后检查一次成就
  if (typeof checkAndUnlockAchievements === "function") {
    checkAndUnlockAchievements();
  }

  // 关闭事件弹窗
  const modal = document.getElementById("event-modal");
  if (modal) modal.remove();

  // 清除所有视觉特效（珍品特效、事件主题等）
  if (typeof clearAllEffects === "function") clearAllEffects();

  // 删除存档，使"重新开始"（页面刷新）时走新游戏流程
  if (typeof deleteSave === "function") deleteSave();

  // 停止游戏
  pauseTextGeneration();
  pauseRoad();

  // 获取结局配置
  const cfg =
    typeof ENDINGS_CONFIG !== "undefined" && ENDINGS_CONFIG[endingType]
      ? ENDINGS_CONFIG[endingType]
      : ENDINGS_CONFIG["game_over_event"];

  const b = [];
  b.push(
    '<div class="ending-modal-inner text-area-scroll rounded-2xl p-8 max-w-2xl w-full mx-4 text-center overflow-y-auto max-h-[90vh]"',
  );
  // 外发光 box-shadow 跟随结局主题色
  const glowColor = cfg.borderColor || "#c41e3a";
  b.push(
    ' style="background:' +
      cfg.bgColor +
      ";border:2px solid " +
      cfg.borderColor +
      ";box-shadow:0 0 60px " + glowColor + "40,0 0 120px " + glowColor + "18;" +
      '">',
  );
  b.push(
    '<h2 class="text-3xl font-bold mb-3" style="color:' +
      cfg.borderColor +
      ';">' +
      cfg.title +
      "</h2>",
  );
  b.push('<p class="text-gray-200 text-lg mb-3">' + cfg.message + "</p>");
  b.push(
    '<p class="text-gray-400 text-sm mb-4 whitespace-pre-line">' +
      cfg.flavor +
      "</p>",
  );
  b.push(
    '<p class="text-gray-500 text-xs mb-6">行驶里程：' +
      gameState.mileage +
      " km</p>",
  );

  // 根据结局类型显示个性化信息
  const passengers = truckState.passengers || [];
  const passengerFavor = gameState.passengerFavor || {};

  // 友谊永恒结局：显示乘客告别话语
  if (endingType === "eternal_friendship" && passengers.length > 0) {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#a78bfa] mb-3">乘客的告别</h3>');
    b.push('<div class="space-y-2 text-left">');
    passengers.forEach((name) => {
      const favor = passengerFavor[name] || 50;
      const cfg = typeof PASSENGER_CONFIG !== "undefined" && PASSENGER_CONFIG[name];
      const color = cfg && cfg.color ? cfg.color : "#94a3b8";
      let farewell = "";
      if (favor >= 90) {
        farewell = `"这段旅程是我人生中最美好的回忆之一。谢谢你，我的朋友！"`;
      } else if (favor >= 80) {
        farewell = `"和你一起的这段路，让我收获了很多。保重！"`;
      } else {
        farewell = `"感谢你的帮助，祝你好运！"`;
      }
      b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700">`);
      b.push(`<span class="font-bold" style="color:${color};">${name}</span>`);
      b.push(`<span class="text-gray-300 text-sm ml-2">${farewell}</span>`);
      b.push(`</div>`);
    });
    b.push('</div></div>');
  }

  // 完美旅程结局：显示属性统计
  if (endingType === "perfect_journey") {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#fbbf24] mb-3">完美平衡</h3>');
    b.push('<div class="grid grid-cols-3 gap-2 text-center">');
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700">`);
    b.push(`<div class="text-xs text-gray-400 mb-1">燃油</div>`);
    b.push(`<div class="text-lg font-bold text-[#f59e0b]">${Math.round(truckState.fuel)}%</div>`);
    b.push(`</div>`);
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700">`);
    b.push(`<div class="text-xs text-gray-400 mb-1">耐久</div>`);
    b.push(`<div class="text-lg font-bold text-[#6b7280]">${Math.round(truckState.durability)}%</div>`);
    b.push(`</div>`);
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700">`);
    b.push(`<div class="text-xs text-gray-400 mb-1">舒适</div>`);
    b.push(`<div class="text-lg font-bold text-[#ec4899]">${Math.round(truckState.comfort)}%</div>`);
    b.push(`</div>`);
    b.push('</div></div>');
  }

  // 传奇司机结局：显示挑战统计
  if (endingType === "legendary_driver") {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#ef4444] mb-3">传奇之路</h3>');
    b.push('<div class="text-sm text-gray-300 space-y-1">');
    b.push(`<div>总里程：<span class="font-bold text-white">${mileage} km</span></div>`);
    if (passengers.length > 0) {
      b.push(`<div>同行乘客：<span class="font-bold text-white">${passengers.length} 位</span></div>`);
    }
    b.push(`<div>当前属性：燃油 ${Math.round(truckState.fuel)}% | 耐久 ${Math.round(truckState.durability)}% | 舒适 ${Math.round(truckState.comfort)}%</div>`);
    b.push('</div></div>');
  }

  // 旅途终点结局：ASCII CG + 属性剩余 + 同行乘客 + 贡献好感度
  if (endingType === "journey_end") {
    // ASCII 艺术 CG
    b.push('<div class="journey-end-ascii-wrap" style="margin:8px 0 20px;padding:0;">');
    b.push('<pre class="journey-end-ascii">');
    b.push([
      '        ·  ✦  ·    ·  ✦  ·    ·  ✦  ·        ',
      '   ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░   ',
      '   ░                                        ░   ',
      '   ░    ════════════════════════════════    ░   ',
      '   ░    ║  ·  ·  ·  路的终点  ·  ·  ·  ║    ░   ',
      '   ░    ════════════════════════════════    ░   ',
      '   ░          ██▄                           ░   ',
      '   ░    ─────██████────────────────────     ░   ',
      '   ░        ██████▌▌▌▌▌  >>>  · · · >>     ░   ',
      '   ░    ───┸◉──◉┸────────────────────────  ░   ',
      '   ░                                        ░   ',
      '   ░    ════════════════════════════════    ░   ',
      '   ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░   ',
      '        ·  ·  T H E   R O A D   E N D S  ·  · ',
    ].join('\n'));
    b.push('</pre></div>');

    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#a78bfa] mb-3">抵达终点时的状态</h3>');
    b.push('<div class="grid grid-cols-3 gap-2 text-center mb-3">');
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700"><div class="text-xs text-gray-400 mb-1">燃油</div><div class="text-lg font-bold text-[#f59e0b]">${Math.round(truckState.fuel)}%</div></div>`);
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700"><div class="text-xs text-gray-400 mb-1">耐久</div><div class="text-lg font-bold text-[#6b7280]">${Math.round(truckState.durability)}%</div></div>`);
    b.push(`<div class="p-2 bg-gray-800/50 rounded border border-gray-700"><div class="text-xs text-gray-400 mb-1">舒适</div><div class="text-lg font-bold text-[#ec4899]">${Math.round(truckState.comfort)}%</div></div>`);
    b.push('</div>');
    if (passengers.length > 0) {
      b.push('<div class="text-sm text-gray-400 mb-2">同行乘客为你减轻了终点的代价：</div>');
      b.push('<div class="space-y-1">');
      passengers.forEach((name) => {
        const favor = Math.round(passengerFavor[name] || 50);
        const cfg = typeof PASSENGER_CONFIG !== "undefined" && PASSENGER_CONFIG[name];
        const color = cfg && cfg.color ? cfg.color : "#94a3b8";
        const barW = Math.round(favor);
        b.push(`<div class="flex items-center gap-2">`);
        b.push(`<span class="text-xs w-16 flex-shrink-0" style="color:${color};">${name}</span>`);
        b.push(`<div class="flex-1 bg-gray-800 rounded-full h-2 overflow-hidden"><div class="h-full rounded-full bg-[#a78bfa]" style="width:${barW}%"></div></div>`);
        b.push(`<span class="text-xs text-gray-400 w-8 text-right">${favor}</span>`);
        b.push(`</div>`);
      });
      b.push('</div>');
    } else {
      b.push('<div class="text-sm text-gray-500 italic">你独自一人走到了这里。没有人分担，也没有人减轻。</div>');
    }
    b.push('</div>');
  }

  // 和谐共存结局：显示鹿和猎人的特殊信息
  if (endingType === "harmony") {
    const deerFavor = passengerFavor["鹿"] || 0;
    const hunterFavor = passengerFavor["猎人"] || 0;
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#10b981] mb-3">和解的见证</h3>');
    b.push('<div class="text-sm text-gray-300 space-y-2">');
    b.push(`<div>鹿的好感度：<span class="font-bold text-[#d4a574]">${Math.round(deerFavor)}</span></div>`);
    b.push(`<div>猎人的好感度：<span class="font-bold text-[#8b7355]">${Math.round(hunterFavor)}</span></div>`);
    b.push(`<div class="text-xs text-gray-400 mt-2">他们在这段旅程中学会了理解与包容。</div>`);
    b.push('</div></div>');
  }

  // 收集者结局：显示收集到的乘客类型
  if (endingType === "collector" && passengersEverOnBoard.length > 0) {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#34d399] mb-3">收集的回忆</h3>');
    b.push('<div class="text-sm text-gray-300">');
    b.push(`<div>你遇见了 <span class="font-bold text-white">${passengersEverOnBoard.length}</span> 位不同的乘客：</div>`);
    b.push('</div></div>');
  }

  // 孤独行者结局：显示独自前行的统计
  if (endingType === "lonely_wanderer") {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#64748b] mb-3">独自前行</h3>');
    b.push('<div class="text-sm text-gray-300 space-y-1">');
    b.push(`<div>独自行驶里程：<span class="font-bold text-white">${mileage} km</span></div>`);
    b.push(`<div>当前属性：燃油 ${Math.round(truckState.fuel)}% | 耐久 ${Math.round(truckState.durability)}% | 舒适 ${Math.round(truckState.comfort)}%</div>`);
    b.push(`<div class="text-xs text-gray-400 mt-2">在这段孤独的旅程中，你找到了真正的自己。</div>`);
    b.push('</div></div>');
  }
  
  // 显示本轮达成的成就
  const sessionAchievements = gameState.sessionAchievements || [];
  if (sessionAchievements.length > 0 && typeof ACHIEVEMENTS_CONFIG !== "undefined") {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#c41e3a] mb-3">本轮达成的成就</h3>');
    b.push('<div class="grid grid-cols-2 gap-2 text-left">');
    sessionAchievements.forEach((achId) => {
      const ach = ACHIEVEMENTS_CONFIG[achId];
      if (ach) {
        b.push(`<div class="flex items-center gap-2 p-2 bg-gray-800/50 rounded border border-gray-700">`);
        b.push(`<span class="text-2xl">${ach.icon}</span>`);
        b.push(`<div class="flex-1 min-w-0">`);
        b.push(`<div class="text-sm font-bold text-white">${ach.title}</div>`);
        b.push(`<div class="text-xs text-gray-400">${ach.description}</div>`);
        b.push(`</div></div>`);
      }
    });
    b.push('</div></div>');
  }
  
  // 显示上过车的乘客
  const passengersEverOnBoard = gameState.passengersEverOnBoard || [];
  if (passengersEverOnBoard.length > 0) {
    b.push('<div class="border-t border-gray-700 pt-4 mt-4 mb-4">');
    b.push('<h3 class="text-lg font-bold text-[#c41e3a] mb-3">上过车的乘客</h3>');
    b.push('<div class="flex flex-wrap gap-2 justify-center">');
    passengersEverOnBoard.forEach((name) => {
      const cfg = typeof PASSENGER_CONFIG !== "undefined" && PASSENGER_CONFIG[name];
      const color = cfg && cfg.color ? cfg.color : "#94a3b8";
      b.push(`<span class="px-3 py-1 rounded-lg bg-gray-800/50 border border-gray-700 text-sm" style="color:${color};">${name}</span>`);
    });
    b.push('</div></div>');
  }
  
  // 好结局：显示"继续无尽模式"按钮；死亡/失败结局：只显示重新开始
  const GOOD_ENDINGS_ENDLESS = {
    perfect_journey:   { color: "#fbbf24", hoverColor: "#fcd34d", textColor: "text-gray-900", label: "✦ 继续无尽模式",       msg: '<span style="color:#fbbf24;">✦ 你选择了继续前行，开启无尽模式！道路没有尽头，旅途永不止息。</span>' },
    journey_end:       { color: "#a78bfa", hoverColor: "#c4b5fd", textColor: "text-gray-900", label: "✦ 继续前行（无尽模式）", msg: '<span style="color:#a78bfa;">✦ 路牌倒下了，地图失效了——但你还在开。无尽模式开启，终点从未存在。</span>' },
    eternal_friendship:{ color: "#a78bfa", hoverColor: "#c4b5fd", textColor: "text-gray-900", label: "✦ 与朋友们继续前行",    msg: '<span style="color:#a78bfa;">✦ 友谊没有终点。你们决定继续这段旅程，无尽模式开启。</span>' },
    collector:         { color: "#34d399", hoverColor: "#6ee7b7", textColor: "text-gray-900", label: "✦ 继续收集故事",        msg: '<span style="color:#34d399;">✦ 还有更多面孔等待相遇。收集者的旅途，永不停歇。</span>' },
    legendary_driver:  { color: "#ef4444", hoverColor: "#f87171", textColor: "text-white",    label: "✦ 续写传奇",           msg: '<span style="color:#ef4444;">✦ 传说不会就此终结。你踩下油门，无尽模式开启。</span>' },
    lonely_wanderer:   { color: "#64748b", hoverColor: "#94a3b8", textColor: "text-white",    label: "✦ 继续独自流浪",       msg: '<span style="color:#94a3b8;">✦ 孤独是你的伴侣。你重新上路，无尽模式开启。</span>' },
    harmony:           { color: "#10b981", hoverColor: "#34d399", textColor: "text-gray-900", label: "✦ 和谐共存，继续前行", msg: '<span style="color:#10b981;">✦ 和解的旅程没有终点。你们一起继续上路，无尽模式开启。</span>' },
    overloaded:        { color: "#f59e0b", hoverColor: "#fbbf24", textColor: "text-gray-900", label: "✦ 继续装满后备箱",     msg: '<span style="color:#f59e0b;">✦ 装不下了？再试试。无尽模式开启，物资永远装不够。</span>' },
    clown_night:       { color: "#ff00ff", hoverColor: "#ff66ff", textColor: "text-white",    label: "✦ 加入小丑马戏团",     msg: '<span style="color:#ff00ff;">✦ 🎪 你戴上了红鼻子，无尽模式开启。欢迎加入。</span>' },
  };

  const endlessInfo = GOOD_ENDINGS_ENDLESS[endingType];
  if (endlessInfo) {
    b.push('<div class="flex flex-col sm:flex-row gap-3 justify-center">');
    b.push(`<button onclick="continueEndlessMode('${endingType}')" class="px-8 py-3 rounded-full font-bold transition-all ${endlessInfo.textColor}" style="background:${endlessInfo.color};">${endlessInfo.label}</button>`);
    b.push('<button onclick="fadeOutAndGo()" class="px-8 py-3 bg-[#374151] text-white rounded-full hover:bg-[#4b5563] transition-all">重新开始</button>');
    b.push('</div>');
  } else {
    b.push(
      '<button onclick="fadeOutAndGo()" class="px-8 py-3 bg-[#c41e3a] text-white rounded-full hover:bg-[#e63950] transition-all">重新开始</button>',
    );
  }
  b.push('<button onclick="openJourneyLog()" style="margin-top:10px;display:block;width:100%;padding:9px 0;background:transparent;border:1px solid #374151;border-radius:9999px;color:#9ca3af;font-size:0.82rem;cursor:pointer;transition:all 0.2s;" onmouseenter="this.style.borderColor=\'#c41e3a\';this.style.color=\'#c41e3a\';" onmouseleave="this.style.borderColor=\'#374151\';this.style.color=\'#9ca3af\';">📜 查看旅途历程</button>');

  // 好结局时显示鸣谢按钮
  const GOOD_ENDINGS = ["perfect_journey", "eternal_friendship", "collector", "legendary_driver", "lonely_wanderer", "harmony", "overloaded", "journey_end", "clown_night"];
  if (GOOD_ENDINGS.includes(endingType)) {
    b.push('<button onclick="openCreditsModal()" style="margin-top:8px;display:block;width:100%;padding:9px 0;background:transparent;border:1px solid #2d2d4a;border-radius:9999px;color:#7c6fa0;font-size:0.78rem;cursor:pointer;transition:all 0.2s;" onmouseenter="this.style.borderColor=\'#a78bfa\';this.style.color=\'#a78bfa\';" onmouseleave="this.style.borderColor=\'#2d2d4a\';this.style.color=\'#7c6fa0\';">🎖️ 鸣谢名单</button>');
  }

  b.push("</div>");

  const gameOverModal = document.createElement("div");
  gameOverModal.id = "game-over-modal";
  gameOverModal.className =
    "fixed inset-0 bg-black/90 flex items-center justify-center z-50 text-area-scroll";
  gameOverModal.innerHTML = b.join("");
  document.body.appendChild(gameOverModal);

  // 终点结局：在全屏遮罩上叠加粒子背景特效
  if (endingType === "journey_end") {
    _spawnJourneyEndParticles(gameOverModal);
  }
}

// 通用：好结局后继续无尽模式
function continueEndlessMode(endingType) {
  // 所有好结局共用同一个无尽模式标志，阻止重复触发已达成的结局
  gameState.perfectJourneyEndlessMode = true;
  // 记录是从哪个结局进入无尽模式的（可用于后续扩展）
  gameState.endlessFromEnding = endingType || "unknown";

  const modal = document.getElementById("game-over-modal");
  if (modal) modal.remove();
  resumeGame();
  if (typeof saveGame === "function") saveGame();

  // 各结局专属入场提示
  const ENDLESS_MSGS = {
    perfect_journey:    '<span style="color:#fbbf24;">✦ 你选择了继续前行，开启无尽模式！道路没有尽头，旅途永不止息。</span>',
    journey_end:        '<span style="color:#a78bfa;">✦ 路牌倒下了，地图失效了——但你还在开。无尽模式开启，终点从未存在。</span>',
    eternal_friendship: '<span style="color:#a78bfa;">✦ 友谊没有终点。你们决定继续这段旅程，无尽模式开启。</span>',
    collector:          '<span style="color:#34d399;">✦ 还有更多面孔等待相遇。收集者的旅途，永不停歇。</span>',
    legendary_driver:   '<span style="color:#ef4444;">✦ 传说不会就此终结。你踩下油门，无尽模式开启。</span>',
    lonely_wanderer:    '<span style="color:#94a3b8;">✦ 孤独是你的伴侣。你重新上路，无尽模式开启。</span>',
    harmony:            '<span style="color:#10b981;">✦ 和解的旅程没有终点。你们一起继续上路，无尽模式开启。</span>',
    overloaded:         '<span style="color:#f59e0b;">✦ 装不下了？再试试。无尽模式开启，物资永远装不够。</span>',
    clown_night:        '<span style="color:#ff00ff;">✦ 🎪 你戴上了红鼻子，无尽模式开启。欢迎加入。</span>',
  };
  const msg = ENDLESS_MSGS[endingType] || '<span style="color:#c41e3a;">✦ 无尽模式开启，道路继续延伸。</span>';

  const textArea = document.getElementById("textArea");
  if (textArea) {
    const p = document.createElement("p");
    p.innerHTML = msg;
    textArea.appendChild(p);
    if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
  }
}

// 向后兼容旧调用（保留以防其他地方引用）
function continueFromPerfectJourney() { continueEndlessMode("perfect_journey"); }
function continueFromJourneyEnd()     { continueEndlessMode("journey_end"); }

// 检查特殊结局条件（在常规失败检查之前调用）
// 按优先级顺序检查，返回满足条件的结局类型，否则返回 null
function checkSpecialEndings() {
  // 特殊结局不应该在属性归零时触发（那是失败结局）
  if (truckState.fuel <= 0 || truckState.durability <= 0 || truckState.comfort <= 0) {
    return null;
  }

  const mileage = gameState.mileage || 0;
  const passengers = truckState.passengers || [];
  const passengerFavor = gameState.passengerFavor || {};
  const passengersEverOnBoard = gameState.passengersEverOnBoard || [];

  // 1. 完美旅程结局（优先级最高）
  // 里程 ≥ 300 km，所有属性 ≥ 50%，至少有一个乘客
  // 若已选择继续无尽模式则不再触发
  if (!gameState.perfectJourneyEndlessMode &&
    mileage >= 300 &&
    truckState.fuel >= 50 &&
    truckState.durability >= 50 &&
    truckState.comfort >= 50 &&
    passengers.length > 0
  ) {
    return "perfect_journey";
  }

  // 2. 传奇司机结局
  // 里程 ≥ 500 km，所有属性 ≥ 30%
  if (
    mileage >= 500 &&
    truckState.fuel >= 30 &&
    truckState.durability >= 30 &&
    truckState.comfort >= 30
  ) {
    return "legendary_driver";
  }

  // 3. 友谊永恒结局
  // 车上至少有3个乘客，所有乘客好感度 ≥ 80，里程 ≥ 100 km
  if (passengers.length >= 3 && mileage >= 100) {
    const allFavorHigh = passengers.every((name) => {
      const favor = passengerFavor[name];
      return typeof favor === "number" && favor >= 80;
    });
    if (allFavorHigh) {
      return "eternal_friendship";
    }
  }

  // 4. 收集者结局
  // 曾经上过车的乘客包含所有7种类型，里程 ≥ 150 km
  const allPassengerTypes = ["鹿", "猎人", "骚福瑞", "旅行者", "年迈妇人", "猫", "流浪艺人"];
  if (mileage >= 150) {
    const hasAllTypes = allPassengerTypes.every((name) =>
      passengersEverOnBoard.includes(name)
    );
    if (hasAllTypes) {
      return "collector";
    }
  }

  // 5. 和谐共存结局
  // 鹿和猎人同时在车上，两者好感度都 ≥ 70，里程 ≥ 80 km
  if (
    passengers.includes("鹿") &&
    passengers.includes("猎人") &&
    mileage >= 80
  ) {
    const deerFavor = passengerFavor["鹿"];
    const hunterFavor = passengerFavor["猎人"];
    if (
      typeof deerFavor === "number" &&
      deerFavor >= 70 &&
      typeof hunterFavor === "number" &&
      hunterFavor >= 70
    ) {
      return "harmony";
    }
  }

  // 6. 孤独行者结局（优先级最低）
  // 里程 ≥ 200 km，车上没有任何乘客，所有属性 ≥ 40%
  if (
    mileage >= 200 &&
    passengers.length === 0 &&
    truckState.fuel >= 40 &&
    truckState.durability >= 40 &&
    truckState.comfort >= 40
  ) {
    return "lonely_wanderer";
  }

  return null;
}

// 检查游戏结束条件（在任何皮卡属性更新后调用）
function checkGameOverConditions() {
  // 先检查特殊结局（在属性归零之前）
  const specialEnding = checkSpecialEndings();
  if (specialEnding) {
    showGameOver(specialEnding);
    return true;
  }

  // 然后检查常规失败条件
  if (truckState.fuel <= 0) {
    showGameOver("fuel_empty");
    return true;
  }
  if (truckState.durability <= 0) {
    showGameOver("durability_zero");
    return true;
  }
  if (truckState.comfort <= 0) {
    showGameOver("comfort_zero");
    return true;
  }
  return false;
}

// ─── 鸣谢界面 ───────────────────────────────────────────────────────
function openCreditsModal() {
  // 防止重复
  const existing = document.getElementById("credits-modal");
  if (existing) return;

  const el = document.createElement("div");
  el.id = "credits-modal";
  el.style.cssText = [
    "position:fixed", "inset:0", "z-index:9999",
    "display:flex", "align-items:center", "justify-content:center",
    "background:rgba(0,0,0,0.75)", "backdrop-filter:blur(4px)",
  ].join(";");
  el.innerHTML = `
    <div style="
      background:#0d0a1a;
      border:2px solid #a78bfa;
      border-radius:20px;
      padding:36px 40px 32px;
      max-width:min(480px, 94vw);
      width:calc(100% - 32px);
      text-align:center;
      box-shadow:0 0 60px #a78bfa40,0 0 120px #a78bfa18;
      animation:creditsModalIn 0.4s cubic-bezier(0.22,1,0.36,1) both;
      position:relative;
      max-height:90vh;
      overflow-y:auto;
    ">
      <!-- 关闭按钮 -->
      <button onclick="closeCreditsModal()"
        style="position:absolute;top:14px;right:18px;background:transparent;border:none;color:#7c6fa0;font-size:1.4rem;cursor:pointer;line-height:1;transition:color 0.2s;"
        onmouseenter="this.style.color='#e2d9f3'" onmouseleave="this.style.color='#7c6fa0'">&times;</button>

      <!-- 标题 -->
      <div style="font-family:'Courier New',monospace;color:#a78bfa;font-size:0.65rem;letter-spacing:0.15em;margin-bottom:6px;opacity:0.7;">
        ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
      </div>
      <h2 style="color:#c4b5fd;font-size:1.05rem;font-weight:900;letter-spacing:0.08em;margin-bottom:2px;">
        🏆 第 0 届独游指针接龙大赛现场
      </h2>
      <div style="font-family:'Courier New',monospace;color:#a78bfa;font-size:0.65rem;letter-spacing:0.15em;margin-top:6px;margin-bottom:20px;opacity:0.7;">
        ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
      </div>

      <!-- 游戏名 -->
      <div style="background:rgba(167,139,250,0.08);border:1px solid #a78bfa30;border-radius:10px;padding:12px 16px;margin-bottom:20px;">
        <div style="color:#e2d9f3;font-size:0.92rem;font-weight:700;margin-bottom:4px;">
          🚗 无人红色皮卡的安保之旅
        </div>
        <div style="color:#7c6fa0;font-size:0.72rem;font-style:italic;">
          （这名字真的好怪）
        </div>
      </div>

      <!-- 接龙名单 -->
      <div style="text-align:left;space-y:8px;">
        <div style="color:#9d8ec7;font-size:0.75rem;font-weight:700;letter-spacing:0.1em;margin-bottom:10px;text-align:center;">
          ── 接 龙 名 单 ──
        </div>

        <div style="display:flex;align-items:center;gap:10px;padding:8px 12px;border-radius:8px;background:rgba(167,139,250,0.06);margin-bottom:6px;">
          <span style="color:#a78bfa;font-size:0.78rem;font-weight:700;min-width:52px;flex-shrink:0;">第一棒</span>
          <span style="color:#c4b5fd;font-size:0.88rem;font-weight:600;">路过的红色皮卡</span>
        </div>
        <div style="display:flex;align-items:center;gap:10px;padding:8px 12px;border-radius:8px;background:rgba(167,139,250,0.06);margin-bottom:6px;">
          <span style="color:#a78bfa;font-size:0.78rem;font-weight:700;min-width:52px;flex-shrink:0;">第二棒</span>
          <span style="color:#c4b5fd;font-size:0.88rem;font-weight:600;">猫猫D菌</span>
        </div>
        <div style="display:flex;align-items:center;gap:10px;padding:8px 12px;border-radius:8px;background:rgba(167,139,250,0.06);margin-bottom:6px;">
          <span style="color:#a78bfa;font-size:0.78rem;font-weight:700;min-width:52px;flex-shrink:0;">第三棒</span>
          <span style="color:#c4b5fd;font-size:0.88rem;font-weight:600;">尘聲</span>
        </div>
        <div style="display:flex;align-items:center;gap:10px;padding:8px 12px;border-radius:8px;background:rgba(167,139,250,0.06);margin-bottom:14px;">
          <span style="color:#a78bfa;font-size:0.78rem;font-weight:700;min-width:52px;flex-shrink:0;">第四棒</span>
          <span style="color:#c4b5fd;font-size:0.88rem;font-weight:600;">龙QAVC</span>
        </div>

        <!-- 特别感谢 -->
        <div style="border-top:1px solid #a78bfa25;padding-top:12px;text-align:center;">
          <div style="color:#9d8ec7;font-size:0.72rem;letter-spacing:0.1em;margin-bottom:8px;">── 最 后 感 谢 ──</div>
          <div style="display:inline-flex;align-items:center;gap:8px;padding:8px 18px;border-radius:9999px;background:rgba(167,139,250,0.1);border:1px solid #a78bfa40;">
            <span style="font-size:1.1rem;">🤖</span>
            <span style="color:#e2d9f3;font-size:0.88rem;font-weight:600;letter-spacing:0.04em;">Claude Sonnet 4.6</span>
          </div>
        </div>
      </div>

      <!-- 关闭按钮（底部） -->
      <button onclick="closeCreditsModal()"
        style="margin-top:20px;padding:9px 36px;background:transparent;border:1px solid #a78bfa50;border-radius:9999px;color:#9d8ec7;font-size:0.82rem;cursor:pointer;transition:all 0.2s;"
        onmouseenter="this.style.borderColor='#a78bfa';this.style.color='#c4b5fd';"
        onmouseleave="this.style.borderColor='#a78bfa50';this.style.color='#9d8ec7';">
        关闭
      </button>
    </div>
  `;

  // 点击遮罩关闭
  el.addEventListener("click", (e) => {
    if (e.target === el) closeCreditsModal();
  });

  document.body.appendChild(el);
}

// ─── 终点结局粒子背景特效 ──────────────────────────────────────
// 在结局全屏遮罩上叠加：
//  · 流动的光道粒子（公路消逝感）
//  · 缓慢漂浮的星光尘埃
//  · 底部渐变光晕（紫色地平线）
function _spawnJourneyEndParticles(container) {
  // ── Canvas 层 ──
  const canvas = document.createElement("canvas");
  canvas.style.cssText = [
    "position:absolute", "inset:0", "width:100%", "height:100%",
    "pointer-events:none", "z-index:0", "border-radius:0",
  ].join(";");
  // 插到最前，内容卡片在上面
  container.insertBefore(canvas, container.firstChild);

  let W, H;
  const resize = () => {
    W = canvas.width  = container.offsetWidth  || window.innerWidth;
    H = canvas.height = container.offsetHeight || window.innerHeight;
  };
  resize();
  window.addEventListener("resize", resize);

  const ctx2d = canvas.getContext("2d");

  // ── 粒子定义 ──
  const ROAD_PARTICLES  = 55;  // 沿公路光道飞逝的粒子
  const DUST_PARTICLES  = 40;  // 漂浮星尘

  const roadPts = [];
  const dustPts = [];

  function randRoadPt() {
    // 从画面中心水平线散开，向两侧飞逝（透视消逝感）
    const progress = Math.random(); // 0=近, 1=远
    return {
      life: Math.random(),
      speed: 0.004 + Math.random() * 0.008,
      // 近端在底部中央，远端在顶部中央
      baseX: 0.5,
      spread: 0.08 + Math.random() * 0.38, // 横向展开量
      side: Math.random() < 0.5 ? -1 : 1,
      alpha: 0.3 + Math.random() * 0.5,
      len: 0.02 + Math.random() * 0.06,
      color: Math.random() < 0.6 ? "#a78bfa" : (Math.random() < 0.5 ? "#c4b5fd" : "#e2d9f3"),
    };
  }
  function randDustPt() {
    return {
      x: Math.random(),
      y: Math.random(),
      r: 0.5 + Math.random() * 1.5,
      dx: (Math.random() - 0.5) * 0.00015,
      dy: -0.00008 - Math.random() * 0.00025,
      alpha: 0.1 + Math.random() * 0.6,
      pulse: Math.random() * Math.PI * 2,
      pulseSpeed: 0.008 + Math.random() * 0.02,
      color: Math.random() < 0.5 ? "#c4b5fd" : "#f0e6ff",
    };
  }
  for (let i = 0; i < ROAD_PARTICLES; i++) {
    const p = randRoadPt();
    p.life = Math.random(); // 初始散布
    roadPts.push(p);
  }
  for (let i = 0; i < DUST_PARTICLES; i++) dustPts.push(randDustPt());

  let alive = true;
  // 结局弹窗关闭时停止动画
  const observer = new MutationObserver(() => {
    if (!document.contains(container)) { alive = false; observer.disconnect(); }
  });
  observer.observe(document.body, { childList: true });

  function draw() {
    if (!alive) return;
    requestAnimationFrame(draw);
    ctx2d.clearRect(0, 0, W, H);

    // ── 底部地平线光晕（渐变紫霾） ──
    const grd = ctx2d.createRadialGradient(W * 0.5, H * 0.85, 0, W * 0.5, H * 0.85, W * 0.65);
    grd.addColorStop(0,   "rgba(120,80,200,0.18)");
    grd.addColorStop(0.5, "rgba(80,30,140,0.10)");
    grd.addColorStop(1,   "rgba(0,0,0,0)");
    ctx2d.fillStyle = grd;
    ctx2d.fillRect(0, 0, W, H);

    // ── 公路消逝光束 ──
    for (const p of roadPts) {
      p.life += p.speed;
      if (p.life > 1) {
        Object.assign(p, randRoadPt());
        p.life = 0;
        continue;
      }
      // 透视坐标：近=底部中央，远=顶部中央，随 life 推进
      const yRatio = 1 - p.life;        // 0(远/顶)→1(近/底)
      const x1 = W * (0.5 + p.side * p.spread * p.life);
      const y1 = H * (0.42 + yRatio * 0.40);
      const tailLen = W * p.len * p.life;
      const x0 = x1 - p.side * tailLen;

      ctx2d.save();
      ctx2d.globalAlpha = p.alpha * (p.life < 0.15 ? p.life / 0.15 : p.life > 0.85 ? (1 - p.life) / 0.15 : 1);
      ctx2d.strokeStyle = p.color;
      ctx2d.lineWidth = 0.5 + p.life * 1.5;
      ctx2d.beginPath();
      ctx2d.moveTo(x0, y1);
      ctx2d.lineTo(x1, y1);
      ctx2d.stroke();
      ctx2d.restore();
    }

    // ── 漂浮星尘 ──
    for (const p of dustPts) {
      p.x += p.dx;
      p.y += p.dy;
      p.pulse += p.pulseSpeed;
      if (p.y < -0.02) p.y = 1.02;
      if (p.x < -0.02 || p.x > 1.02) p.dx *= -1;
      const pulseFactor = 0.7 + 0.3 * Math.sin(p.pulse);
      ctx2d.save();
      ctx2d.globalAlpha = p.alpha * pulseFactor;
      ctx2d.fillStyle = p.color;
      ctx2d.beginPath();
      ctx2d.arc(p.x * W, p.y * H, p.r * pulseFactor, 0, Math.PI * 2);
      ctx2d.fill();
      ctx2d.restore();
    }
  }
  draw();
}

// ─── 终点事件弹窗背景特效 ─────────────────────────────────────
// 在 journey_end_question 事件弹窗出现时注入：
//  · 弹窗遮罩上的"三叉路光晕"
//  · 页面整体添加渐入的紫色染色滤镜
let _journeyEndEventEffect = null;

function startJourneyEndEventEffect() {
  if (_journeyEndEventEffect) return;

  // 给 body 加渐变色调（紫色氛围渗入）
  const veil = document.createElement("div");
  veil.id = "journey-end-veil";
  veil.style.cssText = [
    "position:fixed", "inset:0", "pointer-events:none", "z-index:48",
    "background:radial-gradient(ellipse at 50% 90%, rgba(100,50,180,0.22) 0%, rgba(40,0,80,0.12) 55%, transparent 100%)",
    "opacity:0", "transition:opacity 1.2s ease",
  ].join(";");
  document.body.appendChild(veil);
  requestAnimationFrame(() => { veil.style.opacity = "1"; });

  // 三条发散光道（暗示三条路）
  const beams = document.createElement("div");
  beams.id = "journey-end-beams";
  beams.style.cssText = [
    "position:fixed", "inset:0", "pointer-events:none", "z-index:48", "overflow:hidden",
  ].join(";");
  beams.innerHTML = `
    <div style="
      position:absolute;bottom:0;left:50%;
      transform:translateX(-50%) rotate(-22deg);
      transform-origin:bottom center;
      width:3px;height:60vh;
      background:linear-gradient(to top,rgba(167,139,250,0.35),transparent);
      filter:blur(6px);animation:jendBeam 3.5s ease-in-out infinite alternate;
    "></div>
    <div style="
      position:absolute;bottom:0;left:50%;
      transform:translateX(-50%) rotate(0deg);
      transform-origin:bottom center;
      width:4px;height:72vh;
      background:linear-gradient(to top,rgba(196,181,253,0.45),transparent);
      filter:blur(4px);animation:jendBeam 4.2s ease-in-out 0.4s infinite alternate;
    "></div>
    <div style="
      position:absolute;bottom:0;left:50%;
      transform:translateX(-50%) rotate(22deg);
      transform-origin:bottom center;
      width:3px;height:60vh;
      background:linear-gradient(to top,rgba(167,139,250,0.35),transparent);
      filter:blur(6px);animation:jendBeam 3.8s ease-in-out 0.8s infinite alternate;
    "></div>`;
  document.body.appendChild(beams);

  _journeyEndEventEffect = { veil, beams };
}

function stopJourneyEndEventEffect() {
  if (!_journeyEndEventEffect) return;
  const { veil, beams } = _journeyEndEventEffect;
  veil.style.opacity = "0";
  setTimeout(() => { veil.remove(); beams.remove(); }, 1300);
  _journeyEndEventEffect = null;
}

function closeCreditsModal() {
  const el = document.getElementById("credits-modal");
  if (el) el.remove();
}
