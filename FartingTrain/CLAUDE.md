# 屁電車 (Fart Train) — 项目上下文

> 这份文件供 Claude Code 自动读取，作为项目背景。每次开工前请先读完这份文档，
> 再结合当前代码库实际内容进行判断——如果文档描述与代码现状不符，以代码为准，
> 并提醒开发者更新本文档。

## 项目基本信息

- **游戏名**：屁電車（Fart Train / Fart Tram）
- **类型**：荒诞喜剧向潜行游戏。设定在电车车厢里，玩家需要在不被NPC发现的
  前提下悄悄放屁、管理气体
- **平台目标**：Steam为主，Switch为潜在未来平台
- **开发者**：Francys（创作者人设"Mepu"），全职工作之外的个人副业项目
- **引擎**：Unity 2D
- **主角**：屁山静（Pi Shan Jing），名字取材自开发者本人在日本职场文化中的
  真实体验，是刻意的喜剧选择
- **内容策略**：devlog系列发布在B站（主平台），"Mepu"人设是内容策略核心，
  不只是展示玩法，也要建立角色亲和力（自嘲、贱嘴风格）

## 类型定位（重要，影响所有设计决策）

**这不是纯潜行游戏**，而是"潜行的约束 + 主动制造混乱的喜剧"的混合体。
失败应该是好笑的，不是惩罚性的。这条原则优先于其他考量。

## 灵感参照

Untitled Goose Game / Human Fall Flat / Katamari Damacy / Getting Over It
——荒诞喜剧 + 日本职场讽刺 + 可爱又失控的美术调性。

设计上重视直播/内容创作者向的传播潜力（viral clip potential），机制设计
时会考虑"这个瞬间好不好剪进视频"。

## 现有代码架构

核心脚本（都在项目里，具体路径以实际代码库为准）：

- `NPCController.cs` — NPC共享基类，两条并行状态轴：警觉状态机
  （Relaxed→Alert→Suspicious→Confirmed，跟屁接触事件驱动）+ 乘客/
  下车状态（OnBoard→MovingToExit→Exited，由TrainManager到站广播
  驱动）。开放了一批`protected`字段和`virtual`方法/钩子
  （`OnUpdateExtra()`、`CanWalk`、`EnterState(state, confirmLevel,
  multiplier)`），供角色专属子类接管/暂停基础逻辑，不用改这个文件
  本身
- `OjisanNPCController.cs` — 继承自NPCController，"社畜大叔"专属。
  打盹状态轴：Awake→Yawning→Asleep→[自然醒 或 Shocked→指认]，跟
  警觉/下车两条轴通过`CanWalk`等钩子仲裁优先级（被抓包演出优先级
  最高，不会被下车广播打断；打盹只在两条基础轴都空闲时触发）
- `TrainManager.cs` — 行驶计时+到站判断；`exitingNpcs`列表配置"本关
  谁在哪个门下车"，到站前按`approachLeadTime`提前广播
  `onApproachingStation`，通知列表里的NPC开始走向指定门
- `PlayerController.cs` — 玩家控制
- `GasManager.cs` — 玩家放屁状态管理：Normal / Medium / Hard 三档，
  对应不同动画组和移动速度倍率
- `UIManager.cs` — UI逻辑，包括基于"清白比例"阈值的sprite切换、
  社会性死亡值(social death value)量表
- `FartEffect.cs` — 屁效果的云状爆发+飘散粒子行为
- `FartSoundManager.cs` — 单例，屁音效池化播放，避免连续重复音效
- `InnocentManager.cs` — 清白值/嫌疑管理。`Deduct(int reactionLevel,
  float multiplier = 1f)`认的是"第几档"（1/2/3，对应
  small/medium/largeDeduction），不是直接扣分数值——传其他数字会
  静默扣0，需要更重的惩罚要用`largeDeduction × multiplier`，不要
  加新的硬编码档位

## 代码架构偏好（重要）

- **优先扩展现有脚本，而不是新建脚本**。除非确实是完全独立的新系统
  （比如关卡配置这种），否则先看能不能塞进现有文件的结构里
- **NPC共享轴 vs 角色专属行为要分开**：所有NPC都有的基础能力（警觉
  状态机、乘客/下车状态）留在`NPCController`；某个角色独有的怪癖
  （比如大叔的打盹）用继承子类（`XxxNPCController : NPCController`），
  通过`protected`字段+`virtual`钩子接入，不要在`NPCController`里
  堆一堆`canXXX`开关——不然以后NPC越加越多，基类会变成大杂烩
- **动画全部代码驱动**，用 `animator.Play()` 直接切换，不用Animator
  Transition图形化连线那套
- 命名习惯、代码风格以现有文件为准，新代码要跟现有风格保持一致

## 已验证的设计原则（踩过的坑，别重复踩）

1. **NPC状态机越简单越好**：曾经有过Cooldown状态，测试后发现去掉更自然，
   已经砍掉了。不要轻易往状态机里加新状态，除非有实际测试支撑
2. **非线性惩罚防止дом策略**：早期版本里线性的社会性死亡值惩罚导致"疯狂
   放大屁"成为必胜策略。改成非线性惩罚曲线后解决。任何新的惩罚/奖励机制
   设计时要考虑是否会催生类似的单一支配策略
3. **星级系统**用于跨关卡分组门控进度
4. **NPC行为设计模式**：权重随机idle行为 + 手动编写的触发层，用来创造
   刻意的"安全窗口"，不是纯随机
5. **多条并行状态轴是可行的**：警觉状态机、乘客/下车状态、大叔专属的
   打盹状态，三条轴同时存在没有互相打架，靠的是显式的优先级仲裁——
   谁在忙就用一个钩子（`CanWalk`之类）让其他轴让路，而不是互相监听
   事件猜时序。以后再加新轴照这个模式接
6. **周期性概率判定要给"退出"留口子**：任何"进入某状态后就一直待着"
   的设计（比如打盹）都要配一个对称的"有概率自动离开"判定，不然容易
   变成可以被玩家利用的安全区

## 当前正在做/接下来要扩展的方向

正在把NPC底层逻辑拆成清晰的几层：

### 1. 底层逻辑（NPC基础行为层）—— 尚未做
- 性格原型数据：嗅觉敏感度阈值、反应速度、移动模式、记忆时长，因NPC而异
- 原计划用ScriptableObject承载数值；实际先走了另一条路——数值差异用
  public字段挂在NPCController/子类实例上（Inspector里per-NPC调），行为
  差异（比如打盹）用继承子类。要不要再引入ScriptableObject看后续需求，
  暂时没这个必要
- 现有状态机（Relaxed→Alert→Suspicious→Confirmed）结构保留，转换阈值
  仍是写死的public字段，还没有"因性格原型联动"这一层

### 2. 跟屁互动逻辑 —— 尚未做
- 计划中的"甩锅"主动技能：玩家可以把嫌疑目标从自己身上转移到附近NPC身上，
  应该做成独立方法，供外部（玩家技能系统）调用
- 计划中的"憋住"机制：创造压力值资源管理的张力（尚未设计细节）

### 3. 上下车逻辑 —— 已实现
- `TrainManager.exitingNpcs`（`List<StationExitEntry>`）配置本关谁在哪个
  门下车，到站前`approachLeadTime`秒广播`onApproachingStation`并调用
  对应NPC的`BeginExit(doorTarget)`
- `NPCController`新增`PassengerState`轴（OnBoard/MovingToExit/Exited），
  跟警觉状态机并行独立，走路时若被屁打断会原地暂停等警觉状态机接管
- 还未设计：气味残留累积、移动中的NPC作为障碍物、多站点（目前一关一站）

## 工作方式偏好

- 改动前先说明打算怎么改、改哪些文件，展示思路或diff，让开发者确认后
  再实际写入文件——不要一次性大改
- 大改动前提醒开发者确认是否已用git做版本管理
- devlog脚本类内容由开发者本人撰写，Claude不代写，只做审阅/润色/找钩子
