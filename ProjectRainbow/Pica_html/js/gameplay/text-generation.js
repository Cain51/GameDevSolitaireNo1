// 文本生成模块
// 负责游戏中文本的动态生成和更新

const TEXT_MESSAGES = [
  "皮卡在公路上平稳行驶...",
  "风吹过车窗...",
  "远方的路还很长...",
  "汉字组成的世界如此奇妙...",
  "红色的皮卡继续前行...",
  "道路两旁风景如画...",
  "引擎轰鸣，文字流动...",
];

let textGenerationTimer = null;

// 启动文本生成
function startTextGeneration() {
  const textArea = document.getElementById("textArea");

  // 加载存档
  if (loadGame()) {
    textArea.innerHTML += "<p>继续上次的旅程...</p>";
  }

  // 无论是否有存档，都渲染皮卡并刷新状态与里程记录
  updateTruckDisplay();
  if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  if (typeof updateGoldDisplay === "function") updateGoldDisplay();
  if (typeof updatePassengerListDisplay === "function")
    updatePassengerListDisplay();
  if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();

  // 文本生成定时器
  textGenerationTimer = setInterval(() => {
    // 如果正在事件中或被暂停，停止文本生成
    if (gameState.eventTriggered || isTextGenerationPaused()) return;

    const msg = TEXT_MESSAGES[Math.floor(Math.random() * TEXT_MESSAGES.length)];
    textArea.innerHTML += `<p>${msg}</p>`;
    scrollTextAreaToBottom(textArea);

    // 增加文本计数
    gameState.textCount++;

    // 每行推进时实时增加里程、扣燃油、更新里程与“距目的地”显示、检查乘客下车
    if (typeof advanceMileageForTick === "function") advanceMileageForTick();

    // 限制文本数量
    const paragraphs = textArea.querySelectorAll("p");
    if (paragraphs.length > 20) {
      paragraphs[0].remove();
    }

    // 每 2 行检查是否触发事件节点
    checkEventTrigger();

    // 保存游戏
    saveGame();
  }, GAME_CONFIG.textSpeed);
}

// 停止文本生成
function stopTextGeneration() {
  if (textGenerationTimer) {
    clearInterval(textGenerationTimer);
    textGenerationTimer = null;
  }
}
