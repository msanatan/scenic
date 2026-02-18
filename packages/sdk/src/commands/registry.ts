import { domainReloadCommand } from './domain/contract.ts'
import { executeCommand } from './execute/contract.ts'
import { logsCommand } from './log/contract.ts'
import { sceneActiveCommand, sceneCreateCommand, sceneOpenCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'

export const allCommands = [domainReloadCommand, executeCommand, logsCommand, statusCommand, sceneActiveCommand, sceneCreateCommand, sceneOpenCommand] as const
