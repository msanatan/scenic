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

Or provide a specific project path:

```bash
unibridge --project /path/to/UnityProject init
```

## Common Commands

```bash
unibridge status
unibridge domain reload
unibridge scene active
unibridge scene create Assets/Scenes/NewScene.unity
unibridge scene open Assets/Scenes/SampleScene.unity
```

Run commands with JSON output:

```bash
unibridge --json status
```
