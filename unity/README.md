# com.msanatan.scenic

Scenic is a Unity Editor plugin that lets external tools communicate with your open Unity project over a local IPC bridge.

It is designed to be used with:

- [`@scenicai/cli`](../packages/cli/README.md) for command-line workflows
- [`@scenicai/sdk`](../packages/sdk/README.md) for TypeScript automation

## What It Does

- Exposes Unity Editor commands (status, scene operations, domain reload, execute)
- Enables local automation and tooling from Node.js

## Install The Plugin In Unity

### Option 1: From CLI (recommended)

Install CLI:

```bash
npm install -g @scenicai/cli
```

From your Unity project root:

```bash
scenic init
```

`init` is idempotent. You can also run:

```bash
scenic update
```

(`update` is an alias of `init`.)

### Option 2: From SDK

Install SDK:

```bash
npm install @scenicai/sdk
```

Then initialize from code:

```ts
import { init } from '@scenicai/sdk'

await init({ projectPath: '/path/to/UnityProject' })
```

### Option 3: Add UPM Git Dependency Manually

Add this to your Unity `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.msanatan.scenic": "https://github.com/msanatan/scenic.git?path=unity"
  }
}
```

## Requirements

- Unity `2021.3+`
- Node.js `>=22.18.0` (for CLI/SDK usage)

## Verify Setup

With Unity open on your project:

```bash
scenic status
```

## OpenUPM

OpenUPM support is planned. Until then, install with the Git URL or use `scenic init`.
