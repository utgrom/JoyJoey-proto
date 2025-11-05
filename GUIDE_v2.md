Joy Joey — Gameplay Code Guide (v2)
==================================

This document is the exhaustive companion for designers and engineers. It documents every relevant script, field, asset, and workflow after the simplification refactor.

1. Architecture Overview
------------------------
- **Inputs**: Absolute directions. Every move is resolved by `Direction (Neutral / Horizontal / Down) + Button + MovementContext (Ground / Air) + Rag + Suit state`.
- **Buttons**: `Normal`, `SpecialAttack`, `SpecialSkill`, `Jump`, `Dash`, `Transform`, `SwapNext`, `SwapPrevious`.
- **Rags**: Up to three equipped (Strongman, Balloonmaster, Jester). Hot-swap with Q/E. Each rag supplies Special Attack / Special Skill ActionSets for Ground/Air.
- **Suits**: Transform consumes Fun (per second), applies stat multipliers, scales visuals/collider/hitboxes. Transform is allowed whenever Fun ≥ threshold and the rag’s suit ability is unlocked.
- **Abilities**: Strings with boolean `unlocked` + integer `level`. Levels choose which AttackData version plays. No Scriptable `AbilityData` — everything is handled in `AbilityUnlockManager` + `ActionSet`.
- **Buffers**: Only Jump (8f, OnLand, priority High) and Transform (40f, OnNeutral, priority Normal) are buffered.
- **Dash**: Startup → Active → Recovery timeline. Startup + Active lock inputs when `DashStats.locksInputs` is true. Dash carry flag persists through recovery + carry window.
- **Enemy reactions**: Keep ARM thresholds & regeneration—no simplifications.

2. Script & Field Reference
---------------------------
- `Core/GameplayEnums.cs`
  - `InputButton`, `ActionDirection`, `MovementContext`, `InputBufferConsumeCondition`, `ActionPriority` (Low/Normal/High/VeryHigh).
- `Core/Abilities/AbilityIds.cs`
  - Movement: `movement.jump`, `movement.airJump`, `movement.dash`, `movement.airDash`, `movement.wallJump`.
  - Rag unlocks: `rag.strongman.unlock`, `rag.balloon.unlock`, `rag.jester.unlock`.
  - Suit unlocks: `suit.strongman.transform`, etc.
  - Move version examples: `strongman.normal.neutral`, `strongman.specialAttack.down`, `strongman.specialSkill.neutral`.
- `Core/Abilities/AbilityUnlockManager.cs`
  - Inspector list of abilities. Each entry: `abilityId`, `unlocked`, `level`.
  - Methods: `CanUse(id)`, `GetLevel(id)`, `Unlock(id, level)`, `SetLevel(id, level)`. Missing entries default to unlocked=false, level=0.
- `Core/PlayerRuntimeStats.cs`
  - `MovementStats`: `maxGroundSpeed`, acceleration & deceleration (ground/air).
  - `JumpStats`: `jumpVelocity`, `maxAirJumps`, `coyoteTime`, `gravityScale`, `maxFallSpeed`, wall-jump horizontal/vertical speeds.
  - `DashStats`: `dashSpeed`, `startupTime`, `activeTime`, `recoveryTime`, `momentumPolicy`, `locksInputs`, `carryMomentum`, `carryWindow`, `TotalDuration` (computed).
- `Player/Input/PlayerInputReader.cs`
  - Expose `MoveVector` + queue button events with `Tap`/`Hold` modifiers (config `holdThreshold`).
- `Player/Input/InputBuffer.cs`
  - `InputBuffer.SetPolicy(actionId, new BufferPolicy(enabled, frames, consumeOn, priority))`.
  - Only Jump & Transform configured in `PlayerController.Awake`.
- `Player/PlayerMotor.cs`
  - Ground detection (offset/radius), wall rays (offsetL/R, distance), `wallInputThreshold`, `wallSlideMaxFallSpeed`.
  - Methods: `ApplyGroundMovement`, `ApplyAirMovement`, `ApplyJump`, `ApplyWallSlide`, `PerformWallJump`, `BeginDash`, `EndDash`.
- `Player/States`
  - `PlayerGroundedState` / `PlayerAirborneState` / `PlayerWallSlideState` / `PlayerDashState` (with timeline) / others.
  - Dash state phases: Startup → Active → Recovery → Complete.
- `Player/PlayerController.cs`
  - Manages HFSM, input sampling, buffer consumption, dash carry, rag swaps.
  - Hardcodes Jump/Transform buffers.
- `Combat/Actions/ActionSet.cs`
  - `ActionVariant` fields:
    - `direction` — Neutral / Horizontal / Down.
    - `attackData` — base AttackData when level = 0.
    - `requiredAbilityId` — optional gate (bool unlock).
    - `levelAbilityId` — optional ability whose level selects upgraded AttackData.
    - `levelVariants[]` — array where index 0 = level 1, index 1 = level 2, etc. Null entries fall back to base `attackData`.
  - `Resolve(direction, abilityManager)` returns the appropriate AttackData.
- `Player/Combat/PlayerCombatController.cs`
  - `groundNormalActions`, `airNormalActions`, optional `transformedNormalFallback`.
  - For rag specials: uses `RagProfile.SpecialAttackGround/Air`, `SpecialSkillGround/Air`.
  - When transformed: suit normal override sets (ground/air) are preferred; fallback to base normals if missing.
- `Player/Rags/RagProfile.cs`
  - `specialAttackGroundActions`, `specialAttackAirActions`, `specialSkillGroundActions`, `specialSkillAirActions`.
  - `suitNormalGroundActions`, `suitNormalAirActions` (optional overrides while transformed).
  - Unlock info: `unlockAbilityId`, `unlockRequiredLevel`, `transformAbilityId`, `transformRequiredLevel`.
  - `SuitProfile` reference.
- `Player/Rags/RagManager.cs`
  - Keeps list of rags, ensures only unlocked rags can be selected, raises `RagChanged`.
- `Player/Transformations/SuitProfile.cs`
  - `SpeedMultiplier`, `JumpMultiplier`, `DashSpeedMultiplier`.
  - `VisualScale` (Vector3), `ColliderHeightMultiplier`, `ColliderRadiusMultiplier`, `HitboxScaleMultiplier`.
  - `FunDrainPerSecond`.
- `Player/Transformations/TransformManager.cs`
  - Tracks base stats, visual scale, collider size/offset, hitbox scale.
  - Applies suit multipliers on activation, resets on deactivation.
  - Exposes `HitboxScale` for `AttackExecutor`.
- `Combat/Attacks/AttackData.cs`
  - `AttackTimeline`: startup/active/recovery frames, FPS, list of hitbox events (slot “Main”), i-frame windows.
  - Core properties: `hitstopMs`, `baseDamage`, `breakValue`, `minimumReactionTier`, `launchVector`, `carryMomentum`, `consumeDashCarry`.
- `Combat/Hitboxes/HitboxSlot.cs`
  - Activates collider with scale applied. Draws red wire gizmo when selected and active.
- `Player/Stats/PlayerStatus.cs`
  - Health/Fun/Grace; upgrade and spend helpers.
- `Player/Debug/PlayerDebugHUD.cs`
  - Toggle with F1. Displays context, rag, transform, velocity, wall info, dash carry, Health/Fun/Grace.

3. Abilities & ActionSets
-------------------------
- **Unlock gating (`requiredAbilityId`)**: If empty or `AbilityIds.None`, the variant is always available. Otherwise, `AbilityUnlockManager.CanUse(id)` must be true.
- **Level selection (`levelAbilityId`)**:
  - `level` is retrieved via `AbilityUnlockManager.GetLevel(id)` (0 default).
  - Level 0 → base `attackData`.
  - Level N (≥1) → `levelVariants[Mathf.Min(N-1, levelVariants.Length-1)]` if not null.
- **Typical usage**:
  - Movement core: set Jump/Dash/AirJump/AirDash/WallJump unlocked by default so baseline movement works.
  - Rag moves: `requiredAbilityId = strongman.specialAttack.down` so the move is hidden until unlocked.
  - Move upgrades: `levelAbilityId = strongman.normal.neutral` with level variants for Slash.1 / Slash.2.

4. Scene Wiring Step-by-Step
----------------------------
1. **Create Player GameObject** and add components in order:
   - `PlayerStatus`
   - `AbilityUnlockManager`
   - `RagManager`
   - `TransformManager`
   - `PlayerMotor`
   - `PlayerInputReader`
   - `PlayerController`
   - `PlayerCombatController`
   - Child: `AttackExecutor` (with one or more `HitboxSlot`s) + `HurtboxController` (reference capsule collider).
2. **Configure PlayerMotor**: assign `groundLayers`, adjust `groundCheckOffset`/`radius`, `wallCheckOffsetLeft/right`, `wallCheckDistance`, `wallInputThreshold`, `wallSlideMaxFallSpeed`. Enable `drawGizmos` while tuning.
3. **Bind inputs** in `PlayerInputReader`: `Move`, `Normal`, `SpecialAttack`, `SpecialSkill`, `Jump`, `Dash`, `Transform`, `SwapNext`, `SwapPrevious`.
4. **Stats**: assign `PlayerStatsProfile` to `TransformManager` (base stats). Tune jump/dash/wall parameters.
5. **Abilities**: populate `AbilityUnlockManager` with entries and default values.
6. **ActionSets**: create for each button/context (base normals, rag specials, suit normals). Ensure each has exactly three variants (Neutral/Horizontal/Down).
7. **RagProfile**: assign ActionSets, suit profile, unlock/transform ability IDs. Add the profile to `RagManager`.
8. **SuitProfile**: set multipliers, visual scale, collider multipliers, hitbox scale, Fun drain.
9. **AttackData**: author timeline and hitboxes. Use slot "Main" for all events. Add i-frame windows if required.
10. **Enemy**: configure `EnemyArmourProfile`/`EnemyReactionController` on test targets.

5. Suits — What Happens on Transform
------------------------------------
- Stats multiplied: `maxGroundSpeed`, ground/air acceleration & deceleration, `jumpVelocity`, `maxFallSpeed`, `dashSpeed`.
- Visual scale: player transform scaled by `SuitProfile.VisualScale` (per axis).
- Collider: Capsule size X multiplied by `ColliderRadiusMultiplier`, Y by `ColliderHeightMultiplier`, offset adjusted so feet stay planted.
- Hitboxes: `HitboxSlot` applies `SuitProfile.HitboxScaleMultiplier` whenever an attack is active while transformed.
- Fun drain: `SuitProfile.FunDrainPerSecond` consumed each frame; transform ends when Fun ≤ 0 or the suit ability becomes locked.

6. Example Character (Strongman Mini-Demo)
-----------------------------------------
1. **Abilities** (`AbilityUnlockManager`)
   - Movement: unlock `movement.jump`, `movement.dash`, `movement.wallJump`. Add `movement.airJump` with unlock=false (grant later).
   - Rag unlocks: `rag.strongman.unlock` (true, level 1).
   - Suit: `suit.strongman.transform` (true, level 1).
   - Moves: `strongman.normal.neutral`, `strongman.specialAttack.down`, `strongman.specialSkill.neutral` (unlocked at level 1).
2. **ActionSets**
   - `Normal_Ground`: Neutral→Slash.1, Horizontal→ShoulderCheck, Down→Sweeper.
   - `Normal_Air`: Neutral→SlashAir.1, Horizontal→AirSwipe, Down→DiveKick.
   - `NormalSuit_Ground`: Override Horizontal/Down with heavier suit attacks.
   - `SpecialAttack_Ground`: Down→GroundPound (requiredAbilityId = `strongman.specialAttack.down`).
   - `SpecialSkill_Ground`: Neutral→ShoulderBuff (requiredAbilityId = `strongman.specialSkill.neutral`).
3. **SuitProfile (Strongman)**
   - `SpeedMultiplier=1.1`, `JumpMultiplier=1.0`, `DashSpeedMultiplier=1.15`.
   - `VisualScale = (1.2, 1.2, 1)`, `ColliderHeightMultiplier=1.2`, `ColliderRadiusMultiplier=1.1`, `HitboxScaleMultiplier=1.15`.
   - `FunDrainPerSecond = 6`.
4. **Stats**
   - Jump: velocity 12.5, coyote 0.1, wall jump X=10, Y=12.
   - Dash: startup 0.05, active 0.12, recovery 0.05, speed 15, momentum=Maintain.
5. **Testing**
   - Move, dash, wall slide/jump, Normal Horizontal/Down (ground & air), SpecialAttack Down, SpecialSkill Neutral.
   - Transform when Fun ≥ 25 → check speed, size, hitbox increase; allow reversion when Fun drains.

7. Debugging & Tuning
---------------------
- **Gizmos**: `PlayerMotor.drawGizmos` (ground sphere + wall rays). HitboxSlot draws red wire outlines for active colliders when selected.
- **HUD**: Toggle with F1 (`PlayerDebugHUD`).
- **Physics 2D gizmos**: visualize raycasts & triggers.
- **Feel tuning**: adjust `MovementStats`, `JumpStats`, `DashStats`, wall thresholds.
- **Suit feel**: tweak SuitProfile multipliers and collider/hitbox scales.
- **ARM**: adjust `EnemyArmourProfile` thresholds, break regen, stun gate.

8. Glossary
-----------
- **ActionVariant** — mapping of direction → AttackData. May require an ability and/or select versions by ability level.
- **DashCarry** — flag set during dash; certain attacks can consume it to inherit momentum.
- **Fun** — resource drained while transformed.
- **ARM (Armour)** — enemy poise. Reduced by `AttackData.breakValue`, regenerates over time.
- **SGT (Stun Gate Threshold)** — when ARM ≤ threshold, attacks cause at least Stagger/Launch depending on move.
- **HitboxScaleMultiplier** — suit-wide multiplier applied to hitbox size & offset.

9. Tips & Gotchas
------------------
- Always enter core movement abilities (jump/dash/wall jump) in `AbilityUnlockManager` — missing entries mean the move is treated as locked.
- ActionSets must always have exactly three variants (Neutral/Horizontal/Down) even if some slot is unused (assign null to attackData for now).
- Suit overrides currently affect normals only. Specials use rag ActionSets regardless of transform (override manually by creating suit-specific ActionSets if needed).
- When scaling the collider, keep an eye on ceiling collisions — increasing height reduces head clearance.
- `TransformManager.HitboxScale` affects all attacks fired while transformed; author AttackData at “base” size.
- For move upgrades, set `levelAbilityId` equal to the move ability id; level 0 uses base AttackData, level ≥1 uses the corresponding array entry.
- To author multiple rags, use the same ActionSet assets where possible; their ability gating/levels allow partial unlocks for the demo.

Refer back to `GUIDE.md` for the condensed checklist, and to `AGENTS.md` for the agent-facing summary.
