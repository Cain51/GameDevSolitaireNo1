// 遭遇类事件 - 鹿、猎人、骚福瑞等路上邂逅
// oneTime: true 表示一次性事件

const EVENTS_ENCOUNTER = {
  // 鹿事件
  deer: {
    id: "deer",
    title: "🦌 前方发现一只鹿！",
    image: "鹿！",
    triggerConfig: { char: "鹿", color: "#d4a574", fontSize: "22px" },
    description:
      "一只迷路的鹿站在公路中央，好奇地望着你的皮卡...它的眼睛里闪烁着智慧的光芒。它用眼神和你交流，似乎在乞求你的安保。",
    oneTime: true,
    triggerWeight: 15,
    condition: null,
    choices: [
      {
        id: "invite",
        text: "接受这个委托！",
        description: "打开车门，邀请鹿成为旅途伙伴",
        hintUnknown: "让鹿上车，会发生什么呢？",
        hintKnown: "获得乘客「鹿」/ 舒适 -8 / 解锁后续猎人事件",
        result: {
          message: [
            "鹿高兴地跳上了后车厢！不过它有点紧张，在车里不太安分...",
            "鹿犹豫了一下，然后跟着你上了车。它把头探出车窗，耳朵随风飘动。",
            "鹿轻盈地一跃跳上皮卡！它似乎很享受这趟旅程，只是车厢里多了些鹿毛。",
          ],
          effects: {
            addPassenger: "鹿",
            unlockEvents: ["hunter"],
            comfort: -8,
          },
        },
      },
      {
        id: "honk",
        text: "鸣笛驱赶",
        description: "按响喇叭，让鹿让开道路",
        hintUnknown: "鹿可能不太高兴……",
        hintKnown: "耐久 -15，鹿会踢车",
        result: {
          message: [
            "鹿受惊后猛踢了一脚车头，留下一道凹痕后跑进了森林。",
            "一阵刺耳的喇叭声响起，鹿慌乱地转身，后蹄蹬在引擎盖上，留下两个蹄印。",
            "鹿被吓了一跳，跌跌撞撞地跑进路边树丛，顺手把车侧镜撞歪了。",
          ],
          effects: {
            durability: -15,
          },
        },
      },
      {
        id: "detour",
        text: "小心绕行",
        description: "慢慢绕过鹿，不打扰它",
        hintUnknown: "绕路可能费油？",
        hintKnown: "燃油 -8，安全无事",
        result: {
          message: [
            "你小心翼翼地绕了一段土路，多烧了些油，但平安无事。",
            "你关掉音乐，轻踩油门，慢慢从鹿身边绕过。鹿抬头看了你一眼，继续低头吃草。",
            "绕了个远路，路况很差，皮卡颠得厉害，但鹿安然无恙地留在原地。",
          ],
          effects: {
            fuel: -8,
          },
        },
      },
    ],
  },

  // 猎人事件
  hunter: {
    id: "hunter",
    title: "🏹 遇到猎人",
    image: "猎人！",
    triggerConfig: { char: "猎", color: "#8b7355", fontSize: "22px" },
    description:
      "一位背着猎枪的猎人拦住了你的去路...他看到你的皮卡，眼中闪过一丝惊讶。",
    oneTime: true,
    triggerWeight: 15,
    condition: {
      requiresPassenger: "鹿",
    },
    choices: [
      {
        id: "hide",
        text: "加速逃离",
        description: "猛踩油门，甩掉猎人",
        hintUnknown: "逃跑要消耗不少燃油吧……",
        hintKnown: "燃油 -15 / 舒适 +5，保住鹿",
        result: {
          message: [
            "你猛踩油门扬长而去，猎人在后面追赶了一段，最终被甩开。大量燃油在狂奔中烧掉了。",
            "皮卡轰鸣着冲了出去！后视镜里猎人愤怒地挥着帽子，很快消失在尘土中。",
            "你踩死油门，轮胎在沥青上留下长长的黑痕。猎人呆立在原地，鹿从车窗探出头做了个鬼脸。",
          ],
          effects: {
            fuel: -15,
            comfort: 5,
          },
        },
      },
      {
        id: "talk",
        text: "与猎人对话",
        description: "停下车，和猎人周旋",
        hintUnknown: "也许能和平解决？",
        hintKnown: "金币 +12 / 舒适 +5，保住鹿",
        result: {
          message: [
            "猎人告诉你他正在寻找一只鹿。你假装没看见，还帮他指了条反方向的路。猎人感谢你给了些零钱。",
            '你若无其事地停下来问路，猎人热心地介绍了附近地形。临别时他塞给你几枚金币，"路上小心"。',
            "你和猎人聊起了天气，顺带把鹿藏在了毯子下面。猎人没发现异常，还给了你些路费。",
          ],
          effects: {
            gold: 12,
            comfort: 5,
          },
        },
      },
      {
        id: "surrender",
        text: "把鹿交给猎人",
        description: "放弃鹿的安保，换取赏金",
        hintUnknown: "赏金应该不少，但鹿会……",
        hintKnown: "金币 +35 / 失去乘客「鹿」",
        result: {
          message: [
            "你把鹿交给了猎人，鹿看向你时眼里闪着悲伤的光...猎人掏出了赏金。",
            "鹿慢慢走下车，回头望了你最后一眼。猎人数着金币，满意地点头。",
            "你打开车门，鹿沉默地跳下去。你不敢看它的眼睛，接过赏金默默上路。",
          ],
          effects: {
            removePassenger: "鹿",
            gold: 35,
          },
        },
      },
      {
        id: "share",
        text: "分享食物",
        description: "给猎人零食，请他放过鹿",
        hintUnknown: "用零食能收买猎人吗？",
        hintKnown: "获得乘客「猎人」/ 消耗零食×1 / 耐久 -5 / 舒适 -10",
        result: {
          message: [
            "猎人接受了你的食物，承诺不再追捕这只鹿。你们成为了朋友。不知道为什么，他一定要扒在车底...",
            "猎人吃着零食，聊起了自己的故事。他说他其实不忍心打鹿，这下有了台阶，松了口气。顺便爬上了你的皮卡。",
            '猎人拿过零食，沉默地咬了一口。"好吧，算我欠你个人情。"他把猎枪背到身后，然后坐进了副驾驶。',
          ],
          effects: {
            addPassenger: "猎人",
            removeItems: [{ id: "零食", quantity: 1 }],
            durability: -5,
            comfort: -10,
          },
        },
      },
    ],
  },

  // 骚福瑞事件
  saofurry: {
    id: "saofurry",
    image: "骚福瑞！",
    triggerConfig: { char: "福", color: "#ff69b4", fontSize: "22px" },
    description:
      "路边突然出现一只神秘的生物...它有着奇异的外表，眼神中透着一丝狡黠。这就是传说中的现一只神秘的生物...它有着奇异的外表，眼神中透着一丝狡黠。这就是传说中的骚福瑞！",
    image: "骚福瑞！",
    oneTime: true,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "invite",
        text: "邀请上车",
        description: "让它坐副驾...但它可能很难伺候",
        hintUnknown: "让它上车？后果未知……",
        hintKnown: "获得乘客「骚福瑞」/ 舒适 -15",
        result: {
          message: [
            "骚福瑞兴奋地跳上了副驾驶！它立刻开始调节座椅、翻弄收音机...车内舒适度骤降。",
            '骚福瑞小心翼翼地上车，然后立刻把玉足踩在仪表盘上。"走吧，我知道条近路。"',
            "骚福瑞上车后第一件事是把你的零食（？）翻出来吃，然后无辜地看着你。",
          ],
          effects: {
            addPassenger: "骚福瑞",
            comfort: -15,
          },
        },
      },
      {
        id: "ignore",
        text: "无视/驱赶",
        description: "冷漠离开，但它可能不会善罢甘休",
        hintUnknown: "无视它会怎样？",
        hintKnown: "耐久 -25，车身被抓伤",
        result: {
          message: [
            "你冷漠地踩下油门，骚福瑞恼羞成怒，抓了你的车身好几道深痕才不情愿地放手！",
            "骚福瑞追着皮卡跑了好一段路，顺手在车门上留下了几道爪痕作为纪念。",
            "你无视骚福瑞的招手，它愤怒地朝你扔了块石头，正好砸在后保险杠上。",
          ],
          effects: {
            durability: -25,
          },
        },
      },
      {
        id: "play",
        text: "和骚福瑞哼哼哈嘿",
        description: "试图和骚福瑞做一些奇怪的事情...",
        hintUnknown: "这个选项看着很危险……",
        hintKnown: "⚠️ 游戏结束！千万别选！",
        result: {
          message: [
            "骚福瑞突然暴怒！它展现出了惊人的战斗力...你的皮卡被打爆了！游戏结束。",
            "这不是个好主意。骚福瑞的把皮卡直接搅成了碎片！游戏结束。",
            "骚福瑞愣了一秒，然后以迅雷不及掩耳之势将你的皮卡掀翻了。游戏结束。",
          ],
          effects: {
            gameOver: true,
          },
        },
      },
    ],
  },

  // 下雨事件
  rain: {
    id: "rain",
    title: "🌧️ 突降大雨",
    image: "雨！️",
    tags: ["雨天"],
    triggerConfig: { char: "雨", color: "#60a5fa", fontSize: "22px" },
    theme: {
      emojis: ["💧", "🌧️", "💦"],
      animation: "rain",
      bgColor: "#0f172a",
      borderColor: "#3b82f6",
      barColors: { fuel: "#60a5fa", durability: "#93c5fd", comfort: "#bfdbfe" },
    },
    description: "天空突然乌云密布，倾盆大雨瞬间降临，能见度急剧下降...",
    oneTime: false,
    triggerWeight: 12,
    condition: null,
    choices: [
      {
        id: "continue",
        text: "冒雨前行",
        description: "保持速度，小心驾驶",
        hintUnknown: "车辆可能受损……具体影响未知",
        hintKnown: "耐久 -10 / 舒适 -5 / 燃油 -6 / 12.5%概率获得珍品",
        result: {
          message: [
            "雨水和碎石不断击打车身，皮卡的外壳又多了几道划痕，但你依然坚定地向前行驶。",
            "雨刷疯狂摆动，挡风玻璃模糊一片。你咬着牙继续开，车漆被雨水夹着砂砾打得坑坑洼洼。",
            "雨大得像有人拿水桶往车上泼。路面积水飞溅，皮卡深一脚浅一脚地前进，底盘撞了不少石头。",
          ],
          effects: [
            {
              durability: -10,
              comfort: -5,
              fuel: -6,
            },
            {
              type: "chance",
              chance: 0.125,
              success: { addItems: [{ id: "雨水护符", quantity: 1 }] },
            },
          ],
        },
      },
      {
        id: "wait",
        text: "停车等待",
        description: "找个地方停车，等雨停了再走",
        hintUnknown: "安全但可能浪费燃油？",
        hintKnown: "燃油 -13，车辆不受损",
        result: {
          message: [
            "你停在一棵大树下避雨，发动机怠速运转了很久。雨势渐小后，继续上路。",
            "皮卡停在路边一处土坡下，你靠着座椅听着雨声，发动机低沉地嗡嗡作响。等了约半小时雨才停。",
            "你把皮卡开进路边一座废弃加油站的顶棚下，等雨的时候顺手清理了一下车窗。",
          ],
          effects: {
            fuel: -13,
          },
        },
      },
    ],
  },

  // 路障事件
  roadblock: {
    id: "roadblock",
    title: "🚧 前方道路封闭",
    image: "障！",
    tags: ["危险"],
    triggerConfig: { char: "障", color: "#f97316", fontSize: "22px" },
    description:
      "一块褪色的警示牌挡在路中央：「前方施工，禁止通行」。旁边倒着几个锈迹斑斑的水马，看起来已经没人管了。",
    oneTime: false,
    triggerWeight: 10,
    condition: null,
    choices: [
      {
        id: "push_through",
        text: "强行通过",
        description: "开过去，反正没人管",
        hintUnknown: "强行通过可能会遇到麻烦……",
        hintKnown: "触发二级选择：塞钱(金币-5/耐久+10)、硬冲(耐久-15)、后退(燃油-8/舒适-10)",
        result: {
          message: [
            "你把水马推到一边，开了过去。路面坑洼不平，皮卡颠了好久，底盘挂了几下。",
            "皮卡轰鸣着碾过施工区，碎石乱飞，车底传来几声闷响。出来后你长吐一口气。",
          ],
          effects: {
            type: "choice",
            prompt: "强行进入施工区，一个身穿反光衣的工人突然冲出来拦住路。",
            options: [
              {
                text: "塞点辛苦费",
                description: "花钱免得一身灰",
                message: [
                  "塞给工人一点零花钱，他非常开心，帮你探查了引擎问题。",
                  "工人拿过钱，看你的红色皮卡很不错，帮你护理了一下。完成后，你小心翼翼地开了过去。",
                ],
                effects: { gold: -5, durability: 10 },
              },
              {
                text: "继续硬冲",
                description: "反正他不敢真拦车",
                message: [
                  "你加速冲了过去，工人破口大骂，但最终没追上，丢过来一个砖头打碎了玻璃。",
                  "皮卡轰鸣着冲了出去，工人大骂。你装作没听见。直到丢过来一个砖头打碎了玻璃。",
                ],
                effects: { durability: -15 },
              },
              {
                text: "后退绕路",
                description: "不值得硬刚",
                message: [
                  "你倒车退回去，老老实实绕了条山路。稳健，安全。",
                  "认输绕路。工人在身后射来轻蔑的目光，你装作没看见。",
                ],
                effects: { fuel: -8, comfort: -10 },
              },
            ],
          },
        },
      },
      {
        id: "detour_road",
        text: "绕山路",
        description: "绕一段土路，多消耗燃油",
        hintUnknown: "绕路很安全但费油？",
        hintKnown: "燃油 -12，无其他损失",
        result: {
          message: [
            "你调头走了段山路，弯弯绕绕，风景倒是不错，就是油耗高得吓人。",
            "土路颠簸了将近二十分钟，路边有人卖烤玉米，你没有停车，油表却又往下掉了一格。",
            "绕路比想象中更远，山路在林子里绕来绕去，你差点迷路，油却没少烧。",
          ],
          effects: { fuel: -12 },
        },
      },
      {
        id: "check_sign",
        text: "仔细查看标志",
        description: "也许能找到线索",
        hintUnknown: "也许能发现什么有用的东西？",
        hintKnown: "50%概率获得随机物资；50%概率燃油 -5",
        result: {
          message: "你停下车，走近那块褪色的标志，仔细打量起来——",
          effects: {
            type: "weighted",
            options: [
              {
                weight: 50,
                message: [
                  '标志背面用记号笔写着一行字："右侧土路可绕行，路边有废弃工具箱。"你顺着指引找到了些材料。',
                  "标志旁边压着半张地图，你捡起来研究了半天，照着新路线走，还顺手捡了些路边的遗落物。",
                ],
                effects: { randomLoot: true },
              },
              {
                weight: 50,
                message: [
                  "仔细看完通知后你发现封路原因是熊出没……你果断踩下油门，加速离开。",
                  "标志上密密麻麻全是警告，越看越心慌。你决定不再浪费时间，踩深油门绕路走。",
                ],
                effects: { fuel: -5 },
              },
            ],
          },
        },
      },
    ],
  },

  // 迷雾事件
  fog: {
    id: "fog",
    image: "雾！",
    title: "🌫️ 突遇浓雾",
    tags: ["视线模糊"],
    triggerConfig: { char: "雾", color: "#94a3b8", fontSize: "22px" },
    theme: {
      emojis: ["☁️", "🌫️", "☁️"],
      animation: "fog",
      bgColor: "#1a1a2a",
      borderColor: "#94a3b8",
      barColors: { fuel: "#94a3b8", durability: "#cbd5e1", comfort: "#e2e8f0" },
    },
    description:
      "一团厚重的白雾从山谷中涌出，瞬间将道路淹没。能见度不足五米，前方如同一片空白。从山谷中涌出，瞬间将道路淹没。能见度不足五米，前方如同一片空白。",
    image: "雾！",
    oneTime: false,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "slow_drive",
        text: "开大灯慢行",
        description: "谨慎通过",
        hintUnknown: "慢行应该安全，但有代价……",
        hintKnown: "燃油 -5 / 耐久 -15；15%概率获得珍品",
        result: {
          message: [
            "你打开雾灯，以龟速爬行。路边偶尔有树影闪过，心跳也跟着加速。终于穿出了雾区。",
            "大灯射出两道黄光，在浓雾里只能看到几米远。你把手贴在喇叭上，慢慢挪过了这段路。",
            "浓雾中开车像蒙着眼睛走路，你几乎是凭感觉驾驶的。有惊无险地驶出了雾区，冷汗浸透了衣背。",
          ],
          effects: [
            { fuel: -5, durability: -15 },
            {
              type: "chance",
              chance: 0.15,
              success: {
                message: "在浓雾中，你隐约看到一个人影站在路边。等你停车查看时，那里只有一张泛黄的照片，上面拍着一个站在雾中的模糊身影……",
                addItems: [{ id: "雾中人的照片", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "wait_fog",
        text: "停车等雾散",
        description: "原地等待",
        hintUnknown: "等待总是安全的吧？",
        hintKnown: "燃油 -10 / 舒适 +8（比慢行更划算）",
        result: {
          message: [
            "你把皮卡停到路边，关掉引擎，听着雾中的虫鸣。雾气在约四十分钟后散去，阳光重新照下来。",
            "停下来喝了杯热水，窗外白茫茫一片，像身处云中。雾散之后路面清晰如洗。",
            "趁等雾的功夫，你和乘客们聊了聊天，难得的清静时刻。等雾散后，大家心情都好了不少。",
          ],
          effects: { fuel: -10, comfort: 8 },
        },
      },
    ],
  },

  // 走失小孩事件
  lost_child: {
    id: "lost_child",
    title: "🧒 走失的小孩",
    image: "孩！",
    triggerConfig: { char: "孩", color: "#ff6b9d", fontSize: "22px" },
    description:
      "路边坐着一个哭泣的小孩，衣服脏兮兮的，显然已经在这里哭了很久。他看到你的皮卡，泪眼汪汪地看着你。",
    oneTime: true,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "help_child",
        text: "帮助小孩",
        description: "停下来帮他寻找家人",
        hintUnknown: "帮人应该有好报？",
        hintKnown: "金币 +15 / 舒适 +12（最佳选择）",
        result: {
          message: [
            "你停下车，安慰了小孩。经过耐心询问，你了解到了他家人的信息。最后你帮他找到了家人，全家人都非常感谢。",
            "小孩哭着说出了家人的名字和住处。你花了不少时间帮他找回家人。他们给了你路费和一些物资作为感谢。",
            "你给小孩喝了点水，陪他等待。不久，他的家人出现了。他们对你的帮助千恩万谢。",
          ],
          effects: { gold: 15, comfort: 12 },
        },
      },
      {
        id: "give_directions",
        text: "给他指路",
        description: "告诉他去哪里寻求帮助",
        hintUnknown: "指路也算帮忙吧……",
        hintKnown: "金币 +5 / 舒适 +8 / 消耗零食×1",
        result: {
          message: [
            "你停下车，温柔地问了小孩的家住在哪里。他含糊地指了个方向。你告诉他应该去镇上寻求帮助。",
            "你给小孩一些零食和水，然后给他指了条最近镇子的方向。小孩止住了眼泪，感谢地点头。",
          ],
          effects: {
            gold: 5,
            comfort: 8,
            removeItems: [{ id: "零食", quantity: 1 }],
          },
        },
      },
      {
        id: "ignore_child",
        text: "加速离开",
        description: "这不是你的责任",
        hintUnknown: "无视小孩心里过得去吗……",
        hintKnown: "舒适 -10 / 燃油 -3",
        result: {
          message: [
            "你装作没看见，踩下油门继续走。小孩的哭声在身后越来越远。",
            "你没有停车，加速离开了。后视镜里小孩的身影渐渐消失。",
          ],
          effects: { comfort: -10, fuel: -3 },
        },
      },
    ],
  },

  // 流浪汉/乞丐事件
  vagrant: {
    id: "vagrant",
    title: "🚶 流浪汉的故事",
    image: "汉！",
    triggerConfig: { char: "汉", color: "#666666", fontSize: "22px" },
    description:
      "一个蓬头垢面的流浪汉坐在路边，旧衣服上补丁重重。他抬起头看了你一眼，眼神深处有着说不出的沧桑。",
    oneTime: true,
    triggerWeight: 6,
    condition: null,
    choices: [
      {
        id: "give_money",
        text: "施舍金币",
        description: "给他一些钱",
        hintUnknown: "好心会有好报吗？",
        hintKnown: "金币 -8 / 舒适 +10",
        result: {
          message: [
            "你停下车，给了流浪汉一些金币。他眼眶湿润，一句话都说不出来，只是不停地道谢。",
            "流浪汉接过金币，颤抖的手说明了他的激动。他用沙哑的声音祝福你一路平安。",
          ],
          effects: { gold: -8, comfort: 10 },
        },
      },
      {
        id: "give_food",
        text: "分享食物",
        description: "给他零食和饮水",
        hintUnknown: "用食物帮助他……",
        hintKnown: "消耗零食×2 / 舒适 +8",
        result: {
          message: [
            "你递给流浪汉一些零食和水。他狼吞虎咽地吃下去，吃完后对你表示深深的感谢。",
            "流浪汉很感激你的帮助。他吃完食物后，给你讲了一个很长的故事，虽然故事很悲伤，但也让你收获了对生活的新认识。",
          ],
          effects: {
            removeItems: [{ id: "零食", quantity: 2 }],
            comfort: 8,
          },
        },
      },
      {
        id: "speed_past",
        text: "加速驶过",
        description: "这不关你的事",
        hintUnknown: "无视他会怎样？",
        hintKnown: "舒适 -5 / 燃油 -2",
        result: {
          message: [
            "你没有停下，加速驶过了这个流浪汉。他失望地看着你远去。",
            "你无视了流浪汉的存在，继续赶路。",
          ],
          effects: { comfort: -5, fuel: -2 },
        },
      },
    ],
  },

  // 异域旅客事件
  exotic_traveler: {
    id: "exotic_traveler",
    title: "🌍 异域旅客",
    image: "客！",
    triggerConfig: { char: "客", color: "#fbbf24", fontSize: "22px" },
    description:
      "一个穿着奇特、操着陌生口音的旅客站在路边。他的背包上贴满了来自各地的贴纸，显然是个资深旅行者。",
    oneTime: true,
    triggerWeight: 5,
    condition: null,
    choices: [
      {
        id: "chat_traveler",
        text: "和他聊天",
        description: "听听他的旅途故事",
        hintUnknown: "聊天应该挺轻松的？",
        hintKnown: "舒适 +10 / 燃油 -5",
        result: {
          message: [
            "异域旅客兴高采烈地讲述了他的冒险故事。他去过许多你没听过的地方，讲的故事让你大开眼界。",
            "旅客用带口音的话讲述他在其他国家的经历。虽然有些难以理解，但他的热情是感染人的。",
          ],
          effects: { comfort: 10, fuel: -5 },
        },
      },
      {
        id: "buy_souvenirs",
        text: "购买纪念品",
        description: "买他的家乡特产",
        hintUnknown: "纪念品值不值得买？",
        hintKnown: "金币 -12 / 获得稀有随机物资",
        result: {
          message: [
            "旅客从背包里翻出各种有趣的物品。虽然你不清楚它们的用处，但看起来很有价值。",
            "你买了旅客带来的一些特殊物品。虽然价格有点贵，但看起来还是很稀有的。",
          ],
          effects: {
            gold: -12,
            randomLoot: "稀有",
          },
        },
      },
      {
        id: "ignore_traveler",
        text: "无视他",
        description: "继续赶路",
        hintUnknown: "直接走？",
        hintKnown: "燃油 -2，无其他影响",
        result: {
          message: ["你没有停留，继续前行。旅客的身影在身后消失了。"],
          effects: { fuel: -2 },
        },
      },
    ],
  },

  // 老妇人求助事件
  elderly_woman: {
    id: "elderly_woman",
    title: "👵 年迈的妇人",
    image: "妇！",
    triggerConfig: { char: "妇", color: "#c2185b", fontSize: "22px" },
    description:
      "一位年迈的妇人蹲在路边，她的轮椅翻倒在一旁。她正努力想要扶起来，但显然力不从心。",
    oneTime: true,
    triggerWeight: 7,
    condition: null,
    choices: [
      {
        id: "help_elderly",
        text: "扶起她",
        description: "帮她恢复行动能力",
        hintUnknown: "帮助老人一定有好报吧？",
        hintKnown: "随机物资 / 舒适 +15 / 金币 +10（收益最高）",
        result: {
          message: [
            "你停下车，轻轻地扶起老妇人和她的轮椅。她握着你的手，眼眶湿润，一个劲地说谢谢。",
            "你帮她检查了轮椅，确保没有损伤。老妇人激动得话都说不清楚，给了你一个代代相传的护身符。",
          ],
          effects: { randomLoot: true, comfort: 15, gold: 10 },
        },
      },
      {
        id: "take_elderly",
        text: "送她去下一个镇子",
        description: "让她上车，送她安全地到达目的地",
        hintUnknown: "送人一程会得到什么呢？",
        hintKnown: "获得乘客「年迈妇人」/ 金币 +30 / 燃油 -10 / 舒适 +8",
        result: {
          message: [
            "老妇人坐在副驾驶座上，一路讲述着年轻时的故事。她在镇子下车时，给了你一笔可观的感谢费。",
            "老妇人在车上睡着了。看她在路颠簸中保持着安详的睡颜，你觉得所做的一切都值得。到达镇子后，她的家人很感激你。",
          ],
          effects: {
            addPassenger: "年迈妇人",
            gold: 30,
            fuel: -10,
            comfort: 8,
          },
        },
      },
      {
        id: "give_directions",
        text: "告诉她最近的镇子",
        description: "指路后继续赶路",
        hintUnknown: "指路总比不理要好？",
        hintKnown: "消耗零食×1 / 舒适 +5",
        result: {
          message: [
            "你给老妇人指了最近的镇子的方向，给了她一些零食和水。她虽然失望，但还是感谢了你。",
          ],
          effects: {
            removeItems: [{ id: "零食", quantity: 1 }],
            comfort: 5,
          },
        },
      },
      {
        id: "ignore_elderly",
        text: "无视她",
        description: "这不是你的问题",
        hintUnknown: "不帮忙心里会不安吧……",
        hintKnown: "舒适 -15",
        result: {
          message: [
            "你看到她在挣扎，但还是决定继续赶路。她失望的眼神在你心里留下了阴影。",
          ],
          effects: { comfort: -15 },
        },
      },
    ],
  },
};
