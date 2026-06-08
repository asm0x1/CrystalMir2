# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

- **Required**: Visual Studio 2022 (v17.8+) and .NET 8 SDK
- **Open solution**: `Legend of Mir.sln` (8 projects, all SDK-style .csproj)
- **Build**: `dotnet build "Legend of Mir.sln"` (outputs to `Build/Client/`, `Build/Server/`, etc.)
- **No test project exists** — there are no automated tests in this repository

## Project Architecture

This is an open-source C# server and client engine for **The Legend of Mir 2** (1999 Korean MMORPG). Three main projects form the core, plus ancillary tool projects:

```
Shared (net8.0 library)
  └─ Pure data-and-protocol layer — defines packets, enums, data models, and localization.
     Referenced by both client and server. Contains NO game logic.

Server.Library (net8.0 library)
  └─ Core server game logic — game loop, monster AI, combat, networking, database, NPC scripts.
     Depends on Shared + log4net. Runs as a library consumed by Server.MirForms.

Client (net8.0-windows WinExe)
  └─ Game client — Direct3D9 rendering (SlimDX), WinForms host, NAudio for sound.
     Standalone project referencing Shared.

Tools (net8.0-windows WinExe)
  ├─ Server.MirForms — Windows Forms GUI to administer the running server
  ├─ LibraryEditor — Edit .wzl/.wil image library files
  ├─ LibraryViewer — View image library files
  ├─ AutoPatcherAdmin — Manage the client auto-patcher
  └─ CustomFormControl — Reusable WinForms control (FixedListViewControl)
```

## Network Protocol (Shared/)

All client-server communication uses a **custom binary protocol** — not JSON, not protobuf. Defined in `Shared/`:

| File | Role |
|---|---|
| `Packet.cs` | Abstract base class with `ReadPacket(BinaryReader)` / `WritePacket(BinaryWriter)`. Wire format: `[2B length][2B packet ID][payload]`. Two giant switch statements dispatch packet IDs to concrete instances. |
| `ServerPackets.cs` (191KB) | ~146 server→client packet types (`S.*` namespace). Each is a sealed class with serialized fields. |
| `ClientPackets.cs` (77KB) | ~70 client→server packet types (`C.*` namespace). |
| `Enums.cs` | All shared enums including `ServerPacketIds`, `ClientPacketIds`, `MirClass`, `Spell`, `ItemType`, `Monster`, `CellAttribute`, and ~80 more. |
| `Language.cs` (236KB) | Localization engine — a massive dictionary of `ClientTextKeys`/`ServerTextKeys` enum values mapping to English strings. `GameLanguage.SetClientLanguage()` swaps translations at runtime. |
| `Data/` | Shared data-transfer types (`ItemInfo`, `UserItem`, `ClientMagic`, `Stats`, etc.). Each supports binary serialization via `BinaryReader`/`BinaryWriter` constructors. |

**Key pattern**: Adding a new packet means: (1) add an enum value to `ClientPacketIds` or `ServerPacketIds`, (2) create the sealed class in `ClientPackets.cs` or `ServerPackets.cs`, (3) add a case to the dispatch switch in `Packet.cs`.

## Server Architecture (Server.Library/)

The server runs a **single-threaded game loop with cooperative multitasking** — not an event-driven or ECS architecture.

### Core Loop (`Server/MirEnvir/Envir.cs` — 5,479 lines)

`Envir.Main` is the singleton server instance. `WorkLoop()` runs every ~1ms and sequentially:
1. Processes all `MirConnection` instances (receive packets → dispatch → send queued packets)
2. Iterates `LinkedList<MapObject> Objects`, calling `Process()` on each object when its `OperateTime` has elapsed
3. Calls `Map.Process()` on each loaded map
4. Every 1s: guild wars, conquests, auctions, respawns, robot scripts
5. Periodic saving to disk

Optional multithreaded monster processing: `MobThreads[]` distribute monster AI across background threads via `Monitor.Wait`/`PulseAll`.

### Game Object Hierarchy (`Server/MirObjects/`)

```
MapObject (abstract base — ObjectID, location, Process(), OperateTime)
  ├─ MonsterObject (base for 200+ monster types, AI, targeting, pathfinding)
  │   └─ Monsters/*.cs — one class per monster AI type (HolyDeva, Guard, EvilMir, etc.)
  ├─ HumanObject (stats, inventory, spells, buffs — shared between Players and Heroes)
  │   ├─ PlayerObject (14,682 lines — every player action handler)
  │   └─ HeroObject (AI companions, subclassed by class: WarriorHero, WizardHero, etc.)
  ├─ NPCObject (merchants/quest-givers, loads NPCScripts)
  ├─ ItemObject (items on ground)
  ├─ SpellObject (active spell effects/fireballs)
  ├─ DelayedAction (queued timed effects)
  └─ ConquestObject, GuildObject, DecoObject, IntelligentCreatureObject
```

### Key Server Subsystems

| Directory/File | Purpose |
|---|---|
| `MirDatabase/` | Pure data classes serialized to `Server.MirDB`/`Server.MirADB` binary files. Includes `CharacterInfo`, `MonsterInfo`, `ItemInfo`, `MapInfo`, `QuestInfo`, `GuildInfo`, etc. |
| `MirNetwork/MirConnection.cs` (2,366 lines) | Per-player TCP connection. Huge switch statement dispatching ~150 `ClientPacketIds` to handler methods (movement, combat, inventory, NPC interaction, trading, mail, etc.). |
| `MirEnvir/Map.cs` (2,592 lines) | Per-map 2D cell grid, A* pathfinding, broadcasting packets to nearby players. Supports multiple map file formats. |
| `MirObjects/NPC/NPCActions.cs` | NPC script engine actions (`GiveItem`, `TakeItem`, `GiveExp`, `Mongen`, etc.). `NPCChecks.cs` has conditional checks. |
| `MirEnvir/RespawnTimer.cs` | Monster respawn management, rate-limited by online player count. |
| `Settings.cs` (1,800 lines) | Static config loaded from `Configs/Setup.ini` — every tunable in the game. |

### Persistence

No RDBMS. All data lives in custom binary files under `Server.MirDB/` (world data: maps, items, monsters) and `Server.MirADB/` (player data: accounts, characters, inventory). Guilds and conquests use JSON-style persistence.

## Client Architecture (Client/)

The client uses a **single-threaded game loop** driven by the WinForms idle pattern, rendering with SlimDX (Direct3D9).

### Startup Flow (`Client/Program.cs`)

1. Parse args (`-tc` for test config), optionally run AutoPatcher
2. `Settings.Load()` — read `Mir2Config.ini`
3. Validate display resolution
4. Launch `CMain` (SlimDX RenderForm)

### Rendering (`Client/MirGraphics/DXManager.cs`)

- SlimDX Direct3D9 device with pixel shaders (`normal.ps`, `grayscale.ps`, `magic.ps`)
- Sprite-based rendering (no 3D geometry)
- `MLibrary.cs` — image library system, loads index-based sprite sheets from `Data/` directory
- `ParticleEngine.cs` — particle effects (fog, snow, rain, leaves, etc.)

### Scene System (`Client/MirScenes/`)

```
LoginScene → SelectScene → GameScene (517KB — the main gameplay scene)
```

`GameScene` owns 30+ dialog windows: `MainDialog` (HUD/minimap), `NPCDialog`, `InventoryDialog`, `GuildDialog`, `QuestDialogs`, `TradeDialogs`, `ChatDialog`, etc.

### UI System (`Client/MirControls/`)

Custom UI widget framework (not WinForms). `MirControl` is the base with parent/child hierarchy, off-screen texture rendering, and input routing. All widgets (buttons, labels, text boxes, item cells, dropdowns) inherit from it. `MirScene` extends `MirControl` — scenes are composable control trees.

### Other Client Subsystems

| Directory | Purpose |
|---|---|
| `MirNetwork/Network.cs` | TCP client with async send/receive, packet queueing. `Process()` called each frame drains the receive queue and routes to `MirScene.ActiveScene.ProcessPacket()`. |
| `MirSounds/SoundManager.cs` | NAudio-based audio. Separate one-shot and looping providers, music track management, indexed sound list. |
| `MirObjects/` | Client-side renderable objects mirroring server types (MonsterObject, PlayerObject, SpellObject, etc.) plus rendering-specific types (Effect, Damage, MapCode, Frames). |
| `Forms/` | WinForms windows: `CMain` (game window), `AMain` (launcher), `Config` (settings form). |

## Code Conventions

- **C# .NET 8**, SDK-style projects, no nullable reference types warnings for CS8618 (set to suggestion)
- **Indentation**: 4 spaces, tabs as spaces
- **Line endings**: CRLF
- **Braces**: Always use braces (enforced by `.editorconfig`)
- **`var`**: Never used (`csharp_style_var_* = false`) — always explicit types
- **Naming**: PascalCase for types/methods/properties, `I` prefix for interfaces
- **Expression-bodied members**: Only for properties, indexers, accessors, and lambdas — NOT for methods or constructors
- **Implicit object creation**: Allowed when type is apparent
- **Pattern matching**: Prefer pattern matching and switch expressions

## Key External Dependencies

- **SlimDX** — Direct3D9 wrapper (prebuilt DLL in `Components/`)
- **NAudio** — Audio playback (NuGet, Client project)
- **log4net 3.3.0** — Server logging
- **WinSCP 6.3.6** — FTP/SFTP for AutoPatcherAdmin

## Related Repositories

- [Crystal.Database](https://github.com/Suprcode/Crystal.Database) — Database files
- [Crystal.MapEditor](https://github.com/Suprcode/Crystal.MapEditor) — Map editor tool
- Community resources: [LOMCN Wiki](https://www.lomcn.net/wiki/index.php/Crystal), [Build Guide](https://www.lomcn.net/wiki/index.php/Getting_Started)
