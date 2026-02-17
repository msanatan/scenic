# UniBridge

Interact with Unity Editor projects using TypeScript and the command line.

## Packages

- SDK: [`@unibridge/sdk`](./packages/sdk/README.md)
- CLI: [`@unibridge/cli`](./packages/cli/README.md)
- Unity plugin (UPM package): [`unity/package.json`](./unity/package.json)

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

## Project Status

UniBridge is in active development. The current docs focus on setup, basics, and release-aligned usage.

## Documentation Direction

Planned documentation improvements:

- Generated SDK API docs from TypeScript types
- Generated CLI reference from command help output
