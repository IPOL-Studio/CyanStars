# 文件系统重构设计

## 平台环境

FileManager 在初始化时按编译和运行环境创建唯一一个 IPlatformEnvironment 实现，并注入 FilePortal：

- DiskPlatformEnvironment：编辑器、桌面端、未启用 SAF 的安卓
- AndroidSafPlatformEnvironment：启用 SAF 的安卓，外部存储走 content:// 路径

业务逻辑需要平台相关的路径（持久化数据目录、临时数据目录）或能力（能否导入导出外部文件夹、路径是否必须走 SAF）时，统一经 `GameRoot.File.Environment` 获取，不直接访问 Application 的路径与平台属性。

## 应用临时数据目录结构

在磁盘空间不足等极端情况下，临时文件可能会在运行时清理，需要在每次读取前检查文件（夹）确实存在。业务逻辑应当自行管理各文件句柄的生命周期。

- 平台环境提供的应用临时数据目录
    - $"GameSession_{GUID}"         # 固定前缀 + 随机 GUID，随游戏进程创建并在退出时删除。每次启动游戏时删除之前残余且未被 .session_lock 的 "GameSession_*"
        - 各缓存作用域子目录        # 作用域标识由业务侧自定义并作为子目录名，框架不枚举业务场景
    - 预留用于其他目的的临时文件

## FileManager

FileManager 持有平台环境与 FilePortal，提供文件（夹）选择对话框；Portal 在每次游戏会话开始时，清除未被多开占用且残余的应用临时数据。

文件对话框的过滤器使用框架自有的 FileTypeFilter（显示名 + 后缀），业务侧不引用文件对话框插件的类型。

## 文件句柄

FileHandle 的可读写路径始终指向会话缓存里的一份副本：加载外部文件（夹）时，先复制到缓存，再创建句柄。

FileHandle 字段：

- public string? 来源路径
- public string 来源文件（夹）名，业务侧用作目标路径命名依据
- public string 文件的可读写路径（缓存副本）
- public string? 保存时目标路径 {get; internal set;}
- public FileHandleState 句柄生命周期状态 {get; internal set;}

## 文件读写业务门户

FilePortal 将每个处理的文件封装为 FileHandle。

FilePortal 方法：

- public FileHandle? 加载文件（夹）并创建文件句柄(string pathUri, string cacheScope)  # 内容复制进缓存；加载路径不能位于会话缓存下
- public bool 修改句柄保存位置(FileHandle fileHandle, string? targetPath)              # 保存路径不能位于会话缓存下，也不能等于或位于可读写路径之内
- public bool 保存文件到句柄指定位置(FileHandle fileHandle, bool overwrite = false)     # 目标为已存在的目录时按替换语义先删除再复制
- public bool 释放句柄并删除缓存副本(FileHandle fileHandle)
- public void 清理空的缓存作用域(string cacheScope)                                     # 业务侧用完一个缓存作用域后收尾

## 整目录复制

FolderUtil 提供不走句柄的整目录复制，仅支持普通本地路径。

- public bool 复制文件夹到目标位置(string sourceFolderPath, string targetFolderPath, out string copiedFolderPath)
  # 目标已存在时依次追加 (1)、(2) 等后缀；目标等于或位于源之内时拒绝；失败时清理未复制完成的目录

## 跨平台外部文件读写流程

### 文件读取

- Simple File Browser 路径选择器
- FilePortal 读取
- CatAsset 反序列化文件到实例

### 文件写入

- 内存序列化（JsonSerializer 或其他序列化器），在内存中尝试构建和校验对应文件
- Simple File Browser 路径选择器
- FilePortal 写入
