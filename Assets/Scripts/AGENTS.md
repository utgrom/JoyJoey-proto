Joy Joey — Agent Context (v2)

Scope
- Applies to scripts under `Assets/Scripts`.
- Project target: vertical-slice demo with at most 3 rags (Strongman, Balloonmaster, optional Jester).
- Input is absolute directions; actions are resolved purely from Direction (`Neutral / Horizontal / Down`) + Button + Movement Context (`Ground / Air`) + Rag + Suit state.

Baseline Assumptions
- Rags: player equips up to 3. Hot-swap with Q (`SwapPrevious`) / E (`SwapNext`).
- Buttons per context: `Normal`, `SpecialAttack`, `SpecialSkill`. Each has exactly 3 directions (Ground and Air => 6 moves per button).
- Transform: allowed whenever Fun ≥ threshold and the rag’s suit unlock is true. Suit modifies stats & visuals and drains Fun.
- Abilities: simple unlock + integer level (0 default). Levels choose which AttackData version plays; we do not scale numbers at runtime.
- Buffers: only Jump (8 frames, OnLand) and Transform (40 frames, OnNeutral) are buffered.
- Dash: startup → active → recovery timeline; dash carries momentum flag during active + carry window.
- Enemy reactions: keep ARM regeneration & thresholds (no simplification).

Key Components & Fields
- `Core/PlayerRuntimeStats.cs`
  - `MovementStats`, `JumpStats`, `DashStats` (startup/active/recovery, speed multipliers).
- `Core/Abilities/AbilityIds.cs`
  - Movement IDs: `movement.jump`, `movement.airJump`, `movement.dash`, `movement.airDash`, `movement.wallJump`.
  - Rag IDs: e.g., `rag.strongman.unlock`, `suit.strongman.transform`, `strongman.normal.neutral`, `strongman.specialAttack.down`.
- `Core/Abilities/AbilityUnlockManager.cs`
  - Per-ID: `unlocked` bool + `level` int. No Scriptable `AbilityData`.
- `Player/Input/PlayerInputReader.cs`
  - Bind `Move`, `Normal`, `SpecialAttack`, `SpecialSkill`, `Jump`, `Dash`, `Transform`, `SwapNext`, `SwapPrevious`.
- `Player/Input/InputBuffer.cs`
  - Policies set in code (`InputBuffer.SetPolicy`). Only Jump and Transform configured.
- `Player/PlayerMotor.cs`
  - Ground sphere, wall rays, wall slide clamp, wall jump velocities.
- `Player/States/*`
  - Dash state uses startup/active/recovery timeline and locks inputs during startup + active when `DashStats.locksInputs` is true.
- `Player/PlayerController.cs`
  - High-level HFSM controller. Subscribes to rag changes, handles buffered actions, dash carry flag, ability checks.
- `Combat/Actions/ActionSet.cs`
  - `ActionVariant`: `direction`, `requiredAbilityId`, `levelAbilityId`, `attackData`, `levelVariants[]`.
  - `Resolve` chooses variant by direction and ability gating, then picks `attackData` or a level-specific version.
- `Player/Combat/PlayerCombatController.cs`
  - Looks up ActionSets by button/context/rag/suit. Suit overrides only affect normals (ground/air).
- `Player/Rags/RagProfile.cs`
  - References to special attack/skill ActionSets (ground/air) and optional suit normal overrides (ground/air).
- `Player/Transformations/SuitProfile.cs`
  - Multipliers: speed, jump, dash speed. Visual overrides: `visualScale`, collider height/radius multipliers, hitbox scale multiplier, `funDrainPerSecond`.
- `Player/Transformations/TransformManager.cs`
  - Applies stat multipliers, adjusts sprite scale and capsule collider, scales hitboxes via `HitboxScale` property consumed by `AttackExecutor`.
- `Combat/Attacks/AttackData.cs`
  - Timeline (startup/active/recovery, FPS), hitbox events (single slot “Main”), i-frame windows, `hitstopMs`, `baseDamage`, `breakValue`, `launchVector`, `minimumReactionTier`, `carryMomentum`, `consumeDashCarry`.
- `Combat/Hitboxes/HitboxSlot.cs`
  - Applies hitbox scale (from TransformManager) and draws gizmos when selected.
- `Player/Stats/PlayerStatus.cs`
  - Health, Fun, Grace storage. Provides Add/Spend/Upgrade helpers.
- `Player/Debug/PlayerDebugHUD.cs`
  - Toggle with F1. Displays movement context, rag, transform state, velocity, wall info, dash carry, and player stats.

Authoring Workflow
1. **Player prefab**
   - Components: PlayerStatus → AbilityUnlockManager → RagManager → TransformManager → PlayerMotor → PlayerInputReader → PlayerController → PlayerCombatController. Add AttackExecutor child with HitboxSlot(s) + HurtboxController.
2. **Input System**
   - Map actions listed above. Assign `InputActionReference`s in `PlayerInputReader`.
3. **Stats**
   - Create `PlayerStatsProfile`. Set movement/jump baseline, dash startup/active/recovery, wall-jump speeds.
4. **Abilities**
   - In `AbilityUnlockManager`, add entries for movement basics (jump, airJump, dash, airDash, wallJump), rag unlocks, suit transforms, and move IDs that need levels.
   - Set `unlocked=true` and `level` as needed (e.g., `movement.jump` level 1 by default).
5. **Actions**
   - For each button/context (Normal/SpecialAttack/SpecialSkill × Ground/Air) create an `ActionSet` with three variants: Neutral, Horizontal, Down.
   - Optional gating: set `requiredAbilityId` (boolean unlock) and/or `levelAbilityId` + `levelVariants` for upgraded AttackData.
6. **Rags**
   - `RagProfile`: assign special attack/skill ActionSets, optional suit normal overrides, `SuitProfile`, unlock/transform ability ids/levels.
   - Configure `RagManager` with up to 3 profiles and set `startingRagId`.
7. **Attacks**
   - Author `AttackData`: timeline, hitbox events on slot "Main", invulnerability windows, damage/break/launch values, dash carry flags.
   - NOTE: i-frames persist; add windows where the player should ignore `targetMask` collisions.
8. **Enemy**
   - Use `EnemyArmourProfile` to configure ARM thresholds and regeneration. No changes from the base spec.

Buffers
- Jump: OnLand, 8 frames, high priority.
- Transform: OnNeutral, 40 frames, normal priority.
- No other buffering.

Dash Timeline
- Configure via `DashStats.startupTime`, `activeTime`, `recoveryTime`.
- Startup + Active lock other inputs when `DashStats.locksInputs` is true.
- Dash momentum policy selectable (Maintain / BlendTo / ZeroOnEnter / ZeroOnExit).
- Dash carry flag lasts `active + recovery + carryWindow` and can be consumed by attacks.

Suit Overrides
- `SpeedMultiplier` scales ground/air acceleration & deceleration in addition to max speed.
- `JumpMultiplier` scales jump velocity and max fall speed.
- `DashSpeedMultiplier` scales dash speed.
- `VisualScale` multiplies player transform scale.
- Collider multipliers preserve feet alignment (bottom alignment maintained).
- `HitboxScaleMultiplier` affects all hitboxes triggered while transformed.

ActionSets & Levels
- `requiredAbilityId`: gate variant availability (bool).
- `levelAbilityId`: ability whose level selects which AttackData version plays.
  - Level 0 → base `attackData`.
  - Level N (>0) → `levelVariants[Mathf.Min(N-1, levelVariants.Length-1)]` if assigned.
- Use string IDs such as `strongman.normal.neutral` to denote upgrade tracks.

Abilities Quick Reference
- Movement: `movement.jump`, `movement.airJump`, `movement.dash`, `movement.airDash`, `movement.wallJump`.
- Rag unlocks: `rag.<name>.unlock`.
- Suit unlocks: `suit.<name>.transform`.
- Move versions: `<name>.<button>.<direction>` (freely choose, e.g., `strongman.specialAttack.down`).
- Levels: store integer; level 0 = base, level 1+ = upgraded AttackData via ActionVariant `levelVariants`.

Debugging
- F1 toggles `PlayerDebugHUD`.
- `PlayerMotor.drawGizmos` highlights ground sphere & wall rays.
- HitboxSlot draws active collider when selected in Scene view.
- Use Physics 2D gizmos to visualize raycasts & triggers.

Open Tasks (content)
- Author final ActionSets/RagProfiles for Strongman and Balloonmaster (Jester optional).
- Create AttackData for base normals (ground/air) + rag specials/skills + suit overrides.
- Wire AbilityUnlockManager defaults (movement unlocked by default; rag/suit moves gated as needed).
- Implement damage/ARM pipeline using `HitboxSlot` triggers and `EnemyReactionController`.
