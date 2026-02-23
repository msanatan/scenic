# @scenicai/cli

CLI for interacting with Unity Editor projects through Scenic.

## Install

Use without global install:

```bash
npx @scenicai/cli --help
```

Install globally:

```bash
npm install -g @scenicai/cli
scenic --help
```

## Requirements

- Node.js `>=22.18.0`
- A Unity project
- Scenic Unity plugin installed in that project

## Setup

Install the Unity plugin in the current Unity project:

```bash
scenic init
```

The `execute` function allows you to execute C# code directly in Unity. It's disabled by default. You can enable it explicitly per project:

```bash
scenic init --enable-execute
```

`update` is an alias for `init` and can be used to re-run this idempotently after SDK/CLI upgrades:

```bash
scenic update
```

Or provide a specific project path:

```bash
scenic --project /path/to/UnityProject init
```

## Quick Start (3D Vertical Slice)

This creates a tiny playable setup: a tagged cylinder player with physics, a floor, and two enemy cubes.
The example uses `jq` to capture IDs from `--json` output.

```bash
# 1) Ensure the Player tag exists (idempotent).
scenic tags add Player

# 2) Create Player (cylinder), then tag it as Player.
PLAYER_ID=$(scenic --json gameobject create Player --dimension 3d --primitive cylinder --position 0,1,0 | jq -r '.result.instanceId')
scenic gameobject update --instance-id "$PLAYER_ID" --tag Player

# 3) Add Rigidbody so the player can interact with physics.
scenic components add --instance-id "$PLAYER_ID" --type UnityEngine.Rigidbody --values '{"mass":70,"useGravity":true}'

# 4) Create a floor and two enemies.
scenic gameobject create ArenaFloor --dimension 3d --primitive plane --position 0,0,0 --scale 3,1,3
scenic gameobject create Enemy_A --dimension 3d --primitive cube --position 3,0.5,2
scenic gameobject create Enemy_B --dimension 3d --primitive cube --position -3,0.5,2

# 5) Inspect the scene and enter Play Mode.
scenic scene hierarchy --limit 200 --offset 0
scenic editor play
```

## Common Commands

```bash
scenic status
scenic logs --severity warn --limit 100 --offset 0
scenic components list --path /Player --limit 20 --offset 0
scenic components get --instance-id 12345 --component-instance-id 67890
scenic components add --instance-id 12345 --type UnityEngine.Rigidbody --values '{"mass":5.5,"useGravity":false}'
scenic components update --instance-id 12345 --component-instance-id 67890 --values '{"mass":7.5}'
scenic components remove --instance-id 12345 --component-instance-id 67890
scenic gameobject create Player --dimension 2d --position 0,1,0
scenic gameobject get --instance-id 12345
scenic gameobject find Enemy --limit 50 --offset 0
scenic gameobject create Enemy --parent-instance-id 12345 --primitive cube
scenic gameobject reparent --instance-id 12345 --parent-instance-id 67890
scenic gameobject destroy --instance-id 12345
scenic prefab instantiate Assets/Prefabs/Enemy.prefab --position 0,1,0
scenic prefab save Assets/Prefabs/EnemyVariant.prefab --instance-id 12345
scenic layers get --limit 32 --offset 0
scenic layers add EnemyLayer
scenic layers remove EnemyLayer
scenic tags get
scenic tags add Enemy
scenic tags remove Enemy
scenic test list --mode edit --limit 50 --offset 0
scenic test run --mode edit --filter DomainReloadCommandHandlerTests
scenic editor play
scenic editor pause
scenic editor stop
scenic domain reload
scenic scene active
scenic scene list --limit 50 --offset 0
scenic scene hierarchy --limit 200 --offset 0
scenic scene create Assets/Scenes/NewScene.unity
scenic scene open Assets/Scenes/SampleScene.unity
scenic scriptableobject create Assets/Data/EnemyConfig.asset --type Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject --values '{"number":5,"label":"Enemy","enabledFlag":true}'
scenic scriptableobject get Assets/Data/EnemyConfig.asset
scenic scriptableobject update Assets/Data/EnemyConfig.asset --values '{"number":7.5}'
```

Run commands with JSON output:

```bash
scenic --json status
```

## Identity

- `gameobject create` responses include `instanceId` and `path`.
- `scene hierarchy` node entries include `instanceId`, `path`, `parentIndex`, and `siblingIndex`.
- `instanceId` is session-local and ideal for follow-up commands during an active Unity session.
