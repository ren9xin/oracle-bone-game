# 甲骨文 · Oracle Bone

一款以汉字演变为叙事内核的 3D 剧情解谜游戏。玩家往返于商、秦、唐三个朝代，通过收集甲骨文字、破解机关谜题、参与诗词对战，最终拼出回家的口令。

Unity（团结引擎）实现，C# 脚本 86 个 / 6845 行，11 个关卡场景，156 个数据资产。

![游戏封面](docs/screenshots/cover.png)
<!-- 截图待补充：主界面 / 次元空间 HUB / 长安诗会对战 / 结局拼字 -->

## 玩法

游戏以「次元空间」作为关卡枢纽，每通关一个朝代解锁一道门，顺序推进。

| 阶段 | 场景 | 核心玩法 |
| --- | --- | --- |
| 序章 | `序章` | 剧情引导，建立世界观 |
| 商朝 | `商朝/商朝`、`商朝集市`、`井底`、`王宫` | 场景解谜：水井打捞、攀爬梯子、搬箱、传送门 |
| 秦朝 | `秦朝/咸阳街市`、`太学藏书阁` | 限时救援：计时 + 计分，藏书阁内抢救典籍 |
| 唐朝 | `唐朝/长安诗会` | 飞花令对战：按主题字在候选诗句中选出正确项 |
| 结局 | `结局` | 暗号拼字：从字库中拼出通关口令 |

核心循环：

1. 开始界面点击 → `LoadManager` 异步加载序章，显示加载进度
2. 通关后回到 `次元空间Hub`，按 `unlockedGateIndex` 解锁下一道门
3. 进入对应朝代场景完成解谜/对战
4. 收集到的甲骨字进入「字典」系统，可查看字形与释义
5. 集齐暗号字形后解锁回家门，进入结局拼字

## 技术栈

| 项 | 版本 / 说明 |
| --- | --- |
| 引擎 | 团结引擎 Tuanjie 1.8.1（Editor 2022.3.62t3） |
| 渲染管线 | Built-in |
| 语言 | C# |
| UI | uGUI + TextMeshPro 3.0.9 |
| 美术规格 | 1920×1080，Linear 色彩空间 |

## 运行方式

1. 用团结引擎 1.8.1 或 Unity 2022.3.62f 系列版本打开工程
2. 打开 `Assets/Scenes/开始界面.scene`，按 Play
3. 构建时场景列表已按流程顺序配置好，可直接 Build

> 首次导入会重建 Library，字体资产（35 MB）导入耗时较长，请耐心等待。

## 技术实现

### 异步场景加载

`Assets/脚本/全局管理/LoadManager.cs` 用 `SceneManager.LoadSceneAsync` 配合 `Slider` + `TextMeshProUGUI` 实时显示加载进度，避免朝代切换时的长时间黑屏。

### 事件驱动的模块解耦

全项目 15 处 `event Action`，集中在 5 个核心管理器，订阅方在 `OnEnable` / `Start` 挂载，发布方通过单例派发。

以 `PoetryDuelManager` 为例，对外暴露 8 个事件：`题目刷新`、`玩家状态变化`、`对手状态变化`、`答题结果(bool, string)`、`关卡开始`、`关卡胜利`、`关卡失败`、`全部通关`。HUD 层只订阅事件刷新表现，不反向调用战斗逻辑，把飞花令的规则实现与界面呈现彻底拆开。

### ScriptableObject 数据驱动

5 类 SO 定义 + 156 个数据资产，策划内容与代码完全分离：

| SO 类型 | 用途 | 字段要点 | 资产数 |
| --- | --- | --- | --- |
| `DialogueSO` | 对白节点 | NPC 引用、内容数组、选项数组、下一节点、回调 | 多 |
| `NPCSO` | NPC 档案 | 名称、立绘 | 多 |
| `甲骨字` | 字库条目 | 现代字、字形图、释义 | 20 |
| `PoetryQuestion` | 飞花令题库 | 主题字、4 个候选诗句、选项挖空字、正确索引、出处、难度 | 25 |
| `Inventory` | 背包 | 物品列表 + 增删查接口 | — |

对白系统做成链式节点：每个 `DialogueSO` 持有 `NextDialogueNode` 引用与 `DialogueCallback` 数组，策划在 Inspector 里串联即可写出分支剧情，无需改代码。

### 存档系统

`Assets/脚本/全局管理/GameSaveManager.cs`（213 行），单例 `DontDestroyOnLoad`。

- 序列化：`JsonUtility.ToJson` 转文本后由 `BinaryFormatter` 落盘，路径 `Application.persistentDataPath/game_SaveData/saveData.txt`
- 12 个存档字段：当前场景、生命值、金币、背包、门解锁进度、是否获得甲骨文、字形阶段、三段灵蝉对白标记、UI 隐藏状态
- 载入流程做了完整的状态重建：先 `UIManager.CloseAllUI()` 清场，反序列化后按 `currentScene` 载场景，再在 `SceneManager.sceneLoaded` 回调中回填各管理器状态，并对未获得的碎片清空 `itemList`，避免读到脏数据

### 对象池

`Assets/脚本/玩家/对象池创建金币.cs` 使用 Unity 原生 `UnityEngine.Pool.ObjectPool<GameObject>`，通过 `Create / Get / Release / Destroy` 四个回调接管生命周期。取用时重置 `position` 与 `rotation` 再 `SetActive(true)`，回收时仅 `SetActive(false)`，避免金币频繁实例化带来的 GC 抖动。

### 编辑器工具

`Assets/Editor/暗号道具调试工具.cs`（581 行），11 个 `MenuItem`，菜单根 `Tools/暗号道具/`。这是为反复调试「道具装配」这一高频手工操作而写的：

- 检查装配状态，把缺失引用打印到 Console
- 一键装配修复，自动扫描全部场景并接线
- 赋予 4 个暗号道具（仅 Play 模式可用）
- 字形演化的阶段跳转：甲骨 → 秦篆 → 唐楷
- 解锁到下一关、清空背包、重置全部演化与对白进度
- 删除磁盘存档、打开存档目录

实现基于 `AssetDatabase`、`EditorUtility.SetDirty` 与 `EditorSceneManager`，把原本需要手动改十几个 Inspector 字段的流程压缩成一次点击。

## 项目结构

```
Assets/
├── Editor/          编辑器工具（暗号道具调试工具）
├── 脚本/            全部游戏逻辑 73 个脚本
│   ├── 全局管理/    关卡加载、存档、游戏状态、诗会对战、救援计时
│   ├── 物品/交互/   水井、梯子、宝箱、传送门等场景交互
│   ├── UI/          各系统 HUD 与面板
│   ├── 玩家/        玩家控制与对象池
│   └── 音乐资源/    背景音乐管理
├── SO/              156 个数据资产（对白、诗题、独白）
├── Inventory/       背包与物品域
├── 字典/            甲骨字典系统与 20 个甲骨字资产
├── Scenes/          11 个关卡场景
├── 预制体/          8 个通用预制体
├── 动画/            动画片段与控制器
├── 美术资源/        主视觉素材 225 个
└── font/            字体
```

## 已知问题

- `EditorBuildSettings` 第 7 项指向 `Assets/Scenes/秦朝.scene`，该文件已不存在且 GUID 全零（该项已禁用）。打开 Build Settings 会看到一条缺失条目，不影响其余 11 个场景运行。
- 存档格式无版本号字段，后续若增删 `PlayerSaveData` 字段，旧存档需手动清理。旧存档路径为 `%USERPROFILE%/AppData/LocalLow/DefaultCompany/甲骨文/game_SaveData/`。
- 字体资产 35.41 MB，首次 import 明显拖慢打开速度。
- `Packages/manifest.json` 依赖 GitHub 直取的 `com.coplaydev.unity-mcp`，离线环境会导致包解析失败，可手动移除该条目。
- 部分管理器脚本偏大（`GameManager`、`PoetryDuelManager`）职责偏重，有待拆分。

## 后续计划

- [ ] 存档升级为纯 JSON + 版本号 + 字段迁移
- [ ] 编辑器工具补充 `Undo` 支持，操作可回退
- [ ] 对象池从金币推广到碎片、特效等高频实例化对象
- [ ] 渲染管线 Built-in → URP，统一光照与后处理
- [ ] 拆分巨型脚本，收敛事件中心为统一总线
- [ ] 补充自动化测试（背包、存档序列化）

## 开发分工

> 待补充。