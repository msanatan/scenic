import { existsSync, readFileSync, renameSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { cwd } from 'node:process'
import type { InitOptions, InitResult } from './types.ts'

const PLUGIN_NAME = 'com.msanatan.unibridge'
const EXPECTED_PLUGIN_VERSION = '0.1.0'
const DEFAULT_GIT_SOURCE =
  'https://github.com/msanatan/unibridge.git?path=unity#v0.1.0'

interface Manifest {
  dependencies?: Record<string, string>
}

function readManifest(projectPath: string): Manifest {
  const manifestPath = path.join(projectPath, 'Packages', 'manifest.json')
  return JSON.parse(readFileSync(manifestPath, 'utf-8')) as Manifest
}

function writeManifestAtomic(projectPath: string, manifest: Manifest): void {
  const manifestPath = path.join(projectPath, 'Packages', 'manifest.json')
  const tempPath = `${manifestPath}.tmp`
  const content = `${JSON.stringify(manifest, null, 2)}\n`
  writeFileSync(tempPath, content, 'utf-8')
  renameSync(tempPath, manifestPath)
}

export function findUnityProject(startPath: string = cwd()): string {
  let current = path.resolve(startPath)

  while (true) {
    const assetsPath = path.join(current, 'Assets')
    const versionPath = path.join(current, 'ProjectSettings', 'ProjectVersion.txt')

    if (existsSync(assetsPath) && existsSync(versionPath)) {
      return current
    }

    const parent = path.dirname(current)
    if (parent === current) {
      throw new Error('Unity project not found. Run inside a Unity project or pass --project <path>.')
    }

    current = parent
  }
}

export function parseUnityVersion(projectPath: string): string {
  const versionPath = path.join(projectPath, 'ProjectSettings', 'ProjectVersion.txt')
  const content = readFileSync(versionPath, 'utf-8')
  const match = content.match(/m_EditorVersion:\s*(\S+)/)
  if (!match) {
    throw new Error(`Could not parse m_EditorVersion from ${versionPath}`)
  }

  return match[1]
}

export function isPluginInstalled(projectPath: string): boolean {
  const manifest = readManifest(projectPath)
  return Boolean(manifest.dependencies?.[PLUGIN_NAME])
}

export async function init(options: InitOptions = {}): Promise<InitResult> {
  const projectPath = findUnityProject(options.projectPath)
  const unityVersion = parseUnityVersion(projectPath)

  const manifest = readManifest(projectPath)
  const dependencies = { ...(manifest.dependencies ?? {}) }

  const pluginSource = options.source?.type ?? 'git'
  const pluginReference =
    options.source?.type === 'local'
      ? `file:${options.source.path}`
      : options.source?.type === 'git'
        ? options.source.url
        : DEFAULT_GIT_SOURCE

  if (dependencies[PLUGIN_NAME] !== pluginReference) {
    dependencies[PLUGIN_NAME] = pluginReference
    writeManifestAtomic(projectPath, {
      ...manifest,
      dependencies,
    })
  }

  return {
    projectPath,
    unityVersion,
    pluginVersion: EXPECTED_PLUGIN_VERSION,
    pluginSource,
  }
}
