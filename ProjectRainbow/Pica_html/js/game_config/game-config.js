// 游戏全局配置文件
// 集中管理游戏的各种配置参数

const GAME_CONFIG = {
  // 事件触发配置
  triggerInterval: 2, // 每2行文本触发一次事件
  textSpeed: 1500, // 文本生成间隔（毫秒）

  // 事件调度配置
  overnightRestInterval: 25, // 每隔多少km强制触发一次昼夜休息
  recentEventWindow: 4, // 最近N次事件内不重复同一事件
  maxRestPerCycle: 3, // 每个昼夜间隔内最多触发多少次休息类事件

  // 燃油消耗配置
  fuelConsumptionPer5km: 1.2, // 每5km消耗多少燃油
  // 初始游戏状态配置
  initialGold: 6, // 初始金币数量
  // Cookie配置
  cookieExpiryDays: 3650, // 10年
  cookieName: "chinese_truck_adventure_save",

  // 动画时长配置（毫秒）
  animation: {
    roadDeceleration: 300,
    charSlideIn: 400, // 字滑入 0.4秒
    charStay: 1000,
  },

  // 公路配置
  road: {
    charsPool: ["马", "鹿"], // 公路字符池
    maxLength: 80, // 屏幕显示的字符数量
    updateSpeed: 300,
  },
};

// ─── 困难模式配置 ──────────────────────────────────────────
// 解锁条件：玩家达成过任意结局（localStorage 中有 endings 记录）
// 每个修饰符有 id、名称、描述、分值、以及每次事件选择后的 effect
const HARD_MODE_MODIFIERS = [
  {
    id: "uncomfortable_cabin",
    name: "不舒服的车厢",
    description: "每次事件抉择后舒适 -1%",
    points: 2,
    perChoice: { comfort: -1 },
  },
  {
    id: "rusty_frame",
    name: "生锈的车架",
    description: "每次事件抉择后耐久 -1%",
    points: 2,
    perChoice: { durability: -1 },
  },
  {
    id: "leaky_tank",
    name: "漏油的油箱",
    description: "每次事件抉择后燃油 -1%",
    points: 2,
    perChoice: { fuel: -1 },
  },
  {
    id: "heavy_cargo",
    name: "沉重的货物",
    description: "后备箱最大载重 -15kg",
    points: 3,
    onStart: { maxWeight: -15 },
  },
  {
    id: "empty_pockets",
    name: "身无分文",
    description: "初始金币归零",
    points: 2,
    onStart: { gold: -999 },
  },
  {
    id: "fragile_body",
    name: "脆弱车身",
    description: "初始耐久降为 60%",
    points: 3,
    onStart: { durability: -40 },
  },
  {
    id: "low_fuel_start",
    name: "油量告急",
    description: "初始燃油降为 50%",
    points: 3,
    onStart: { fuel: -50 },
  },
  {
    id: "gloomy_mood",
    name: "阴郁氛围",
    description: "初始舒适降为 60%",
    points: 2,
    onStart: { comfort: -40 },
  },
  {
    id: "bad_luck",
    name: "霉运缠身",
    description: "每次事件抉择后 10% 概率随机损失一项属性 5%",
    points: 4,
    perChoiceRandom: { chance: 0.1, amount: -5 },
  },
  {
    id: "triple_drain",
    name: "全面衰退",
    description: "每次事件抉择后燃油/耐久/舒适各 -1%",
    points: 5,
    perChoice: { fuel: -1, durability: -1, comfort: -1 },
  },
];

// ─── 全局衰变 debuff 配置 ──────────────────────────────────
// 每经历 10 个事件选择获得一个随机 debuff；层数可叠加
// 可在难度选择界面关闭（关闭后视为简单模式）
const DECAY_DEBUFF_INTERVAL = 10; // 每隔多少个事件选择触发一次 debuff

const DECAY_DEBUFFS = [
  {
    id: "tank_aging",
    name: "油箱老化",
    icon: "⛽",
    description: "每次事件抉择后，燃油 -1%（每层叠加）",
    perChoice: { fuel: -1 },
  },
  {
    id: "frame_corrosion",
    name: "车架腐蚀",
    icon: "🔩",
    description: "每次事件抉择后，耐久 -1%（每层叠加）",
    perChoice: { durability: -1 },
  },
  {
    id: "passenger_fatigue",
    name: "旅途疲惫",
    icon: "😩",
    description: "每次事件抉择后，舒适 -1%（每层叠加）",
    perChoice: { comfort: -1 },
  },
  {
    id: "wallet_leak",
    name: "钱包漏洞",
    icon: "💸",
    description: "每次事件抉择后，25% 概率金币 -1（每层叠加概率）",
    perChoiceChance: 0.25,
    perChoice: { gold: -1 },
  },
  {
    id: "heavy_burden",
    name: "沉重负担",
    icon: "📦",
    description: "获得时最大载重 -3kg（每层叠加）",
    onApply: { maxWeight: -3 },
  },
  {
    id: "deepening_misfortune",
    name: "霉运加深",
    icon: "🎲",
    description: "每次事件抉择后，15% 概率随机属性 -3%（每层叠加概率）",
    perChoiceRandom: { baseChance: 0.15, amount: -3 },
  },
  {
    id: "overall_aging",
    name: "全面老化",
    icon: "⏳",
    description: "每次事件抉择后，燃油/耐久/舒适各 -0.5%（每层叠加）",
    perChoice: { fuel: -0.5, durability: -0.5, comfort: -0.5 },
  },
  {
    id: "fuel_guzzler",
    name: "油耗飙升",
    icon: "🛢️",
    description: "每次事件抉择后，燃油 -2%（每层叠加）",
    perChoice: { fuel: -2 },
  },
  {
    id: "creaking_chassis",
    name: "嘎吱车身",
    icon: "🔧",
    description: "每次事件抉择后，耐久 -0.5%，舒适 -0.5%（每层叠加）",
    perChoice: { durability: -0.5, comfort: -0.5 },
  },
];

// ─── 困难模式加成配置 ──────────────────────────────────────
// 玩家选择难度标签获得分数后，可用分数兑换加成（加成总花费 ≤ 难度总分）
const HARD_MODE_BONUSES = [
  {
    id: "balance_stats",
    name: "取长补短",
    description: "每次事件抉择后，最高属性 -1%，最低属性 +1%",
    cost: 5,
    perChoice: true,
  },
  {
    id: "luck_boost",
    name: "概率平衡",
    description: "所有概率型效果（chance）触发概率 +15%，珍品掉率额外 ×1.3",
    cost: 5,
    passive: true,
  },
  {
    id: "treasure_collector",
    name: "珍品收藏家",
    description: "开局随机获得一件珍品",
    cost: 6,
    onStart: true,
  },
  {
    id: "trunk_incubator",
    name: "后备箱培养仓",
    description: "每次事件抉择后，15% 概率获得草药×1",
    cost: 8,
    perChoice: true,
  },
  {
    id: "member_card",
    name: "会员卡",
    description: "商店购买物品价格 -20%",
    cost: 6,
    passive: true,
  },
  {
    id: "better_start",
    name: "更好的开局",
    description: "开局后备箱额外获得 2~3 种随机基础物资（废金属、布料、草药、空罐、原油），每种 1~2 个",
    cost: 12,
    onStart: true,
  },
];

// ── 事件特性词条配置 ──
// 每个事件可通过 tags: ["夜晚", "雨天"] 标记词条
// 词条会显示在事件界面标题下方，同时影响珍品的被动效果触发
const EVENT_TAGS = {
  夜晚: {
    label: "夜晚",
    emoji: "🌙",
    color: "#a78bfa",
    bgColor: "#a78bfa18",
    borderColor: "#a78bfa40",
    description: "此事件发生在夜间",
  },
  雨天: {
    label: "雨天",
    emoji: "🌧️",
    color: "#60a5fa",
    bgColor: "#60a5fa18",
    borderColor: "#60a5fa40",
    description: "此事件伴随降雨",
  },
  视线模糊: {
    label: "视线模糊",
    emoji: "🌫️",
    color: "#94a3b8",
    bgColor: "#94a3b818",
    borderColor: "#94a3b840",
    description: "此事件中视线受阻",
  },
  危险: {
    label: "危险",
    emoji: "⚠️",
    color: "#f87171",
    bgColor: "#f8717118",
    borderColor: "#f8717140",
    description: "此事件存在高风险",
  },
  神秘: {
    label: "神秘",
    emoji: "🔮",
    color: "#c084fc",
    bgColor: "#c084fc18",
    borderColor: "#c084fc40",
    description: "此事件蕴含神秘力量",
  },
  商人: {
    label: "商人",
    emoji: "🪙",
    color: "#fbbf24",
    bgColor: "#fbbf2418",
    borderColor: "#fbbf2440",
    description: "此事件涉及交易",
  },
  废墟: {
    label: "废墟",
    emoji: "🏚️",
    color: "#a1887f",
    bgColor: "#a1887f18",
    borderColor: "#a1887f40",
    description: "此事件发生在废弃建筑中",
  },
};
