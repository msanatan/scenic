import { createHash } from 'node:crypto'
import { realpathSync } from 'node:fs'
import os from 'node:os'
import path from 'node:path'

export function canonicalizeProjectPath(projectPath: string): string {
  const resolved = path.resolve(projectPath)
  let canonical = resolved

  try {
    canonical = realpathSync.native(resolved)
  } catch {
    // Fall back to the resolved path when it does not exist yet.
  }

  canonical = canonical.replace(/\\/g, '/')
  canonical = canonical.replace(/\/+$/g, '')

  if (process.platform === 'win32') {
    canonical = canonical.toLowerCase()
  }

  return canonical
}

export function projectHash(projectPath: string): string {
  const canonical = canonicalizeProjectPath(projectPath)
  return createHash('sha256').update(canonical).digest('hex').slice(0, 12)
}

function stateBaseDir(): string {
  if (process.platform === 'win32') {
    return path.join(os.tmpdir(), 'unibridge')
  }

  return '/tmp/unibridge'
}

export function stateDir(projectPath: string): string {
  return path.join(stateBaseDir(), projectHash(projectPath))
}

export function pipePath(projectPath: string): string {
  const hash = projectHash(projectPath)
  if (process.platform === 'win32') {
    return `\\\\.\\pipe\\unibridge-${hash}`
  }

  return path.join(stateBaseDir(), hash, 'bridge.sock')
}
