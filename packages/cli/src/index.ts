#!/usr/bin/env node
import { readFileSync } from 'node:fs'
import path from 'node:path'
import { program } from 'commander'
import { registerInit } from './commands/init.ts'
import { registerExecute } from './commands/execute.ts'
import { registerStatus } from './commands/status.ts'

const { version } = JSON.parse(
  readFileSync(path.join(import.meta.dirname, '..', 'package.json'), 'utf-8'),
) as { version: string }

program
  .name('unibridge')
  .description('Bridge between Unity and your code')
  .version(version)
  .option('-p, --project <path>', 'Path to Unity project')
  .option('--json', 'Output result as JSON')
  .option('--no-execute', 'Disable execute tool for this invocation')

registerInit(program)
registerExecute(program)
registerStatus(program)

program.parse()
