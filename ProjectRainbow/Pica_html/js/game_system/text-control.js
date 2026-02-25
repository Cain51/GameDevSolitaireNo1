// 文本生成控制模块
// 管理文本生成的暂停和恢复

function pauseTextGeneration() {
	textGenerationPaused = true;
	stopWheelAnimation();  // 暂停轮子动画
}

function resumeTextGeneration() {
	textGenerationPaused = false;
	startWheelAnimation();  // 恢复轮子动画
}

function isTextGenerationPaused() {
	return textGenerationPaused;
}
