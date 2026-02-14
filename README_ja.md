![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| [简体中文](README.md) | [English](README_en.md) | [繁體中文](README_zh-Hant.md) | **日本語** | [![Crowdin](https://badges.crowdin.net/cyanstars/localized.svg)](https://crowdin.com/project/cyanstars)

# CyanStars

> 音楽は宇宙の共通言語です。

翠蓝星穹（CyanStars）は、无圻寻光开发组（IPOL）によって開発された中国語バーチャルシンガーの同人、非商用のコミュニティ音楽ゲームです。

## 開発に参加する

欢迎参与代码/游戏内容/翻译的贡献，请阅读 [贡献指南](CONTRIBUTING.md)！

源代码与多媒体文件请在 [LICENSE](LICENSE) 中约定的范围内使用。

## 関連リンク

コミュニティ貢献者/収録曲/プライバシーポリシーなどは、[付属文書](https://ipol-studio.github.io/CyanStars_Docs/)を参照してください。

- HIV/：[附属文库](https://github.com/IPOL-Studio/CyanStars_Docs)

マルチメディアファイルは、サブモジュールとして引用され、パスは `Cyan-Stars/Assets/CysMultimediaAssets` です。マルチメディアファイルの貢献ガイドラインについてはサブモジュールのリポジトリの説明を参照してください。

- 附：[マルチメディアファイルのリポジトリ](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

プレイヤー交流 QQ グループ：`827556233`

著作権、安全性、メンバーおよびコミュニティ、またはその他の問題については、お気軽にご連絡ください <chenluan@cyanstars.onaliyun.com>。

## 開発ドキュメント

### リポジトリとサブモジュールをクローンする（git コマンドラインを使用している場合）

```
git clone --recurse-submodules https://github.com/IPOL-Studio/CyanStars.git
```

または

```
git clone https://github.com/IPOL-Studio/CyanStars.git
cd CyanStars
git submodule update --init --recursive
```

### Unity エディタをダウンロード

若您已安装 Unity Hub，可将此链接复制到浏览器，调起 Unity Hub 内下载：`unityhub://6000.3.0f1/d1870ce95baf`。

或从 [Unity 官网](https://unity3d.com/get-unity/download/archive) 手动下载 `6000.3.0f1` 版本的编辑器。

### 导入项目和依赖包

在 Unity 中选择打开 `Cyan-Stars` 项目目录。

项目依赖于 Unity 包和 Nuget 包，首次启动时将通过 Unity 编辑器自动下载安装 Unity 包，选择 `Ignore` 忽略编译错误后 Nuget For Unity 将自动下载安装 Nuget 包。

重启编辑器并打开 `CyanStarsEntry` 场景。

### 他の必要なリソースをダウンロードする

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - UnityプロジェクトでLive2Dリソースを再生するためのサードパーティのUnityパッケージです。合意契約の後、Unity エディタにダウンロードしてドラッグ＆ドロップしてインポートし、その後エディタを再起動してください。

### インストールパッケージを構築する

Unity エディタの上部メニューから `CyanStars 工具箱`（CyanStars ツールボックス） を選択してビルドします。
