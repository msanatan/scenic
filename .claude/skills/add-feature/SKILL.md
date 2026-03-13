---
name: add-feature
description: Scaffold and implement a new Scenic command across Unity, SDK, and CLI layers.
user-invokable: true
argument-hint: "[feature-name]"
---

# Add Feature

Add a new command to Scenic following the three-tier architecture: Unity plugin (C#) → SDK (TypeScript/Valibot) → CLI (TypeScript/Commander).

## Step 1: Feature Specification

1. Read `FEATURES.md` at the project root. Find the section matching the requested feature.
2. Determine the **namespace** (lowercase singular, e.g., `material`, `animation`, `asset`).
3. List the **actions** needed (e.g., `create`, `get`, `update`, `remove`, `list`, `search`).
4. For each action, decide:
   - Wire command name: `<namespace>.<action>`
   - Request fields (params) and response fields (result)
5. If the scope is ambiguous, ask the user before proceeding.

## Step 2: Scaffold

Run the scaffolding script once per action:

```bash
npm run scaffold -- <namespace> <action>
```

Examples:
```bash
npm run scaffold -- material create
npm run scaffold -- material get
npm run scaffold -- material update
npm run scaffold -- material remove
```

Use `--dry-run` first to preview what will be created. After scaffolding, verify all expected files exist.

## Step 3: Unity Models

Fill in the generated models file at `unity/Editor/Commands/<Namespace>/<Namespace><Action>CommandModels.cs`:

- Add parameter fields with `[JsonProperty("camelCase")]` attributes to the Params class.
- Add result fields with `[JsonProperty("camelCase")]` attributes to the Result class.
- Implement the `From(CommandRequest)` factory with validation. Use `CommandModelHelpers.ParsePayload(request)` for JSON parsing.
- Throw `CommandHandlingException` for invalid input with actionable messages.

Reference: `unity/Editor/Commands/Layers/LayersAddCommandModels.cs` for a clean example.

## Step 4: Unity Handler

Fill in the handler at `unity/Editor/Commands/<Namespace>/<Namespace><Action>CommandHandler.cs`:

- Implement the `Handle()` body with the actual Unity API calls.
- For state-changing commands, call `EditorUtility.SetDirty()` and `AssetDatabase.SaveAssets()` as needed.
- Never generate `.meta` files manually — let Unity handle them.
- Keep the handler focused: extract reusable helpers only if there is clear reuse across multiple handlers in the same namespace.

Reference: `unity/Editor/Commands/Layers/LayersAddCommandHandler.cs`

## Step 5: Unity Tests

Fill in the test at `unity/Tests/Editor/Commands/<Namespace>/<Namespace><Action>CommandHandlerTests.cs`:

- Test via `CommandRouter.Route(new CommandRequest { ... }, executeEnabled: true)`.
- Cover at minimum: one success path and one validation error path.
- Clean up created objects/assets in `[TearDown]`.
- Keep tests deterministic — no dependency on execution order.

Reference: `unity/Tests/Editor/Commands/Layers/LayersAddCommandHandlerTests.cs`

## Step 6: SDK Contract

Fill in the contract at `packages/sdk/src/commands/<namespace>/contract.ts`:

- Define Valibot schemas for input and result that match the Unity model fields exactly.
- Field names in schemas must be camelCase, matching the `[JsonProperty]` values in C#.
- Fill in the `params` mapping function to convert typed input to wire params.

Reference: `packages/sdk/src/commands/layer/contract.ts`

**Checkpoint:** Run `npm run build` to confirm TypeScript compiles.

## Step 7: SDK Registration

Wire up the new command(s):

1. **Registry** — In `packages/sdk/src/commands/registry.ts`:
   - Import the command definition(s) from the contract file.
   - Add them to the `allCommands` array.

2. **Contracts barrel** — In `packages/sdk/src/commands/contracts.ts`:
   - Export all public types (Input, Result, shared item types).

Reference: Look at existing imports/exports for `layers` in both files.

## Step 8: CLI Command

Fill in the CLI command at `packages/cli/src/commands/<namespace>.ts`:

- Define the Deps interface with the SDK method signature.
- Parse CLI options (strings from Commander) into typed SDK input in the handler.
- Format human-readable output in the `runWithOutput` callback.
- Add `.option()` and `.argument()` calls to the Commander subcommand.
- Use `withUnityClient(command, { requirePlugin: true }, ...)` for the action.

Reference: `packages/cli/src/commands/layers.ts`

**Checkpoint:** Run `npm run build` to confirm full build succeeds.

## Step 9: CLI Registration

In `packages/cli/src/index.ts`:

1. Import the `register<Namespace>` function.
2. Add a `register<Namespace>(program)` call alongside the existing registrations.

## Step 10: Integration Tests

Fill in the integration tests:

- **SDK test** at `integration/sdk/safe/<namespace>.test.ts` — use `createTestClient()` and call `client.<method>()`.
- **CLI test** at `integration/cli/safe/<namespace>.test.ts` — use `runCli('<namespace>', '<action>', ...)` and assert the JSON response shape.
- Ensure `after()` hooks properly clean up and `await` all async teardown.

Reference: `integration/sdk/safe/layers.test.ts`, `integration/cli/safe/layers.test.ts`

## Step 11: Final Verification

1. Run `npm run build` — full build must pass.
2. Run `npm test` — unit tests must pass.
3. Run targeted integration tests if a Unity instance is connected.
4. Update `FEATURES.md` — change `- [ ]` to `- [x]` for shipped items.
5. Remind the user to open Unity Editor for `.meta` file generation via domain reload.

Review the full checklist in `ADD_FEATURE.md` for any remaining items.

## Naming Conventions

Given `namespace` = `material` and `action` = `create`:

| Context | Convention | Example |
|---------|-----------|---------|
| Wire command | `namespace.action` | `material.create` |
| SDK method | `namespaceAction` | `materialCreate` |
| SDK command var | `namespaceActionCommand` | `materialCreateCommand` |
| SDK schema | `NamespaceActionResultSchema` | `MaterialCreateResultSchema` |
| SDK type | `NamespaceActionResult` | `MaterialCreateResult` |
| CLI group | `namespace` | `material` |
| CLI subcommand | `action` | `create` |
| CLI register fn | `registerNamespace` | `registerMaterial` |
| CLI handler fn | `handleNamespaceAction` | `handleMaterialCreate` |
| Unity namespace | `Scenic.Editor.Commands.Namespace` | `Scenic.Editor.Commands.Material` |
| Unity handler | `NamespaceActionCommandHandler` | `MaterialCreateCommandHandler` |
| Unity params | `NamespaceActionCommandParams` | `MaterialCreateCommandParams` |
| Unity result | `NamespaceActionCommandResult` | `MaterialCreateCommandResult` |
| Unity attribute | `[ScenicCommand("namespace.action")]` | `[ScenicCommand("material.create")]` |
| Unity folder | `unity/Editor/Commands/Namespace/` | `unity/Editor/Commands/Material/` |
| Unity test class | `NamespaceActionCommandHandlerTests` | `MaterialCreateCommandHandlerTests` |
| JSON properties | camelCase | `fieldName` |

## Key Reference Files

- `packages/sdk/src/commands/define.ts` — `defineCommand()` API and types
- `packages/sdk/src/commands/registry.ts` — SDK command registration
- `packages/sdk/src/commands/contracts.ts` — SDK type exports
- `packages/cli/src/commands/output.ts` — `runWithOutput()` helper
- `packages/cli/src/commands/with-unity-client.ts` — `withUnityClient()` helper
- `packages/cli/src/index.ts` — CLI command registration
- `unity/Editor/Commands/CommandModelHelpers.cs` — shared parsing utilities
- `unity/Editor/Commands/ICommandHandler.cs` — handler interface
- `unity/Editor/Commands/CommandAttribute.cs` — `[ScenicCommand]` attribute
- `ADD_FEATURE.md` — full add-feature checklist
- `FEATURES.md` — feature roadmap with checkboxes
