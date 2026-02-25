// 事件触发字动画模块
// 在公路区域上叠加显示触发字和场景描述

// 显示场景描述文字动画
function showSceneText(event) {
  const sceneText = event && event.triggerConfig && event.triggerConfig.sceneText;
  if (!sceneText) return Promise.resolve();

  hideSceneText();

  return new Promise((resolve) => {
    const overlay = document.createElement("div");
    overlay.id = "scene-text-overlay";
    overlay.style.cssText = `
			position: absolute; inset: 0;
			display: flex; align-items: center; justify-content: center;
			pointer-events: none; z-index: 19;
			opacity: 0;
		`;

    const textEl = document.createElement("div");
    textEl.style.cssText = `
			font-size: clamp(1.2rem, 4vw, 2rem);
			font-weight: bold;
			color: #ffffff;
			text-shadow: 0 0 10px rgba(196, 30, 58, 0.8), 0 0 20px rgba(196, 30, 58, 0.6), 2px 2px 4px rgba(0, 0, 0, 0.8);
			text-align: center;
			padding: 1rem 2rem;
			background: rgba(0, 0, 0, 0.6);
			border-radius: 12px;
			border: 2px solid rgba(196, 30, 58, 0.5);
			backdrop-filter: blur(4px);
			transform: translateX(100%);
			transition: transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.4s ease-in;
		`;
    textEl.textContent = sceneText;

    overlay.appendChild(textEl);

    const roadEl = document.getElementById("road");
    if (roadEl && roadEl.parentElement) {
      roadEl.parentElement.appendChild(overlay);
      
      requestAnimationFrame(() => {
        overlay.style.opacity = "1";
        textEl.style.transform = "translateX(0)";
      });

      // 停留后淡出
      setTimeout(() => {
        textEl.style.transition = "transform 0.5s ease-out, opacity 0.4s ease-out";
        textEl.style.transform = "translateX(-100%)";
        overlay.style.opacity = "0";
        setTimeout(() => {
          overlay.remove();
          resolve();
        }, 500);
      }, 1500);
    } else {
      resolve();
    }
  });
}

// 隐藏场景描述文字
function hideSceneText() {
  const overlay = document.getElementById("scene-text-overlay");
  if (overlay) overlay.remove();
}

// 显示触发字
function showTriggerChar(event) {
  let config = null;
  if (event && event.triggerConfig) {
    config = event.triggerConfig;
  } else if (
    typeof EVENT_TRIGGER_CHARS !== "undefined" &&
    EVENT_TRIGGER_CHARS[event]
  ) {
    config = EVENT_TRIGGER_CHARS[event];
  }

  if (!config) return;

  hideTriggerChar();

  const overlay = document.createElement("div");
  overlay.id = "trigger-char-overlay";
  overlay.style.cssText = `
		position: absolute; inset: 0;
		display: flex; align-items: center; justify-content: center;
		pointer-events: none; z-index: 20;
		opacity: 0; transition: opacity 0.5s ease-in;
	`;
  overlay.innerHTML = `
		<span style="
			font-size: 4rem; font-weight: bold;
			color: ${config.color};
			text-shadow: 0 0 20px ${config.color}, 0 0 40px ${config.color};
		">${config.char}</span>
	`;

  // 插入到公路容器的父元素中
  const roadEl = document.getElementById("road");
  if (roadEl && roadEl.parentElement) {
    roadEl.parentElement.appendChild(overlay);
    // 触发重排后淡入
    requestAnimationFrame(() => {
      overlay.style.opacity = "1";
    });
  }
}

// 隐藏触发字（淡出后移除DOM）
function hideTriggerChar() {
  const overlay = document.getElementById("trigger-char-overlay");
  if (!overlay) return;
  overlay.style.transition = "opacity 0.3s ease-out";
  overlay.style.opacity = "0";
  setTimeout(() => overlay.remove(), 300);
}

// ─── 事件背景主题特效 ───────────────────────────────────────────────

// 保存默认样式以便恢复
let _defaultThemeStyles = null;

// 特效计时器 & 叠层管理
let _activeThemeIntervals = [];
let _activeThemeTimeouts  = [];
let _clownCounterRafId    = null; // 小丑弹窗反向补偿 RAF

// 清除所有主题计时器
function _clearThemeTimers() {
  _activeThemeIntervals.forEach(id => clearInterval(id));
  _activeThemeIntervals = [];
  _activeThemeTimeouts.forEach(id => clearTimeout(id));
  _activeThemeTimeouts  = [];
}

// ─── 小丑之夜：弹窗反向抵消抖动 ──────────────────────────────────
// 每帧读取 #game-canvas 的实际 transform matrix，对 #event-modal 施加逆变换
// 使弹窗在屏幕上保持视觉静止
function _startClownModalCounterShake() {
  if (_clownCounterRafId) return;
  const canvas = document.getElementById("game-canvas");
  if (!canvas) return;

  function _loop() {
    const modal = document.getElementById("event-modal");
    if (modal && canvas.classList.contains("clown-canvas-shake")) {
      // 读取 canvas 当前 computed transform matrix
      const mat = new DOMMatrix(window.getComputedStyle(canvas).transform);
      // 反向：平移取负，旋转取负
      const tx = -mat.m41;
      const ty = -mat.m42;
      // 从 matrix 提取旋转角度
      const angle = -Math.atan2(mat.m21, mat.m11);
      const deg = angle * (180 / Math.PI);
      modal.style.transform = `translate(${tx}px,${ty}px) rotate(${deg}deg)`;
    } else if (modal) {
      modal.style.transform = "";
    }
    _clownCounterRafId = requestAnimationFrame(_loop);
  }
  _clownCounterRafId = requestAnimationFrame(_loop);
}

function _stopClownModalCounterShake() {
  if (_clownCounterRafId) {
    cancelAnimationFrame(_clownCounterRafId);
    _clownCounterRafId = null;
  }
  // 清除弹窗残留 transform
  const modal = document.getElementById("event-modal");
  if (modal) modal.style.transform = "";
}

// ─── 时间银行：时钟虚影层 ─────────────────────────────────────────

function _createTimeBankClockGhost() {
  if (document.getElementById("time-bank-clock-ghost")) return;

  // 扫描线底层
  const scanlines = document.createElement("div");
  scanlines.id = "time-bank-scanlines";
  setTimeout(() => scanlines.classList.add("active"), 50);
  document.body.appendChild(scanlines);

  // 时钟主层
  const ghost = document.createElement("div");
  ghost.id = "time-bank-clock-ghost";

  // 主时间行
  const digitEl = document.createElement("div");
  digitEl.className = "time-ghost-digit";
  digitEl.id = "time-ghost-main";

  // 日期副行
  const dateEl = document.createElement("div");
  dateEl.className = "time-ghost-date";
  dateEl.id = "time-ghost-date";

  ghost.appendChild(digitEl);
  ghost.appendChild(dateEl);
  document.body.appendChild(ghost);

  // 辅助：格式化时间
  function _fmt2(n) { return String(n).padStart(2, "0"); }

  function _updateClock() {
    const now   = new Date();
    const hh    = _fmt2(now.getHours());
    const mm    = _fmt2(now.getMinutes());
    const ss    = _fmt2(now.getSeconds());
    const yyyy  = now.getFullYear();
    const mo    = _fmt2(now.getMonth() + 1);
    const dd    = _fmt2(now.getDate());
    const main  = document.getElementById("time-ghost-main");
    const date  = document.getElementById("time-ghost-date");
    if (main) main.textContent = `${hh}:${mm}:${ss}`;
    if (date) date.textContent = `${yyyy}.${mo}.${dd}`;
  }

  _updateClock();
  // 每秒更新时钟
  const clockInterval = setInterval(_updateClock, 1000);
  _activeThemeIntervals.push(clockInterval);

  // 钟面持续脉冲动画
  digitEl.style.animation = "ghostPulse 3s ease-in-out infinite";

  // 随机故障触发器：每 3~7 秒随机触发一次虚影故障
  function _scheduleNextGlitch() {
    const delay = 3000 + Math.random() * 4000;
    const tid = setTimeout(() => {
      _triggerClockGlitch();
      _scheduleNextGlitch();
    }, delay);
    _activeThemeTimeouts.push(tid);
  }
  _scheduleNextGlitch();

  // 故障时也会对 #game-canvas 做短暂 filter 扰动
  function _triggerClockGlitch() {
    const main = document.getElementById("time-ghost-main");
    if (!main) return;
    // 取消原脉冲，触发故障帧
    main.style.animation = "none";
    main.classList.add("glitching");
    const canvas = document.getElementById("game-canvas");
    if (canvas) {
      canvas.style.transition = "filter 0.05s";
      canvas.style.filter = "hue-rotate(180deg) contrast(1.8) saturate(3)";
    }
    const rid = setTimeout(() => {
      main.classList.remove("glitching");
      main.style.animation = "ghostPulse 3s ease-in-out infinite";
      if (canvas) {
        canvas.style.filter = "";
        canvas.style.transition = "filter 0.3s";
      }
    }, 600);
    _activeThemeTimeouts.push(rid);
  }
}

function _removeTimeBankClockGhost() {
  const ghost = document.getElementById("time-bank-clock-ghost");
  if (ghost) {
    ghost.style.transition = "opacity 0.5s ease-out";
    ghost.style.opacity = "0";
    setTimeout(() => ghost.remove(), 500);
  }
  const scanlines = document.getElementById("time-bank-scanlines");
  if (scanlines) {
    scanlines.style.transition = "opacity 0.5s ease-out";
    scanlines.style.opacity = "0";
    setTimeout(() => scanlines.remove(), 500);
  }
}

// 应用事件主题（emoji 粒子 + UI 变色），作用于整个页面
function applyEventTheme(event) {
  if (!event || !event.theme) return;
  const theme = event.theme;
  const body = document.body;
  const canvas = document.getElementById("game-canvas");

  // 保存默认样式（仅首次）
  if (!_defaultThemeStyles) {
    _defaultThemeStyles = {
      bodyBg: body.style.backgroundColor || "",
      canvasBg: canvas ? (canvas.style.backgroundColor || "") : "",
      canvasBorder: canvas ? (canvas.style.borderColor || "") : "",
    };
  }

  // 1) 改变页面整体背景色 + 游戏画面背景/边框色
  if (theme.bgColor) {
    body.style.transition = "background-color 0.8s ease";
    body.style.backgroundColor = theme.bgColor;
    if (canvas) {
      canvas.style.transition = "background-color 0.8s ease, border-color 0.8s ease";
      canvas.style.backgroundColor = theme.bgColor;
    }
  }
  if (theme.borderColor && canvas) {
    canvas.style.borderColor = theme.borderColor;
  }

  // 2) 改变状态条颜色
  if (theme.barColors) {
    const bars = {
      fuel: document.getElementById("fuel-bar"),
      durability: document.getElementById("durability-bar"),
      comfort: document.getElementById("comfort-bar"),
    };
    Object.entries(theme.barColors).forEach(([key, color]) => {
      if (bars[key]) {
        bars[key].dataset.defaultBg = bars[key].style.backgroundColor || "";
        bars[key].style.transition = "background-color 0.8s ease";
        bars[key].style.backgroundColor = color;
      }
    });
  }

  // 3) 创建 emoji 粒子层（挂在 body 上，覆盖整个页面）
  clearEmojiParticles();
  if (theme.emojis && theme.emojis.length > 0) {
    const particleLayer = document.createElement("div");
    particleLayer.id = "event-theme-particles";
    particleLayer.style.cssText = "position:fixed;inset:0;pointer-events:none;z-index:9999;overflow:hidden;";

    // 根据 animation 类型决定粒子数量、速度和动画
    const animType = theme.animation || "float"; // 默认漂浮
    const isFog = animType === "fog";
    const isRain = animType === "rain";
    const isSandstorm = animType === "sandstorm";
    const count = isRain ? 40 : (isFog ? 18 : (isSandstorm ? 30 : 25));

    for (let i = 0; i < count; i++) {
      const span = document.createElement("span");
      span.className = "theme-emoji-particle";
      span.textContent = theme.emojis[Math.floor(Math.random() * theme.emojis.length)];
      const left = Math.random() * 100;

      if (isFog) {
        // 雾气：超大号、慢速水平漂移、半透明
        const size = 2.5 + Math.random() * 2.5;
        const delay = Math.random() * 6;
        const duration = 6 + Math.random() * 5;
        const top = Math.random() * 90;
        span.style.cssText = `
          position:absolute;
          left:${-15 + Math.random() * 30}%;
          top:${top}%;
          font-size:${size}rem;
          opacity:0;
          animation: emojiFogDrift ${duration}s ${delay}s ease-in-out infinite;
          filter: blur(2px) drop-shadow(0 0 8px rgba(200,200,220,0.3));
        `;
      } else if (isSandstorm) {
        // 沙尘暴：超大号黄色云朵、快速水平横扫、高密度覆盖
        const size = 3 + Math.random() * 3.5;
        const delay = Math.random() * 3;
        const duration = 2.5 + Math.random() * 2.5;
        const top = Math.random() * 100;
        span.style.cssText = `
          position:absolute;
          right:${-20 - Math.random() * 15}%;
          top:${top}%;
          font-size:${size}rem;
          opacity:0;
          animation: emojiSandstorm ${duration}s ${delay}s linear infinite;
          filter: brightness(1.3) saturate(2) sepia(0.4) drop-shadow(0 0 12px rgba(217,119,6,0.5));
        `;
      } else if (isRain) {
        // 雨滴：小号、快速、线性下落
        const size = 0.6 + Math.random() * 0.6;
        const delay = Math.random() * 2;
        const duration = 0.6 + Math.random() * 0.8;
        span.style.cssText = `
          position:absolute;
          left:${left}%;
          top:-5%;
          font-size:${size}rem;
          opacity:0;
          animation: emojiRainDrop ${duration}s ${delay}s linear infinite;
        `;
      } else {
        // 默认漂浮：大号、慢速、带旋转
        const size = 0.8 + Math.random() * 1.2;
        const delay = Math.random() * 4;
        const duration = 3 + Math.random() * 4;
        span.style.cssText = `
          position:absolute;
          left:${left}%;
          top:-8%;
          font-size:${size}rem;
          opacity:0;
          animation: emojiFloat ${duration}s ${delay}s ease-in-out infinite;
          filter: drop-shadow(0 0 4px rgba(255,255,255,0.4));
        `;
      }
      particleLayer.appendChild(span);
    }
    body.appendChild(particleLayer);
  }

  // ── 特殊事件专属全页特效 ──────────────────────────────
  if (event.id === "clown_night") {
    // body 只做全页彩虹滤镜
    document.body.classList.add("clown-active");
    // game-canvas 承担抖动
    const canvas = document.getElementById("game-canvas");
    if (canvas) canvas.classList.add("clown-canvas-shake");
    // RAF 循环：实时读取 canvas 的 transform 并对 #event-modal 施加反向补偿
    _startClownModalCounterShake();
    // 启动小丑背景音乐（压低环境音）
    if (typeof ambientGain !== "undefined" && ambientGain) {
      try { ambientGain.gain.linearRampToValueAtTime(0.02, getAudioContext().currentTime + 0.8); } catch (e) {}
    }
    if (typeof startClownMusic === "function") startClownMusic();
  }

  if (event.id === "time_bank") {
    // 时钟虚影层 + 周期性 glitch
    _createTimeBankClockGhost();
  }
}

// 清除 emoji 粒子层
function clearEmojiParticles() {
  const layer = document.getElementById("event-theme-particles");
  if (layer) layer.remove();
}

// 清除事件主题，恢复默认 UI 外观
function clearEventTheme() {
  const body = document.body;
  const canvas = document.getElementById("game-canvas");

  // ── 清除特殊全页特效 ──────────────────────────────────
  // 小丑之夜：移除彩虹滤镜类、canvas 抖动类、停止 RAF 补偿、停止背景音乐
  document.body.classList.remove("clown-active");
  const _clCanvas = document.getElementById("game-canvas");
  if (_clCanvas) _clCanvas.classList.remove("clown-canvas-shake");
  _stopClownModalCounterShake();
  if (typeof stopClownMusic === "function") stopClownMusic();
  if (typeof ambientGain !== "undefined" && ambientGain) {
    try {
      const targetVol = typeof getAmbientVolume === "function" ? getAmbientVolume() : 0.12;
      ambientGain.gain.linearRampToValueAtTime(targetVol, getAudioContext().currentTime + 0.8);
    } catch (e) {}
  }

  // 时间银行：清除虚影层、计时器
  _clearThemeTimers();
  _removeTimeBankClockGhost();

  // 确保 canvas filter 被清除（时间银行 glitch 可能留下）
  if (canvas) {
    canvas.style.transition = "filter 0.3s ease";
    canvas.style.filter = "";
  }

  // 恢复页面背景
  body.style.transition = "background-color 0.6s ease";
  body.style.backgroundColor = _defaultThemeStyles ? _defaultThemeStyles.bodyBg : "";

  // 恢复游戏画面背景/边框
  if (canvas) {
    canvas.style.transition = "background-color 0.6s ease, border-color 0.6s ease, filter 0.3s ease";
    canvas.style.backgroundColor = _defaultThemeStyles ? _defaultThemeStyles.canvasBg : "";
    canvas.style.borderColor = _defaultThemeStyles ? _defaultThemeStyles.canvasBorder : "";
  }

  // 恢复状态条颜色
  ["fuel-bar", "durability-bar", "comfort-bar"].forEach((id) => {
    const bar = document.getElementById(id);
    if (bar && bar.dataset.defaultBg !== undefined) {
      bar.style.transition = "background-color 0.6s ease";
      bar.style.backgroundColor = bar.dataset.defaultBg;
      delete bar.dataset.defaultBg;
    }
  });

  // 粒子淡出后移除
  const layer = document.getElementById("event-theme-particles");
  if (layer) {
    layer.style.transition = "opacity 0.5s ease-out";
    layer.style.opacity = "0";
    setTimeout(() => layer.remove(), 500);
  }

  _defaultThemeStyles = null;
}

