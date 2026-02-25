// 等宽空白字符（全角空格，用于皮卡显示的背景）
const B = "　";

// 乘客配置
// 定义每个乘客的显示字符、颜色、位置等信息
const PASSENGER_CONFIG = {
  // 鹿乘客
  鹿: {
    displayChars: ["鹿"],
    color: "#d4a574", // 金色/棕色
    positions: [
      { row: 1, col: 2 }, // 第1行第3列（0-indexed）
    ],
  },

  // 猎人乘客
  猎人: {
    displayChars: ["猎", "人"],
    color: "#8b7355", // 猎人棕
    positions: [
      { row: 4, col: 6 }, // 第5行第7列
      { row: 4, col: 7 }, // 第5行第8列
    ],
  },

  // 骚福瑞乘客
  骚福瑞: {
    displayChars: ["福"],
    color: "#ff69b4", // 骚粉色
    positions: [
      { row: 1, col: 10 }, // 第2行第11列
    ],
  },

  // 迷路旅行者（捎一程上车，到达目的地后下车）
  旅行者: {
    displayChars: ["旅"],
    color: "#34d399", // 青绿色
    positions: [
      { row: 1, col: 6 }, // 第2行第7列
    ],
  },

  // 年迈妇人（送她去镇子，到达目的地后下车）
  年迈妇人: {
    displayChars: ["妇"],
    color: "#c2185b",
    positions: [
      { row: 2, col: 8 },
    ],
  },

  // 流浪猫（第三次喂食后上车）
  猫: {
    displayChars: ["猫"],
    color: "#fb923c",
    positions: [
      { row: 3, col: 4 },
    ],
  },

  // 流浪艺人
  流浪艺人: {
    displayChars: ["艺"],
    color: "#f59e0b",
    positions: [
      { row: 1, col: 4 },
    ],
  },
};

// 皮卡基础模板（只有车体，无乘客）
// 后车厢在左(col 0-5)，车头在右(col 6-12)，前车灯在车头最前端
const TRUCK_BASE_TEMPLATE = [
  [B, B, B, B, B, B, "皮", "卡", "皮", "卡", "皮", "灯", "灯", B, B],
  ["后", "车", "厢", "后", "车", "厢", B, B, "丨", B, B, "皮", B, B, B],
  [
    "皮",
    "卡",
    "皮",
    "轮",
    "卡",
    "车",
    "门",
    "车",
    "门",
    "车",
    "轮",
    "门",
    "卡",
    B,
    B,
  ],
  [
    "皮",
    "卡",
    "轮",
    "轮",
    "轮",
    "车",
    "门",
    "车",
    "门",
    "轮",
    "轮",
    "轮",
    "卡",
    B,
    B,
  ],
  [B, B, B, "轮", B, B, B, B, B, B, "轮", B, B, B, B],
];

// 轮子转动动画配置
// 轮子在两种状态间交替：十字形 ↔ 斜方形
const WHEEL_ANIMATION = {
  // 前轮位置（左侧）
  frontWheel: {
    // 形态1：十字形（轮在上下左右）
    state1: [
      { row: 2, col: 3, char: "轮" }, // 上
      { row: 3, col: 2, char: "轮" }, // 左
      { row: 3, col: 4, char: "轮" }, // 右
      { row: 4, col: 3, char: "轮" }, // 下
    ],
    // 形态2：斜方形（轮在四个角，上部变为皮，下部变为空白）
    state2: [
      { row: 2, col: 2, char: "轮" }, // 左上
      { row: 2, col: 3, char: "皮" }, // 上（恢复为皮）
      { row: 2, col: 4, char: "轮" }, // 右上
      { row: 3, col: 2, char: B }, // 左（变为空白）
      { row: 3, col: 4, char: B }, // 右（变为空白）
      { row: 4, col: 2, char: "轮" }, // 左下
      { row: 4, col: 3, char: B }, // 下（变为空白）
      { row: 4, col: 4, char: "轮" }, // 右下
    ],
  },

  // 后轮位置（右侧）
  rearWheel: {
    // 形态1：十字形
    state1: [
      { row: 2, col: 10, char: "轮" }, // 上
      { row: 3, col: 9, char: "轮" }, // 左
      { row: 3, col: 11, char: "轮" }, // 右
      { row: 4, col: 10, char: "轮" }, // 下
    ],
    // 形态2：斜方形（轮在四个角，上部变为皮，下部变为空白）
    state2: [
      { row: 2, col: 9, char: "轮" }, // 左上
      { row: 2, col: 10, char: "皮" }, // 上（恢复为皮）
      { row: 2, col: 11, char: "轮" }, // 右上
      { row: 3, col: 9, char: B }, // 左（变为空白）
      { row: 3, col: 11, char: B }, // 右（变为空白）
      { row: 4, col: 9, char: "轮" }, // 左下
      { row: 4, col: 10, char: B }, // 下（变为空白）
      { row: 4, col: 11, char: "轮" }, // 右下
    ],
  },

  // 动画时间间隔（毫秒）
  interval: 300,
};

// 根据乘客列表生成皮卡模板
function generateTruckTemplate(passengers, wheelState = 1) {
  const template = TRUCK_BASE_TEMPLATE.map((row) => [...row]);

  // 对每个乘客，将其放置在指定位置
  passengers.forEach((passengerName) => {
    const passengerConfig = PASSENGER_CONFIG[passengerName];
    if (passengerConfig) {
      passengerConfig.positions.forEach((pos, index) => {
        if (
          pos.row >= 0 &&
          pos.row < template.length &&
          pos.col >= 0 &&
          pos.col < template[pos.row].length &&
          index < passengerConfig.displayChars.length
        ) {
          template[pos.row][pos.col] = passengerConfig.displayChars[index];
        }
      });
    }
  });

  // 应用轮子状态
  const wheelPositions = wheelState === 1 ? "state1" : "state2";
  [WHEEL_ANIMATION.frontWheel, WHEEL_ANIMATION.rearWheel].forEach((wheel) => {
    wheel[wheelPositions].forEach((pos) => {
      if (
        pos.row >= 0 &&
        pos.row < template.length &&
        pos.col >= 0 &&
        pos.col < template[pos.row].length
      ) {
        template[pos.row][pos.col] = pos.char;
      }
    });
  });

  return template;
}
