# @unibridge/cli

CLI for interacting with Unity Editor projects through UniBridge.

## Install

Use without global install:

```bash
npx @unibridge/cli --help
```

Install globally:

```bash
npm install -g @unibridge/cli
unibridge --help
```

## Requirements

- Node.js `>=22.18.0`
- A Unity project
- UniBridge Unity plugin installed in that project

## Setup

Install the Unity plugin in the current Unity project:

```bash
unibridge init
```

`update` is an alias for `init` and can be used to re-run this idempotently after SDK/CLI upgrades:

```bash
unibridge update
```

Or provide a specific project path:

```bash
unibridge --project /path/to/UnityProject init
```

## Quick Start (3D Vertical Slice)

This creates a tiny playable setup: a tagged cylinder player with physics, a floor, and two enemy cubes.
The example uses `jq` to capture IDs from `--json` output.

```bash
# 1) Ensure the Player tag exists (idempotent).
unibridge tags add Player

# 2) Create Player (cylinder), then tag it as Player.
PLAYER_ID=$(unibridge --json gameobject create Player --dimension 3d --primitive cylinder --position 0,1,0 | jq -r '.result.instanceId')
unibridge gameobject update --instance-id "$PLAYER_ID" --tag Player

# 3) Add Rigidbody so the player can interact with physics.
unibridge components add --instance-id "$PLAYER_ID" --type UnityEngine.Rigidbody --values '{"mass":70,"useGravity":true}'

# 4) Create a floor and two enemies.
unibridge gameobject create ArenaFloor --dimension 3d --primitive plane --position 0,0,0 --scale 3,1,3
unibridge gameobject create Enemy_A --dimension 3d --primitive cube --position 3,0.5,2
unibridge gameobject create Enemy_B --dimension 3d --primitive cube --position -3,0.5,2

# 5) Inspect the scene and enter Play Mode.
unibridge scene hierarchy --limit 200 --offset 0
unibridge editor play
```

## Common Commands

```bash
unibridge status
unibridge logs --severity warn --limit 100 --offset 0
unibridge components list --path /Player --limit 20 --offset 0
unibridge components get --instance-id 12345 --component-instance-id 67890
unibridge components add --instance-id 12345 --type UnityEngine.Rigidbody --values '{"mass":5.5,"useGravity":false}'
unibridge components update --instance-id 12345 --component-instance-id 67890 --values '{"mass":7.5}'
unibridge components remove --instance-id 12345 --component-instance-id 67890
unibridge gameobject create Player --dimension 2d --position 0,1,0
unibridge gameobject get --instance-id 12345
unibridge gameobject find Enemy --limit 50 --offset 0
unibridge gameobject create Enemy --parent-instance-id 12345 --primitive cube
unibridge gameobject reparent --instance-id 12345 --parent-instance-id 67890
unibridge gameobject destroy --instance-id 12345
unibridge prefab instantiate Assets/Prefabs/Enemy.prefab --position 0,1,0
unibridge prefab save Assets/Prefabs/EnemyVariant.prefab --instance-id 12345
unibridge layers get --limit 32 --offset 0
unibridge layers add EnemyLayer
unibridge layers remove EnemyLayer
unibridge tags get
unibridge tags add Enemy
unibridge tags remove Enemy
unibridge test list --mode edit --limit 50 --offset 0
unibridge test run --mode edit --filter DomainReloadCommandHandlerTests
unibridge editor play
unibridge editor pause
unibridge editor stop
unibridge domain reload
unibridge scene active
unibridge scene list --limit 50 --offset 0
unibridge scene hierarchy --limit 200 --offset 0
unibridge scene create Assets/Scenes/NewScene.unity
unibridge scene open Assets/Scenes/SampleScene.unity
```

Run commands with JSON output:

```bash
unibridge --json status
```

## Identity

- `gameobject create` responses include `instanceId` and `path`.
- `scene hierarchy` node entries include `instanceId`, `path`, `parentIndex`, and `siblingIndex`.
- `instanceId` is session-local and ideal for follow-up commands during an active Unity session.
