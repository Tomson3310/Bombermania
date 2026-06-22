# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to Semantic Versioning.

## [0.5.1] - 2026-06-23
### Added
- Centralized `AudioManager` Singleton to handle background music and SFX with scene persistence.
- Advanced Audio Features: Added pitch shifting, volume control, and an anti-stacking limiter to prevent audio clipping when multiple sounds trigger in the same frame.
- Audio Variance: Bomb planting and explosions now support multiple audio clips and randomized pitch values for a more dynamic and less repetitive soundscape.
- Comprehensive SFX implementation for: Bomb Spawning, Explosions, Power-Up Spawn/Collect, Gate Spawn, Key Spawn/Collect, Player Death, and Game Over.
- Game Completed UI: Added a new `GameCompletedPanel` that appears upon finishing the final level, handling the High Score input transition.
- End-Game Bonus System: Implemented dramatic score tallying coroutines. The game now visually counts down remaining time (10 pts/sec) and remaining lives (5000 pts/life) with dynamic, ascending pitch-shifted sound effects.

### Changed
- Level Progression Flow: Replaced the instant level load with a time bonus tally sequence. 
- Refactored SFX calls across `Bomb`, `BombSpawner`, `PowerUp`, `Gate`, and `PlayerStats` to utilize the new `AudioManager` structure.

### Fixed
- End-Game Soft-Lock: Fixed a critical bug where completing the last level froze the game state without triggering the final UI.

---

## [0.5.0] - 2026-06-15
### Added
- Complete Animation System: Implemented directional movement (using 2D Simple Directional Blend Trees), idle, and death animations for both the Player and Enemies.
- Modular Explosion Architecture: Created separate `Center`, `Extension`, and `End` prefabs. `Bomb.cs` now dynamically spawns and correctly rotates these pieces to build explosions of any size.
- Dynamic Animator Injection: Added `RuntimeAnimatorController` field to `EnemyData` ScriptableObjects, allowing the `Enemy_Basic` prefab to dynamically load the correct animation brain based on the injected enemy profile.
- `DeathType` Enum System: Introduced a classification system (`Normal`, `Burn`) that triggers specific death animations (e.g., the player turns to ashes when killed by a bomb, but plays a standard death sequence when touched by an enemy).
- Crate Burn Mechanics: Added a 5-frame burning animation for crates. Burning crates now act as a temporary "death zone" that eliminates entities with collision-pass abilities (like Ghosts or players with `CratePass`) if they stand inside the fire.

### Changed
- Crate Collision Logic: `Crate.cs` no longer disables its collider instantly upon destruction. It now remains a solid obstacle for normal entities until the 0.5-second burn animation finishes.
- Animator Transitions: Bypassed default Unity crossfade blending by setting `Transition Duration` to 0 and utilizing direct `animator.Play()` calls. This ensures frame-perfect, snappy 2D pixel art animations without floating states.
- Sorting Layers Optimization: Updated global sorting layers (`Order in Layer`) to guarantee that character and enemy death animations render clearly above the explosion fire.
- Code Architecture: Updated `PlayerMovement`, `PlayerStats`, `EnemyAI`, `GameManager`, and `Explosion` to explicitly pass `DeathType` context when triggering deaths.

### Fixed
- Animation Loop Glitches: Disabled `Loop Time` on one-shot animations (crate burning, player death) and implemented dynamic length calculation via `AnimatorStateInfo` to prevent clipping, looping artifacts, or race conditions with Coroutines.
- Unit Tests: Updated `PlayerStatsTests` to accommodate the new `DeathType` requirement in `LoseLife()`, resolving compilation errors and ensuring 100% test pass rate.

---

## [0.4.2] - 2026-06-09
### Added
- Intermission Screen: Implemented a 3-second transition screen showing level number and current lives count. Game time is paused until the level starts.
- Telemetry System: Added colorful Rich Text logging in the console for critical moments in the game loop (`GameManager`, `PlayerStats`, `PlayerMovement`, `Explosion`) to improve debugging.
- Safe Zone: Added configurable `safeZoneRadius` parameter in `LevelGenerator` to prevent enemies and crates from spawning too close to the player's starting position.
- Main Menu Scene: Fully implemented functional main menu with options to start the game, view high scores, and quit the application.
- Leaderboard System (Top 10): Added persistent high score saving to disk in JSON format via `HighScoreManager`

### Changed
- State Management Architecture: Updated how `GameManager` persists data after death. Player keeps reduced lives, bomb count, and explosion range, but loses temporary power-ups (Speed, BombPass, CratePass, Detonator). Unique items are returned to the loot pool and removed from UI inventory.
- Game Over Flow: Final death no longer instantly loads Level 1. Instead, it shows a game over screen, then either opens the new high score panel or returns to the Main Menu.

### Fixed
- Critical StackOverflowException: Eliminated infinite recursion loop between `PlayerMovement` and `PlayerStats` that was crashing the Unity Editor on last life.
- Spawn Kill Bug: Prevented enemies (`EnemyAI`) and the player from moving or attacking during the intermission screen.
- Physics State Leak (BombPass/CratePass): Enforced full reset of `Physics2D.IgnoreLayerCollision` rules in player's `Start()` method so power-up effects no longer carry over to new lives.
- Lives Display Issue: Intermission screen now correctly reads the updated lives count directly from `GameManager.savedLives`, fixing timing problems with player prefab loading.

---

## [0.4.1] - 2026-05-26
### Added
- Single Scene Architecture: Implemented seamless transitions between levels.
- LevelData System: Introduced ScriptableObject files to design level rules (time, dimensions, enemies, rewards) directly from the Inspector.
- State Vault in GameManager: Added session progress saving system (player stats, lives, inventory) that carries over between stages.
- Dynamic Camera: New intelligent `CameraController` that automatically reads map size, centers view or follows the player while respecting boundaries and UI.
- Universal Power-Up Prefab: Replaced multiple prefabs with a single dynamic reward "bubble" that changes appearance and properties at runtime.
- `EnemyData` ScriptableObject class to define enemy identities, stats, and abilities.
- Configurable `ScoreValue` for each enemy type.
- FastBasic and Ghost enemy profiles created using the new ScriptableObject system.

### Changed
- Major Architecture Update: Refactored Power-Up system to use `PowerUpData` ScriptableObjects and polymorphism.
- Major Architecture Update: Decoupled enemy behavior from stats using `EnemyData` ScriptableObject.
- Overhauled grid movement system: Replaced velocity-based pushing with precise waypoint-based movement.
- Completely rebuilt `LevelGenerator.cs` — now reads data from GameManager, spawns player in top-left and injects `EnemyData` profiles.
- Improved `GameManager.cs` with `DontDestroyOnLoad` and better session handling.
- Decoupled `PlayerStats` from UI concerns (removed Sprite references).
- `PlayerStats.cs` now loads upgrades from previous level on scene start.
- Simplified `PowerUp.cs` collision logic — now directly executes attached ScriptableObject's `ApplyEffect()`.
- `UIManager.cs` gained methods for exporting and importing inventory graphical state.

### Fixed
- Fixed high-speed enemies overshooting grid centers and getting stuck in corners.
- Eliminated enemy movement jitter and physics fights by nullifying residual forces and enforcing strict axis-locking.
- Fixed collision loops by adding `bounceCooldown` and checking head-on collision angles.

### Removed
- Removed hardcoded objects in the scene (Player etc.) — everything is now procedurally generated.
- Removed old separate prefabs for individual Power-Ups.
- Removed manual level references from the Inspector in camera and generator.

---

## [0.4.0] - 2026-05-25
### Added
- Implemented the remaining 4 Power-Ups: `SpeedMove`, `CratePass`, `BombPass`, and `ExtraLife`.
- Added a safety reset in `PlayerStats` to restore default physics collisions upon player death/restart.

### Changed
- Refactored `PlayerMovement` to dynamically fetch movement speed from `PlayerStats` instead of using an isolated local variable.
- Improved `BombSpawner` logic with `Physics2D.OverlapPoint` checking the `obstacleLayer`, strictly preventing bomb placement while standing inside a crate.

### Fixed
- Updated the custom raycast radar in `PlayerMovement` to safely ignore `Bomb` and `Crate` layers when the respective pass power-ups are active.
- Fixed rigid physics body stuttering by dynamically toggling `Physics2D.IgnoreLayerCollision` when passing through crates and bombs.
- Removed unused dead code (`SetRadius`) from `Bomb.cs`.

---

## [0.3.0] - 2026-03-27

### Added
- Dynamic HUD and inventory UI system (`UIManager` singleton).
- Top panel tracking global stats (Score, Level, Lives).
- Visual state transitions for key/power-up icons (grayscale → color upon collection).
- Bottom panel 6-slot inventory system using `HorizontalLayoutGroup`:
  - 2 fixed stat slots
  - 4 buffer slots for one-time power-ups

### Changed
- Integrated `GameManager` and `PlayerStats` with the new UI system.
- Automatic state change broadcasting to UI.

---

## [0.2.0] - 2026-03-19

### Added
- Detonator power-up with manual FIFO (First-In-First-Out) bomb detonation.
- Bomb chain reactions (explosions trigger other bombs instantly).
- Force explosion mechanics.
- Scalable enum-based power-up system (`PowerUp.cs`).
- Dynamic loot pool generation.
- Player bomb limits tied to player stats (`MaxBombs`).
- Level progression loop managed by `GameManager`:
  - Enemy tracking
  - Key drops after clearing the map
  - Exit gate system
- Basic enemy AI with spontaneous movement.
- Solid bomb physics enabling enemy trapping mechanics.
- Unity Test Framework setup:
  - PlayMode tests (player, explosions, level generator)
  - Assembly definitions configured

### Changed
- Refined explosion and enemy hitboxes for fairer gameplay.
- Disabled physical collisions between player and enemies:
  - Death now handled via trigger system

### Fixed
- Grid snapping physics issues.
- Wall friction problems affecting enemy AI pathfinding.

---

## [0.1.0] - 2026-03-14

### Added
- Procedural level generation system.
- Hidden items mechanic.
- Fire radius power-up.
- Crate prefabs with destruction logic and future drop support.
- Bomb spawning with precise `+0.5f` grid snapping.
- Explosion prefab linked to bomb detonation.
- Grid-based colliders and physics layers:
  - `SolidWall`
  - `DestructibleWall`
- Initial player setup:
  - `Rigidbody2D`
  - Basic movement script
- Tilemap layers:
  - Floor
  - SolidWalls
- Initial 17x11 grid with custom sprites

### Changed
- Improved player movement:
  - Custom raycast gap detection
  - Corner sliding / gap seeking
  - Input priority system for responsiveness