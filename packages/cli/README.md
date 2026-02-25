# @scenicai/cli

Scenic is a CLI for interacting with the Unity editor.

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

Run commands with JSON output:

```bash
scenic --json status
```

Learn more about Scenic [here](https://github.com/msanatan/scenic).
