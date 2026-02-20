import { componentsAddCommand, componentsGetCommand, componentsListCommand, componentsRemoveCommand, componentsUpdateCommand } from './component/contract.ts'
import { domainReloadCommand } from './domain/contract.ts'
import { editorPauseCommand, editorPlayCommand, editorStopCommand } from './editor/contract.ts'
import { executeCommand } from './execute/contract.ts'
import { gameObjectCreateCommand, gameObjectDestroyCommand, gameObjectFindCommand, gameObjectGetCommand, gameObjectReparentCommand, gameObjectUpdateCommand } from './gameobject/contract.ts'
import { layersAddCommand, layersGetCommand, layersRemoveCommand } from './layer/contract.ts'
import { logsCommand } from './log/contract.ts'
import { prefabInstantiateCommand, prefabSaveCommand } from './prefab/contract.ts'
import { sceneActiveCommand, sceneCreateCommand, sceneHierarchyCommand, sceneListCommand, sceneOpenCommand } from './scene/contract.ts'
import { statusCommand } from './status/contract.ts'
import { tagsAddCommand, tagsGetCommand, tagsRemoveCommand } from './tag/contract.ts'
import { testListCommand, testRunCommand } from './test/contract.ts'

export const allCommands = [componentsListCommand, componentsAddCommand, componentsGetCommand, componentsRemoveCommand, componentsUpdateCommand, domainReloadCommand, editorPlayCommand, editorPauseCommand, editorStopCommand, executeCommand, gameObjectCreateCommand, gameObjectDestroyCommand, gameObjectUpdateCommand, gameObjectReparentCommand, gameObjectGetCommand, gameObjectFindCommand, layersGetCommand, layersAddCommand, layersRemoveCommand, logsCommand, prefabInstantiateCommand, prefabSaveCommand, statusCommand, sceneListCommand, sceneHierarchyCommand, sceneActiveCommand, sceneCreateCommand, sceneOpenCommand, tagsGetCommand, tagsAddCommand, tagsRemoveCommand, testListCommand, testRunCommand] as const
