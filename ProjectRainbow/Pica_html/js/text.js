// 主控制脚本 - 游戏初始化和启动
// 整合所有模块，负责游戏的初始化和启动

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

// 游戏初始化函数
function initializeGame() {
	// 加载跨档成就
	if (typeof loadAchievementsFromStorage === "function") {
		loadAchievementsFromStorage();
	}
	
	// 加载存档
	const hasExistingSave = loadGame();
	
	// 新游戏：给予初始物品，并重置本轮成就
	if (!hasExistingSave) {
		addItem('油桶', 1);
		addItem('马年红包', 1);
		gameState.sessionAchievements = [];
		gameState.passengersEverOnBoard = [];
		gameState.itemsCrafted = 0;
		gameState.itemsUsed = 0;
		gameState.totalEventsHandled = 0;
		gameState.hasTradedWithMerchant = false;
		gameState.hasOpenedRedPacket = false;
		gameState.activeDebuffs = [];
		gameState.survivedLowStats = false;
		gameState.perfectRun = true; // 初始为 true，如果属性低于 50% 则设为 false
		gameState.lowStatsMileage = 0;
		gameState.minFuelDuringRun = 100;
		gameState.minDurabilityDuringRun = 100;
		gameState.minComfortDuringRun = 100;

		// 读取困难模式标签
		try {
			const hardTags = sessionStorage.getItem("hard_mode_tags");
			if (hardTags) {
				gameState.hardModeTags = JSON.parse(hardTags);
				sessionStorage.removeItem("hard_mode_tags");
			} else {
				gameState.hardModeTags = [];
			}
		} catch (e) {
			gameState.hardModeTags = [];
		}

		// 读取困难模式加成
		try {
			const hardBonuses = sessionStorage.getItem("hard_mode_bonuses");
			if (hardBonuses) {
				gameState.hardModeBonuses = JSON.parse(hardBonuses);
				sessionStorage.removeItem("hard_mode_bonuses");
			} else {
				gameState.hardModeBonuses = [];
			}
		} catch (e) {
			gameState.hardModeBonuses = [];
		}

		// 读取简单模式标志（关闭衰变 debuff）
		try {
			const easyFlag = sessionStorage.getItem("easy_mode");
			gameState.easyMode = easyFlag === "true";
			sessionStorage.removeItem("easy_mode");
		} catch (e) {
			gameState.easyMode = false;
		}

		// 简单模式：自动开启 debug 面板（方便新手）
		if (gameState.easyMode) {
			// 延迟执行，确保 audio.js 中的 toggleDebugMode 已加载
			setTimeout(() => {
				if (typeof toggleDebugMode === "function") {
					toggleDebugMode(true);
					const toggle = document.getElementById("debug-toggle");
					if (toggle) toggle.checked = true;
				}
			}, 0);
		}

		// 应用困难模式 onStart 修饰符
		if (gameState.hardModeTags.length > 0 && typeof HARD_MODE_MODIFIERS !== "undefined") {
			for (const tagId of gameState.hardModeTags) {
				const mod = HARD_MODE_MODIFIERS.find(m => m.id === tagId);
				if (!mod || !mod.onStart) continue;
				if (mod.onStart.fuel) truckState.fuel = Math.max(0, truckState.fuel + mod.onStart.fuel);
				if (mod.onStart.durability) truckState.durability = Math.max(0, truckState.durability + mod.onStart.durability);
				if (mod.onStart.comfort) truckState.comfort = Math.max(0, truckState.comfort + mod.onStart.comfort);
				if (mod.onStart.maxWeight) inventoryState.maxWeight = Math.max(5, inventoryState.maxWeight + mod.onStart.maxWeight);
				if (mod.onStart.gold) inventoryState.gold = Math.max(0, inventoryState.gold + mod.onStart.gold);
			}
		}

		// 应用困难模式加成 onStart
		if (gameState.hardModeBonuses.length > 0 && typeof HARD_MODE_BONUSES !== "undefined") {
			// 珍品收藏家：开局随机获得一件珍品
			if (gameState.hardModeBonuses.includes("treasure_collector")) {
				const treasurePool = [];
				if (typeof ITEMS_CONFIG !== "undefined") {
					for (const [key, item] of Object.entries(ITEMS_CONFIG)) {
						// 排除变形产物（破损的雕塑、空白书）
						if (item.category === "treasure" && key !== "破损的雕塑" && key !== "空白书") {
							treasurePool.push(key);
						}
					}
				}
				if (treasurePool.length > 0) {
					const randomTreasure = treasurePool[Math.floor(Math.random() * treasurePool.length)];
					addItem(randomTreasure, 1);
				}
			}

			// 更好的开局：开局获得 2~3 种随机基础物资，每种 1~2 个
			if (gameState.hardModeBonuses.includes("better_start")) {
				const starterMats = ["废金属", "布料", "草药", "空罐", "原油"];
				// 随机打乱后取前 2~3 种
				for (let i = starterMats.length - 1; i > 0; i--) {
					const j = Math.floor(Math.random() * (i + 1));
					[starterMats[i], starterMats[j]] = [starterMats[j], starterMats[i]];
				}
				const kinds = Math.floor(Math.random() * 2) + 2; // 2~3 种
				for (let i = 0; i < kinds; i++) {
					const qty = Math.floor(Math.random() * 2) + 1; // 1~2 个
					addItem(starterMats[i], qty);
				}
			}
		}
	}
	
	// 显示困难模式标签栏
	updateHardModeBar();

	// 检查时间银行余额：若有存款，发放时间存折珍品
	if (typeof getTimeBankBalance === "function") {
		const timeBankBalance = getTimeBankBalance();
		if (timeBankBalance > 0 && !hasItem("时间存折")) {
			addItem("时间存折", 1);
		}
	}

	// 更新衰变 debuff 显示栏
	if (typeof updateDebuffBar === "function") updateDebuffBar();

	// 初始化皮卡显示
	initializeTruck();
	
	// 初始化背景景观
	if (typeof initScenery === "function") initScenery();
	
	// 初始化库存显示
	initInventoryDisplay();
	
	// 启动文本生成
	startTextGeneration();
	
	// 启动轮子动画
	startWheelAnimation();
}

// 更新困难模式标签栏显示
function updateHardModeBar() {
	const bar = document.getElementById("hard-mode-bar");
	if (!bar) return;

	const hasTags = gameState.hardModeTags && gameState.hardModeTags.length > 0;
	const isEasy = gameState.easyMode === true;

	if (!hasTags && !isEasy) {
		bar.classList.add("hidden");
		return;
	}
	if (typeof HARD_MODE_MODIFIERS === "undefined") return;
	bar.classList.remove("hidden");

	const tagsContainer = document.getElementById("hard-mode-tags");
	const pointsEl = document.getElementById("hard-mode-points");
	let totalPoints = 0;
	let html = "";

	// 简单模式标签
	if (isEasy) {
		html += `<span class="inline-block bg-emerald-900/60 text-emerald-300 text-xs px-1.5 py-0.5 rounded border border-emerald-800/50 cursor-default">🛡️ 简单模式</span>`;
	}

	for (const tagId of (gameState.hardModeTags || [])) {
		const mod = HARD_MODE_MODIFIERS.find(m => m.id === tagId);
		if (!mod) continue;
		totalPoints += mod.points || 0;
		// 构建详细效果文本
		let detailLines = [];
		if (mod.onStart) {
			if (mod.onStart.fuel) detailLines.push(`初始燃油 ${mod.onStart.fuel > 0 ? '+' : ''}${mod.onStart.fuel}%`);
			if (mod.onStart.durability) detailLines.push(`初始耐久 ${mod.onStart.durability > 0 ? '+' : ''}${mod.onStart.durability}%`);
			if (mod.onStart.comfort) detailLines.push(`初始舒适 ${mod.onStart.comfort > 0 ? '+' : ''}${mod.onStart.comfort}%`);
			if (mod.onStart.maxWeight) detailLines.push(`最大载重 ${mod.onStart.maxWeight > 0 ? '+' : ''}${mod.onStart.maxWeight}kg`);
			if (mod.onStart.gold) detailLines.push(`初始金币 ${mod.onStart.gold > 0 ? '+' : ''}${Math.max(-999, mod.onStart.gold)}`);
		}
		if (mod.perChoice) {
			const parts = [];
			if (mod.perChoice.fuel) parts.push(`燃油${mod.perChoice.fuel}`);
			if (mod.perChoice.durability) parts.push(`耐久${mod.perChoice.durability}`);
			if (mod.perChoice.comfort) parts.push(`舒适${mod.perChoice.comfort}`);
			if (parts.length) detailLines.push(`每次抉择: ${parts.join(', ')}%`);
		}
		if (mod.perChoiceRandom) {
			detailLines.push(`每次抉择 ${(mod.perChoiceRandom.chance * 100).toFixed(0)}% 概率随机属性 ${mod.perChoiceRandom.amount}%`);
		}
		const detailText = detailLines.join(' | ');
		const escapedDetail = detailText.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
		html += `<span class="hard-mode-tag-chip inline-block bg-red-900/60 text-red-300 text-xs px-1.5 py-0.5 rounded border border-red-800/50 cursor-default relative" data-tooltip="${escapedDetail}" data-points="${mod.points}">${mod.name}</span>`;
	}
	tagsContainer.innerHTML = html;
	pointsEl.textContent = `+${totalPoints}pt`;

	// 显示加成标签
	if (Array.isArray(gameState.hardModeBonuses) && gameState.hardModeBonuses.length > 0 && typeof HARD_MODE_BONUSES !== "undefined") {
		let bonusHtml = "";
		for (const bonusId of gameState.hardModeBonuses) {
			const bonus = HARD_MODE_BONUSES.find(b => b.id === bonusId);
			if (!bonus) continue;
			const escapedDesc = bonus.description.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
			bonusHtml += `<span class="hard-mode-tag-chip inline-block bg-emerald-900/60 text-emerald-300 text-xs px-1.5 py-0.5 rounded border border-emerald-800/50 cursor-default relative" data-tooltip="${escapedDesc}" data-points="${bonus.cost}">🎁${bonus.name}</span>`;
		}
		tagsContainer.innerHTML += bonusHtml;
	}

	// 绑定悬停 tooltip
	tagsContainer.querySelectorAll('.hard-mode-tag-chip').forEach(chip => {
		chip.addEventListener('mouseenter', showHardModeTooltip);
		chip.addEventListener('mouseleave', hideHardModeTooltip);
	});
}

// 更新衰变 debuff 显示栏
function updateDebuffBar() {
	const bar = document.getElementById("debuff-bar");
	if (!bar) return;
	if (gameState.easyMode || !Array.isArray(gameState.activeDebuffs) || gameState.activeDebuffs.length === 0) {
		bar.classList.add("hidden");
		return;
	}
	if (typeof DECAY_DEBUFFS === "undefined") return;
	bar.classList.remove("hidden");

	const tagsContainer = document.getElementById("debuff-tags");
	let html = "";
	for (const debuffEntry of gameState.activeDebuffs) {
		const cfg = DECAY_DEBUFFS.find(d => d.id === debuffEntry.id);
		if (!cfg) continue;
		const stacks = debuffEntry.stacks || 1;
		const stackText = stacks > 1 ? ` ×${stacks}` : "";

		// 构建动态效果描述：显示当前层数下的实际数值
		let effectDesc = cfg.description;
		if (stacks > 1) {
			if (cfg.perChoice) {
				const parts = [];
				if (cfg.perChoice.fuel) parts.push(`燃油 ${cfg.perChoice.fuel * stacks}%`);
				if (cfg.perChoice.durability) parts.push(`耐久 ${cfg.perChoice.durability * stacks}%`);
				if (cfg.perChoice.comfort) parts.push(`舒适 ${cfg.perChoice.comfort * stacks}%`);
				if (cfg.perChoice.gold) parts.push(`金币 ${cfg.perChoice.gold * stacks}`);
				if (parts.length) effectDesc += ` → 当前：每次抉择 ${parts.join('，')}`;
			}
			if (cfg.onApply && cfg.onApply.maxWeight) {
				effectDesc += ` → 已累计减少载重 ${Math.abs(cfg.onApply.maxWeight * stacks)}kg`;
			}
			if (cfg.perChoiceRandom) {
				const chance = Math.min(100, (cfg.perChoiceRandom.baseChance || 0.15) * stacks * 100);
				effectDesc += ` → 当前触发概率 ${chance.toFixed(0)}%`;
			}
		}

		const escapedDesc = effectDesc.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
		html += `<span class="debuff-tag-chip inline-block bg-orange-900/60 text-orange-300 text-xs px-1.5 py-0.5 rounded border border-orange-800/50 cursor-default relative" data-tooltip="${escapedDesc}" data-stacks="${stacks}">${cfg.icon} ${cfg.name}${stackText}</span>`;
	}
	tagsContainer.innerHTML = html;

	// 绑定悬停 tooltip
	tagsContainer.querySelectorAll('.debuff-tag-chip').forEach(chip => {
		chip.addEventListener('mouseenter', showDebuffTooltip);
		chip.addEventListener('mouseleave', hideDebuffTooltip);
	});
}

// 衰变 debuff 标签悬停 tooltip
function showDebuffTooltip(e) {
	hideDebuffTooltip();
	const chip = e.currentTarget;
	const text = chip.getAttribute('data-tooltip');
	const stacks = chip.getAttribute('data-stacks');
	if (!text) return;

	const tip = document.createElement('div');
	tip.id = 'debuff-tooltip';
	tip.className = 'fixed z-[9999] bg-[#1a0f0a] border border-orange-800 rounded-lg px-3 py-2 text-xs shadow-lg pointer-events-none max-w-[260px]';
	tip.innerHTML = `<div class="text-orange-400 font-bold mb-1">${chip.textContent} <span class="text-orange-300 font-mono">×${stacks}</span></div><div class="text-gray-300 leading-relaxed">${text}</div>`;
	document.body.appendChild(tip);

	const rect = chip.getBoundingClientRect();
	tip.style.left = Math.min(rect.left, window.innerWidth - 270) + 'px';
	tip.style.top = (rect.bottom + 6) + 'px';
}

function hideDebuffTooltip() {
	const existing = document.getElementById('debuff-tooltip');
	if (existing) existing.remove();
}

// 困难模式标签悬停 tooltip
function showHardModeTooltip(e) {
	hideHardModeTooltip(); // 清除已有的
	const chip = e.currentTarget;
	const text = chip.getAttribute('data-tooltip');
	const pts = chip.getAttribute('data-points');
	if (!text) return;

	const tip = document.createElement('div');
	tip.id = 'hard-mode-tooltip';
	tip.className = 'fixed z-[9999] bg-[#1a0a0a] border border-red-800 rounded-lg px-3 py-2 text-xs shadow-lg pointer-events-none max-w-[260px]';
	tip.innerHTML = `<div class="text-red-400 font-bold mb-1">${chip.textContent} <span class="text-amber-400 font-mono">+${pts}pt</span></div><div class="text-gray-300 leading-relaxed">${text.replace(/ \| /g, '<br>')}</div>`;
	document.body.appendChild(tip);

	const rect = chip.getBoundingClientRect();
	tip.style.left = Math.min(rect.left, window.innerWidth - 270) + 'px';
	tip.style.top = (rect.bottom + 6) + 'px';
}

function hideHardModeTooltip() {
	const existing = document.getElementById('hard-mode-tooltip');
	if (existing) existing.remove();
}

// 首次用户交互时初始化音效（浏览器要求）
function initAudioOnFirstInteraction() {
	const init = () => {
		if (typeof initGameAudio === "function") initGameAudio();
		document.removeEventListener("click", init);
		document.removeEventListener("keydown", init);
		document.removeEventListener("touchstart", init);
	};
	document.addEventListener("click", init, { once: false });
	document.addEventListener("keydown", init, { once: false });
	document.addEventListener("touchstart", init, { once: false });
}

// 等待DOM和所有脚本加载完毕后启动游戏
document.addEventListener('DOMContentLoaded', () => {
	initializeGame();
	if (typeof initGameAudio === "function") initAudioOnFirstInteraction();
});
