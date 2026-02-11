# 制谱器程序架构说明

## 术语

- ChartEditor 制谱器
- ChartPack 谱包
- Chart 谱面
- BpmGroup Bpm 组
- SpeedTemplate 变速模板

## 架构

CyanStars 的制谱器采用 MVVM 架构，主要依赖于 R3 和 ObservableCollections 这两个库。

- ChartEditorProcedure（制谱器流程）
    - 负责深拷贝谱包和谱面数据（详见 Assets/Scripts/Chart/readme.md）并传入 Model，防止制谱器内未保存的数据污染内存中已加载的谱面（这些谱面将用于音游流程）。
    - 加载指定的谱包谱面并获取谱包谱面数据、元数据、路径信息。
    - 每次加载新的谱包或谱面都销毁下文的所有实例，并重新启动一边实例化的流程。
- CommandManager（指令管理器）
    - 单例模式，继承自 MonoBehaviour。
    - 用于撤销重做的指令栈管理器，对于有撤销重做需求的操作，需要实例化一个委托交由 CommandManager 执行。
- MvvmBindManager
    - 单例模式，继承自 MonoBehaviour。
    - 将制谱器流程传入的谱包和谱面数据传递给 Model 实例化。
    - 实例化静态的 M、VM、CommandManager，并建立 M==VM==V 的绑定关系。
    - 管理静态 M 和 VM 的生命周期，在 OnDestroy() 时通知所有 VM 解绑订阅。
- Model（M）
    - 单例模式，在内存中存储经过验证后的合法数据，需要手动序列化到磁盘。
    - 实例化时将谱包和谱面实例转换为制谱器内使用的可观察谱包和可观察谱面实例。
    - 可观察实例的字段在引用变化时（如果是值类型，则为变化时）会通知观察（订阅）对象。字段内部嵌套的字段变化时，父字段不会通知。
    - 可观察列表和字典的增删改由 ObservableCollections 通知，集合元素内部嵌套的字段变化时不会触发集合通知。
- ViewModel（VM）
    - 从 M 中选择性订阅和转换所需的数据，并以只读可观察的方式公开。
    - 提供供 V 调用的方法，调用后验证数据合法性，不合法时通知 M 强制刷新并舍弃数据，合法时通过 CommandManager 或直接修改 M 中对应数据。
    - 对于有动态生成 VM 和 V 的场合，父 VM 持有一个列表用于管理何时增删，并负责在实例化时实时绑定。
    - 在绑定时需要通过 .AddTo(base.Disposables) 绑定生命周期以防止内存泄漏，由 MvvmBindManager 自动调用。
    - 对于动态生成的 ViewModel：
        - 在动态实例化时由父 VM 或对应的 V 绑定。
        - ObservableCollections 会自动在销毁子 VM 时调用其 Dispose()，只要完成绑定即可，无需手动调用。
- View（V）
    - 继承自 MonoBehaviour。
    - 从对应的 VM 中订阅和转换数据，并将其更新到 Unity 组件上。
    - 接受 Unity 组件触发的事件回调，并传递给 VM 方法。
    - 在绑定时需要通过 .AddTo(this)，在物体被 Unity 销毁时自动释放内存。
    - 对于动态生成的 View：
      - 在动态实例化时由父 VM 或对应的 V 绑定。
      - 依旧是 .AddTo(this)，在物体被 Unity 销毁时自动释放内存。
