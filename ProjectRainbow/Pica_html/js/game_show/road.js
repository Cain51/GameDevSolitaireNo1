// 公路动画脚本
// 管理公路字符的流动和动画效果
// 公路状态
let roadCharsPool = GAME_CONFIG.road.charsPool;
let maxRoadLength = GAME_CONFIG.road.maxLength;
let currentRoadChars = [];
let roadSpeed = GAME_CONFIG.road.updateSpeed;
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

// 更新公路 — 右侧添加新字符，左侧移除
function updateRoad() {
  currentRoadChars.push(getRandomRoadChar());
  currentRoadChars.shift();
  renderRoad();
}

// 渲染公路
function renderRoad() {
  const road = document.getElementById("road");
  if (!road) return;
  road.innerHTML = currentRoadChars
    .map(
      (char) =>
        `<span class="inline-block text-3xl text-gray-600 mx-0.5">${char}</span>`,
    )
    .join("");
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
  if (typeof stopEngineSound === "function") stopEngineSound();
  if (typeof pauseScenery === "function") pauseScenery();
}

// 恢复道路
function resumeRoad() {
  if (!roadInterval) {
    roadInterval = setInterval(updateRoad, roadSpeed);
  }
  if (typeof startEngineSound === "function") startEngineSound();
  if (typeof resumeScenery === "function") resumeScenery();
}

// 逐渐减速停止道路
function gradualStopRoad(duration) {
  return new Promise((resolve) => {
    const startSpeed = roadSpeed;
    const steps = 20;
    const stepDuration = duration / steps;
    const speedIncrement = (2000 - startSpeed) / steps;
    let currentStep = 0;

    const decelerate = () => {
      currentStep++;
      roadSpeed += speedIncrement;

      if (currentStep >= steps) {
        pauseRoad();
        roadSpeed = startSpeed;
        resolve();
        return;
      }

      // 用新速度重启interval
      if (roadInterval) clearInterval(roadInterval);
      roadInterval = setInterval(updateRoad, roadSpeed);
      setTimeout(decelerate, stepDuration);
    };

    decelerate();

    // 无论如何都在 duration+500ms 后resolve
    setTimeout(() => {
      pauseRoad();
      roadSpeed = startSpeed;
      resolve();
    }, duration + 500);
  });
}

// 启动
initRoad();
startRoadAnimation();
