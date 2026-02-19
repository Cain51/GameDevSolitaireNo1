// 公路字符配置
const roadCharsPool = [ '马', '鹿'];
const maxRoadLength = 80; // 屏幕显示的字符数量
let currentRoadChars = [];
let roadSpeed = 300; // 毫秒，越小越快
let roadInterval = null;

// 获取随机字符
function getRandomRoadChar() {
	return roadCharsPool[Math.floor(Math.random() * roadCharsPool.length)];
}

// 初始化公路
function initRoad() {
	currentRoadChars = [];
	for (let i = 0; i < maxRoadLength; i++) {
		currentRoadChars.push(getRandomRoadChar());
	}
	renderRoad();
}

// 更新公路 - 右侧添加新字符，左侧移除
function updateRoad() {
	currentRoadChars.push(getRandomRoadChar()); // 右侧添加新字符
	currentRoadChars.shift(); // 左侧移除旧字符
	renderRoad();
}

// 渲染公路
function renderRoad() {
	const road = document.getElementById('road');
	const html = currentRoadChars.map(char =>
		`<span class="inline-block text-3xl text-gray-600 mx-0.5">${char}</span>`
	).join('');
	road.innerHTML = html;
}

// 开始公路移动
function startRoadAnimation() {
	if (roadInterval) clearInterval(roadInterval);
	roadInterval = setInterval(updateRoad, roadSpeed);
}

// 暂停道路
function pauseRoad() {
	if (roadInterval) {
		clearInterval(roadInterval);
		roadInterval = null;
	}
}

// 恢复道路
function resumeRoad() {
	if (!roadInterval) {
		roadInterval = setInterval(updateRoad, roadSpeed);
	}
}

// 逐渐减速停止道路（返回Promise）
function gradualStopRoad(duration) {
	return new Promise(resolve => {
		const startSpeed = roadSpeed;
		const steps = 60; // 60帧
		const stepDuration = duration / steps;
		const speedIncrement = (3000 - startSpeed) / steps; // 逐步增加到3000ms（很慢）
		
		let currentStep = 0;
		
		const decelerate = () => {
			currentStep++;
			roadSpeed += speedIncrement;
			
			if (currentStep >= steps) {
				// 完全停止
				pauseRoad();
				roadSpeed = startSpeed; // 恢复原速度，为下次启动做准备
				resolve();
			} else {
				// 重启interval以应用新速度
				if (roadInterval) clearInterval(roadInterval);
				roadInterval = setInterval(updateRoad, roadSpeed);
				setTimeout(decelerate, stepDuration);
			}
		};
		
		decelerate();
	});
}

initRoad();
startRoadAnimation();
