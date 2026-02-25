// 特殊/随机事件 - 奇遇、发现、意外等低概率随机事件

const EVENTS_SPECIAL = {
  mystery_box: {
    id: "mystery_box",
    title: "📦 路边神秘箱子",
    image: "箱！",
    tags: ["神秘"],
    triggerConfig: { char: "箱", color: "#fbbf24", fontSize: "22px" },
    description:
      "路边有一只落满灰尘的金属箱子，没有任何标记。它好像在等待被打开。",
    oneTime: false,
    triggerWeight: 6,
    condition: null,
    choices: [
      {
        id: "open_box",
        text: "打开它",
        description: "有风险，但也许有惊喜",
        hintUnknown: "打开会有什么？也许有陷阱……",
        hintKnown: "50%随机物资/耐久-5；30%稀有/耐久-5；20%陷阱/耐久-10/舒适-20；5%珍品；15%随机升级材料",
        result: {
          message: "你停下车，走过去，蹲下身子，把手放在箱盖上——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 50,
                  message: [
                    "箱子里满是物资！看来上一个主人走得太匆忙了。打开的瞬间车门蹭了一下。",
                    '里面有一张字条："留给有缘人。"字条下面压着不少东西。开箱时手肘碰了下车门。',
                    "箱盖弹开，一股旧金属气息涌出。里面的东西不算新，但都还能用。搬运时车身蹭了几下。",
                  ],
                  effects: { randomLoot: true, durability: -5 },
                },
                {
                  weight: 30,
                  message: [
                    "这箱子里的东西质量不一般！看来上一个主人是个讲究人。只不过，开箱时手肘碰了下车门。",
                    "打开箱盖，里面的物资比你预想的好得多。只不过搬运时车身蹭了几下。",
                  ],
                  effects: { randomLoot: "稀有", durability: -5 },
                },
                {
                  weight: 20,
                  message: [
                    "箱子里有个弹簧装置！铁片弹出来砸到了车门，留下一道凹痕。这是个陷阱！",
                    "箱底漏气，里面全是些废弃物。更糟的是，打开的瞬间箱盖反弹，狠狠砸了一下车身。",
                  ],
                  effects: { durability: -10, comfort: -20 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.05,
              success: { addItems: [{ id: "金马雕像", quantity: 1 }] },
            },
            {
              type: "chance",
              chance: 0.05,
              success: {
                message: "箱子的夹层里还塞着一些铜线——运气不错！",
                addItems: [{ id: "铜线", quantity: 1 }],
              },
            },
            {
              type: "chance",
              chance: 0.05,
              success: {
                message: "箱底的暗格里有一块橡胶垫片，看起来还能用。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
            {
              type: "chance",
              chance: 0.05,
              success: {
                message: "箱子角落里滚出一颗小电池，居然还有电！",
                addItems: [{ id: "电池", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "ignore_box",
        text: "不要动来路不明的东西",
        description: "谨慎路过",
        hintUnknown: "不碰应该最安全？",
        hintKnown: "无任何消耗，安全离开",
        result: {
          message: [
            "你绕过箱子继续走，时不时在后视镜里看一眼，它没有追上来。",
            "安全第一。你没有停车，留箱子继续待在路边等下一个有缘人。",
            "不明物体，不碰为妙。你加快了速度离开。",
          ],
          effects: {},
        },
      },
    ],
  },

  lost_traveler: {
    id: "lost_traveler",
    title: "🗺️ 迷路的旅行者",
    image: "旅！",
    triggerConfig: { char: "旅", color: "#34d399", fontSize: "22px" },
    description:
      "一个背着大背包的旅行者站在岔路口，手里拿着地图转来转去，显然已经迷路了。",
    oneTime: false,
    triggerWeight: 9,
    condition: null,
    choices: [
      {
        id: "give_directions",
        text: "帮他指路",
        description: "好人有好报",
        hintUnknown: "帮助别人应该有回报？",
        hintKnown: "金币 +8 / 舒适 +8 / 全员好感 +5",
        result: {
          message: [
            '旅行者感激地接过你画的简易地图，掏出仅剩的几枚金币塞给你。"谢谢，真的谢谢！"',
            "你花了五分钟帮他搞清楚方向，旅行者激动地握了握你的手，送上路上带的零食和金币。",
            "旅行者听完你的指引，如释重负地笑了笑，从背包里摸出一把零钱感谢你。",
          ],
          effects: { gold: 8, comfort: 8, favorAll: 5 },
        },
      },
      {
        id: "take_aboard",
        text: "顺路带一程",
        description: "捎他一段路",
        hintUnknown: "带上他会有好处吗？",
        hintKnown: "获得临时乘客「旅行者」/ 金币 +5 / 舒适 +10 / 燃油 -8 / 全员好感 +8",
        result: {
          message: [
            "旅行者爬上车，一路不停地说谢谢。他说到最近的镇口还有一段路，会把路上带的零食拿出来分享。",
            "旅行者坐在后座，说要去前方镇子。路上他讲了不少有意思的旅途故事，大家都听得津津有味。",
            '旅行者上了车，感激地指着前方说镇口不远了。他掏出零食分给大家，说"当谢礼"。',
          ],
          effects: {
            addPassenger: "旅行者",
            setPassengerGetOffMileage: { "旅行者": [10, 25] },
            gold: 5,
            comfort: 10,
            fuel: -8,
            favorAll: 8,
          },
        },
      },
      {
        id: "ignore_traveler",
        text: "没空管，继续走",
        description: "时间紧迫",
        hintUnknown: "不理他有什么后果？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "你假装没看见，踩下油门离开。后视镜里那个旅行者还在原地转圈。",
            "你加速驶过，旅行者的求助声被风声盖住了。路还很长，你告诉自己。",
            "继续赶路。或许他自己能找到方向。",
          ],
          effects: {},
        },
      },
    ],
  },

  stray_cat: {
    id: "stray_cat",
    title: "🐱 流浪小猫",
    image: "猫！",
    triggerConfig: { char: "猫", color: "#fb923c", fontSize: "22px" },
    description:
      "一只橘色的小猫坐在路中央，用金色的眼睛平静地看着你的皮卡，完全没有让路的意思。",
    oneTime: false,
    triggerWeight: 8,
    condition: null,
    choices: [
      {
        id: "pet_cat",
        text: "下车撸猫",
        description: "猫猫治愈心灵",
        hintUnknown: "撸猫肯定很治愈吧！",
        hintKnown: "舒适 +7 / 燃油 -3",
        result: {
          message: [
            "小猫抬起头，让你摸了好一会儿。离开时它在原地坐着目送你，尾巴轻轻摇摆。",
            "小猫靠过来蹭了蹭你的手，发出咕噜咕噜的声音。你在原地蹲了五分钟才依依不舍地离开。",
            "小猫很享受你的抚摸，还在你的胳膊上蹭了蹭。这大概是今天最治愈的时刻了。",
          ],
          effects: { comfort: 7, fuel: -3 },
        },
      },
      {
        id: "honk_cat",
        text: "按喇叭让它走",
        description: "赶时间",
        hintUnknown: "猫咪会不会报复？",
        hintKnown: "耐久 -4，猫会抓车",
        result: {
          message: [
            "小猫被吓得一跳，跑进路边草丛。但临走前用爪子刮了一下你的轮毂盖。",
            "喇叭一响，小猫竖起尾巴跑开了，回头给了你一个鄙视的眼神。",
            "猫不慌不忙地走开了，仿佛一切都在它计划之中。只是车门多了道细细的抓痕。",
          ],
          effects: { durability: -4 },
        },
      },
      {
        id: "feed_cat",
        text: "喂点零食",
        description: "分享零食给猫咪",
        hintUnknown: "喂猫会有什么惊喜？",
        hintKnown: "消耗零食×1 / 舒适 +15 / 全员好感 +8 / 有概率猫咪上车！",
        result: {
          message: [
            "小猫低头闻了闻，然后开始专心吃零食。你站在旁边看了好一会儿，感觉整个世界都温柔了。",
            "小猫吃完零食，用头顶了顶你的手表示感谢，然后跳上路边的石头，优雅地消失在草丛中。",
            "零食一放下，小猫立刻不顾形象地扑了上去。吃完之后它爬进了你的车厢，在角落窝成一团。",
          ],
          effects: {
            removeItems: [{ id: "零食", quantity: 1 }],
            comfort: 15,
            fuel: -2,
            favorAll: 8,
            strayCatFeedAndMaybeBoard: true,
          },
        },
      },
    ],
  },

  radio_tower: {
    id: "radio_tower",
    title: "📡 废弃广播塔",
    image: "塔！",
    tags: ["废墟"],
    triggerConfig: { char: "塔", color: "#6366f1", fontSize: "22px" },
    description:
      "路边一座高耸的铁塔，锈迹斑斑，顶上的灯还在慢慢闪烁。附近散落着废旧的电子设备，隐约传来沙沙的噪音。",
    oneTime: false,
    triggerWeight: 6,
    condition: null,
    choices: [
      {
        id: "search_tower",
        text: "搜寻电子废料",
        description: "也许能找到有用的零件",
        hintUnknown: "电子废料里有好东西？",
        hintKnown: "随机物资 / 耐久 -8 / 30%铜线或电池",
        result: {
          message: [
            "你从废旧设备里拆出了不少有价值的东西，代价是车子在乱石堆里蹭了几下。",
            "电子废料里藏着不少好东西，你折腾了一阵，收获颇丰，皮卡被铁架子刮了道口子。",
            "你撬开几个控制柜，发现里面的元件还没完全氧化，收了不少有用的材料。车顶被掉落的铁片砸了一下。",
          ],
          effects: [
            { randomLoot: true, durability: -8 },
            {
              type: "chance",
              chance: 0.3,
              success: {
                message: "控制柜里的线路板上，你拆下了一些完好的铜线和一块小电池。",
                addItems: [
                  { id: "铜线", quantity: 1 },
                  { id: "电池", quantity: 1 },
                ],
              },
            },
          ],
        },
      },
      {
        id: "tune_radio",
        text: "尝试调出信号",
        description: "听听有没有广播",
        hintUnknown: "也许能收到有趣的节目？",
        hintKnown: "舒适 +8 / 燃油 -5；2.5%概率外星信号",
        result: {
          message: [
            "经过一番调试，收音机里传出了一首老歌，车厢里的气氛瞬间轻松了不少。",
            "噪音中断断续续传来一段广播，是个说书的节目。大家安静地听了一路，心情都好了。",
            "信号时断时续，但偶尔能听到几句清晰的音乐。已经很久没听到音乐了，感觉不错。",
          ],
          effects: [
            { comfort: 8, fuel: -5 },
            {
              type: "chance",
              chance: 0.025,
              success: {
                message:
                  "突然，收音机发出刺耳的啸叫！天空闪过一道绿光，一艘飞碟降落在公路上。一群马头外星人从中走出，围着你的皮卡比比划划。你还没来得及反应，它们已经开始改造你的车了——引擎被拆开、重组，一颗散发荧绿光芒的核心被嵌入其中。马头外星人们满意地嘶鸣了几声，登上飞碟扬长而去。你的皮卡……好像变轻了？",
                addItems: [{ id: "马头皮卡核心", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "drive_on",
        text: "不做停留",
        description: "继续赶路",
        hintUnknown: "直接走应该没事？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "铁塔在后视镜里缓缓缩小，消失在尘土中。",
            "你看了一眼广播塔，觉得没什么值得停留的，继续赶路。",
            "广播塔的灯在你离开后还在闪烁，像是在目送你。",
          ],
          effects: {},
        },
      },
    ],
  },

  sandstorm: {
    id: "sandstorm",
    title: "🌪️ 沙尘暴来袭",
    image: "沙！",
    tags: ["视线模糊", "危险"],
    triggerConfig: { char: "沙", color: "#d97706", fontSize: "22px" },
    theme: {
      emojis: ["☁️", "☁️", "☁️"],
      animation: "sandstorm",
      bgColor: "#2a1f0e",
      borderColor: "#d97706",
      barColors: { fuel: "#b45309", durability: "#92400e", comfort: "#78350f" },
    },
    description:
      "天边一堵褐色的墙壁正以惊人的速度逼近，那是沙尘暴！细沙开始击打挡风玻璃，发出沙沙的声音。",
    oneTime: false,
    triggerWeight: 5,
    condition: null,
    choices: [
      {
        id: "race_storm",
        text: "全速冲出去",
        description: "赌一把！",
        hintUnknown: "冲出去能成功吗？风险未知……",
        hintKnown: "40%燃油-10/耐久-8；60%燃油-20/耐久-25（高风险！）；20%珍品",
        result: {
          message: "你猛踩油门，引擎嘶吼着冲向沙墙——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 40,
                  message: [
                    "皮卡擦着沙暴边缘飞驰而过，险之又险地甩开了沙墙。油耗不小，但车身完整。",
                    "你判断准确，从沙暴侧翼撕开了一条缝隙冲了出去。震耳欲聋的风声在身后渐渐远去。",
                  ],
                  effects: { fuel: -10, durability: -8 },
                },
                {
                  weight: 60,
                  message: [
                    "砂砾铺天盖地打在车上，能见度瞬间归零。你靠感觉硬撑着冲过去，代价惨烈。",
                    "沙暴比预想凶猛得多！皮卡被卷进最密集的沙尘中，轮胎打滑，车身被砂砾打得坑坑洼洼。",
                  ],
                  effects: { fuel: -20, durability: -25 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.2,
              success: { addItems: [{ id: "海市蜃楼雕塑", quantity: 1 }] },
            },
          ],
        },
      },
      {
        id: "shelter",
        text: "寻找掩体躲避",
        description: "原地找地方躲",
        hintUnknown: "躲避应该比较安全？",
        hintKnown: "燃油 -18（等待沙暴消耗不少燃油）",
        result: {
          message: [
            "你把皮卡开进路边一座土坡背面，大家在车里等待沙暴过去。沙暴过后一切正常，就是憋闷了好一阵。",
            "找了处山岩背风处停车，发动机开着取暖，等到风沙渐止。乘客们无聊地打牌，气氛还不错。",
            "躲过了最猛烈的沙暴，出来后皮卡盖了层沙子，但大家毫发无损。",
          ],
          effects: { fuel: -18 },
        },
      },
    ],
  },

  // 废弃村庄事件
  abandoned_village: {
    id: "abandoned_village",
    title: "🏘️ 废弃村庄",
    image: "村！",
    tags: ["废墟"],
    triggerConfig: { char: "村", color: "#9f1239", fontSize: "22px" },
    description:
      "路边出现了一个被时间遗忘的废弃村庄。破旧的房屋沉默地站立，仿佛在讲述往日的故事。风吹过，传来沙沙的声响。",
    oneTime: false,
    triggerWeight: 5,
    condition: null,
    choices: [
      {
        id: "explore_village",
        text: "进村搜索",
        description: "冒险深入探索",
        hintUnknown: "废弃村庄里有什么？可能很危险……",
        hintKnown: "解锁神秘石碑事件 / 随机结果：45%无收获；35%物资/耐久-6；20%受惊/舒适-15；20%铜线",
        result: {
          message: "你停下皮卡，走进了这座诡异的废弃村庄——",
          effects: [
            // 探索村庄后解锁神秘石碑事件（发现古代遗迹的线索）
            { unlockEvents: ["ancient_monument"] },
            {
              type: "weighted",
              options: [
                {
                  weight: 45,
                  message: [
                    "村子里几乎一无所有，只找到了些生锈的家具和一张泛黄的照片。失望地走出来时天色已晚。",
                    "翻过几间房子，只发现了些无用的破烂。倒是在一面镜子里看到了自己疲惫的样子。",
                  ],
                  effects: { fuel: -8, comfort: -8 },
                },
                {
                  weight: 35,
                  message: [
                    "村子里发现了一些保存完好的日用品和工具！虽然进退两难地搜寻了好久，但收获不错。",
                    "在一间半倒的房屋里，你找到了一个装满零件的旧箱子。搬运时皮卡被残破的木板刮了几下。",
                  ],
                  effects: { randomLoot: true, durability: -6 },
                },
                {
                  weight: 20,
                  message: [
                    "黑暗中传来奇怪的声响，你加快了脚步。冲回皮卡时不小心磕到了杂物，刮伤了胳膊。",
                    "一阵不明来源的风声吓了你一跳，你惊慌地跑回车上，启动引擎准备逃离。",
                  ],
                  effects: { comfort: -15, fuel: -3 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.2,
              success: {
                message: "在一间房屋的墙壁里，你发现了被藏起来的电线。拽出来后得到了一些铜线。",
                addItems: [{ id: "铜线", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "avoid_village",
        text: "绕过村庄",
        description: "这个地方看起来不吉利",
        hintUnknown: "不进去应该比较安全？",
        hintKnown: "燃油 -5，安全绕行",
        result: {
          message: [
            "你加速驶过废弃村庄，在后视镜里看了它最后一眼。有些地方还是不要惹为妙。",
            "你从另一条路绕过去，那些破败的房屋逐渐消失在视野里。",
            "直觉告诉你这里不是个好地方。你没有停车，继续向前赶路。",
          ],
          effects: { fuel: -5 },
        },
      },
    ],
  },

  // 神秘石碑事件
  ancient_monument: {
    id: "ancient_monument",
    title: "🗿 神秘石碑",
    image: "碑！",
    tags: ["神秘"],
    triggerConfig: { char: "碑", color: "#6f46c6", fontSize: "22px" },
    description:
      "路边矗立着一块布满苔藓和古老花纹的石碑。你无法理解上面刻着的文字，但有种莫名的吸引力。",
    oneTime: false,
    triggerWeight: 4,
    condition: null,
    choices: [
      {
        id: "study_monument",
        text: "停下来研究",
        description: "仔细观察这块石碑",
        hintUnknown: "石碑上的秘密值得研究吗？",
        hintKnown: "触发二级选择：打开(稀有物资/舒适+5)或留下(舒适+8/燃油+10)",
        result: {
          message: [
            "你停下车，走近石碑，尝试破译上面的文字。虽然没有完全理解，但感觉得到了一种神秘的力量。",
            "石碑上的图案很复杂，你花了好一会儿才看出一点门道。也许这里曾经很重要。",
            "你用手指描摹石碑上的纹理，仿佛能感受到历史的沧桑。",
          ],
          effects: {
            type: "choice",
            prompt: "你发现石碑底部有一个隐藏的空间，里面似乎装着什么东西...",
            options: [
              {
                text: "打开查看",
                description: "好奇心战胜了理性",
                message: [
                  "你小心地打开了空间，里面有一件古老的物件。虽然不知道用途，但看起来值些钱。",
                  "空间里躺着一个布娃娃和一些古旧的钱币。你把它们收进包里。",
                ],
                effects: {
                  randomLoot: "稀有",
                  comfort: 5,
                },
              },
              {
                text: "留下来原地",
                description: "这些东西不应该被打扰",
                message: [
                  "你决定尊重历史，没有去打开那个隐藏的空间。但是在心中获得了一种内心的宁静。你感觉有什么变多了。",
                  "也许有些东西不应该被打搅。你留下它们，驾车离开。你感觉有什么东西变多了。",
                ],
                effects: { comfort: 8, fuel: 10 },
              },
            ],
          },
        },
      },
      {
        id: "touch_monument",
        text: "触摸石碑",
        description: "伸手触碰那些古老的纹路",
        hintUnknown: "触摸它会怎样？纹路在微微发光……",
        hintKnown: "舒适-5/耐久-3；75%舒适-25/25%耐久+10；31%概率获得珍品",
        result: {
          message: [
            "你将手掌贴上石碑表面，冰凉的石头下似乎有什么东西在脉动。纹路突然亮了起来，一股奇异的力量涌入你的指尖——",
            "指尖触碰到石碑的一刹那，上面的文字开始旋转、跳跃。一阵眩晕袭来，你赶紧扶住了石碑——",
          ],
          effects: [
            { durability: -3, comfort: -5 },
            {
              type: "weighted",
              options: [
                {
                  weight: 75,
                  message: "石碑上的纹路灼烧了你的皮肤，一阵刺痛让你不由得后退。那种不安的感觉久久无法散去。",
                  effects: { comfort: -25 },
                },
                {
                  weight: 25,
                  message: "石碑散发出温暖的光芒，你感到车身的某处裂痕正在被一股神秘力量修复。",
                  effects: { durability: 10 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.3125,
              success: { addItems: [{ id: "呓语之书", quantity: 1 }] },
            },
          ],
        },
      },
      {
        id: "pass_monument",
        text: "匆匆路过",
        description: "不想浪费时间",
        hintUnknown: "路过不会有损失吧？",
        hintKnown: "燃油 -2",
        result: {
          message: [
            "你瞥了一眼石碑，继续开车。这种古老的东西对你没什么帮助。",
            "没有停留的欲望，你继续前行。石碑在身后逐渐缩小。",
          ],
          effects: { fuel: -2 },
        },
      },
    ],
  },

  // 萤火虫夜景事件
  firefly_night: {
    id: "firefly_night",
    title: "✨ 萤火虫之夜",
    image: "萤！",
    tags: ["夜晚"],
    triggerConfig: { char: "萤", color: "#fbbf24", fontSize: "22px" },
    description:
      "夜晚降临，突然一片闪烁的光芒包围了皮卡。数百只发光的萤火虫在黑暗中翩翩起舞，如同天空中落下的星星。",
    oneTime: false,
    triggerWeight: 7,
    condition: null,
    choices: [
      {
        id: "enjoy_fireflies",
        text: "停下欣赏",
        description: "享受这份自然的美景",
        hintUnknown: "欣赏萤火虫应该很美好？",
        hintKnown: "舒适 +18 / 燃油 -5 / 12.5%概率获得珍品",
        result: {
          message: [
            "你停下皮卡，熄灭引擎。乘客们都下车了，仰头看着萤火虫在夜空中舞蹈。大家的脸上都洋溢着笑容。",
            "这是一个难得的宁静时刻。萤火虫的光芒照亮了你们疲惫的心。",
            "整个世界仿佛都闪闪发光。你和乘客们一起沉浸在这份梦幻的夜景中，暂时忘记了旅途的艰辛。",
          ],
          effects: [
            { comfort: 18, fuel: -5 },
            {
              type: "chance",
              chance: 0.125,
              success: { addItems: [{ id: "萤火虫之愿", quantity: 1 }] },
            },
          ],
        },
      },
      {
        id: "collect_fireflies",
        text: "尝试捕捉",
        description: "收集一些萤火虫",
        hintUnknown: "捕捉萤火虫会有什么用？",
        hintKnown: "随机物资 / 舒适 +12 / 燃油 -8",
        result: {
          message: [
            "你追逐着萤火虫，小心地捕捉了几只放进一个透明的容器里。它们的光芒在黑暗中显得特别温暖。",
            "经过一番追逐，你成功地收集了一些萤火虫。它们在容器里闪闪发光，就像装着一瓶星光。",
          ],
          effects: {
            randomLoot: true,
            comfort: 12,
            fuel: -8,
          },
        },
      },
      {
        id: "drive_through_fireflies",
        text: "缓速驶过",
        description: "继续行驶，融入这场光的盛宴",
        hintUnknown: "慢慢开过去应该也很美？",
        hintKnown: "舒适 +10 / 燃油 -6",
        result: {
          message: [
            "你以极慢的速度驾驶着皮卡，穿过萤火虫群。光芒在车前闪烁，如同在光的隧道中行驶。",
            "皮卡缓缓前进，四周的萤火虫伴随着你的旅程。这是一段难忘的夜间驾驶。",
          ],
          effects: { comfort: 10, fuel: -6 },
        },
      },
    ],
  },

  // 暴雨泥泞事件
  muddy_downpour: {
    id: "muddy_downpour",
    title: "🌧️ 暴雨泥泞路",
    image: "泥！",
    tags: ["雨天", "危险"],
    triggerConfig: { char: "泥", color: "#78350f", fontSize: "22px" },
    theme: {
      emojis: ["🌧️", "💧", "💦", "🌊"],
      animation: "rain",
      bgColor: "#1c1917",
      borderColor: "#78350f",
      barColors: { fuel: "#a16207", durability: "#92400e", comfort: "#78716c" },
    },
    description:
      "突然大雨倾盆而下！路面迅速变成了泥泞的沼泽。皮卡的轮胎深陷其中，发动机发出吃力的轰鸣。",
    oneTime: false,
    triggerWeight: 6,
    condition: null,
    choices: [
      {
        id: "push_through_mud",
        text: "冲出去",
        description: "猛踩油门穿过泥路",
        hintUnknown: "看起来很冒险，后果难料……",
        hintKnown: "50%概率：耐久-18/燃油-15；50%概率：耐久-25/燃油-20/舒适-20",
        result: {
          message: "你深吸一口气，踩下油门——",
          effects: {
            type: "weighted",
            options: [
              {
                weight: 50,
                message: [
                  "轮胎在泥里打转，车身摇晃剧烈。但最后还是冲了出去！代价是引擎磨损和满身泥浆。",
                  "皮卡在泥里挣扎，轮胎飞溅出大量泥浆。经过几分钟的奋力，终于驶上了干地。",
                ],
                effects: { durability: -18, fuel: -15 },
              },
              {
                weight: 50,
                message: [
                  "糟糕！轮胎彻底陷进去了，皮卡动弹不得。你只能下车推车，全身沾满了泥。",
                  "皮卡在泥里越陷越深，轮胎转不动了。你和乘客一起下车，花了好大力气才把车从泥里拔出来。",
                ],
                effects: { durability: -25, fuel: -20, comfort: -20 },
              },
            ],
          },
        },
      },
      {
        id: "find_alternate_route",
        text: "寻找绕路",
        description: "找一条更安全的路",
        hintUnknown: "绕路应该安全一些？",
        hintKnown: "燃油 -12 / 耐久 +8（安全稳妥）",
        result: {
          message: [
            "你往周围看了看，发现了一条石子路。虽然绕远了，但总比陷在泥里好。",
            "好在前方有条山路还比较干燥。你决定绕过这片泥地。",
            "你停下来，认真地研究了地形，找到了一条可以通行的替代路线。",
          ],
          effects: { fuel: -12, durability: 8 },
        },
      },
      {
        id: "wait_weather",
        text: "停车等待",
        description: "等雨停再说",
        hintUnknown: "等一等总没错吧？",
        hintKnown: "燃油 -12 / 舒适 +8（最舒适的选择）",
        result: {
          message: [
            "你停下皮卡，在车里等待。大雨逐渐减弱，约半小时后天气转晴，路面也干得差不多了。",
            "躲在车里听雨声，乘客们打起了牌。等雨停后，大家都精神了不少。",
          ],
          effects: { fuel: -12, comfort: 8 },
        },
      },
    ],
  },

  // 偶遇流浪艺人事件
  wandering_performer: {
    id: "wandering_performer",
    title: "🎭 流浪艺人",
    image: "艺！",
    triggerConfig: { char: "艺", color: "#ec4899", fontSize: "22px" },
    description:
      "一个穿着破旧但色彩鲜艳的艺人站在路边，正在为一个看不见的观众表演。他注意到了你的皮卡，做出了一个夸张的邀请手势。",
    oneTime: false,
    triggerWeight: 7,
    condition: { notPassenger: "流浪艺人" },
    choices: [
      {
        id: "watch_performance",
        text: "停下观看",
        description: "看看这位艺人的表演",
        hintUnknown: "看表演要花钱吗？",
        hintKnown: "舒适 +12 / 金币 -3 / 全员好感 +8",
        result: {
          message: [
            "艺人献上了一段精彩的杂技表演。虽然有点粗糙，但他的热情感染了所有人，大家都笑了起来。",
            "艺人表演了一场即兴的单人话剧，虽然情节古怪，但非常逗趣。乘客们笑得眼泪都流出来了。",
            "艺人变出了一些简单的戏法，虽然不精妙，但气氛很欢乐。",
          ],
          effects: { comfort: 12, gold: -3, favorAll: 8 },
        },
      },
      {
        id: "give_performance",
        text: "邀请上车表演",
        description: "让艺人登上皮卡表演",
        hintUnknown: "让艺人上车会怎样？",
        hintKnown: "获得乘客「流浪艺人」/ 舒适 +15 / 金币 -5 / 耐久 -8 / 全员好感 +10",
        result: {
          message: [
            "艺人喜出望外地跳上了皮卡，在后车厢里表演了一路。虽然颠簸，但他的表演让大家忘记了旅途的疲劳。",
            "艺人上车后，在行驶的皮卡上进行了一段惊险的平衡表演。乘客们叹为观止，还给了他一些零钱。",
          ],
          effects: {
            addPassenger: "流浪艺人",
            comfort: 15,
            gold: -5,
            durability: -8,
            favorAll: 10,
          },
        },
      },
      {
        id: "ignore_performer",
        text: "无视艺人",
        description: "继续赶路",
        hintUnknown: "不理他有什么后果？",
        hintKnown: "无任何消耗",
        result: {
          message: [
            "你没有理会艺人，踩下油门继续前行。艺人在身后做出了一个夸张的失望手势。",
            "你装作没看见，继续开车。艺人的表演声在风中渐渐消失。",
          ],
          effects: {},
        },
      },
    ],
  },

  // 陨石坑事件
  meteor_crater: {
    id: "meteor_crater",
    title: "☄️ 陨石坑",
    image: "坑！",
    tags: ["危险", "神秘"],
    triggerConfig: { char: "坑", color: "#713f12", fontSize: "22px" },
    description:
      "前方出现了一个巨大的圆形坑洞，中心还闪闪发光。这看起来像是陨石撞击留下的痕迹。坑底隐约传来奇异的声响。",
    oneTime: true,
    triggerWeight: 3,
    condition: null,
    choices: [
      {
        id: "explore_crater",
        text: "下去探索",
        description: "冒险深入陨石坑",
        hintUnknown: "陨石坑底有什么？非常危险……",
        hintKnown: "30%稀有/耐久-15；40%普通/耐久-20；30%迷失/耐久-25/舒适-15（极高风险）；20%橡胶",
        result: {
          message: "你小心地开着皮卡，驶进了陨石坑——",
          effects: [
            {
              type: "weighted",
              options: [
                {
                  weight: 30,
                  message: [
                    "坑底有一块黑色的陨铁！它散发着神秘的光芒。你费力地把它搬上皮卡。",
                    "在坑底发现了一些奇异的矿物。你收集了一些，虽然不知道用处，但感觉很珍贵。",
                  ],
                  effects: { randomLoot: "稀有", durability: -15, fuel: -10 },
                },
                {
                  weight: 40,
                  message: [
                    "坑底到处是碎石和灰烬，没什么特别的收获。倒是在爬出坑时，皮卡被几块尖锐的石头割伤了。",
                    "除了一些普通的碎石外，没什么有价值的东西。代价是车身被磨损了不少。",
                  ],
                  effects: { randomLoot: true, durability: -20 },
                },
                {
                  weight: 30,
                  message: [
                    "坑很深，而且下去后你发现了奇异的光。你被吸引了几个小时，完全丧失了时间观念。",
                    "在坑底你感觉到了一种奇异的能量。下来一趟消耗了大量的体力和燃油。",
                  ],
                  effects: { durability: -25, fuel: -20, comfort: -15 },
                },
              ],
            },
            {
              type: "chance",
              chance: 0.2,
              success: {
                message: "陨石坑边缘的地面上散落着一些受热变形的橡胶碎片，你捡了几块大的带走。",
                addItems: [{ id: "橡胶", quantity: 1 }],
              },
            },
          ],
        },
      },
      {
        id: "bypass_crater",
        text: "绕过陨石坑",
        description: "这太危险了",
        hintUnknown: "安全绕行？",
        hintKnown: "燃油 -8 / 舒适 +5（安全选择）",
        result: {
          message: [
            "你没有冒险进入坑底，而是小心地绕过了陨石坑。离开前你拍了几张照片作为纪念。",
            "面对这个神秘的陨石坑，理智战胜了好奇心。你绕过了它，继续赶路。",
            "你决定不冒险。安全第一。绕路的代价是消耗了更多燃油。",
          ],
          effects: { fuel: -8, comfort: 5 },
        },
      },
    ],
  },

  // ────────────────────────────────────────────────────────
  // 乘客专属事件
  // ────────────────────────────────────────────────────────

  // 鹿的专属事件
  deer_nostalgia: {
    id: "deer_nostalgia",
    title: "🦌 鹿的记忆",
    image: "鹿！",
    triggerConfig: { char: "忆", color: "#d4a574", fontSize: "22px" },
    description:
      "路边一片茂密的树林，空气中弥漫着青草的香气。坐在后车厢的鹿抬起头，眼神渐渐飘向远方。它似乎看到了什么熟悉的东西——曾经的家乡。",
    oneTime: false,
    triggerWeight: 4,
    condition: {
      requiresPassenger: "鹿",
    },
    choices: [
      {
        id: "stop_forest",
        text: "停车让它休息",
        description: "给鹿一些时间去寻找过去",
        hintUnknown: "让鹿回忆过去……会有好事？",
        hintKnown: "舒适 +12 / 燃油 -3 / 获得鹿角护符 / 鹿好感 +15",
        result: {
          message: [
            "你停下车。鹿轻轻跳下车，慢慢走向树林。它在那里停留了一会儿，似乎在回忆。回来时，它的眼神更加温和了。",
            "鹿用鼻子嗅了嗅空气中的味道，迈步向林地走去。等了一会儿，它回来了，仿佛获得了心灵的宁静。",
            "林中的某个地方似乎唤醒了鹿心中的某些记忆。它靠在你的肩上，发出轻微的鹿鸣。",
          ],
          effects: {
            comfort: 12,
            fuel: -3,
            addItems: [{ id: "鹿角护符", quantity: 1 }],
            favor: { 鹿: 15 },
          },
        },
      },
      {
        id: "keep_driving",
        text: "继续赶路",
        description: "不能为了一只鹿浪费时间",
        hintUnknown: "不停车鹿会失望？",
        hintKnown: "舒适 -8 / 燃油 -2 / 鹿好感 -10",
        result: {
          message: [
            "你没有停车。鹿的眼神渐渐黯淡下来，回到了车厢的角落。",
            "你踩下油门，继续前进。鹿在后座上无声地叹了口气。",
            "赶路最重要。但你从后视镜看到鹿的背影，似乎有些失落。",
          ],
          effects: { comfort: -8, fuel: -2, favor: { 鹿: -10 } },
        },
      },
      {
        id: "give_food",
        text: "给鹿喂点零食",
        description: "温暖一下它的心",
        hintUnknown: "零食能安慰鹿吗？",
        hintKnown: "消耗零食×1 / 舒适 +15 / 鹿好感 +10",
        result: {
          message: [
            "你掏出一些零食递给鹿。它接受了你的好意，用头蹭了蹭你的手。虽然回不到过去，但此刻有你陪伴。",
            "零食虽然不能替代家乡，但鹿吃得很开心。它用眼神感谢了你。",
          ],
          effects: {
            removeItems: [{ id: "零食", quantity: 1 }],
            comfort: 15,
            favor: { 鹿: 10 },
          },
        },
      },
    ],
  },

  // 猎人的专属事件
  hunter_and_deer: {
    id: "hunter_and_deer",
    title: "🏹 猎人的改变",
    image: "心！",
    triggerConfig: { char: "心", color: "#8b7355", fontSize: "22px" },
    description:
      "车上的猎人看着后车厢里的鹿，沉默了片刻。他没有再提起往日的狩猎，反而低声讲起了一些过往的故事。",
    oneTime: true,
    triggerWeight: 3,
    condition: {
      requiresPassenger: "猎人",
    },
    choices: [
      {
        id: "stop_and_talk",
        text: "停下来交流",
        description: "让猎人诉说他心中的故事",
        hintUnknown: "和猎人深谈会有什么结果？",
        hintKnown: "金币 +25 / 舒适 +20 / 稀有物资 / 猎人徽章 / 猎人&鹿好感 +15（大丰收！）",
        result: {
          message: [
            "你停下车。猎人走近，看着鹿，缓缓诉说起自己的故事。原来他曾失去过一个亲密的人，那时他放弃了狩猎，转而去帮助他人。看到你们，他想起了那份救赎。",
            "猎人轻轻摸了摸鹿的头。'我以前用枪，现在我用心。'他说完这句话，给了你一些贵重的猎人装备作为纪念。",
            "双方有了一个深层次的理解。猎人决定再也不伤害任何无辜的生命。",
          ],
          effects: {
            gold: 25,
            comfort: 20,
            randomLoot: "稀有",
            addItems: [{ id: "猎人徽章", quantity: 1 }],
            favor: { 猎人: 15, 鹿: 15 },
          },
        },
      },
      {
        id: "hide_deer",
        text: "隐藏鹿",
        description: "假装鹿不在",
        hintUnknown: "藏起鹿会怎样？",
        hintKnown: "舒适 -12 / 鹿好感 -8",
        result: {
          message: [
            "你赶紧把鹿藏在毛毯下面。猎人走近时没有看出异常，最后失望地转身离开。鹿在毛毯下发出了无声的抗议。",
            "你加速驶过，背后传来猎人无奈的叹气声。鹿在你身边瑟缩了起来。",
          ],
          effects: { comfort: -12, favor: { 鹿: -8 } },
        },
      },
      {
        id: "speed_past",
        text: "加速离开",
        description: "不想冒任何风险",
        hintUnknown: "直接跑？",
        hintKnown: "燃油 -10 / 舒适 -5 / 鹿好感 -5",
        result: {
          message: [
            "你猛踩油门冲过去。猎人在身后喊了些什么，但你已经听不清了。",
            "皮卡在尘土中疾驰，留下猎人孤独的身影。鹿紧张地蜷缩在车厢里。",
          ],
          effects: { fuel: -10, comfort: -5, favor: { 鹿: -5 } },
        },
      },
    ],
  },

  // 骚福瑞的专属事件（疯狂且危险的事件）
  saofurry_chaos: {
    id: "saofurry_chaos",
    title: "🐾 骚福瑞的狂欢",
    image: "狂！",
    tags: ["危险"],
    triggerConfig: { char: "狂", color: "#ff69b4", fontSize: "22px" },
    description:
      "骚福瑞突然激动起来！它尖叫着指向前方——一场大型音乐节正在举行。音乐声、人群声混成一片，骚福瑞的眼睛开始发光。'我们必须停下！'它用不可抗拒的命令式语气说。",
    oneTime: false,
    triggerWeight: 3,
    condition: {
      requiresPassenger: "骚福瑞",
    },
    choices: [
      {
        id: "join_party",
        text: "停车加入派对",
        description: "随骚福瑞疯狂一番",
        hintUnknown: "加入派对会很疯狂？",
        hintKnown: "舒适 +25 / 耐久 -15 / 燃油 -8 / 金币 +15 / 骚福瑞手办 / 骚福瑞好感 +20",
        result: {
          message: [
            "骚福瑞拉着你冲向人群。它在舞台上跳起了夸张的舞蹈，所有人都被它的热情感染了。尽管混乱，但这是一个难忘的夜晚。",
            "你们在音乐节上度过了狂野的几个小时。骚福瑞甚至成为了临时DJ，播放它喜欢的歌曲。虽然皮卡被不小心碰撞了几次，但气氛太棒了。",
            "现场的人都喜欢上了骚福瑞。它简直就是派对的灵魂。回到皮卡时，所有人都给了你们热烈的掌声。",
          ],
          effects: {
            comfort: 25,
            durability: -15,
            fuel: -8,
            gold: 15,
            addItems: [{ id: "骚福瑞手办", quantity: 1 }],
            favor: { 骚福瑞: 20 },
          },
        },
      },
      {
        id: "resist",
        text: "拒绝骚福瑞",
        description: "继续赶你的路",
        hintUnknown: "拒绝骚福瑞它会暴怒吧……",
        hintKnown: "耐久 -20 / 舒适 -25 / 燃油 -5 / 骚福瑞好感 -25（后果严重！）",
        result: {
          message: [
            "你拒绝了骚福瑞的请求。它在车里尖叫、踢踏，甚至破坏了你的收音机。你不得不花费时间修复。",
            "骚福瑞生气了，在皮卡里大闹一场。它踩烂了几个零食，还用爪子抓了车座。",
            "你坚定地继续前进，骚福瑞的咆哮声充满了怨恨和失望。它在后座上用力跺脚，几乎把车底板踏穿。",
          ],
          effects: { durability: -20, comfort: -25, fuel: -5, favor: { 骚福瑞: -25 } },
        },
      },
      {
        id: "compromise",
        text: "妥协 - 停留一小时",
        description: "找个平衡点",
        hintUnknown: "折中方案会怎样？",
        hintKnown: "舒适 +12 / 燃油 -4 / 金币 +5 / 耐久 +5 / 骚福瑞好感 +10（最佳选择）",
        result: {
          message: [
            "你同意停留一个小时。骚福瑞开心地哼着歌，在派对边缘跳舞。时间一到，它意外地很痛快地上了车。",
            "骚福瑞在音乐节的外围享受了一会儿，甚至和一些人交了朋友。它心满意足地回到了皮卡。",
            "折中方案让双方都满意。骚福瑞心情愉悦，甚至帮你清理了一下皮卡。",
          ],
          effects: { comfort: 12, fuel: -4, gold: 5, durability: 5, favor: { 骚福瑞: 10 } },
        },
      },
    ],
  },

  // ─── 古老的神庙 ──────────────────────────────────────────
  ancient_temple: {
    id: "ancient_temple",
    title: "🏛️ 古老的神庙",
    image: "🏛️",
    tags: ["神秘", "废墟"],
    triggerConfig: { char: "庙", color: "#a78bfa", fontSize: "22px" },
    description:
      "路边出现了一座被藤蔓覆盖的古老石质神庙。入口处的石碑上刻满了奇异的符号，像是某种古代谜题。空气中弥漫着檀香的气息。",
    oneTime: true,
    triggerWeight: 0, // 不参与常规抽取，由强制触发逻辑插入
    condition: null,
    theme: { borderColor: "#a78bfa" },
    choices: [
      {
        id: "solve_puzzle",
        text: "尝试解开石碑上的谜题",
        description: "石碑上的符号排列似乎暗藏规律……",
        hintUnknown: "也许能获得一些报酬？",
        hintKnown: "扫雷谜题，60秒内每正确标记一个地雷获得1金币，正确>10获得珍品「密钥」",
        result: {
          message: "你走近石碑，符号开始微微发光，排列成一个方形阵列。你意识到这是一个地雷探测谜题……",
          effects: { openMinesweeper: true },
        },
      },
      {
        id: "pray",
        text: "在神庙前祈祷",
        description: "虔诚地祈祷也许会得到庇佑",
        hintUnknown: "神明是否还在倾听？",
        hintKnown: "获得3种随机物资（各1~2个）",
        result: {
          message: [
            "你在神庙前合掌祈祷。一阵微风拂过，地面上浮现出几件物资，像是神明的馈赠。",
            "古老的神像似乎微微颔首。供台上凭空出现了几件物品，散发着淡淡的光芒。",
          ],
          effects: { templeOffering: true },
        },
      },
      {
        id: "leave_temple",
        text: "敬畏地离开",
        description: "不去打扰这片古老的宁静",
        hintUnknown: "安全的选择。",
        hintKnown: "舒适+12%，耐久+12%，金币+8",
        result: {
          message: [
            "你对着神庙鞠了一躬，然后默默离开。神庙的宁静似乎赐予了你力量。",
            "你望了一眼那座古老的建筑，选择继续赶路。临走时一阵暖风拂过，你感到精神焕发。",
          ],
          effects: { comfort: 12, durability: 12, gold: 8 },
        },
      },
    ],
  },

  imaginary_phenomenon: {
    id: "imaginary_phenomenon",
    title: "⚠️ 虚数现象",
    image: "⚠️",
    tags: ["神秘"],
    triggerConfig: { char: "虚", color: "#22d3ee", fontSize: "22px" },
    description:
      "公路两侧的景色开始故障、扭曲，像是显示器坏掉的像素块在不断闪烁。空气里弥漫着一股焦糊的电气味。就在路边，悬浮着一个不属于这个世界的文件夹——半透明的，图标上闪烁着你看不懂的权限符号。你口袋里的密钥忽然发烫。",
    oneTime: true,
    triggerWeight: 0,
    condition: { requiresItem: "密钥" },
    choices: [
      {
        id: "use_key",
        text: "🔑 用密钥打开文件夹",
        description: "密钥与文件夹之间似乎存在某种共鸣",
        hintUnknown: "密钥会怎样？文件夹里有什么？",
        hintKnown: "消耗密钥，获得「一次性管理员权限」（珍品）——持有时开启 Debug，使用后自毁",
        result: {
          message: [
            "你将密钥对准文件夹的图标——钥匙发出耀眼的蓝光，随后碎成无数像素散逸在空气里。文件夹缓缓打开，一个蓝色的令牌从里面漂出，悬停在你手边。",
            "密钥触碰到文件夹的瞬间，整条公路闪烁了一次，像是整个世界重启了帧数。当你睁开眼，密钥已经消失，一张半透明的权限卡正悬浮在你手掌上方。",
          ],
          effects: {
            removeItems: [{ id: "密钥", quantity: 1 }],
            addItems: [{ id: "一次性管理员权限", quantity: 1 }],
          },
        },
      },
      {
        id: "be_careful",
        text: "🪤 还是小心为上",
        description: "不明来路的东西，碰了说不定有什么麻烦",
        hintUnknown: "谨慎一些也许是对的……",
        hintKnown: "随机获得3件物资，密钥保留",
        result: {
          message: [
            "你把手缩了回去，盯着那个文件夹看了许久。它好像感受到了你的拒绝，缓缓上升，消失在扭曲的天空里。你低头看看密钥，它安静地躺在口袋里，不再发烫。回到车上，你发现副驾上多了几件不知从哪来的物资。",
            "你深吸一口气，没有动手。不明来历的东西，碰了说不定倒霉。文件夹在空中悬了片刻，像素化成粒子消散。等你坐回皮卡，座位上多了几样东西——也许是这个地方送你的补偿。",
          ],
          effects: {
            randomLoot: true,
            randomLootCount: 3,
          },
        },
      },
      {
        id: "format_world",
        text: "🌀 试着格式化这片区域",
        description: "如果这里是虚数空间，那也许可以……重置它？",
        hintUnknown: "听起来很疯狂",
        hintKnown: "三项属性全部重置为70%，金币重置为30，并伴随虚数特效",
        result: {
          message: [
            "你对着空气大喊：「格式化！」出乎意料地……有反应。周围的景色闪烁了三次，所有的故障像素整齐地归位，公路恢复正常，仪表盘上的三项数值同时跳动，最终稳定在一个中间值。口袋里掉出来几枚金币，不知是谁的打赏。",
            "你不知道自己在干什么，但还是对着那个文件夹伸出手，做了个删除的手势。整个空间抖动了一下——仪表盘疯狂刷新，燃油、耐久、舒适全都归零又重置，最终定格在70。那个文件夹发出一声像是系统提示音的响声，消失了，地上散落着三十枚金币。",
          ],
          effects: {
            fuelSet: 70,
            durabilitySet: 70,
            comfortSet: 70,
            goldSet: 30,
            imaginaryFormatEffect: true,
          },
        },
      },
    ],
  },
};
