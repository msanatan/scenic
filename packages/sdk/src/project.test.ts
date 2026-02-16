import { afterEach, beforeEach, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { findUnityProject, init, isPluginInstalled, parseUnityVersion } from './project.ts'

const baseDir = '/tmp/unibridge-sdk-project-tests'
const testDir = `${baseDir}/test-unity-project`
const testDirWithSpaces = `${baseDir}/test unity project`

function createFakeProject(path: string, version = '2022.3.10f1') {
  mkdirSync(`${path}/Assets`, { recursive: true })
  mkdirSync(`${path}/ProjectSettings`, { recursive: true })
  mkdirSync(`${path}/Packages`, { recursive: true })
  writeFileSync(`${path}/ProjectSettings/ProjectVersion.txt`, `m_EditorVersion: ${version}\n`)
  writeFileSync(`${path}/Packages/manifest.json`, JSON.stringify({ dependencies: {} }, null, 2))
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
  beforeEach(() => createFakeProject(testDir))
  afterEach(() => rmSync(baseDir, { recursive: true, force: true }))

  it('adds plugin to manifest.json with git source', async () => {
    const result = await init({ projectPath: testDir })
    assert.equal(result.unityVersion, '2022.3.10f1')
    assert.equal(result.pluginSource, 'git')

    const manifest = JSON.parse(readFileSync(`${testDir}/Packages/manifest.json`, 'utf-8'))
    assert.ok(manifest.dependencies['com.msanatan.unibridge'])
  })

  it('adds plugin with local source', async () => {
    const result = await init({
      projectPath: testDir,
      source: { type: 'local', path: '../unibridge/unity' },
    })
    assert.equal(result.pluginSource, 'local')

    const manifest = JSON.parse(readFileSync(`${testDir}/Packages/manifest.json`, 'utf-8'))
    assert.equal(manifest.dependencies['com.msanatan.unibridge'], 'file:../unibridge/unity')
  })

  it('no-ops when plugin is already at correct version', async () => {
    await init({ projectPath: testDir })
    const result = await init({ projectPath: testDir })
    assert.equal(result.pluginSource, 'git')
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
