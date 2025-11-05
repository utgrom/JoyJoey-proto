Joy Joey — Gameplay Quickstart
================================

Use this as the short checklist when wiring a new character. See `GUIDE_v2.md` for the exhaustive explanation.

Key Ideas
- Inputs are absolute. Every action is `Button + Direction (Neutral / Horizontal / Down) + Context (Ground / Air)`.
- The player can equip up to three rags (Strongman, Balloonmaster, Jester). Each rag supplies Special Attack / Special Skill ActionSets.
- Transforming activates the current rag’s suit: stat multipliers, visual scaling, bigger hitboxes, Fun drain.
- Abilities are string IDs with a boolean unlock + integer level. Levels choose which AttackData version plays.
- Only Jump and Transform are buffered (8f on land, 40f on neutral).

1. Input Actions
   - Map: `Move`, `Normal`, `SpecialAttack`, `SpecialSkill`, `Jump`, `Dash`, `Transform`, `SwapNext` (E), `SwapPrevious` (Q).
   - Assign them in `PlayerInputReader`. Hold vs Tap is derived from `holdThreshold`.

2. Player Prefab Components (in this order)
   1. `PlayerStatus` — Health / Fun / Grace defaults.
   2. `AbilityUnlockManager` — add entries for core movement, rag unlocks, suit transforms, and move IDs that need levels.
   3. `RagManager` — list up to 3 rags, set `startingRagId`.
   4. `TransformManager` — reference stats profile, PlayerStatus, AbilityUnlockManager, RagManager; set transform threshold.
   5. `PlayerMotor` — configure ground & wall sensors (`groundLayers`, offsets, distances, `wallInputThreshold`, `wallSlideMaxFallSpeed`).
   6. `PlayerInputReader` — bind the Input Actions.
   7. `PlayerController` — wires everything, handles buffer + FSM.
   8. `PlayerCombatController` — assign base Normal ActionSets and optional transformed fallback.
   9. Child `AttackExecutor` with `HitboxSlot`(s) + `HurtboxController` (usually the capsule collider).

3. Stats
   - Create `PlayerStatsProfile`.
   - Movement: max speed / accel / decel (ground & air).
   - Jump: jump velocity, coyote time, wall jump horiz/vert speeds.
   - DashStats: `startupTime`, `activeTime`, `recoveryTime`, `dashSpeed`, momentum policy, carry window.

4. Abilities
   - Movement IDs: `movement.jump`, `movement.airJump`, `movement.dash`, `movement.airDash`, `movement.wallJump`.
   - Rag unlocks: `rag.<name>.unlock`.
   - Suit unlocks: `suit.<name>.transform`.
   - Move upgrades: `<name>.<button>.<direction>` (e.g., `strongman.normal.neutral`).
   - Set `unlocked=true` for anything available in the demo and assign `level` (0 = base, 1+ selects upgraded AttackData).

5. ActionSets
   - Create one `ActionSet` per button & context (Normal Ground/Air, SpecialAttack Ground/Air, SpecialSkill Ground/Air, plus suit normal overrides if needed).
   - Add exactly three `ActionVariant` entries: Neutral, Horizontal, Down.
   - Optional: `requiredAbilityId` to hide variants until unlocked.
   - Optional: `levelAbilityId` + `levelVariants[]` to swap AttackData when ability level ≥ N.

6. Rags & Suit
   - `RagProfile`: assign special attack/skill ActionSets (ground/air). Assign suit normal overrides if the transformed form changes normals.
   - `SuitProfile`: fill multipliers (`speed`, `jump`, `dashSpeed`), visual scale (`visualScale`), collider multipliers (height/radius), hitbox scale, Fun drain per second.

7. Attacks
   - Author `AttackData`: timeline (startup/active/recovery, FPS), hitbox events on slot “Main”, optional i-frame windows.
   - Set `baseDamage`, `breakValue`, `launchVector`, `minimumReactionTier`, `hitstopMs`, `carryMomentum`, `consumeDashCarry`.

Testing Checklist
- Movement + dash feel right (dash obeys startup/active/recovery, locks inputs during startup/active).
- Wall slide engages only while holding toward the wall; wall jump uses configured horizontal/vertical speeds.
- Normals fire (Neutral/Horizontal/Down × Ground/Air). Suit overrides take effect when transformed.
- Rag specials trigger from the correct ActionSets and respect ability unlocks.
- Transform works when Fun ≥ threshold, drains Fun, scales visuals/collider/hitboxes, and reverts cleanly.
- PlayerDebugHUD (toggle F1) displays movement context, rag, transform state, velocity, wall info, Buffers, Health/Fun/Grace.

Tuning Quick Reference
- `movement.maxGroundSpeed`, `groundAcceleration`, `groundDeceleration`: ground responsiveness.
- `movement.airAcceleration`, `airDeceleration`: aerial control.
- `jump.jumpVelocity`, `maxFallSpeed`, `gravityScale`: jump arc.
- `dash.startupTime`, `activeTime`, `recoveryTime`, `dashSpeed`, `momentumPolicy`: dash behavior.
- `PlayerMotor.wallInputThreshold`, `wallSlideMaxFallSpeed`, `JumpStats.wallJump*`: wall slide/jump tuning.
- `SuitProfile` multipliers: transform feel + size (collider/visual/hitbox).

Debug
- Enable Scene gizmos to visualize ground sphere & wall rays (`PlayerMotor.drawGizmos`).
- Hitboxes draw red wire outlines when active and the slot is selected.
- Physics 2D gizmos help verify raycasts and triggers.

Need more detail? Open `GUIDE_v2.md` for a comprehensive breakdown of every field and an end-to-end example.
