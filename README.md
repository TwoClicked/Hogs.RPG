# 🐗 Hogs RPG

A persistent, Discord-native RPG bot built for the **Hogs Viking Rise** tribal community. Players level up, hunt, fight bosses, run dungeons, collect pets, forge relics, and trade — all without leaving their tribe's text channels.

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
- **Dungeon Bosses** — Solo instanced fights via button-driven UI (Attack / Heal / Flee)
- **Global Bosses** — Server-wide events with coordinated raid mechanics
- **Raid Integration** — Victory messages post automatically to the RPG feed channel

### 🐾 Pet System
- **Tier 1 & 2 Pets** — Equippable companions with passive bonuses and level progression
- **Tier 3 Evolution** — Combine all three Tier 2 pets via `/pet-evolve` to unlock the Primal Chimera
- **Pet Dungeons** — Solo dungeons unlocked at pet levels 15, 20, and 25 (3% rare drop rate)
- **Custom Names** — Players can rename their pets with Pet Snack items

### 💎 Relic System
- Unlock relics by spending Tier 1 Relic Shards (dropped from dungeons, global bosses, raids)
- Two equip slots per player; reroll bonus stats with additional shards
- Affinity-based stat bonuses with rank progression

### 🔨 Job Classes *(In Development)*
Four crafting classes — Alchemist, Cook, BlackSmith, Enchanter. Players choose two, but **BlackSmith + Enchanter is a prohibited combo** by design — forcing player-to-player trade for complete crafted items.

### 🛒 Gold Shop
- Button-driven ephemeral UI with category tabs
- Instant-delivery items: Double XP, Stamina Boost, Stamina Reset, Loot Crate, Energy Refill, Pet Snacks, Dungeon Reset, Pet Rename
- Live auction support with admin fulfillment commands

### 🎒 Player Progression
- XP-based leveling with stamina and energy systems
- Equipment slots with stat bonuses (ATK / DEF / HP)
- 9 tracked stat categories with `/mystats` leaderboard rankings
- XP Potion auto-use support

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

The bot is deployed on **Railway** from the `master` branch. Feature work should be done on feature branches (e.g. `feature/shop-system`) to avoid triggering premature production deploys.

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Apply EF Core migrations (ensure DATABASE_URL points to your local DB)
dotnet ef database update --project Hogs.RPG.Data --startup-project Hogs.RPG.Bot

# Run the bot
dotnet run --project Hogs.RPG.Bot
```

> ⚠️ **EF Core migration note:** If `GameDbContextFactory` points to localhost rather than the production DB, empty migrations may be generated. When in doubt, apply schema changes directly via SQL on Railway.

---

## 🗃️ Database

- **Provider:** PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Migrations:** Located in `Hogs.RPG.Data/Migrations/`
- **Context factory:** `GameDbContextFactory` — used by EF tooling

---

## 🛠️ Key Technical Notes

- **Button ID conflicts** — Wildcard pattern matching (`*_*_*`) in Discord.NET breaks when sub-category values contain underscores. Fix: use lowercase sub-category values in button IDs and title-case on retrieval.
- **Discord image previews** — Posting multi-boss content in a single message causes images to stack. Split into individual per-boss messages for inline image previews.
- **Discord CDN links** — CDN attachment URLs contain expiry parameters and will eventually break. Use permanent hosting or the bot's own embed system for pinned/permanent content.

---

## 📜 License

Private project — for internal use within the Hogs Viking Rise community.
