// 扫雷小游戏模块
// 25×15 网格（宽×高），比例≈1.67:1 匹配游戏窗口
// 48 个地雷，限时 60 秒
// 首次点击安全：点击后才放置地雷，确保首次点击格及其周围无雷
// 工具切换：⛏铲子(翻开) / 🚩旗子(标记地雷) / ❓问号(不确定标记)
// 倒计时结束后结算：每正确标记一个雷 +1 金币，标记错误 -1 金币，总收益不为负

const MS_ROWS = 15;
const MS_COLS = 25;
const MS_MINES = 48;
const MS_TIME_LIMIT = 60;
const MS_VP_ROWS = 14;
const MS_VP_COLS = 23;

// ─── 扫雷核心逻辑 ─────────────────────────────────────────

function createMinesweeperBoard() {
  const board = [];
  for (let r = 0; r < MS_ROWS; r++) {
    board[r] = [];
    for (let c = 0; c < MS_COLS; c++) {
      board[r][c] = {
        mine: false,
        revealed: false,
        flagged: false,
        questioned: false, // 问号状态
        adjacentMines: 0,
      };
    }
  }
  return board;
}

function placeMines(board, safeR, safeC) {
  const safeSet = new Set();
  for (let dr = -1; dr <= 1; dr++) {
    for (let dc = -1; dc <= 1; dc++) {
      const nr = safeR + dr, nc = safeC + dc;
      if (nr >= 0 && nr < MS_ROWS && nc >= 0 && nc < MS_COLS) {
        safeSet.add(nr * MS_COLS + nc);
      }
    }
  }
  let placed = 0;
  while (placed < MS_MINES) {
    const r = Math.floor(Math.random() * MS_ROWS);
    const c = Math.floor(Math.random() * MS_COLS);
    if (!board[r][c].mine && !safeSet.has(r * MS_COLS + c)) {
      board[r][c].mine = true;
      placed++;
    }
  }
  for (let r = 0; r < MS_ROWS; r++) {
    for (let c = 0; c < MS_COLS; c++) {
      if (board[r][c].mine) continue;
      let count = 0;
      for (let dr = -1; dr <= 1; dr++) {
        for (let dc = -1; dc <= 1; dc++) {
          const nr = r + dr, nc = c + dc;
          if (nr >= 0 && nr < MS_ROWS && nc >= 0 && nc < MS_COLS && board[nr][nc].mine) count++;
        }
      }
      board[r][c].adjacentMines = count;
    }
  }
}

function revealCell(board, r, c) {
  if (r < 0 || r >= MS_ROWS || c < 0 || c >= MS_COLS) return;
  const cell = board[r][c];
  if (cell.revealed || cell.flagged) return;
  cell.revealed = true;
  cell.questioned = false;
  if (cell.mine) return;
  if (cell.adjacentMines === 0) {
    for (let dr = -1; dr <= 1; dr++) {
      for (let dc = -1; dc <= 1; dc++) {
        if (dr === 0 && dc === 0) continue;
        revealCell(board, r + dr, c + dc);
      }
    }
  }
}

// ─── 扫雷 UI ──────────────────────────────────────────────

let _msBoard = null;
let _msTimer = null;
let _msTimeLeft = 0;
let _msGameOver = false;
let _msFirstClick = true;
let _msViewRow = 0;
let _msViewCol = 0;
let _msKeyHandler = null;
let _msTool = "dig"; // 当前工具: "dig" | "flag" | "question"

function msClampView() {
  _msViewRow = Math.max(0, Math.min(MS_ROWS - MS_VP_ROWS, _msViewRow));
  _msViewCol = Math.max(0, Math.min(MS_COLS - MS_VP_COLS, _msViewCol));
}

function msMoveView(dr, dc) {
  _msViewRow += dr;
  _msViewCol += dc;
  msClampView();
  renderMinesweeperGrid();
  msUpdateArrowStates();
}

function msUpdateArrowStates() {
  const upBtn = document.getElementById("ms-arrow-up");
  const downBtn = document.getElementById("ms-arrow-down");
  const leftBtn = document.getElementById("ms-arrow-left");
  const rightBtn = document.getElementById("ms-arrow-right");
  if (upBtn) upBtn.style.opacity = _msViewRow <= 0 ? "0.25" : "1";
  if (downBtn) downBtn.style.opacity = _msViewRow >= MS_ROWS - MS_VP_ROWS ? "0.25" : "1";
  if (leftBtn) leftBtn.style.opacity = _msViewCol <= 0 ? "0.25" : "1";
  if (rightBtn) rightBtn.style.opacity = _msViewCol >= MS_COLS - MS_VP_COLS ? "0.25" : "1";
  const coordEl = document.getElementById("ms-coord");
  if (coordEl) coordEl.textContent = `[列${_msViewCol + 1}~${_msViewCol + MS_VP_COLS}, 行${_msViewRow + 1}~${_msViewRow + MS_VP_ROWS}]`;
}

// 切换工具
function msSetTool(tool) {
  _msTool = tool;
  msUpdateToolUI();
}

function msUpdateToolUI() {
  const tools = ["dig", "flag", "question"];
  tools.forEach(t => {
    const btn = document.getElementById("ms-tool-" + t);
    if (!btn) return;
    if (t === _msTool) {
      btn.style.background = "#7c3aed";
      btn.style.borderColor = "#a78bfa";
      btn.style.color = "#fff";
      btn.style.transform = "scale(1.1)";
    } else {
      btn.style.background = "#2d225080";
      btn.style.borderColor = "#a78bfa40";
      btn.style.color = "#a78bfa";
      btn.style.transform = "scale(1)";
    }
  });
}

function showMinesweeperModal() {
  _msBoard = createMinesweeperBoard();
  _msTimeLeft = MS_TIME_LIMIT;
  _msGameOver = false;
  _msFirstClick = true;
  _msTool = "dig";
  _msViewRow = Math.floor((MS_ROWS - MS_VP_ROWS) / 2);
  _msViewCol = Math.floor((MS_COLS - MS_VP_COLS) / 2);
  msClampView();

  const gameCanvas = document.getElementById("game-canvas");
  if (!gameCanvas) return;

  const modal = document.createElement("div");
  modal.id = "minesweeper-modal";
  modal.className = "absolute inset-0 bg-black/90 flex items-center justify-center z-[60]";
  modal.style.cssText = "animation: eventModalFadeIn 0.3s ease-out;";

  // 工具按钮样式
  const toolBtnBase = `
    display:flex; align-items:center; justify-content:center; gap:3px;
    padding:4px 10px; border-radius:6px; cursor:pointer; user-select:none;
    transition:all 0.15s; font-size:13px; line-height:1;
    border:1.5px solid #a78bfa40; background:#2d225080; color:#a78bfa;
  `;

  modal.innerHTML = `
    <div class="flex flex-col items-center w-full h-full" style="max-width:100%;max-height:100%;padding:6px 8px;">
      <!-- 顶部信息栏 -->
      <div class="flex items-center justify-between w-full mb-1 px-1" style="flex-shrink:0;">
        <div class="text-purple-400 font-bold text-sm">🏛️ 古老的谜题</div>
        <div class="flex items-center gap-3">
          <span class="text-gray-500 text-xs font-mono" id="ms-coord"></span>
          <span class="text-gray-400 text-xs">🚩 <span id="ms-flag-count">0</span>/${MS_MINES}</span>
          <span class="text-yellow-400 text-xs font-mono" id="ms-timer">⏱ ${MS_TIME_LIMIT}s</span>
        </div>
      </div>

      <!-- 工具栏：铲子/旗子/问号 + 操作提示 -->
      <div class="flex items-center justify-between w-full mb-1 px-1" style="flex-shrink:0;">
        <div class="flex items-center gap-2">
          <button id="ms-tool-dig" onclick="msSetTool('dig')" style="${toolBtnBase}">⛏️ 翻开</button>
          <button id="ms-tool-flag" onclick="msSetTool('flag')" style="${toolBtnBase}">🚩 标雷</button>
          <button id="ms-tool-question" onclick="msSetTool('question')" style="${toolBtnBase}">❓ 存疑</button>
        </div>
        <div class="text-gray-600 text-xs">PC右键=标雷 | 倒计时结束自动结算</div>
      </div>

      <!-- 网格+箭头区域：缩小留出边距给箭头 -->
      <div class="flex-1 flex items-center justify-center" style="min-height:0; width:100%; padding:24px 30px;">
        <div id="ms-wrapper" style="position:relative; width:100%; max-height:100%; aspect-ratio:${MS_VP_COLS}/${MS_VP_ROWS};">
          <!-- 网格本体 -->
          <div id="ms-grid" class="select-none" style="
            position:absolute; inset:0;
            display:grid;
            grid-template-columns:repeat(${MS_VP_COLS}, 1fr);
            grid-template-rows:repeat(${MS_VP_ROWS}, 1fr);
            gap:0;
            overflow:hidden;
            background:#0d0d1a;
            border:1px solid #a78bfa40;
            border-radius:6px;
          "></div>

          <!-- 上箭头 -->
          <button id="ms-arrow-up" onclick="msMoveView(-1,0)" style="
            position:absolute; top:-22px; left:0; right:0; height:20px;
            display:flex; align-items:center; justify-content:center;
            background:#2d225080; border:1px solid #a78bfa40; border-radius:4px 4px 0 0;
            color:#a78bfa; cursor:pointer; user-select:none; transition:all 0.15s;
            font-size:12px; line-height:1;
          " onmouseenter="this.style.background='#3b327080';this.style.borderColor='#a78bfa'" onmouseleave="this.style.background='#2d225080';this.style.borderColor='#a78bfa40'">▲</button>

          <!-- 下箭头 -->
          <button id="ms-arrow-down" onclick="msMoveView(1,0)" style="
            position:absolute; bottom:-22px; left:0; right:0; height:20px;
            display:flex; align-items:center; justify-content:center;
            background:#2d225080; border:1px solid #a78bfa40; border-radius:0 0 4px 4px;
            color:#a78bfa; cursor:pointer; user-select:none; transition:all 0.15s;
            font-size:12px; line-height:1;
          " onmouseenter="this.style.background='#3b327080';this.style.borderColor='#a78bfa'" onmouseleave="this.style.background='#2d225080';this.style.borderColor='#a78bfa40'">▼</button>

          <!-- 左箭头 -->
          <button id="ms-arrow-left" onclick="msMoveView(0,-1)" style="
            position:absolute; left:-28px; top:0; bottom:0; width:26px;
            display:flex; align-items:center; justify-content:center;
            background:#2d225080; border:1px solid #a78bfa40; border-radius:4px 0 0 4px;
            color:#a78bfa; cursor:pointer; user-select:none; transition:all 0.15s;
            font-size:12px; line-height:1;
          " onmouseenter="this.style.background='#3b327080';this.style.borderColor='#a78bfa'" onmouseleave="this.style.background='#2d225080';this.style.borderColor='#a78bfa40'">◀</button>

          <!-- 右箭头 -->
          <button id="ms-arrow-right" onclick="msMoveView(0,1)" style="
            position:absolute; right:-28px; top:0; bottom:0; width:26px;
            display:flex; align-items:center; justify-content:center;
            background:#2d225080; border:1px solid #a78bfa40; border-radius:0 4px 4px 0;
            color:#a78bfa; cursor:pointer; user-select:none; transition:all 0.15s;
            font-size:12px; line-height:1;
          " onmouseenter="this.style.background='#3b327080';this.style.borderColor='#a78bfa'" onmouseleave="this.style.background='#2d225080';this.style.borderColor='#a78bfa40'">▶</button>
        </div>
      </div>
    </div>
  `;

  gameCanvas.appendChild(modal);

  // 初始化工具高亮
  msUpdateToolUI();

  // 绑定键盘事件
  _msKeyHandler = function(e) {
    if (_msGameOver) return;
    const m = document.getElementById("minesweeper-modal");
    if (!m) return;
    switch (e.key) {
      case "w": case "W": case "ArrowUp":    msMoveView(-1, 0); e.preventDefault(); break;
      case "s": case "S": case "ArrowDown":  msMoveView(1, 0);  e.preventDefault(); break;
      case "a": case "A": case "ArrowLeft":  msMoveView(0, -1); e.preventDefault(); break;
      case "d": case "D": case "ArrowRight": msMoveView(0, 1);  e.preventDefault(); break;
      // 快捷键切换工具
      case "1": msSetTool("dig");      e.preventDefault(); break;
      case "2": msSetTool("flag");     e.preventDefault(); break;
      case "3": msSetTool("question"); e.preventDefault(); break;
    }
  };
  document.addEventListener("keydown", _msKeyHandler);

  renderMinesweeperGrid();
  msUpdateArrowStates();
  startMinesweeperTimer();
}

function renderMinesweeperGrid() {
  const grid = document.getElementById("ms-grid");
  if (!grid || !_msBoard) return;

  grid.style.gridTemplateColumns = `repeat(${MS_VP_COLS}, 1fr)`;
  grid.style.gridTemplateRows = `repeat(${MS_VP_ROWS}, 1fr)`;
  grid.innerHTML = "";
  let flagCount = 0;

  for (let r = 0; r < MS_ROWS; r++) {
    for (let c = 0; c < MS_COLS; c++) {
      if (_msBoard[r][c].flagged) flagCount++;
    }
  }

  for (let vr = 0; vr < MS_VP_ROWS; vr++) {
    for (let vc = 0; vc < MS_VP_COLS; vc++) {
      const r = _msViewRow + vr;
      const c = _msViewCol + vc;
      const cell = _msBoard[r][c];

      const div = document.createElement("div");
      div.dataset.r = r;
      div.dataset.c = c;
      div.style.cssText = `
        display:flex; align-items:center; justify-content:center;
        font-size:clamp(8px, 1.6vw, 14px); font-family:monospace; cursor:pointer;
        border:0.5px solid #374151; transition: background 0.1s;
        line-height:1; user-select:none;
        min-width:0; min-height:0;
      `;

      if (cell.revealed) {
        if (cell.mine) {
          div.style.background = "#7f1d1d";
          div.textContent = "💥";
          div.style.cursor = "default";
        } else {
          div.style.background = "#1a1a2e";
          div.style.cursor = "default";
          if (cell.adjacentMines > 0) {
            const colors = ["", "#3b82f6", "#22c55e", "#ef4444", "#1e40af", "#991b1b", "#0891b2", "#111", "#6b7280"];
            div.textContent = cell.adjacentMines;
            div.style.color = colors[cell.adjacentMines] || "#fff";
            div.style.fontWeight = "bold";
          }
        }
      } else if (cell.flagged) {
        div.style.background = "#1e1b4b";
        div.textContent = "🚩";
      } else if (cell.questioned) {
        div.style.background = "#2d2250";
        div.textContent = "❓";
        div.onmouseenter = function() { this.style.background = "#3b3270"; };
        div.onmouseleave = function() { this.style.background = "#2d2250"; };
      } else {
        div.style.background = "#2d2250";
        div.onmouseenter = function() { this.style.background = "#3b3270"; };
        div.onmouseleave = function() { this.style.background = "#2d2250"; };
      }

      if (!_msGameOver && !cell.revealed) {
        // 左键：根据当前工具执行操作
        div.addEventListener("click", (e) => {
          e.preventDefault();
          onMsCellAction(r, c);
        });
        // 右键：始终为标旗（PC快捷操作）
        div.addEventListener("contextmenu", (e) => {
          e.preventDefault();
          onMsCellToggleFlag(r, c);
        });
      }

      grid.appendChild(div);
    }
  }

  const flagEl = document.getElementById("ms-flag-count");
  if (flagEl) flagEl.textContent = flagCount;
}

// 根据当前工具处理格子点击
function onMsCellAction(r, c) {
  if (_msGameOver || !_msBoard) return;
  const cell = _msBoard[r][c];
  if (cell.revealed) return;

  switch (_msTool) {
    case "dig":
      if (cell.flagged) return; // 旗子格不能直接挖，需先取消
      if (cell.questioned) cell.questioned = false;
      // 首次点击安全
      if (_msFirstClick) {
        _msFirstClick = false;
        placeMines(_msBoard, r, c);
      }
      if (cell.mine) {
        cell.revealed = true;
      } else {
        revealCell(_msBoard, r, c);
      }
      break;

    case "flag":
      if (cell.flagged) {
        // 已有旗子 → 取消旗子
        cell.flagged = false;
      } else {
        // 放旗子（取消问号）
        cell.questioned = false;
        cell.flagged = true;
      }
      break;

    case "question":
      if (cell.questioned) {
        // 已有问号 → 取消
        cell.questioned = false;
      } else {
        // 放问号（取消旗子）
        cell.flagged = false;
        cell.questioned = true;
      }
      break;
  }

  renderMinesweeperGrid();
}

// 右键快捷：切换旗子（PC用户惯用操作）
function onMsCellToggleFlag(r, c) {
  if (_msGameOver || !_msBoard) return;
  const cell = _msBoard[r][c];
  if (cell.revealed) return;

  if (cell.flagged) {
    cell.flagged = false;
  } else {
    cell.questioned = false;
    cell.flagged = true;
  }
  renderMinesweeperGrid();
}

function startMinesweeperTimer() {
  if (_msTimer) clearInterval(_msTimer);
  _msTimer = setInterval(() => {
    _msTimeLeft--;
    const timerEl = document.getElementById("ms-timer");
    if (timerEl) {
      timerEl.textContent = `⏱ ${_msTimeLeft}s`;
      if (_msTimeLeft <= 10) timerEl.style.color = "#ef4444";
    }
    if (_msTimeLeft <= 0) {
      clearInterval(_msTimer);
      _msTimer = null;
      endMinesweeper();
    }
  }, 1000);
}

function endMinesweeper() {
  _msGameOver = true;
  if (_msTimer) { clearInterval(_msTimer); _msTimer = null; }

  if (_msKeyHandler) {
    document.removeEventListener("keydown", _msKeyHandler);
    _msKeyHandler = null;
  }

  let correct = 0;
  let wrong = 0;
  for (let r = 0; r < MS_ROWS; r++) {
    for (let c = 0; c < MS_COLS; c++) {
      const cell = _msBoard[r][c];
      if (cell.flagged) {
        if (cell.mine) correct++;
        else wrong++;
      }
    }
  }
  const rawReward = correct - wrong;
  const reward = Math.max(0, rawReward);

  // 揭示所有地雷
  for (let r = 0; r < MS_ROWS; r++) {
    for (let c = 0; c < MS_COLS; c++) {
      if (_msBoard[r][c].mine) _msBoard[r][c].revealed = true;
    }
  }
  renderMinesweeperGrid();

  if (reward > 0 && typeof addGold === "function") {
    addGold(reward);
  }

  // 正确标记超过10个：奖励珍品"密钥"
  let gotKey = false;
  if (correct > 10 && typeof addItem === "function" && typeof ITEMS_CONFIG !== "undefined" && ITEMS_CONFIG["密钥"]) {
    gotKey = addItem("密钥", 1);
  }

  const grid = document.getElementById("ms-grid");
  if (grid) {
    const overlay = document.createElement("div");
    overlay.className = "absolute inset-0 flex items-center justify-center z-10";
    overlay.style.cssText = "background:rgba(0,0,0,0.85); border-radius:4px;";
    const keyHtml = gotKey
      ? `<p class="text-purple-300 font-bold mt-2">🔑 石碑闪耀，你获得了珍品 <span style="color:#c084fc">「密钥」</span>！</p>`
      : (correct > 10 && !gotKey ? `<p class="text-gray-500 mt-1">（背包已满，密钥消散在空气中…）</p>` : '');
    overlay.innerHTML = `
      <div class="text-center p-4">
        <div class="text-2xl mb-2">${reward > 0 ? '🎉' : '😅'}</div>
        <div class="text-purple-300 font-bold text-sm mb-2">谜题结束！</div>
        <div class="text-xs text-gray-400 space-y-1">
          <p>🚩 正确标记：<span class="text-green-400 font-bold">${correct}</span> 个地雷</p>
          <p>❌ 错误标记：<span class="text-red-400 font-bold">${wrong}</span> 个</p>
          <p class="text-yellow-400 font-bold mt-2">💰 获得 ${reward} 金币${rawReward < 0 ? '（收益已保底为0）' : ''}</p>
          ${keyHtml}
        </div>
        <button onclick="closeMinesweeperModal()" class="mt-3 px-4 py-1.5 bg-purple-700 hover:bg-purple-600 text-white text-xs rounded-full transition-all">
          继续旅程
        </button>
      </div>
    `;
    grid.style.position = "relative";
    grid.appendChild(overlay);
  }

  const textArea = document.getElementById("textArea");
  if (textArea) {
    textArea.innerHTML += `<p style="color:#a78bfa">🏛️ 神庙谜题结算：正确标记 ${correct} 个地雷，错误 ${wrong} 个，获得 <span style="color:#facc15">${reward} 金币</span></p>`;
    if (gotKey) {
      textArea.innerHTML += `<p style="color:#c084fc">🔑 石碑上的符号汇聚成形——你获得了珍品「密钥」！</p>`;
    }
    if (typeof scrollTextAreaToBottom === "function") scrollTextAreaToBottom(textArea);
  }
}

function closeMinesweeperModal() {
  if (_msKeyHandler) {
    document.removeEventListener("keydown", _msKeyHandler);
    _msKeyHandler = null;
  }
  const modal = document.getElementById("minesweeper-modal");
  if (modal) {
    let resumed = false;
    const doResume = () => {
      if (resumed) return;
      resumed = true;
      modal.remove();
      if (typeof resumeGame === "function") resumeGame();
    };
    modal.classList.add("modal-fade-out");
    modal.addEventListener("animationend", doResume, { once: true });
    // 安全兜底：万一 animationend 未触发，300ms 后强制恢复
    setTimeout(doResume, 300);
  } else {
    // 弹窗不存在时也要确保游戏恢复
    if (typeof resumeGame === "function") resumeGame();
  }
  if (typeof updateTruckStatusDisplay === "function") updateTruckStatusDisplay();
  if (typeof updateInventoryDisplay === "function") updateInventoryDisplay();
}
