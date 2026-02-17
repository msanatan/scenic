import { executeCommand } from './execute/contract.ts'
import { sceneActiveCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'

export const allCommands = [executeCommand, statusCommand, sceneActiveCommand] as const
