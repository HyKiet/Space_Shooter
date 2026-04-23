# 🚀 Space Shooter - Unity 2D

A classic 2D space shooter game built with **Unity 6** and **Universal Render Pipeline (URP 2D)**.

![Unity](https://img.shields.io/badge/Unity-6000.x-blue?logo=unity)
![C#](https://img.shields.io/badge/C%23-12-purple?logo=csharp)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 🎮 Gameplay

- **Move:** `WASD` or `Arrow Keys`
- **Shoot:** `Space` or `J`
- Destroy enemy ships and dodge meteors
- Survive as many waves as possible
- Track your highscores!

---

## 🏗️ Project Architecture

```
Assets/
├── Fonts/                  # Xolonium font
├── Prefabs/
│   ├── PlayerShip.prefab   # Player with Damageable + PlayerController
│   ├── PlayerProjectile.prefab  # Bullet with Projectile script
│   ├── EnemyShip.prefab    # Enemy with EnemyController + HealthBar
│   ├── Meteor.prefab       # Small meteor
│   └── Meteor2.prefab      # Large meteor
├── Scenes/
│   └── SampleScene.unity   # Main game scene
├── Scripts/Core/
│   ├── GameManager.cs      # Singleton - game state, scoring, persistence
│   ├── PlayerController.cs # New Input System - movement + shooting
│   ├── Projectile.cs       # Bullet movement + collision damage
│   ├── EnemyController.cs  # Enemy AI - drift movement + contact damage
│   ├── MeteorController.cs # Meteor rotation + movement
│   ├── Damageable.cs       # Generic HP component with events
│   ├── WaveSpawner.cs      # Progressive wave spawning system
│   ├── HUDManager.cs       # Health bar, wave text, kill counters
│   ├── GameOverUI.cs       # Game Over panel (current vs highscore)
│   ├── ResultsUI.cs        # Results screen
│   ├── BackgroundScroller.cs # Infinite vertical scrolling
│   ├── AutoDestroy.cs      # Self-destruct timer for VFX
│   └── HealthBar.cs        # World-space procedural health bar
├── Sound/cc0/              # Sound effects (blaster, explosion)
└── Sprites/                # Ship, enemy, meteor, background sprites
```

### Key Design Decisions

| Feature | Implementation |
|---------|---------------|
| **Input** | New Input System (`Keyboard.current`) |
| **HP System** | Generic `Damageable` component with `OnHealthChanged` / `OnDeath` events |
| **UI** | Unity UI (`UnityEngine.UI`) with `CanvasScaler` (1920×1080 reference) |
| **State Machine** | `GameManager` singleton: `Menu → Playing → GameOver → Results` |
| **Persistence** | `PlayerPrefs` for highscores |
| **Rendering** | URP 2D with `SpriteRenderer` |

---

## 🛠️ Setup & Requirements

- **Unity Version:** 6000.x (Unity 6)
- **Render Pipeline:** Universal 2D
- **Input:** New Input System Package (active in Player Settings)
- **Packages Required:**
  - `com.unity.inputsystem`
  - `com.unity.render-pipelines.universal`

### Quick Start

```bash
git clone https://github.com/HyKiet/Space_Shooter.git
# Open in Unity Hub → Add project from disk
# Open Assets/Scenes/SampleScene.unity
# Press Play ▶️
```

---

## 🤖 AI Assistant Integration (MCP Unity)

This project is configured to work with **Unity MCP (Model Context Protocol)** for AI-assisted development. Any AI assistant (Claude, Gemini, GPT, etc.) can connect to Unity Editor and programmatically control the game.

### What is Unity MCP?

Unity MCP allows AI assistants to:
- 📝 Read and modify C# scripts
- 🎮 Create/modify GameObjects and Components
- 🔍 Inspect scene hierarchy and properties
- ▶️ Start/stop Play Mode
- 📊 Read console logs for debugging
- 🏗️ Generate assets (meshes, materials, sprites)

### Setup Guide for AI Assistants

#### Step 1: Install Unity MCP Package

In Unity Editor:
1. Open **Window → Package Manager**
2. Click **+ → Add package by name**
3. Enter: `com.unity.ai.assistant`
4. Click **Add**

Or via `manifest.json` (`Packages/manifest.json`):
```json
{
  "dependencies": {
    "com.unity.ai.assistant": "0.3.0-pre.1"
  }
}
```

#### Step 2: Enable MCP Server in Unity

1. Open **Window → AI → AI Assistant**
2. In the AI Assistant window, click **Settings** (gear icon)
3. Enable **MCP Server**
4. Note the **port number** (default: varies per instance)

#### Step 3: Configure MCP Client

Add the following to your AI tool's MCP configuration file:

**For Antigravity / Gemini CLI** (`~/.gemini/settings.json` or workspace `.gemini/settings.json`):
```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "node",
      "args": [
        "%LOCALAPPDATA%/Programs/Antigravity/resources/app/extensions/antigravity/bin/mcp_unity.js"
      ],
      "env": {
        "UNITY_MCP_PORT": "auto"
      }
    }
  }
}
```

**For Claude Desktop** (`claude_desktop_config.json`):
```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "npx",
      "args": ["-y", "@anthropic/unity-mcp-server"],
      "env": {
        "UNITY_PORT": "auto"
      }
    }
  }
}
```

**For VS Code (Copilot / Continue.dev)** (`.vscode/mcp.json`):
```json
{
  "servers": {
    "unity-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@anthropic/unity-mcp-server"]
    }
  }
}
```

#### Step 4: Verify Connection

Once connected, the AI assistant should be able to:
```
✅ Unity_RunCommand       - Execute C# in Unity Editor
✅ Unity_GetConsoleLogs   - Read errors/warnings
✅ Unity_Camera_Capture   - Screenshot scene/game view
✅ Unity_SceneView_*      - Multi-angle scene capture
✅ Unity_AssetGeneration_* - Generate meshes, materials, sprites
```

### MCP Tool Reference (for AI Assistants)

| Tool | Purpose | Example Use |
|------|---------|-------------|
| `Unity_RunCommand` | Execute C# code | Create objects, modify components, wire references |
| `Unity_GetConsoleLogs` | Debug errors | Check for runtime exceptions |
| `Unity_Camera_Capture` | Capture camera view | Validate visual output |
| `Unity_SceneView_CaptureMultiAngleSceneView` | 4-angle scene view | Validate 3D layouts |
| `Unity_AssetGeneration_GenerateAsset` | AI asset generation | Create meshes, sprites, materials |

### Important Notes for AI Assistants

> ⚠️ **Namespace Conflicts:** When using `Unity_RunCommand`, the execution environment wraps code in `Unity.AI.Assistant.Agent.Dynamic.Extension.Editor` namespace. This causes conflicts with:
> - `Image` → Use `UnityEngine.UI.Image` (fully qualified)
> - `CanvasScaler` → Use `UnityEngine.UI.CanvasScaler`
> - Other UI types may need full qualification

> ⚠️ **Input System:** This project uses **New Input System** exclusively. Do NOT use `UnityEngine.Input` (old API). Use `UnityEngine.InputSystem.Keyboard.current` instead.

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
| `Enemy` | EnemyShip, Meteor, Meteor2 |

---

## 📜 License

MIT License - Free to use, modify, and distribute.
