![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| [简体中文](README.md) | **English** | [繁體中文](README_zh-Hant.md) | [日本語](README_ja.md) | [![Crowdin](https://badges.crowdin.net/cyanstars/localized.svg)](https://crowdin.com/project/cyanstars)

# CyanStars

> Music is the universal language of the cosmos.

CyanStars is a fan-made, non-commercial rhythm game about Chinese Virtual Singers. It's developed by the Infinite Pursuit of Light (IPOL) Development Team.

## Contribute to the Development

Thank you for participating in the contribution of code/game content/translation. Please read the [Contribution Guide](CONTRIBUTING.md)!

源代码与多媒体文件请在 [LICENSE](LICENSE.md) 中约定的范围内使用。

## Related Links

Community contributors/included tracks/privacy policy, etc., can be found in the [Associated Documents](https://ipol-studio.github.io/CyanStars_Docs/).

- Please check the documentation webpage [here](https://ipol-studio.github.io/CyanStars_Docs)

Multimedia files are referenced as submodules at the path `Cyan-Stars/Assets/CysMultimediaAssets`. Please refer to the submodule repository documentation for contribution guidelines regarding multimedia files.

- Attachment: [Multimedia Assets Repository](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

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

If you have Unity Hub installed, you can copy this link to your browser to launch Unity Hub for download: `unityhub://6000.3.0f1/d1870ce95baf`.

Alternatively, manually download version `6000.3.0f1` of the editor from the [Unity official website](https://unity3d.com/get-unity/download/archive).

### Import projects and dependencies

Select the `Cyan-Stars` project directory in Unity.

The project relies on Unity packages and NuGet packages. Upon first launch, the Unity editor will automatically download and install the Unity packages. After selecting `Ignore` to disregard the compilation errors, NuGet For Unity will automatically download and install the NuGet packages.

Restart the editor and open the `CyanStarsEntry` scene.

### Download Other Necessary Resources

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - A third-party Unity package for playing Live2D resources in Unity projects.After agreeing to the terms, download and drag it into the Unity editor for import, then restart the editor.

### Build Installation Package

Select `CyanStars 工具箱` (CyanStars Toolbox) from the menu above the Unity editor for building.
