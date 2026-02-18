import { domainReloadCommand } from './domain/contract.ts'
import { executeCommand } from './execute/contract.ts'
import { gameObjectCreateCommand, gameObjectDestroyCommand } from './gameobject/contract.ts'
import { logsCommand } from './log/contract.ts'
import { sceneActiveCommand, sceneCreateCommand, sceneHierarchyCommand, sceneListCommand, sceneOpenCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'
import { testListCommand, testRunCommand } from './test/contract.ts'

export const allCommands = [domainReloadCommand, executeCommand, gameObjectCreateCommand, gameObjectDestroyCommand, logsCommand, statusCommand, sceneListCommand, sceneHierarchyCommand, sceneActiveCommand, sceneCreateCommand, sceneOpenCommand, testListCommand, testRunCommand] as const
