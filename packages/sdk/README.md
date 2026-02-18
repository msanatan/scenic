# @unibridge/sdk

TypeScript SDK for interacting with Unity Editor.

## Install

```bash
npm install @unibridge/sdk
```

## Requirements

- Node.js `>=22.18.0`
- A Unity project
- UniBridge Unity plugin installed in that project

You can install the plugin with the CLI, or by adding this to your Unity `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.msanatan.unibridge": "https://github.com/msanatan/unibridge.git?path=unity"
  }
}
```

## Quick Start

```ts
import { createClient } from '@unibridge/sdk'

const client = createClient({ projectPath: '/path/to/UnityProject' })

const status = await client.status()
console.log(status)

const logs = await client.logs({ severity: 'warn', limit: 50, offset: 0 })
console.log(logs)

const tests = await client.testList({ mode: 'edit', limit: 50, offset: 0 })
console.log(tests)

const hierarchy = await client.sceneHierarchy({ limit: 200, offset: 0 })
console.log(hierarchy)

const run = await client.testRun({ mode: 'edit', filter: 'DomainReloadCommandHandlerTests' })
console.log(run)

client.close()
```

## Install Plugin Programmatically

```ts
import { init } from '@unibridge/sdk'

await init({
  projectPath: '/path/to/UnityProject',
})
```
