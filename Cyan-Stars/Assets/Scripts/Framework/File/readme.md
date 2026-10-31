# 文件系统

FileManager 是业务层与外部文件系统之间的边界。业务侧统一经 `GameRoot.File` 调用，不直接访问 `Application` 的路径与平台属性。

## 应用层调用

### 平台环境

需要平台默认路径（持久化数据目录、临时数据目录、常用文件夹）或平台能力（导入导出、本地路径判定、应用目录迁移、自动重启）时，经 `GameRoot.File.Environment` 获取。`IPlatformEnvironment.IsLocalPath` 只表达路径能否直接交给 `System.IO`，不代表具体的平台机制。

### 应用自有目录

应用自己管理、允许玩家重新指定的目录使用 `AppPathKind` 标识，目前包括玩家谱包目录和临时文件目录。`GameRoot.File.GetAppPath(kind)` 返回本次运行实际使用的本地绝对路径，取得后可直接使用 `System.IO` 读写，不需要创建文件句柄。

外部路径可以是普通绝对路径或安卓 `content://` 路径，其中 `content://` 路径不能直接交给 `System.IO`。

### 存在性检查

以下静态方法同时支持普通路径和 `content://` 路径：

- `public static bool IsFileExists(string path)` / `public static bool IsFolderExists(string path)`
- `public static string GetFileOrFolderName(string path)`：取文件（夹）名

### 文件（夹）对话框

- `TryOpenLoadFilePathBrowser`：选择文件。过滤器使用框架自有的 FileTypeFilter（显示名 + 后缀），业务侧不引用文件对话框插件的类型
- `TryOpenLoadFolderPathBrowser`：选择文件夹
- `TryOpenSaveFolderPathBrowser`：选择保存位置

已有对话框打开时，打开方法返回 false 并触发 onCancel。

### 文本读写

- `public bool TryReadTextFromPath(string sourcePath, out string text)`：一次性读取文本，不创建暂存句柄或会话副本；读取成功时通过 text 返回内容，失败时 text 为空字符串
- `public bool TryWriteTextToPath(string targetPath, string text)`：写入文本；目标路径不能位于会话缓存内

仅需读取内容，且不需要固定副本、编辑隔离或延迟保存时，不应调用 `TryStageFile`。需要这些暂存语义时，即使来源是普通本地路径，也应显式创建暂存副本。

### 文件夹导入导出

导入要求来源位于应用自有目录之外、目标位于其中；导出相反。目标已存在时追加序号后缀。

- `TryImportFolder`：将外部文件夹复制到应用自有目录
- `TryExportFolder`：将应用自有目录的文件夹复制到外部位置
- `TryValidateFolderTransfer`：预先校验路径组合并获取失败原因

### 会话暂存句柄

文件句柄面向需要暂存外部内容的场景，不是普通文件读取的零开销别名。当前主要用于制谱器资源导入：

- 将安卓 SAF 的 `content://` 资源转换为可供 `System.IO` 和 CatAsset 读取的本地路径
- 保存前不修改来源文件和谱包工作区；取消编辑时释放句柄即可丢弃尚未保存的内容
- 本次编辑会话使用固定的暂存副本，来源文件后续的移动、删除或内容变化不影响预览
- 撤销、重做可以在句柄生命周期内继续引用同一副本，无需重新导入来源文件

`TryStageFile` 对普通本地路径和 `content://` 路径采用相同语义，都会先复制完整文件（夹），会产生读写时间和磁盘空间开销。暂存副本不是持久存储，业务必须在用完后释放句柄，并自行管理各句柄的生命周期。在磁盘空间不足等极端情况下，临时文件可能在运行时被清理，每次读取前应检查文件（夹）确实存在，或经 `IsHandleReadable` 判断。

FileHandle 字段与属性：

- `public readonly string SourcePath`：来源路径
- `public readonly string EntryName`：来源文件（夹）名，业务侧用作目标路径命名依据
- `public readonly string ReadablePath`：文件（夹）的可读写路径，指向缓存副本
- `public string? TargetPath { get; private set; }`：保存时的目标路径
- `public FileHandleState State { get; private set; }`：句柄生命周期状态

句柄操作：

- `public FileHandle? TryStageFile(string pathUri, string cacheScope)`：内容复制进缓存；来源路径不能位于会话缓存下
- `public bool TrySetSaveTarget(FileHandle fileHandle, string? targetPath)`：设置或解绑保存路径；保存路径不能位于会话缓存下，也不能等于或位于可读写路径之内
- `public bool IsHandleReadable(FileHandle? fileHandle)`：句柄已释放或缓存副本被清理时返回 false
- `public bool TrySaveToTarget(FileHandle fileHandle, bool overwrite = false)`：保存到句柄指定位置；`overwrite` 为 false 时拒绝已存在的目标，为 true 时允许覆盖文件，目录目标先删除再复制
- `public bool TrySaveAll(IEnumerable<FileHandle> fileHandles, bool overwrite = false)`：批量保存；任一项失败不中断其余项，调用方自行决定失败后是否再写引用这些资源的元数据文件
- `public bool TryReleaseFile(FileHandle fileHandle)` / `public int TryReleaseAll(IEnumerable<FileHandle> fileHandles)`：释放句柄并删除缓存副本
- `public void ClearCacheScopeIfEmpty(string cacheScope)`：缓存作用域已为空时删除其目录，供业务侧收尾

### 路径设置

配置保存在默认持久化数据目录下的 `Settings/AppPaths.json`，不跟随谱包目录或临时目录迁移。当前只在 Windows 上读取路径覆盖，其它平台仍使用平台默认路径。

设置界面和运行时修改接口尚未开放，目前只能手动编辑 `Settings/AppPaths.json`，修改在下次启动时生效。切换目录不会自动移动旧目录中的内容，需要玩家自行迁移。支持自动重启的平台可调用 `ApplicationRestart.Restart(GameRoot.File.Environment)`；编辑器下调用该方法只会退出播放模式。

启动时保存的路径不可用（例如移动硬盘未连接、目录没有权限）时，本次运行回退到默认路径并记录警告日志；回退状态查询接口尚未公开。

### 典型流程

普通文本读取：按需通过文件对话框选择文件或直接使用已知路径 → `TryReadTextFromPath` 读取 → 使用 JsonSerializer 或其他反序列化器转换为数据实例。

暂存资源读取：按需通过文件对话框选择文件（夹） → `TryStageFile` 创建会话副本和句柄 → 经 `System.IO` 或 CatAsset 读取句柄的 `ReadablePath`。

文件写入：内存中序列化并尝试构建和校验对应文件 → 文件对话框选择保存位置 → `TryWriteTextToPath` 写入，或经句柄保存到目标路径。

## 内部实现

以下类型均为框架内部实现，不面向业务逻辑。

### 组件结构

FileManager 初始化时经 PlatformEnvironmentFactory 创建唯一一个 IPlatformEnvironment 实现，并持有以下内部服务：

- AppPathManager：解析应用路径配置，维护平台默认路径、保存的覆盖、本次生效路径与回退原因
- FileHandleService：文件复制与会话暂存句柄生命周期；每次游戏会话开始时清除上次运行残留且未被多开占用的应用临时数据，退出时关闭服务并删除本次会话的临时目录
- FileDialogService：对话框展示与快捷入口；平台环境返回的当前平台可用常用文件夹由 FileManager 注册到弹窗侧边栏
- FileContentService：普通文本读取
- FolderUtil 与 FolderTransferPolicy：整目录复制与导入导出路径校验

### 平台环境

- DiskPlatformEnvironment：编辑器、桌面端、未启用 SAF 的安卓
- AndroidSafPlatformEnvironment：启用 SAF 的安卓，外部存储走 `content://` 路径；整目录导入导出暂未开放

SAF 等具体平台机制由对应实现内部处理：本地路径使用 `System.IO`，SAF 路径由 Simple File Browser 的 `FileBrowserHelpers` 处理。

### 会话临时目录

- 平台环境提供的应用临时数据目录
    - `GameSession_{GUID}`：固定前缀加随机 GUID，随游戏进程创建并在退出时删除；每次启动删除之前残余且未被 `.session_lock` 占用的 `GameSession_*`
        - 各缓存作用域子目录：作用域标识由业务侧自定义并作为子目录名，框架不枚举业务场景
    - 预留用于其他目的的临时文件

会话目录用于隔离不同游戏实例的暂存内容。目录名在创建 AppPathManager 与 FileHandleService 前生成并共享：AppPathManager 校验路径时排除它，FileHandleService 实际占用它，临时目录回退后两边仍指向同一目录。`.session_lock` 由持有该目录的进程独占打开，用于避免启动清理误删仍在运行的其他实例；进程异常退出后，下一次启动可以清理其残留目录。当前实现仍存在启动竞态：一个实例已创建会话目录但尚未获取锁时，另一个实例可能将该目录误判为残留并清理，因此多实例同时启动尚不完全安全。

自定义临时目录初始化失败时，FileManager 回退到平台默认目录重建 FileHandleService，并记录回退原因。

### 整目录复制

FolderUtil 由 FileManager 与 FileHandleService 使用，复制模式：

- UniqueName：目标已存在时依次追加 (1)、(2) 等后缀
- ReplaceExisting：目标已存在时先删除再复制；复制失败时旧内容不会恢复；目标包含源目录时拒绝
- FailIfExists：目标已存在时拒绝

目标等于或位于源之内时一律拒绝；失败时清理未复制完成的目录。
