import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { stateDir } from './hash.ts'

const CONFIG_JSON_NAME = 'config.json'

interface ConfigJson {
  executeEnabled?: boolean
}

function configJsonPath(projectPath: string): string {
  return path.join(stateDir(projectPath), CONFIG_JSON_NAME)
}

function readConfigJson(filePath: string): ConfigJson {
  if (!existsSync(filePath)) {
    return {}
  }

  try {
    const parsed = JSON.parse(readFileSync(filePath, 'utf-8')) as unknown
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
      return {}
    }
    return parsed as ConfigJson
  } catch {
    return {}
  }
}

export function readExecuteEnabled(projectPath: string): boolean {
  return readConfigJson(configJsonPath(projectPath)).executeEnabled === true
}

export function writeExecuteEnabled(projectPath: string, enabled: boolean): void {
  const directory = stateDir(projectPath)
  mkdirSync(directory, { recursive: true })
  writeFileSync(
    path.join(directory, CONFIG_JSON_NAME),
    `${JSON.stringify({ executeEnabled: enabled }, null, 2)}\n`,
    'utf-8',
  )
}
