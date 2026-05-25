# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to Semantic Versioning.

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