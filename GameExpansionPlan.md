# Runefell Crystal Conquest — Game Expansion Plan

## Context

The game currently has a 9-wave flat campaign with one enemy type (orc) that scales per wave, 9 boss prefabs (5 unique + 4 crystal variants), and a single strategy loop (break crystal → get clones → shoot). The game feels repetitive — same visuals, same strategy every wave, gems/runes feel pointless, and there's no real progression arc.

This plan restructures the game into a 3-Act × 5-Wave campaign with visual variety per act, a gauntlet weapon system that changes gameplay each wave, randomized orb economy, between-wave buff shop, permanent upgrades (Rune Forge), and IAP monetization. All built on existing systems with minimal rewrites.

---

## Phase 10: Act System + Wave Restructure (3 Acts × 5 Waves)

**Goal:** Replace 9 flat waves with 3 Acts of 5 waves. Each act reuses existing enemies/bosses with different materials + stat scaling. Retire the 4 Crystal-variant boss scripts.

### Wave/Boss Structure
| Wave | Boss Owner | Additional Bosses (random from prior) |
|------|-----------|--------------------------------------|
| 1 | Orc Boss | — |
| 2 | Troll | + Orc Boss |
| 3 | Wraith | + any from W1-2 |
| 4 | Sorcerer | + any from W1-3 |
| 5 | Crystal Necromancer | + any from W1-4 |

### Act Scaling
| Act | Theme | Materials | HP Mult | Speed Mult | Scale Mult |
|-----|-------|-----------|---------|------------|------------|
| 1 | Orc Horde | Current (green/brown) | 1.0x | 1.0x | 1.0x |
| 2 | Undead Legion | Dark/gray/decayed | 1.5x | 1.3x | 1.1x |
| 3 | Crystal Corruption | Glowing purple/crystal | 2.5x | 1.6x | 1.2x |

### Files Modified
- **`WaveScalingConfig.cs`** — Add `int actIndex` and `int[] bossPrefabIndices` (array of boss prefab indices for this wave, index 0 = owner, rest = additional) to `WaveParams`. Add `float hpMultiplier`, `float speedMultiplier`, `float scaleMultiplier` per act grouping.
- **`WaveSpawner.cs`** — `bossPrefabs[]` shrinks from 9 to 5 slots (Orc, Troll, Wraith, Sorcerer, CrystalNecromancer). Refactor `SpawnBosses()` to use `wp.bossPrefabIndices` instead of wave index. Support spawning 2+ bosses from different prefab indices. Add `currentActIndex` property.
- **`Enemy.cs`** — Add `SetActMaterials(Material actMaterial)` to swap renderer materials at spawn. Act material applied in `SpawnScaledOrc()`.
- **`DefaultCampaign.asset`** — Reconfigure to 15 waves (3 acts × 5) with per-wave boss indices and act multipliers.
- **`UIManager.cs`** — Wave display changes to "Act X — Wave Y / 5".
- **`PooledProjectile.cs`** — Remove `CrystalSorcererBossBehavior` shield check (the base `SorcererBossBehavior` check handles it).

### New Files
- **`ActConfig.cs`** — ScriptableObject: `string actName`, `Material enemyMaterial`, `Material[] bossMaterials` (indexed 0-4), `float hpMult`, `float speedMult`, `float scaleMult`.
- **`Act1_OrcHorde.asset`**, **`Act2_UndeadLegion.asset`**, **`Act3_CrystalCorruption.asset`** — ActConfig instances.

### Crystal-Variant Boss Retirement
- CrystalOrc/Troll/Wraith/Sorcerer BossBehavior scripts and prefabs are no longer referenced by the campaign. Keep files in project but unused. Act 3's crystal theme is achieved via ActConfig materials on the base 5 bosses.
- CrystalNecromancer keeps its unique behavior (resurrection mechanic) — it's the Wave 5 boss in all acts.

### Testing
- Play all 15 waves. Verify correct boss spawns per wave (W1: 2 Orc bosses, W3: Wraith + 1 random from {Orc, Troll}, etc.).
- Verify Act 2/3 enemies have visually distinct materials.
- Verify stat scaling per act (Act 2 orcs have ~1.5x HP).
- Verify Sorcerer shield still works in all acts.

**Dependencies:** None — foundation for everything else.

---

## Phase 11: Crystal Orb Randomization

**Goal:** Crystal orbs come in 3 types with visual distinction and different rewards.

### Orb Types
| Type | Color | Drop Rate | Reward |
|------|-------|-----------|--------|
| Clone (Blue) | Cyan (0.6, 0.85, 1.0) | 60% | Clone shooter (current behavior) |
| Gem (Gold) | Gold (1.0, 0.85, 0.3) | 25% | +2 Gems |
| Rune (Purple) | Purple (0.7, 0.3, 1.0) | 15% | +1 Rune |

### Files Modified
- **`CrystalBall.cs`** — Add `OrbType` enum (Clone, Gem, Rune). Add `SetOrbType(OrbType)` method that sets reward behavior + material color. `Crack()` branches on type: Clone → `AddCloneShooter()`, Gem → `PlayerWallet.AddGems()`, Rune → `PlayerWallet.AddRunes()`.
- **`CrystalBallSpawner.cs`** — In spawn method, roll weighted random for orb type. Call `ball.SetOrbType()` before activation. Add serialized weight/reward fields.
- **`CurrencyEarner.cs`** — Remove alternating gem/rune logic from `HandleCrystalBallCollected()` (orbs now self-award).

### Testing
- Observe orb spawns over several minutes. Verify ~60/25/15 distribution.
- Verify blue orbs grant clones, gold orbs add gems, purple orbs add runes.
- Verify orb colors are clearly distinguishable during gameplay.

**Dependencies:** None — can be built in parallel with Phase 10.

---

## Phase 12: Gauntlet Weapon System

**Goal:** HeroCrystal encases a gauntlet weapon per wave. Breaking it grants elemental projectile effects for the wave.

### Gauntlet Tiers
| Gauntlet | Wave | Projectile Color | Effect |
|----------|------|-----------------|--------|
| Green "Verdant Fist" | 1 | Green (0.2, 0.9, 0.3) | Poison DoT (3 dmg/sec for 3s) |
| Blue "Frost Grip" | 2 | Blue (0.3, 0.7, 1.0) | Slow 40% for 2s, every 5th shot freezes AoE 1s |
| Red "Ember Claw" | 3 | Red/Orange (1.0, 0.4, 0.1) | AoE splash (2u radius), 2x boss damage |
| Crystal "Prism Fury" | 5 | Rainbow cycling | All effects combined, highest multipliers |
| (Wave 4) | 4 | Player's choice or Red | Configurable per WaveParams |

### Files Modified
- **`WaveScalingConfig.cs`** — Add `GauntletType gauntletType` enum field to `WaveParams`.
- **`HeroCrystal.cs`** — Add `currentGauntletType` field set by `SetWaveScaling()`. Fire new event `OnGauntletRevealed(GauntletType)` when crystal breaks (alongside existing `OnCrystalBroken`). Add gauntlet color glow to crystal pieces before breaking.
- **`CrystalBuffManager.cs`** — Rename conceptually to handle gauntlet lifecycle. On `OnGauntletRevealed`, apply gauntlet-specific stats to `PlayerShooter` and set projectile `DamageType`. On wave end/buff expire, clear gauntlet.
- **`PlayerShooter.cs`** — Add `activeGauntletType` field. Add `SetGauntlet(GauntletType, GauntletConfig)` method. Projectile spawning applies gauntlet color to projectile material and sets `PooledProjectile.damageType`.
- **`PooledProjectile.cs`** — Add `DamageType` enum (Normal, Poison, Frost, Fire, Prism). In `OnTriggerEnter`, apply status effects to enemies based on `damageType`. Poison: add `StatusEffect` component. Frost: apply speed debuff. Fire: `Physics.OverlapSphere` AoE damage. Add `bossDamageMultiplier` field.
- **`Enemy.cs`** — Add `float moveSpeedMultiplier = 1f` for frost slow. Add `ApplyPoison(float dps, float duration)` and `ApplyFrost(float slowAmount, float duration)`. Poison ticks in `Update()`.

### New Files
- **`GauntletConfig.cs`** — ScriptableObject: `GauntletType type`, `string displayName`, `Color projectileColor`, `float damageMultiplier`, `float bossDamageMultiplier`, `float effectDuration`, `float effectValue`, `int unlockCostRunes`.
- **`StatusEffect.cs`** — MonoBehaviour added to enemies on hit. Manages poison tick + frost slow with auto-removal after duration. Stacks refresh duration, don't stack damage.
- **`GauntletVFX.cs`** — Cinematic: crystal shatters → gauntlet object lerps from crystal to player over ~0.8s → brief flash → projectile color changes. Uses `Time.unscaledDeltaTime`.

### Testing
- Break crystal each wave, verify correct gauntlet type activates.
- Green: enemies take DoT (health ticks down after hit).
- Blue: enemies slow visibly, periodic freeze AoE.
- Red: hits cause splash damage to nearby enemies, bosses take 2x.
- Crystal: all effects combined.
- Verify gauntlet expires at wave end. Verify clone projectiles also get gauntlet effects.

**Dependencies:** Phase 10 (wave-to-gauntlet mapping via WaveParams).

---

## Phase 13: Between-Wave Buff Shop

**Goal:** After each wave, player picks 1 of 3 random buff cards (or skips). Buffs persist for the rest of the run.

### Buff Cards
**Gem-Tier (50-200 gems):**
- Clone Surge: +2 clone slots this run
- Thick Skin: +25 max HP
- Quick Hands: +15% fire rate this run
- Gem Magnet: orbs auto-pull toward player next wave

**Rune-Tier (5-15 runes, stronger):**
- Gauntlet Mastery: keep current gauntlet into next wave
- Boss Bane: 2x damage to bosses next wave
- Phoenix Down: one free auto-revive next wave
- Double Orbs: orb spawn rate doubled next wave

### Files Modified
- **`WaveSpawner.cs`** — After `OnWaveCompleted`, fire `OnBuffShopRequested(waveIndex)`. Wait for `OnBuffShopClosed` callback before proceeding to breather. Skip shop after final wave (wave 5 of each act).
- **`GameManager.cs`** — Add `GameState.BuffShop`. Pause game during shop (`Time.timeScale = 0`).
- **`PowerupManager.cs`** — Add run-level buff tracking: `int bonusCloneSlots`, `int bonusHP`, `float bonusFireRate`, `bool hasGauntletMastery`, `bool hasBossBane`, `bool hasPhoenixDown`, `bool hasDoubleOrbs`. Reset on game restart. Query methods for each.
- **`ShooterManager.cs`** — `maxShooters` adds `PowerupManager.BonusCloneSlots`.
- **`ShooterHealth.cs`** — `maxHP` adds `PowerupManager.BonusHP`. Check `HasPhoenixDown` on death for auto-revive.
- **`CrystalBallSpawner.cs`** — Check `PowerupManager.HasDoubleOrbs` to halve spawn interval.

### New Files
- **`BuffShopConfig.cs`** — ScriptableObject: array of `BuffCardDef` (name, description, cost, currency type, buff type, value, icon).
- **`BuffShopPanel.cs`** — UI panel: shows 3 random cards from pool, pick one or skip. Deducts currency. Fires `OnBuffShopClosed`. Uses `Time.unscaledDeltaTime` for animations. Ad-reroll button (watch ad → redraw 3 cards).

### Testing
- Complete a wave, verify shop overlay appears with 3 cards.
- Buy a card, verify currency deducted and buff applied.
- Skip, verify proceeds to next wave.
- Verify Phoenix Down triggers on death (auto-revive once).
- Verify Gauntlet Mastery carries weapon to next wave.
- Verify shop does NOT appear after Act's final wave.
- Verify all buffs reset on game restart.

**Dependencies:** Phase 10 (wave timing), Phase 12 (Gauntlet Mastery needs gauntlet system).

---

## Phase 14: Rune Forge (Permanent Upgrades)

**Goal:** Between-run upgrade screen. Spend runes to permanently unlock gauntlets, boost base stats, and unlock Acts 2-3.

### Upgrade Tree
| Category | Upgrade | Tiers | Cost (Runes) |
|----------|---------|-------|-------------|
| Gauntlets | Unlock Green | 1 | 30 |
| | Unlock Blue | 1 | 60 |
| | Unlock Red | 1 | 100 |
| | Unlock Crystal | 1 | 200 |
| Stats | Base HP (+10/tier) | 5 | 10, 20, 35, 50, 75 |
| | Base Fire Rate (+0.5/tier) | 5 | 10, 20, 35, 50, 75 |
| | Clone Cap (+1/tier) | 3 | 15, 30, 60 |
| Acts | Unlock Act 2 | 1 | 50 |
| | Unlock Act 3 | 1 | 150 |

### Files Modified
- **`PlayerWallet.cs`** — Raise `SoftCap` to 99999 (IAP needs room). Add `AddGemsUncapped(int)` for IAP.
- **`GameManager.cs`** — Add `GameState.RuneForge`. Navigate: GameOver/Victory → Rune Forge → Play Again.
- **`WaveSpawner.cs`** — After Act 1 victory, check `PlayerProgression.IsActUnlocked(2)`. If locked, show victory + "Unlock Act 2" prompt. If unlocked, continue to Act 2.
- **`ShooterManager.cs`** — Read `PlayerProgression.BaseCloneCap` on start, add to `maxShooters`.
- **`ShooterHealth.cs`** — Read `PlayerProgression.BaseHP` on start, add to `maxHP`.
- **`PlayerShooter.cs`** — Read `PlayerProgression.BaseFireRate` on start, add to `_baseFireRate`. If player has permanently unlocked gauntlet, equip it at wave 1 start (before HeroCrystal is broken).

### New Files
- **`PlayerProgression.cs`** — Static class wrapping PlayerPrefs for all progression: `bool HasGauntlet(type)`, `int GetStatLevel(stat)`, `bool IsActUnlocked(act)`, purchase methods. Keys prefixed `Runefell_Prog_`.
- **`RuneForgeConfig.cs`** — ScriptableObject: arrays of upgrade definitions per category with costs per tier.
- **`RuneForgePanel.cs`** — Full-screen UI. Shows upgrade categories with current level, cost, buy button. Grayed out if unaffordable. Rune balance displayed.

### Testing
- Open Rune Forge, verify all upgrades with correct costs.
- Purchase stat upgrade, restart, verify base stat increased.
- Unlock gauntlet, start run, verify equipped at wave 1.
- Verify Act 2 locked until purchased. Purchase, verify progression.
- Verify all unlocks persist across app restart (PlayerPrefs).

**Dependencies:** Phase 10 (acts), Phase 12 (gauntlets).

---

## Phase 15: IAP Store + Rewarded Ads

**Goal:** Monetize with gem/rune packs, starter pack, and expanded rewarded ad placements.

### IAP Products
| Product | Price | Reward |
|---------|-------|--------|
| Gem Pack S | $0.99 | 500 Gems |
| Gem Pack M | $4.99 | 3,000 Gems |
| Gem Pack L | $9.99 | 8,000 Gems |
| Rune Pack S | $1.99 | 20 Runes |
| Rune Pack M | $4.99 | 60 Runes |
| Rune Pack L | $9.99 | 150 Runes |
| Starter Pack (one-time) | $2.99 | Blue Gauntlet + 1,000 Gems + 20 Runes |

### Rewarded Ad Placements
- **Free Revive** (death screen) — already exists via RevivePanel
- **Double Wave Gems** (breather) — extend BreatherAdOffer
- **Shop Reroll** (buff shop) — redraw 3 cards
- **Bonus Currency** (breather) — existing BreatherAdOffer (+5 gems/runes)

### Files Modified
- **`AdManager.cs`** — Add placement-specific methods with cooldowns.
- **`PlayerWallet.cs`** — `AddGemsUncapped()` for IAP that bypasses soft cap.
- **`RevivePanel.cs`** — Update ad button to use new placement method.
- **`BreatherAdOffer.cs`** — Add "Double Gems" ad offer alongside existing currency offer.
- **`BuffShopPanel.cs`** (Phase 13) — Add "Reroll (Ad)" button.

### New Files
- **`IAPConfig.cs`** — ScriptableObject: product definitions.
- **`IAPManager.cs`** — Singleton wrapping Unity IAP (IStoreListener). DummyIAPProvider for testing.
- **`IAPStorePanel.cs`** — Store UI accessible from main menu, Rune Forge, pause menu, death screen.

### Testing
- Verify store displays all products with correct prices.
- DummyIAPProvider simulates purchases, verify currency delivered.
- Starter pack purchasable once, shows "SOLD OUT" after.
- All rewarded ad placements function correctly.

**Dependencies:** Phase 13 (buff shop reroll), Phase 14 (Rune Forge for gauntlet delivery).

---

## Dependency Graph & Build Order

```
Phase 10 (Acts + Waves) ──┬──> Phase 12 (Gauntlets) ──> Phase 14 (Rune Forge) ──> Phase 15 (IAP)
                           │                                      ↑
                           └──> Phase 13 (Buff Shop) ─────────────┘

Phase 11 (Orb Randomization) — independent, parallel with Phase 10
```

**Recommended order:** 10 → 11 (parallel) → 12 → 13 → 14 → 15

Each phase produces a playable, testable game state.

---

## Key Existing Files Reference

| File | Role |
|------|------|
| `Assets/_Project/Scripts/Data/WaveScalingConfig.cs` | Campaign data model (WaveParams) |
| `Assets/_Project/Scripts/Gameplay/WaveSpawner.cs` | Campaign orchestration, boss spawning |
| `Assets/_Project/Scripts/Gameplay/SpawnDirector.cs` | Adaptive pressure-based orc spawning |
| `Assets/_Project/Scripts/Gameplay/Enemy.cs` | Enemy lifecycle, damage, movement |
| `Assets/_Project/Scripts/Data/EnemyData.cs` | Enemy stats ScriptableObject |
| `Assets/_Project/Scripts/Gameplay/HeroCrystal.cs` | Crystal breaking + buff trigger |
| `Assets/_Project/Scripts/Gameplay/CrystalBuffManager.cs` | Buff lifecycle (→ becomes Gauntlet manager) |
| `Assets/_Project/Scripts/Gameplay/PlayerShooter.cs` | Fire rate, damage calc, projectile spawn |
| `Assets/_Project/Scripts/Gameplay/PooledProjectile.cs` | Projectile damage + pierce |
| `Assets/_Project/Scripts/Gameplay/CrystalBall.cs` | Orb collection → clone spawn |
| `Assets/_Project/Scripts/Gameplay/CrystalBallSpawner.cs` | Orb pool + spawn timing |
| `Assets/_Project/Scripts/Data/PlayerWallet.cs` | Gem/Rune persistence (PlayerPrefs) |
| `Assets/_Project/Scripts/Gameplay/PowerupManager.cs` | 8 consumable powerups + state queries |
| `Assets/_Project/Scripts/UI/UIManager.cs` | HUD, wave display, panels |
| `Assets/_Project/Scripts/UI/PowerupHUD.cs` | Bottom bar powerup buttons |
| `Assets/_Project/Scripts/UI/RevivePanel.cs` | Death screen with revive options |
| `Assets/_Project/Scripts/Gameplay/GameManager.cs` | Game state machine |
| `Assets/_Project/Scripts/Gameplay/BossBehavior.cs` | Base boss class |
| `Assets/_Project/Scripts/Gameplay/Bosses/AnimatedBossBehavior.cs` | Theatrical boss phases |
| `Assets/_Project/Scripts/Monetization/AdManager.cs` | Ad provider interface |
| `Assets/_Project/Scripts/Gameplay/CurrencyEarner.cs` | Currency reward distribution |
