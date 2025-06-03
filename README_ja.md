![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| **简体中文** | [English](README_en.md) |

# CyanStars

> 音乐，是宇宙的共通语言。

翠蓝星穹（CyanStars）是一款由无圻寻光开发组（IPOL）开发的中文虚拟歌手同人、非商、社区音乐游戏。

## 参与开发

在您第一次开发之前，请务必花一点点时间阅读一下 [贡献指南](CONTRIBUTING.md)，非常感谢！

源代码与多媒体文件请在 [LICENSE](LICENSE) 和 [NOTICE](NOTICE) 中约定的规范内使用。

若要发布修改后的游戏文件，请明确指出“此游戏以经过修改，非 IPOL 原版内容”等类似表达，以将您的作品与此仓库发布的作品进行区分。修改后的游戏文件不得在任何国家/地区注册版权。

記事やプロジェクトの翻訳に参加したいですか？[crowdin](https://zh.crowdin.com/project/cyanstars) に参加してください！

## 相关链接

ドキュメントへの貢献

- ドキュメントのウェブページは[こちら](https://ipol-studio.github.io/CyanStars_Docs)でご覧ください。

多媒体文件作为子模块引用，路径为 `Cyan-Stars/Assets/CysMultimediaAssets`，对于多媒体文件的贡献指南请查阅子模块仓库说明。

- 附：[多媒体文件仓库](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

在美术/音乐/谱面/程序/翻译等方面有特长，且有兴趣参与我们的内部讨论？欢迎点 [这里](http://chenluan.mikecrm.com/JeKq3DU) 来申请加入我们。

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

若您已安装 Unity Hub，可将此链接复制到浏览器，调起 Unity Hub 内下载：`unityhub://2020.3.28f1/f5400f52e03f`。

或从 [Unity 官网](https://unity3d.com/get-unity/download/archive) 手动下载 `2020.3.28f1` 版本的编辑器。

### 下载其他必要资源

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - 一个第三方 Unity 包，用于在 Unity 项目中播放 Live2D 资源。在同意协议后、下载并拖动到 Unity 编辑器内导入，然后重启一次编辑器。

### 构建安装包

从 Unity 编辑器上方菜单中选中 `CyanStars 工具箱` 进行构建。
