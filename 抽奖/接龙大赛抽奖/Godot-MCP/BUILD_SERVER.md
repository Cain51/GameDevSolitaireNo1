# 构建 Godot-MCP 服务器（必做一步）

本机未检测到 Node.js。请先安装 Node.js，再在终端执行以下命令完成 MCP 服务器构建：

## 1. 安装 Node.js

- 打开 https://nodejs.org/ 下载并安装 LTS 版本。
- 安装完成后**重新打开终端**（或重启 Cursor），使 `node` 和 `npm` 生效。

## 2. 构建 MCP 服务器

在终端中执行（请根据实际路径调整）：

```powershell
cd "d:\游戏制作\GameDevSolitaireNo1\抽奖\接龙大赛抽奖\Godot-MCP\server"
npm install
npm run build
```

构建成功后，`server/dist/index.js` 会出现，Cursor 中的 Godot-MCP 即可使用。

## 3. 在 Godot 中启用插件

1. 用 Godot 打开本项目（`接龙大赛抽奖` 的 `project.godot`）。
2. 打开 **项目 → 项目设置 → 插件**。
3. 找到 **Godot MCP** 并勾选启用。

完成以上步骤后，在 Cursor 中重启一次 Cursor，即可通过 MCP 与 Godot 项目交互。
