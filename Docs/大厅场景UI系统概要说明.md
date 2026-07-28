# 大厅场景 UI 系统概要说明

> **文档版本**：v3.0  
> **最后更新**：2026-07-09  
> **对应项目**：`_plus` (Unity + HybridCLR 热更框架)  
> **分析范围**：`Assets/_HotUpdate/Scripts/UI/` 目录下的所有大厅 UI 脚本，及调用链终点 `Assets/_HotUpdate/Scripts/GameProcess/HotFixEntry.cs`

---

## 目录

1. [系统综述](#1-系统综述)
2. [核心架构：MVP 设计模式](#2-核心架构mvp-设计模式)
3. [文件清单与职责](#3-文件清单与职责)
4. [完整调用链](#4-完整调用链)
5. [核心类详细说明](#5-核心类详细说明)
6. [子模块分解](#6-子模块分解)
7. [接口总览](#7-接口总览)
8. [待实现部分与已知技术债务](#8-待实现部分与已知技术债务)

---

## 1. 系统综述

大厅场景 UI 系统是游戏**主大厅（Lobby）场景**的核心前端层，采用 **MVP（Model-View-Presenter）设计模式**搭建。它负责：

- **多子屏（Screen）切换**：使用 `LobbyScreenState` 枚举定义所有子屏状态，由 `LobbyUIManager` 统一调度。
- **网络数据驱动的 UI 刷新**：通过 `LobbyNetworkManager` 监听大厅网络数据变更（玩家装备、角色信息等），自动推送到当前活跃的 Presenter 进行视图刷新。
- **Cinemachine 运镜联动**：每个子屏可绑定一个 `CinemachineVirtualCamera`，在子屏激活/休眠时自动切换相机优先级，实现 UI 与场景运镜的联动。
- **武器选择与概览**：提供武器选择子屏（WeaponChoseUI）和装备概览子屏（OverviewUI），均遵循 MVP 结构。

---

## 2. 核心架构：MVP 设计模式

### 2.1 三层定义

| 层次 | 职责 | 对应基类/接口 |
|------|------|--------------|
| **Model（数据层）** | 存放数据结构和纯数据定义 | `LobbyScreenState`(枚举)、`ItemSlotData`、`ItemCategory`、`WeaponInfo` |
| **View（视图层）** | 纯 UI 显示与交互事件发射，不包含业务逻辑 | `ItemSlotView`, `ItemSelectView`, `OverviewView`, `EquipmentSlotView`, `MainActionButtonView` |
| **Presenter（表现层）** | 业务逻辑、数据-视图绑定、生命周期管理 | `BaseLobbyPresenter`(抽象基类), `ItemSelectPresenter`, `OverviewPresenter` |

### 2.2 生命周期管理：WakeUp / Sleep 机制

所有子屏 Presenter 继承自 `BaseLobbyPresenter`，遵循以下生命周期：

```
ForceSleep(初始隐藏)
    └── WakeUp()     → 激活：显示 UI → 提升虚拟相机优先级 → 调用 RenderView() 刷新数据
    └── Sleep()      → 休眠：隐藏 UI → 降低虚拟相机优先级
```

关键设计点：
- **不使用 `SetActive` 控制显示/隐藏**，而是通过 `CanvasGroup.alpha` 和 `interactable/raycast` 切换，避免反复触发 `Awake/OnEnable` 等生命周期函数。
- **网络数据变更拦截**：`BaseLobbyPresenter` 在 `Start` 时订阅 `LobbyNetworkManager.OnLobbyDataChanged`，在 `OnDestroy` 时取消订阅。仅在 `_isWorking == true` 时响应数据变更并调用 `RenderView()`。

---

## 3. 文件清单与职责

### 3.1 根目录 (`Assets/_HotUpdate/Scripts/UI/`)

| 文件 | 类型 | 职责 |
|------|------|------|
| `BaseLobbyPresenter.cs` | **抽象基类** | 所有大厅 Presenter 的基类，提供 `WakeUp/Sleep` 生命周期、CanvasGroup 控制、Cinemachine 相机切换、网络数据拦截 |
| `LobbyUIManager.cs` | **管理器** | 大厅 UI 系统的总调度中心，负责子屏切换、数据协调、网络管理 |
| `LobbyScreenState.cs` | **枚举** | 定义所有大厅子屏状态 (`WeaponSelect`, `Overview`, `Room`, `Chat`, `Settings` 等) |
| `ItemCategory.cs` | **枚举** | 物品分类定义（武器/防具/消耗品等） |
| `ItemSlotData.cs` | **数据类** | 单个物品槽位的数据结构（物品 ID、数量、分类、图标索引等） |
| `AvatarResManager.cs` | **资源管理** | 角色化身/模型资源加载管理器 |

### 3.2 WeaponChoseUI 子目录 (`Assets/_HotUpdate/Scripts/UI/WeaponChoseUI/`)

| 文件 | 类型 | 职责 |
|------|------|------|
| `ItemSelectView.cs` | **View** | 武器选择界面的整体视图容器，管理所有物品槽位的布局与显示 |
| `ItemSelectPresenter.cs` | **Presenter** | 武器选择的业务逻辑，继承 `BaseLobbyPresenter`，负责数据注入和槽位刷新 |
| `ItemSlotView.cs` | **View** | 单个物品槽位的视图组件，处理点击和选中状态显示 |
| `WeaponInfo.cs` | **数据类** | 武器信息数据结构定义 |

### 3.3 OverviewUI 子目录 (`Assets/_HotUpdate/Scripts/UI/OverviewUI/`)

| 文件 | 类型 | 职责 |
|------|------|------|
| `OverviewView.cs` | **View** | 装备概览界面的视图容器，包含装备槽位布局和主操作按钮的引用 |
| `OverviewPresenter.cs` | **Presenter** | 装备概览的业务逻辑，继承 `BaseLobbyPresenter`，负责当前装备信息的展示和装备切换 |
| `EquipmentSlotView.cs` | **View** | 单个装备槽位的纯视觉组件，处理鼠标悬停/点击动效（DOTween），向外抛出 `OnSlotClicked` 事件 |
| `MainActionButtonView.cs` | **View** | 通用动态按钮视图，处理悬停放大、按压缩小、松开回弹等多态动效，向外抛出 `OnClicked` 事件 |

---

## 4. 完整调用链

以下从**入口**到**终点**追踪整个调用链：

### 4.1 总链路

```
┌──────────────────────────────────────────────────────────────────┐
│  HotFixEntry.OnAssemblyLoaded()                                  │
│    │  (Assets/_HotUpdate/Scripts/GameProcess/HotFixEntry.cs)     │
│    │                                                             │
│    ├─→ UIManager.Instance.Init()                                  │
│    │     └─ UI 框架底层初始化                                     │
│    │                                                             │
│    ├─→ LobbyUIManager.Instance.Init()                            │
│    │     └─ 大厅 UI 系统初始化                                    │
│    │     └─ 注册各子屏 Presenter                                  │
│    │     └─ 绑定 LobbyNetworkManager 数据监听                    │
│    │                                                             │
│    └─→ 等待场景加载完成 → 触发首次 UI 渲染                        │
│                                                                  │
│  LobbyUIManager.SwitchScreen(newState)                           │
│    │                                                             │
│    ├─ 当前活跃 Presenter.Sleep()                                  │
│    │     ├─ CanvasGroup α=0, interactable=false, raycast=false   │
│    │     └─ VirtualCamera.Priority = 0                           │
│    │                                                             │
│    └─ 目标 Presenter.WakeUp()                                    │
│          ├─ CanvasGroup α=1, interactable=true, raycast=true     │
│          ├─ VirtualCamera.Priority = 10                          │
│          └─ RenderView()  ← 抽象方法，由子类实现                  │
│               │                                                  │
│               ├─ [WeaponChoseUI] ItemSelectPresenter.RenderView()│
│               │    └─ 从网络数据刷新 ItemSlotView 列表            │
│               │    └─ ItemSlotView.OnSlotClicked → 事件回调       │
│               │                                                  │
│               └─ [OverviewUI] OverviewPresenter.RenderView()     │
│                    └─ 刷新 EquipmentSlotView 装备槽               │
│                    └─ MainActionButtonView.OnClicked → 事件回调   │
│                                                                  │
│  网络数据驱动刷新：                                               │
│  LobbyNetworkManager.OnLobbyDataChanged                          │
│    └─→ BaseLobbyPresenter.InterceptDataChanged()                 │
│          └─ if (_isWorking) → RenderView() (子类实现)            │
│                                                                  │
│  终点：HotFixEntry 中没有进一步处理 UI 后续逻辑，UI 系统在此      │
│  进入自我驱动的 MVP 循环。                                        │
└──────────────────────────────────────────────────────────────────┘
```

### 4.2 详细调用顺序

1. **`HotFixEntry.OnAssemblyLoaded()`**
   - 调用 `UIManager.Instance.Init()` 初始化 UI 底层框架
   - 调用 `LobbyUIManager.Instance.Init()` 初始化大厅 UI 系统

2. **大厅 UI 初始化阶段**
   - `LobbyUIManager` 在 `Init()` 中：
     - 根据场景配置注册所有子屏 Presenter (`ItemSelectPresenter`, `OverviewPresenter` 等)
     - 绑定 `LobbyNetworkManager.Instance.OnLobbyDataChanged` 事件
     - 设置默认显示子屏（通常是 `WeaponSelect`）

3. **子屏切换阶段**
   - `LobbyUIManager.SwitchScreen(newState)` 被调用后：
     - 对当前活跃 Presenter 调用 `Sleep()`
     - 从注册表中查找目标状态对应的 Presenter
     - 对目标 Presenter 调用 `WakeUp()`

4. **数据变更驱动刷新**
   - 网络数据变更 → `LobbyNetworkManager` 触发 `OnLobbyDataChanged`
   - `BaseLobbyPresenter` 拦截事件 → 检查 `_isWorking` → 调用子类的 `RenderView()`
   - 子类 Presenter 根据数据类（`ItemSlotData`, `WeaponInfo` 等）构建视图数据
   - 注入到对应的 View 中，View 负责纯 UI 渲染

---

## 5. 核心类详细说明

### 5.1 `BaseLobbyPresenter`（抽象基类）

- **文件**：`Assets/_HotUpdate/Scripts/UI/BaseLobbyPresenter.cs`
- **命名空间**：`ProjectGame.HotFix.UI.Lobby`
- **继承**：`MonoBehaviour`
- **组件依赖**：`CanvasGroup` (自动添加)
- **核心字段**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `_associatedState` | `LobbyScreenState` | 与本 Presenter 关联的子屏状态 |
| `_virtualCamera` | `CinemachineVirtualCamera` | 可选绑定的虚拟相机（Inspector 拖入） |
| `_canvasGroup` | `CanvasGroup` | 私有，在 `Awake` 中通过 `GetComponent` 获取 |
| `_isWorking` | `bool` | 是否处于激活状态 |

- **核心方法**：

| 方法 | 访问级别 | 说明 |
|------|---------|------|
| `WakeUp()` | `public` | 激活面板：显示 UI → 提升相机优先级 → 调用 `RenderView()` |
| `Sleep()` | `public` | 休眠面板：隐藏 UI → 降低相机优先级（调用 `ForceSleep()`） |
| `ForceSleep()` | `private` | 强制隐藏：CanvasGroup 清零（α=0, interactable=false, raycast=false） |
| `RenderView()` | `protected abstract` | **子类必须实现的抽象方法**，用于刷新视图数据 |
| `InterceptDataChanged()` | `private` | 网络数据变更拦截：仅在 `_isWorking` 时调用 `RenderView()` |

- **生命周期流程**：
  ```
  Awake() → ForceSleep() (初始隐藏)
     ↓
  Start() → 订阅 LobbyNetworkManager.OnLobbyDataChanged
     ↓
  WakeUp() → _isWorking=true → Show UI → Camera↑ → RenderView()
     ↓
  Sleep() → _isWorking=false → ForceSleep() → Camera↓
     ↓
  OnDestroy() → 取消订阅 LobbyNetworkManager.OnLobbyDataChanged
  ```

### 5.2 `LobbyUIManager`（总调度管理器）

- **文件**：`Assets/_HotUpdate/Scripts/UI/LobbyUIManager.cs`
- **模式**：单例（Singleton）
- **核心职责**：
  - 维护一个 `Dictionary<LobbyScreenState, BaseLobbyPresenter>` 的子屏注册表
  - 提供 `SwitchScreen(LobbyScreenState)` 接口进行子屏切换
  - 协调网络数据与 UI 刷新
- **核心方法猜测**（基于调用链推断，未完整阅读该文件）：
  - `Init()`：初始化，注册子屏 Presenter
  - `SwitchScreen(newState)`：切换子屏
  - `GetPresenter(state)`：根据状态获取对应的 Presenter

### 5.3 `LobbyScreenState`（子屏状态枚举）

- **文件**：`Assets/_HotUpdate/Scripts/UI/LobbyScreenState.cs`
- **枚举值（已知）**：
  - `WeaponSelect`：武器选择界面
  - `Overview`：装备概览界面
  - `Room`：房间管理界面
  - `Chat`：聊天界面
  - `Settings`：设置界面

---

## 6. 子模块分解

### 6.1 WeaponChoseUI 模块（武器选择）

| 类 | 类型 | 核心职责 |
|---|------|---------|
| `ItemSelectPresenter` | Presenter | 继承 `BaseLobbyPresenter`，在 `RenderView()` 中从网络数据读取可选武器列表，注入到 `ItemSelectView` 中 |
| `ItemSelectView` | View | 管理物品槽位列表，持有 `List<ItemSlotView>` 引用，提供 `SetItems(List<ItemSlotData>)` 等方法 |
| `ItemSlotView` | View | 单个物品槽位：显示图标、名称、选中状态，点击时通过事件回调通知 Presenter |
| `ItemSlotData` | Model | 槽位数据结构：物品ID、数量、分类(`ItemCategory`)、图标索引 |
| `ItemCategory` | Model | 枚举：武器、防具、消耗品等 |
| `WeaponInfo` | Model | 武器详细数据结构 |

**数据流**：
```
LobbyNetworkManager (网络数据)
    └─→ ItemSelectPresenter.RenderView()
         └─ 解析网络数据 → List<ItemSlotData>
              └─→ ItemSelectView.SetItems(slotDataList)
                   └─→ 遍历 slotDataList → ItemSlotView.BindData(slotData)
```

### 6.2 OverviewUI 模块（装备概览）

| 类 | 类型 | 核心职责 |
|---|------|---------|
| `OverviewPresenter` | Presenter | 继承 `BaseLobbyPresenter`，在 `RenderView()` 中刷新玩家装备信息，处理装备切换逻辑 |
| `OverviewView` | View | 装备概览容器视图，持有 `List<EquipmentSlotView>` 和 `MainActionButtonView` 的引用 |
| `EquipmentSlotView` | View (子View) | 单个装备槽位的纯视觉组件，处理鼠标悬停/点击动效（DOTween），向外抛出 `OnSlotClicked(int)` 事件 |
| `MainActionButtonView` | View (子View) | 通用动态按钮，处理多态动效（悬停放大、按压缩小、松开回弹），向外抛出 `OnClicked` 事件 |

**数据流**：
```
LobbyNetworkManager (网络数据)
    └─→ OverviewPresenter.RenderView()
         └─ 解析网络数据 → 当前装备信息
              └─→ OverviewView 中刷新各 EquipmentSlotView
                   └─ EquipmentSlotView 点击 → OnSlotClicked → Presenter 处理
              └─→ MainActionButtonView.OnClicked → Presenter 处理
```

### 6.3 `EquipmentSlotView` 详细说明

- **文件**：`Assets/_HotUpdate/Scripts/UI/OverviewUI/EquipmentSlotView.cs`
- **组件依赖**：`RectTransform` (自动添加)
- **外部依赖**：DOTween (`DG.Tweening`)
- **核心字段**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `SlotIndex` | `int` | 槽位索引（public，Inspector 可设或在代码中动态设置） |
| `_hoverOffsetY` | `float` (序列化) | 鼠标悬停时的 Y 轴上浮偏移量（默认 30） |
| `_tweenDuration` | `float` (序列化) | 动效持续时间（默认 0.2s） |
| `OnSlotClicked` | `event Action<int>` | 点击事件，携带 `SlotIndex` |

- **动效说明**：
  - **悬停** (`OnPointerEnter`)：DOTween 动画 → Y 轴上浮偏移（`Ease.OutBack` 回弹效果）
  - **离开** (`OnPointerExit`)：DOTween 动画 → Y 轴归位（`Ease.OutQuad`）
  - **点击** (`OnPointerClick`)：触发 `OnSlotClicked` 事件 + 缩放入拳效果（`DOPunchScale` 负向）
  - **禁用保护** (`OnDisable`)：强制杀死所有 DOTween 动画并恢复原始位置和缩放

### 6.4 `MainActionButtonView` 详细说明

- **文件**：`Assets/_HotUpdate/Scripts/UI/OverviewUI/MainActionButtonView.cs`
- **外部依赖**：DOTween (`DG.Tweening`)
- **核心字段**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `_hoverScale` | `float` (序列化) | 鼠标悬浮时的缩放倍率（默认 1.05） |
| `_pressScale` | `float` (序列化) | 鼠标按下时的缩放倍率（默认 0.95） |
| `_tweenDuration` | `float` (序列化) | 动效持续时间（默认 0.15s） |
| `OnClicked` | `event Action` | 点击事件 |

- **动效状态机**：

| 触发事件 | 动画行为 | 缓动函数 |
|---------|---------|---------|
| 鼠标悬浮 (`OnPointerEnter`) | 缓动放大至 `_hoverScale` | `Ease.OutQuad` |
| 鼠标移出 (`OnPointerExit`) | 缓动恢复至原始缩放 | `Ease.OutQuad` |
| 鼠标按下 (`OnPointerDown`) | 快速缩小至 `_pressScale`（持续时间为一半） | `Ease.OutQuad` |
| 鼠标抬起 (`OnPointerUp`) | 缓动恢复至 `_hoverScale`（带"果汁感"回弹） | `Ease.OutBack` |
| 鼠标点击 (`OnPointerClick`) | 触发 `OnClicked` 事件 | - |
| 禁用 (`OnDisable`) | 杀死所有动画、强制恢复原始缩放 | - |

---

## 7. 接口总览

### 7.1 `BaseLobbyPresenter` 对外接口

```csharp
// 公共方法
public void WakeUp();                              // 激活面板
public void Sleep();                               // 休眠面板

// 公共属性
public LobbyScreenState AssociatedState { get; }   // 关联的子屏状态

// 受保护抽象方法（子类必须实现）
protected abstract void RenderView();              // 刷新视图数据

// 序列化字段（Inspector 配置）
[SerializeField] private LobbyScreenState _associatedState;          // 关联状态
[SerializeField] private CinemachineVirtualCamera _virtualCamera;    // 运镜相机
```

### 7.2 `LobbyUIManager` 对外接口（已知方法）

```csharp
// 静态单例
public static LobbyUIManager Instance { get; }

// 公共方法
public void Init();                                          // 初始化
public void SwitchScreen(LobbyScreenState newState);         // 切换子屏
// 推测可能还有：
// public BaseLobbyPresenter GetPresenter(LobbyScreenState state);
// public void RegisterPresenter(LobbyScreenState state, BaseLobbyPresenter presenter);
```

### 7.3 View 层事件接口

```csharp
// ItemSlotView - 物品槽位点击
void OnSlotClicked(int slotIndex);

// EquipmentSlotView - 装备槽位点击
event Action<int> OnSlotClicked;    // 携带 SlotIndex

// MainActionButtonView - 主操作按钮点击
event Action OnClicked;             // 无参点击事件

// ItemSelectView - 推测接口
public void SetItems(List<ItemSlotData> items);
```

### 7.4 数据模型定义

```csharp
// LobbyScreenState (枚举)
enum LobbyScreenState {
    WeaponSelect,
    Overview,
    Room,
    Chat,
    Settings,
    // ... 更多状态
}

// ItemCategory (枚举)
enum ItemCategory {
    Weapon,
    Armor,
    Consumable,
    // ... 更多分类
}

// ItemSlotData (数据结构)
class ItemSlotData {
    int id;
    int count;
    ItemCategory category;
    int iconIndex;
    // ... 更多字段
}

// WeaponInfo (数据结构)
class WeaponInfo {
    // 武器详细信息字段
}
```

---

## 8. 待实现部分与已知技术债务

### 8.1 `BaseLobbyPresenter` 中的待实现/优化项

| 编号 | 类型 | 描述 | 优先级 |
|------|------|------|---------|
| BP-01 | **双端静音保护** | `InterceptDataChanged()` 方法仅在 `_isWorking` 时响应数据变更。但**当前 `Sleep()` 在 ForceSleep 后会降低相机优先级，而网络数据可能在面板休眠后被推送到 `RenderView()`**（虽然被 `_isWorking` 过滤了）。但 **ForceSleep 时未清理视觉残留**（如之前注入的列表数据仍在内存中），应在 WakeUp 时增加数据脏检标记以保证首帧拉取最新数据。 | 中 |
| BP-02 | **Camera 优先级策略** | 当前 `WakeUp` 中硬编码 `_virtualCamera.Priority = 10`，`Sleep` 中硬编码为 0。多子屏叠加场景（如双屏同开）中将出现相机冲突。建议使用**动态权重策略**或与 `LobbyUIManager` 的切换栈联动。 | 高 |
| BP-03 | **错误处理缺失** | `GetComponent<CanvasGroup>()` 在 `Awake()` 中没有 null 检查。如果 Inspector 中意外移除了 `RequireComponent` 自动添加的 CanvasGroup（虽然不太可能），将导致 `NullReferenceException`。另外 `LobbyNetworkManager.Instance` 在 `Start()` 和 `OnDestroy()` 中也未做 null 检查（虽然做了判空防御）。 | 低 |
| BP-04 | **OnDestroy 清理不完整** | `OnDestroy` 中仅取消订阅 `OnLobbyDataChanged`，但未杀死可能正在进行的 DOTween 动画（如果子类使用了）。建议添加 `DOTween.Kill(gameObject)` 调用。 | 低 |

### 8.2 `EquipmentSlotView` 中的待实现/优化项

| 编号 | 类型 | 描述 | 优先级 |
|------|------|------|---------|
| ESV-01 | **视觉数据绑定缺失** | 当前 `EquipmentSlotView` 仅有动效系统，**没有显示装备图标、名称、等级等视觉元素**。需要添加 `Image icon`, `TMP_Text itemName` 等字段和 `BindData(ItemSlotData)` 方法，使槽位能实际显示装备内容。 | 高 |
| ESV-02 | **槽位状态反馈** | 当前仅实现了"点击"反馈，缺少**选中态**（高亮边框/背景）、**禁用态**（灰色不可交互）、**空槽位**（显示"+"或提示符）等视觉状态。 | 高 |
| ESV-03 | **动效参数序列化** | `_hoverOffsetY` 和 `_tweenDuration` 虽可通过 Inspector 调整，但建议增加 **Ease 曲线配置项**（当前硬编码 `Ease.OutBack` / `Ease.OutQuad`），让 UI 美术可自行调整。 | 低 |
| ESV-04 | **动画时序保护** | `OnDisable` 中虽有 `DOKill` 保护，但 `OnPointerEnter/Exit/Click` 中没有防快速重复点击的保护。快速连击时 `DOPunchScale` 可能堆积。建议添加 `_isAnimating` 锁或使用 `DOTween.SetId` 并 `Kill(id)` 来管理。 | 低 |

### 8.3 `MainActionButtonView` 中的待实现/优化项

| 编号 | 类型 | 描述 | 优先级 |
|------|------|------|---------|
| MABV-01 | **音频反馈缺失** | 按钮动效丰富但缺少音频系统集成。建议添加 `AudioClip` 字段和 `OnPointerClick` 中的音效播放。 | 中 |
| MABV-02 | **功能绑定方式待定** | 当前 `OnClicked` 事件通过代码订阅，但**如何在 `OverviewPresenter` 中将 `MainActionButtonView.OnClicked` 绑定到具体逻辑（如"进入游戏"/"返回主菜单"）尚未实现**。需要明确 Presenter 与 View 的事件代理模式。 | 高 |
| MABV-03 | **`EnsureInitialized` 调用时机** | 每次交互事件都调用 `EnsureInitialized()` 做惰性初始化，`Start()` 中也调用。但若 Game Object 通过对象池复用（非当前场景），`_isInitialized` 标记不会重置导致缩放错误。建议在 `OnEnable` 中重置 `_isInitialized = false`。 | 中 |
| MABV-04 | **禁用态处理** | 当前按钮**没有禁用态 (`interactable = false`)** 的处理。应在 `OnPointerEnter/Exit/Down/Up/Click` 前检查 `interactable` 状态，禁用时跳过所有动效。 | 中 |

### 8.4 模块级待实现/待完善

| 编号 | 类型 | 描述 | 优先级 |
|------|------|------|---------|
| MOD-01 | **LobbyNetworkManager 数据契约未明确** | `OnLobbyDataChanged` 是一个无参委托（`delegate void OnLobbyDataChanged()`），这意味着 Presenter 必须**主动拉取**更新后的数据，但数据拉取接口尚未在 Presenter 中明确定义。建议明确数据契约：由 `LobbyNetworkManager` 提供 `GetWeaponList()`, `GetEquipmentData()` 等读接口，或改为数据推送的 `Action<T>` 事件。 | 高 |
| MOD-02 | **WeaponChoseUI 和 OverviewUI 之间数据传递** | 用户在武器选择界面选择武器后，装备概览界面应实时反映变化。当前该跨子屏数据传递机制未明确（是通过 `LobbyNetworkManager` 同步到服务器再回落，还是通过 `LobbyUIManager` 做中介传递？）。 | 高 |
| MOD-03 | **Room/Chat/Settings 子屏未实现** | `LobbyScreenState` 枚举中定义了 `Room`, `Chat`, `Settings` 等状态，但代码中仅发现 `WeaponSelect` 和 `Overview` 的 Presenter 实现。其余子屏的 Presenter/View 尚未创建。 | 中 |
| MOD-04 | **AvatarResManager 集成待实现** | UI 目录下存在 `AvatarResManager.cs`，用于管理角色化身资源的加载，但**当前未在任何 Presenter 中发现对 `AvatarResManager` 的调用**。需要在 `OverviewPresenter` 或其他相应处集成 3D 角色预览。 | 中 |
| MOD-05 | **LoadingView 与大厅 UI 的衔接** | `HotFixEntry` 中初始化大厅 UI 后，加载界面 (`LoadingView` / `LoadingUI`) 与大厅 UI 的切换时机和过渡动画未明确定义。 | 低 |

### 8.5 技术债务总结

| 类别 | 数量 | 说明 |
|------|------|------|
| 🔴 核心功能缺失（高优先级） | 4 项 | 数据绑定、跨屏数据传递、数据契约、功能绑定 |
| 🟡 功能完善（中优先级） | 5 项 | 双端保护、Camera 策略、禁用态、音频、对象池复用 |
| 🟢 优化建议（低优先级） | 5 项 | 错误处理、动画保护、配置序列化、动画清理、场景切换衔接 |
| **合计** | **14 项** | |

---

## 附录

### A. 外部依赖

| 依赖 | 用途 |
|------|------|
| `Cinemachine` | 虚拟相机运镜联动（`BaseLobbyPresenter`） |
| `DOTween` (DG.Tweening) | UI 动效系统（`EquipmentSlotView`, `MainActionButtonView`） |
| `HybridCLR` | 热更新框架，承载所有 `_HotUpdate` 脚本 |
| `LobbyNetworkManager` (>Netcode命名空间) | 大厅网络数据管理 |
| `UnityEngine.EventSystems` | UI 交互事件接口 |

### B. 命名空间约定

```
ProjectGame.HotFix.UI.Lobby          ← 大厅 UI 系统核心
ProjectGame.HotFix.Netcode            ← 网络相关（LobbyNetworkManager）
```

### C. 文件位置速查

```
Assets/_HotUpdate/Scripts/UI/
├── BaseLobbyPresenter.cs              ← 抽象基类
├── LobbyUIManager.cs                  ← 总调度管理器
├── LobbyScreenState.cs                ← 子屏状态枚举
├── ItemCategory.cs                    ← 物品分类枚举
├── ItemSlotData.cs                     ← 槽位数据结构
├── AvatarResManager.cs                ← 角色资源管理
├── WeaponChoseUI/
│   ├── ItemSelectView.cs              ← 武器选择 View
│   ├── ItemSelectPresenter.cs         ← 武器选择 Presenter
│   ├── ItemSlotView.cs                ← 武器槽位 View
│   └── WeaponInfo.cs                  ← 武器数据结构
└── OverviewUI/
    ├── OverviewView.cs                 ← 装备概览 View
    ├── OverviewPresenter.cs           ← 装备概览 Presenter
    ├── EquipmentSlotView.cs           ← 装备槽位 View (子View)
    └── MainActionButtonView.cs        ← 主操作按钮 View (子View)
```

---

> **文档维护说明**：本文档基于 2026-07-09 的代码分析生成，记录了当前大厅 UI 系统的设计意图和已知问题。随着项目开发推进，待实现部分可能已落地——届时应更新本文档以反映最新状态。