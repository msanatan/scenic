# @scenicai/sdk

Scenic is a TypeScript SDK for interacting with Unity Editor.

## Install

```bash
npm install @scenicai/sdk
```

## Requirements

- Node.js `>=22.18.0`
- A Unity project
- Scenic Unity plugin installed in that project

You can install the plugin with the CLI, or by adding this to your Unity `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.msanatan.scenic": "https://github.com/msanatan/scenic.git?path=unity"
  }
}
```

## Quick Start

```ts
import { createClient } from '@scenicai/sdk'

const client = createClient({ projectPath: '/path/to/UnityProject' })

try {
  const status = await client.status()
  console.log('Unity status:', status)

  // Ensure Player tag exists, then create a simple tagged player body.
  await client.tagsAdd({ name: 'Player' })
  const player = await client.gameObjectCreate({
    name: 'Player',
    dimension: '3d',
    primitive: 'cylinder',
    transform: { space: 'world', position: { x: 0, y: 1, z: 0 } },
  })
  await client.gameObjectUpdate({ instanceId: player.instanceId, tag: 'Player' })

  await client.componentsAdd({
    instanceId: player.instanceId,
    type: 'UnityEngine.Rigidbody',
    initialValues: { mass: 70, useGravity: true },
  })

  // Add a floor and two enemies for a tiny 3D gameplay slice.
  await client.gameObjectCreate({
    name: 'ArenaFloor',
    dimension: '3d',
    primitive: 'plane',
    transform: { space: 'world', position: { x: 0, y: 0, z: 0 }, scale: { x: 3, y: 1, z: 3 } },
  })

  await client.gameObjectCreate({
    name: 'Enemy_A',
    dimension: '3d',
    primitive: 'cube',
    transform: { space: 'world', position: { x: 3, y: 0.5, z: 2 } },
  })
  await client.gameObjectCreate({
    name: 'Enemy_B',
    dimension: '3d',
    primitive: 'cube',
    transform: { space: 'world', position: { x: -3, y: 0.5, z: 2 } },
  })

  const hierarchy = await client.sceneHierarchy({ limit: 200, offset: 0 })
  console.log('Scene objects:', hierarchy.total)

  await client.editorPlay()
  console.log('Entered Play Mode')
} finally {
  client.close()
}
```

## Install Plugin Programmatically

```ts
import { init } from '@scenicai/sdk'

await init({
  projectPath: '/path/to/UnityProject',
})
```

The `execute` function allows you to execute C# code directly in Unity. It's disabled by default. You can enable it by setting this flag when calling `init()`:

```ts
await init({
  projectPath: '/path/to/UnityProject',
  enableExecute: true,
})
```

Learn more about Scenic [here](https://github.com/msanatan/scenic).
