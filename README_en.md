![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| [简体中文](README.md) | **English** | [繁體中文](README_zh-Hant.md) | [日本語](README_ja.md) | [![Crowdin](https://badges.crowdin.net/cyanstars/localized.svg)](https://crowdin.com/project/cyanstars)

# CyanStars

> Music is the universal language of the cosmos.

CyanStars is a fan-made, non-commercial rhythm game about Chinese Virtual Singers. It's developed by the Infinite Pursuit of Light (IPOL) Development Team.

## Contribute to the Development

If you'd like to contribute to our project, please spend a moment to read through the [Contribution Guidelines](CONTRIBUTING.md), It will be really appreciated!

Please use the specifications outlined in [LICENSE](LICENSE) and [NOTICE](NOTICE) for source code and multimedia files.

If you wish to publish modified game files, please clearly state that "This game has been modified and is not the original IPOL version" to differentiate your work from what is released in this repository.The modified game files must not be registered for copyright in any country/region.

Are you able to participate in the translation of the project and documents?Please assist us on [Crowdin](https://crowdin.com/project/cyanstars) or submit via PR.

## Related Links

Community contributors/included tracks/privacy policy, etc., can be found in the [Associated Documents](https://ipol-studio.github.io/CyanStars_Docs/).

- Please check the documentation webpage [here](https://ipol-studio.github.io/CyanStars_Docs)

Multimedia files are referenced as submodules at the path `Cyan-Stars/Assets/CysMultimediaAssets`. Please refer to the submodule repository documentation for contribution guidelines regarding multimedia files.

- Attachment: [Multimedia Assets Repository](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

Do you have expertise in art/music/charting/programming/translation, etc., and are interested in participating in our internal discussions?Click [here](http://chenluan.mikecrm.com/JeKq3DU) to apply and join us.

Player's communication QQ group: `827556233`

For copyright, security, members and community, or other issues, please feel free to contact us at <chenluan@cyanstars.onaliyun.com>.

## Development Documentation

### Cloning the repository and submodules (if using the git command line)

```
git clone --recurse-submodules https://github.com/IPOL-Studio/CyanStars.git
```

or

```
git clone https://github.com/IPOL-Studio/CyanStars.git
cd CyanStars
git submodule update --init --recursive
```

### Download Unity Editor

若您已安装 Unity Hub，可将此链接复制到浏览器，调起 Unity Hub 内下载：`unityhub://6000.3.0f1/d1870ce95baf`。

或从 [Unity 官网](https://unity3d.com/get-unity/download/archive) 手动下载 `6000.3.0f1` 版本的编辑器。

### 导入项目和依赖包

在 Unity 中选择打开 `Cyan-Stars` 项目目录。

项目依赖于 Unity 包和 Nuget 包，首次启动时将通过 Unity 编辑器自动下载安装 Unity 包，选择 `Ignore` 忽略编译错误后 Nuget For Unity 将自动下载安装 Nuget 包。

重启编辑器并打开 `CyanStarsEntry` 场景。

### Download Other Necessary Resources

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - A third-party Unity package for playing Live2D resources in Unity projects.After agreeing to the terms, download and drag it into the Unity editor for import, then restart the editor.

### Build Installation Package

Select `CyanStars 工具箱` (CyanStars Toolbox) from the menu above the Unity editor for building.
