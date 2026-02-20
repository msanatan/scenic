import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { stateDir } from './hash.ts'

const SERVER_JSON_NAME = 'server.json'

interface ServerJson {
  capabilities?: {
    executeEnabled?: boolean
  }
  [key: string]: unknown
}

function serverJsonPath(projectPath: string): string {
  return path.join(stateDir(projectPath), SERVER_JSON_NAME)
}

function readServerJson(filePath: string): ServerJson {
  if (!existsSync(filePath)) {
    return {}
  }

  try {
    const parsed = JSON.parse(readFileSync(filePath, 'utf-8')) as unknown
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
      return {}
    }
    return parsed as ServerJson
  } catch {
    return {}
  }
}

export function readExecuteEnabled(projectPath: string): boolean {
  const serverJson = readServerJson(serverJsonPath(projectPath))
  return serverJson.capabilities?.executeEnabled === true
}

export function writeExecuteEnabled(projectPath: string, enabled: boolean): void {
  const directory = stateDir(projectPath)
  const filePath = path.join(directory, SERVER_JSON_NAME)
  const serverJson = readServerJson(filePath)
  const capabilities =
    serverJson.capabilities && typeof serverJson.capabilities === 'object'
      ? serverJson.capabilities
      : {}

  mkdirSync(directory, { recursive: true })
  writeFileSync(
    filePath,
    `${JSON.stringify({ ...serverJson, capabilities: { ...capabilities, executeEnabled: enabled } }, null, 2)}\n`,
    'utf-8',
  )
}
