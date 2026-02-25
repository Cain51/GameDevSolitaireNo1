// 罕见事件配置
// 所有事件触发时有 1% 概率被替换为罕见事件
// 罕见事件拥有动态流光边框（rare: true）

const EVENTS_RARE = {
  clown_night: {
    id: "clown_night",
    title: "🤡 小丑之夜",
    image: "🤡",
    tags: ["夜晚"],
    triggerConfig: { char: "丑", color: "#ff00ff", fontSize: "22px" },
    description:
      "夜色忽然变得诡异起来。公路两旁冒出五颜六色的气球，远处传来扭曲的马戏团音乐。一群小丑从黑暗中走出，彩色的灯光在他们身后旋转。他们似乎在邀请你参加一场特别的……派对。",
    rare: true,
    rareClass: "clown-event-border",
    theme: {
      borderColor: "#ff00ff",
      bgColor: "#0d001a",
      emojis: ["🤡", "🎈", "🎪", "🃏", "✨", "🎭"],
      animation: "float",
    },
    choices: [
      {
        id: "carnival",
        text: "🎪 加入狂欢！",
        description: "小丑们围成一圈，朝你的后备箱施展了魔法……",
        hintUnknown: "你的物品会怎样？",
        hintKnown: "所有非珍品物品变为小丑盲盒（打开后随机获得物品，2%概率出珍品）",
        result: {
          message: "小丑们欢呼着涌向你的皮卡，五彩的魔法烟雾弥漫开来。当烟雾散去，你发现后备箱里的东西全变成了色彩斑斓的盒子……",
          effects: { clownCarnival: true },
        },
      },
      {
        id: "confusion",
        text: "🌀 陷入迷茫",
        description: "小丑们的笑声让你头晕目眩，一切都在旋转……",
        hintUnknown: "你会怎样？",
        hintKnown: "三项属性（燃油/耐久/舒适）随机打乱互换",
        result: {
          message: "小丑们的笑声越来越大，你感到世界在疯狂旋转。当一切停下来时，你发现仪表盘上的数字……似乎变了位置？",
          effects: { clownConfusion: true },
        },
      },
      {
        id: "escape",
        text: "🏃 赶紧逃跑！",
        description: "马上踩油门离开这个诡异的地方",
        hintUnknown: "能逃得掉吗？",
        hintKnown: "75%无事发生，25%触发小丑结局",
        result: {
          message: "你猛踩油门，皮卡轮胎在地上划出火花——",
          effects: {
            type: "weighted",
            options: [
              {
                weight: 75,
                message: "你成功甩开了小丑们！在后视镜中，你看到他们挥手告别，彩色气球飘向夜空。",
                effects: {},
              },
              {
                weight: 25,
                message: "然而……前方的公路上也站满了小丑。他们微笑着，张开了双臂。",
                effects: { clownEnding: true },
              },
            ],
          },
        },
      },
    ],
  },

  fate_roulette: {
    id: "fate_roulette",
    title: "🎡 命运轮盘",
    image: "🎡",
    tags: ["神秘"],
    triggerConfig: { char: "命", color: "#ffd700", fontSize: "22px" },
    description:
      "公路中央升起一座金光闪闪的转盘，四个扇区在黑暗中旋转发光。一个无形的声音从四面传来：「欢迎参与命运的游戏。无论你做什么选择……结果都是一样的。」",
    rare: true,
    rareClass: "fate-roulette-border",
    theme: {
      borderColor: "#ffd700",
      bgColor: "#0d0a00",
      emojis: ["🎡", "🌀", "✨", "🎰", "💫", "🃏"],
      animation: "float",
    },
    choices: [
      {
        id: "spin1",
        text: "🎡 转动转盘",
        description: "命运已经注定，转吧",
        hintUnknown: "四种命运，听天由命",
        hintKnown: "转盘四区：金钱/物资/死亡/再来一次（死区会随次数扩大）",
        result: {
          message: "你伸手拨动了转盘……",
          effects: { fateRouletteOpen: true },
        },
      },
      {
        id: "spin2",
        text: "🎡 转动转盘",
        description: "命运已经注定，转吧",
        hintUnknown: "四种命运，听天由命",
        hintKnown: "转盘四区：金钱/物资/死亡/再来一次（死区会随次数扩大）",
        result: {
          message: "你伸手拨动了转盘……",
          effects: { fateRouletteOpen: true },
        },
      },
      {
        id: "spin3",
        text: "🎡 转动转盘",
        description: "命运已经注定，转吧",
        hintUnknown: "四种命运，听天由命",
        hintKnown: "转盘四区：金钱/物资/死亡/再来一次（死区会随次数扩大）",
        result: {
          message: "你伸手拨动了转盘……",
          effects: { fateRouletteOpen: true },
        },
      },
    ],
  },

  time_bank: {
    id: "time_bank",
    title: "⏳ 时间银行",
    image: "⏳",
    tags: ["神秘"],
    triggerConfig: { char: "时", color: "#00e5ff", fontSize: "22px" },
    description:
      "公路上忽然出现了一扇不应该存在的大门。门上写着「时间银行」——整个浏览器界面开始闪烁、扭曲，像是信号失真的老式显示器。一个没有面孔的柜员出现在屏幕里，它的声音从像素间的缝隙中传来：「欢迎光临。请问您今天需要什么服务？」",
    rare: true,
    rareClass: "time-bank-event-border",
    theme: {
      borderColor: "#00e5ff",
      bgColor: "#000a1a",
      emojis: ["⏳", "🕐", "💰", "📊", "⌚", "🔮"],
      animation: "float",
    },
    choices: [
      {
        id: "deposit",
        text: "💰 存入金钱",
        description: "将本局积攒的金币存入时间银行，跨存档保留",
        hintUnknown: "存入的金币下局可以取出",
        hintKnown: "弹出存款窗口，选择存入金额",
        result: {
          message: "柜员的像素脸上浮现出某种难以名状的表情：「好的。请输入您希望存入的金额。」",
          effects: { timeBankDeposit: true },
        },
      },
      {
        id: "withdraw",
        text: "🏦 取出金钱",
        description: "取出之前存在时间银行里的金币",
        hintUnknown: "没存过就没得取",
        hintKnown: "弹出取款窗口，选择取出金额",
        result: {
          message: "柜员翻查着某本不存在的账本：「您的账户有存款记录。请问您需要取出多少？」",
          effects: { timeBankWithdraw: true },
        },
      },
      {
        id: "refuse",
        text: "✊ 不玩资本主义的游戏",
        description: "拒绝这个荒谬的存在……后果自负",
        hintUnknown: "也许会有奇怪的事情发生",
        hintKnown: "触发时间乱流结局（死亡），但会留下10金币的抚恤金",
        result: {
          message: "你猛地关上车窗。「我不知道你是什么，但我不会——」",
          effects: { timeBankRefuse: true },
        },
      },
    ],
  },
};
