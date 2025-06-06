![20240905161248130-b9c1d9d9-903d-4473-baa1-223d600c3b88](https://github.com/user-attachments/assets/263705de-58e3-41be-b792-75218b15d8a1)

---

\| [简体中文](README.md) | [English](README_en.md) | [繁體中文](README_zh-Hant.md) | **日本語** | [![Crowdin](https://badges.crowdin.net/cyanstars/localized.svg)](https://crowdin.com/project/cyanstars)

# CyanStars

> 音楽は宇宙の共通言語です。

翠蓝星穹（CyanStars）は、无圻寻光开发组（IPOL）によって開発された中国語バーチャルシンガーの同人、非商用のコミュニティ音楽ゲームです。

## 開発に参加する

最初の開発を始める前に、ぜひ少しの時間を使って [貢献ガイド](CONTRIBUTING.md) を読んでください。ありがとうございます！

ソースコードとマルチメディアファイルは、[LICENSE](LICENSE)と[NOTICE](NOTICE)に定められた規範に従って使用してください。

修正されたゲームファイルを公開する場合は、「このゲームは修正されており、IPOLのオリジナルコンテンツではない」といった表現を明確に示し、あなたの作品をこのリポジトリで公開された作品から区別してください。修正されたゲームファイルは、いかなる国または地域でも著作権を登録してはなりません。

記事やプロジェクトの翻訳に参加したいですか。[crowdin](https://ja.crowdin.com/project/cyanstars) に参加してください、または PR を提出してください。

## 関連リンク

コミュニティ貢献者/収録曲/プライバシーポリシーなどは、[付属文書](https://ipol-studio.github.io/CyanStars_Docs/)を参照してください。

- HIV/：[附属文库](https://github.com/IPOL-Studio/CyanStars_Docs)

マルチメディアファイルは、サブモジュールとして引用され、パスは `Cyan-Stars/Assets/CysMultimediaAssets` です。マルチメディアファイルの貢献ガイドラインについてはサブモジュールのリポジトリの説明を参照してください。

- 附：[マルチメディアファイルのリポジトリ](https://github.com/IPOL-Studio/CyanStarsMultimediaAssets)

美術、音楽、譜面、プログラム、翻訳などの分野で特技があり、私たちの内部ディスカッションに参加することに興味があります。ようこそ、[こちら](http://chenluan.mikecrm.com/JeKq3DU)をクリックして参加申請を行きます。

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

Unity Hubがインストールされている場合は、以下のリンクをブラウザにコピーして、Unity Hub内でダウンロードを開いてください：`unityhub://2020.3.28f1/f5400f52e03f`。

または、[Unity 公式サイト](https://unity3d.com/get-unity/download/archive)から`2020.3.28f1`バージョンのエディターを手動でダウンロードします。

### 他の必要なリソースをダウンロードする

[Live2D Cubism SDK for Unity](https://www.live2d.com/zh-CHS/sdk/download/unity/) - UnityプロジェクトでLive2Dリソースを再生するためのサードパーティのUnityパッケージです。合意契約の後、Unity エディタにダウンロードしてドラッグ＆ドロップしてインポートし、その後エディタを再起動してください。

### インストールパッケージを構築する

Unity エディタの上部メニューから `CyanStars 工具箱`（CyanStars ツールボックス） を選択してビルドします。
