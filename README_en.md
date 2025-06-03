![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| [简体中文](README.md) | **English** |

# CyanStars

> Music, the universal language of the cosmos.

CyanStars is a Chinese virtual singer fan-made, non-commercial, community music game developed by the Infinite Pursuit of Light Development Team (IPOL).

## Contribute to Development

Before you start contributing, please take a moment to read the [Contribution Guidelines](CONTRIBUTING.md), thank you very much!

Please use the specifications outlined in [LICENSE](LICENSE) and [NOTICE](NOTICE) for source code and multimedia files.

If you wish to publish modified game files, please clearly state expressions like "This game has been modified and is not the original IPOL version" to differentiate your work from what is released in this repository. Modified game files must not be copyrighted in any country/region.修改后的游戏文件不得在任何国家/地区注册版权。

有能力参与项目与文档的翻译吗？Do you have the ability to contribute to the project and document translation? Please assist us on [Crowdin](https://crowdin.com/project/cyanstars) or submit via PR.

## Related Links

Community contributors/included tracks/privacy policy, etc., can be found in the [Associated Documents](https://ipol-studio.github.io/CyanStars_Docs/).

- Please check the documentation webpage [here](https://ipol-studio.github.io/CyanStars_Docs).

Multimedia files are referenced as submodules at the path `Cyan-Stars/Assets/CysMultimediaAssets`. Please refer to the submodule repository documentation for contribution guidelines regarding multimedia files.

- Attachment: [Multimedia Assets Repository](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

Do you have expertise in art/music/charting/programming/translation, etc., and are interested in participating in our internal discussions? Feel free to apply to join us [here](http://chenluan.mikecrm.com/JeKq3DU).欢迎点 [这里](http://chenluan.mikecrm.com/JeKq3DU) 来申请加入我们。

玩家交流 QQ 群：`827556233`

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

If you have Unity Hub installed, you can copy this link to your browser to launch Unity Hub for download: `unityhub://2020.3.28f1/f5400f52e03f`.

Alternatively, manually download version `2020.3.28f1` of the editor from the [Unity official website](https://unity3d.com/get-unity/download/archive).

### Download Other Necessary Resources

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - A third-party Unity package for playing Live2D resources in Unity projects. After agreeing to the license, download and drag it into the Unity editor for import, then restart the editor.在同意协议后、下载并拖动到 Unity 编辑器内导入，然后重启一次编辑器。

### Build Installation Package

Select `CyanStars 工具箱` (CyanStars Toolbox) from the menu above the Unity editor for building.
