# Scenic

Interact with Unity Editor projects using the command line and TypeScript.

## Packages

- SDK: [`@scenicai/sdk`](./packages/sdk/README.md)
- CLI: [`@scenicai/cli`](./packages/cli/README.md)
- Unity plugin (UPM package): [`unity`](./unity/README.md)

## Install

CLI:

```bash
npm install -g @scenicai/cli
```

SDK:

```bash
npm install @scenicai/sdk
```

Unity plugin:

Use the CLI in the folder of the Unity project and run:

```bash
scenic init
```

The `execute` function allows you to execute C# code directly in Unity. It's disabled by default. You can enable it explicitly per project:

```bash
scenic init --enable-execute
```

To inspect or change settings while Unity is running:

```bash
scenic settings get
scenic settings update --execute-enabled true
```

You can also install the plugin using Unity Package Manager (git URL):

```text
https://github.com/msanatan/scenic.git?path=unity
```

## Minimum Versions

- Node.js: `>=22.18.0`
- Unity: `2021.3+`

## Getting Started

1. Add the Unity plugin to your project via `scenic init`
   - You can also install the package in your Unity project directly with a git URL.
2. Install either the SDK or CLI depending on your workflow.
3. Use package-specific docs:
   - SDK usage and examples: [`packages/sdk/README.md`](./packages/sdk/README.md)
   - CLI usage and examples: [`packages/cli/README.md`](./packages/cli/README.md)

## Capabilities

Scenic gives you programmatic control over the Unity Editor from the command line or TypeScript. Below is a summary of what you can do today.

| Area | What you can do |
|---|---|
| **GameObjects** | Create, inspect, update, destroy, reparent, and find GameObjects. Supports 2D/3D primitives (cube, sphere, capsule, cylinder, plane, quad), transforms (position, rotation, scale in world or local space), and parent-child relationships. |
| **Components** | List, add, get, update, and remove components on any GameObject. Pass field values as JSON inline or from a file. |
| **Scenes** | Get the active scene, list all project scenes, inspect the hierarchy as a flat tree, create new scenes, and open existing ones. |
| **Prefabs** | Instantiate a prefab into the active scene with optional transform and parent. Save a GameObject hierarchy back to a prefab asset. |
| **ScriptableObjects** | Create ScriptableObject assets, read serialized data, and update fields using JSON patches. |
| **Editor Control** | Start, pause, and stop Unity play mode. |
| **Domain Reload** | Trigger an asset refresh and domain reload on demand. |
| **Logs** | Read Unity Editor console logs, filtered by severity (info, warn, error). |
| **Packages (UPM)** | List installed Unity packages with pagination/search, and add or remove direct dependencies idempotently. |
| **Tests** | List and run Unity Test Framework tests in edit or play mode, with name filtering. |
| **Layers & Tags** | Inspect, add, and remove project layers and tags. Built-in layers and tags are protected. |
| **Execute** | Run arbitrary C# code in the Unity Editor. Disabled by default; opt in with `scenic init --enable-execute`. |

All listing operations support **pagination** (`--limit` / `--offset`). Pass `--json` to any CLI command for machine-readable output.

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) before opening issues or pull requests. The short version: **open an issue first and wait for approval BEFORE submitting a PR.**

## Why?

Unity is a beast of a system, there's a lot to learn but it can do a lot. AI tools have been helpful for many learning the ropes and for professionals looking for quicker iterations. Your general purpose software development AI tool can probably get you through 80% of a project. Add an MCP, you'll get through about 90% of your project. A bespoke tool like [Coplay](https://coplay.dev/) can get you 95% there. So why this CLI?

- It's fast. We use Unix domain sockets for Mac/Linux and Named Pipes on Windows (Node.js abstracts this for us), if you're coming from MCP you'll feel the speed difference when it makes changes in Unity.
- CLI/SDK do not bloat context with all the capabilities from the get-go, agents can easily read what they need to learn how to use it
- Chaining commands in Unity via Editor scripts forces domain reloads, which slows you down. With the CLI or SDK, you can orchestrate multiple tasks in Unity and skip many domain reloads \- saving you a lot of time
- LLMs are really good at using CLIs in bash and PowerShell, and they're really good at generating JavaScript/TypeScript. Whether you're doing one off tasks or orchestrating many tasks, this tool will get the most out of your model.
- We're still figuring it out. How agents interact with applications is very much an open space. It's worth exploring and pushing the boundaries at the options we have available.
