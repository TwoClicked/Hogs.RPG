# 🐗 Hogs RPG

A persistent, Discord-native RPG bot built for the **Hogs Viking Rise** tribal community. Players level up, hunt, fight bosses, run dungeons and raids, climb the Tower of Doom, battle in the Colosseum, collect pets, forge relics, enhance gear, brew potions, and trade — all without leaving their tribe's text channels.

---

## 📖 Overview

Hogs RPG is a fully embedded Discord bot experience — not a session-based Activity. All gameplay happens inline in community channels and DMs, creating ambient and persistent engagement across the server.

- **Platform:** Discord (text channels, DMs, slash commands, button-driven UIs)
- **Stack:** C# .NET 8 · Discord.NET 3.19 / Discord.Interactions · EF Core · PostgreSQL · Railway
- **Architecture:** Multi-project solution with strict layer separation
- **Size:** ~41,000 lines of C# across 320+ files

---

## 🏗️ Solution Structure

```
Hogs.RPG.sln
├── Hogs.RPG.Bot          # Discord bot entry point, slash commands, interaction modules, autocomplete
├── Hogs.RPG.Services     # Game logic, schedulers/background services, combat systems
├── Hogs.RPG.Data         # EF Core DbContext, repositories, local migrations
├── Hogs.RPG.Core         # Entities, enums, static game data, registries
└── Hogs.RPG.Shared       # Shared utilities and extension types
```

### Layer Conventions

| Layer | Location | Pattern |
|---|---|---|
| Entities | `Core/Entities` | Plain C# classes, grouped per system (`*Objects` folders) |
| Enums | `Core/Enums` | Strongly-typed enums, grouped per system |
| Static game data | `Core/GameData` | `static readonly` definitions (bosses, dungeons, gear, recipes, rates) |
| Registries | `Core/Registries` | ID → definition dictionaries |
| Repositories | `Data/Repositories` | EF Core data access |
| Services | `Services/` | Business logic, stateful game services, schedulers |
| Commands | `Bot/Commands` | Discord.NET `InteractionModuleBase` |

---

## ⚔️ Features

### 🌍 Global Bosses
- 9 server-wide bosses on a daily schedule: Aurelius, Gravelmaw, Primordial Serpent, Xerathul, Two Tier Tyr, King Thorlak, Click the Punisher, Gullveig-Huld and Sir Racha, the Saucecerer
- Community attacks collectively in the feed channel; rewards scale with damage dealt
- Each boss drops its own slot of **Global Boss Gear** (9 slots total) — the gear that feeds the enhancement system
- Pre-boss warning automatically swaps players into their saved combat gear set

### 🏰 Dungeons
- 13 solo dungeons from Lv 5 to Lv 40, from Rotwood Hollow to the endgame trio Bonecarver's Descent (Lv 36), The Warcaller's Siege (Lv 38) and The Ashen End (Lv 40)
- Button-driven combat (Attack / Heal / Flee) sent to DMs, with unique boss behaviours and drop tables
- 2-hour cooldown between runs (reduced via achievement milestones); reset via shop
- Alchemist potion buffs (dodge, damage reduction, first strike, revival, gold boost) carry into the session
- **Pet Dungeons** — 6 pet-focused dungeons (Blazewing's Gorge through Ember ClankaVille) with exclusive rare drops

### ⚔️ Raids
- 3-player cooperative content with Tank / DPS / Healer role abilities; all players act simultaneously each round
- 6 tiers (Lv 10–35): Wafflera, Unc Click, Buldiablo, Lysandra, Zarokh and The Demolisher
- Daily cap of 5 raids; Blackstones awarded per tier cleared
- **Solo Raid** (`/raid-solo`) — skip the lobby and play all three roles yourself, at a higher key cost
- Raid keys crafted via `/raidkey craft` from tiered hunt materials, or converted via `/convert-materials` (2,500 materials → 1 key)
- Stat potions apply at raid start via `StatService`; 5-minute round timeout handled by `RaidTimerService`

### 🗼 Tower of Doom
- Infinite-floor climb in **solo** or **duo** mode (`/tower solo`, `/tower duo`)
- Elite floors every 5, milestone bosses every 25, with random buffs and debuffs along the way
- **Sigils** — 6 permanent stackable bonuses (damage, defense, max HP, lifesteal, gold, XP; max 3 stacks each)
- Rewards: gold per floor, flat XP/pet XP, and Blackstones
- Separate Solo and Duo leaderboards
- Resilient retry/refund logic when Discord API calls fail mid-run

### 🏟️ Colosseum
- Daily PvP tournament opening at 12:00 UTC: 16-slot double-elimination bracket
- Sandboxed AP economy — buy gear, pets and buffs inside the event without touching your real character
- Simultaneous combat resolution; bots pad the field when fewer than 16 players sign up
- Rewards in Cron Stones (4 base / 7 runner-up / 10 winner)

### 🔨 Gear Enhancement
- Global Boss Gear can be enhanced **+1 → +15**, then through the prestige tiers **PRI → DUO → TRI → TET → PEN** (20 levels total)
- Materials: Blackstones, Cron Stones, Upgrade Pieces, Infuse Crystals and Concentrated Blackstones
- Enhancement level is tracked per slot; bonuses only apply while the matching boss gear piece is equipped
- `/enhance salvage` destroys spare boss gear for Blackstones
- Prestige-tier successes are announced in the feed

### 🐾 Pet System
- **Tier 1 & 2 Pets** — Equippable companions with passives and level progression
- **Tier 3 Evolution** — Combine Tier 2 pets via `/pet-evolve` to unlock Tier 3 pets
- Reroll passives (`/pet-reroll`), bag/equip by instance ID, rename via the shop
- **Companions** — Always-on passive bonuses (e.g. the Hunting Companion: +5% XP, +5% materials, +3% rare drops); view with `/companion`

### 💎 Relic System
- Two equip slots per player; unlock with Relic Shards from dungeons, bosses and raids
- 5 ranks with role affinities and stat bonuses (ATK%, DEF%, HP%, XP%, lifesteal, executioner)
- Upgrade (`/relic-upgrade`) and reroll (`/relic-reroll`) with additional shards
- Relic bonuses feed directly into `StatService` across all combat systems

### 🧰 Gear Sets
- Save and load named loadouts with `/gearset save` and `/gearset load`
- Sets snapshot equipped gear, relics and pet
- Automatically swapped in before global boss spawns

### 🏆 Achievements
- 129 achievements across every system
- Milestone bonuses unlock as the count grows (flat stats, shorter dungeon cooldown and more)
- Retroactive catch-up — existing progress is credited on a player's first relevant action
- Dedicated achievement leaderboard (`/achievements leaderboard`)

### 🔨 Blacksmith Job Class
- Full ore → bar → weapon pipeline: `/gather mine` → `/blacksmith smelt` → `/blacksmith craft`
- RuneScape-inspired 1–99 Smithing level with N²×50 XP curve
- 13 weapon types across 7 tiers (Bronze through Dragon)
- Every forged item auto-lists in the player's personal NPC shop
- `NpcShopService` fires daily at 12 UTC with randomized demand — buys items, DMs a receipt, respects the 5,000g daily cap
- Dragon Crystal — 0.03% drop rate when mining at Smithing 99; forges into the Dragon Blade

### 🧪 Alchemist Job Class
- Ingredients from swamp gathering (`/gather swamp`) and alchemy monster hunts
- 24 potions across levels 1–99 using the same N²×50 XP curve as Smithing
- Two active buff slots — one Stat buff (ATK/DEF/HP) and one Utility buff (XP, loot, dungeon)
- 5 potion daily use cap; instant potions (stamina, trail, revival) bypass the slot system
- Blacksmith's Elixir (Lv 60) — cross-class potion; NPCs buy max stock at the next 12 UTC run

### 🏹 Hunting & Gathering
- 5 hunt tiers (Forest → Wild → Deep → Storm → Mythic) with tiered material drops
- Alchemy hunt category — Swamp Serpent, Corrupted Golem, Shadow Wraith, Elder Alchemist
- 3 gather zones: Forest (alchemy materials), Mine (smithing ores), Swamp (alchemist ingredients)
- Hunting gear set bonuses, companion bonuses and alchemist loot/XP potions all stack

### 🏕️ Ashwood Trail
- 3 daily DM-based trail runs (`/trail run`) with 8–15 random events per run
- Event types: Fresh Tracks, Snare Set, Ambush Encounter, Tracker's Gamble, Rare Sighting, Legendary Encounter
- Earn Tracker Tokens to spend at the Tracker's Camp shop, including the Hunter gear set (`/hunter-gear`)

### 🛒 Gold Shop
- Button-driven ephemeral UI with separate tabs: **RPG Perks**, **Resets**, **Enhance Items**, **Discord Rewards**, **VR Ranks** and **VR Resources**
- Instant-delivery perks (Double XP, stamina/energy refills, loot crates, pet snacks, pet rename) and resets (dungeon, pet dungeon, raid, trail)
- Live auction support with admin fulfilment commands
- Confirm/cancel flow on all purchases to prevent misclicks

### 🏪 Player Market & Trading
- Player-to-player auction house for items, pets and relics (`/market`)
- Base price + optional buyout; outbid players refunded instantly with a DM and bid-again link
- Direct player trades with items, gold, pets and relics, confirmed by both sides (`/trade`)

### 🎒 Player Progression
- XP-based leveling (max 40) with stat gains per level
- 9 equipment slots (Main Hand, Off Hand, Helmet, Body, Legs, Gloves, Boots, Ring, Amulet)
- Smithing (1–99) and Alchemist (1–99) levels tracked separately
- Auto-updating leaderboard channel with 15 categories: Gold, Level, Gear Score, Dungeons, Raids, Boss Damage, Enhance Attempts, Deaths, Trails, Smithing Level, Alchemist Level, Achievements, Solo Tower, Duo Tower, Gold Spent

---

## 🔧 Configuration

Environment variables required at runtime (set in Railway or `.env`):

```
HOGS_RPG_TOKEN=        # Discord bot token
DATABASE_URL=          # PostgreSQL connection string
```

Channel, role and guild IDs (feed, auction/market, raid, trade, tower, leaderboard, admin role, VR resource role) are currently hardcoded as `const ulong` values in their respective modules and services.

---

## 🚀 Deployment

The bot is deployed on **Railway** from the `master` branch using the included `Dockerfile` (.NET 8 SDK build → .NET 8 runtime image). Feature work should be done on feature branches to avoid triggering premature production deploys.

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Apply EF Core migrations (ensure DATABASE_URL points to your local DB)
dotnet ef database update --project Hogs.RPG.Data --startup-project Hogs.RPG.Bot

# Run the bot
dotnet run --project Hogs.RPG.Bot
```

> ⚠️ **EF Core migration note:** Migrations do not run against Railway production. All schema changes are applied as direct SQL via the Railway console.

---

## 🗃️ Database

- **Provider:** PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Migrations:** Located in `Hogs.RPG.Data/Migrations/` — for local use only
- **Production schema changes:** Applied directly via the Railway SQL console
- **Data fixes:** `UPDATE` statements go through bot admin commands, since the Railway console auto-appends `LIMIT`
- **Context factory:** `GameDbContextFactory` — used by EF tooling

---

## 🛠️ Key Technical Notes

- **EF migrations vs Railway** — Migrations are never run against production. All `ALTER TABLE` and `CREATE TABLE` statements are applied manually via Railway's SQL console.
- **Background services** — `BossScheduler`, `NpcShopService`, `TradeCleanupService`, `RaidTimerService`, `TowerService`, `ColosseumScheduler` and `LeaderboardUpdater` are registered as singletons and explicitly started in `Program.cs` inside the `ReadyAsync` handler.
- **Discord API resilience** — Tower and Colosseum retry failed Discord calls (3 attempts, 5s delay) and refund players if a run can't continue.
- **Discord embed limits** — Profile embeds stay under the 25-field limit; Colosseum logs are trimmed to the 4,000-character description limit.
- **Button ID conflicts** — Wildcard pattern matching (`*_*_*`) in Discord.NET breaks when sub-category values contain underscores. Fix: use lowercase sub-category values in button IDs and title-case on retrieval.
- **PostgreSQL advisory locks** — `pg_advisory_xact_lock` is used for race conditions in concurrent button-press scenarios (raids, boss attacks).
- **`AsNoTracking`** — Required in raid/combat systems to prevent stale EF cache reads when multiple players interact simultaneously.
- **`UpdateAsync` on button interactions** — Can cause buttons to disappear for some players. Prefer `DeferAsync` with round-number validation instead.
- **Stat pipeline** — All ATK/DEF/HP sources (gear, enhancement, pet, relic, sigils, achievements, alchemist potions) flow through `StatService.CalculateStatsAsync`. Adding a new stat source only requires updating `StatService`.
- **Enhancement slot map** — `EnhancementSlotMap` is the single source of truth tying each equipment slot to its boss gear item, Player column and material IDs; `EnhancementLevelLabels` owns the 0–20 → `+N` / PRI–PEN display text.
- **Dungeon potion buffs** — Scoped utility buffs are read once at `StartDungeonAsync` and cached in `ActiveDungeon` to avoid DB calls during combat.
- **NPC shop cap** — `NpcShopService` uses `continue` (not `break`) when an expensive item can't fit in the remaining daily cap, so cheaper items still get purchased.
- **Discord CDN links** — CDN attachment URLs contain expiry parameters. Use permanent hosting or the bot's embed system for pinned content.
- **Discord image previews** — Posting multi-boss content in a single message causes images to stack. Split into individual per-boss messages for inline previews.
- **AutocompleteCache** — Generic cache keyed by `ulong` userId with configurable TTL. Invalidated automatically by `InventoryService.GiveItemAsync` / `TakeItemAsync` and `PlayerRepository.UpdatePlayerAsync`.

---

## 📜 License

Private project — for internal use within the Hogs Viking Rise community.
