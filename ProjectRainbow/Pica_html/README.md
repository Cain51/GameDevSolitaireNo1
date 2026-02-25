# 开发指南

## 项目概览

- 入口页面：`index.html` / `game.html`
- 核心 JS 目录：`js/`。主要子目录：
  - `game_config/`：数据与配置（事件、物品、商人、乘客、皮卡样式、结局）
  - `gameplay/`：游戏玩法逻辑（`event-handler.js`、`text-generation.js`、`game-over.js` 等）
  - `game_show/`：显示/动画（皮卡渲染、道路、弹窗）
  - `game_system/`：保存/状态/背包等系统

此文档侧重帮助开发者快速理解事件系统与配置结构，便于新增事件、调整平衡或调试流程。

## 关键模块：事件调度与处理（`js/gameplay/event-handler.js`）

- 主要职责：检测触发条件、选择事件、暂停游戏、播放前奏动画、展示事件弹窗，处理玩家选择并应用效果。
- 重要函数：
  - `checkEventTrigger()`：根据文本计数和 `GAME_CONFIG.triggerInterval` 决定是否触发事件；会更新里程与燃油并可能触发强制昼夜休息事件。
  - `findAvailableEvent()`：遍历 `getAllEvents()` 返回的事件字典，过滤条件（一次性、最近去重、资源/乘客要求、解锁状态），按 `triggerWeight` 抽取事件。
  - `triggerEvent(event)`：记录触发、管理最近事件队列、道路减速并显示触发字与事件弹窗。
  - `displayEventModal(event)` / `removeEventModal()`：生成并移除事件弹窗 DOM。
  - `handleEventChoice(eventId, choiceId)`：应用选择的 message 与 `result.effects`，调用 `processEffects()` 并恢复游戏流（若无子模态）。
  - `processEffects(fx, textArea)`：递归解析并执行效果，支持 `weighted`、`chance`、`sequence`/`all`、`choice`（二级选择）等复合结构。
  - `applyBasicEffect(fx, textArea)`：处理基础效果（加/减物品、金币、燃油、耐久、舒适度、添加/移除乘客、解锁事件、打开子模态框等）。

关键全局状态与依赖：
- `gameState`（文本计数、触发历史、解锁等）、`truckState`（燃油/耐久/舒适/乘客数组）、`inventoryState`（金币、物品）。
- 配置与数据依赖：`GAME_CONFIG`、`ITEMS_CONFIG`、`GAME_EVENTS` / 各类 `EVENTS_*` 常量。

## 配置与数据（`js/game_config/`）

- `game-config.js`：全局数值与动画时间（`triggerInterval`、`fuelConsumptionPer5km`、`animation`、`maxRestPerCycle` 等）。修改这里会影响全局节奏与事件频率。
- `items-config.js`：`ITEMS_CONFIG` 定义物品属性（name、color、useEffect 等）和 `CRAFTING_RECIPES`。
- `merchant-config.js`：`MERCHANT_CONFIG` 定义不同商人及商品价格列表。
- `passenger-config.js`、`truck-style-config.js`：皮卡模板、乘客字符与渲染样式。
- `endings/endings-config.js`：结局文本与配色。
- `events/`：按分类拆分的事件源文件（`events-encounter.js`、`events-stop.js`、`events-merchant.js`、`events-special.js` 等），`events-index.js` 会合并为 `GAME_EVENTS`。

### 如何新增事件

1. 在 `js/game_config/events/` 新建或编辑合适的文件（例如 `events-encounter.js`）。
2. 按现有事件格式定义对象并赋给 `EVENTS_*`（或直接在已有文件中添加）：

```javascript
// 简单事件示例
my_event: {
  id: "my_event",
  title: "路边的奇怪箱子",
  description: "一个破旧的木箱放在路边……",
  image: "箱！",
  oneTime: false, // 是否是唯一事件
  triggerWeight: 10,
  condition: { minGold: 5 }, // 目前可选：requiresItem / requiresPassenger / minGold
  choices: [
    {
      id: "open",
      text: "打开箱子",
      description: "也许里面有宝贝",
      result: {
        message: "你打开了箱子，里面有金币。",
        effects: { gold: 12, randomLoot: true }
      }
    },
  ]
}
```

1. `events-index.js` 会在运行时合并已定义的 `EVENTS_*` 对象到 `GAME_EVENTS`。

事件效果说明：
- addItems / removeItems: [{id, quantity}]
- gold: 正为获得，负为消耗
- fuel / durability / comfort: 数值变动
- addPassenger / removePassenger
- favor: { "鹿": 10, "猎人": -5 } — 对指定乘客增减好感度（仅对当前在车上的乘客生效，0–100）
- favorAll: 5 — 对当前车上所有乘客增加相同好感度
- unlockEvents: ["event_id"]
- openMerchantModal / openRestModal / openCraftingModal
- gameOver: true
- 特殊效果类型：`{type: 'weighted'|'chance'|'choice'|'sequence', ...}`（参见 `processEffects`）

## 修改配置与平衡

- 调整事件频率：修改 `GAME_CONFIG.triggerInterval` 或 `triggerWeight`。
- 控制休息频率与最大休息次数：`overnightRestInterval` / `maxRestPerCycle`。
- 平衡物价：编辑 `merchant-config.js`。