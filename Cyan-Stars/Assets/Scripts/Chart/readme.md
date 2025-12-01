## 用途

此路径定义了谱包的数据格式，用于以下三个场景：

- 序列化到 .json 谱面文件和反序列化
- 加载到游戏内
- 加载到制谱器内

## 构成

一个谱包路径主要由以下几个部分构成：

- 1 个谱包元数据文件`.json`（详见`ChartPackData.cs`）
  - 此文件不含完整的谱面数据，而是引用了谱面文件相对路径。
  - 此文件将会在选曲界面加载。
- 任意数量（可不存在）的谱面文件`.json`（详见`ChartData.cs`）
  - 包含单个难度的完整的谱面数据。
  - 此文件将会在进入音游时加载。
- 谱包用到的其他资源文件（图片、视频、音效、MMD 等），由谱师在编辑器内导入，编辑器将文件从外部路径复制到谱包路径下，由谱面文件引用。

## 游戏内加载

加载到游戏时，会根据是否为内置谱面，在`ChartPackData`外包装一层（详见`RuntimeChartPack.cs`）

---

## Usage

This document defines the data format of a chart pack, used for the following three scenarios:

- Serializing to and deserializing from `.json` files
- Loading into the game
- Loading into the chart editor

## Structure

A chart pack directory is primarily composed of the following parts:

- One `.json` chart pack metadata file (see `ChartPackData.cs` for details)
  - This file does not contain the complete chart data; instead, it references the chart files via relative paths.
  - This file is loaded on the song selection screen.
- Zero or more `.json` chart files (see `ChartData.cs` for details)
  - Each file contains the complete chart data for a single difficulty.
  - This file is loaded when entering gameplay.
- Other asset files used by the chart pack (e.g., images, videos, sound effects, MMD), which are imported by the charter in the editor. The editor copies these files from their external paths into the chart pack's directory, where they are then referenced by the chart files.

## In-Game Loading

When a chart pack is loaded into the game, the `ChartPackData` is wrapped in an outer layer (see `RuntimeChartPack.cs`), depending on whether it is a built-in chart pack.
