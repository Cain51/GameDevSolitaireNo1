// ═══════════════════════════════════════════════════════════
// 背景景观系统 - ASCII/汉字 构建行驶沿途远景
// ═══════════════════════════════════════════════════════════
// 在游戏画面中用暗色文字描绘远景（山、树林、城市、废墟、沙丘等）
// 多层视差滚动，随里程自动切换场景

// ─── 场景配置 ──────────────────────────────────────────────
const SCENERY_CONFIG = {
  // 滚动速度（像素/帧），远景慢 → 近景快，营造视差
  farSpeed: 3.6,
  midSpeed: 7.2,
  nearSpeed: 12.0,
  frameInterval: 50, // 帧间隔 ms
  sceneDuration: 2, // 每个场景持续多少次事件后可能切换（2~3次事件）
  sceneRandom: 2, // 随机额外 0~1 次事件
  transitionDuration: 1500, // 场景过渡时长 ms
};

// ─── 场景定义 ──────────────────────────────────────────────
// 每个场景包含 3 层（far / mid / near），每层是多行字符串
// 用全角字符（汉字）保持等宽对齐
// 颜色从远到近逐渐加深一点
const SCENERY_SCENES = {
  // ═══ 山脉 ═══
  mountain: {
    name: "远山",
    far: {
      color: "#1c1c32",
      lines: [
        "　　　　　　　　△　　　　　　　　　　　△△　　　　　　　　　　　　△　　　　　　　",
        "　　　　　　　△△△　　　　　　　　　△△△△　　　　　　　　　　△△△　　　　　　",
        "　　　　　　△△△△△　　　　　　　△△△△△△　　　　　　　　△△△△△　　　　　",
        "　　　　　△山△山△山△　　　　　△山△山△山△山△　　　　　△山△山△山△　　　　",
        "　　　　△山山山山山山山△　　　△山山山山山山山山山△　　　△山山山山山山山△　　　",
      ],
    },
    mid: {
      color: "#1a2848",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　丘　　　　　　丘　　　　　丘丘　　　　　　　丘　　　　　　丘　　　　　",
        "　　　丘丘丘丘　　　丘丘丘丘　　丘丘丘丘丘　　　　丘丘丘丘　　丘丘丘丘　　　　",
        "　丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘丘　",
      ],
    },
    near: {
      color: "#343460",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　石　　　　　　　　　石　　　　　　　石石　　　　　　　　石　　　　　　　",
        "石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石石",
      ],
    },
  },

  // ═══ 树林 ═══
  forest: {
    name: "密林",
    far: {
      color: "#0e3820",
      lines: [
        "　　　　杉　　　杉　　　　杉　　　杉　　　　杉　　　杉　　　　杉　　　杉　　　",
        "　　　杉杉杉　杉杉杉　　杉杉杉　杉杉杉　　杉杉杉　杉杉杉　　杉杉杉　杉杉杉　",
        "　　杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉　",
        "　杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉杉　",
        "林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林林",
      ],
    },
    mid: {
      color: "#144428",
      lines: [
        "　　　木　　　　木　　　木　　　　　木　　　　木　　　木　　　　　木　　　木　　",
        "　　木木木　　木木木　木木木　　　木木木　　木木木　木木木　　　木木木　木木木　　",
        "　木木木木木木木木木木木木木木　木木木木木木木木木木木木木木　木木木木木木木木木　",
        "　木干木干木木干木干木木干木干木木干木干木木干木干木木干木干木木干木干木干木干木　",
        "草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草草",
      ],
    },
    near: {
      color: "#28724c",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　树　　　　　　　　树　　　　　　树　　　　　　　　树　　　　　　　　树　　",
        "　　树干树　　　　　　树干树　　　　树干树　　　　　　树干树　　　　　　树干树　　",
        "灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌灌",
      ],
    },
  },

  // ═══ 城市 ═══
  city: {
    name: "城镇",
    far: {
      color: "#201a2a",
      lines: [
        "　　　厂　　口口　　　　厂厂　口口口　　　厂　　口口　　　　厂厂　口口口　　　",
        "　　厂厂厂口口口口　　厂厂厂口口口口口　　厂厂厂口口口口　　厂厂厂口口口口口　　",
        "　厂厂厂厂口口口口口厂厂厂厂口口口口口口厂厂厂厂口口口口口厂厂厂厂口口口口口口　",
        "　楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼楼　",
        "建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建建",
      ],
    },
    mid: {
      color: "#261e3a",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　宅　　　　店　　　　宅宅　　　　店　　　宅　　　　店　　　　宅宅　　　　店",
        "　　宅宅宅　　店店店　　宅宅宅宅　　店店店　宅宅宅　　店店店　　宅宅宅宅　　店店",
        "　宅宅门宅宅店店门店店宅宅宅门宅宅店店门店宅宅门宅宅店店门店店宅宅宅门宅宅店店",
        "路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路路",
      ],
    },
    near: {
      color: "#463868",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　灯　　　　　　　　灯　　　　　　灯　　　　　　　　灯　　　　　　灯　　",
        "　杆　　灯杆灯　　杆　　　灯杆灯　　杆　灯杆灯　　杆　　灯杆灯　　杆　灯杆灯　",
        "栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏栏",
      ],
    },
  },

  // ═══ 废墟 ═══
  ruins: {
    name: "废墟",
    far: {
      color: "#201c14",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　残　　　　　　残残　　　　　　　残　　　　残　　　　　残残　　　　　　残　",
        "　　　残残残　　　　残残残残　　　　　残残残　　残残残　　　残残残残　　　　残残残",
        "　　残残残残残　　残残残残残残　　　残残残残残残残残残残　残残残残残残　　残残残残",
        "墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟墟",
      ],
    },
    mid: {
      color: "#2a241a",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　裂　　　　　壁　　　　裂裂　　　　　　裂　　　　壁　　　　裂裂　　　　　",
        "　　　壁裂壁　　　壁壁壁　　壁裂壁壁　　　　壁裂壁　壁壁壁　　壁裂壁壁　　　　",
        "砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾砾",
      ],
    },
    near: {
      color: "#4a4430",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　柱　　　　　　柱　　　　　　柱　　　　　　柱　　　　　柱　　　　　　柱　　",
        "碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎碎",
      ],
    },
  },

  // ═══ 沙丘 ═══
  desert: {
    name: "荒漠",
    far: {
      color: "#262014",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　～　　　　　　　　　　～～　　　　　　　　　～　　　　　　　　",
        "　　　　　　～～～～～～　　　　　　～～～～～～～　　　　～～～～～～　　　　　",
        "沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙",
      ],
    },
    mid: {
      color: "#2e2818",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　丘　　　　　　　　　丘　　　　　　丘丘　　　　　　　　丘　　　　　　丘　　",
        "漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠漠",
      ],
    },
    near: {
      color: "#50482c",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　仙　　　　　　　　　　骨　　　　　　仙　　　　　　　　骨　　　　　　",
        "尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘尘",
      ],
    },
  },

  // ═══ 雪原 ═══
  snowfield: {
    name: "雪原",
    far: {
      color: "#1e2434",
      lines: [
        "　　　　　　　　雪　　　　　　　　　　　雪雪　　　　　　　　　　　雪　　　　　　",
        "　　　　　　　雪雪雪　　　　　　　　　雪雪雪雪　　　　　　　　　雪雪雪　　　　　",
        "　　　　　　雪雪雪雪雪　　　　　　　雪雪雪雪雪雪　　　　　　　雪雪雪雪雪　　　　",
        "　　　　　雪峰雪峰雪峰雪　　　　　雪峰雪峰雪峰雪峰雪　　　　雪峰雪峰雪峰雪　　",
        "　　　　雪雪雪雪雪雪雪雪雪　　　雪雪雪雪雪雪雪雪雪雪雪　　雪雪雪雪雪雪雪雪雪　",
      ],
    },
    mid: {
      color: "#242c40",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　冰　　　　　冰　　　　　　冰冰　　　　　　冰　　　　　冰　　　　　冰　",
        "　　　冰冰冰冰　　冰冰冰冰　　　冰冰冰冰冰　　　冰冰冰冰　　冰冰冰冰　　冰冰　",
        "白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白白",
      ],
    },
    near: {
      color: "#404870",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　※　　　　　　　　※　　　　　　※　　　　　　　　※　　　　　　　※　　",
        "霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜霜",
      ],
    },
  },

  // ═══ 海岸 ═══
  coast: {
    name: "海岸",
    far: {
      color: "#102036",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波波",
        "海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海",
        "海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海海",
      ],
    },
    mid: {
      color: "#142840",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "～～～浪～～～浪浪～～～浪～～～浪浪～～～浪～～～浪浪～～～浪～～～浪浪～～～浪",
        "潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮潮",
      ],
    },
    near: {
      color: "#28486c",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　礁　　　　　　　贝　　　　　　礁礁　　　　　贝　　　　　　礁　　　贝　　",
        "沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙沙",
      ],
    },
  },

  // ═══ 荒野（默认/开始场景） ═══
  plains: {
    name: "荒野",
    far: {
      color: "#18181e",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─",
      ],
    },
    mid: {
      color: "#202024",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　草　　　　　草　　　　　草草　　　　草　　　　　草　　　　　草草　　　　",
        "野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野野",
      ],
    },
    near: {
      color: "#383844",
      lines: [
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　",
        "土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土土",
      ],
    },
  },
};

// 场景顺序池（所有可用场景 key）
const SCENERY_KEYS = Object.keys(SCENERY_SCENES);

// ─── 运行时状态 ──────────────────────────────────────────
let sceneryState = {
  currentScene: "plains",
  nextScene: null,
  transitioning: false,
  transitionProgress: 0, // 0~1
  ticksSinceChange: 0,
  ticksToChange: SCENERY_CONFIG.sceneDuration + Math.floor(Math.random() * SCENERY_CONFIG.sceneRandom), // 2~3 次事件
  // 三个层的水平偏移量（像素）
  farOffset: 0,
  midOffset: 0,
  nearOffset: 0,
  // 缓存每个层的 block 宽度
  blockWidths: { far: 0, mid: 0, near: 0 },
};

let sceneryAnimFrame = null;
let sceneryLastTime = 0;
let sceneryPaused = false;

// ─── 初始化 ──────────────────────────────────────────────
function initScenery() {
  const container = document.getElementById("scenery");
  if (!container) return;

  // 创建三个层，各层用不同的 bottom 和 height 营造纵深
  // far  → 最高处，最暗最小；mid → 中间；near → 最低处，最亮最大
  const layerDefs = {
    far:  { bottom: "48%", height: "40%", zIndex: "1", fontSize: "12px", opacity: "0.4" },
    mid:  { bottom: "32%", height: "32%", zIndex: "2", fontSize: "13px", opacity: "0.55" },
    near: { bottom: "24%", height: "28%", zIndex: "3", fontSize: "15px", opacity: "0.85" },
  };

  for (const [layer, def] of Object.entries(layerDefs)) {
    const el = document.createElement("div");
    el.id = `scenery-${layer}`;
    el.className = "scenery-layer";
    el.dataset.fontSize = def.fontSize;
    el.style.cssText = `
      position: absolute;
      bottom: ${def.bottom};
      left: 0;
      width: 100%;
      height: ${def.height};
      overflow: hidden;
      white-space: nowrap;
      pointer-events: none;
      display: flex;
      align-items: flex-end;
      line-height: 1.15;
      z-index: ${def.zIndex};
      opacity: ${def.opacity};
    `;
    container.appendChild(el);
  }

  renderSceneryLayer("far");
  renderSceneryLayer("mid");
  renderSceneryLayer("near");
  startSceneryAnimation();
}

// ─── 渲染某一层 ──────────────────────────────────────────
function renderSceneryLayer(layer) {
  const el = document.getElementById(`scenery-${layer}`);
  if (!el) return;

  const scene = SCENERY_SCENES[sceneryState.currentScene];
  if (!scene || !scene[layer]) return;

  const data = scene[layer];
  const lines = data.lines;
  const color = data.color;
  const fontSize = el.dataset.fontSize || "14px";

  // 用两份拼接实现无缝循环
  const blockHtml = lines
    .map(
      (line) =>
        `<div style="white-space:pre;font-size:${fontSize};letter-spacing:0;">${line}</div>`,
    )
    .join("");

  el.innerHTML = `
    <div class="scenery-scroll-inner" style="display:inline-flex;color:${color};font-family:'Microsoft YaHei',monospace;will-change:transform;">
      <div class="scenery-block" style="flex-shrink:0;">${blockHtml}</div>
      <div class="scenery-block" style="flex-shrink:0;">${blockHtml}</div>
    </div>
  `;

  // 在下一帧缓存 block 宽度
  requestAnimationFrame(() => {
    const block = el.querySelector(".scenery-block");
    if (block) {
      sceneryState.blockWidths[layer] = block.offsetWidth;
    }
  });
}

// ─── 动画循环 ──────────────────────────────────────────────
function startSceneryAnimation() {
  sceneryLastTime = performance.now();
  sceneryPaused = false;
  animateScenery();
}

function animateScenery(now) {
  if (sceneryPaused) return;
  sceneryAnimFrame = requestAnimationFrame(animateScenery);

  if (!now) now = performance.now();
  const delta = now - sceneryLastTime;
  if (delta < SCENERY_CONFIG.frameInterval) return;
  sceneryLastTime = now;

  // 如果游戏暂停或事件中，不滚动
  if (typeof gameState !== "undefined" && gameState.eventTriggered) return;

  // 更新偏移
  sceneryState.farOffset -= SCENERY_CONFIG.farSpeed;
  sceneryState.midOffset -= SCENERY_CONFIG.midSpeed;
  sceneryState.nearOffset -= SCENERY_CONFIG.nearSpeed;

  // 应用偏移，用 translateX 让整个内容左移
  applySceneryOffset("far", sceneryState.farOffset);
  applySceneryOffset("mid", sceneryState.midOffset);
  applySceneryOffset("near", sceneryState.nearOffset);

  // 检查是否需要重置偏移（无缝循环）
  checkSceneryLoop("far");
  checkSceneryLoop("mid");
  checkSceneryLoop("near");
}

function applySceneryOffset(layer, offset) {
  const el = document.getElementById(`scenery-${layer}`);
  if (!el) return;
  const inner = el.querySelector(".scenery-scroll-inner");
  if (!inner) return;
  inner.style.transform = `translateX(${offset}px)`;
}

function checkSceneryLoop(layer) {
  const blockWidth = sceneryState.blockWidths[layer];
  if (!blockWidth || blockWidth <= 0) return;

  // 当第一个 block 完全移出视野，重置偏移
  const offsetKey = `${layer}Offset`;
  if (Math.abs(sceneryState[offsetKey]) >= blockWidth) {
    sceneryState[offsetKey] += blockWidth;
  }
}

// ─── 暂停 / 恢复 ──────────────────────────────────────────
function pauseScenery() {
  sceneryPaused = true;
  if (sceneryAnimFrame) {
    cancelAnimationFrame(sceneryAnimFrame);
    sceneryAnimFrame = null;
  }
}

function resumeScenery() {
  if (!sceneryPaused) return;
  sceneryPaused = false;
  sceneryLastTime = performance.now();
  animateScenery();
}

// ─── 场景切换 ──────────────────────────────────────────────
// 在 handleEventChoice 中调用，每次事件选择后递增
function sceneryTick() {
  sceneryState.ticksSinceChange++;
  if (sceneryState.ticksSinceChange >= sceneryState.ticksToChange) {
    // 切换到随机一个不同的场景
    changeScenery();
  }
}

function changeScenery(targetScene) {
  if (sceneryState.transitioning) return;

  // 选择一个不同的场景
  let next = targetScene;
  if (!next) {
    const candidates = SCENERY_KEYS.filter(
      (k) => k !== sceneryState.currentScene,
    );
    next = candidates[Math.floor(Math.random() * candidates.length)];
  }

  sceneryState.transitioning = true;
  sceneryState.nextScene = next;
  sceneryState.ticksSinceChange = 0;
  sceneryState.ticksToChange = SCENERY_CONFIG.sceneDuration + Math.floor(Math.random() * SCENERY_CONFIG.sceneRandom);

  const container = document.getElementById("scenery");
  if (!container) {
    sceneryState.transitioning = false;
    return;
  }

  const halfDur = SCENERY_CONFIG.transitionDuration / 2;

  // ① 淡出当前场景
  container.style.transition = `opacity ${halfDur}ms ease-out`;
  container.style.opacity = "0";

  setTimeout(() => {
    // ② 淡出完成后，切换场景数据（此时 opacity=0，用户看不见）
    sceneryState.currentScene = next;
    // 不重置 offset，保持滚动连贯（新场景从当前位置继续滚）
    renderSceneryLayer("far");
    renderSceneryLayer("mid");
    renderSceneryLayer("near");

    // ③ 等待 blockWidth 缓存完毕后再淡入（两帧确保布局完成）
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        // 重新缓存 blockWidth 并校正偏移，防止超出范围
        ["far", "mid", "near"].forEach((layer) => {
          const el = document.getElementById(`scenery-${layer}`);
          if (!el) return;
          const block = el.querySelector(".scenery-block");
          if (block) {
            const w = block.offsetWidth;
            sceneryState.blockWidths[layer] = w;
            // 校正偏移：确保不超过一个 block 宽度
            if (w > 0) {
              sceneryState[`${layer}Offset`] = sceneryState[`${layer}Offset`] % w;
            }
          }
        });

        // ④ 淡入新场景
        container.style.transition = `opacity ${halfDur}ms ease-in`;
        container.style.opacity = "1";

        setTimeout(() => {
          sceneryState.transitioning = false;
          container.style.transition = "";
        }, halfDur);
      });
    });
  }, halfDur);
}

// ─── 与公路同步的停止/恢复 ──────────────────────────────
// 在事件触发时暂停，事件结束时恢复
// 由 road.js 的 pauseRoad / resumeRoad 中调用
