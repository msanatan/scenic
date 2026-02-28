import { afterEach, beforeEach, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { stateDir } from './hash.ts'
import { writeExecuteEnabled } from './config.ts'
import { findUnityProject, init, isPluginInstalled, parseUnityVersion } from './project.ts'

const baseDir = '/tmp/scenic-sdk-project-tests'
const testDir = `${baseDir}/test-unity-project`
const testDirWithSpaces = `${baseDir}/test unity project`

function createFakeProject(path: string, version = '2022.3.10f1') {
  mkdirSync(`${path}/Assets`, { recursive: true })
  mkdirSync(`${path}/ProjectSettings`, { recursive: true })
  mkdirSync(`${path}/Packages`, { recursive: true })
  writeFileSync(`${path}/ProjectSettings/ProjectVersion.txt`, `m_EditorVersion: ${version}\n`)
  writeFileSync(`${path}/Packages/manifest.json`, JSON.stringify({ dependencies: {} }, null, 2))
}

function readConfigJson(projectPath: string): { executeEnabled?: boolean } {
  const configPath = path.join(stateDir(projectPath), 'config.json')
  if (!existsSync(configPath)) {
    return {}
  }
  return JSON.parse(readFileSync(configPath, 'utf-8')) as {
    executeEnabled?: boolean
  }
}

describe('findUnityProject', () => {
  beforeEach(() => createFakeProject(testDir))
  afterEach(() => rmSync(baseDir, { recursive: true, force: true }))

  it('finds project at the given path', () => {
    assert.equal(findUnityProject(testDir), testDir)
  })

  it('finds project from a subdirectory', () => {
    mkdirSync(`${testDir}/Assets/Scripts`, { recursive: true })
    assert.equal(findUnityProject(`${testDir}/Assets/Scripts`), testDir)
  })

  it('supports project paths with spaces', () => {
    createFakeProject(testDirWithSpaces)
    assert.equal(findUnityProject(testDirWithSpaces), testDirWithSpaces)
  })

  it('throws when no Unity project is found', () => {
    assert.throws(() => findUnityProject('/tmp'), /Unity project not found/)
  })
})

describe('parseUnityVersion', () => {
  beforeEach(() => createFakeProject(testDir, '6000.0.23f1'))
  afterEach(() => rmSync(baseDir, { recursive: true, force: true }))

  it('extracts version string', () => {
    assert.equal(parseUnityVersion(testDir), '6000.0.23f1')
  })
})

describe('init', () => {
  beforeEach(() => {
    createFakeProject(testDir)
    rmSync(stateDir(testDir), { recursive: true, force: true })
  })
  afterEach(() => {
    rmSync(stateDir(testDir), { recursive: true, force: true })
    rmSync(baseDir, { recursive: true, force: true })
  })

  it('adds plugin to manifest.json with git source', async () => {
    const result = await init({ projectPath: testDir })
    assert.equal(result.unityVersion, '2022.3.10f1')
    assert.equal(result.pluginSource, 'git')
    assert.equal(result.executeEnabled, false)

    const manifest = JSON.parse(readFileSync(`${testDir}/Packages/manifest.json`, 'utf-8'))
    assert.ok(manifest.dependencies['com.msanatan.scenic'])
  })

  it('adds plugin with local source', async () => {
    const result = await init({
      projectPath: testDir,
      source: { type: 'local', path: '../scenic/unity' },
    })
    assert.equal(result.pluginSource, 'local')
    assert.equal(result.executeEnabled, false)

    const manifest = JSON.parse(readFileSync(`${testDir}/Packages/manifest.json`, 'utf-8'))
    assert.equal(manifest.dependencies['com.msanatan.scenic'], 'file:../scenic/unity')
  })

  it('no-ops when plugin is already at correct version', async () => {
    await init({ projectPath: testDir })
    const result = await init({ projectPath: testDir })
    assert.equal(result.pluginSource, 'git')
    assert.equal(result.executeEnabled, false)
  })

  it('returns executeEnabled false by default', async () => {
    await init({ projectPath: testDir })
    assert.equal(readConfigJson(testDir).executeEnabled, undefined)
  })

  it('returns persisted executeEnabled when present', async () => {
    writeExecuteEnabled(testDir, true)
    const result = await init({ projectPath: testDir })
    assert.equal(result.executeEnabled, true)
  })

  it('does not overwrite persisted executeEnabled value', async () => {
    writeExecuteEnabled(testDir, true)
    const second = await init({ projectPath: testDir })
    assert.equal(second.executeEnabled, true)

    const configJson = readConfigJson(testDir)
    assert.equal(configJson.executeEnabled, true)
  })
})

describe('isPluginInstalled', () => {
  beforeEach(() => createFakeProject(testDir))
  afterEach(() => rmSync(baseDir, { recursive: true, force: true }))

  it('returns false when dependency is absent', () => {
    assert.equal(isPluginInstalled(testDir), false)
  })

  it('returns true after init installs the plugin', async () => {
    await init({ projectPath: testDir })
    assert.equal(isPluginInstalled(testDir), true)
  })
})
