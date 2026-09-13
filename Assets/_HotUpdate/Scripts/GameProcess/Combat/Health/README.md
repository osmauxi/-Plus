# Health

玩家、怪物、可破坏物共用的集中式 Health 模块。7 个实现脚本；每个实体只注册数据，不挂载 Health 行为组件。

- 权威端：创建 `HealthRuntime` → `Register` → `TryEnqueue` → `Step` → `Unregister`。
- 客户端：创建 `HealthReplica` → 应用全量/增量 `HealthFrame`。
- UI：依赖 `IHealthStateSource`；反应/统计：订阅 `IHealthEventSource`。
- 命中对象：由 `HealthEntityBindings<Collider>` 集中维护映射。

不自动运行，也未接入 GameRoot、Weapon、网络消息或玩家/怪物生成器。参数通过 `IHealthDefinitionSource` 注入，正式读表由后续装配层完成。

完整规则、调用示例、生命周期约定及未接入项见仓库根目录 [Health模块使用指南.md](../../../../../../Health模块使用指南.md)。
