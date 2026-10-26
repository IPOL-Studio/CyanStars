# 文件系统

## 平台环境

FileManager 初始化时经 PlatformEnvironmentFactory 创建唯一一个 IPlatformEnvironment 实现，并注入内部的 FileHandleService：

- DiskPlatformEnvironment：编辑器、桌面端、未启用 SAF 的安卓
- AndroidSafPlatformEnvironment：启用 SAF 的安卓，外部存储走 content:// 路径；整目录导入导出暂未开放

业务逻辑需要平台默认路径（持久化数据目录、临时数据目录、常用文件夹）或平台能力（导入导出、本地路径判定、应用目录迁移、自动重启）时，统一经 `GameRoot.File.Environment` 获取，不直接访问 Application 的路径与平台属性。应用自己管理、允许玩家迁移的目录则统一经 `GameRoot.File.GetAppPath` 获取，该接口返回本地绝对路径。

平台环境返回当前平台实际可用的常用文件夹列表，FileManager 将这些文件夹注册到弹窗侧边栏。`IPlatformEnvironment.IsLocalPath` 只表达路径能否直接交给 `System.IO`，SAF 等具体平台机制由对应实现内部处理。

## 应用路径设置

应用自己管理的、允许玩家重新指定的目录使用 `AppPathKind` 标识，目前包括玩家谱包目录和临时文件目录。FileManager 持有的内部 AppPathManager 负责解析配置并维护以下三态，业务侧经 `GameRoot.File` 访问：

- `GetDefaultAppPath(kind)`：平台默认路径
- `GetAppPath(kind)`：本次运行实际使用的路径
- `GetAppPathInfo(kind)`：以上信息、保存的覆盖与回退原因的完整状态

配置保存在默认持久化数据目录下的 `Settings/AppPaths.json`，不会跟随谱包目录或临时目录迁移。当前只在 Windows 上开放修改；其它平台仍使用平台默认路径。

启动时，如果保存的路径不可用（例如移动硬盘未连接、目录没有权限），本次运行会回退到默认路径，并通过 `HasUnavailableAppPaths` / `GetUnavailableAppPaths()` 暴露原因。

修改路径只写配置，不改变本次运行已经打开的路径和文件句柄，需要重启游戏后生效。`GameRoot.File.TrySetAppPath` 返回 `AppPathChangeResult.RequiresRestart`；`ApplicationRestart.Restart(GameRoot.File.Environment)` 用于拉起新进程并退出当前进程。切换目录时不会自动移动旧目录中的内容，需要玩家自行迁移。

设置界面尚未开放，目前配置只能手动编辑 `Settings/AppPaths.json`。

## 会话暂存的设计目的

文件句柄面向需要暂存外部内容的场景，不是普通文件读取的零开销别名。当前主要用于制谱器资源导入：

- 将安卓 SAF 的 `content://` 资源转换为可供 `System.IO` 和 CatAsset 读取的本地路径。
- 保存前不修改来源文件和谱包工作区；取消编辑时释放句柄即可丢弃尚未保存的内容。
- 本次编辑会话使用固定的暂存副本，来源文件后续的移动、删除或内容变化不影响预览。
- 撤销、重做可以在句柄生命周期内继续引用同一副本，无需重新导入来源文件。

`TryStageFile` 对普通本地路径和 `content://` 路径采用相同语义，都会先复制完整文件（夹）。该操作会产生读写时间和磁盘空间开销；暂存副本不是持久存储，业务必须在用完后释放句柄。

## 应用临时数据目录结构

在磁盘空间不足等极端情况下，临时文件可能会在运行时清理，需要在每次读取前检查文件（夹）确实存在。业务逻辑应当自行管理各文件句柄的生命周期。

- 平台环境提供的应用临时数据目录
    - $"GameSession_{GUID}"         # 固定前缀 + 随机 GUID，随游戏进程创建并在退出时删除。每次启动游戏时删除之前残余且未被 .session_lock 的 "GameSession_*"
        - 各缓存作用域子目录        # 作用域标识由业务侧自定义并作为子目录名，框架不枚举业务场景
    - 预留用于其他目的的临时文件

会话目录用于隔离不同游戏实例的暂存内容。`.session_lock` 由持有该目录的进程独占打开，避免启动清理误删仍在运行的其他实例；进程异常退出后，下一次启动可以清理其残留目录。

## FileManager

FileManager 是业务层与外部文件系统之间的边界：持有平台环境，提供文件（夹）选择对话框、普通文本读写、外部文件夹导入导出、应用路径解析与会话暂存句柄；内部的 FileContentService 负责普通文本读取，FileHandleService 负责文件复制与句柄生命周期，并在每次游戏会话开始时清除上次运行残留且未被多开占用的应用临时数据。

文件对话框的过滤器使用框架自有的 FileTypeFilter（显示名 + 后缀），业务侧不引用文件对话框插件的类型。对话框展示与快捷入口由内部的 FileDialogService 负责；已有对话框打开时，打开方法返回 false 并触发 onCancel。

### 应用自有目录

`GameRoot.File.GetAppPath(kind)` 返回应用自有目录，保证为本地绝对路径；业务侧取得路径后可以直接使用 `System.IO` 读写，不需要创建文件句柄。

外部路径可以是普通绝对路径或安卓 `content://` 路径，其中 `content://` 路径不能直接交给 `System.IO`。普通文本内容可通过 `GameRoot.File.TryReadTextFromPath` 读取；需要在外部位置与应用自有目录之间复制文件夹时，使用 `GameRoot.File.TryImportFolder` 或 `GameRoot.File.TryExportFolder`。

导入要求来源位于应用自有目录之外、目标位于其中；导出相反。业务侧可先调用 `GameRoot.File.TryValidateFolderTransfer` 获取失败原因。

## 普通文本读取

`GameRoot.File.TryReadTextFromPath` 用于一次性读取文本，不创建 FileHandle 或会话暂存副本。普通本地路径（包括安卓应用持久化数据目录中的路径）直接读取；`content://` 路径由 Simple File Browser 通过 SAF 处理。

- public bool TryReadTextFromPath(string sourcePath, out string text)    # 读取成功时通过 text 返回内容，失败时 text 为空字符串

仅需读取内容，且不需要固定副本、编辑隔离或延迟保存时，不应调用 `TryStageFile`。需要这些暂存语义时，即使来源是普通本地路径，也应显式创建暂存副本。

## 会话暂存句柄

FileHandle 的可读写路径始终指向会话缓存里的一份副本。暂存外部文件（夹）时，先复制到缓存，再创建句柄，不直接引用来源文件。

FileHandle 字段：

- public string 来源路径
- public string 来源文件（夹）名，业务侧用作目标路径命名依据
- public string 文件的可读写路径（缓存副本）
- public string? 保存时目标路径 {get; internal set;}
- public FileHandleState 句柄生命周期状态 {get; internal set;}

## 会话暂存与句柄操作

只有显式暂存外部文件（夹）时才创建 FileHandle，普通只读加载不经过此流程。暂存与句柄操作统一经 `GameRoot.File` 调用：

- public FileHandle? TryStageFile(string pathUri, string cacheScope)                        # 内容复制进缓存；来源路径不能位于会话缓存下
- public bool 修改句柄保存位置(FileHandle fileHandle, string? targetPath)              # 保存路径不能位于会话缓存下，也不能等于或位于可读写路径之内
- public bool 保存文件到句柄指定位置(FileHandle fileHandle, bool overwrite = false)     # 目标为已存在的目录时按替换语义先删除再复制
- public bool 释放句柄并删除缓存副本(FileHandle fileHandle)
- public void 清理空的缓存作用域(string cacheScope)                                     # 业务侧用完一个缓存作用域后收尾

FileHandleService 是 FileManager 的内部实现，不面向业务逻辑。

## 整目录复制

FolderUtil 是框架内部的整目录复制工具，由 FileManager 与 FileHandleService 使用，不面向业务逻辑。本地路径使用 `System.IO`，SAF 路径由 `FileBrowserHelpers` 处理。

- `UniqueName`：目标已存在时依次追加 (1)、(2) 等后缀
- `ReplaceExisting`：目标已存在时先删除再复制；复制失败时旧内容不会恢复
- `FailIfExists`：目标已存在时拒绝
- 目标等于或位于源之内时拒绝
- 失败时清理未复制完成的目录

## 跨平台外部文件读写流程

### 普通文本读取

- 按需通过 Simple File Browser 选择文件，或直接使用已知路径
- 通过 `GameRoot.File.TryReadTextFromPath` 读取文本，不创建暂存句柄
- 使用 JsonSerializer 或其他反序列化器将文本转换为数据实例

### 暂存资源读取

- 按需通过 Simple File Browser 选择文件（夹）
- 通过 `GameRoot.File.TryStageFile` 创建会话副本和句柄
- 通过 `System.IO` 或 CatAsset 读取句柄的 `ReadablePath`

### 文件写入

- 内存序列化（JsonSerializer 或其他序列化器），在内存中尝试构建和校验对应文件
- Simple File Browser 路径选择器
- FileManager 写入
