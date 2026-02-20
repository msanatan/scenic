import { createHash } from 'node:crypto'
import { existsSync, mkdirSync, readFileSync, realpathSync, writeFileSync } from 'node:fs'
import os from 'node:os'
import path from 'node:path'

interface ServerJson {
  capabilities?: {
    executeEnabled?: boolean
  }
  [key: string]: unknown
}

function canonicalizeProjectPath(projectPath: string): string {
  const resolved = path.resolve(projectPath)
  let canonical = resolved

  try {
    canonical = realpathSync.native(resolved)
  } catch {
    // Fall back to resolved path when target does not exist.
  }

  canonical = canonical.replace(/\\/g, '/').replace(/\/+$/g, '')
  if (process.platform === 'win32') {
    canonical = canonical.toLowerCase()
  }

  return canonical
}

function stateDir(projectPath: string): string {
  const hash = createHash('sha256').update(canonicalizeProjectPath(projectPath)).digest('hex').slice(0, 12)
  const baseDir = process.platform === 'win32' ? path.join(os.tmpdir(), 'unibridge') : '/tmp/unibridge'
  return path.join(baseDir, hash)
}

function serverJsonPath(projectPath: string): string {
  return path.join(stateDir(projectPath), 'server.json')
}

function readServerJson(projectPath: string): ServerJson {
  const filePath = serverJsonPath(projectPath)
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
  return readServerJson(projectPath).capabilities?.executeEnabled === true
}

export function writeExecuteEnabled(projectPath: string, enabled: boolean): void {
  const directory = stateDir(projectPath)
  const filePath = serverJsonPath(projectPath)
  const serverJson = readServerJson(projectPath)
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
