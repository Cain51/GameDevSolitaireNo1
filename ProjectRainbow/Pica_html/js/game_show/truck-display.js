// 皮卡显示和更新模块
// 负责皮卡的视觉渲染和轮子转动动画

let wheelAnimationInterval = null;

// 更新皮卡显示
function updateTruckDisplay() {
  const truck = document.getElementById("truck");

  // 根据当前乘客列表和轮子状态动态生成皮卡模板
  const dynamicTemplate = generateTruckTemplate(
    truckState.passengers,
    truckState.wheelState,
  );

  if (truck && dynamicTemplate) {
    let html = "";
    // 5行13列
    for (let i = 0; i < 5; i++) {
      const row = dynamicTemplate[i];
      html += '<div class="flex justify-left whitespace-nowrap">';
      row.forEach((char) => {
        // 使用样式配置获取字符的颜色和动画
        const charStyle = getCharStyle(char);

        html += `<span class="${charStyle.colorClass} font-bold text-[22px] ${charStyle.animationClass}">${char}</span>`;
      });
      html += "</div>";
    }
    truck.innerHTML = html;
  }
}

// 初始化皮卡显示
function initializeTruck() {
  updateTruckDisplay();
}

// 启动轮子转动动画
function startWheelAnimation() {
  if (wheelAnimationInterval) {
    clearInterval(wheelAnimationInterval);
  }
  wheelAnimationInterval = setInterval(() => {
    // 在两种状态间切换
    truckState.wheelState = truckState.wheelState === 1 ? 2 : 1;
    updateTruckDisplay();
  }, WHEEL_ANIMATION.interval);
}

// 停止轮子转动动画
function stopWheelAnimation() {
  if (wheelAnimationInterval) {
    clearInterval(wheelAnimationInterval);
    wheelAnimationInterval = null;
  }
}
