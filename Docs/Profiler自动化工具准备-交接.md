# Profiler 自动化工具准备：交接说明

> 交接日期：2026-10-22
>
> 当前状态：源码与静态检查已完成；由于当前机器没有 Unity Editor，所有 Unity 编译、构建、双进程运行和 Profiler 数据验证仍待执行。
>
> 测试基线：1024 / 2048 / 5012 个怪物。

## 1. 本次交付目标

在不绕过项目正式启动与网络链路的前提下，提供 Unity Editor 内一键构建并顺序运行 Host、Client 两个独立 Player 的性能基准工具。每个角色独立采样和输出报告，Editor 汇总结果，并为后续基线比较保留环境元数据。

本次实现没有声称已经得到真实性能结论。当前交付的是“可进入 Unity 环境继续验证的自动化工具源码”。完整需求、实现范围和 20 项待测清单见 `Docs/大致需求.md` 第十七节。

## 2. 主要入口

- Unity 菜单：`Tools/Benchmark/Dual Player Benchmark`
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

推荐首次验证不要直接跑完整三轮。接手者应临时在窗口中把 `Repeat Count` 设为 1，但工具仍会依次执行 1024、2048、5012 三档。若需严格只跑 1024，建议先在本地调试分支暂时缩减 `BenchmarkProcessOrchestrator.Cases`，确认链路后恢复固定三档，避免把调试改动提交进正式分支。

按钮语义：

- `Build & Run Dual Benchmark`：生成 HybridCLR 内容、构建 Addressables、构建 Development Player、写构建指纹并开始测试。
- `Run Existing Build`：只允许运行带有效 `benchmark-build.json` 且 Player SHA-256、Catalog Hash 匹配的已有构建。
- `Select Baseline`：选择含 `SUMMARY.md` 的历史 RunId 目录作为基线。
- `Stop Benchmark`：只终止本轮记录的 PID 且进程启动时间仍匹配的 Host/Client。
- `Open Benchmark Results`：打开结果根目录。

## 4. 正常执行链路

1. Host、Client 都走原 Bootstrap、Addressables、HybridCLR 和 Lobby 加载流程。
2. Editor 先启动 Host；收到 `host-ready` 后再启动 Client。
3. 双方进入两人 Lobby、提交 Profile 并通过现有 Ready RPC、倒计时和服务器权威场景切换进入游戏。
4. Host 使用正式 `MonsterRuntimeService` 权威生成指定数量怪物；Client 必须具备相符的 Replica 和 View 数量。
5. 自动移动通过 `PlayerSyncController.SubmitLocalInput` 注入，不直接修改 Transform。
6. 双端都报告 `workload-ready` 后，Editor 写统一 UTC 采样窗口；采样完成后两端独立写结果并退出。
7. 所有档位与重复完成后生成综合报告及 latest 指针。

Host 的工作负载有效性额外要求：玩家发生实际位移，并且采样期内 `Source FlowField` 重建计数增加。只看到角色移动不算通过。

## 5. 输出目录与关键产物

构建目录和结果目录都已加入 `.gitignore`：

```text
BenchmarkBuild/
BenchmarkResults/
  runs/<RunId>/
    run-metadata.json
    SUMMARY.md
    monsters-<Count>/repeat-<NN>/host/
      player.log
      status.json
      metrics.json
      frames.csv
      markers.csv
      REPORT.md
    monsters-<Count>/repeat-<NN>/client/
      ...同上
  baseline.json
  latest.json
```

结果解读原则：

- Host 与 Client 独立报告，不把两端 CPU、GPU、内存或网络数值相加。
- Counter 不可用时应显示 unavailable 或 `-1`，不得当作 0。
- 同机双进程结果只代表共享硬件、共享暖缓存下的稳态表现。
- `ProfilerRecorder` 结果不能替代完整 Profiler Timeline/Hierarchy 调用树。

## 6. 接手后的验证顺序

1. 使用 Unity 2022.3.44f1c1 打开项目，先处理 C#、asmdef、序列化或 Unity API 兼容错误。
2. 确认 Active Build Target 为 `StandaloneWindows64`，项目中存在 `Performant` Quality。
3. 单独构建 HotFix DLL、AOT 元数据与 Addressables，检查 `ServerData/StandaloneWindows64` 的 Catalog。
4. 先运行一次普通非 Benchmark 的创建房间、加入房间和单人流程，确认连接入口抽取没有造成回归。
5. 打开 Benchmark 窗口，先以 Repeat Count 1 执行；重点观察 1024 档的 Host、Client 日志和状态文件。
6. 验证 Lobby 两人、场景 barrier、怪物数量、Replica/View 数量、移动与 Source FlowField 重建。
7. 核对双端采样开始和结束偏差是否都不超过 50 ms，并验证 JSON、CSV、Markdown 可读。
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
- 完整 Profiler 原始文件尚未实现并验证；不得将占位文件当作 `.raw` 结果。

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

只有在 `Docs/大致需求.md` 第十七节“待逐项运行验证”的 20 项被逐项实际执行并记录结果后，才能把本交付从“工具准备”升级为“已验证”。任何失败或不可用指标都应原样写回需求文档和本交接文档，不用推测值补齐。
