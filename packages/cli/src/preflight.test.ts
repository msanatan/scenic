import { afterEach, beforeEach, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { mkdirSync, rmSync, writeFileSync } from 'node:fs'
import { resolveCommandProject } from './preflight.ts'

const projectPath = '/tmp/scenic-cli-preflight/My Game'

beforeEach(() => {
  mkdirSync(`${projectPath}/Assets`, { recursive: true })
  mkdirSync(`${projectPath}/ProjectSettings`, { recursive: true })
  mkdirSync(`${projectPath}/Packages`, { recursive: true })
  writeFileSync(`${projectPath}/ProjectSettings/ProjectVersion.txt`, 'm_EditorVersion: 2022.3.10f1\n')
  writeFileSync(`${projectPath}/Packages/manifest.json`, JSON.stringify({ dependencies: {} }, null, 2))
})

afterEach(() => {
  rmSync('/tmp/scenic-cli-preflight', { recursive: true, force: true })
})

describe('resolveCommandProject', () => {
  it('throws when plugin is missing for command execution', () => {
    assert.throws(
      () => resolveCommandProject({ project: projectPath, execute: true }, { requirePlugin: true, requiresExecute: true }),
      /Run `scenic init` \(or `scenic update`\) first/,
    )
  })

  it('throws when execute is disabled', () => {
    writeFileSync(
      `${projectPath}/Packages/manifest.json`,
      JSON.stringify({ dependencies: { 'com.msanatan.scenic': 'file:../unity' } }, null, 2),
    )

    assert.throws(
      () => resolveCommandProject({ project: projectPath, execute: false }, { requirePlugin: true, requiresExecute: true }),
      /Execute is disabled/,
    )
  })
})
