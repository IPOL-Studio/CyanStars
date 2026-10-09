# 文件系统重构设计

## 应用临时数据目录结构

在磁盘空间不足等极端情况下，临时文件可能会在运行时清理，需要在每次读取前检查文件（夹）确实存在。业务逻辑应当自行管理各文件句柄的生命周期。

- Application.temporaryCachePath    # 应用临时数据目录
    - $"GameSession_{GUID}"         # 固定前缀 + 随机 GUID，随游戏进程创建并在退出时删除。每次启动游戏时删除之前残余且未被 .session_lock 的 "GameSession_*"
        - 跨平台文件缓存              # 生命周期同进程
        - 制谱器可撤销文件缓存         # 生命周期为每次进入和退出制谱器期间
    - 预留用于其他目的的临时文件

## FileManager

FileManager 提供文件（夹）选择对话框，并在初始化时启动 PlatformFilePortal；后者在每次游戏会话开始时，清除未被多开占用且残余的应用临时数据。

## PathProvider

继承自 IPathProvider，提供跨平台的常用文件路径（CommonFolders，枚举）转换。

- 下载
- 桌面

## 文件句柄

FileHandle 的可读写路径始终指向会话缓存里的一份副本：加载外部文件（夹）时，先复制到缓存，再创建句柄。

FileHandle 字段：

- public string? 来源路径
- public string 来源文件（夹）名，业务侧用作目标路径命名依据
- public string 文件的可读写路径（缓存副本）
- public string? 保存时目标路径 {get; internal set;}
- public FileHandleState 句柄生命周期状态 {get; internal set;}

## 文件读写业务门户

PlatformFilePortal 将每个处理的文件封装为 FileHandle：

根据编译和动态环境判断，在初始化时持有唯一的一个 PathProvider 实例。

PlatformFilePortal 方法：

- public FileHandle? 加载文件（夹）并创建文件句柄(string pathUri, FileCacheKind cacheKind)  # 内容复制进缓存；加载路径不能位于会话缓存下
- public bool 修改句柄保存位置(FileHandle fileHandle, string? targetPath)                   # 保存路径不能位于会话缓存下，也不能等于或位于可读写路径之内
- public bool 保存文件到句柄指定位置(FileHandle fileHandle, bool overwrite = false)         # 目标为已存在的目录时按替换语义先删除再复制
- public bool 释放句柄并删除缓存副本(FileHandle fileHandle)

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
