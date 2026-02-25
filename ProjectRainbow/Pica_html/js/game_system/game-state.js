// 游戏状态管理模块
// 管理游戏的全局状态数据

// 乘客好感度范围 0–100，默认 50
const DEFAULT_FAVOR = 50;
const FAVOR_MIN = 0;
const FAVOR_MAX = 100;

let gameState = {
  textCount: 0, // 文本计数
  eventTriggered: false, // 是否正在事件中
  triggeredEvents: [], // 已触发的事件ID列表（一次性事件用）
  unlockedEvents: [], // 已解锁的事件ID列表
  recentEvents: [], // 最近4次触发的事件ID（防重复）
  restCountSinceOvernight: 0, // 当前昼夜间隔内已触发的休息类事件次数
  mileage: 0, // 行驶里程（km），每个事件+5km
  totalEventsHandled: 0, // 玩家做出事件选择的总次数
  lastOvernightMileage: 0, // 上次昼夜休息时的里程
  passengerFavor: {}, // 乘客好感度 { "鹿": 65, "猎人": 40, ... }
  passengerGetOffMileage: {}, // 乘客到达目的地下车里程 { "旅行者": 65 } 表示里程>=65时下车
  travelerDropOffCount: 0, // 旅行者被送到目的地的次数
  permanentPassengers: [], // 永久上车的乘客列表（不再下车）
  // 成就系统
  unlockedAchievements: [], // 已解锁的成就ID列表（跨档保留）
  sessionAchievements: [], // 本轮游戏解锁的成就ID列表
  passengersEverOnBoard: [], // 曾经上过车的乘客列表（用于成就检查）
  itemsCrafted: 0, // 合成物品次数
  itemsUsed: 0, // 使用物品次数
  hasTradedWithMerchant: false, // 是否与商人交易过
  survivedLowStats: false, // 是否在低属性下生存过
  perfectRun: false, // 是否达成完美旅程
  lowStatsMileage: 0, // 低属性下行驶的里程
  minFuelDuringRun: 100, // 本轮最低燃油
  minDurabilityDuringRun: 100, // 本轮最低耐久
  minComfortDuringRun: 100, // 本轮最低舒适
  hasOpenedRedPacket: false, // 是否打开过红包（用于成就检查）
  achievedEndings: [], // 已达成的结局ID列表（跨档保留）
  triggeredConditionalStories: [], // 已触发的条件剧情ID（防重复）
  hardModeTags: [], // 困难模式已选修饰符ID列表
  hardModeBonuses: [], // 困难模式已选加成ID列表
  activeDebuffs: [], // 衰变 debuff 列表: [{id: string, stacks: number}]
  easyMode: false, // 简单模式（关闭衰变 debuff）
  adminPermitPeakCount: 0, // 背包中「一次性管理员权限」曾同时存在的历史最大数量
  journeyLog: [], // 事件历程记录 [{type, mileage, ts, icon, main, sub, ...}]
  _lastJourneyMilestone: 0, // 上次记录里程碑时的里程（防重复）
};

// 获取乘客当前好感度（0–100）
function getPassengerFavor(name) {
  if (typeof gameState.passengerFavor !== "object") return DEFAULT_FAVOR;
  const v = gameState.passengerFavor[name];
  return typeof v === "number" && !isNaN(v)
    ? Math.min(FAVOR_MAX, Math.max(FAVOR_MIN, v))
    : DEFAULT_FAVOR;
}

// 皮卡状态管理
let truckState = {
  passengers: [], // 乘客列表
  wheelState: 1, // 轮子状态：1或2
  fuel: 100, // 燃油量 (0-100)
  durability: 100, // 车辆耐久 (0-100)
  comfort: 100, // 乘客舒适度 (0-100)
};

// 库存状态管理
let inventoryState = {
  gold: GAME_CONFIG.initialGold, // 金币
  maxWeight: 50, // 最大载重
  trunkLevel: 1, // 后备箱等级
  items: [], // 物品列表: [{id: string, quantity: number}]
};

// 暂停文本生成的标记
let textGenerationPaused = false;
