![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| **简体中文** | [English](README_en.md) | [繁體中文](README_zh-Hant.md) | [日本語](README_ja.md) | [![Crowdin](https://badges.crowdin.net/cyanstars/localized.svg)](https://crowdin.com/project/cyanstars)

# CyanStars

> 音乐，是宇宙的共通语言。

翠蓝星穹（CyanStars）是一款由无圻寻光开发组（IPOL）开发的中文虚拟歌手同人、非商、社区音乐游戏。

## 参与开发

欢迎参与代码/游戏内容/翻译的贡献，请阅读 [贡献指南](CONTRIBUTING.md)！

源代码与多媒体文件请在 [LICENSE](LICENSE.md) 中约定的范围内使用。

## 相关链接

社区贡献者/收录曲目/隐私协议等，见于 [附属文档](https://ipol-studio.github.io/CyanStars_Docs/)。

- 附：[附属文档仓库](https://github.com/IPOL-Studio/CyanStars_Docs)

多媒体文件作为子模块引用，路径为 `Cyan-Stars/Assets/CysMultimediaAssets`，对于多媒体文件的贡献指南请查阅子模块仓库说明。

- 附：[多媒体文件仓库](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

玩家交流 QQ 群：`827556233`

版权、安全性、成员与社区、或是其他问题，欢迎随时联系我们 <chenluan@cyanstars.onaliyun.com>。

## 开发文档

### 克隆仓库和子模块（如果正在使用 git 命令行）

```
git clone --recurse-submodules https://github.com/IPOL-Studio/CyanStars.git
```

或

```
git clone https://github.com/IPOL-Studio/CyanStars.git
cd CyanStars
git submodule update --init --recursive
```

### 下载 Unity 编辑器

若您已安装 Unity Hub，可将此链接复制到浏览器，调起 Unity Hub 内下载：`unityhub://6000.3.0f1/d1870ce95baf`。

或从 [Unity 官网](https://unity3d.com/get-unity/download/archive) 手动下载 `6000.3.0f1` 版本的编辑器。

### 导入项目和依赖包

在 Unity 中选择打开 `Cyan-Stars` 项目目录。

项目依赖于 Unity 包和 Nuget 包，首次启动时将通过 Unity 编辑器自动下载安装 Unity 包，选择 `Ignore` 忽略编译错误后 Nuget For Unity 将自动下载安装 Nuget 包。

重启编辑器并打开 `CyanStarsEntry` 场景。

### 下载其他必要资源

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - 一个第三方 Unity 包，用于在 Unity 项目中播放 Live2D 资源。在同意协议后、下载并拖动到 Unity 编辑器内导入，然后重启一次编辑器。

### 构建安装包

从 Unity 编辑器上方菜单中选中 `CyanStars 工具箱` 进行构建。
