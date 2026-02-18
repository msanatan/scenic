import { domainReloadCommand } from './domain/contract.ts'
import { executeCommand } from './execute/contract.ts'
import { logsCommand } from './log/contract.ts'
import { sceneActiveCommand, sceneCreateCommand, sceneOpenCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'
import { testListCommand, testRunCommand } from './test/contract.ts'

export const allCommands = [domainReloadCommand, executeCommand, logsCommand, statusCommand, sceneActiveCommand, sceneCreateCommand, sceneOpenCommand, testListCommand, testRunCommand] as const
