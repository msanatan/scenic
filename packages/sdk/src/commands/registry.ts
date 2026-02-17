import { executeCommand } from './execute/contract.ts'
import { sceneActiveCommand, sceneCreateCommand, sceneOpenCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'

export const allCommands = [executeCommand, statusCommand, sceneActiveCommand, sceneCreateCommand, sceneOpenCommand] as const
