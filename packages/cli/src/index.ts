#!/usr/bin/env node
import { program } from 'commander'
import { registerInit } from './commands/init.ts'
import { registerExecute } from './commands/execute.ts'

program
  .name('unibridge')
  .description('Bridge between Unity and your code')
  .version('0.1.0')
  .option('--no-execute', 'Disable execute tool for this invocation')

registerInit(program)
registerExecute(program)

program.parse()
