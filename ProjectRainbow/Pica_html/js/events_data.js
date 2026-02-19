// 游戏事件数据
const GAME_EVENTS = {
	deer: {
		id: "deer",
		title: "前方发现一只鹿！",
		description: "一只迷路的鹿站在公路中央，好奇地望着你的皮卡...它的眼睛里闪烁着智慧的光芒。它用眼神和你交流，似乎在乞求你的安保。",
		image: "鹿！",
		oneTime: true,
		condition: null,
		choices: [
			{
				id: "invite",
				text: "接受这个委托！",
				description: "打开车门，邀请鹿成为旅途伙伴",
				result: {
					message: "鹿高兴地跳上了后车厢！它成为了你的第一位用户。",
					effects: {
						addPassenger: "鹿",
						modifyTruck: "deer",
						unlockEvents: ["hunter"]
					}
				}
			},
			{
				id: "honk",
				text: "鸣笛驱赶",
				description: "按响喇叭，让鹿让开道路",
				result: {
					message: "鹿受惊跑进了森林，你继续独自前行。",
					effects: {}
				}
			},
			{
				id: "detour",
				text: "小心绕行",
				description: "慢慢绕过鹿，不打扰它",
				result: {
					message: "你小心地开过，鹿目送你的皮卡远去。",
					effects: {}
				}
			}
		]
	},
	hunter: {
		id: "hunter",
		title: "遇到猎人",
		description: "一位背着猎枪的猎人拦住了你的去路...他看到你的皮卡，眼中闪过一丝惊讶。",
		image: "猎人！",
		oneTime: true,
		condition: {
			requiresPassenger: "鹿"
		},
		choices: [
			{
				id: "hide",
				text: "加速逃离",
				description: "猛踩油门，甩掉猎人",
				result: {
					message: "你加速逃离，猎人在后面追赶了一段，最终被甩开了。",
					effects: {}
				}
			},
			{
				id: "talk",
				text: "与猎人对话",
				description: "停下车，询问猎人的来意",
				result: {
					message: "猎人告诉你他正在寻找一只迷路的鹿。你假装没看见，猎人就离开了。",
					effects: {}
				}
			},
{
				id: "surrender",
				text: "把鹿交给猎人",
				description: "你决定放弃鹿的安保",
				result: {
					message: "你把鹿交给了猎人，鹿看向你时眼里闪着悲伤的光",
					effects: {
						removePassenger: "鹿",
						modifyTruck: "default"
					}
				}
			},
			{
				id: "share",
				text: "分享食物",
				description: "给猎人一些食物，请求他放过鹿",
				result: {
					message: "猎人接受了你的食物，承诺不再追捕这只鹿。你们成为了朋友。不知道为什么，他一定要扒在车底",
					effects: {
						addPassenger: "猎人",
						modifyTruck: "hunter"
					}
				}
			}
		]
	},
	rain: {
		id: "rain",
		title: "突降大雨",
		description: "天空突然乌云密布，倾盆大雨瞬间降临，能见度急剧下降...",
		image: "雨！️",
		oneTime: true,
		condition: null,
		choices: [
			{
				id: "continue",
				text: "冒雨前行",
				description: "保持速度，小心驾驶",
				result: {
					message: "雨水模糊了视线，但你依然坚定地向前行驶。",
					effects: {}
				}
			},
			{
				id: "wait",
				text: "停车等待",
				description: "找个地方停车，等雨停了再走",
				result: {
					message: "你停在一棵大树下避雨，等待了片刻后，雨势渐小，继续上路。",
					effects: {}
				}
			}
		]
	},
	saofurry: {
		id: "saofurry",
		title: "遇见骚福瑞！",
		description: "路边突然出现一只神秘的生物...它有着奇异的外表，眼神中透着一丝狡黠。这就是传说中的骚福瑞！",
		image: "骚福瑞！",
		oneTime: true,
		condition: null,
		choices: [
			{
				id: "invite",
				text: "邀请上车",
				description: "它很开心的坐到了副驾",
				result: {
					message: "骚福瑞兴奋地跳上了车，坐到了副驾驶的位置。它看起来很满意这个座位。",
					effects: {
						addPassenger: "骚福瑞",
						modifyTruck: "saofurry"
					}
				}
			},
			{
				id: "ignore",
				text: "无视/驱赶",
				description: "你冷漠地离开了福瑞",
				result: {
					message: "你冷漠地踩下油门，骚福瑞看着你的皮卡远去，眼神中流露出一丝遗憾。",
					effects: {}
				}
			},
			{
				id: "play",
				text: "和骚福瑞哼哼哈嘿",
				description: "试图和骚福瑞做一些奇怪的事情...",
				result: {
					message: "骚福瑞突然暴怒！它展现出了惊人的战斗力...你的皮卡被打爆了！游戏结束。",
					effects: {
						gameOver: true
					}
				}
			}
		]
	}
};

// 皮卡显示模板
const TRUCK_TEMPLATES = {
	default: {
		row1: ["黑", "黑", "黑", "黑", "黑", "黑", "皮", "卡", "皮", "卡", "皮", "黑", "黑"],
		row2: ["后", "车", "厢", "后", "车", "厢", "黑", "黑", "丨", "黑", "黑", "皮", "黑"],
		row3: ["皮", "卡", "皮", "轮", "卡", "车", "门", "车", "门", "车", "轮", "门", "卡"],
		row4: ["皮", "卡", "轮", "轮", "轮", "车", "门", "车", "门", "轮", "轮", "轮", "卡"],
		row5: ["黑", "黑", "黑", "轮", "黑", "黑", "黑", "黑", "黑", "黑", "轮", "黑", "黑"]
	},
	deer: {
		row1: ["黑", "黑", "鹿", "黑", "黑", "黑", "皮", "卡", "皮", "卡", "皮", "黑", "黑"],
		row2: ["后", "车", "厢", "后", "车", "厢", "黑", "黑", "丨", "黑", "黑", "皮", "黑"],
		row3: ["皮", "卡", "皮", "轮", "卡", "车", "门", "车", "门", "车", "轮", "门", "卡"],
		row4: ["皮", "卡", "轮", "轮", "轮", "车", "门", "车", "门", "轮", "轮", "轮", "卡"],
		row5: ["黑", "黑", "黑", "轮", "黑", "黑", "黑", "黑", "黑", "黑", "轮", "黑", "黑"]
	},
	hunter: {
		row1: ["黑", "黑", "鹿", "黑", "黑", "黑", "皮", "卡", "皮", "卡", "皮", "黑", "黑"],
		row2: ["后", "车", "厢", "后", "车", "厢", "黑", "黑", "丨", "黑", "黑", "皮", "黑"],
		row3: ["皮", "卡", "皮", "轮", "卡", "车", "门", "车", "门", "车", "轮", "门", "卡"],
		row4: ["皮", "卡", "轮", "轮", "轮", "车", "门", "车", "门", "轮", "轮", "轮", "卡"],
		row5: ["黑", "黑", "黑", "轮", "黑", "黑", "猎", "人", "黑", "黑", "轮", "黑", "黑"]
	},
	saofurry: {
		row1: ["黑", "黑", "鹿", "黑", "黑", "黑", "皮", "卡", "皮", "卡", "皮", "黑", "黑"],
		row2: ["后", "车", "厢", "后", "车", "厢", "黑", "黑", "丨", "黑", "福", "皮", "黑"],
		row3: ["皮", "卡", "皮", "轮", "卡", "车", "门", "车", "门", "车", "轮", "门", "卡"],
		row4: ["皮", "卡", "轮", "轮", "轮", "车", "门", "车", "门", "轮", "轮", "轮", "卡"],
		row5: ["黑", "黑", "黑", "轮", "黑", "黑", "猎", "人", "黑", "黑", "轮", "黑", "黑"]
	}
};

// 事件触发字配置
const EVENT_TRIGGER_CHARS = {
	deer: {
		char: '鹿',
		color: '#d4a574',  // 金色/棕色
		fontSize: '22px'   // 和皮卡一样大
	},
	hunter: {
		char: '猎',
		color: '#8b7355',  // 猎人棕
		fontSize: '22px'
	},
	saofurry: {
		char: '福',
		color: '#ff69b4',  // 骚粉色
		fontSize: '22px'
	}
	// rain 等场景事件不需要触发字
};

// 游戏配置
const GAME_CONFIG = {
	triggerInterval: 2,  // 每2行文本触发一次
	cookieExpiryDays: 3650,  // 10年
	cookieName: "chinese_truck_adventure_save",
	// 动画时长配置（毫秒）
	animation: {
		roadDeceleration: 2500,  // 道路减速 2.5秒
		charSlideIn: 1500,       // 字滑入 1.5秒
		charStay: 1000           // 停留 1秒
	}
};

// 导出（如果是模块化环境）
if (typeof module !== 'undefined' && module.exports) {
	module.exports = { GAME_EVENTS, TRUCK_TEMPLATES, GAME_CONFIG, EVENT_TRIGGER_CHARS };
}
