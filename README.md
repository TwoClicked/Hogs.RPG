# 🐗 Hogs RPG

A persistent, Discord-native RPG bot built for the **Hogs Viking Rise** tribal community. Players level up, hunt, fight bosses, run dungeons, collect pets, forge relics, brew potions, and trade — all without leaving their tribe's text channels.

---

## 📖 Overview

Hogs RPG is a fully embedded Discord bot experience — not a session-based Activity. All gameplay happens inline in community channels, creating ambient and persistent engagement across the server.

- **Platform:** Discord (text channels + slash commands)
- **Stack:** C# .NET 8 · Discord.NET 3.19 · PostgreSQL · Railway
- **Architecture:** Multi-project solution with strict layer separation

---

## 🏗️ Solution Structure

```
Hogs.RPG.sln
├── Hogs.RPG.Bot          # Discord bot entry point, slash commands, interaction modules
├── Hogs.RPG.Services     # Game logic, background timers, boss/dungeon/hunt services
├── Hogs.RPG.Data         # EF Core DbContext, repositories, PostgreSQL migrations
├── Hogs.RPG.Core         # Entities, enums, static game data, registries
└── Hogs.RPG.Shared       # Shared utilities and extension types
```

### Layer Conventions

| Layer | Location | Pattern |
|---|---|---|
| Entities | `Core/Entities` | Plain C# classes |
| Enums | `Core/Enums` | Strongly-typed enums |
| Static game data | `Core/GameData` | `static readonly` properties |
| Registries | `Core/Registries` | ID → definition dictionaries |
| Repositories | `Data/Repositories` | EF Core data access |
| Services | `Services/` | Business logic, stateful game services |
| Commands | `Bot/Commands` | Discord.NET `InteractionModuleBase` |

---

## ⚔️ Features

### 🗡️ Combat & Bosses
- **Daily Bosses** — Auto-spawn each day; community attacks collectively in the feed channel
- **Dungeon Bosses** — Solo instanced fights via button-driven UI (Attack / Heal / Flee) sent to DMs
- **Global Bosses** — Server-wide events with coordinated raid mechanics; rewards scale with damage dealt
- **Raid Integration** — Victory and death messages post automatically to the RPG feed channel

### 🏰 Dungeons
- Solo instanced fights across multiple floors scaling up to a boss
- 2 hour cooldown between runs; reset via shop
- Health Potion healing via in-dungeon button
- Alchemist potion buffs (dodge, damage reduction, revival, gold boost) carry into the session
- Pet Dungeons unlock at pet levels 15, 20 and 25 with exclusive rare drops (3% drop rate)

### ⚔️ Raids
- 3-player group content with Tank / DPS / Healer roles
- 5 tiers requiring crafted raid keys; all three players act simultaneously each round
- Raid keys crafted via `/raidkey craft` using tiered hunt materials
- Stat potions (Berserker Brew, Raid Elixir, Dragon's Blood etc.) apply at raid start via StatService

### 🐾 Pet System
- **Tier 1 & 2 Pets** — Equippable companions with passive bonuses and level progression
- **Tier 3 Evolution** — Combine all three Tier 2 pets via `/pet-evolve` to unlock the Primal Chimera
- **Hunting Companion** — Unlocked via Ashwood Trail; permanent +5% XP, +5% materials, +3% rare drops
- **Custom Names** — Players can rename their equipped pet via the shop

### 💎 Relic System
- Two equip slots per player; unlock relics with Relic Shards from dungeons, bosses and raids
- Affinity-based stat bonuses (ATK%, DEF%, HP%, XP%, lifesteal, executioner)
- Reroll bonus stats with additional shards via `/relic-reroll`
- Relic bonuses feed directly into StatService and apply across all combat systems

### 🔨 Blacksmith Job Class
- Full ore → bar → weapon pipeline: `/gather mine` → `/blacksmith smelt` → `/blacksmith craft`
- RuneScape-inspired 1–99 Smithing level with N²×50 XP curve
- 13 weapon types across 7 tiers (Bronze through Dragon)
- Every forged item auto-lists in the player's personal NPC shop
- NpcShopService fires daily at 12 UTC — buys items, DMs player receipt, respects 5,000g daily cap
- Dragon Crystal — 0.03% drop rate when mining at Smithing level 99; forges into Dragon Blade (2,500g)
- Smithing level tracked on `/profile`, `/mystats` and leaderboard with feed announcements on level up

### 🧪 Alchemist Job Class
- Ingredients from two sources: swamp gathering (`/gather swamp`) and alchemy monster hunts
- 24 potions across 15 levels (1–99) using same N²×50 XP curve as Smithing
- Two active buff slots — one Stat buff (ATK/DEF/HP) and one Utility buff (XP, loot, dungeon) simultaneously
- 5 potion daily use cap; instant potions (stamina, trail, revival) bypass the slot system
- Stat buffs apply across dungeons, pet dungeons, boss fights and raids via StatService
- Dungeon-scoped buffs (dodge, damage reduction, first strike, revival, gold boost) cached at dungeon start
- Blacksmith's Elixir (Lv 60) — cross-class potion; NPCs buy max stock at next 12 UTC NPC run
- Alchemist level tracked on `/profile`, `/mystats` and leaderboard with feed announcements on level up

### 🏹 Hunting & Gathering
- 5 hunt tiers (Forest → Wild → Deep → Storm → Mythic) with tiered material drops
- Alchemy hunt category — Swamp Serpent, Corrupted Golem, Shadow Wraith, Elder Alchemist
- Hunting gear set bonuses, hunting pet bonus and alchemist loot/XP potions all stack
- 3 gather zones: Forest (alchemy materials), Mine (smithing ores), Swamp (alchemist ingredients)
- Swamp and mine gathering grant Alchemy and Smithing XP respectively
- Dragon Crystal rolled separately at mine for Smithing level 99 players (0.03% per energy)

### 🏕️ Ashwood Trail
- 3 daily trail runs sent to DMs with 8–15 random events per run
- Event types: Fresh Tracks, Snare Set, Ambush Encounter, Tracker's Gamble, Rare Sighting, Legendary Encounter
- Earn Tracker Tokens spent in the trail shop for exclusive rewards
- Trail reset available in RPG Perks shop for 5,000 gold (once per day)

### 🛒 Gold Shop
- Button-driven ephemeral UI with category tabs (VR Ranks, Discord Rewards, RPG Perks)
- Instant-delivery RPG Perks: Double XP, Stamina Boost, Stamina Reset, Energy Refill, Loot Crate, Dungeon Reset, Pet Dungeon Reset, Raid Reset, Trail Reset, Pet Snacks (S/M/L), Pet Rename
- Live auction support with admin fulfilment commands
- Confirm/cancel flow on all purchases to prevent misclicks

### 🏪 Player Market
- Player-to-player auction system for items, pets and relics
- Base price + optional buyout price; seller closes listing manually
- Outbid players refunded instantly with DM notification including bid-again link
- Market channel shows live listings with current bid and seller

### 🎒 Player Progression
- XP-based leveling (max 50) with stat gains per level
- 9 equipment slots with ATK/DEF/HP bonuses; gear set save/load system
- Smithing level (1–99) and Alchemist level (1–99) tracked separately
- 11 leaderboard categories: Gold, Level, Gear Score, Dungeons, Raids, Boss Damage, Pet Power, Deaths, Trails, Smithing Level, Alchemist Level
- Basic alchemy: XP Potions and Health Potions craftable by everyone via `/alchemy craft`

---

## 🔧 Configuration

Environment variables required at runtime (set in Railway or `.env`):

```
DISCORD_TOKEN=
DATABASE_URL=          # PostgreSQL connection string
FEED_CHANNEL_ID=1485357755433750549
AUCTION_CHANNEL_ID=1491919891090112633
ADMIN_ROLE_ID=1483528182106685691
VR_RESOURCE_ROLE_ID=1222668156271591485
```

---

## 🚀 Deployment

The bot is deployed on **Railway** from the `master` branch. Feature work should be done on feature branches (e.g. `feature/blacksmith`) to avoid triggering premature production deploys.

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
- **Production schema changes:** Applied directly via Railway SQL console
- **Context factory:** `GameDbContextFactory` — used by EF tooling

---

## 🛠️ Key Technical Notes

- **EF migrations vs Railway** — Migrations are never run against production. All `ALTER TABLE` and `CREATE TABLE` statements are applied manually via Railway's SQL console.
- **BackgroundService singletons** — `BossScheduler`, `NpcShopService`, `TradeCleanupService`, `RaidTimerService` are registered as singletons and explicitly started in `Program.cs` inside the `ReadyAsync` handler.
- **Discord embed 25-field limit** — Profile embeds are carefully managed to stay under the hard limit. Fields are collapsed where necessary.
- **Button ID conflicts** — Wildcard pattern matching (`*_*_*`) in Discord.NET breaks when sub-category values contain underscores. Fix: use lowercase sub-category values in button IDs and title-case on retrieval.
- **PostgreSQL advisory locks** — `pg_advisory_xact_lock` used for race conditions in concurrent button-press scenarios (raids, boss attacks).
- **`AsNoTracking`** — Required in raid/combat systems to prevent stale EF cache reads when multiple players interact simultaneously.
- **`UpdateAsync` on button interactions** — Can cause buttons to disappear for some players. Prefer `DeferAsync` with round-number validation instead.
- **Stat buff pipeline** — All ATK/DEF/HP buffs (gear, pet, relic, alchemist potions) flow through `StatService.CalculateStatsAsync`. Adding new stat sources only requires updating StatService.
- **Dungeon potion buffs** — Scoped utility buffs (dodge, damage reduction, first strike, revival, gold boost) are read from the player once at `StartDungeonAsync` and cached in `ActiveDungeon` to avoid DB calls during combat.
- **NPC shop cap** — `NpcShopService` uses `continue` (not `break`) when an expensive item can't fit in the remaining daily cap, so cheaper items listed after it still get purchased.
- **Discord CDN links** — CDN attachment URLs contain expiry parameters. Use permanent hosting or the bot's embed system for pinned content.
- **Discord image previews** — Posting multi-boss content in a single message causes images to stack. Split into individual per-boss messages for inline image previews.
- **AutocompleteCache** — Generic cache keyed by `ulong` userId with configurable TTL. Invalidated automatically by `InventoryService.GiveItemAsync` and `TakeItemAsync` and by `PlayerRepository.UpdatePlayerAsync`.

---

## 📜 License

Private project — for internal use within the Hogs Viking Rise community.
