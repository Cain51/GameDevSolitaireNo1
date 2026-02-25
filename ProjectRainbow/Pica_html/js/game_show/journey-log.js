// 事件历程模块
// 记录玩家旅途中的所有事件、抉择、物品使用，并以幽默口吻展示

// ─────────────────────────────────────────────
// 幽默文案模板库
// ─────────────────────────────────────────────

const _JL_EVENT_COMMENTS = [
  "皮卡发出一声叹息（如果皮卡会叹气的话）。",
  "命运的齿轮咔哒一声，转了个方向。",
  "此时窗外飘过一只不明生物，但你选择无视。",
  "天空中出现了一朵奇怪的云。你感到不安，但还是继续开车。",
  "路边的野猫用审视的眼神打量了你一秒，然后继续洗脸。",
  "你隐约听到了《公路之王》的前奏，但音响坏了。",
  "皮卡的仪表盘灯闪了一下，像是在回应什么。",
  "一阵不知从哪来的风，掀走了你帽子——如果你戴了帽子的话。",
  "这是命运，还是路况不好？答案是：都有。",
  "远方的地平线微微发亮，但别高兴太早。",
];

const _JL_CHOICE_COMMENTS = [
  "展现出一个老司机的果断——或者说莽撞。",
  "这个决定会在未来某个凌晨三点让你辗转反侧。",
  "历史会记住这一刻。至少你会。",
  "皮卡表示支持。（皮卡无法表达反对。）",
  "你的选择充满了一种说不清道不明的气质。",
  "事后来看，也许还行。",
  "你以秒为单位做出了这个决定。非常果敢。",
  "旁边的乘客假装没看见。",
  "如果重来一次，你大概还是会这么选。",
  "你的直觉大声说了什么，你选择了相信它。或者无视它。",
];

const _JL_ITEM_COMMENTS = [
  "用得心应手，就像你早就知道它在那里。",
  "皮卡表示这个操作有点迷，但尊重你。",
  "效果拔群！（或者说，聊胜于无。）",
  "你从背包底部掏出了它，背包因此轻了那么一丢丢。",
  "物品使用了，世界还在转。你继续开车。",
  "下次应该多备几个。这是事后说的。",
  "这一刻你显得非常专业。",
  "乘客们若有所思地看了你一眼。",
];

const _JL_MILESTONE_COMMENTS = [
  "轮胎表示它没有意见。",
  "公里数字跳动了一下，历史沉默地见证了这一切。",
  "皮卡发出了引擎的低吟，像是某种形式的庆祝。",
  "路边的标牌飞速掠过，没人知道上面写着什么。",
  "你感到一种莫名的成就感，以及轻微的腰酸。",
];

const _JL_PASSENGER_ON = [
  "车厢里多了一股活人气。不完全是好事，但也不全是坏事。",
  "乘客落座，行程继续。命运再下一城。",
  "皮卡的后视镜里多了一张脸，你装作没注意。",
  "背包角落的零食减少了一包。这之间有没有关联，你不得而知。",
];

const _JL_PASSENGER_OFF = [
  "他们走了，车厢又空了一些。",
  "旅程有始有终，这就是公路故事的规则。",
  "皮卡悄悄叹了口气，至少你觉得是这样。",
  "你没有回头看，但你知道他们也没有。",
];

const _JL_SYSTEM_COMMENTS = [
  "系统在背后做了些什么，你不太清楚。",
  "某些数值发生了变化，这是命运的手笔。",
  "皮卡内部某处发出了声音。你选择忽略。",
];

// 随机取一条评语
function _jlPick(arr) {
  return arr[Math.floor(Math.random() * arr.length)];
}

// ─────────────────────────────────────────────
// 数据记录函数
// ─────────────────────────────────────────────

/**
 * 记录一条历程条目
 * @param {string} type - 'event' | 'choice' | 'item' | 'milestone' | 'passenger_on' | 'passenger_off' | 'system'
 * @param {object} data - 条目数据
 */
function recordJourneyEvent(type, data) {
  if (typeof gameState === "undefined") return;
  if (!Array.isArray(gameState.journeyLog)) gameState.journeyLog = [];

  const mileage = Math.floor((gameState && gameState.mileage) || 0);
  let entry = { type, mileage, ts: Date.now() };

  switch (type) {
    case "event": {
      const title = data.title || data.eventId || "未知事件";
      const comment = _jlPick(_JL_EVENT_COMMENTS);
      entry.icon = data.image || "🎲";
      entry.main = title;
      entry.sub = comment;
      entry.rare = data.rare || false;
      entry.tags = data.tags || [];
      break;
    }
    case "choice": {
      const eventTitle = data.eventTitle || data.eventId || "事件";
      const choiceText = data.choiceText || data.choiceId || "某选项";
      const comment = _jlPick(_JL_CHOICE_COMMENTS);
      entry.icon = "🖊️";
      entry.main = `在「${eventTitle}」中选择了：${choiceText}`;
      entry.sub = comment;
      break;
    }
    case "item": {
      const itemName = data.itemName || data.itemId || "某物品";
      const comment = _jlPick(_JL_ITEM_COMMENTS);
      entry.icon = "🎒";
      entry.main = `使用了 ${itemName}`;
      entry.sub = comment;
      break;
    }
    case "milestone": {
      const km = data.km || mileage;
      const comment = _jlPick(_JL_MILESTONE_COMMENTS);
      entry.icon = "📍";
      entry.main = `里程抵达 ${km} km`;
      entry.sub = comment;
      break;
    }
    case "passenger_on": {
      const name = data.name || "某位乘客";
      const comment = _jlPick(_JL_PASSENGER_ON);
      entry.icon = "🧑‍🤝‍🧑";
      entry.main = `${name} 上车了`;
      entry.sub = comment;
      break;
    }
    case "passenger_off": {
      const name = data.name || "某位乘客";
      const comment = _jlPick(_JL_PASSENGER_OFF);
      entry.icon = "👋";
      entry.main = `${name} 下车了`;
      entry.sub = comment;
      break;
    }
    case "system":
    default: {
      const msg = data.msg || "系统事件";
      const comment = _jlPick(_JL_SYSTEM_COMMENTS);
      entry.icon = data.icon || "⚙️";
      entry.main = msg;
      entry.sub = comment;
      break;
    }
  }

  gameState.journeyLog.push(entry);

  // 里程碑自动记录（每 50 km 记录一次，防重复）
  if (type !== "milestone") {
    const lastMilestone = gameState._lastJourneyMilestone || 0;
    if (mileage >= lastMilestone + 50) {
      const snap = Math.floor(mileage / 50) * 50;
      gameState._lastJourneyMilestone = snap;
      recordJourneyEvent("milestone", { km: snap });
    }
  }
}

// ─────────────────────────────────────────────
// 弹窗 UI
// ─────────────────────────────────────────────

function openJourneyLog() {
  // 如果已打开则关闭
  const existing = document.getElementById("journey-log-modal");
  if (existing) { existing.remove(); return; }

  const log = (typeof gameState !== "undefined" && Array.isArray(gameState.journeyLog))
    ? gameState.journeyLog
    : [];

  // 构建条目 HTML
  let itemsHtml = "";
  if (log.length === 0) {
    itemsHtml = `<div style="text-align:center;color:#6b7280;padding:40px 0;font-size:0.95rem;">
      还没有任何记录……<br><span style="font-size:0.8rem;">开始你的旅途，历程自动记录。</span>
    </div>`;
  } else {
    // 倒序显示（最新在上）
    const reversed = [...log].reverse();
    itemsHtml = reversed.map((entry, idx) => {
      const isRare = entry.rare;
      const borderColor = isRare ? "#ff00ff" : _jlTypeColor(entry.type);
      const dotColor = borderColor;
      const timeStr = _jlFormatTime(entry.ts);
      const kmStr = entry.mileage != null ? `${entry.mileage} km` : "";
      const tagsBadges = Array.isArray(entry.tags) && entry.tags.length > 0
        ? entry.tags.map(t => `<span style="font-size:0.6rem;background:#ffffff18;border-radius:999px;padding:1px 5px;color:#9ca3af;margin-right:3px;">${t}</span>`).join("")
        : "";
      return `
        <div class="jl-entry" style="display:flex;gap:10px;align-items:flex-start;padding:10px 12px;border-left:3px solid ${borderColor};background:#0d0d0d;border-radius:6px;margin-bottom:6px;">
          <div style="font-size:1.4rem;line-height:1;flex-shrink:0;margin-top:1px;">${entry.icon || "📌"}</div>
          <div style="flex:1;min-width:0;">
            <div style="font-size:0.88rem;font-weight:600;color:#e5e7eb;line-height:1.4;margin-bottom:2px;">${entry.main || ""}</div>
            <div style="font-size:0.75rem;color:#9ca3af;font-style:italic;margin-bottom:3px;">${entry.sub || ""}</div>
            <div style="display:flex;align-items:center;gap:6px;flex-wrap:wrap;">
              ${kmStr ? `<span style="font-size:0.7rem;color:#6b7280;background:#ffffff0a;border-radius:4px;padding:1px 5px;">📍 ${kmStr}</span>` : ""}
              ${timeStr ? `<span style="font-size:0.7rem;color:#6b7280;">${timeStr}</span>` : ""}
              ${tagsBadges}
            </div>
          </div>
          <div style="width:8px;height:8px;border-radius:50%;background:${dotColor};flex-shrink:0;margin-top:5px;opacity:0.7;"></div>
        </div>`;
    }).join("");
  }

  // 统计摘要
  const totalEvents = log.filter(e => e.type === "event").length;
  const totalChoices = log.filter(e => e.type === "choice").length;
  const totalItems = log.filter(e => e.type === "item").length;
  const totalMilestones = log.filter(e => e.type === "milestone").length;
  const currentMileage = (typeof gameState !== "undefined" && gameState.mileage) ? Math.floor(gameState.mileage) : 0;

  const modal = document.createElement("div");
  modal.id = "journey-log-modal";
  modal.style.cssText = `
    position:fixed;inset:0;z-index:99990;background:rgba(0,0,0,0.88);
    display:flex;align-items:center;justify-content:center;
    padding:16px;box-sizing:border-box;
    animation:jlFadeIn 0.25s ease;
  `;

  modal.innerHTML = `
    <div id="journey-log-inner" style="
      background:#111827;border:2px solid #c41e3a;border-radius:16px;
      width:100%;max-width:min(520px, 94vw);max-height:88vh;
      display:flex;flex-direction:column;
      box-shadow:0 0 60px rgba(196,30,58,0.35);
      overflow:hidden;position:relative;
    ">
      <!-- 标题栏 -->
      <div style="padding:16px 20px 12px;border-bottom:1px solid #1f2937;display:flex;align-items:center;justify-content:space-between;flex-shrink:0;">
        <div>
          <div style="font-size:1.1rem;font-weight:800;color:#c41e3a;letter-spacing:1px;">📜 旅途历程</div>
          <div style="font-size:0.72rem;color:#6b7280;margin-top:2px;">记录了这趟旅程里发生的一切</div>
        </div>
        <button onclick="closeJourneyLog()" style="
          width:32px;height:32px;border-radius:50%;
          background:#1f2937;border:1px solid #374151;
          color:#9ca3af;font-size:1rem;cursor:pointer;
          display:flex;align-items:center;justify-content:center;
          flex-shrink:0;transition:all 0.2s;
        " onmouseenter="this.style.background='#374151';this.style.color='#fff';"
           onmouseleave="this.style.background='#1f2937';this.style.color='#9ca3af';">✕</button>
      </div>

      <!-- 统计摘要 -->
      <div style="padding:10px 20px;border-bottom:1px solid #1f2937;display:flex;gap:12px;flex-wrap:wrap;flex-shrink:0;">
        <div style="text-align:center;flex:1;min-width:60px;">
          <div style="font-size:1.2rem;font-weight:700;color:#c41e3a;">${currentMileage}</div>
          <div style="font-size:0.65rem;color:#6b7280;">公里</div>
        </div>
        <div style="text-align:center;flex:1;min-width:60px;">
          <div style="font-size:1.2rem;font-weight:700;color:#60a5fa;">${totalEvents}</div>
          <div style="font-size:0.65rem;color:#6b7280;">遭遇事件</div>
        </div>
        <div style="text-align:center;flex:1;min-width:60px;">
          <div style="font-size:1.2rem;font-weight:700;color:#34d399;">${totalChoices}</div>
          <div style="font-size:0.65rem;color:#6b7280;">艰难抉择</div>
        </div>
        <div style="text-align:center;flex:1;min-width:60px;">
          <div style="font-size:1.2rem;font-weight:700;color:#fbbf24;">${totalItems}</div>
          <div style="font-size:0.65rem;color:#6b7280;">物品使用</div>
        </div>
        <div style="text-align:center;flex:1;min-width:60px;">
          <div style="font-size:1.2rem;font-weight:700;color:#a78bfa;">${totalMilestones}</div>
          <div style="font-size:0.65rem;color:#6b7280;">里程节点</div>
        </div>
      </div>

      <!-- 历程列表 -->
      <div id="journey-log-list" style="flex:1;overflow-y:auto;padding:14px 16px;scroll-behavior:smooth;">
        ${itemsHtml}
      </div>

      <!-- 底部按钮 -->
      <div style="padding:12px 16px;border-top:1px solid #1f2937;display:flex;gap:8px;flex-shrink:0;">
        <button onclick="exportJourneyLogAsPng()" style="
          flex:1;padding:9px 12px;border-radius:8px;border:1px solid #374151;
          background:#1f2937;color:#9ca3af;font-size:0.8rem;cursor:pointer;
          transition:all 0.2s;
        " onmouseenter="this.style.background='#374151';this.style.color='#fff';"
           onmouseleave="this.style.background='#1f2937';this.style.color='#9ca3af';">
          📷 导出图片
        </button>
        <button onclick="closeJourneyLog()" style="
          flex:1;padding:9px 12px;border-radius:8px;border:none;
          background:#c41e3a;color:#fff;font-size:0.8rem;cursor:pointer;
          font-weight:600;transition:all 0.2s;
        " onmouseenter="this.style.background='#e63950';"
           onmouseleave="this.style.background='#c41e3a';">
          关闭
        </button>
      </div>
    </div>
  `;

  // 点击背景关闭
  modal.addEventListener("click", (e) => {
    if (e.target === modal) closeJourneyLog();
  });

  document.body.appendChild(modal);
}

function closeJourneyLog() {
  const modal = document.getElementById("journey-log-modal");
  if (!modal) return;
  modal.style.animation = "jlFadeOut 0.2s ease forwards";
  modal.addEventListener("animationend", () => modal.remove(), { once: true });
}

// 获取类型对应颜色
function _jlTypeColor(type) {
  const map = {
    event: "#60a5fa",
    choice: "#34d399",
    item: "#fbbf24",
    milestone: "#a78bfa",
    passenger_on: "#f9a8d4",
    passenger_off: "#94a3b8",
    system: "#6b7280",
  };
  return map[type] || "#6b7280";
}

// 格式化时间戳为"X:XX前"
function _jlFormatTime(ts) {
  if (!ts) return "";
  const diff = Date.now() - ts;
  if (diff < 60000) return "片刻前";
  if (diff < 3600000) return `${Math.floor(diff / 60000)} 分钟前`;
  return `${Math.floor(diff / 3600000)} 小时前`;
}

// ─────────────────────────────────────────────
// PNG 导出
// ─────────────────────────────────────────────

function exportJourneyLogAsPng() {
  const log = (typeof gameState !== "undefined" && Array.isArray(gameState.journeyLog))
    ? gameState.journeyLog
    : [];
  const mileage = (typeof gameState !== "undefined" && gameState.mileage) ? Math.floor(gameState.mileage) : 0;

  // 构建用于截图的独立 DOM 节点（不在 viewport 中）
  const wrapper = document.createElement("div");
  wrapper.style.cssText = `
    position:fixed;left:-9999px;top:0;
    width:min(420px, 94vw);background:#0f172a;padding:20px;box-sizing:border-box;
    font-family:system-ui,-apple-system,'Segoe UI',sans-serif;
    border-radius:16px;border:2px solid #c41e3a;
  `;

  // 标题
  const header = document.createElement("div");
  header.style.cssText = "text-align:center;margin-bottom:16px;padding-bottom:12px;border-bottom:1px solid #1f2937;";
  header.innerHTML = `
    <div style="font-size:1.3rem;font-weight:800;color:#c41e3a;">📜 旅途历程</div>
    <div style="font-size:0.75rem;color:#6b7280;margin-top:4px;">行驶里程：${mileage} km · 共 ${log.length} 条记录</div>
  `;
  wrapper.appendChild(header);

  if (log.length === 0) {
    const empty = document.createElement("div");
    empty.style.cssText = "text-align:center;color:#6b7280;padding:30px 0;font-size:0.9rem;";
    empty.textContent = "暂无历程记录";
    wrapper.appendChild(empty);
  } else {
    [...log].reverse().forEach(entry => {
      const item = document.createElement("div");
      item.style.cssText = `
        display:flex;gap:10px;align-items:flex-start;
        padding:8px 10px;margin-bottom:5px;
        border-left:3px solid ${_jlTypeColor(entry.type)};
        background:#0d1117;border-radius:5px;
      `;
      const kmStr = entry.mileage != null ? ` · ${entry.mileage} km` : "";
      item.innerHTML = `
        <div style="font-size:1.2rem;line-height:1;flex-shrink:0;margin-top:1px;">${entry.icon || "📌"}</div>
        <div style="flex:1;min-width:0;">
          <div style="font-size:0.82rem;font-weight:600;color:#e5e7eb;margin-bottom:2px;">${entry.main || ""}</div>
          <div style="font-size:0.7rem;color:#9ca3af;font-style:italic;">${entry.sub || ""}${kmStr ? `<span style="color:#6b7280;"> ${kmStr}</span>` : ""}</div>
        </div>
      `;
      wrapper.appendChild(item);
    });
  }

  // 版权脚注
  const footer = document.createElement("div");
  footer.style.cssText = "text-align:center;margin-top:12px;padding-top:10px;border-top:1px solid #1f2937;font-size:0.65rem;color:#374151;";
  footer.textContent = "皮卡奇遇记 · 旅途历程报告";
  wrapper.appendChild(footer);

  document.body.appendChild(wrapper);

  // 检查 html2canvas
  if (typeof html2canvas === "undefined") {
    document.body.removeChild(wrapper);
    _showJlToast("导出失败：html2canvas 未加载，请检查网络连接。");
    return;
  }

  // 提示正在生成
  _showJlToast("正在生成图片……");

  html2canvas(wrapper, {
    backgroundColor: "#0f172a",
    scale: 2,
    useCORS: true,
    logging: false,
  }).then(canvas => {
    document.body.removeChild(wrapper);
    const link = document.createElement("a");
    link.download = `旅途历程_${mileage}km.png`;
    link.href = canvas.toDataURL("image/png");
    link.click();
    _showJlToast("✅ 图片已导出！");
  }).catch(err => {
    document.body.removeChild(wrapper);
    console.error("历程导出失败:", err);
    _showJlToast("导出失败，请重试。");
  });
}

// 简易 toast 提示
function _showJlToast(msg) {
  const old = document.getElementById("jl-toast");
  if (old) old.remove();

  const toast = document.createElement("div");
  toast.id = "jl-toast";
  toast.style.cssText = `
    position:fixed;bottom:24px;left:50%;transform:translateX(-50%);
    background:#1f2937;border:1px solid #374151;border-radius:8px;
    padding:8px 18px;color:#e5e7eb;font-size:0.82rem;
    z-index:999999;pointer-events:none;
    animation:jlFadeIn 0.2s ease;
  `;
  toast.textContent = msg;
  document.body.appendChild(toast);
  setTimeout(() => {
    toast.style.animation = "jlFadeOut 0.3s ease forwards";
    toast.addEventListener("animationend", () => toast.remove(), { once: true });
  }, 2500);
}
