# Profiler 自动化工具准备：交接说明

> 交接日期：2026-10-24
>
> 当前状态：Unity 编译、构建、三档单轮、三档各 3 次、基线比较、Stop、焦点检测与双前台轮均已完成；正常非 Benchmark 手动 UI 回归仍待执行。
>
> 测试基线：1024 / 2048 / 5012 个怪物。

## 1. 本次交付目标

在不绕过项目正式启动与网络链路的前提下，提供 Unity Editor 内一键构建并运行 Host、Client 两个独立 Player 的性能基准工具。每次重复分别执行 Host 前台轮与 Client 前台轮；每轮仍运行一对 Player，但只采用当前前台目标角色的性能数据。每个角色独立输出报告，Editor 汇总结果，并为后续基线比较保留环境元数据。

本次实现已得到同机双进程、共享暖缓存条件下的可重复结果，但没有把它冒充双机绝对性能。完整需求、实现范围和逐项结果见 `Docs/大致需求.md` 第十七节。

## 2. 主要入口

- Unity 菜单：`Tools/性能基准/双端性能测试`
- Editor 编排入口：`Assets/Editor/Benchmark/BenchmarkWindow.cs`
- AOT 参数与文件协议：
  - `Assets/_AOT/Scripts/Bootstrap/BenchmarkLaunchOptions.cs`
  - `Assets/_AOT/Scripts/Bootstrap/BenchmarkFileProtocol.cs`
- HotFix 运行协调与采集：
  - `Assets/_HotUpdate/Scripts/Entry/BenchmarkRuntimeCoordinator.cs`
  - `Assets/_HotUpdate/Scripts/Entry/BenchmarkRuntimeRecorder.cs`
- 自动移动：`Assets/_HotUpdate/Scripts/GameProcess/BenchmarkMovementDriver.cs`
- Lobby 共用连接入口：`Assets/_HotUpdate/Scripts/Network_Lobby/LobbyConnectionService.cs`

## 3. 默认参数与执行方式

窗口默认参数：

```text
MonsterCounts = 1024, 2048, 5012
RandomSeed = 12345
RepeatCount = 3
WarmupSeconds = 5
CaptureSeconds = 30
StageTimeoutSeconds = 180
Resolution = 1024x768
Quality = Performant
VSync = 0
TargetFrameRate = -1
```

推荐首次验证不要直接跑完整三轮。接手者应临时在窗口中把“每档重复次数”设为 1，但工具仍会依次执行 1024、2048、5012 三档。若需严格只跑 1024，建议先在本地调试分支暂时缩减 `BenchmarkProcessOrchestrator.Cases`，确认链路后恢复固定三档，避免把调试改动提交进正式分支。

按钮语义：

- `构建并运行双端基准测试`：生成 HybridCLR 内容、构建 Addressables、构建 Development Player、写构建指纹并开始测试。
- `使用现有构建运行`：只允许运行带有效 `benchmark-build.json` 且 Player SHA-256、Catalog Hash 匹配的已有构建。
- `选择历史基线`：选择含 `SUMMARY.md` 的历史 RunId 目录作为基线。
- `停止当前基准测试`：只终止本轮记录的 PID 且进程启动时间仍匹配的 Host/Client。
- `打开测试结果目录`：打开结果根目录。

## 4. 正常执行链路

1. Host、Client 都走原 Bootstrap、Addressables、HybridCLR 和 Lobby 加载流程。
2. Editor 先启动 Host；收到 `host-ready` 后再启动 Client。
3. 双方进入两人 Lobby、提交 Profile 并通过现有 Ready RPC、倒计时和服务器权威场景切换进入游戏。
4. Host 使用正式 `MonsterRuntimeService` 权威生成指定数量怪物；Client 必须具备相符的 Replica 和 View 数量。
5. 自动移动通过 `PlayerSyncController.SubmitLocalInput` 注入，不直接修改 Transform。
6. 每次重复先执行 Host 前台轮，再执行 Client 前台轮；目标 Player 自主申请 Windows 前台，Editor 通过前台窗口 PID 确认成功后才允许双端报告 `workload-ready`。
7. 双端都报告 `workload-ready` 后，Editor 写统一 UTC 采样窗口；采样完成后两端独立写结果并退出。
8. 所有档位与重复完成后生成中文 `SUMMARY.md` 综合报告及 latest 指针。

Host 的工作负载有效性额外要求：玩家发生实际位移，并且采样期内 `Source FlowField` 重建计数增加。只看到角色移动不算通过。

## 5. 输出目录与关键产物

构建目录和结果目录都已加入 `.gitignore`：

```text
BenchmarkBuild/
BenchmarkResults/
  runs/<RunId>/
    run-metadata.json
    SUMMARY.md
    monsters-<Count>/repeat-<NN>/focus-host/host/
      player.log / status.json / metrics.json / frames.csv / markers.csv / REPORT.md / profiler.raw（可选）
    monsters-<Count>/repeat-<NN>/focus-host/client/
      ...同上
    monsters-<Count>/repeat-<NN>/focus-client/<host|client>/
      ...同上
  baseline.json
  latest.json
```

结果解读原则：

- Host 与 Client 独立报告，不把两端 CPU、GPU、内存或网络数值相加。
- Counter 不可用时应显示 unavailable 或 `-1`，不得当作 0。
- 同机双进程结果只代表共享硬件、共享暖缓存下的稳态表现。
- `ProfilerRecorder` 结果不能替代完整 Profiler Timeline/Hierarchy 调用树。
- 报告中的 `Focused` 来自 Windows 前台窗口所属 PID；焦点条件不同的两轮不会输出性能差值。
- `Pass` 与 `Role` 相同的行才是性能样本；另一角色是 support role，仅用于证明网络和工作负载正常。
- 被测角色前台帧占比低于 95% 时该轮无效；每次重复实际会启动两对 Player，总时长约为旧单轮模式的两倍。

已验证结果：

- 三档各 3 次：`BenchmarkResults/runs/20261022-133947-3cca93ce`。
- 带 Windows 前台证据的基线：`BenchmarkResults/runs/20261023-130440-72eeba90`。
- 相同 0% 前台条件下的比较轮：`BenchmarkResults/runs/20261023-131133-f3e8ba16`。
- 比较轮 1024/2048 的平均帧时波动在约 ±4% 内；5012 Host 平均值下降 8.3%，但 P99 上升 11.8%，仍需完整 Profiler 调用树定位长帧。
- 纯自动化比较轮中 Host、Client 的 Windows 前台占比均为 0%，因此当前数值只代表同机后台双进程条件。
- 双前台轮验证：`BenchmarkResults/runs/20261024-035109-16b5cb39`。三档 12/12 角色结果有效，6 个被测角色前台占比均为 100%，双端采样开始/结束偏差均小于 50ms。
- 完整 Profiler 三档验证：`BenchmarkResults/runs/20261025-064407-c376a952`。三档各 3 次 Host/Client 前台采样全部有效，18/18 份 `profiler.raw`、36/36 份角色指标完整落盘；raw 合计 6.16 GiB，最大和最小样本均已由 Unity 实际加载验证。
- 本轮前台占比最低 99.98%，双端最大停止偏差 59.1ms；正式对齐容差调整为 100ms（30 秒窗口的 0.33%），避免在 5012 档把约两帧的跨进程停止边界误判为数据缺失。
- 每个 Player 采样完成后的销毁阶段固定出现 17 条 `NetworkPrefabRegistry.ReleaseAll` / `TMP_SubMeshUI.OnDisable` 空引用；它们发生在 `completed` 之后，不影响本轮指标，但应作为独立退出清理缺陷处理。

1024 怪 Host 手动 Profiler 对照（2026-10-24）：

- 单 Player Host 取 299 个稳定帧，双端自动化 Host 取 300 个稳定帧；两边均为每帧 4 次 `Monster.VAT.Upload`，权威 Tick 分别为 28 次和 29 次，压力口径一致。
- 双端自动化相对单 Player 的平均帧时为 3.226 / 3.148 ms（+2.5%），P50 为 2.669 / 2.548 ms（+4.7%），P95 为 8.156 / 7.904 ms（+3.2%），P99 为 9.195 / 9.098 ms（+1.1%）。
- 主要增量来自 `WaitForJobGroupID` 的 0.272 / 0.192 ms（+0.080 ms）以及权威 Tick 约 5%～7% 的小幅变慢；VAT 表现和 Render Thread 没有回退。
- 结论：同机双进程自动化会让 Host 稍慢，但当前 1024 怪样本的整体偏差约为 1%～5%，可作为单 Player 稳态表现的近似值；不能把这部分共享硬件与调度开销解释为业务代码回退。

## 6. 接手后的验证顺序

1. 使用 Unity 2022.3.44f1c1 打开项目，先处理 C#、asmdef、序列化或 Unity API 兼容错误。
2. 确认 Active Build Target 为 `StandaloneWindows64`，项目中存在 `Performant` Quality。
3. 单独构建 HotFix DLL、AOT 元数据与 Addressables，检查 `ServerData/StandaloneWindows64` 的 Catalog。
4. 先运行一次普通非 Benchmark 的创建房间、加入房间和单人流程，确认连接入口抽取没有造成回归。
5. 打开 Benchmark 窗口，先以“每档重复次数 = 1”执行；重点观察 1024 档的 Host、Client 日志和状态文件。
6. 验证 Lobby 两人、场景 barrier、怪物数量、Replica/View 数量、移动与 Source FlowField 重建。
7. 核对双端采样开始和结束偏差是否都不超过 100 ms，并验证 JSON、CSV、Markdown 可读。
8. 依次完成 2048、5012，再运行三档各 3 次。
9. 选取完整有效且环境一致的一轮作为基线，再验证回归比较。
10. 最后测试 Stop、超时、异常退出、连接失败、工作负载无效和可选完整 `.raw` Profiler 文件。

任一步失败时保留完整 `BenchmarkResults/runs/<RunId>`，不要只截取 Console；修复后从该步重新验证。

## 7. 首轮重点排查点

- `HotFix.Entry.asmdef` 新增引用是否在 Unity 中能正确解析，且不存在 Editor/Runtime 程序集边界错误。
- `ProfilerRecorder` 的 Main Thread、Render Thread、GPU Frame Time、GC Alloc 等名称在 Unity 2022.3 Player 中是否可用。
- 两个 Player 共用 `persistentDataPath` 与 Addressables 缓存时是否存在并发写冲突。
- `Build & Run` 会更新现有 `ServerData/StandaloneWindows64`；发布前应决定是否新增独立 Benchmark Addressables Profile。
- Host 是服务器加本地客户端，不是 Dedicated Server；不要把结果标注为专用服务器性能。
- 当前只统计应用层 Named Message Payload，不包含 UTP Header、RTT、丢包和重传。
- 工具提供“保存完整 Profiler 原始数据”可选项，只在被测前台角色的正式采样窗口写 `profiler.raw`；该模式有额外开销，采样模式与普通轻量基线隔离。

## 8. 已完成的静态检查

- 19 个新增或修改的 C# 文件通过 Roslyn 语法树解析。
- 新增及修改的 asmdef JSON 可解析，本地程序集依赖图未发现循环。
- 新增 Unity `.meta` GUID 无重复。
- `git diff --check` 通过。
- 主要失败路径、输出隔离和 Host/Client 状态机已完成源码逻辑检查。

这些检查只能证明文本、语法结构和主要逻辑关系，没有替代 Unity 编译与运行验证。

## 9. 版本库注意事项

- `ProjectSettings/XRSettings.asset` 是本轮开始前已存在的工作区改动，本次 Profiler 自动化交付不包含该文件。
- 提交时应只纳入 Benchmark 相关源码、对应 `.meta`、`.gitignore`、`Docs/大致需求.md` 和本交接文档。
- 不要提交 `BenchmarkBuild/`、`BenchmarkResults/`、Player 日志或未来生成的采样数据。

## 10. 完成交接的判定

完整 `.raw` 自动采集现已通过三档各 3 次验证；Render Thread 仍按不可用处理。正常非 Benchmark UI 的单人、创建房间和加入房间流程仍需人工回归，不应由 Benchmark 主链路替代。任何失败或不可用指标继续原样记录，不用推测值补齐。
