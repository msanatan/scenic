# UniBridge

Interact with Unity Editor projects using the command line and TypeScript.

## Packages

- SDK: [`@unibridge/sdk`](./packages/sdk/README.md)
- CLI: [`@unibridge/cli`](./packages/cli/README.md)
- Unity plugin (UPM package): [`unity`](./unity/README.md)

## Install

CLI:

```bash
npm install -g @unibridge/cli
```

SDK:

```bash
npm install @unibridge/sdk
```

Unity plugin:

Use the CLI in the folder of the Unity project and run:

```bash
unibridge init
```

The `execute` function allows you to execute C# code directly in Unity. It's disabled by default. You can enable it explicitly per project:

```bash
unibridge init --enable-execute
```

You can also install the plugin using Unity Package Manager (git URL):

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

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) before opening issues or pull requests. The short version: **open an issue first and wait for approval BEFORE submitting a PR.**

## Why?

Unity is a beast of a system, there's a lot to learn but it can do a lot. AI tools have been helpful for many learning the ropes and for professionals looking for quicker iterations. Your general purpose software development AI tool can probably get you through 80% of a project. Add an MCP, you'll get through about 90% of your project. A bespoke tool like [Coplay](https://coplay.dev/) can get you 95% there. So why this CLI?

- It's fast. We use Unix domain sockets for Mac/Linux and Named Pipes on Windows (Node.js abstracts this for us), if you're coming from MCP you'll feel the speed difference when it makes changes in Unity.
- CLI/SDK do not bloat context with all the capabilities from the get-go, agents can easily read what they need to learn how to use it
- Chaining commands in Unity via Editor scripts forces domain reloads, which slows you down. With the CLI or SDK, you can orchestrate multiple tasks in Unity and skip many domain reloads \- saving you a lot of time
- LLMs are really good at using CLIs in bash and PowerShell, and they're really good at generating JavaScript/TypeScript. Whether you're doing one off tasks or orchestrating many tasks, this tool will get the most out of your model.
- We're still figuring it out. How agents interact with applications is very much an open space. It's worth exploring and pushing the boundaries at the options we have available.
