# UniBridge

Interact with Unity Editor projects using TypeScript and the command line.

## Packages

- SDK: [`@unibridge/sdk`](./packages/sdk/README.md)
- CLI: [`@unibridge/cli`](./packages/cli/README.md)
- Unity plugin (UPM package): [`unity`](./unity/README.md)

## Install

SDK:

```bash
npm install @unibridge/sdk
```

CLI:

```bash
npm install -g @unibridge/cli
```

Unity plugin (Unity Package Manager, git URL):

```text
https://github.com/msanatan/unibridge.git?path=unity
```

## Minimum Versions

- Node.js: `>=22.18.0`
- Unity: `2021.3+`

## Getting Started

1. Add the Unity plugin to your project via `unibridge init`
   - You can also install the package in your Unity project directly with a git URL.
2. Install either the SDK or CLI depending on your workflow.
3. Use package-specific docs:
   - SDK usage and examples: [`packages/sdk/README.md`](./packages/sdk/README.md)
   - CLI usage and examples: [`packages/cli/README.md`](./packages/cli/README.md)

## Future Work

Planned improvements include command-level batch mutations for GameObjects.

- Add `gameobject.createMany` / `unibridge gameobject create-many` for creating multiple GameObjects in one request.
- Add `gameobject.destroyMany` / `unibridge gameobject destroy-many` for deleting multiple GameObjects in one request.
- Batch support is planned per command (not a mixed-command batch endpoint), with per-item success/error reporting.
