import assert from 'node:assert/strict'
import { mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { join } from 'node:path'
import { spawnSync } from 'node:child_process'
import { afterEach, beforeEach, describe, it } from 'node:test'

const fixtureRoot = '/tmp/unibridge-cli-init-integration'
const projectPath = join(fixtureRoot, 'My Game')
const manifestPath = join(projectPath, 'Packages', 'manifest.json')
const pluginName = 'com.msanatan.unibridge'
const cliDir = import.meta.dirname

function runCli(args: string[]) {
  return spawnSync(
    process.execPath,
    ['--experimental-strip-types', 'index.ts', '--project', projectPath, ...args],
    {
      cwd: cliDir,
      encoding: 'utf-8',
    },
  )
}

beforeEach(() => {
  mkdirSync(join(projectPath, 'Assets'), { recursive: true })
  mkdirSync(join(projectPath, 'ProjectSettings'), { recursive: true })
  mkdirSync(join(projectPath, 'Packages'), { recursive: true })
  writeFileSync(join(projectPath, 'ProjectSettings', 'ProjectVersion.txt'), 'm_EditorVersion: 2022.3.10f1\n')
  writeFileSync(manifestPath, JSON.stringify({ dependencies: {} }, null, 2))
})

afterEach(() => {
  rmSync(fixtureRoot, { recursive: true, force: true })
})

describe('cli init/update integration', () => {
  it('updates manifest via update alias and remains idempotent', () => {
    const pluginUrl = 'https://example.com/repo.git?path=unity#v9.9.9'

    const first = runCli(['update', '--git', pluginUrl])
    assert.equal(first.status, 0, `stderr: ${first.stderr}`)

    const manifestAfterFirst = JSON.parse(readFileSync(manifestPath, 'utf-8')) as {
      dependencies?: Record<string, string>
    }
    assert.equal(manifestAfterFirst.dependencies?.[pluginName], pluginUrl)

    const second = runCli(['update', '--git', pluginUrl])
    assert.equal(second.status, 0, `stderr: ${second.stderr}`)

    const manifestAfterSecond = JSON.parse(readFileSync(manifestPath, 'utf-8')) as {
      dependencies?: Record<string, string>
    }
    assert.equal(manifestAfterSecond.dependencies?.[pluginName], pluginUrl)
  })
})
