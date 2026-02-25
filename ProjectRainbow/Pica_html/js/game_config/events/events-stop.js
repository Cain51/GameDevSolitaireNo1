// 停留类事件 - 休息、制作、商人等可反复触发的停车事件

const EVENTS_STOP = {
  // 休息事件
  rest: {
    id: "rest",
    title: "☕ 路边休息站",
    image: "休！",
    triggerConfig: { char: "休", color: "#4ade80", fontSize: "22px" },
    description:
      "前方有一处适合停车休息的空地。皮卡发出疲惫的引擎声，也许是时候检查一下状态了。",
    oneTime: false,
    triggerWeight: 20,
    condition: null,
    choices: [
      {
        id: "rest_use",
        text: "停车休整",
        description: "检查车辆，修复损伤",
        hintUnknown: "花点钱修修车？",
        hintKnown: "耐久 +10 / 金币 -5",
        result: {
          message: [
            "你把皮卡停到路边，大家一起下车活动活动筋骨，随便聊了聊天。气氛不错。",
            "停车休整，乘客们纷纷帮忙检查轮胎、擦车窗，还顺便举行了一场小型野餐。",
            "短暂停靠，你检查了一下机油，乘客们站在路边伸懒腰。短暂的休息让大家精神了不少。",
          ],
          effects: {
            durability: 10,
            gold: -5,
          },
        },
      },
      {
        id: "rest_nap",
        text: "小憩片刻",
        description: "在车上打个盹",
        hintUnknown: "打个盹应该能恢复精神？",
        hintKnown: "舒适 +10 / 金币 -5",
        result: {
          message: [
            "你在驾驶座上闭目养神了一会儿，发动机怠速运转消耗了些燃油，但感觉好了不少。",
            "你把帽子盖在脸上，靠着车窗眯了一会儿。醒来时天色稍晚，人却清醒多了。",
            "你调低座椅靠背打了个短盹，梦里还在开车。醒来后揉了揉眼睛，继续上路。",
          ],
          effects: {
            comfort: 10,
            gold: -5,
          },
        },
      },
      {
        id: "rest_skip",
        text: "继续赶路",
        description: "时间紧迫，不做停留",
        hintUnknown: "不休息可能会影响什么？",
        hintKnown: "燃油 -6",
        result: {
          message: [
            "你踩下油门，继续前行。乘客们叹了口气，看来大家都想休息。",
            "没时间停，继续走。后座传来几声抱怨，你装作没听见。",
            "赶路要紧，歇息的事以后再说。乘客们蔫蔫地靠在座位上，气氛有些沉闷。",
          ],
          effects: {
            fuel: -6,
          },
        },
      },
    ],
  },

  // 加油站遗址事件
  gas_station: {
    id: "gas_station",
    title: "⛽ 废弃加油桩",
    image: "油！",
    triggerConfig: { char: "油", color: "#ef4444", fontSize: "22px" },
    description:
      "路边一座废弃加油桩，油泵锈迹小招牌还竖着。附近有些散落的杂物和一个上锁的储藏室。",
    oneTime: false,
    triggerWeight: 10,
    condition: null,
    choices: [
      {
        id: "check_pumps",
        text: "检查油泵",
        description: "也许还有残余燃油",
        hintUnknown: "也许能找到油？",
        hintKnown: "随机结果：60%燃油+15/耐久-5；30%燃油+25/耐久-4；10%燃油+5/耐久-3",
        result: {
          message: [
            "老旧的油泵居然还能出油！虽然油质不太好，但总归是燃油。搬油罐的时候碰破了个角，看看还有多少吧",
          ],
          effects: {
            type: "weighted",
            options: [
              {
                weight: 60,
                message:
                  "油泵还就行，油质一般但足够用。给车加满油，带走了一点耐久损耗。",
                effects: { fuel: 15, durability: -5, comfort: 3 },
              },
              {
                weight: 30,
                message:
                  "这台油泵居然还运转良好！你善加利用，注入了大量燃油，且损耗较小。",
                effects: { fuel: 25, durability: -4, comfort: 5 },
              },
              {
                weight: 10,
                message: "油泵里游了快一年了，搞了半天才倒出来一点。白忙活。",
                effects: { fuel: 5, durability: -3, comfort: -5 },
              },
            ],
          },
        },
      },
      {
        id: "search_storage",
        text: "撬开储藏室",
        description: "看看里面有什么",
        hintUnknown: "储藏室里会有什么？",
        hintKnown: "随机物资 / 耐久 -3 / 舒适 +6 / 20%电池",
        result: {
          message: [
            "铁门被你撬开，里面积满了灰尘，但货架上还有些遗留物资。",
            "储藏室里有人住过的痕迹——角落有个睡袋，旁边整整齐齐摆着几样东西。",
            "撬锁费了些功夫，进去后发现满地都是旧杂志，但墙角有个箱子，里面装着不少有用的东西。",
          ],
          effects: [
            { randomLoot: true, durability: -3, comfort: 6 },
            {
              type: "chance",
              chance: 0.2,
              success: {
                message: "储藏室的角落里有个手电筒，里面的电池居然还没完全报废。",
                addItems: [{ id: "电池", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "pass_by",
        text: "继续前行",
        description: "只是路过",
        hintUnknown: "路过不会有损失吧？",
        hintKnown: "无任何消耗，安全离开",
        result: {
          message: [
            "废弃加油站在后视镜里渐渐变小，你没有停留。",
            "你扫了一眼，没什么特别的。踩下油门继续走。",
            "荒废的加油站像一个被遗忘的句号，你路过，然后离开。",
          ],
          effects: {},
        },
      },
    ],
  },

  // ──────────────────────────────────────────────────────────────────────
  // 昼夜休息事件（每40km强制触发一次）
  // ──────────────────────────────────────────────────────────────────────
  overnight_rest: {
    id: "overnight_rest",
    title: "🌙 夜幕降临，必须扎营",
    image: "夜！",
    tags: ["夜晚"],
    triggerConfig: { char: "夜", color: "#8b41e7ff", fontSize: "22px" },
    theme: {
      emojis: ["⭐", "🌙", "✨", "🌟"],
      bgColor: "#0a0a2e",
      borderColor: "#6366f1",
      barColors: { fuel: "#a78bfa", durability: "#818cf8", comfort: "#7dd3fc" },
    },
    description:
      "太阳沉入地平线，夜色迅速笼罩公路。继续行驶太危险了——你找了一处空地，决定在这里度过今晚。",
    oneTime: false,
    triggerWeight: 0,
    condition: null,
    choices: [
      {
        id: "overnight_camp",
        text: "生火扎营",
        description: "捡些柴火，安心休息",
        hintUnknown: "应该能好好休息，但要消耗点什么……",
        hintKnown: "耐久 +8",
        result: {
          message: [
            "你捡来枯枝生了堆篝火，大家围坐在一起。火光映在每个人脸上，今晚很安心。",
            "篝火噼啪作响，夜空中星光密布。很久没有这么踏实地休息过了。",
            "生火的时候，乘客们各自找了个舒服的位置。有人讲了个故事，有人已经睡着了。",
          ],
          effects: {
            durability: 8,
          },
        },
      },
      {
        id: "overnight_watch",
        text: "轮流守夜",
        description: "分配守夜任务，确保安全",
        hintUnknown: "比较辛苦，但安全？",
        hintKnown: "舒适 -12 / 耐久 +15",
        result: {
          message: [
            "大家商量了守夜的顺序，每人轮值两小时。天亮后，虽然有些困倦，但都安全无事。",
            "守夜的人听着远处的虫鸣和风声。到了换班时分，月亮已经高挂。还算太平的一夜。",
            "你盯着黑暗中偶尔闪过的光芒，手搭在方向盘上打了个盹。幸好什么都没发生。",
          ],
          effects: {
            comfort: -12,
            durability: 15,
          },
        },
      },
      {
        id: "overnight_stargazing",
        text: "仰望星空",
        description: "难得的晴夜，好好欣赏",
        hintUnknown: "听起来很浪漫，效果如何呢……",
        hintKnown: "舒适 +3；12%概率获得珍品",
        result: {
          message: [
            "你关掉引擎，躺在皮卡车顶上看星星。银河横贯天际，这条路上最美的时刻，就是现在。",
            "夜风轻柔，星空清澈。你数了数星星，数到一半就睡着了。醒来时露水已经打湿了衣襟。",
            "有一颗流星划过，每个人都默默许了愿，但没有人说出口。第二天，大家的心情都好了不少。",
          ],
          effects: [
            { comfort: 3 },
            {
              type: "chance",
              chance: 0.12,
              success: {
                message: "你盯着夜空出神，一颗流星划过天马座的方向，尾迹闪耀了很久很久。你低头时，发现手边多了一颗温热的、散发着微光的小石头……",
                addItems: [{ id: "天马星座的流星", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "overnight_nightmare",
        text: "深夜奇遇",
        description: "夜里有什么动静......",
        hintUnknown: "??? 完全不知道会发生什么",
        hintKnown: "随机结果：可能获得物资和金币，也可能损失耐久（燃油 -10）",
        result: {
          message: [
            "半夜被奇怪的声音惊醒，出去看看吧...",
            "凌晨三点，远处有动静——小心点。",
            "夜里下起了小雨，夜色很安静。",
            "深夜里，有人从窗边探出一张脸。",
          ],
          effects: {
            type: "weighted",
            options: [
              {
                weight: 40,
                message: [
                  "半夜被奇怪的声音惊醒，你打开手电筒，发现有只老鼠在翻你的零食篮。双方都愣了一秒，它叫了几声，还留下了些东西。",
                  "凌晨三点，远处有灯光逼近——是一辆同样走夜路的老式车。对方按了声喇叭，你也回应。很快车停到旁边，对方递来一些物资，默默开走了。",
                ],
                effects: { randomLoot: "默认", comfort: 5, gold: 3, fuel: -10 },
              },
              {
                weight: 30,
                message: [
                  "深夜灯光微弱，你在周围搜寻了一圈。附近有一个被小心收藏的补给箱，里面的东西比想象的好多了！",
                  "深夜中一阵风吹过，吸引你走到路边一块防水布下面。拿开查看，居然是有人小心藏起来的物资！",
                ],
                effects: { randomLoot: "稀有", comfort: 5, fuel: -10 },
              },
              {
                weight: 20,
                message: [
                  "夜里下起了小雨。雨声打在车顶，反而格外安静，所有人都睡得出奇地好。第二天起来，空气清新了很多。",
                  "一队流浪者从远处经过，唱了一首简单但温暖的歌。一夜无事。",
                ],
                effects: { comfort: 10, fuel: -10 },
              },
              {
                weight: 10,
                message: [
                  "夜里传来恶心的刮擦声，你打开车门出去查看——居然是凹陷！车底被有东西刮到了。皮卡的损伤立刻显现。",
                  "凌晨感觉有动静，你赶紧下车查看，车身被不明物体刮到了几下。天亮后检查，全是石头留下的伤痕。",
                ],
                effects: { comfort: -8, durability: -5, fuel: -10 },
              },
            ],
          },
        },
      },
    ],
  },

  // 废弃仓库搜刮
  abandoned_warehouse: {
    id: "abandoned_warehouse",
    title: "🏚️ 废弃仓库",
    image: "仓！",
    tags: ["废墟"],
    triggerConfig: { char: "仓", color: "#7c3aed", fontSize: "22px" },
    description:
      "前方有一座巨大的废弃仓库，铁皮屋顶已经生锈，木门敞开着。你可以看到里面堆积着各种物资。",
    oneTime: false,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "loot_warehouse",
        text: "进去搜刮",
        description: "充分利用这个机会",
        hintUnknown: "仓库深处会有什么？风险很大……",
        hintKnown: "随机结果：40%废料+耐久-5；30%稀有+耐久-8；30%随机+舒适-20；12.5%珍品；25%铜线",
        result: {
          message: "你走进了昏暗的仓库——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 40,
                  message: [
                    "仓库里有大量生锈的工具和旧机械。虽然不是崭新的，但还能用。你装满了整个后车厢。",
                    "仓库里堆放着各种材料和工具。你花了不少时间整理和打包，最后收获颇丰。",
                  ],
                  effects: { randomLoot: "废料", durability: -5, fuel: -6 },
                },
                {
                  weight: 30,
                  message: [
                    "仓库深处有一个保险柜！虽然被腐蚀了，但你还是撬开了。里面装着一些保存完好的贵重物品。",
                    "在仓库角落里发现了一个被遗忘的箱子。里面装着看起来很珍贵的东西。",
                  ],
                  effects: { randomLoot: "稀有", durability: -8, fuel: -8 },
                },
                {
                  weight: 30,
                  message: [
                    "你正在搜刮时，老鼠窝坍塌了！大量尘埃涌起，你咳得眼泪都流出来了。虽然安全无虞，但消耗了很多精力。",
                    "仓库的二楼突然传来响声！吓得你逃出仓库。什么都没捞到，心情也坏透了。",
                  ],
                  effects: { randomLoot: true, comfort: -20, fuel: -5 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.125,
              success: {
                message:
                  "在仓库最深处的暗角里，你发现了一块被层层油布包裹的金属碎片。拨开灰尘，碎片表面精密的纹路泛起温暖的金色光泽——这是繁荣时代的遗物，据说持有它的人总能以更低的价格买到商品。",
                addItems: [{ id: "繁荣时代的金属碎片", quantity: 1 }],
              },
            },
            {
              type: "chance",
              chance: 0.25,
              success: {
                message:
                  "在一堆废旧电器中，你拆出了几段还算完好的铜线。这种材料在荒野里可不好找。",
                addItems: [{ id: "铜线", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "quick_search",
        text: "快速搜索门口区域",
        description: "只搜索显而易见的东西",
        hintUnknown: "门口应该比较安全？",
        hintKnown: "随机物资 / 燃油 -4 / 15%橡胶（安全稳妥）",
        result: {
          message: [
            "你在仓库门口找到了一些较轻便的物品。虽然没有进深处，但也有收获。",
            "门口堆放着一些相对完好的东西。你快速地收集了一些，然后离开了。",
          ],
          effects: [
            { randomLoot: true, fuel: -4 },
            {
              type: "chance",
              chance: 0.15,
              success: {
                message: "门口的废弃轮胎堆里，你剥下了一块还算完好的橡胶片。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "skip_warehouse",
        text: "不做停留",
        description: "继续赶路",
        hintUnknown: "走掉不会有损失吧？",
        hintKnown: "燃油 -2",
        result: {
          message: [
            "你看了一眼仓库，决定不浪费时间。继续前行。",
            "一个人搜刮太危险了。你加速驶过仓库。",
          ],
          effects: { fuel: -2 },
        },
      },
    ],
  },

  // 矿场废墟搜刮
  abandoned_mine: {
    id: "abandoned_mine",
    title: "⛏️ 废弃矿场",
    image: "矿！",
    tags: ["废墟", "危险"],
    triggerConfig: { char: "矿", color: "#1e40af", fontSize: "22px" },
    description:
      "路边有一个废弃的采矿场遗迹。被挖空的地面、生锈的机械和随处散落的矿石碎片显示这里曾经很繁忙。",
    oneTime: false,
    triggerWeight: 7,
    condition: null,
    choices: [
      {
        id: "mine_deep",
        text: "深入矿场",
        description: "冒险进入矿洞",
        hintUnknown: "矿洞深处可能很危险……",
        hintKnown: "高风险高回报：35%燃油物资/耐久-12；40%坍塌/耐久-20/舒适-15；25%稀有/耐久-10；5%珍品；25%橡胶",
        result: {
          message: "你驾车进入了被遗弃的矿场——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 35,
                  message: [
                    "矿洞里有大量高品质的矿石还未被开采！你装了满满一车。虽然车有点颠簸，但收获很大。",
                    "矿场深处还有不少没被完全开采的矿脉。你成功地采集了一些高品质的矿物。",
                  ],
                  effects: { randomLoot: "燃油", durability: -12, fuel: -10 },
                },
                {
                  weight: 40,
                  message: [
                    "矿洞的一堵墙突然坍塌了！你险些被埋住。狼狈地逃出来后，只收集到一些普通的矿石。",
                    "矿洞里传来奇异的声音，吓得你匆匆逃离。虽然没什么收获，但至少没出事。",
                  ],
                  effects: {
                    randomLoot: true,
                    durability: -20,
                    comfort: -15,
                    fuel: -12,
                  },
                },
                {
                  weight: 25,
                  message: [
                    "矿场内还留有一些工人的物品。虽然年久失修，但其中有几件看起来还是很有价值的。",
                    "在矿场办公室里发现了一些保存完好的物品。这些可能是矿工们留下的遗物。",
                  ],
                  effects: { randomLoot: "稀有", durability: -10, fuel: -8 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.05,
              success: {
                message:
                  "在矿洞最深处，你发现了一面闪烁着幽蓝光芒的矿壁。走近一看，一把嵌在岩石里的钻石镐头映入眼帘——它的镐尖在黑暗中折射出耀眼的光芒，仿佛在等待有缘人将它带走。你费了好大力气才把它撬了出来。",
                addItems: [{ id: "钻石稿", quantity: 1 }],
              },
            },
            {
              type: "chance",
              chance: 0.25,
              success: {
                message:
                  "在矿场的机械残骸旁，你发现了一些被油泥包裹的橡胶密封圈，清理后还能用。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "mine_surface",
        text: "在地表搜刮",
        description: "只收集地面上的物品",
        hintUnknown: "地表搜刮比较安全？",
        hintKnown: "默认随机物资 / 燃油 -3 / 20%铜线（安全选项）",
        result: {
          message: [
            "地面上散落着许多矿石碎片和工具。虽然都不大值钱，但积少成多。",
            "你在矿场外围捡了一些遗留的物品。看起来都被长期风吹雨打了，但还是有用的。",
          ],
          effects: [
            { randomLoot: "默认", fuel: -3 },
            {
              type: "chance",
              chance: 0.2,
              success: {
                message: "地上散落的电缆里有几段铜线，你拽了出来卷好收起。",
                addItems: [{ id: "铜线", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "avoid_mine",
        text: "绕开矿场",
        description: "这太危险了",
        hintUnknown: "绕路应该安全……",
        hintKnown: "燃油 -8，安全但费油",
        result: {
          message: [
            "矿场看起来太不安全了。你决定绕路。",
            "你不想冒生命危险，选择了更安全的路线。",
          ],
          effects: { fuel: -8 },
        },
      },
    ],
  },

  // 农场搜刮
  abandoned_farm: {
    id: "abandoned_farm",
    title: "🌾 废弃农场",
    image: "田！",
    tags: ["废墟"],
    triggerConfig: { char: "田", color: "#84cc16", fontSize: "22px" },
    description:
      "一座被放弃的农场出现在视野里。破旧的谷仓、废弃的田地和倒塌的农具显示这里已经很久没人管理了。",
    oneTime: false,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "raid_farm",
        text: "搜刮农场",
        description: "查看农场里有什么有用的",
        hintUnknown: "农场里也许有物资？",
        hintKnown: "随机结果：45%食物/燃油-4；35%稀有/燃油-5；20%被追赶/耐久-8/舒适-15；15%橡胶",
        result: {
          message: "你停下皮卡，走进了这座被遗弃的农场——",
          effects: [
            // 搜刮废弃农场后解锁物资贩子事件（在农场附近见过他的招牌）
            { unlockEvents: ["supply_merchant"] },
            {
              type: "weighted",
              options: [
                {
                  weight: 45,
                  message: [
                    "谷仓里还有一些储备的粮食。虽然有点陈旧，但吃起来没问题。你装了一些零食和物资。",
                    "农场的工具房里有各种工具。你找到了一些还能使用的东西。",
                  ],
                  effects: { randomLoot: "食物", fuel: -4 },
                },
                {
                  weight: 35,
                  message: [
                    "农场的地下室里有一些保存的物品。虽然有点脏，但看起来还是值钱的。",
                    "你在农场的某个角落发现了一个被遗忘的箱子。里面装着一些看起来很有价值的东西。",
                  ],
                  effects: { randomLoot: "稀有", fuel: -5 },
                },
                {
                  weight: 20,
                  message: [
                    "农场里有一些野生动物。它们对你的到来并不欢迎，追得你狼狈不堪。虽然安全逃脱了，但什么都没捞到。",
                    "农场的一个老蜂窝被你无意中惹恼了。蜜蜂的追击让你仓皇逃离，皮卡还被蛰了几个包。",
                  ],
                  effects: { durability: -8, comfort: -15, fuel: -6 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.15,
              success: {
                message: "农场的灌溉设备旁边有些废弃的橡胶管。你截了一段完好的部分带走。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "harvest_crops",
        text: "采集农作物",
        description: "收集地里还有的农作物",
        hintUnknown: "也许能找到些吃的？",
        hintKnown: "食物类随机物资 / 燃油 -4",
        result: {
          message: [
            "农田里还有一些没有完全枯死的植物。你采集了一些可食用的部分。",
            "虽然田地已经很久没人打理了，但你还是找到了一些有用的植物。",
          ],
          effects: { randomLoot: "食物", fuel: -4 },
        },
      },
      {
        id: "avoid_farm",
        text: "不进去",
        description: "直接通过",
        hintUnknown: "不进去就没事吧？",
        hintKnown: "燃油 -2，安全离开",
        result: {
          message: [
            "你没有停留，直接驶过了这座废弃农场。",
            "继续赶路，不浪费时间。",
          ],
          effects: { fuel: -2 },
        },
      },
    ],
  },

  // 废弃加油站搜刮
  abandoned_gas_station: {
    id: "abandoned_gas_station",
    title: "🔧 废弃加油站",
    image: "站！",
    tags: ["废墟"],
    triggerConfig: { char: "站", color: "#dc2626", fontSize: "22px" },
    description:
      "一座破旧的加油站出现在荒凉的路边。虽然泵已经不工作了，但站里可能还有一些遗留的物品。",
    oneTime: false,
    triggerWeight: 9,
    condition: null,
    choices: [
      {
        id: "search_station",
        text: "彻底搜索",
        description: "仔细查看加油站的每个角落",
        hintUnknown: "仔细搜索收获应该更多？",
        hintKnown: "随机结果：40%燃油物资；30%稀有+耐久+10；30%中毒/舒适-15；12.5%珍品；25%电池",
        result: {
          message: "你停下皮卡，走进了静寂的加油站——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 40,
                  message: [
                    "加油站的办公室里还有一些储备物资！虽然有点尘埃，但都还能用。",
                    "地下的存储室里有一些遗留的物品。你清理干净后装进了皮卡。",
                  ],
                  effects: { randomLoot: "燃油", fuel: -3 },
                },
                {
                  weight: 30,
                  message: [
                    "你在加油站找到了一个尘封的钱箱。虽然里面没有现金，但有一些有价值的工具。",
                    "加油站的修车间里有一些遗留的零件。虽然不是最新的，但对你的皮卡有帮助。",
                  ],
                  effects: { randomLoot: "稀有", fuel: -4, durability: 10 },
                },
                {
                  weight: 30,
                  message: [
                    "地下油箱有渗漏！有毒的气体让你头晕目眩。你赶快逃出加油站，什么都没捞到。",
                    "加油站的结构看起来很不安全。你正在搜索时，一根铁管掉下来，差点砸中你。",
                  ],
                  effects: { comfort: -15, fuel: -5 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.125,
              success: {
                message:
                  "在加油站地下管道的尽头，你发现了一个被遗忘的奇特装置。它的外壳漆黑如沥青，内部的管路中正缓缓渗出黏稠的液滴。你小心翼翼地将它取了出来——它似乎还在运转着。",
                addItems: [{ id: "沥青滴落装置", quantity: 1 }],
              },
            },
            {
              type: "chance",
              chance: 0.25,
              success: {
                message:
                  "在加油站的配电箱后面，你找到了一块还有余电的旧电池。虽然电量不满，但作为材料完全够用。",
                addItems: [{ id: "电池", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "search_quick",
        text: "快速搜索",
        description: "只查看显而易见的地方",
        hintUnknown: "快速搜索比较安全？",
        hintKnown: "默认随机物资 / 燃油 -2 / 15%橡胶（安全稳妥）",
        result: {
          message: [
            "加油站外的货架上还有一些物品。虽然不多，但还是值得收集。",
            "柜台后面有一些遗留的商品。你装了一些进去。",
          ],
          effects: [
            { randomLoot: "默认", fuel: -2 },
            {
              type: "chance",
              chance: 0.15,
              success: {
                message: "柜台下面有几条旧的橡胶管，你截了一段带走。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "check_pumps",
        text: "检查油泵",
        description: "看看能不能从油泵里获得燃油",
        hintUnknown: "旧油泵里还会有油吗？",
        hintKnown: "燃油 +15 / 耐久 -5（补油好选择）",
        result: {
          message: [
            "油泵虽然不工作了，但下面的管道里还有一些残留的燃油。你费了不少力气才把它们抽出来。",
            "你成功地从旧泵的储液器中获得了一些燃油。收获意外的大！",
          ],
          effects: { fuel: 15, durability: -5, fuel: -3 },
        },
      },
    ],
  },

  // ── 旅途的终点...?（经历事件数 ≥ 48 后触发一次）──
  journey_end_question: {
    id: "journey_end_question",
    title: "旅途的终点...?",
    image: "终",
    triggerConfig: { char: "终", color: "#a78bfa", fontSize: "28px" },
    description:
      "路越来越窄，天色变得奇异。一块褪色的路牌立在前方，上面写着三个方向，却没有任何地名。\n\n皮卡的引擎发出一声低沉的鸣响，像是在问你：这段旅程，究竟要往哪里去？",
    oneTime: true,
    triggerWeight: 0,
    tags: ["神秘"],
    choices: [
      {
        id: "jend_fuel",
        text: "沿着燃油的痕迹走",
        description: "那条路上有烧焦的油渍，也许通向某个加油站……或者某场事故。",
        hintUnknown: "那条路不知道通向哪里。",
        hintKnown: "消耗大量燃油，乘客好感度可减轻损失",
        result: {
          message: [
            "你踩下油门，沿着油渍延伸的方向驶去。引擎轰鸣，像在燃烧什么不该燃烧的东西。",
          ],
          effects: { type: "journey_end", stat: "fuel" },
        },
      },
      {
        id: "jend_durability",
        text: "走那条颠簸的山路",
        description: "布满碎石和深坑，每走一步都是对车架的考验。但另一侧似乎有光。",
        hintUnknown: "那条路看起来对车很不友好。",
        hintKnown: "消耗大量耐久，乘客好感度可减轻损失",
        result: {
          message: [
            "皮卡在碎石路上颠簸前行，车身发出痛苦的嘶鸣。你握紧方向盘，告诉自己不能停。",
          ],
          effects: { type: "journey_end", stat: "durability" },
        },
      },
      {
        id: "jend_comfort",
        text: "走那条漫长的荒路",
        description: "空旷、单调、看不到尽头。乘客们开始沉默，空气变得沉重。",
        hintUnknown: "无尽的荒原……会怎样？",
        hintKnown: "消耗大量舒适，乘客好感度可减轻损失",
        result: {
          message: [
            "车厢里安静得只剩引擎声。你望着前方笔直延伸的荒路，感到一种说不清的重量压在胸口。",
          ],
          effects: { type: "journey_end", stat: "comfort" },
        },
      },
    ],
  },
};
