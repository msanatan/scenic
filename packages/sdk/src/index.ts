export { init, findUnityProject, isPluginInstalled } from './project.ts'
export { createClient } from './client.ts'
export { readExecuteEnabled, writeExecuteEnabled } from './config.ts'
export type {
  InitOptions,
  InitResult,
  CommandResponse,
  ClientOptions,
  ScenicClient,
} from './types.ts'
