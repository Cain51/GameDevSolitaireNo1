// 商人类事件 - 流浪商人、神秘商人及相关交易事件

const EVENTS_MERCHANT = {
  // 商人事件
  merchant: {
    id: "merchant",
    title: "🛒 遇见流浪商人",
    tags: ["商人"],
    triggerConfig: { char: "商", color: "#c084fc", fontSize: "22px" },
    description:
      "一位推着小推车的流浪商人出现在路边。他的推车上摆满了各种稀奇古怪的物品，向你热情地招手。",
    image: "商！",
    oneTime: false,
    triggerWeight: 10,
    condition: null,
    choices: [
      {
        id: "merchant_trade",
        text: "看看货物",
        description: "和商人做买卖",
        hintUnknown: "商人有什么好货？",
        hintKnown: "打开商店界面，可以购买物资",
        result: {
          message: [
            "商人打开了他的百宝箱...",
            "商人摊开一块破布，上面整整齐齐地摆着各种物资，笑着向你介绍。",
            "商人从推车里翻出一个破旧的账本，上面密密麻麻写满了商品名和价格。",
          ],
          effects: {
            openMerchantModal: "流浪商人",
          },
        },
      },
      {
        id: "merchant_chat",
        text: "闲聊几句",
        description: "和商人聊聊路上的见闻",
        hintUnknown: "聊天也许有好处？",
        hintKnown: "金币 +5 / 舒适 +5",
        result: {
          message: [
            "商人讲了个好笑的旅途故事，心情愉悦！临走还给了你一些零钱。",
            "商人喝着自带的茶，跟你聊起了最近路上的传闻，说前几天有人看见一辆会说话的车。你觉得他在说你。",
            '商人熟悉这片路，给你指了条新路，还顺手塞给你几枚金币说"当见面礼"。',
          ],
          effects: {
            gold: 5,
            comfort: 5,
          },
        },
      },
      {
        id: "merchant_barter",
        text: "以物易物",
        description: "用废金属换些有用的东西",
        hintUnknown: "废金属能换到什么？",
        hintKnown: "消耗废金属×2 / 随机物资",
        result: {
          message: [
            "商人眼睛一亮，从推车里翻出几样东西换走了你的废金属。",
            "商人掂了掂废金属，满意地点头，然后从箱子底部摸出一件东西递给你。",
            "废金属在商人眼里是宝贝。他接过去，把推车翻了个底朝天，找出了几样存货交换。",
          ],
          effects: {
            removeItems: [{ id: "废金属", quantity: 2 }],
            randomLoot: true,
          },
        },
      },
      {
        id: "merchant_skip",
        text: "没有兴趣",
        description: "礼貌地拒绝商人",
        hintUnknown: "不买东西也没关系吧？",
        hintKnown: "无任何消耗，安全离开",
        result: {
          message: [
            "你摆了摆手，商人耸了耸肩，推着小车继续走了。",
            '商人遗憾地收起招手的姿势，喃喃道"下次再来啊"，推着车远去了。',
            "你婉言谢绝，商人豁达地笑笑，转身吹着口哨继续赶路。",
          ],
          effects: {},
        },
      },
    ],
  },

  // 神秘商人事件
  rare_merchant: {
    id: "rare_merchant",
    title: "🎩 神秘商人现身",
    tags: ["商人", "神秘"],
    triggerConfig: { char: "商", color: "#a855f7", fontSize: "22px" },
    description:
      "一阵紫色的烟雾散去，一位穿着斗篷的神秘人出现在路边。他的眼神深邃，手中握着一个发光的宝箱。",
    image: "商！",
    oneTime: false,
    triggerWeight: 5,
    condition: null,
    choices: [
      {
        id: "rare_trade",
        text: "交易",
        description: "看看神秘商人有什么珍品",
        hintUnknown: "神秘商人的货物一定很特殊？",
        hintKnown: "打开神秘商店界面，有稀有物品",
        result: {
          message: [
            "神秘商人缓缓打开了他的宝箱...",
            "宝箱内部散发着淡淡的光，神秘商人微微一笑，伸手示意你自己挑选。",
            '商人从斗篷里取出一本古旧的目录，用手指慢慢翻动。"你想要什么？"',
          ],
          effects: {
            openMerchantModal: "神秘商人",
          },
        },
      },
      {
        id: "rare_skip",
        text: "保持距离",
        description: "不太信任这个神秘人",
        hintUnknown: "不交易会怎样？",
        hintKnown: "金币 +4，白捡的",
        result: {
          message: [
            "你警惕地后退了一步，神秘商人微微一笑，消失在紫色烟雾中。地上留下了几枚金币。",
            "你婉拒了神秘人的邀请，他耸耸肩，化为一缕紫烟消散。地面上多了几枚亮晶晶的金币。",
            "神秘商人没有勉强，轻轻鞠了个躬，随即不见了踪影。留下的金币在阳光下闪着光。",
          ],
          effects: {
            gold: 4,
          },
        },
      },
    ],
  },

  // 拾荒者事件
  scavenger: {
    id: "scavenger",
    title: "♻️ 遇见拾荒者",
    image: "拾！",
    tags: ["商人"],
    triggerConfig: { char: "拾", color: "#737373", fontSize: "22px" },
    description:
      "一位推着破旧手推车的拾荒者站在路边，车上堆满了各种废弃零件，他向你挥手示意停车。",
    oneTime: false,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "buy_scrap",
        text: "收购废料",
        description: "从拾荒者手里购买废料",
        hintUnknown: "废料有什么用？",
        hintKnown: "金币 -5 / 获得废金属×2 / 随机物资",
        result: {
          message: [
            '拾荒者把一堆废金属哗啦倒进你的车厢，还顺手塞了件他觉得"没用"的东西。',
            "拾荒者数着五枚金币，把自己认为最值钱的废料都搬上了你的车。",
            "交易完成，拾荒者高兴地挥手道别，继续推着空了一半的手推车前行。",
          ],
          effects: {
            gold: -5,
            addItems: [{ id: "废金属", quantity: 2 }],
            randomLoot: true,
          },
        },
      },
      {
        id: "sell_scrap",
        text: "卖出废料",
        description: "把自己的废金属卖给拾荒者",
        hintUnknown: "卖废料能赚多少？",
        hintKnown: "消耗废金属×2 / 金币 +4",
        result: {
          message: [
            "拾荒者接过废金属，仔细检查后掏出四枚金币，露出满意的笑容。",
            "拾荒者把废金属翻来翻去，最后还是点头收了，给了你几枚金币。",
            "废金属找到了新主人，拾荒者像收到宝贝一样把它们一件件摆好，然后给了你报酬。",
          ],
          effects: {
            removeItems: [{ id: "废金属", quantity: 2 }],
            gold: 4,
          },
        },
      },
      {
        id: "pass_scavenger",
        text: "礼貌经过",
        description: "不做交易",
        hintUnknown: "不交易没关系吧？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "你朝拾荒者点了点头，继续赶路。",
            '你摇下车窗说了声"不用了"，拾荒者挥手道别，继续翻他的废料堆。',
            "拾荒者目送你离去，然后低下头继续整理他的战利品。",
          ],
          effects: {},
        },
      },
    ],
  },

  // 技术商人事件
  tech_merchant: {
    id: "tech_merchant",
    title: "🔬 遇见技术商人",
    tags: ["商人"],
    triggerConfig: { char: "械", color: "#38bdf8", fontSize: "22px" },
    description:
      "路边停着一辆改装过的工具车，上面堆满了修理配件和技术零件。一位穿着满是油污工装的商人正在整理货物，看见你停下，擦了擦手朝你点了点头。",
    image: "商！",
    oneTime: false,
    triggerWeight: 6,
    condition: null,
    choices: [
      {
        id: "tech_trade",
        text: "查看修理装备",
        description: "看看有没有皮卡需要的零件",
        hintUnknown: "技术商人的货物应该很专业？",
        hintKnown: "打开技术商店界面，有修理零件",
        result: {
          message: [
            "商人把货单递过来，上面密密麻麻全是技术名词，但价格标得很清楚。",
            "技术商人介绍了一下最新到货的修理套件，说是从工厂直接拿的货，质量保证。",
            '商人擦了擦手上的油，翻出一本产品手册递给你。"需要什么，直接说。"',
          ],
          effects: {
            openMerchantModal: "技术商人",
          },
        },
      },
      {
        id: "tech_consult",
        text: "请他检查皮卡",
        description: "让他帮忙看看有什么问题",
        hintUnknown: "让技师检查要花钱吧？",
        hintKnown: "金币 -8 / 耐久 +20（性价比很高）",
        result: {
          message: [
            "技术商人围着皮卡转了一圈，指出了几个小问题，顺手帮你处理了，收了点辛苦费。",
            "商人蹲下来看了看底盘，拿出工具敲了敲，说了一堆专业词汇，最后帮你加固了几个关键螺丝。",
            "商人仔细检查了皮卡，找出两处隐患并及时修复，只收了个工本费。皮卡状态好多了。",
          ],
          effects: {
            gold: -8,
            durability: 20,
          },
        },
      },
      {
        id: "tech_skip",
        text: "不需要，继续赶路",
        description: "礼貌告别",
        hintUnknown: "走掉不会有损失？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "技术商人点了点头，继续整理他的工具，没有多说什么。",
            "你婉言谢绝，商人耸耸肩，转身忙自己的事。",
            '商人挥手道别，说了句"有需要再来"，继续摆弄他的零件。',
          ],
          effects: {},
        },
      },
    ],
  },

  // 物资贩子事件（需解锁：首次搜刮废弃农场后触发）
  supply_merchant: {
    id: "supply_merchant",
    title: "🧺 路边物资贩子",
    tags: ["商人"],
    triggerConfig: { char: "物", color: "#86efac", fontSize: "22px" },
    description:
      "路边搭着一个简易帆布棚，下面摆满了食物、日用品和各种生活物资。一个精瘦的小贩正热情地向路过的车辆挥手招揽生意。",
    image: "商！",
    oneTime: false,
    triggerWeight: 7,
    condition: null,
    choices: [
      {
        id: "supply_trade",
        text: "查看物资",
        description: "补充一些生活必需品",
        hintUnknown: "物资贩子有什么好货？",
        hintKnown: "打开物资商店界面，可以补给",
        result: {
          message: [
            "物资贩子把布单掀开，展示了琳琅满目的补给品，种类齐全，价格也算公道。",
            '贩子热情地介绍："什么都有！吃的用的，保证新鲜！"',
            "摊位虽然简陋，但货物很充实。贩子麻利地帮你整理起你挑选的物品。",
          ],
          effects: {
            openMerchantModal: "物资贩子",
          },
        },
      },
      {
        id: "supply_barter",
        text: "用废金属换食物",
        description: "以物易物",
        hintUnknown: "废金属能换到吃的？",
        hintKnown: "消耗废金属×2 / 获得零食×2 + 草药×1",
        result: {
          message: [
            "贩子掂了掂废金属，点头同意了。换来的食物虽然不算多，但还新鲜。",
            "物资贩子收下废金属，挑了一些食物和日用品递给你，算是公平交易。",
            "双方各取所需，愉快地完成了交易。贩子说废金属拿去也能换不少东西。",
          ],
          effects: {
            removeItems: [{ id: "废金属", quantity: 2 }],
            addItems: [
              { id: "零食", quantity: 2 },
              { id: "草药", quantity: 1 },
            ],
          },
        },
      },
      {
        id: "supply_skip",
        text: "没有需要",
        description: "继续赶路",
        hintUnknown: "不买也没关系？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "贩子遗憾地摆了摆手，转头招呼下一辆路过的车。",
            '你摇下车窗说"不用了"，贩子点头道别，继续在摊边守着。',
            "你没有停留太久，贩子在身后喊了句『欢迎下次光顾』。",
          ],
          effects: {},
        },
      },
    ],
  },
};
