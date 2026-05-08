# 流萤桌宠

Windows 轻量桌面小桌宠，使用 WPF/C# 实现。窗口为透明无边框、默认置顶、不显示普通任务栏按钮，并通过托盘菜单控制显示、置顶、鼠标跟随、缩放、开机自启和素材重载。

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

## 素材替换

把透明 PNG 放进运行目录的 `assets` 文件夹。开发时也可以放在项目里的 `assets` 文件夹，然后重新构建运行。

默认素材文件名见 `assets/manifest.json`。如果图片文件名不同，只需要改 manifest，不需要改代码。

## 互动

- 鼠标靠近：开心回应。
- 鼠标快速经过：短暂受惊。
- 鼠标距离较远：开启跟随时会小跑靠近。
- 慢速移动鼠标：根据方向注视鼠标。
- 左键拖拽：抱起状态并移动桌宠。
- 单击：害羞开心回应。
- 现实时间：早晨、午间、傍晚、夜晚、深夜会切换不同待机动态。

## 设置

设置保存到：

```text
%AppData%\流萤桌宠\settings.json
```
