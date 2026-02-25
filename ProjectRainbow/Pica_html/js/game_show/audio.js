// 音效模块 - 按钮音效 + 公路环境白噪音
// 使用 Web Audio API 合成，无需外部音频文件

let audioCtx = null;
let ambientNode = null;
let ambientGain = null;
let ambientStarted = false;
let engineNodes = [];
let engineGain = null;
let engineStarted = false;
let lastHoverTime = 0;
const HOVER_THROTTLE_MS = 80;

// ─── 音量状态 ────────────────────────────────────
const VOLUME_STORAGE_KEY = "chinese_truck_adventure_volume";
const DEBUG_STORAGE_KEY = "chinese_truck_adventure_debug";
let volumeSettings = { master: 1.0, ambient: 1.0, sfx: 1.0 };

function loadVolumeSettings() {
  try {
    const saved = localStorage.getItem(VOLUME_STORAGE_KEY);
    if (saved) {
      const parsed = JSON.parse(saved);
      volumeSettings.master = typeof parsed.master === "number" ? parsed.master : 1.0;
      volumeSettings.ambient = typeof parsed.ambient === "number" ? parsed.ambient : 1.0;
      volumeSettings.sfx = typeof parsed.sfx === "number" ? parsed.sfx : 1.0;
    }
  } catch (e) {}
  // debug 状态不从 localStorage 恢复（非简单模式下入口隐藏，admin permit 使用后自毁）
  // 清除可能遗留的旧 debug=true 记录，避免刷新后自动弹出控制台
  try { localStorage.removeItem(DEBUG_STORAGE_KEY); } catch (e) {}
}

function saveVolumeSettings() {
  try {
    localStorage.setItem(VOLUME_STORAGE_KEY, JSON.stringify(volumeSettings));
  } catch (e) {}
}

// 计算实际增益值
function getAmbientVolume() { return 0.12 * volumeSettings.master * volumeSettings.ambient; }
function getEngineVolume() { return 0.08 * volumeSettings.master * volumeSettings.ambient; }
function getSfxClickVolume() { return 0.15 * volumeSettings.master * volumeSettings.sfx; }
function getSfxHoverVolume() { return 0.04 * volumeSettings.master * volumeSettings.sfx; }

// 实时更新所有增益节点
function applyVolumeToNodes() {
  if (ambientGain) ambientGain.gain.value = getAmbientVolume();
  if (engineGain) engineGain.gain.value = getEngineVolume();
}

function getAudioContext() {
  if (!audioCtx) {
    audioCtx = new (window.AudioContext || window.webkitAudioContext)();
  }
  return audioCtx;
}

// 按钮点击音效
function playClick() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.frequency.setValueAtTime(800, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(400, ctx.currentTime + 0.05);
    osc.type = "sine";
    gain.gain.setValueAtTime(vol, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.08);
    osc.start(ctx.currentTime);
    osc.stop(ctx.currentTime + 0.08);
  } catch (e) {}
}

// 按钮悬停音效（轻微滴答）
function playHover() {
  const now = Date.now();
  if (now - lastHoverTime < HOVER_THROTTLE_MS) return;
  lastHoverTime = now;
  try {
    const vol = getSfxHoverVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.frequency.setValueAtTime(600, ctx.currentTime);
    osc.type = "sine";
    gain.gain.setValueAtTime(vol, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.02);
    osc.start(ctx.currentTime);
    osc.stop(ctx.currentTime + 0.02);
  } catch (e) {}
}

// 事件选项按钮点击音效（比普通 click 更厚重，有确认感）
function playEventChoice() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const now = ctx.currentTime;
    // 主音：中频短促下扫，有力感
    const osc1 = ctx.createOscillator();
    const g1 = ctx.createGain();
    osc1.type = "sine";
    osc1.frequency.setValueAtTime(520, now);
    osc1.frequency.exponentialRampToValueAtTime(280, now + 0.1);
    g1.gain.setValueAtTime(vol * 1.1, now);
    g1.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
    osc1.connect(g1);
    g1.connect(ctx.destination);
    osc1.start(now);
    osc1.stop(now + 0.12);
    // 叠加低频脉冲，增加厚度
    const osc2 = ctx.createOscillator();
    const g2 = ctx.createGain();
    osc2.type = "triangle";
    osc2.frequency.setValueAtTime(180, now);
    osc2.frequency.exponentialRampToValueAtTime(100, now + 0.08);
    g2.gain.setValueAtTime(vol * 0.5, now);
    g2.gain.exponentialRampToValueAtTime(0.001, now + 0.1);
    osc2.connect(g2);
    g2.connect(ctx.destination);
    osc2.start(now);
    osc2.stop(now + 0.1);
  } catch (e) {}
}

// ─── 属性恢复音效 ────────────────────────────────────────────────
// playStatRestore(type): type = "fuel" | "durability" | "comfort"
// 燃油恢复：低沉的"咕噜"加注音（上扫）
// 耐久恢复：金属敲击修复感（双击）
// 舒适恢复：轻柔的上扬弦音
let _lastStatRestoreTime = 0;
const STAT_RESTORE_THROTTLE_MS = 120; // 防止同帧多次触发叠音

function playStatRestore(type) {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    // 节流：防止同一帧内多属性同时恢复时叠音过密
    const now = Date.now();
    if (now - _lastStatRestoreTime < STAT_RESTORE_THROTTLE_MS) return;
    _lastStatRestoreTime = now;
    const ctx = getAudioContext();
    const t = ctx.currentTime;

    if (type === "fuel") {
      // 燃油：低沉上扫，像油箱加满的"咕噜"声
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.type = "sine";
      osc.frequency.setValueAtTime(160, t);
      osc.frequency.exponentialRampToValueAtTime(320, t + 0.18);
      gain.gain.setValueAtTime(vol * 0.8, t);
      gain.gain.exponentialRampToValueAtTime(0.001, t + 0.22);
      osc.connect(gain);
      gain.connect(ctx.destination);
      osc.start(t);
      osc.stop(t + 0.22);
      // 叠一个短促的气泡感泛音
      const osc2 = ctx.createOscillator();
      const g2 = ctx.createGain();
      osc2.type = "triangle";
      osc2.frequency.setValueAtTime(480, t + 0.06);
      osc2.frequency.exponentialRampToValueAtTime(240, t + 0.2);
      g2.gain.setValueAtTime(0, t);
      g2.gain.setValueAtTime(vol * 0.3, t + 0.06);
      g2.gain.exponentialRampToValueAtTime(0.001, t + 0.22);
      osc2.connect(g2);
      g2.connect(ctx.destination);
      osc2.start(t + 0.06);
      osc2.stop(t + 0.22);

    } else if (type === "durability") {
      // 耐久：金属敲击双音，修复感（叮-叮）
      const playHit = (offset, freq, vol_) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = "square";
        osc.frequency.setValueAtTime(freq, t + offset);
        osc.frequency.exponentialRampToValueAtTime(freq * 0.6, t + offset + 0.06);
        gain.gain.setValueAtTime(0, t);
        gain.gain.setValueAtTime(vol_, t + offset);
        gain.gain.exponentialRampToValueAtTime(0.001, t + offset + 0.1);
        // 低通滤波让方波变得没那么刺耳
        const filter = ctx.createBiquadFilter();
        filter.type = "lowpass";
        filter.frequency.value = 900;
        osc.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        osc.start(t + offset);
        osc.stop(t + offset + 0.1);
      };
      playHit(0,      440, vol * 0.7);  // 第一击
      playHit(0.11,   550, vol * 0.6);  // 第二击（稍高）

    } else if (type === "comfort") {
      // 舒适：轻柔上扬三连音，像舒缓的弦乐拨奏
      const notes = [523.25, 659.25, 783.99]; // C5 E5 G5
      notes.forEach((freq, i) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = "sine";
        osc.frequency.setValueAtTime(freq, t + i * 0.07);
        gain.gain.setValueAtTime(0, t);
        gain.gain.setValueAtTime(vol * 0.55, t + i * 0.07);
        gain.gain.exponentialRampToValueAtTime(0.001, t + i * 0.07 + 0.18);
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start(t + i * 0.07);
        osc.stop(t + i * 0.07 + 0.18);
      });
    }
  } catch (e) {}
}

// 金币获得音效：清脆的"叮！"（高频正弦波双音，模拟金属碰撞感）
function playGoldCoin() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const now = ctx.currentTime;
    // 第一声：明亮主音（C6 = 1046.5Hz）
    const osc1 = ctx.createOscillator();
    const g1 = ctx.createGain();
    osc1.type = "sine";
    osc1.frequency.setValueAtTime(1046.5, now);
    osc1.frequency.exponentialRampToValueAtTime(880, now + 0.18);
    g1.gain.setValueAtTime(vol * 0.9, now);
    g1.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
    osc1.connect(g1);
    g1.connect(ctx.destination);
    osc1.start(now);
    osc1.stop(now + 0.22);
    // 第二声：轻柔泛音（E6 = 1318Hz），略微延迟，增加金属感
    const osc2 = ctx.createOscillator();
    const g2 = ctx.createGain();
    osc2.type = "sine";
    osc2.frequency.setValueAtTime(1318, now + 0.012);
    osc2.frequency.exponentialRampToValueAtTime(1100, now + 0.16);
    g2.gain.setValueAtTime(0, now);
    g2.gain.setValueAtTime(vol * 0.45, now + 0.012);
    g2.gain.exponentialRampToValueAtTime(0.001, now + 0.18);
    osc2.connect(g2);
    g2.connect(ctx.destination);
    osc2.start(now + 0.012);
    osc2.stop(now + 0.18);
  } catch (e) {}
}

// ─── 升级音效 ────────────────────────────────────────────────────
// 三段上升音阶 + 最后一声高亮长音，有"等级提升"的爽感
function playUpgrade() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const t = ctx.currentTime;
    // 四音上行：C5 → E5 → G5 → C6
    const upgNotes = [523.25, 659.25, 783.99, 1046.5];
    upgNotes.forEach((freq, i) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.type = i === 3 ? "sine" : "triangle";
      osc.frequency.setValueAtTime(freq, t + i * 0.09);
      const dur = i === 3 ? 0.35 : 0.12;
      gain.gain.setValueAtTime(0, t);
      gain.gain.setValueAtTime(vol * (i === 3 ? 1.0 : 0.65), t + i * 0.09);
      gain.gain.exponentialRampToValueAtTime(0.001, t + i * 0.09 + dur);
      osc.connect(gain);
      gain.connect(ctx.destination);
      osc.start(t + i * 0.09);
      osc.stop(t + i * 0.09 + dur);
    });
    // 叠一个和弦泛音让最后的高音更丰满
    const oscChord = ctx.createOscillator();
    const gChord = ctx.createGain();
    oscChord.type = "sine";
    oscChord.frequency.setValueAtTime(1318.5, t + 0.27); // E6
    gChord.gain.setValueAtTime(0, t);
    gChord.gain.setValueAtTime(vol * 0.35, t + 0.27);
    gChord.gain.exponentialRampToValueAtTime(0.001, t + 0.62);
    oscChord.connect(gChord);
    gChord.connect(ctx.destination);
    oscChord.start(t + 0.27);
    oscChord.stop(t + 0.62);
  } catch (e) {}
}

// ─── 制作音效 ────────────────────────────────────────────────────
// 两声金属敲击（锤打感）+ 完成时一声明亮叮响
function playCraft() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const t = ctx.currentTime;
    // 两声锤击（低频方波，配低通滤波）
    [0, 0.14].forEach((offset, i) => {
      const osc = ctx.createOscillator();
      const filt = ctx.createBiquadFilter();
      const gain = ctx.createGain();
      osc.type = "sawtooth";
      osc.frequency.setValueAtTime(200 - i * 30, t + offset);
      osc.frequency.exponentialRampToValueAtTime(80, t + offset + 0.09);
      filt.type = "lowpass";
      filt.frequency.value = 700;
      gain.gain.setValueAtTime(vol * 0.75, t + offset);
      gain.gain.exponentialRampToValueAtTime(0.001, t + offset + 0.11);
      osc.connect(filt);
      filt.connect(gain);
      gain.connect(ctx.destination);
      osc.start(t + offset);
      osc.stop(t + offset + 0.11);
    });
    // 完成叮响（高频正弦，0.32s后）
    const oscDing = ctx.createOscillator();
    const gDing = ctx.createGain();
    oscDing.type = "sine";
    oscDing.frequency.setValueAtTime(1200, t + 0.32);
    oscDing.frequency.exponentialRampToValueAtTime(900, t + 0.52);
    gDing.gain.setValueAtTime(vol * 0.8, t + 0.32);
    gDing.gain.exponentialRampToValueAtTime(0.001, t + 0.52);
    oscDing.connect(gDing);
    gDing.connect(ctx.destination);
    oscDing.start(t + 0.32);
    oscDing.stop(t + 0.52);
  } catch (e) {}
}

// ─── Debug 操作音效 ──────────────────────────────────────────────
// 电子故障感：快速乱码扫频 + 低沉脉冲，带"系统入侵"气质
function playDebugAction() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const t = ctx.currentTime;
    // 高频锯齿扫频（乱码感）
    const osc1 = ctx.createOscillator();
    const filt1 = ctx.createBiquadFilter();
    const g1 = ctx.createGain();
    osc1.type = "sawtooth";
    osc1.frequency.setValueAtTime(1800, t);
    osc1.frequency.linearRampToValueAtTime(400, t + 0.06);
    osc1.frequency.linearRampToValueAtTime(1200, t + 0.09);
    osc1.frequency.linearRampToValueAtTime(200, t + 0.14);
    filt1.type = "bandpass";
    filt1.frequency.value = 800;
    filt1.Q.value = 2;
    g1.gain.setValueAtTime(vol * 0.7, t);
    g1.gain.exponentialRampToValueAtTime(0.001, t + 0.16);
    osc1.connect(filt1);
    filt1.connect(g1);
    g1.connect(ctx.destination);
    osc1.start(t);
    osc1.stop(t + 0.16);
    // 低频确认脉冲（赛博朋克感）
    const osc2 = ctx.createOscillator();
    const g2 = ctx.createGain();
    osc2.type = "square";
    osc2.frequency.setValueAtTime(120, t + 0.05);
    osc2.frequency.exponentialRampToValueAtTime(60, t + 0.18);
    const filt2 = ctx.createBiquadFilter();
    filt2.type = "lowpass";
    filt2.frequency.value = 300;
    g2.gain.setValueAtTime(vol * 0.55, t + 0.05);
    g2.gain.exponentialRampToValueAtTime(0.001, t + 0.22);
    osc2.connect(filt2);
    filt2.connect(g2);
    g2.connect(ctx.destination);
    osc2.start(t + 0.05);
    osc2.stop(t + 0.22);
  } catch (e) {}
}

// ─── 命运轮盘背景音乐 ────────────────────────────────────────────
// 风格：诡异神秘，低沉颤音 + 随机失调短音 + 慢速嗡鸣
// 由三个层叠的循环音效构成，持续播放直到 stopFateRouletteMusic() 被调用
let _fateRouletteMusicGain   = null;
let _fateRouletteMusicActive = false;
let _fateRouletteMusicTimers = [];

function startFateRouletteMusic() {
  if (_fateRouletteMusicActive) return;
  _fateRouletteMusicActive = true;
  try {
    const ctx = getAudioContext();
    const masterGain = ctx.createGain();
    masterGain.gain.setValueAtTime(0, ctx.currentTime);
    masterGain.gain.linearRampToValueAtTime(
      0.18 * volumeSettings.master * volumeSettings.ambient,
      ctx.currentTime + 1.0
    );
    masterGain.connect(ctx.destination);
    _fateRouletteMusicGain = masterGain;

    // ── 层1：低频嗡鸣（诡异震颤基底，LFO调制） ─────────────────
    const droneOsc = ctx.createOscillator();
    const droneLFO = ctx.createOscillator();
    const droneMod = ctx.createGain();
    const droneGain = ctx.createGain();
    droneOsc.type = "sawtooth";
    droneOsc.frequency.value = 55; // A1，低沉
    droneLFO.type = "sine";
    droneLFO.frequency.value = 0.35; // 极慢颤音
    droneMod.gain.value = 8;         // 颤音幅度 ±8Hz
    droneGain.gain.value = 0.5;
    droneLFO.connect(droneMod);
    droneMod.connect(droneOsc.frequency);
    droneOsc.connect(droneGain);
    droneGain.connect(masterGain);
    droneOsc.start();
    droneLFO.start();

    // 低通滤波，只保留低频部分
    const droneFilter = ctx.createBiquadFilter();
    droneFilter.type = "lowpass";
    droneFilter.frequency.value = 180;
    droneGain.disconnect();
    droneGain.connect(droneFilter);
    droneFilter.connect(masterGain);

    // ── 层2：中频颤音（诡异空洞感，泛音叠加） ──────────────────
    const midOsc1 = ctx.createOscillator();
    const midOsc2 = ctx.createOscillator();
    const midLFO  = ctx.createOscillator();
    const midMod  = ctx.createGain();
    const midGain = ctx.createGain();
    midOsc1.type = "sine";
    midOsc1.frequency.value = 110; // A2
    midOsc2.type = "sine";
    midOsc2.frequency.value = 110.7; // 微量失调，制造拍频"哇哇"感
    midLFO.type = "sine";
    midLFO.frequency.value = 0.8;
    midMod.gain.value = 12;
    midGain.gain.value = 0.35;
    midLFO.connect(midMod);
    midMod.connect(midOsc1.frequency);
    midOsc1.connect(midGain);
    midOsc2.connect(midGain);
    midGain.connect(masterGain);
    midOsc1.start();
    midOsc2.start();
    midLFO.start();

    // ── 层3：随机神秘短音序列（不规则间隔的单音拨弦） ──────────
    const mysteryNotes = [110, 146.83, 164.81, 196, 220, 246.94]; // A2~B3 五声音阶
    function scheduleMysteryNote() {
      if (!_fateRouletteMusicActive) return;
      const freq = mysteryNotes[Math.floor(Math.random() * mysteryNotes.length)];
      const detune = (Math.random() - 0.5) * 30; // 随机微失调
      const delay = 0.6 + Math.random() * 1.8;   // 0.6~2.4s 随机间隔
      try {
        const t = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = "triangle";
        osc.frequency.value = freq;
        osc.detune.value = detune;
        gain.gain.setValueAtTime(0, t);
        gain.gain.linearRampToValueAtTime(0.6, t + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.001, t + 0.5);
        osc.connect(gain);
        gain.connect(masterGain);
        osc.start(t);
        osc.stop(t + 0.5);
      } catch (_) {}
      const tid = setTimeout(scheduleMysteryNote, delay * 1000);
      _fateRouletteMusicTimers.push(tid);
    }
    scheduleMysteryNote();
  } catch (e) {}
}

function stopFateRouletteMusic() {
  if (!_fateRouletteMusicActive) return;
  _fateRouletteMusicActive = false;
  _fateRouletteMusicTimers.forEach(t => clearTimeout(t));
  _fateRouletteMusicTimers = [];
  if (_fateRouletteMusicGain) {
    try {
      const ctx = getAudioContext();
      _fateRouletteMusicGain.gain.linearRampToValueAtTime(0, ctx.currentTime + 0.8);
      const g = _fateRouletteMusicGain;
      setTimeout(() => { try { g.disconnect(); } catch (_) {} }, 900);
    } catch (_) {}
    _fateRouletteMusicGain = null;
  }
}

// ─── 命运轮盘旋转音效 ────────────────────────────────────────────
// 转盘旋转时播放的"嗖"加速音 + 滴答节拍，落定时有一声沉闷碰撞
function playRouletteSpinStart() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const t = ctx.currentTime;
    // 加速嗖声（低→高频扫描）
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = "sawtooth";
    osc.frequency.setValueAtTime(80, t);
    osc.frequency.exponentialRampToValueAtTime(600, t + 0.4);
    const filt = ctx.createBiquadFilter();
    filt.type = "bandpass";
    filt.frequency.value = 300;
    filt.Q.value = 1.5;
    gain.gain.setValueAtTime(vol * 0.6, t);
    gain.gain.linearRampToValueAtTime(vol * 0.3, t + 0.4);
    gain.gain.exponentialRampToValueAtTime(0.001, t + 0.55);
    osc.connect(filt);
    filt.connect(gain);
    gain.connect(ctx.destination);
    osc.start(t);
    osc.stop(t + 0.55);
  } catch (e) {}
}

function playRouletteSpinStop() {
  try {
    const vol = getSfxClickVolume();
    if (vol <= 0) return;
    const ctx = getAudioContext();
    const t = ctx.currentTime;
    // 沉闷碰撞（低频冲击）
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = "sine";
    osc.frequency.setValueAtTime(120, t);
    osc.frequency.exponentialRampToValueAtTime(40, t + 0.25);
    gain.gain.setValueAtTime(vol * 1.1, t);
    gain.gain.exponentialRampToValueAtTime(0.001, t + 0.3);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start(t);
    osc.stop(t + 0.3);
    // 金属回响
    const osc2 = ctx.createOscillator();
    const g2 = ctx.createGain();
    osc2.type = "triangle";
    osc2.frequency.setValueAtTime(350, t + 0.02);
    osc2.frequency.exponentialRampToValueAtTime(180, t + 0.3);
    g2.gain.setValueAtTime(vol * 0.4, t + 0.02);
    g2.gain.exponentialRampToValueAtTime(0.001, t + 0.35);
    osc2.connect(g2);
    g2.connect(ctx.destination);
    osc2.start(t + 0.02);
    osc2.stop(t + 0.35);
  } catch (e) {}
}

// ─── 时间银行背景音乐 ────────────────────────────────────────────
// 风格：命运感时钟，规律节拍 + 回声 + 低沉钟声泛音
// 每 0.8s 一次"滴-哒"双击节拍，叠加缓慢渐入的钟声共鸣层
let _timeBankMusicGain   = null;
let _timeBankMusicActive = false;
let _timeBankMusicTimers = [];

function startTimeBankMusic() {
  if (_timeBankMusicActive) return;
  _timeBankMusicActive = true;
  try {
    const ctx = getAudioContext();
    const masterGain = ctx.createGain();
    masterGain.gain.setValueAtTime(0, ctx.currentTime);
    masterGain.gain.linearRampToValueAtTime(
      0.20 * volumeSettings.master * volumeSettings.ambient,
      ctx.currentTime + 0.8
    );
    masterGain.connect(ctx.destination);
    _timeBankMusicGain = masterGain;

    // ── 层1：时钟节拍（规律的"滴-哒"双击） ────────────────────
    const TICK_INTERVAL = 800; // 0.8s 每拍
    function scheduleTick(phase) {
      if (!_timeBankMusicActive) return;
      const isHigh = phase % 2 === 0; // 偶拍高音（滴），奇拍低音（哒）
      try {
        const t = ctx.currentTime;
        const freq = isHigh ? 1600 : 1200;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = "sine";
        osc.frequency.setValueAtTime(freq, t);
        osc.frequency.exponentialRampToValueAtTime(freq * 0.85, t + 0.04);
        gain.gain.setValueAtTime(isHigh ? 0.85 : 0.6, t);
        gain.gain.exponentialRampToValueAtTime(0.001, t + 0.06);
        osc.connect(gain);
        gain.connect(masterGain);
        osc.start(t);
        osc.stop(t + 0.06);
      } catch (_) {}
      const tid = setTimeout(() => scheduleTick(phase + 1), TICK_INTERVAL);
      _timeBankMusicTimers.push(tid);
    }
    scheduleTick(0);

    // ── 层2：回声延迟（每 2 拍有一次更轻的余音） ───────────────
    function scheduleEcho(phase) {
      if (!_timeBankMusicActive) return;
      try {
        const t = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = "sine";
        osc.frequency.value = 1400;
        gain.gain.setValueAtTime(0.25, t);
        gain.gain.exponentialRampToValueAtTime(0.001, t + 0.12);
        osc.connect(gain);
        gain.connect(masterGain);
        osc.start(t);
        osc.stop(t + 0.12);
      } catch (_) {}
      const tid = setTimeout(() => scheduleEcho(phase + 1), TICK_INTERVAL * 2);
      _timeBankMusicTimers.push(tid);
    }
    setTimeout(() => scheduleEcho(0), TICK_INTERVAL * 1.5);

    // ── 层3：低沉钟声共鸣（持续嗡鸣，周期性渐强渐弱） ──────────
    const bellOsc1 = ctx.createOscillator();
    const bellOsc2 = ctx.createOscillator();
    const bellLFO  = ctx.createOscillator();
    const bellMod  = ctx.createGain();
    const bellGain = ctx.createGain();
    bellOsc1.type = "sine";
    bellOsc1.frequency.value = 73.4; // D2
    bellOsc2.type = "sine";
    bellOsc2.frequency.value = 146.8; // D3 八度泛音
    bellLFO.type = "sine";
    bellLFO.frequency.value = 0.2; // 5s 周期渐强渐弱
    bellMod.gain.value = 0.15;
    bellGain.gain.value = 0.3;
    bellLFO.connect(bellMod);
    bellMod.connect(bellGain.gain);
    bellOsc1.connect(bellGain);
    bellOsc2.connect(bellGain);
    bellGain.connect(masterGain);
    bellOsc1.start();
    bellOsc2.start();
    bellLFO.start();
  } catch (e) {}
}

function stopTimeBankMusic() {
  if (!_timeBankMusicActive) return;
  _timeBankMusicActive = false;
  _timeBankMusicTimers.forEach(t => clearTimeout(t));
  _timeBankMusicTimers = [];
  if (_timeBankMusicGain) {
    try {
      const ctx = getAudioContext();
      _timeBankMusicGain.gain.linearRampToValueAtTime(0, ctx.currentTime + 0.6);
      const g = _timeBankMusicGain;
      setTimeout(() => { try { g.disconnect(); } catch (_) {} }, 700);
    } catch (_) {}
    _timeBankMusicGain = null;
  }
}

// ─── 小丑之夜背景音乐 ────────────────────────────────────────────
// 风格：托马斯小火车主题曲调子 + 失调扭曲处理，欢快进行曲变邪恶版
// 原曲为 G 大调 6/8 拍，以三角波模拟八音盒/钢片琴，加入微量失调和混响
let _clownMusicNodes = [];
let _clownMusicGain  = null;
let _clownMusicActive = false;
let _clownMusicScheduleTimeout = null;

// G 大调音阶频率（G4 起 = 392Hz）
// 索引:  0     1     2     3     4     5     6     7     8     9    10    11    12    13    14
// 音名:  G4    A4    B4    C5    D5    E5   F#5    G5    A5    B5    C6    D6   G3    D4    B3
const _TH_FREQ = [
  392.00, 440.00, 493.88, 523.25, 587.33, 659.25, 739.99, 783.99,
  880.00, 987.77, 1046.50, 1174.66,
  196.00, 293.66, 246.94  // 低音区
];

// 托马斯主题旋律（近似还原，G大调6/8，每单位拍 = beat秒）
// [频率索引, 时值, 力度0~1]
// 第一句：G G G  E G - | A A A  F# A - ...
const _TH_MELODY = [
  // ── 第一句（"Thomas the Tank Engine"） ──────────────────
  [0, 0.5, 0.9],  // G
  [0, 0.5, 0.7],  // G
  [0, 0.5, 0.85], // G
  [4, 0.5, 0.8],  // D
  [2, 0.5, 0.9],  // B
  [0, 1.0, 0.85], // G (长)
  [1, 0.5, 0.8],  // A
  [1, 0.5, 0.7],  // A
  [1, 0.5, 0.85], // A
  [5, 0.5, 0.8],  // E
  [3, 0.5, 0.9],  // C
  [1, 1.0, 0.85], // A (长)
  // ── 第二句（上行旋律） ────────────────────────────────────
  [0, 0.5, 0.85], // G
  [1, 0.5, 0.8],  // A
  [2, 0.5, 0.9],  // B
  [3, 0.5, 0.85], // C5
  [4, 0.5, 0.9],  // D5
  [5, 1.0, 0.95], // E5 (长，高潮)
  [6, 0.5, 0.9],  // F#5
  [7, 0.5, 0.85], // G5
  [5, 0.5, 0.8],  // E5
  [4, 0.5, 0.85], // D5
  [2, 0.5, 0.9],  // B4
  [0, 1.0, 0.85], // G4 (回落)
  // ── 第三句（呼应句） ─────────────────────────────────────
  [4, 0.5, 0.8],  // D5
  [4, 0.5, 0.7],  // D5
  [4, 0.5, 0.85], // D5
  [2, 0.5, 0.8],  // B4
  [4, 0.5, 0.9],  // D5
  [5, 1.0, 0.9],  // E5 (长)
  [3, 0.5, 0.8],  // C5
  [2, 0.5, 0.75], // B4
  [1, 0.5, 0.85], // A4
  [0, 0.5, 0.8],  // G4
  [2, 0.5, 0.9],  // B4
  [0, 1.5, 0.85], // G4 (结尾)
];

// 低音伴奏（G-D 交替的进行曲 bass）
// [频率索引, 时值, 力度]
const _TH_BASS = [
  [12, 1.0, 0.7],  // G3
  [13, 1.0, 0.6],  // D4
  [12, 1.0, 0.7],  // G3
  [13, 1.0, 0.6],  // D4
  [12, 1.0, 0.7],
  [14, 1.0, 0.6],  // B3
  [12, 1.5, 0.7],
];

function startClownMusic() {
  if (_clownMusicActive) return;
  _clownMusicActive = true;
  try {
    const ctx = getAudioContext();
    const masterGain = ctx.createGain();
    masterGain.gain.value = 0;
    masterGain.connect(ctx.destination);
    _clownMusicGain = masterGain;

    // 渐入 1.2s
    masterGain.gain.linearRampToValueAtTime(
      0.25 * volumeSettings.master * volumeSettings.ambient,
      ctx.currentTime + 1.2
    );

    // ── 混响模拟（卷积混响用短反馈延迟替代） ──────────────────
    const reverbDelay = ctx.createDelay(0.08);
    reverbDelay.delayTime.value = 0.055;
    const reverbFb = ctx.createGain();
    reverbFb.gain.value = 0.25;
    const reverbOut = ctx.createGain();
    reverbOut.gain.value = 0.18;
    reverbDelay.connect(reverbFb);
    reverbFb.connect(reverbDelay);
    reverbDelay.connect(reverbOut);
    reverbOut.connect(masterGain);

    // ── 旋律层：八音盒/钢片琴音色（三角波 + 中频滤波） ────────
    const melodyGain = ctx.createGain();
    melodyGain.gain.value = 0.7;
    const melodyFilter = ctx.createBiquadFilter();
    melodyFilter.type = "peaking";
    melodyFilter.frequency.value = 2800;
    melodyFilter.gain.value = 5;    // 轻微提亮高频，更像钢片琴
    melodyFilter.Q.value = 1.5;
    melodyGain.connect(melodyFilter);
    melodyFilter.connect(masterGain);
    melodyFilter.connect(reverbDelay); // 同时送混响

    const beat = 0.22; // 6/8拍基准单位（较快的进行曲）

    function _scheduleMelodyLoop() {
      if (!_clownMusicActive) return;
      let t = ctx.currentTime + 0.05;
      _TH_MELODY.forEach(([idx, dur, vel]) => {
        if (!_clownMusicActive) return;
        const freq = _TH_FREQ[idx];
        // 微量失调 ±5 音分，保留小丑感但不破坏旋律
        const detune = (Math.random() - 0.5) * 10;

        // 主旋律音（三角波，八音盒感）
        const osc = ctx.createOscillator();
        osc.type = "triangle";
        osc.frequency.value = freq;
        osc.detune.value = detune;

        // 叠加八度泛音（正弦，轻微），更有质感
        const osc2 = ctx.createOscillator();
        osc2.type = "sine";
        osc2.frequency.value = freq * 2;
        osc2.detune.value = detune + 3;

        const noteGain = ctx.createGain();
        noteGain.gain.setValueAtTime(0, t);
        noteGain.gain.linearRampToValueAtTime(vel * 0.85, t + 0.008);
        noteGain.gain.exponentialRampToValueAtTime(0.001, t + dur * beat * 0.88);

        const note2Gain = ctx.createGain();
        note2Gain.gain.setValueAtTime(0, t);
        note2Gain.gain.linearRampToValueAtTime(vel * 0.2, t + 0.008);
        note2Gain.gain.exponentialRampToValueAtTime(0.001, t + dur * beat * 0.7);

        osc.connect(noteGain);   noteGain.connect(melodyGain);
        osc2.connect(note2Gain); note2Gain.connect(melodyGain);

        osc.start(t);  osc.stop(t + dur * beat);
        osc2.start(t); osc2.stop(t + dur * beat);
        _clownMusicNodes.push(osc, osc2);
        t += dur * beat;
      });

      const loopDur = _TH_MELODY.reduce((s, [, d]) => s + d, 0) * beat;
      _clownMusicScheduleTimeout = setTimeout(_scheduleMelodyLoop, (loopDur - 0.15) * 1000);
    }
    _scheduleMelodyLoop();

    // ── 低音伴奏层（正弦波，进行曲 bass） ─────────────────────
    const bassGain = ctx.createGain();
    bassGain.gain.value = 0.45;
    const bassFilter = ctx.createBiquadFilter();
    bassFilter.type = "lowpass";
    bassFilter.frequency.value = 320;
    bassFilter.Q.value = 0.7;
    bassGain.connect(bassFilter);
    bassFilter.connect(masterGain);

    function _scheduleBassLoop() {
      if (!_clownMusicActive) return;
      let t = ctx.currentTime + 0.05;
      // bass 循环长度匹配旋律
      const loopBars = Math.ceil(
        _TH_MELODY.reduce((s, [, d]) => s + d, 0) /
        _TH_BASS.reduce((s, [, d]) => s + d, 0)
      );
      for (let bar = 0; bar < loopBars; bar++) {
        _TH_BASS.forEach(([idx, dur, vel]) => {
          if (!_clownMusicActive) return;
          const freq = _TH_FREQ[idx];
          const osc = ctx.createOscillator();
          osc.type = "sine";
          osc.frequency.value = freq;
          const ng = ctx.createGain();
          ng.gain.setValueAtTime(0, t);
          ng.gain.linearRampToValueAtTime(vel * 0.7, t + 0.015);
          ng.gain.exponentialRampToValueAtTime(0.001, t + dur * beat * 0.75);
          osc.connect(ng);
          ng.connect(bassGain);
          osc.start(t);
          osc.stop(t + dur * beat);
          _clownMusicNodes.push(osc);
          t += dur * beat;
        });
      }
      const melodyDur = _TH_MELODY.reduce((s, [, d]) => s + d, 0) * beat;
      _clownMusicScheduleTimeout = setTimeout(_scheduleBassLoop, (melodyDur - 0.15) * 1000);
    }
    _scheduleBassLoop();

    // ── 进行曲鼓点层（规律的强拍噪音脉冲，6/8 拍感） ──────────
    // 每两拍打一下（模拟进行曲 bass drum + snare 交替）
    let _drumT = ctx.currentTime + beat * 0.5;
    const drumBarDur = beat * 6; // 一小节6拍
    function _scheduleDrumLoop() {
      if (!_clownMusicActive) return;
      const ctx2 = getAudioContext();
      const now2 = ctx2.currentTime;
      // 强拍（第1拍）：低频 bass drum
      [0, 3].forEach(offset => {
        const t2 = now2 + offset * beat;
        const bufLen = Math.floor(ctx2.sampleRate * 0.12);
        const buf = ctx2.createBuffer(1, bufLen, ctx2.sampleRate);
        const data = buf.getChannelData(0);
        for (let i = 0; i < bufLen; i++) {
          data[i] = (Math.random() * 2 - 1) * Math.exp(-i / (bufLen * 0.25));
        }
        const src = ctx2.createBufferSource();
        src.buffer = buf;
        const pf = ctx2.createBiquadFilter();
        pf.type = "lowpass";
        pf.frequency.value = offset === 0 ? 200 : 500;
        const pg = ctx2.createGain();
        pg.gain.value = offset === 0 ? 0.6 : 0.35;
        src.connect(pf); pf.connect(pg); pg.connect(masterGain);
        src.start(t2);
        _clownMusicNodes.push(src);
      });
      _clownMusicScheduleTimeout = setTimeout(_scheduleDrumLoop, drumBarDur * 1000);
    }
    _scheduleDrumLoop();

    // ── 背景颤音层（轻微失调三角波，制造小丑不安感） ───────────
    const wobbleOsc = ctx.createOscillator();
    wobbleOsc.type = "triangle";
    wobbleOsc.frequency.value = 392.0; // G4
    const wobbleLfo = ctx.createOscillator();
    wobbleLfo.type = "sine";
    wobbleLfo.frequency.value = 5.5;    // 快颤音
    const wobbleLfoGain = ctx.createGain();
    wobbleLfoGain.gain.value = 8;        // 轻微音高抖动
    wobbleLfo.connect(wobbleLfoGain);
    wobbleLfoGain.connect(wobbleOsc.frequency);
    const wobbleGain = ctx.createGain();
    wobbleGain.gain.value = 0.06;
    const wobbleFilter = ctx.createBiquadFilter();
    wobbleFilter.type = "highpass";
    wobbleFilter.frequency.value = 300;
    wobbleOsc.connect(wobbleGain);
    wobbleGain.connect(wobbleFilter);
    wobbleFilter.connect(masterGain);
    wobbleOsc.start(ctx.currentTime);
    wobbleLfo.start(ctx.currentTime);
    _clownMusicNodes.push(wobbleOsc, wobbleLfo);

  } catch (e) {}
}

function stopClownMusic() {
  if (!_clownMusicActive) return;
  _clownMusicActive = false;
  if (_clownMusicScheduleTimeout) {
    clearTimeout(_clownMusicScheduleTimeout);
    _clownMusicScheduleTimeout = null;
  }
  try {
    const ctx = getAudioContext();
    if (_clownMusicGain) {
      _clownMusicGain.gain.cancelScheduledValues(ctx.currentTime);
      _clownMusicGain.gain.setValueAtTime(_clownMusicGain.gain.value, ctx.currentTime);
      _clownMusicGain.gain.linearRampToValueAtTime(0.0, ctx.currentTime + 0.8);
    }
    setTimeout(() => {
      _clownMusicNodes.forEach(node => {
        try { node.stop(); } catch (e) {}
        try { node.disconnect(); } catch (e) {}
      });
      _clownMusicNodes = [];
      _clownMusicGain = null;
    }, 900);
  } catch (e) {
    _clownMusicNodes = [];
    _clownMusicGain = null;
  }
}

// 启动公路白噪音（ brown noise 低吟）
function startAmbient() {
  if (ambientStarted) return;
  try {
    const ctx = getAudioContext();
    const bufferSize = 2 * ctx.sampleRate;
    const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
    const data = buffer.getChannelData(0);
    let last = 0;
    for (let i = 0; i < bufferSize; i++) {
      const white = Math.random() * 2 - 1;
      last = 0.98 * last + 0.02 * white;
      data[i] = last * 0.3;
    }
    const source = ctx.createBufferSource();
    source.buffer = buffer;
    source.loop = true;
    ambientGain = ctx.createGain();
    ambientGain.gain.value = getAmbientVolume();
    source.connect(ambientGain);
    ambientGain.connect(ctx.destination);
    source.start(0);
    ambientNode = source;
    ambientStarted = true;
  } catch (e) {}
}

// 停止环境音
function stopAmbient() {
  if (!ambientNode) return;
  try {
    ambientNode.stop();
    ambientNode = null;
    ambientStarted = false;
  } catch (e) {}
}

// 汽车引擎声（低频嗡鸣，皮卡移动时循环播放）
function startEngineSound() {
  if (engineStarted) return;
  try {
    const ctx = getAudioContext();
    const mainGain = ctx.createGain();
    mainGain.gain.value = getEngineVolume();
    engineGain = mainGain;
    const filter = ctx.createBiquadFilter();
    filter.type = "lowpass";
    filter.frequency.value = 200;
    filter.Q.value = 0.5;
    mainGain.connect(filter);
    filter.connect(ctx.destination);
    const freqs = [55, 62, 73];
    freqs.forEach((f, i) => {
      const osc = ctx.createOscillator();
      osc.type = "sawtooth";
      osc.frequency.setValueAtTime(f, ctx.currentTime);
      osc.detune.setValueAtTime((i - 1) * 3, ctx.currentTime);
      const oscGain = ctx.createGain();
      oscGain.gain.value = 0.4;
      osc.connect(oscGain);
      oscGain.connect(mainGain);
      osc.start(ctx.currentTime);
      engineNodes.push({ osc });
    });
    engineStarted = true;
  } catch (e) {}
}

function stopEngineSound() {
  if (!engineStarted) return;
  try {
    engineNodes.forEach(({ osc }) => {
      try { osc.stop(); } catch (e) {}
    });
    engineNodes = [];
    engineGain = null;
    engineStarted = false;
  } catch (e) {}
}

// 绑定按钮音效（事件委托，动态添加的按钮也会生效）
function setupButtonSounds() {
  document.addEventListener("click", (e) => {
    const btn = e.target.closest("button");
    if (btn && !btn.disabled) playClick();
  });
  document.addEventListener("mouseover", (e) => {
    const btn = e.target.closest("button");
    if (btn && !btn.disabled && (!e.relatedTarget || !btn.contains(e.relatedTarget))) playHover();
  });
}

// 初始化（首次用户交互时调用）
function initGameAudio() {
  loadVolumeSettings();
  setupButtonSounds();
  startAmbient();
  startEngineSound();
}

// ─── 设置面板 ─────────────────────────────────────
function openSettingsModal() {
  const modal = document.getElementById("settings-modal");
  if (!modal) return;
  modal.classList.remove("hidden");
  // 同步滑块到当前值
  const masterSlider = document.getElementById("master-vol-slider");
  const ambientSlider = document.getElementById("ambient-vol-slider");
  const sfxSlider = document.getElementById("sfx-vol-slider");
  if (masterSlider) masterSlider.value = Math.round(volumeSettings.master * 100);
  if (ambientSlider) ambientSlider.value = Math.round(volumeSettings.ambient * 100);
  if (sfxSlider) sfxSlider.value = Math.round(volumeSettings.sfx * 100);
  document.getElementById("master-vol-label").textContent = Math.round(volumeSettings.master * 100) + "%";
  document.getElementById("ambient-vol-label").textContent = Math.round(volumeSettings.ambient * 100) + "%";
  document.getElementById("sfx-vol-label").textContent = Math.round(volumeSettings.sfx * 100) + "%";
  // 同步 debug 开关
  const debugToggle = document.getElementById("debug-toggle");
  if (debugToggle) debugToggle.checked = _debugMode;
  // Debug 区域：仅在简单模式 OR 管理员权限激活时显示
  const debugSection = document.getElementById("debug-mode-section");
  if (debugSection) {
    const isEasy = typeof gameState !== "undefined" && gameState.easyMode === true;
    if (isEasy || _debugFromAdminPermit) {
      debugSection.classList.remove("hidden");
    } else {
      debugSection.classList.add("hidden");
    }
  }
}

function closeSettingsModal() {
  const modal = document.getElementById("settings-modal");
  if (modal) modal.classList.add("hidden");
}

function onMasterVolumeChange(val) {
  volumeSettings.master = parseInt(val) / 100;
  document.getElementById("master-vol-label").textContent = val + "%";
  applyVolumeToNodes();
  saveVolumeSettings();
}

function onAmbientVolumeChange(val) {
  volumeSettings.ambient = parseInt(val) / 100;
  document.getElementById("ambient-vol-label").textContent = val + "%";
  applyVolumeToNodes();
  saveVolumeSettings();
}

function onSfxVolumeChange(val) {
  volumeSettings.sfx = parseInt(val) / 100;
  document.getElementById("sfx-vol-label").textContent = val + "%";
  saveVolumeSettings();
}

// 重新开始（两步确认）
function confirmRestart() {
  const btn = document.getElementById("settings-restart-btn");
  if (!btn) return;
  if (btn.dataset.confirming === "true") {
    // 第二次点击：删除存档并跳转
    if (typeof deleteSave === "function") deleteSave();
    window.location.href = "index.html";
  } else {
    // 第一次点击：变为确认状态
    btn.dataset.confirming = "true";
    btn.innerHTML = "⚠️ 确认放弃本局？再次点击确认";
    btn.classList.remove("border-red-900/60", "text-red-400", "hover:bg-red-900/30", "hover:border-red-700");
    btn.classList.add("border-red-600", "text-red-300", "bg-red-900/40", "hover:bg-red-800/60");
    // 3秒后自动恢复
    setTimeout(() => {
      if (btn.dataset.confirming === "true") {
        btn.dataset.confirming = "";
        btn.innerHTML = "🔄 重新开始";
        btn.classList.add("border-red-900/60", "text-red-400", "hover:bg-red-900/30", "hover:border-red-700");
        btn.classList.remove("border-red-600", "text-red-300", "bg-red-900/40", "hover:bg-red-800/60");
      }
    }, 3000);
  }
}

// ─── Debug 模式 ─────────────────────────────────────

let _debugMode = false;
let _debugSelectedEventId = null;
let _debugSelectedItemId = null;
let _debugCollapsed = false;
// 标记当前 debug 是否由"一次性管理员权限"激活，激活时设置页 toggle 禁用
let _debugFromAdminPermit = false;

function toggleDebugMode(enabled, fromAdmin) {
  // 若当前是管理员权限模式，手动调用关闭（非程序调用）时先检查
  // fromAdmin === undefined 表示来自设置页 toggle，此时若已锁定则禁止
  if (!enabled && fromAdmin === undefined && _debugFromAdminPermit) {
    // 手动尝试关闭：不阻止，但同样清除标志（用户自己关掉没问题）
    _debugFromAdminPermit = false;
    _updateDebugToggleUI(false);
  }
  // 若当前非 admin 模式，手动尝试开启（fromAdmin 未传）：正常处理
  // 若 fromAdmin === true，标记为管理员权限模式
  if (enabled && fromAdmin === true) {
    _debugFromAdminPermit = true;
  } else if (!enabled) {
    _debugFromAdminPermit = false;
  }

  _debugMode = enabled;
  // debug 状态不持久化到 localStorage（简单模式下手动开关、admin permit 激活均不跨页面保存）
  const panel = document.getElementById("debug-panel");
  if (panel) {
    if (enabled) {
      // 开启：清除可能残留的淡出动画，立即显示
      panel.style.animation = "";
      panel.style.opacity = "";
      panel.classList.remove("hidden");
      // 标记此次由 admin 权限激活，关闭时触发淡出
      panel._closedByAdmin = (fromAdmin === true);
      // admin permit 模式：数量上限改为 3
      const qtyInput = document.getElementById("debug-item-qty");
      if (qtyInput) {
        if (fromAdmin === true) {
          qtyInput.max = "3";
          if (parseInt(qtyInput.value) > 3) qtyInput.value = "3";
        } else {
          qtyInput.max = "99";
        }
      }
    } else {
      // 关闭：若由 admin 权限激活，播放淡出动画
      if (panel._closedByAdmin) {
        _hideDebugPanelWithFade(panel);
      } else {
        panel.classList.add("hidden");
        panel.style.animation = "";
      }
      // 关闭后恢复数量输入框上限
      const qtyInput = document.getElementById("debug-item-qty");
      if (qtyInput) { qtyInput.max = "99"; qtyInput.value = "1"; }
      panel._closedByAdmin = false;
      _debugSelectedEventId = null;
      _debugSelectedItemId = null;
    }
  }
  _updateDebugToggleUI(enabled);
}

// debug 面板淡出后隐藏（admin 权限失效时使用）
// admin permit 令牌使用一次后立即自毁：移除物品 + 关闭 debug（淡出）
function _triggerAdminPermitSelfDestruct() {
  // 从背包中移除一次性管理员权限
  if (typeof removeItem === "function") {
    removeItem("一次性管理员权限", 1);
  } else if (typeof inventoryState !== "undefined" && inventoryState.items) {
    const idx = inventoryState.items.findIndex(s => s.id === "一次性管理员权限");
    if (idx !== -1) inventoryState.items.splice(idx, 1);
  }
  if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

  // 打印提示到 textArea
  const textArea = document.getElementById("textArea");
  if (textArea) {
    const p = document.createElement("p");
    p.innerHTML = `<span style="color:#6b7280">📡 「一次性管理员权限」令牌检测到 Debug 操作已执行，已自我销毁。Debug 控制台随之关闭。</span>`;
    textArea.appendChild(p);
    if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
  }

  // 关闭 debug（触发淡出动画）
  toggleDebugMode(false);
}

function _hideDebugPanelWithFade(panel) {
  if (!panel || panel.dataset.closing) return;
  panel.dataset.closing = "1";
  panel.style.animation = "eventModalFadeOut 0.4s ease-in both";
  let done = false;
  const finish = () => {
    if (done) return;
    done = true;
    delete panel.dataset.closing;
    panel.classList.add("hidden");
    panel.style.animation = "";
    panel.style.opacity = "";
  };
  panel.addEventListener("animationend", finish, { once: true });
  setTimeout(finish, 500); // 兜底
}

// 更新设置页 debug toggle 的 UI 状态（选中/禁用）
function _updateDebugToggleUI(enabled) {
  const toggle = document.getElementById("debug-toggle");
  const toggleHint = document.getElementById("debug-toggle-hint");
  const debugSection = document.getElementById("debug-mode-section");
  if (!toggle) return;
  toggle.checked = enabled;

  // debug 区域整体显隐：简单模式 或 管理员权限激活时显示
  if (debugSection) {
    const isEasy = typeof gameState !== "undefined" && gameState.easyMode === true;
    if (isEasy || _debugFromAdminPermit) {
      debugSection.classList.remove("hidden");
    } else {
      debugSection.classList.add("hidden");
    }
  }

  if (_debugFromAdminPermit) {
    // 管理员权限模式：禁用 toggle，显示提示
    toggle.disabled = true;
    toggle.closest("label") && (toggle.closest("label").style.opacity = "0.4");
    toggle.closest("label") && (toggle.closest("label").style.cursor = "not-allowed");
    if (toggleHint) {
      toggleHint.textContent = "⚠️ 当前由「一次性管理员权限」控制，使用一次后自动关闭";
      toggleHint.style.color = "#22d3ee";
    }
  } else {
    // 正常模式：恢复可用
    toggle.disabled = false;
    toggle.closest("label") && (toggle.closest("label").style.opacity = "");
    toggle.closest("label") && (toggle.closest("label").style.cursor = "");
    if (toggleHint) {
      toggleHint.textContent = "开启后可在左下角手动触发任意事件";
      toggleHint.style.color = "";
    }
  }
}

// 供设置页 toggle onchange 调用：若处于管理员权限锁定状态则阻止手动开启
function onDebugToggleChange(checkbox) {
  if (checkbox.checked && _debugFromAdminPermit) {
    // 已由管理员权限激活，不允许再次手动打开（实际上此时 toggle 已 disabled，这是双重保险）
    checkbox.checked = true;
    return;
  }
  if (!checkbox.checked && _debugFromAdminPermit) {
    // 用户主动关闭 admin 模式：允许，并解除锁定
    _debugFromAdminPermit = false;
    _updateDebugToggleUI(false);
  }
  toggleDebugMode(checkbox.checked);
}

function toggleDebugCollapse() {
  _debugCollapsed = !_debugCollapsed;
  const body = document.getElementById("debug-body");
  const icon = document.getElementById("debug-collapse-icon");
  if (body) body.style.display = _debugCollapsed ? "none" : "";
  if (icon) {
    icon.textContent = _debugCollapsed ? "▶" : "▼";
  }
}

function _positionDebugDropdown(inputId, listId) {
  const input = document.getElementById(inputId);
  const list = document.getElementById(listId);
  if (!input || !list) return;
  const rect = input.getBoundingClientRect();
  // 先暂时显示以获取滚动高度，然后重新隐藏（如果本来是隐藏的）
  const wasHidden = list.classList.contains("hidden");
  if (wasHidden) {
    list.style.visibility = "hidden";
    list.classList.remove("hidden");
  }
  const listH = Math.min(parseInt(list.style.maxHeight || "260", 10) || 260, list.scrollHeight || 260);
  if (wasHidden) {
    list.classList.add("hidden");
    list.style.visibility = "";
  }
  const spaceAbove = rect.top;
  const spaceBelow = window.innerHeight - rect.bottom;
  list.style.width = rect.width + "px";
  list.style.left = rect.left + "px";
  if (spaceAbove >= listH + 8 && spaceAbove >= spaceBelow) {
    // 向上展开
    list.style.top = (rect.top - listH - 4) + "px";
    list.style.bottom = "";
  } else {
    // 向下展开
    list.style.top = (rect.bottom + 4) + "px";
    list.style.bottom = "";
  }
}

function onDebugSearchInput(query) {
  const listEl = document.getElementById("debug-event-list");
  if (!listEl) return;

  const allEvents = typeof getAllEvents === "function" ? getAllEvents() : {};
  const inventoryEvents = typeof INVENTORY_EVENTS !== "undefined" ? INVENTORY_EVENTS : {};
  const rareEvents = typeof EVENTS_RARE !== "undefined" ? EVENTS_RARE : {};
  const merged = Object.assign({}, allEvents, inventoryEvents, rareEvents);
  const entries = Object.entries(merged);

  const q = (query || "").trim().toLowerCase();

  // 过滤匹配
  const filtered = entries.filter(([id, evt]) => {
    if (!q) return true;
    return id.toLowerCase().includes(q) ||
           (evt.title && evt.title.toLowerCase().includes(q)) ||
           (evt.image && evt.image.includes(q));
  });

  if (filtered.length === 0) {
    listEl.innerHTML = '<div class="px-2.5 py-2 text-gray-600 text-xs">无匹配事件</div>';
    _positionDebugDropdown("debug-event-search", "debug-event-list");
    listEl.classList.remove("hidden");
    return;
  }

  // 最多显示50个
  const shown = filtered.slice(0, 50);
  listEl.innerHTML = shown.map(([id, evt]) => {
    const isOneTime = evt.oneTime ? '<span class="text-amber-600 ml-1">⚡一次</span>' : '';
    const isSelected = id === _debugSelectedEventId ? 'bg-purple-900/50 border-l-2 border-purple-400' : 'hover:bg-[#1a1a2e]';
    return `<div class="px-2.5 py-1.5 cursor-pointer text-xs transition-colors ${isSelected} border-b border-gray-800/50"
                 onclick="debugSelectEvent('${id}')">
              <div class="flex items-center gap-1">
                <span>${evt.image || '📋'}</span>
                <span class="text-gray-300 font-medium truncate">${evt.title || id}</span>
                ${isOneTime}
              </div>
              <div class="text-gray-600 text-[10px] mt-0.5 truncate">${id}</div>
            </div>`;
  }).join("");

  if (filtered.length > 50) {
    listEl.innerHTML += `<div class="px-2.5 py-1.5 text-gray-600 text-[10px]">…还有 ${filtered.length - 50} 个结果，请缩小搜索范围</div>`;
  }

  _positionDebugDropdown("debug-event-search", "debug-event-list");
  listEl.classList.remove("hidden");
}

// 点击事件列表外部时关闭
document.addEventListener("click", (e) => {
  const listEl = document.getElementById("debug-event-list");
  const searchEl = document.getElementById("debug-event-search");
  if (!listEl || !searchEl) return;
  if (!searchEl.contains(e.target) && !listEl.contains(e.target)) {
    listEl.classList.add("hidden");
  }
});

function debugSelectEvent(eventId) {
  _debugSelectedEventId = eventId;

  const allEvents = typeof getAllEvents === "function" ? getAllEvents() : {};
  const inventoryEvents = typeof INVENTORY_EVENTS !== "undefined" ? INVENTORY_EVENTS : {};
  const rareEvents = typeof EVENTS_RARE !== "undefined" ? EVENTS_RARE : {};
  const merged = Object.assign({}, allEvents, inventoryEvents, rareEvents);
  const evt = merged[eventId];

  const infoEl = document.getElementById("debug-selected-info");
  const btn = document.getElementById("debug-trigger-btn");
  const listEl = document.getElementById("debug-event-list");
  const searchEl = document.getElementById("debug-event-search");

  if (infoEl && evt) {
    infoEl.innerHTML = `<span class="text-purple-400">${evt.image || '📋'} ${evt.title || eventId}</span> <span class="text-gray-600">(${eventId})</span>`;
  }
  if (btn) btn.disabled = false;
  if (listEl) listEl.classList.add("hidden");
  if (searchEl) searchEl.value = evt ? (evt.title || eventId) : eventId;
}

function debugTriggerEvent() {
  if (!_debugSelectedEventId) return;

  const allEvents = typeof getAllEvents === "function" ? getAllEvents() : {};
  const inventoryEvents = typeof INVENTORY_EVENTS !== "undefined" ? INVENTORY_EVENTS : {};
  const rareEvents = typeof EVENTS_RARE !== "undefined" ? EVENTS_RARE : {};
  const merged = Object.assign({}, allEvents, inventoryEvents, rareEvents);
  const evt = merged[_debugSelectedEventId];

  if (!evt) {
    const infoEl = document.getElementById("debug-selected-info");
    if (infoEl) infoEl.innerHTML = '<span class="text-red-400">❌ 事件不存在</span>';
    return;
  }

  // 如果当前正在事件中，先清理
  if (gameState.eventTriggered) {
    const existingModals = ["event-modal", "sub-choice-modal", "rest-modal", "crafting-modal", "merchant-modal", "minesweeper-modal"];
    existingModals.forEach(id => {
      const el = document.getElementById(id);
      if (el) el.remove();
    });
    gameState.eventTriggered = false;
    if (typeof clearEventTheme === "function") clearEventTheme();
  }

  // 直接触发事件
  if (typeof triggerEvent === "function") {
    triggerEvent(evt);
  }

  // 记录 debug 使用并检查成就
  if (typeof gameState !== "undefined") {
    gameState.debugUsed = true;
    playDebugAction();
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
  }

  // admin permit 模式：使用一次后立即自毁并关闭 debug
  if (_debugFromAdminPermit) {
    _triggerAdminPermitSelfDestruct();
  }

  const infoEl = document.getElementById("debug-selected-info");
  if (infoEl) infoEl.innerHTML = `<span class="text-green-400">✓ 已触发 ${evt.image || ''} ${evt.title || _debugSelectedEventId}</span>`;
}

// ── Debug：物品搜索 ──────────────────────────────────
function onDebugItemSearchInput(query) {
  const listEl = document.getElementById("debug-item-list");
  if (!listEl) return;

  const cfg = typeof ITEMS_CONFIG !== "undefined" ? ITEMS_CONFIG : {};
  const entries = Object.entries(cfg);
  const q = (query || "").trim().toLowerCase();

  const catLabels = { consumable: "消耗品", material: "材料", treasure: "珍品", special: "特殊" };
  const catColors = { consumable: "#4ade80", material: "#9ca3af", treasure: "#f59e0b", special: "#facc15" };

  const filtered = entries.filter(([id]) =>
    !q || id.toLowerCase().includes(q) || id.includes(query.trim())
  );

  if (filtered.length === 0) {
    listEl.innerHTML = '<div class="px-2.5 py-2 text-gray-600 text-xs">无匹配物品</div>';
    _positionDebugDropdown("debug-item-search", "debug-item-list");
    listEl.classList.remove("hidden");
    return;
  }

  const shown = filtered.slice(0, 60);
  listEl.innerHTML = shown.map(([id, item]) => {
    const catLabel = catLabels[item.category] || item.category;
    const catColor = catColors[item.category] || "#9ca3af";
    const isSelected = id === _debugSelectedItemId ? "bg-green-900/40 border-l-2 border-green-400" : "hover:bg-[#0a1a0a]";
    return `<div class="px-2.5 py-1.5 cursor-pointer text-xs transition-colors ${isSelected} border-b border-gray-800/50"
                 onclick="debugSelectItem('${id}')">
              <div class="flex items-center gap-1.5">
                <span style="color:${item.color || '#e5e5e5'};font-weight:bold;">${item.name || id}</span>
                <span style="color:${catColor};font-size:10px;">${catLabel}</span>
              </div>
              <div class="text-gray-600 text-[10px] mt-0.5">${item.weight}kg${item.description ? " · " + item.description.slice(0, 30) + (item.description.length > 30 ? "…" : "") : ""}</div>
            </div>`;
  }).join("");

  if (filtered.length > 60) {
    listEl.innerHTML += `<div class="px-2.5 py-1.5 text-gray-600 text-[10px]">…还有 ${filtered.length - 60} 个结果</div>`;
  }

  _positionDebugDropdown("debug-item-search", "debug-item-list");
  listEl.classList.remove("hidden");
}

function debugSelectItem(itemId) {
  _debugSelectedItemId = itemId;
  const cfg = typeof ITEMS_CONFIG !== "undefined" ? ITEMS_CONFIG : {};
  const item = cfg[itemId];

  const infoEl = document.getElementById("debug-item-info");
  const btn = document.getElementById("debug-add-item-btn");
  const listEl = document.getElementById("debug-item-list");
  const searchEl = document.getElementById("debug-item-search");

  if (infoEl && item) {
    const catLabels = { consumable: "消耗品", material: "材料", treasure: "珍品", special: "特殊" };
    infoEl.innerHTML = `<span style="color:${item.color || '#e5e5e5'}">${item.name || itemId}</span> <span class="text-gray-600">(${catLabels[item.category] || item.category}，${item.weight}kg)</span>`;
  }
  if (btn) btn.disabled = false;
  if (listEl) listEl.classList.add("hidden");
  if (searchEl) searchEl.value = item ? (item.name || itemId) : itemId;
}

function debugAddItem() {
  if (!_debugSelectedItemId) return;

  const cfg = typeof ITEMS_CONFIG !== "undefined" ? ITEMS_CONFIG : {};
  const item = cfg[_debugSelectedItemId];
  if (!item) {
    const infoEl = document.getElementById("debug-item-info");
    if (infoEl) infoEl.innerHTML = '<span class="text-red-400">❌ 物品不存在</span>';
    return;
  }

  const qtyInput = document.getElementById("debug-item-qty");
  let qty = parseInt(qtyInput ? qtyInput.value : "1", 10);
  if (isNaN(qty) || qty < 1) qty = 1;
  // admin permit 激活时每次最多添加 3 个
  const maxQty = _debugFromAdminPermit ? 3 : 99;
  if (qty > maxQty) {
    qty = maxQty;
    if (qtyInput) qtyInput.value = maxQty;
  }

  if (typeof addItem === "function") {
    addItem(_debugSelectedItemId, qty);
  } else if (typeof inventoryState !== "undefined") {
    // 降级：直接操作 inventoryState
    for (let i = 0; i < qty; i++) {
      const existing = inventoryState.items.find(s => s.id === _debugSelectedItemId && item.stackable);
      if (existing) {
        existing.qty = (existing.qty || 1) + 1;
      } else {
        inventoryState.items.push({ id: _debugSelectedItemId, qty: 1 });
      }
    }
  }

  if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

  // 记录 debug 使用并检查成就
  if (typeof gameState !== "undefined") {
    gameState.debugUsed = true;
    playDebugAction();
    // 通过 debug 添加「一次性管理员权限」时，解锁贪婪成就 + 更新 peakCount
    if (_debugSelectedItemId === "一次性管理员权限") {
      gameState.debugAddedAdminPermit = true;
      // debugAddItem 直接调用 addItem，addItem 内已有 peakCount 更新逻辑
      // 此处作为兜底：确保 peakCount 已被正确更新
      if (typeof getItemQuantity === "function") {
        const currentQty = getItemQuantity("一次性管理员权限");
        if (currentQty > (gameState.adminPermitPeakCount || 0)) {
          gameState.adminPermitPeakCount = currentQty;
        }
      }
    }
    if (typeof checkAndUnlockAchievements === "function") checkAndUnlockAchievements();
  }

  // admin permit 模式：使用一次后立即自毁并关闭 debug
  if (_debugFromAdminPermit) {
    _triggerAdminPermitSelfDestruct();
    return; // 自毁后 infoEl 已不可见，跳过后续更新
  }

  const infoEl = document.getElementById("debug-item-info");
  if (infoEl) {
    infoEl.innerHTML = `<span class="text-green-400">✓ 已添加 <span style="color:${item.color || '#e5e5e5'}">${item.name || _debugSelectedItemId}</span> ×${qty}</span>`;
  }
}

// 点击物品列表外部时关闭物品下拉
document.addEventListener("click", (e) => {
  const listEl = document.getElementById("debug-item-list");
  const searchEl = document.getElementById("debug-item-search");
  if (!listEl || !searchEl) return;
  if (!searchEl.contains(e.target) && !listEl.contains(e.target)) {
    listEl.classList.add("hidden");
  }
});
