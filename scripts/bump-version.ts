import { readFileSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { spawnSync } from 'node:child_process'

const ROOT = path.resolve(import.meta.dirname, '..')

type BumpType = 'major' | 'minor' | 'patch'

const PACKAGE_JSON_PATHS = [
  'package.json',
  'packages/sdk/package.json',
  'packages/cli/package.json',
  'unity/package.json',
]

function parseVersion(version: string): [number, number, number] {
  const parts = version.split('.').map(Number)
  if (parts.length !== 3 || parts.some(isNaN)) {
    throw new Error(`Invalid semver: ${version}`)
  }
  return parts as [number, number, number]
}

function bump(version: string, type: BumpType): string {
  const [major, minor, patch] = parseVersion(version)
  switch (type) {
    case 'major':
      return `${major + 1}.0.0`
    case 'minor':
      return `${major}.${minor + 1}.0`
    case 'patch':
      return `${major}.${minor}.${patch + 1}`
  }
}

function readJson(relativePath: string): Record<string, unknown> {
  const fullPath = path.join(ROOT, relativePath)
  return JSON.parse(readFileSync(fullPath, 'utf-8')) as Record<string, unknown>
}

function writeJson(relativePath: string, data: Record<string, unknown>): void {
  const fullPath = path.join(ROOT, relativePath)
  writeFileSync(fullPath, JSON.stringify(data, null, 2) + '\n')
}

function main(): void {
  const type = process.argv[2] as BumpType | undefined
  if (!type || !['major', 'minor', 'patch'].includes(type)) {
    console.error('Usage: bump-version <major|minor|patch>')
    process.exit(1)
  }

  const root = readJson('package.json')
  const currentVersion = root.version as string
  const newVersion = bump(currentVersion, type)

  console.log(`${currentVersion} -> ${newVersion}`)

  for (const relativePath of PACKAGE_JSON_PATHS) {
    const parsed = readJson(relativePath)
    parsed.version = newVersion

    const deps = parsed.dependencies as Record<string, string> | undefined
    if (deps?.['@scenicai/sdk']) {
      deps['@scenicai/sdk'] = newVersion
    }

    writeJson(relativePath, parsed)
    console.log(`  updated ${relativePath}`)
  }

  const npmCmd = process.platform === 'win32' ? 'npm.cmd' : 'npm'
  const lockUpdate = spawnSync(npmCmd, ['install', '--package-lock-only'], {
    cwd: ROOT,
    stdio: 'inherit',
  })

  if (lockUpdate.status !== 0) {
    throw new Error('Failed to update package-lock.json')
  }

  console.log('  updated package-lock.json')
}

main()
