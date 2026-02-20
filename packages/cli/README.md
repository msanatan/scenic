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
