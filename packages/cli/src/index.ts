#!/usr/bin/env node
import { readFileSync } from 'node:fs'
import path from 'node:path'
import { program } from 'commander'
import { registerDomain } from './commands/domain.ts'
import { registerComponents } from './commands/components.ts'
import { registerEditor } from './commands/editor.ts'
import { registerInit } from './commands/init.ts'
import { registerExecute } from './commands/execute.ts'
import { registerGameObject } from './commands/gameobject.ts'
import { registerLayers } from './commands/layers.ts'
import { registerLogs } from './commands/logs.ts'
import { registerPrefab } from './commands/prefab.ts'
import { registerStatus } from './commands/status.ts'
import { registerScene } from './commands/scene.ts'
import { registerScriptableObject } from './commands/scriptableobject.ts'
import { registerTags } from './commands/tags.ts'
import { registerTest } from './commands/test.ts'

const { version } = JSON.parse(
  readFileSync(path.join(import.meta.dirname, '..', 'package.json'), 'utf-8'),
) as { version: string }

program
  .name('scenic')
  .description('Bridge between Unity and your code')
  .version(version)
  .option('-p, --project <path>', 'Path to Unity project')
  .option('--json', 'Output result as JSON')
  .option('--no-execute', 'Disable execute tool for this invocation')

registerDomain(program)
registerComponents(program)
registerEditor(program)
registerInit(program)
registerExecute(program)
registerGameObject(program)
registerLayers(program)
registerLogs(program)
registerPrefab(program)
registerStatus(program)
registerScene(program)
registerScriptableObject(program)
registerTags(program)
registerTest(program)

program.parse()
