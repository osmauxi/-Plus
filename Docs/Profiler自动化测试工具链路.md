# 任务：为当前 Unity 项目实现完整的自动化性能压测与诊断工具链

## 一、项目背景与核心目标

当前项目正在进行怪物系统的性能优化与重构，后续还会继续重构子弹系统。

目前的手动性能测试流程大致为：

1. 修改代码后重新打包。
2. 启动游戏，正常进入大厅。
3. 加载 Addressables 资源、配置表和 HybridCLR 热更新 DLL，完成必要的 AOT 元数据处理及网络序列化注册。
4. 通过现有服务器权威场景流程加载 `GameRunTimeScene + UIGameUIScene`。
5. 生成指定数量的怪物。
6. 手动控制玩家在地图中移动，以提高寻路目标变化频率和流场重建频次。
7. 打开 Profiler，手动采样并停止。
8. 保存 Profiler 数据，整理关键指标。
9. 将数据交给 Codex，分析瓶颈、修改代码，然后重复测试。

这套流程重复性很高，尤其是在怪物数量较大时，手动操作与分析数据会占用大量时间。

此外，目前的压测主要集中在 Host 端，缺少独立远端 Client 的性能数据。后续需要同时测量权威模拟、网络同步、客户端状态处理和渲染开销。

因此，请实现一套可复用的自动化性能测试工具链。

**最终目标：在 Unity Editor 中点击一次按钮，自动构建一次 Player，启动 Host 和 Client 两个真实游戏进程，自动完成正常启动与联网流程，执行固定参数的压测，分别采集两端性能数据，最终输出 Host 报告、Client 报告和综合对比报告。**

之后修改代码时，可以直接重复运行测试，由 Codex 优先读取整理好的报告分析性能瓶颈，而不需要每次重新检查完整 Profiler 数据。

不要调用外部大模型 API，不要在游戏内实现 AI 分析。所有性能采集、统计、报告生成和历史对比均在本地完成。

## 二、第一步：全面检查当前项目

在编写代码之前，先检查项目中的实际实现，不得根据常见 Unity 项目结构猜测。

### 1. 检查基础环境

确认：

- Unity 版本及目标构建平台。
- 当前 Player 的 Build Settings 或 Build Profiles。
- 当前启动场景及场景加载流程。
- 项目使用的渲染管线、画质配置、分辨率、VSync 和帧率限制。
- 当前项目是否已有自动构建脚本、性能测试工具或相关依赖。
- 当前项目的 AOT、HotUpdate、Editor 等程序集划分与 asmdef 依赖关系。

### 2. 检查正常启动流程

实际追踪以下链路：

- 游戏启动与大厅初始化。
- Addressables Catalog 检查、资源下载和加载。
- Excel 配置表加载及其生成的数据结构。
- HybridCLR DLL 下载、加载和 AOT 元数据补充。
- NGO 网络序列化注册及 NetworkManager 初始化。
- Lobby 中开始游戏的业务入口。
- Lobby 到 `GameRunTimeScene + UIGameUIScene` 的正常场景切换。
- Game Runtime、Game UI 初始化、玩家创建和怪物生成。

自动化必须能够复用这条实际运行链路。

不能为了压测而直接跳过大厅、热更新或配置初始化，也不能通过单独编写一套简化的怪物模拟来代替真实游戏。

可以在正常初始化完成后自动调用原有的业务入口，代替人工点击 UI，但不得破坏正式游戏流程。

### 3. 检查网络架构

实际确认：

- Host 和 Client 分别如何启动。
- Client 如何指定服务器 IP 和端口。
- 当前 NGO 使用的连接、玩家生成流程，以及项目自定义 Addressables 场景 barrier。
- 怪物由哪个进程权威生成与模拟。
- 怪物快照、增量数据、序列化和网络发送的实现。
- 玩家输入如何发送给权威端。
- 玩家移动是否会影响服务器的流场目标及重建。
- 如何判断 Client 已经真正接收并处理怪物状态。

不要假设 Host 与 Client 都走相同的初始化代码，也不要假设任何玩家移动都会触发流场重建。

### 4. 检查已有性能观测

搜索并复用现有 ProfilerMarker、ProfilerRecorder、自定义计时器和统计逻辑。

重点检查：

- FlowFieldBuild。
- Integration。
- Source / Cost。
- FlowFieldUpdate。
- Separation。
- 怪物更新与空间分组。
- 网络快照、序列化、反序列化及发送接收。
- Animator、SkinnedMesh 和其他实际存在的表现层逻辑。

以上是目标指标示例。必须使用项目中的真实 Marker 名称与实际调用路径。

检查结束后，给出简短的实施计划、需要修改的文件以及可能存在的架构限制，然后继续实际实现，不要只停留在分析阶段。

## 三、整体自动化架构

实现以下结构：

### 1. Unity Editor 端的测试入口

提供简洁的 EditorWindow 或菜单项，至少包括：

- **Build & Run Dual Benchmark**：构建一次 Player，自动启动双进程压测。
- **Run Existing Build**：使用已有构建重新测试，避免仅修改测试参数时重复构建。
- **Open Benchmark Results**：打开最新报告。
- **Select Baseline**：指定用于性能比较的历史基线。
- **Stop Benchmark**：停止当前测试，正确关闭测试进程并保留已有结果。

如果现有项目已有构建入口，应优先复用。

正常情况下，只需要点击一次即可启动完整测试。

### 2. 外部测试启动器

可以使用 Editor 内的进程管理代码或必要的外部脚本完成：

- 构建 Player。
- 启动 Host 进程。
- 启动 Client 进程。
- 传入角色、测试 ID 和测试参数。
- 监控连接、进程状态、超时和异常退出。
- 等待两端生成报告。
- 生成最终汇总报告。

第一版优先采用最简单、最稳定、容易维护的实现，不要引入不必要的测试框架。

### 3. 运行时测试控制器

创建独立的 BenchmarkRunner 及必要的测试辅助组件。

测试控制器只在显式启用的 Benchmark 模式下工作，不得影响正常游戏。

测试控制器应支持：

- 读取命令行参数或测试配置。
- 判断当前进程角色。
- 等待游戏初始化完成。
- 协调 Host 与 Client。
- 执行怪物生成和玩家移动测试。
- 控制预热与正式采样。
- 保存数据并通知测试启动器。

必须遵守项目现有的 AOT / HotUpdate 架构和程序集依赖规则。不要为了实现工具而重构现有游戏架构。

## 四、双进程 Host + Client 压测

**双进程压测是第一版的核心功能，不能只实现单进程 Host 测试就宣告完成。**

### 1. 启动两个真实 Player

一次构建后，启动两个独立的游戏进程：

- 进程 A：Host。
- 进程 B：Client。

通过命令行参数或等价的可靠机制区分角色。

建议支持类似以下的参数：

```
-benchmark
-benchmark-role=host
-benchmark-run-id=<RunId>
-benchmark-monsters=5012
-benchmark-seed=12345
-benchmark-warmup=5
-benchmark-duration=30
-benchmark-host=127.0.0.1
```

以上仅为参数设计示例，实际参数解析方式根据项目现有代码实现。

Client 进程使用项目现有的连接流程，通过 `127.0.0.1` 连接 Host。

如果项目已经有可靠的 IP 输入或连接参数机制，优先复用，不要额外复制一套网络初始化逻辑。

两个进程必须分别运行自己的正常启动、资源加载、热更新和场景初始化流程。

### 2. 协调测试阶段

设计明确的测试状态机：

```
LaunchHost
    ↓
HostReady
    ↓
LaunchClient
    ↓
ClientConnected
    ↓
BothReady
    ↓
SpawnMonsters
    ↓
Warmup
    ↓
Capture
    ↓
StopCapture
    ↓
WriteReports
    ↓
Shutdown
```

只有在 Host 已经能够接受连接、Client 成功加入、双方完成必要初始化后，才能开始正式压测。

Host 应当等待 Client 成功连接。Client 可以在正常初始化后重试连接，但必须设置超时。

如果 Client 连接失败、热更新失败、怪物数量不匹配或任一进程提前退出，整轮测试必须标记为失败，不能生成看似成功的报告。

建议使用独立的本地 IPC 控制通道或测试专用的协调消息，通知两端开始和结束采样。

IPC 仅负责测试控制，不应替代实际的 NGO 游戏网络通信。

不得为了协调测试而随意修改正式网络协议。如果确实需要增加测试消息，应保证其只在明确启用的测试模式下生效。

### 3. 两端时间对齐

每轮测试生成唯一 RunId。

Host 与 Client 应记录：

- 初始化完成时间。
- 正式采样开始时间。
- 正式采样结束时间。
- 实际有效采样帧数。
- 实际采样时长。
- 关键测试阶段的开始和结束时间。

尽可能让两端在统一测试控制信号下开始和结束采样。

记录时间偏差。只有采样区间和测试条件足够一致时，才进行直接对比。

不能仅凭两个报告都有 30 秒采样，就默认两者的逐帧数据完全对齐。

### 4. Host 权威生成与同步验证

必须调用项目当前真实的服务器权威怪物生成逻辑。

不能让 Client 自行生成一份本地怪物来代替服务器同步。

测试控制器需要确认：

- Host 已经生成指定数量的怪物。
- 怪物初始化和必要的模拟已经开始。
- Client 已成功进入实际游戏场景。
- Client 确实接收到并处理了怪物状态。
- Client 侧实际可见或可确认的怪物数量与预期一致。

如果当前项目使用固定 Slot、死亡掩码或分层同步等设计，应根据实际协议验证有效怪物数量，不能仅通过对象数量判断。

需要在报告中记录 Host 的权威怪物数量、Client 的有效接收数量，以及无法核验的部分。

## 五、自动执行完整游戏流程

双进程测试不能直接跳过 Lobby 和 `GameRunTimeScene + UIGameUIScene` 初始化。

完整流程应当是：

1. 启动 Host Player。
2. Host 正常加载大厅、配置、资源及热更新程序集。
3. Host 正常初始化网络并开始监听。
4. 启动 Client Player。
5. Client 正常加载大厅、配置、资源及热更新程序集。
6. Client 通过 `127.0.0.1` 加入 Host。
7. 使用原有业务入口触发正常的游戏开始流程。
8. 双方按项目现有的 Addressables 场景 barrier 进入 `GameRunTimeScene + UIGameUIScene`。
9. 等待玩家、地图、怪物系统及必要依赖初始化完成。
10. Host 权威生成指定数量的怪物。
11. 等待 Client 接收到有效怪物状态。
12. 执行预设移动路线。
13. 预热并正式采样。
14. 自动保存报告和性能数据。
15. 正确关闭两个测试进程。

如果实际项目存在必须先下载 Addressables 资源或 DLL 的步骤，测试必须真实执行这些步骤。

不要假设资源和程序集一定已经缓存。

应当明确区分：

- 冷启动或缓存未准备状态。
- 资源与热更新缓存已准备状态。
- 正式稳态压测。

当前主要目标是稳态性能对比，但启动流程也可以记录初始化耗时。

### 并发文件与缓存安全

由于两个 Player 在同一台机器上运行，必须检查：

- `Application.persistentDataPath` 是否相同。
- Addressables 缓存是否存在并发更新冲突。
- Catalog、DLL 和其他热更新文件是否被两个进程同时写入。
- 日志和测试结果是否互相覆盖。
- 临时文件和下载文件是否存在共享路径冲突。

不要为了避免冲突而直接绕过热更新流程。

如果必须隔离可写目录，应在确认项目当前实现和资源系统支持后采用合适方案；如果必须串行执行资源更新，也应保留完整初始化链路。

## 六、自动化怪物生成与移动路线

### 1. 固定测试用例

第一版至少提供以下怪物规模：

| 用例   | 怪物数量 | 目的               |
| ------ | -------- | ------------------ |
| Small  | 1024     | 快速回归测试       |
| Medium | 2048     | 中等压力测试       |
| Large  | 5012     | 高负载和长尾帧测试 |

数量可以通过配置修改。

建议初始配置：

```
WarmupSeconds = 5
CaptureSeconds = 30
RandomSeed = 12345
RepeatCount = 3
```

以上是初始建议，实际参数必须可以配置。

一轮批量测试应尽可能只构建一次，然后依次执行多个测试用例。

如果当前项目无法可靠地在两个进程中重置测试场景、清理怪物并恢复相同状态，可以针对不同测试用例重新启动两个 Player 进程，但不必重新构建。

每个用例都必须具有独立的测试记录。

### 2. 自动化玩家移动

当前手动压测需要玩家在地图中到处移动，以提高流场重建频次。

必须将这部分也纳入自动化。

创建独立的 Benchmark Movement Driver，支持：

- 配置固定路线。
- 配置目标点、移动速度和等待时间。
- 循环执行路线。
- 记录实际路线执行情况。
- 记录玩家输入、移动状态和必要的目标位置变化。

优先通过现有的输入处理或玩家移动业务入口驱动玩家，不要直接修改 Transform 来替代正常移动。

如果当前项目的输入系统不方便自动注入，可以增加只在 Benchmark 模式启用的输入提供器，并尽可能复用真实移动逻辑。

必须根据当前项目实际的网络权威机制决定由哪个进程控制玩家。

如果 Client 输入需要经过 RPC 或其他网络流程到达 Host，测试必须保留该流程，不能在 Host 上直接设置玩家目标来替代真实的 Client 输入。

### 3. 验证路线是否真正触发了目标工作负载

移动玩家不一定必然触发流场重建。

必须检查实际寻路目标更新、重建阈值、拓扑缓存和动态障碍更新机制。

每轮测试尽可能记录：

- 流场实际重建次数。
- 每次重建的时间戳。
- 相邻重建之间的间隔。
- 单次重建耗时。
- 动态障碍拓扑缓存的更新次数。
- 玩家路线是否完整执行。
- 怪物数量是否保持稳定。

不能假定移动路线一定产生了预期的重建频率。

如果没有触发预期工作负载，应在报告中明确指出，并将测试标记为无效或需要检查，而不是继续输出可用于优化判断的正常结果。

## 七、性能指标与分端采集

### 1. 整体帧性能

Host 和 Client 分别采集：

- 有效采样帧数。
- 平均帧耗时。
- P50、P90、P95、P99 帧耗时。
- 最大帧耗时。
- 帧耗时分布。
- 明显的长帧与周期性尖峰。
- 平均 FPS，作为辅助指标。
- 能够可靠获取时的主线程、渲染线程和 GPU 耗时。

必须明确区分：

- CPU 实际执行耗时。
- GPU 耗时。
- `Time.unscaledDeltaTime` 等表示的帧间隔。

不能将这些指标混为一谈。

不能仅使用平均 FPS 判断优化是否有效。

P50、P95、P99 必须基于实际有效样本计算，并记录样本数与统计方法。

### 2. GC 与内存

尽可能采集：

- 每帧 GC Alloc。
- 采样期间累计 GC Alloc。
- GC 次数。
- 能可靠获取时的 GC 耗时。
- 托管内存。
- Native 内存。
- 可用的显存指标。

必须区分累计分配量、每帧分配量和当前内存占用。

### 3. 关键系统耗时

优先复用当前项目已有的 Marker，分别采集 Host 与 Client 中实际可用的系统指标。

重点关注：

- FlowFieldBuild。
- Integration。
- Source / Cost。
- FlowFieldUpdate。
- Separation。
- 怪物更新与空间分组。
- 快照生成和序列化。
- 网络发送与接收。
- 快照反序列化和状态更新。
- 插值及表现层更新。
- Animator、SkinnedMesh 和其他实际存在的渲染相关逻辑。

以上仅为目标示例。必须通过源码确认系统是否存在，以及在哪个进程中执行。

如果现有 Marker 不足，可以在必要的系统边界增加轻量级 ProfilerMarker 或自定义计时。

不要为了采集性能而大规模修改怪物系统。

### 4. Host 报告必须注意的限制

Host 通常同时承担服务器权威模拟和本地客户端工作。

因此：

- Host 整体帧耗时不是纯服务器耗时。
- 应尽可能单独记录服务器模拟、快照构建和网络发送的 Marker。
- 在没有独立服务器进程时，不得将 Host 总帧耗时直接解释为 Dedicated Server 的纯模拟耗时。

### 5. Client 报告必须注意的限制

Client 报告重点关注：

- 接收网络状态后的实际处理开销。
- 网络快照和插值相关耗时。
- 怪物渲染、动画和蒙皮。
- GC 分配与长帧。
- 实际可获取的网络流量、丢包或延迟指标。

不能仅因为某个系统存在于项目中，就假定其耗时可以被独立测量。

无法获取的指标必须明确标记 Unavailable。

### 6. 网络指标

在现有代码和 NGO 接口支持的前提下，尽可能采集：

- 每秒发送字节数。
- 每秒接收字节数。
- 每秒发送和接收消息数。
- 快照大小。
- 网络处理相关 Marker。
- 实际可获取的 RTT、丢包或重传相关指标。

对于无法直接从当前网络栈可靠获取的指标，不得自行估算后冒充真实值。

Host 与 Client 的网络数据需要使用相同 RunId 关联。

## 八、Profiler 采样文件

需要区分完整 Profiler 采样文件和轻量性能统计数据。

### 1. 自动保存完整采样

Unity Profiler 窗口保存的完整采样文件通常使用 `.data` 后缀。

通过 `-profiler-log-file` 或其他运行时二进制日志机制产生的文件可能使用 `.raw`。

请先检查当前 Unity 版本的实际能力，再决定如何自动保存完整采样。

要求：

- 不要通过修改扩展名伪装文件格式。
- 不要假定 `.raw` 与 `.data` 完全等价。
- 不要假定 `ProfilerRecorder` 能替代完整的 Profiler 调用树。
- 如果可以可靠地自动保存完整采样，应为 Host 和 Client 分别保存。
- 如果当前版本无法可靠地自动保存完整 `.data`，应说明原因，并采用实际支持的 Profiler 采样保存方式。

完整采样文件是深入诊断的可选证据，不是主要性能报告的必要依赖。

### 2. 不启用 Deep Profiling 作为常规测试

不要默认启用 Deep Profiling。

Deep Profiling 的额外开销可能显著改变测试结果。

正常测试以轻量 Marker、ProfilerRecorder、自定义计时和逐帧统计为主。

如果需要额外诊断，可以提供可选的 Profiler 采样模式，并明确标记其数据不能直接与普通压测基线混用。

## 九、自动生成性能报告

每次测试必须自动生成三份报告：

1. Host 性能报告。
2. Client 性能报告。
3. 双端综合报告。

### 1. 目录结构

建议使用以下目录：

```
BenchmarkResults/
  latest/
    SUMMARY.md
    run-metadata.json
    host/
      REPORT.md
      metrics.json
      frames.csv
      markers.csv
      profiler.data 或 profiler.raw
    client/
      REPORT.md
      metrics.json
      frames.csv
      markers.csv
      profiler.data 或 profiler.raw
  runs/
    <RunId>/
      SUMMARY.md
      run-metadata.json
      host/
        REPORT.md
        metrics.json
        frames.csv
        markers.csv
      client/
        REPORT.md
        metrics.json
        frames.csv
        markers.csv
```

根据实际实现可以调整结构，但必须保证：

- 每轮测试具有独立结果目录。
- 两端数据不会互相覆盖。
- 历史结果不会被覆盖。
- `latest` 能明确定位最近一次完整测试。
- 原始逐帧数据与 Markdown 报告分开保存。

报告及大型采样文件不要直接放入 Assets，避免触发 Unity 资源导入。

### 2. REPORT.md 内容

Host 和 Client 报告分别包含：

1. 测试有效性。
2. 测试环境和构建信息。
3. 平均帧耗时、P50、P90、P95、P99、最大帧耗时。
4. 关键系统耗时排行。
5. 长帧与周期性卡顿特征。
6. GC Alloc、GC 次数和可获取的内存指标。
7. 网络指标及其可用性。
8. 当前最值得检查的可疑瓶颈。
9. 与历史基线的对比。
10. 下一步值得检查的源码位置与验证方向。

报告必须简洁、信息密度高，方便直接交给 Codex 阅读。

不要将所有原始逐帧数据复制进 Markdown。

### 3. SUMMARY.md 内容

综合报告应包含：

- RunId。
- Host 与 Client 是否均成功完成测试。
- 实际怪物数量。
- 实际有效采样帧数和时长。
- 两端测试是否有效、采样是否对齐。
- Host 主要性能指标。
- Client 主要性能指标。
- 双端性能差异。
- 服务器模拟、网络同步、客户端处理和渲染方面的可疑瓶颈。
- 与历史基线的差异。
- 测试限制、缺失指标及需要人工确认的内容。

不能把 Host 与 Client 的帧耗时简单相加。

不能仅凭 Host 的指标推断 Client 性能，也不能仅凭 Client 掉帧就认定是渲染问题。

如果采样区间、构建配置或工作负载不一致，应明确标记不可直接比较。

### 4. 机器可读数据

`metrics.json` 保存完整汇总指标及元数据。

`frames.csv` 保存逐帧数据。

`markers.csv` 保存系统 Marker 的统计数据。

字段名、单位、统计口径必须统一。

缺失值应明确表示，不得使用零代替未知值。

## 十、测试环境与结果可信度

这是性能工具必须重点保证的部分。

### 1. 固定测试环境

记录：

- Unity 版本。
- Git Commit。
- 工作区是否存在未提交修改。
- 构建类型。
- 目标平台。
- 分辨率。
- 画质配置。
- VSync。
- 帧率限制。
- 随机种子。
- 怪物数量。
- 预热时长。
- 采样时长。
- 测试路线及移动参数。
- Host 与 Client 的启动参数。
- 缓存状态。
- 测试开始和结束时间。

如果当前项目可以可靠地获取硬件和驱动信息，也应记录主要 CPU、GPU 和显卡驱动信息。

### 2. Development Build 与 Release Player

优先通过 Development Player 和轻量采样建立可诊断的性能基线。

如需验证正式运行性能，应额外提供固定配置的非 Development Player 测试。

不同构建类型必须建立独立基线。

不能直接比较 Editor、Development Player 和 Release Player 的帧耗时。

### 3. 两个进程的相互影响

Host 和 Client 在同一台机器上运行，会共同消耗 CPU、GPU、内存和网络资源。

这是本次双进程测试的一部分，不应把两个进程当成完全独立的机器。

但必须注意：

- 固定两端的分辨率与画质。
- 记录两端的运行参数。
- 尽量避免窗口焦点和后台运行状态造成不必要的帧率限制。
- 根据项目实际情况检查 `Application.runInBackground` 等设置。
- 不得为了保持后台采样而随意改变正式游戏逻辑。
- 记录测试机器负载可能造成的限制。
- 不把同一台机器上的双进程数据直接解释为真实双机环境的性能表现。

如果后续需要测试真实联网条件，可以再增加双机测试或网络模拟模式，不要求第一版同时实现复杂的网络环境模拟。

### 4. 缓存与启动耗时

热更新、Addressables 缓存和配置加载会影响启动时间。

必须区分：

- 冷启动或冷缓存测试。
- 缓存已准备的启动测试。
- 正式稳态压测。

当前重点是稳态性能，因此启动和资源下载阶段的数据不得混入稳态帧耗时。

如果两个进程共享缓存，必须确保不存在并发写入冲突。

## 十一、历史基线与性能回归检测

提供基线选择与自动对比功能。

可以将某一轮确认有效的测试结果设为基线，后续运行自动比较。

比较前必须检查：

- 怪物数量一致。
- 测试路线与工作负载一致。
- 构建类型一致。
- 平台、分辨率和画质一致。
- 采样方式与统计口径一致。
- Host 与 Client 角色和测试模式一致。

对于环境不同、指标缺失或测试无效的结果，不得进行无条件的直接比较。

建议初始提供可配置的预警阈值：

- 平均帧耗时上升 5%。
- P95 上升 5%。
- P99 上升 10%。

这些只是初始预警阈值，不是绝对质量标准。

可以支持每个测试用例连续运行多轮，分别保留每轮数据并汇总各轮结果。

如果数据波动较大，应提示需要重复测试，而不是仅凭一次结果判定性能退化。

Host、Client 和综合报告均应展示与基线的差异。

## 十二、性能诊断结论必须可追溯

报告可以根据实际数据自动列出可疑瓶颈，但不得伪造根因。

例如：

- 如果 P95 和 P99 显著上升，应指出对应的长帧特征。
- 如果 FlowFieldBuild 耗时下降而 Separation 耗时上升，应提示可能发生了瓶颈转移。
- 如果 Host 帧耗时稳定但 Client 帧耗时升高，应进一步检查客户端的状态处理、渲染、动画及资源开销。
- 如果网络数据量增大，应检查实际发送速率、快照大小和序列化相关 Marker。
- 如果出现周期性尖峰，应结合逐帧数据、Marker 耗时和更新频率进一步判断。

以上只是诊断逻辑示例，必须由真实采集数据触发。

报告应区分：

- 已由指标证实的事实。
- 有证据支持的可疑原因。
- 需要进一步验证的假设。

不要仅凭 Marker 耗时就声称已经定位到具体代码行。

如果需要进一步分析，应在报告中指出对应 Marker、相关源码文件和建议的下一步检查方向。

## 十三、与 Codex 的日常工作流

工具实现后，日常性能优化应使用以下流程：

1. Codex 修改代码。
2. 在 Unity Editor 中点击“构建并运行双端基准测试”。
3. 自动运行 Host 与 Client，执行固定测试用例。
4. 自动输出两端报告和综合报告。
5. Codex 优先读取最新的 `SUMMARY.md`、Host 报告、Client 报告、关键 CSV 和相关源码。
6. Codex 根据实际指标分析可疑瓶颈，提出修改方案。
7. 修改后再次运行相同测试。
8. 自动比较基线，验证优化效果。

**不要求 Codex 每次都完整分析 Profiler 文件。**

只有当报告和关键指标不足以定位问题时，才进一步检查完整 Profiler 采样文件、调用树和线程时间线。

工具本身不负责自动修改游戏代码，也不负责自动决定最终优化方案。

## 十四、后续可扩展性

第一版优先服务当前怪物系统。

但测试架构应保持可复用，未来可以加入：

- 子弹数量与发射频率测试。
- 怪物与子弹混合负载测试。
- 网络同步层级测试。
- 不同怪物数量下的性能曲线。
- 不同同步频率、快照大小和数据分组策略的对比。
- Dedicated Server + Client 独立服务器模式。
- 双机网络测试及延迟、抖动、丢包模拟。

这些是后续扩展方向，不要求第一版一次性实现。

第一版不要为了通用性设计复杂的测试框架，也不要重构现有游戏架构。

## 十五、实施约束

1. 不修改现有怪物行为、寻路算法、Separation、同步协议和渲染逻辑，除非为测试提供入口确实不可避免。
2. 不绕过正式资源加载、热更新和场景转场流程。
3. 不让 Client 本地生成怪物来替代服务器权威同步。
4. 不在 Host 与 Client 间混用性能数据。
5. 不引入不必要的第三方依赖。
6. 不默认启用 Deep Profiling。
7. 不伪造采样指标或使用零代替缺失数据。
8. 不将测试控制消息混入正式游戏协议，除非明确设计成仅在 Benchmark 模式启用。
9. 遵守当前项目的编码规范、程序集划分与 AOT / HotUpdate 依赖。
10. 代码修改尽可能集中在独立测试工具中，避免污染正式游戏逻辑。
11. 如果当前项目存在阻碍双进程测试的架构问题，先说明具体问题，并优先寻找最小改动的接入方案。
12. 如果当前环境无法实际运行 Player 或无法完成双进程连接，必须明确说明原因，不得把代码编译通过等同于完整验证。

## 十六、最终验收标准

完成后必须实际验证：

1. Unity Editor 中存在清晰的一键双进程压测入口。
2. 构建后能够自动启动 Host 与 Client 两个独立 Player。
3. 两个进程均执行真实的正常启动、资源加载、热更新和必要的场景流程。
4. Client 能通过 `127.0.0.1` 成功加入 Host。
5. Host 权威生成指定数量的怪物。
6. Client 确实接收到实际怪物状态。
7. 自动移动路线能够执行，并验证实际流场重建频率。
8. Host 和 Client 均能独立完成预热、采样和数据保存。
9. 自动生成 Host 报告、Client 报告和综合报告。
10. 报告中包含真实数据、明确单位、统计口径及缺失指标说明。
11. 可以保留历史结果并设置性能基线。
12. 再次运行能够生成新的测试结果，不覆盖历史记录。
13. 测试失败时有明确日志和失败状态。
14. 进程退出后不留下失控的 Player。
15. 不需要人工点击游戏窗口、手动移动玩家或手动操作 Profiler，就能完成一轮主要性能测试。
16. 完整 Profiler 采样如果无法自动生成，必须说明实际限制，但不能因此影响主要性能报告的生成。

第一轮实际验证至少应包含一个完整的双进程测试用例。通过后，再尝试运行 1024、2048、5012 三种怪物规模，并在环境允许时完成多轮测试。

最后请输出：

- 新增与修改的文件清单。
- 工具的使用步骤。
- 测试参数配置方法。
- 实际双进程测试结果。
- Host、Client 和综合报告摘要。
- 与基线比较的结果（如果已有可用基线）。
- 未完成的功能及明确原因。
- 当前仍然存在的测试限制。

请先检查项目，再实施、构建、运行、验证。不要仅生成代码或设计文档就结束任务。

## 十七、项目实际结构、实施方案与进度记录

> 更新日期：2026-10-24
>
> 当前限制：本机已完成 Unity 编译、构建和真实双 Player 验证；同机自动化已拆成 Host 前台轮与 Client 前台轮，正常非 Benchmark 的手动 UI 回归仍待执行。

### 1. 已确认的实际项目结构

- Unity 版本：2022.3.44f1c1。
- Player Build Settings 当前只有 `Assets/_HotUpdate/Scenes/BootStrapScene.unity`。
- 启动链路为：Bootstrap → 局域网端点解析 → Addressables 初始化与 Catalog 更新 → Preload 下载 → AOT 元数据 → HybridCLR HotFix DLL → 配表 → Lobby。
- 游戏阶段不是单一 `GameScene`；服务器通过 `GameSceneFlowController` 协调加载 `GameRunTimeScene + UIGameUIScene`。
- NGO 的内置 Scene Management 已关闭；项目使用自定义 Addressables Prepare/Ready/Commit barrier 同步场景。
- Host/Client 原连接实现位于 Lobby UI Presenter；Profile 提交由 `LobbyOverviewCoordinator` 在注册到权威 Lobby 名单后自动完成。
- StressArena 当前复用正式的 `MonsterRuntimeService`、Flow Field、网络同步、Replica 和 View 链路。
- 原 Prefab 默认目标为 5012；测试档位不再通过修改 Prefab 切换，而是在地图构建前由 Benchmark 启动参数覆盖。
- 玩家移动必须通过 `PlayerSyncController.SubmitLocalInput`；Client 输入仍经过预测、Named Message 和服务器权威模拟。
- Source FlowField 重建存在 12 tick 间隔、24 tick 最大陈旧时间和至少 2 格移动阈值，不能只凭“玩家在移动”判定工作负载有效。

### 2. 第一版实现范围

第一版固定执行 `1024 / 2048 / 5012` 三档怪物规模，默认参数：

```text
WarmupSeconds = 5
CaptureSeconds = 30
RandomSeed = 12345
RepeatCount = 3
Resolution = 1024x768
Quality = Performant
VSync = 0
TargetFrameRate = -1
```

Player 与 Addressables 构建一次；每个档位、每次重复依次执行 Host 前台轮与 Client 前台轮，每轮重新启动一对 Host/Client，并且只采用当前前台目标角色的性能数据。Host 先启动并完成 Lobby Ready，随后启动 Client，以降低两个进程同时更新共享 Addressables 缓存的风险。结果定义为“同机双进程、共享暖缓存、分角色前台稳态压测”，不冒充双机或冷缓存结果。

### 3. 已完成源码改动

- [x] AOT 侧命令行参数解析，支持 role、前台目标、RunId、怪物数、随机种子、重复次数、预热、采样时长、超时、端口、结果目录和 Addressables 内容目录。
- [x] Benchmark 模式显式固定后台运行、1024×768、VSync 关闭和无帧率上限；非 Benchmark 模式不修改这些设置。
- [x] Bootstrap 各阶段通过原子状态文件上报，并支持 Benchmark 指定的本地 Addressables 内容根目录。
- [x] 抽出 UI 与自动化共用的 `LobbyConnectionService`；正式 UI 仍走同一 NetworkSessionBootstrap、LanConnectionPayload、UnityTransport 和 NGO StartHost/StartClient 流程。
- [x] HotFix 入口在 Lobby 加载完成后，仅在 `-benchmark` 模式创建持久化 Benchmark 协调器。
- [x] Benchmark 协调器等待双人 Lobby 名单和 Profile，调用现有 Ready RPC，由原倒计时及服务器权威场景流程进入游戏。
- [x] Stress manager 在 MapBuilt 之前接收怪物数量与 Seed 覆盖，仍调用正式 `MonsterRuntimeService.BeginRoom/Spawn`。
- [x] 新增固定路线移动 Driver，在普通本地输入之后、Gameplay 网络 Tick 之前调用 `PlayerSyncController.SubmitLocalInput`，不直接写 Transform。
- [x] 增加 FlowField Topology/Source/Cost 重建次数和最后 Tick 诊断；报告使用采样区间增量。
- [x] 增加 Client Replica ActiveCount、View Slot/Active 数量，用于确认 Client 真正处理并显示怪物状态。
- [x] Gameplay Named Message 增加应用层接收消息与 Payload 统计；不估算 UTP Header、RTT、丢包或重传。
- [x] 新增轻量 ProfilerRecorder 和逐帧采集，输出 `metrics.json`、`frames.csv`、`markers.csv`、角色 `REPORT.md`。
- [x] 新增 EditorWindow：Build & Run、Run Existing Build、Select Baseline、Stop、Open Results。
- [x] 构建后写入 Player SHA-256 与 Catalog Hash manifest；Run Existing Build 拒绝缺少 manifest 或指纹不一致的构建。
- [x] Editor 编排器在每次重复中依次运行 Host 前台轮与 Client 前台轮；每轮按 Host → Client → 目标 Player 自主切换前台 → 双端 Workload Ready → 统一 UTC 采样窗口 → 完成退出顺序运行，并以 PID + StartTime 限定停止目标。
- [x] 自动生成 `SUMMARY.md`、`run-metadata.json`、`latest.json`；基线比较前检查构建类型、平台、分辨率、质量、VSync、帧率、Seed、采样时长、怪物档位、采样模式和路线版本。
- [x] `BenchmarkBuild/` 与 `BenchmarkResults/` 已加入 `.gitignore`，避免 Player、CSV 和 Profiler 产物进入版本库。

### 4. 静态检查结果

- [x] 新增及修改的 asmdef JSON 可以正常解析。
- [x] 使用 Roslyn 对本轮 19 个新增/修改 C# 文件进行语法树解析，未发现语法错误；分隔符与文件结构检查同时通过。
- [x] Git whitespace/diff check 通过。
- [x] 检查了程序集方向：AOT 不依赖 HotFix；`HotFix.Entry` 只增加对 AOT Bootstrap、Lobby Network、Lobby UI 和已有 Gameplay 的依赖，未发现循环依赖。
- [x] 检查了双端状态机的主要失败路径：Bootstrap 失败、连接失败、阶段超时、怪物/Replica/View 数量不符、移动无实际位移、Host 采样期间无 Source 重建、进程异常退出均标记失败。
- [x] 检查了输出隔离：每个 RunId/怪物档位/重复次数/前台轮次/角色使用独立目录，Host 与 Client 不会覆盖彼此文件。
- [x] Unity 2022.3.44f1c1 C# 编译通过。
- [x] HybridCLR DLL、AOT 补充元数据、Addressables 与 StandaloneWindows64 Development Player 构建通过。

### 5. 待逐项运行验证

以下结果来自 Unity 2022.3.44f1c1 与真实 StandaloneWindows64 Development Player，不能用静态检查代替：

1. [x] Unity 打开工程后无 C#、asmdef、序列化或 API 版本错误。
2. [x] `Tools/性能基准/双端性能测试` 中文窗口能够打开，关键按钮带说明提示。
3. [x] StandaloneWindows64 Development Player、HotFix DLL、AOT 元数据和 Addressables 构建通过。
4. [x] Host 执行完整 Bootstrap 并进入 Lobby，Editor 能识别 `host-ready`。
5. [x] Client 执行完整 Bootstrap，通过 127.0.0.1 加入 Host。
6. [x] 两端使用不同 Benchmark PersistentPlayerId；日志确认 ClientId 0/1、两份 Profile 与两人 Lobby。
7. [x] 两端 Ready 后通过现有倒计时及 Addressables barrier 进入 GameRuntime 与 GameUI 场景。
8. [x] 1024、2048、5012 三档中，Host 权威怪物数与 Client Replica/View 数量全部一致。
9. [x] Client 自动移动输入经过 PlayerSync 网络链路，Host 端工作负载验证通过。
10. [x] 各轮实际位移约 339–357 米，Host 采样期 Source FlowField 重建均大于 0。
11. [x] 已完成各档单轮和 RepeatCount=3；正式双端采样开始/结束对齐容差为 100ms（30 秒窗口的 0.33%）。
12. [x] 双端 JSON/CSV/Markdown 可读；不可用 Counter 记录为 `-1`，没有伪装成 0。
13. [x] 1024 单轮通过后，2048、5012 均完整通过。
14. [x] `20261022-133947-3cca93ce` 完成三档各 3 次；5012 Host 波动明显，详见综合报告。
15. [x] `20261023-130440-72eeba90` 已设为带 Windows 前台证据的基线；`20261023-131133-f3e8ba16` 在相同双端 0% 前台条件下完成比较。前台占比未知或差异超过 5 个百分点时不输出性能差值。
16. [x] `20261023-131030-aacf4c53` 中两端进程启动后执行 Stop，PID + StartTime 匹配的 Player 均退出，无 `_plus` 进程遗留。
17. [x] Unity 2022.3 Player 中 Main Thread、GPU Frame Time、GC Alloc 可用；Render Thread 当前返回 `-1`，按不可用处理。
18. [x] 已实现可选完整 `.raw` Profiler 自动采集；`20261025-064407-c376a952` 生成 18/18 份可读 raw，合计 6.16 GiB。它与轻量采样模式隔离，不伪装为 `.data`。
19. [x] 多轮 Host/Client 共用 persistentDataPath 与暖 Addressables 缓存运行完成，未出现缓存并发写冲突。状态文件写入竞争已改为进程内串行，并为 Windows 文件替换失败提供覆盖写入降级。
20. [x] `20261024-035109-16b5cb39` 完成三档各 1 次双前台轮：12/12 角色结果有效；6 个被测角色前台占比均为 100%；双端采样开始/结束偏差均小于 50ms；无 `_plus` 进程遗留。
21. [ ] 正常非 Benchmark 的单人、手动创建房间、手动加入房间流程仍需人工 UI 回归；Benchmark 已覆盖同一 Bootstrap、Lobby 连接、两人 Ready 与场景切换主链路。
22. [x] `20261025-064407-c376a952` 完成三档各 3 次完整 Profiler 双前台轮：18 个测量角色全部有效，前台占比最低 99.98%，36/36 份 metrics 与 18/18 份 raw 完整落盘；详见该 Run 的 `FULL_PROFILER_REPORT.md`。

### 5.1 前台焦点对同机双端结果的影响

- `Application.runInBackground = true` 只保证失焦后继续执行，不保证 Windows 或显卡驱动给予与前台窗口相同的调度。
- 同一 Windows 桌面不可能让两个独立 Player 同时成为前台窗口。纯自动化还可能让 Editor 或自动化工具保持前台。
- 从 metrics schema 3 起，每帧通过 Windows 前台窗口所属 PID 记录 `focused`；综合报告显示每个角色的前台帧占比。
- `20261023-130440-72eeba90` 与 `20261023-131133-f3e8ba16` 中 Host、Client 的前台占比都为 0%，证明纯自动化时确实可能两个游戏窗口都不在前台。
- 当前同机模式已实现“Host 前台”和“Client 前台”两轮：目标 Player 通过 PID 定位并自主申请 Windows 前台，Editor 确认焦点后才下发采样命令；采样期间目标角色前台占比低于 95% 时该角色结果无效。
- 综合报告仍保留陪跑角色数据用于工作负载证据，但标记为 support role，不参与性能差值；目录结构为 `repeat-<NN>/focus-<host|client>/<role>/`。
- `20261024-035109-16b5cb39` 中 1024、2048、5012 的 Host/Client 被测角色前台占比均为 100%，所有双端采样区间偏差均小于 50ms。
- 该单机方案解决了后台降帧导致的角色测量偏差，但两端仍共享 CPU、GPU、内存和缓存；需要隔离硬件争用的绝对数据时仍应使用两台物理机。

### 6. 当前明确限制

- Editor 一键构建当前会更新项目现有 `ServerData/StandaloneWindows64`，尚未验证独立 Benchmark Addressables Profile。正式发布前需要决定是否保留此行为或增加隔离 Profile。
- 首版只支持当前实际发布目标 `StandaloneWindows64`；其他平台会直接拒绝启动 Benchmark 构建。
- Host 是服务器加本地客户端，不代表 Dedicated Server。
- 两端运行在同一台机器，CPU、GPU、内存和缓存互相影响。
- `ProfilerRecorder` 轻量数据不能替代完整 Profiler 调用树。
- 当前只记录应用层 Named Message Payload；RTT、丢包、重传和传输层额外字节仍是 Unavailable。
- 同机自动化不能同时让 Host、Client 都成为 Windows 前台窗口，因此每次重复会启动两对进程分别采样；报告必须保留每端前台帧占比，且只比较相同前台目标与采样模式的结果。

### 7. 后续测试顺序

后续继续验证按以下顺序执行，任一步失败就保留完整 RunId 结果并修复：

1. 人工完成正常非 Benchmark 的单人、创建房间、加入房间 UI 回归。
2. 如需隔离双进程硬件争用的绝对性能，增加双机远程编排；同机结果继续只在相同前台目标、前台占比和采样模式下比较。
3. 根据 `20261025-064407-c376a952/FULL_PROFILER_REPORT.md`，优先拆解 5012 Host 的 `Monster.Simulation.EntityTick` 与 `Monster.View.ApplyAuthority`；完整 raw 已保留，轻量 Marker 用于跨版本稳定比较。
4. 如需隔离内容缓存影响，再单独增加冷缓存模式；完整 Profiler 原始文件模式已经完成。

### 8. 本轮新增与修改文件

新增：

- `Assets/_AOT/Scripts/Bootstrap/BenchmarkLaunchOptions.cs`
- `Assets/_AOT/Scripts/Bootstrap/BenchmarkFileProtocol.cs`
- `Assets/_HotUpdate/Scripts/Network_Lobby/LobbyConnectionService.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/BenchmarkMovementDriver.cs`
- `Assets/_HotUpdate/Scripts/Entry/BenchmarkRuntimeCoordinator.cs`
- `Assets/_HotUpdate/Scripts/Entry/BenchmarkRuntimeRecorder.cs`
- `Assets/Editor/Benchmark/BenchmarkWindow.cs`
- 上述 Unity 资源对应的 `.meta` 文件。

修改：

- `.gitignore`
- `Assets/_AOT/Editor/AddressablesContentBuildTool.cs`
- `Assets/_AOT/Scripts/Bootstrap/BootstrapRunner.cs`
- `Assets/_HotUpdate/Scripts/Entry/HotFix.Entry.asmdef`
- `Assets/_HotUpdate/Scripts/Entry/HotFixEntry.cs`
- `Assets/_HotUpdate/Scripts/Network_Lobby/LobbyNetworkManager.cs`
- `Assets/_HotUpdate/Scripts/UI/LobbyUI/OverviewUI/OverviewPresenter.cs`
- `Assets/_HotUpdate/Scripts/Network_GamePlay/Transport/NetworkMessageTransport.cs`
- `Assets/_HotUpdate/Scripts/Network_GamePlay/Transport/NetworkTransportStats.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Runtime/MonsterNavigationStressTestManager.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Runtime/MonsterRoomRuntime.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Simulation/MonsterSimulation.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Presentation/MonsterPresentation.cs`
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Presentation/MonsterViewRuntimeService.cs`
- `Docs/大致需求.md`

### 9. 交接入口

独立交接说明已整理至 `Docs/Profiler自动化工具准备-交接.md`，包含工具入口、默认参数、执行链路、输出目录、验证结果、重点风险和版本库边界。后续每一项实测结果仍需同步回本节“待逐项运行验证”，不得只记录在临时日志或口头交接中。
