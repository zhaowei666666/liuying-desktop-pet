<p align="center">
  <img src="assets/ChatGPT%20Image%202026%E5%B9%B45%E6%9C%888%E6%97%A5%2014_21_59.png" width="210" alt="流萤桌宠">
</p>

<h1 align="center">流萤桌宠</h1>

<p align="center">
  一个轻量、透明、不会乱跑的 Windows Q 版流萤桌面小伙伴。
</p>

<p align="center">
  <img alt="WPF" src="https://img.shields.io/badge/WPF-.NET%2010-4CBFBA?style=for-the-badge">
  <img alt="Windows" src="https://img.shields.io/badge/Windows-Desktop-1F2937?style=for-the-badge">
  <img alt="License" src="https://img.shields.io/badge/status-cute%20and%20tiny-F6C6D9?style=for-the-badge">
</p>

## 亮点

| 能力 | 说明 |
| --- | --- |
| 轻量悬浮 | WPF 透明无边框窗口，默认置顶，不占普通任务栏。 |
| 不打扰 | 不会追鼠标、不会头顶冒字，只保留必要互动。 |
| 关键帧动作 | 呼吸、弹跳、受惊、拖拽、松手回弹都用缓动关键帧。 |
| 随机小动作 | 空闲时偶尔挥手、眨眼或切换时间段动作。 |
| 安静模式 | 托盘一键切到只呼吸和眨眼，适合学习或专注。 |
| 素材可替换 | 所有 PNG 由 `assets/manifest.json` 映射，不需要改代码。 |

## 互动

- 鼠标靠近：短暂开心回应。
- 鼠标快速经过：短暂受惊。
- 单击：害羞开心。
- 拖拽：抱起移动，松手后轻轻回弹。
- 空闲：自动随机播放低打扰小动作。
- 时间：早晨、午间、傍晚、夜晚、深夜有不同状态。
- 安静模式：关闭随机、靠近、受惊、点击反馈，只保留待机呼吸和眨眼。

## 运行

```powershell
dotnet run
```

模拟现实时间动态：

```powershell
dotnet run -- --time 23:30
```

框架依赖发布：

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

## 托盘菜单

- 显示/隐藏桌宠
- 置顶显示
- 安静模式
- 开机自启
- 缩放 75% / 100% / 125% / 150%
- 重新加载素材
- 打开素材文件夹
- 退出

## 素材

把透明 PNG 放进 `assets` 文件夹，然后在 `assets/manifest.json` 里映射到对应状态。当前素材已经做过透明背景处理，原始备份保留在本地忽略目录中。

## 设置位置

```text
%AppData%\流萤桌宠\settings.json
```

## 仓库展示页

仓库里附带了一个轻量静态展示页：

```text
仓库主页/index.html
```
