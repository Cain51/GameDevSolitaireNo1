// 游戏状态管理
let gameState = {
	textCount: 0,              // 文本计数
	eventTriggered: false,     // 是否正在事件中
	passengers: [],            // 乘客列表
	triggeredEvents: [],       // 已触发的事件ID列表
	unlockedEvents: [],        // 已解锁的事件ID列表
	currentTruckTemplate: 'default'  // 当前皮卡模板
};

// Cookie管理
function setCookie(name, value, days) {
	const expires = new Date();
	expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
	document.cookie = name + '=' + encodeURIComponent(JSON.stringify(value)) + ';expires=' + expires.toUTCString() + ';path=/';
}

function getCookie(name) {
	const nameEQ = name + '=';
	const ca = document.cookie.split(';');
	for (let i = 0; i < ca.length; i++) {
		let c = ca[i];
		while (c.charAt(0) === ' ') c = c.substring(1, c.length);
		if (c.indexOf(nameEQ) === 0) {
			return JSON.parse(decodeURIComponent(c.substring(nameEQ.length, c.length)));
		}
	}
	return null;
}

function saveGame() {
	setCookie('chinese_truck_game', gameState, 3650);  // 保存10年
}

function loadGame() {
	const saved = getCookie('chinese_truck_game');
	if (saved) {
		gameState = saved;
		return true;
	}
	return false;
}

// 添加动态文本
function addRandomText() {
	const textArea = document.getElementById('textArea');
	const messages = [
		'皮卡在公路上平稳行驶...',
		'风吹过车窗...',
		'远方的路还很长...',
		'汉字组成的世界如此奇妙...',
		'红色的皮卡继续前行...',
		'道路两旁风景如画...',
		'引擎轰鸣，文字流动...'
	];

	// 加载存档
	if (loadGame()) {
		textArea.innerHTML += '<p>继续上次的旅程...</p>';
	}
	
	// 无论是否有存档，都渲染皮卡
	updateTruckDisplay();

	// 文本生成定时器
	setInterval(() => {
		// 如果正在事件中，暂停文本生成
		if (gameState.eventTriggered) return;

		const msg = messages[Math.floor(Math.random() * messages.length)];
		textArea.innerHTML += `<p>${msg}</p>`;
		textArea.scrollTop = textArea.scrollHeight;

		// 增加文本计数
		gameState.textCount++;

		// 限制文本数量
		const paragraphs = textArea.querySelectorAll('p');
		if (paragraphs.length > 20) {
			paragraphs[0].remove();
		}

		// 检查是否触发事件
		checkEventTrigger();

		// 保存游戏
		saveGame();
	}, 5000);
}

// 检查事件触发
function checkEventTrigger() {
	// 每2行文本触发一次
	if (gameState.textCount % 2 === 0 && gameState.textCount > 0) {
		// 查找可触发的事件
		const availableEvent = findAvailableEvent();
		if (availableEvent) {
			triggerEvent(availableEvent);
		}
	}
}

// 查找可用事件
function findAvailableEvent() {
	for (const eventId in GAME_EVENTS) {
		const event = GAME_EVENTS[eventId];

		// 检查是否已触发（一次性事件）
		if (event.oneTime && gameState.triggeredEvents.includes(eventId)) {
			continue;
		}

		// 检查条件
		if (event.condition) {
			if (event.condition.requiresPassenger) {
				if (!gameState.passengers.includes(event.condition.requiresPassenger)) {
					continue;
				}
			}
		}

		// 检查是否已解锁
		if (!gameState.unlockedEvents.includes(eventId) && eventId !== 'deer' && eventId !== 'rain' && eventId !== 'saofurry') {
			continue;
		}

		return event;
	}
	return null;
}

// 触发事件（带前奏动画）
async function triggerEvent(event) {
	gameState.eventTriggered = true;
	
	// 暂停文本生成
	pauseTextGeneration();
	
	// 阶段1：道路减速停止（2.5秒）
	await gradualStopRoad(GAME_CONFIG.animation.roadDeceleration);
	
	// 阶段2：显示触发字滑入动画
	await showTriggerChar(event.id);
	
	// 阶段3：停留1秒后显示弹窗
	setTimeout(() => {
		// 隐藏触发字
		hideTriggerChar();
		// 显示事件弹窗
		displayEventModal(event);
	}, GAME_CONFIG.animation.charStay);
	
	// 记录已触发
	if (!gameState.triggeredEvents.includes(event.id)) {
		gameState.triggeredEvents.push(event.id);
	}
}

// 显示触发字动画
function showTriggerChar(eventId) {
	return new Promise(resolve => {
		const config = EVENT_TRIGGER_CHARS[eventId];
		if (!config) {
			// 如果该事件没有配置触发字，直接跳过
			resolve();
			return;
		}
		
		const container = document.getElementById('event-trigger-container');
		const charSpan = document.getElementById('event-trigger-char');
		
		// 设置字符和样式
		charSpan.textContent = config.char;
		charSpan.style.color = config.color;
		charSpan.style.fontSize = config.fontSize;
		
		// 初始位置：右侧屏幕外
		container.style.transform = 'translate(100vw, -50%)';
		container.style.opacity = '1';
		container.style.transition = `transform ${GAME_CONFIG.animation.charSlideIn}ms linear`;
		
		// 强制重绘
		container.offsetHeight;
		
		// 开始动画：移动到中央
		setTimeout(() => {
			container.style.transform = 'translate(-50%, -50%)';
		}, 50);
		
		// 动画完成后resolve
		setTimeout(() => {
			resolve();
		}, GAME_CONFIG.animation.charSlideIn + 50);
	});
}

// 隐藏触发字
function hideTriggerChar() {
	const container = document.getElementById('event-trigger-container');
	container.style.opacity = '0';
	container.style.transition = 'opacity 0.3s ease-out';
}

// 显示事件弹窗
function displayEventModal(event) {
	const modal = document.createElement('div');
	modal.id = 'event-modal';
	modal.className = 'fixed inset-0 bg-black/80 flex items-center justify-center z-50';

	let choicesHtml = '';
	event.choices.forEach(choice => {
		choicesHtml += `
			<button onclick="handleEventChoice('${event.id}', '${choice.id}')" 
				class="w-full px-6 py-4 mb-3 bg-gradient-to-r from-gray-700 to-gray-800 hover:from-red-700 hover:to-red-800 text-white rounded-lg text-left transition-all duration-300 border border-gray-600 hover:border-red-400">
				<div class="text-lg font-bold mb-1">${choice.text}</div>
				<div class="text-sm text-gray-400">${choice.description}</div>
			</button>
		`;
	});

	modal.innerHTML = `
		<div class="bg-[#1a1a2e] border-2 border-[#c41e3a] rounded-2xl p-8 max-w-lg w-full mx-4 shadow-[0_0_50px_rgba(196,30,58,0.5)] animate-fade-in">
			<div class="text-center mb-6">
				<div class="text-6xl mb-4">${event.image}</div>
				<h2 class="text-2xl font-bold text-[#c41e3a] mb-2">${event.title}</h2>
				<p class="text-gray-300 leading-relaxed">${event.description}</p>
			</div>
			<div class="space-y-2">
				${choicesHtml}
			</div>
		</div>
	`;

	document.body.appendChild(modal);
}

// 处理事件选择
function handleEventChoice(eventId, choiceId) {
	const event = GAME_EVENTS[eventId];
	const choice = event.choices.find(c => c.id === choiceId);

	// 应用结果
	if (choice.result) {
		// 显示结果消息
		const textArea = document.getElementById('textArea');
		textArea.innerHTML += `<p class="text-[#c41e3a]">【事件】${choice.result.message}</p>`;
		textArea.scrollTop = textArea.scrollHeight;

		// 应用效果
		if (choice.result.effects) {
			// 添加乘客
			if (choice.result.effects.addPassenger) {
				if (!gameState.passengers.includes(choice.result.effects.addPassenger)) {
					gameState.passengers.push(choice.result.effects.addPassenger);
				}
			}
			
			// 移除乘客
			if (choice.result.effects.removePassenger) {
				const index = gameState.passengers.indexOf(choice.result.effects.removePassenger);
				if (index > -1) {
					gameState.passengers.splice(index, 1);
				}
			}

			// 修改皮卡
			if (choice.result.effects.modifyTruck) {
				gameState.currentTruckTemplate = choice.result.effects.modifyTruck;
				updateTruckDisplay();
			}

			// 解锁事件
			if (choice.result.effects.unlockEvents) {
				choice.result.effects.unlockEvents.forEach(evtId => {
					if (!gameState.unlockedEvents.includes(evtId)) {
						gameState.unlockedEvents.push(evtId);
					}
				});
			}
			
			// 处理游戏结束
			if (choice.result.effects.gameOver) {
				showGameOver();
				return; // 不恢复游戏，不保存
			}
		}
	}

	// 关闭弹窗
	const modal = document.getElementById('event-modal');
	if (modal) {
		modal.remove();
	}

	// 恢复游戏
	gameState.eventTriggered = false;
	
	// 重新开始道路动画（使用当前保存的速度）
	resumeRoad();
	
	// 恢复文本生成
	resumeTextGeneration();

	// 保存游戏
	saveGame();
}

// 显示游戏结束画面
function showGameOver() {
	// 关闭事件弹窗
	const modal = document.getElementById('event-modal');
	if (modal) {
		modal.remove();
	}
	
	// 创建游戏结束弹窗
	const gameOverModal = document.createElement('div');
	gameOverModal.id = 'game-over-modal';
	gameOverModal.className = 'fixed inset-0 bg-black/90 flex items-center justify-center z-50';
	
	gameOverModal.innerHTML = `
		<div class="bg-[#1a1a2e] border-2 border-[#ff0000] rounded-2xl p-8 max-w-md w-full mx-4 text-center animate-fade-in">
			<div class="text-6xl mb-4">💥</div>
			<h2 class="text-3xl font-bold text-[#ff0000] mb-4">游戏结束</h2>
			<p class="text-gray-300 mb-6">你的皮卡被打爆了！</p>
			<button onclick="location.reload()" 
				class="px-8 py-3 bg-[#c41e3a] text-white rounded-full hover:bg-[#e63950] transition-all">
				重新开始
			</button>
		</div>
	`;
	
	document.body.appendChild(gameOverModal);
}

// 暂停文本生成（需要在addRandomText中实现）
let textGenerationPaused = false;
function pauseTextGeneration() {
	textGenerationPaused = true;
}

function resumeTextGeneration() {
	textGenerationPaused = false;
}

// 更新皮卡显示
function updateTruckDisplay() {
	const template = TRUCK_TEMPLATES[gameState.currentTruckTemplate] || TRUCK_TEMPLATES.default;
	const truck = document.getElementById('truck');

	if (truck && template) {
		let html = '';
		// 改为5行
		for (let i = 1; i <= 5; i++) {
			const row = template[`row${i}`];
			html += '<div class="flex justify-left whitespace-nowrap">';
			row.forEach(char => {
				let colorClass = 'text-[#0d0d0d]';
				let animationClass = '';

				if (char === '皮' || char === '卡' || char === '后' || char === '车' || char === '厢' || char === '门' || char === '|' || char === '丨') {
					colorClass = 'text-[#c41e3a]';
					animationClass = 'animate-truck-glow';
				} else if (char === '轮') {
					colorClass = 'text-[#63635E]';
				} else if (char === '鹿') {
					colorClass = 'text-[#d4a574]';
					animationClass = 'animate-truck-glow';
				} else if (char === '猎' || char === '人') {
					colorClass = 'text-[#8b7355]';
					animationClass = 'animate-truck-glow';
				} else if (char === '福') {
					colorClass = 'text-[#ff69b4]';
					animationClass = 'animate-truck-glow';
				}

				html += `<span class="${colorClass} font-bold text-[22px] ${animationClass}">${char}</span>`;
			});
			html += '</div>';
		}
		truck.innerHTML = html;
	}
}

// 修改addRandomText函数以支持暂停
const originalAddRandomText = addRandomText;
addRandomText = function() {
	const textArea = document.getElementById('textArea');
	const messages = [
		'皮卡在公路上平稳行驶...',
		'风吹过车窗...',
		'远方的路还很长...',
		'红色的皮卡继续前行...',
		'道路两旁风景如画...',
		'引擎轰鸣，文字流动...'
	];

	// 加载存档
	if (loadGame()) {
		updateTruckDisplay();
		textArea.innerHTML += '<p>继续上次的旅程...</p>';
	}

	// 文本生成定时器
	setInterval(() => {
		// 如果正在事件中，暂停文本生成
		if (gameState.eventTriggered || textGenerationPaused) return;

		const msg = messages[Math.floor(Math.random() * messages.length)];
		textArea.innerHTML += `<p>${msg}</p>`;
		textArea.scrollTop = textArea.scrollHeight;

		// 增加文本计数
		gameState.textCount++;

		// 限制文本数量
		const paragraphs = textArea.querySelectorAll('p');
		if (paragraphs.length > 20) {
			paragraphs[0].remove();
		}

		// 检查是否触发事件
		checkEventTrigger();

		// 保存游戏
		saveGame();
	}, 5000);
};

// 添加淡入动画样式
const style = document.createElement('style');
style.textContent = `
	@keyframes fadeIn {
		from { opacity: 0; transform: scale(0.9); }
		to { opacity: 1; transform: scale(1); }
	}
	.animate-fade-in {
		animation: fadeIn 0.3s ease-out;
	}
`;
document.head.appendChild(style);

// 初始化
addRandomText();
