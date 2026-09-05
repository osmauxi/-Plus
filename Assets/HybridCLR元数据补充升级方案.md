# HybridCLR 元数据补充升级方案

## 一、这次问题的根因

`AOTGenericReferences.PatchedAOTAssemblyList` 已经分析出 `AOT_Core.dll` 需要补充元数据，但旧的 `HotUpdateBuilderTool` 另外维护了一份只有 5 个 DLL 的硬编码名单。

因此“生成了泛型引用”不等于“运行时真的加载了对应元数据”：`AOT_Core.dll.bytes` 没有进入 Addressables 的 `AOT_DLL` 标签，Bootstrap 也就没有执行 `LoadMetadataForAOTAssembly(AOT_Core)`。热更层调用 AOT 层的 `LocalEvents.Publish<ShowOverviewMessageEvent>` 时，IL2CPP Player 最终报 `MissingMethodException`。

事件总线现已迁移到独立热更程序集 `HotFix.Events`，消除了这条高频泛型调用的 AOT/HotFix 边界；但 AOT 补充元数据仍必须正确维护，因为项目中的 Unity、NGO、UniTask、MessagePack 等 AOT 程序集仍可能被热更代码以泛型方式调用。

## 二、已完成的一级升级

元数据同步不再维护人工 DLL 数组。`Tools > HotUpdate > Build And Sync DLLs` 会反射读取 HybridCLR 生成的：

```csharp
AOTGenericReferences.PatchedAOTAssemblyList
```

并用它完成以下工作：

1. 从当前目标平台的 `AssembliesPostIl2CppStrip` 复制完整裁剪后 AOT DLL。
2. 将 DLL 以 `.bytes` 形式同步到 `Assets/_HotUpdate/DLLS`。
3. 全量同步 Addressables `AOT_DLL` 标签。
4. 删除由工具管理、但已不在最新生成名单中的旧 AOT 条目。
5. 缺少生成文件、列表为空或任一源 DLL 不存在时立即中止，避免产出“看似成功、实际缺元数据”的内容包。

当前生成名单共 16 个程序集，已经包含 `AOT_Core.dll`。Player 冒烟日志已确认 16 个程序集的元数据加载结果全部为 `OK`。

## 三、迁移后的程序集边界

```text
HotFix.Lobby.UI / HotFix.Gameplay / HotFix.SceneFlow
                    │
                    ▼
              HotFix.Events
                    │
                    ▼
     AOT_Core 中仅保留非泛型公共辅助类型
```

`LocalEvents`、`LocalEventBus`、事件接口及事件订阅组位于 `HotFix.Events`。事件类型继续使用 `readonly struct`，事件总线泛型实例由 HybridCLR 热更环境处理，不再要求 IL2CPP 为每种业务事件预先生成 AOT 实例。

## 四、以后每次升级的标准顺序

只改纯热更逻辑且没有改变 AOT 调用面时，可从第 3 步开始；修改 AOT 代码、泛型用法、asmdef 归属、HybridCLR 程序集列表或 Unity/第三方包后，必须执行完整流程。

1. 在 HybridCLR Settings 中确认所有热更程序集已登记，特别是新增或拆分出的 `HotFix.*` asmdef。
2. 执行 `HybridCLR > Generate > All`，重新生成桥接函数、裁剪配置和 `AOTGenericReferences.cs`。
3. 构建一次当前平台 Player，让 `AssembliesPostIl2CppStrip/<平台>` 产生与该 Player 一致的裁剪后 AOT DLL。
4. 执行 `Tools > HotUpdate > Build And Sync DLLs`，同步热更 DLL 和最新 AOT 补充元数据。
5. 构建 Addressables Player Content，把 `.bytes` 文件和 Catalog 写入 `ServerData/<平台>`。
6. 将完整的新 `ServerData/<平台>` 部署到资源服务器；不能只替换某一个 bundle。
7. 如果 AOT/Bootstrap、Player Settings、初始 Catalog 地址或 Build Settings 改过，重新构建 Player。
8. 用 Player 而非只用 Editor 做一次启动、Lobby、创建房间、开始游戏冒烟测试。

关键约束：第 3 步生成的裁剪后 DLL 必须来自最终准备发布的同目标平台 Player。不要拿旧 Player、其他平台或 Editor 的 DLL 作为补充元数据。

## 五、验收清单

构建前检查：

- `AOTGenericReferences.PatchedAOTAssemblyList` 包含本轮分析出的所有 AOT 程序集。
- `Assets/_HotUpdate/DLLS` 中存在同名 `.dll.bytes`。
- Addressables `HotfixDLLs` Group 中，AOT 条目带有 `AOT_DLL` 标签，热更条目带有 `Hotfix_DLL` 标签。
- `HotFix.Events`、`HotFix.SceneFlow` 等新增热更程序集同时存在于 HybridCLR Settings、Addressables 和 Bootstrap 加载顺序中。

Player 日志检查：

```text
加载 AOT 元数据：AOT_Core.dll，结果：OK
加载热更程序集：HotFix.Events
LobbyScene 加载成功
```

并确认不存在：

```text
MissingMethodException
ExecutionEngineException
TypeLoadException
AssemblyResolutionException
LoadMetadataForAOTAssembly ... Invalid
```

## 六、下一阶段升级建议

建议后续把完整流程封装为一个发布前命令，并加入以下校验：

1. 记录本次 Player 构建目标、时间和裁剪后 DLL 哈希，拒绝同步旧平台产物。
2. 比较生成名单、Addressables 标签和实际 `.bytes` 文件，三者不一致则构建失败。
3. 为每个 `LoadMetadataForAOTAssembly` 结果生成汇总，任一非 `OK` 时阻止进入热更入口。
4. 在 CI 中扫描 AOT asmdef 对 HotFix asmdef 的反向依赖，防止再次形成 AOT 依赖热更程序集。
5. 保存一份发布清单，绑定 Player、Catalog、HotFix DLL 和 AOT metadata 的版本/哈希，避免客户端与 `ServerData` 串版本。

这一级先解决“人工名单漂移”这一最直接、最容易复发的问题；下一阶段再把平台一致性和发布物一致性变成自动化门禁。
