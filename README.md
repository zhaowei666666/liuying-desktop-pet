<div align="center">
  <img src="assets/ChatGPT%20Image%202026%E5%B9%B45%E6%9C%888%E6%97%A5%2014_21_59.png" width="180" alt="流萤桌宠">
  <h1>流萤桌宠 (Firefly Desktop Pet)</h1>
  <p>一个轻量、透明、治愈系的 Windows Q 版流萤桌面小伙伴</p>

  <div>
    <img src="https://img.shields.io/badge/Platform-Windows-0078D4?style=for-the-badge&logo=windows" alt="Windows">
    <img src="https://img.shields.io/badge/Framework-WPF%20.NET%2010-512BD4?style=for-the-badge&logo=.net" alt="WPF">
    <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License">
    <img src="https://img.shields.io/badge/Status-Cute%20&%20Tiny-F6C6D9?style=for-the-badge" alt="Status">
  </div>
</div>

---

## 🌟 项目简介

**流萤桌宠** 是一款专为 Windows 设计的桌面伴侣。她安静地待在你的屏幕角落，陪伴你工作和学习。采用 WPF 技术实现，具有极低的内存占用和极高的响应速度。

> "我会，在那里的。" —— 流萤

## ✨ 核心特性

- **🍃 轻量纯净**：无边框透明窗口，不占任务栏，支持点击穿透（安静模式）。
- **💓 灵动呼吸**：基于关键帧的缓动动画，呼吸、眨眼、回弹效果丝滑自然。
- **🕙 随心而动**：根据系统时间（早晨、午间、傍晚、夜晚、深夜）自动切换不同状态。
- **🤫 安静模式**：一键进入深度专注模式，仅保留基础呼吸动作，绝不打扰。
- **🎨 高度自定义**：通过 `assets/manifest.json` 轻松替换所有图片素材。

## 🚀 快速开始

### 1. 下载即用
您可以直接从 [Releases](../../releases) 页面下载打包好的安装包：
- 下载 `流萤桌宠_安装包.zip`
- 解压后运行 `安装流萤桌宠.cmd` 即可完成部署。

### 2. 本地开发
如果您想自行编译运行，请确保已安装 [.NET 10 SDK](https://dotnet.microsoft.com/download)。

```bash
# 克隆仓库
git clone https://github.com/zhaowei666666/liuying-desktop-pet.git

# 进入目录
cd liuying-desktop-pet

# 运行
dotnet run
```

## 🎮 互动指南

| 操作 | 响应 |
| :--- | :--- |
| **鼠标靠近** | 捕捉到你的气息，给予开心的回应 |
| **快速经过** | 吓一跳！(°Д°) |
| **鼠标单击** | 害羞地对你笑 |
| **拖拽移动** | 抱起流萤，松手后她会优雅回弹 |
| **闲置状态** | 偶尔会眨眨眼、挥挥手，或者陷入沉思 |

## 🛠️ 托盘功能

右键点击系统托盘图标，可以快速访问：
- 👁️ 显示/隐藏桌宠
- 📌 置顶显示开关
- 🌙 安静模式切换
- ⚙️ 开机自启设置
- 🔍 缩放比例调节 (75% - 150%)
- 🔄 重新加载素材

## 📂 目录结构

- `assets/`：存放所有 PNG 素材及 `manifest.json` 配置文件。
- `仓库主页/`：项目的静态展示页，可以在 [GitHub Pages](https://zhaowei666666.github.io/liuying-desktop-pet/) 查看。
- `正式发行/`：包含预编译的二进制文件及一键安装脚本。

## 📄 开源协议

本项目采用 [MIT License](LICENSE) 开源。

---

<div align="center">
  <p>如果您喜欢这个项目，欢迎给一个 ⭐️ Star！</p>
  <p>Made with ❤️ by zhaowei</p>
</div>
