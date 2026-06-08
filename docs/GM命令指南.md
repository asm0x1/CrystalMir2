# Crystal Mir2 - GM 命令指南

> 作者: asm0x1

## 如何成为 GM

### 方法一：管理员账户（推荐）

使用管理员账户登录（`AdminAccount = true`），登录后自动获得 GM 权限。

默认管理员账户：`asm0x1` / `123456`

### 方法二：GM 密码

1. 进入游戏后，在聊天框输入 `@LOGIN`
2. 系统提示输入 GM 密码
3. 输入 GM 密码（默认：`C#Mir 4.0`，可在 `Configs/Setup.ini` 的 `GMPassword` 修改）
4. 验证通过后获得 GM 权限

## 命令格式

在聊天框中以 `@` 开头输入命令。

- `@!消息` — GM 广播（全服公告）
- `@命令 [参数]` — 执行 GM 命令

---

## 玩家管理

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **KILL** | `@KILL [玩家名]` | 杀死指定玩家（前方怪物/玩家） | GM |
| **LEVEL** | `@LEVEL 等级` | 设置自身等级 | GM/Test |
| | `@LEVEL 玩家名 等级` | 设置指定玩家等级 | GM |
| **LEVELHERO** | `@LEVELHERO 等级` | 设置自身英雄等级 | GM/Test |
| | `@LEVELHERO 玩家名 等级` | 设置指定玩家英雄等级 | GM |
| **CHANGEGENDER** | `@CHANGEGENDER [玩家名]` | 切换性别 | GM/Test |
| **CHANGECLASS** | `@CHANGECLASS [玩家名] 职业` | 切换职业（Warrior/Wizard/Taoist/Assassin/Archer） | GM/Test |
| **ADJUSTPKPOINT** | `@ADJUSTPKPOINT 数值` | 调整自身 PK 值 | GM/Test |
| | `@ADJUSTPKPOINT 玩家名 数值` | 调整指定玩家 PK 值 | GM |
| **REVIVE** | `@REVIVE` | 原地复活 | GM |
| **HAIR** | `@HAIR [编号0-9]` | 更换发型 | GM/Test |

## 物品与货币

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **MAKE** | `@MAKE 物品名/编号 [数量]` | 制造物品 | GM/Test |
| **GIVEGOLD** | `@GIVEGOLD 数量` | 给自己金币 | GM/Test |
| | `@GIVEGOLD 玩家名 数量` | 给指定玩家金币 | GM |
| **GIVECREDIT** | `@GIVECREDIT 数量` | 给自己元宝 | GM/Test |
| | `@GIVECREDIT 玩家名 数量` | 给指定玩家元宝 | GM |
| **GIVEPEARLS** | `@GIVEPEARLS 数量` | 给自己珍珠 | GM/Test |
| | `@GIVEPEARLS 玩家名 数量` | 给指定玩家珍珠 | GM |
| **GIVESKILL** | `@GIVESKILL 技能名 [等级0-3]` | 学习/提升技能 | GM/Test |
| | `@GIVESKILL 玩家名 技能名 [等级0-3]` | 给指定玩家技能 | GM |
| **CLEARBAG** | `@CLEARBAG [玩家名]` | 清空背包 | GM/Test |
| **DELETESKILL** | `@DELETESKILL 技能名` | 删除技能（不填技能名则列出） | 所有人 |
| **ADDINVENTORY** | `@ADDINVENTORY` | 扩展背包（消耗金币） | 所有人 |
| **ADDSTORAGE** | `@ADDSTORAGE` | 扩展仓库（消耗金币） | 所有人 |

## 装备与强化

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **AWAKENING** | `@AWAKENING 装备类型 觉醒类型` | 装备觉醒 | GM/Test |
| **REMOVEAWAKENING** | `@REMOVEAWAKENING 装备类型` | 移除觉醒 | GM/Test |

## 传送移动

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **MOVE** | `@MOVE` | 随机传送 | GM/Test |
| | `@MOVE X坐标 Y坐标` | 传送到指定坐标 | GM |
| **MAPMOVE** | `@MAPMOVE 地图文件名 [实例ID] [X Y]` | 切换地图 | GM/Test |
| **GOTO** | `@GOTO 玩家名` | 飞到指定玩家身边 | GM |
| **RECALL** | `@RECALL 玩家名` | 召唤玩家到自己身边 | GM |
| **GROUPRECALL** | `@GROUPRECALL` | 召唤全组成员（需传送戒指） | 所有人 |
| **RECALLMEMBER** | `@RECALLMEMBER 组员名` | 召唤指定组员（需传送戒指） | 所有人 |
| **RECALLLOVER** | `@RECALLLOVER` | 召唤伴侣（需婚戒） | 所有人 |
| **FIND** | `@FIND 玩家名` | 查找玩家位置 | GM/探测 |

## 怪物

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **MOB** | `@MOB 怪物名/编号 [数量] [扩散范围]` | 刷怪 | GM/Test |
| **RECALLMOB** | `@RECALLMOB 怪物名/编号 [数量] [宠物等级0-7]` | 召唤怪物为宠物 | GM/Test |
| **CLEARMOB** | `@CLEARMOB [地图名]` | 清除当前地图怪物 | GM |

## 自身状态

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **SUPERMAN** | `@SUPERMAN` | 切换无敌模式（GMNeverDie） | GM/Test |
| **GAMEMASTER** | `@GAMEMASTER` | 切换 GM 模式（GMGameMaster） | GM/Test |
| **OBSERVER** | `@OBSERVER` | 切换隐身观察模式 | GM |
| **CLEARBUFFS** | `@CLEARBUFFS` | 清除所有 Buff | 所有人 |
| **DIE** | `@DIE` | 自杀 | 所有人 |
| **RIDE** | `@RIDE` | 上/下马 | 所有人 |
| **TOGGLETRANSFORM** | `@TOGGLETRANSFORM` | 暂停/恢复变形效果 | 所有人 |

## 信息查询

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **INFO** | `@INFO` | 查看前方对象信息 | GM/Test |
| | `@INFO 玩家名` | 查看指定玩家信息 | GM/Test |
| **MAP** | `@MAP` | 显示当前地图信息 | 所有人 |
| **TIME** | `@TIME` | 显示服务器时间 | 所有人 |
| **LISTFLAGS** | `@LISTFLAGS` | 列出已设置的标记 | GM/Test |

## 任务与标记

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **SETFLAG** | `@SETFLAG 标记编号` | 切换指定标记 | GM/Test |
| **CLEARFLAGS** | `@CLEARFLAGS [玩家名]` | 清除所有标记 | GM/Test |
| **CLEARQUESTS** | `@CLEARQUESTS [玩家名]` | 清除所有任务 | GM/Test |
| **SETQUEST** | `@SETQUEST 任务ID 状态` | 设置任务状态（0=取消,1=完成） | GM/Test |
| | `@SETQUEST 任务ID 状态 玩家名` | 设置指定玩家任务 | GM |
| **TRIGGER** | `@TRIGGER 触发器名 [玩家名]` | 触发 NPC 自定义脚本 | GM |

## 公会与攻城

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **CREATEGUILD** | `@CREATEGUILD 公会名` | 创建公会 | GM/Test |
| | `@CREATEGUILD 玩家名 公会名` | 为指定玩家创建公会 | GM |
| **LEAVEGUILD** | `@LEAVEGUILD` | 退出公会 | 所有人 |
| **STARTWAR** | `@STARTWAR 公会名` | 向指定公会宣战（需会长） | 所有人 |
| **ALLOWGUILD** | `@ALLOWGUILD` | 切换允许公会邀请 | 所有人 |
| **STARTCONQUEST** | `@STARTCONQUEST 攻城ID` | 强制开启/关闭攻城战 | GM/Test |
| **RESETCONQUEST** | `@RESETCONQUEST 攻城ID` | 重置攻城战 | GM/Test |
| **GATES** | `@GATES [OPEN/CLOSE]` | 开关城门（需占领公会） | 公会 |
| **CHANGEFLAG** | `@CHANGEFLAG [编号0-11]` | 更换旗帜图案 | 公会 |
| **CHANGEFLAGCOLOUR** | `@CHANGEFLAGCOLOUR [R G B]` | 更换旗帜颜色 | 公会 |

## 服务器管理

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **RELOADDROPS** | `@RELOADDROPS` | 重新加载掉落配置 | GM |
| **RELOADNPCS** | `@RELOADNPCS` | 重新加载 NPC 脚本 | GM |
| **CLEARIPBLOCKS** | `@CLEARIPBLOCKS` | 清除 IP 封锁列表 | GM |

## 账号数据

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **BACKUPPLAYER** | `@BACKUPPLAYER 玩家名` | 备份玩家数据 | GM |
| **ARCHIVEPLAYER** | `@ARCHIVEPLAYER 玩家名` | 归档玩家（移除角色） | GM |
| **LOADPLAYER** | `@LOADPLAYER 玩家名` | 从备份加载玩家数据 | GM |
| **RESTOREPLAYER** | `@RESTOREPLAYER 玩家名 [账号名]` | 恢复已删除角色 | GM |

## 其他

| 命令 | 语法 | 说明 | 权限 |
|------|------|------|:--:|
| **ROLL** | `@ROLL` | 掷骰子（组队） | 所有人 |
| **DECO** | `@DECO 图片编号` | 放置装饰物 | GM/Test |
| **ALLOWTRADE** | `@ALLOWTRADE` | 切换允许交易 | 所有人 |
| **ALLOWOBSERVE** | `@ALLOWOBSERVE` | 切换允许观察 | 所有人 |
| **OBSERVE** | `@OBSERVE 玩家名` | 观察指定玩家 | 所有人 |
| **SUMMONHERO** | `@SUMMONHERO` | 召唤/收回英雄 | 所有人 |
| **ENABLEGROUPRECALL** | `@ENABLEGROUPRECALL` | 切换允许组队召唤 | 所有人 |
| **SETLIGHT** | `@SETLIGHT 亮度(0-255)` | 设置自身光照强度 | GM |
| **SETTIMER** | `@SETTIMER 计时器名 时间(秒)` | 设置计时器 | GM/Test |
| **CHANGEFLAGCOLOUR** | `@CHANGEFLAGCOLOUR [R G B]` | 更换旗帜颜色 | 公会 |
| **GATES** | `@GATES [OPEN/CLOSE]` | 开关城门 | 公会 |

## GM 状态说明

| 状态 | 效果 |
|------|------|
| **GM** (`IsGM`) | 管理员权限，可使用所有 GM 命令 |
| **SUPERMAN** (`GMNeverDie`) | 无敌状态，不受伤害 |
| **GAMEMASTER** (`GMGameMaster`) | 隐蔽模式，不显示装备、不能 PK |
| **OBSERVER** (`Observer`) | 隐身观察，其他玩家不可见 |
