# 🚀 Space Shooter - Unity 2D

A classic 2D space shooter game built with **Unity 6** and **Universal Render Pipeline (URP 2D)**.

![Unity](https://img.shields.io/badge/Unity-6000.x-blue?logo=unity)
![C#](https://img.shields.io/badge/C%23-12-purple?logo=csharp)
![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Windows-orange)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 🎮 Gameplay

- **Move:** `WASD` or `Arrow Keys`
- **Shoot:** `Space` or `J`
- Destroy enemy ships and dodge meteors
- Collect item drops to restore health or upgrade weapons
- Survive as many waves as possible
- Track your highscores!

---

## ✅ Features Implemented

### Core Mechanics
- [x] **Player Movement** — WASD + Arrow Keys, smooth SmoothDamp movement with screen boundary clamping
- [x] **Shooting** — Space / J key, auto-fire while held
- [x] **Wave System** — Progressive difficulty: enemy count, speed, and fire rate increase each wave
- [x] **Boss Wave** — Boss C7 appears every 5 waves with warning screen and dedicated health bar

### Weapons System
- [x] **4 Weapon Types** — Blaster, Laser (piercing), Missile (homing), Plasma (AoE)
- [x] **3 Weapon Levels** — Spread shot upgrades (1 → 2 → 3 projectiles)
- [x] **Weapon HUD** — Real-time icon + level display in bottom-left corner

### Items & Pickups
- [x] **Health Pickup** — Restores player HP (drops from meteors, 55% chance)
- [x] **Weapon Upgrade** — Level up current weapon (25% chance)
- [x] **Weapon Type Pickups** — Switch to Laser / Missile / Plasma (20% chance)
- [x] **Shield Pickup** — 6-second invincibility with rotating visual + warning flash

### Enemy Diversity
- [x] **5 Enemy Types** with unique sprites, stats, and wave unlock thresholds:

  | Type | HP | Speed | Fire Rate | Aim | Unlocks |
  |------|----|-------|-----------|-----|---------|
  | Scout 🟢 | 40 | Fast 3.5 | 3.0s | 30% | Wave 1 |
  | Fighter ⚪ | 80 | Normal 2.2 | 2.5s | 50% | Wave 1 |
  | Interceptor 🟡 | 60 | Fast 3.0 | 2.0s | 80% | Wave 3 |
  | Bomber 🔴 | 180 | Slow 1.3 | 1.8s | 40% | Wave 5 |
  | Elite 🟣 | 130 | Fast 2.8 | 1.5s | 70% | Wave 8 |

- [x] **Weighted Random Spawn** — Higher-tier enemies spawn more rarely; earlier waves favour basic types
- [x] **Advanced Enemy AI v2** — Off-screen shooting guard, aim-at-player scaling, hard mode spread shot (wave 10+)

### Space Effects
- [x] **Background Scrolling** — Infinite vertical parallax starfield
- [x] **Moving Meteors** — Random rotation, horizontal drift, 4 meteor sprite variants

### Audio
- [x] **Background Music** — 3 tracks with smooth cross-fade (Menu / Playing / Boss)
- [x] **Sound Effects** — Shoot, explosion, pickup sounds
- [x] **WebGL Audio Unlock** — JavaScript AudioContext resume for browser autoplay policy

### UI & HUD
- [x] **Main Menu** — Play button, animated drifting objects
- [x] **HUD** — Health bar, wave counter, score, enemy/meteor kill counters
- [x] **Results Screen** — Enemy kills, meteor kills, waves survived, **Score**, **Best (High Score)**
- [x] **Boss Warning UI** — Dramatic warning screen before boss spawn
- [x] **Boss Health Bar** — Dedicated UI bar for boss HP

### Technical
- [x] **Unity 6** (6000.x) with URP 2D
- [x] **New Input System** (`Keyboard.current`) — WASD, Arrow Keys, Space, J
- [x] **PlayerPrefs Highscore** — Persisted locally (WebGL: IndexedDB)
- [x] **WebGL Compatible** — No `System.IO`, no `Application.Quit()`, no `DontDestroyOnLoad` conflicts

---

## 🏗️ Project Architecture

```
Assets/
├── Prefabs/
│   ├── Enemies/            # 5 enemy type prefabs (Scout/Fighter/Interceptor/Bomber/Elite)
│   ├── Pickups/            # ShieldPickup
│   ├── PlayerShip.prefab
│   ├── EnemyProjectile.prefab
│   ├── Meteor.prefab / Meteor2.prefab / Meteor3.prefab / Meteor4.prefab
│   ├── LaserProjectile.prefab / MissileProjectile.prefab / PlasmaProjectile.prefab
│   ├── LaserPickup.prefab / MissilePickup.prefab / PlasmaPickup.prefab
│   └── Boss_C7.prefab
├── Scenes/
│   └── SampleScene.unity   # Single-scene game
├── Scripts/Core/
│   ├── GameManager.cs          # Singleton — state machine, scoring, PlayerPrefs
│   ├── PlayerController.cs     # Movement + 4-weapon shooting system
│   ├── EnemyController.cs      # AI v2 — drift, aim, spread shot, wave scaling
│   ├── WaveSpawner.cs          # Wave loop + weighted enemy spawn table
│   ├── Damageable.cs           # Generic HP component + invincibility support
│   ├── Projectile.cs           # Blaster/Laser/Missile/Plasma behaviour
│   ├── MeteorController.cs     # Movement + item drop system
│   ├── ItemPickup.cs           # Health/Weapon/Shield pickup logic
│   ├── PlayerShield.cs         # Shield timer, visual, flash warning
│   ├── MusicManager.cs         # Cross-fade music (Menu/Playing/Boss)
│   ├── WebGLAudioUnlock.cs     # Browser AudioContext resume fix
│   ├── HUDManager.cs           # Health bar, wave, score, kill counters
│   ├── WeaponHUDManager.cs     # Weapon icon + level display
│   ├── ResultsUI.cs            # Results screen with Score + HighScore
│   ├── GameOverUI.cs           # Game Over panel
│   ├── BossController.cs       # Boss AI — phases, dive, bullet patterns
│   ├── BossHealthBarUI.cs      # Boss HP bar UI
│   ├── BossWarningUI.cs        # Pre-boss warning sequence
│   ├── BackgroundScroller.cs   # Infinite vertical scrolling background
│   ├── CameraShake.cs          # Impact camera shake
│   ├── HealthBar.cs            # World-space procedural health bar
│   └── AutoDestroy.cs          # VFX self-destruct timer
├── Sound/                  # SFX + CC0 music tracks
└── Sprites/                # Ship, enemy, meteor, background sprites
```

### Key Design Decisions

| Feature | Implementation |
|---------|---------------|
| **Input** | New Input System (`Keyboard.current`) |
| **HP System** | Generic `Damageable` with `OnHealthChanged` / `OnDeath` / `isInvincible` |
| **State Machine** | `GameManager`: `Menu → Playing → GameOver → Results` |
| **Enemy Spawning** | `EnemySpawnEntry[]` with `minWave` + `weight` for weighted random |
| **Music** | Dual-`AudioSource` cross-fade, `Time.unscaledDeltaTime` (pause-safe) |
| **Persistence** | `PlayerPrefs` — works on WebGL (IndexedDB) |
| **Rendering** | URP 2D, `SpriteRenderer`, Sorting Layers |

---

## 🤖 AI Assistant Integration (MCP Unity)

This project was developed with **Unity MCP (Model Context Protocol)** for AI-assisted development.

### Important Notes for AI Assistants

> ⚠️ **Namespace Conflicts:** When using `Unity_RunCommand`, wrap code in the class structure. Use fully qualified types:
> - `Image` → `UnityEngine.UI.Image`
> - `CanvasScaler` → `UnityEngine.UI.CanvasScaler`

> ⚠️ **Input System:** Uses **New Input System** exclusively. Do NOT use `UnityEngine.Input`. Use `UnityEngine.InputSystem.Keyboard.current`.

> ⚠️ **SerializedObject Pattern:** To wire references programmatically:
> ```csharp
> var so = new SerializedObject(component);
> so.FindProperty("fieldName").objectReferenceValue = targetObject;
> so.ApplyModifiedProperties();
> ```

---

## 📋 Tags

| Tag | Used By |
|-----|---------|
| `Player` | PlayerShip |
| `Enemy` | Enemy prefabs (Scout/Fighter/Interceptor/Bomber/Elite) |
| `Meteor` | Meteor prefabs |

---

## 📜 License

MIT License — Free to use, modify, and distribute.
