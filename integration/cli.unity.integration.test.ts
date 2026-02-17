import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import path from 'node:path'
import { execFile } from 'node:child_process'
import { promisify } from 'node:util'
import { test } from 'node:test'

const execFileAsync = promisify(execFile)

const projectPath = path.resolve(import.meta.dirname, '../TestProjects/UniBridge3Dv6.3')
const cliEntrypoint = path.resolve(import.meta.dirname, '../packages/cli/dist/index.js')

function parseLastJsonLine(text: string): unknown {
  const lines = text
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
  if (lines.length === 0) {
    throw new Error('CLI produced no output.')
  }

  return JSON.parse(lines[lines.length - 1])
}

test('CLI integration: can query Unity status and active scene via CLI', async () => {
  assert.ok(
    existsSync(cliEntrypoint),
    'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
  )

  const statusResult = await execFileAsync(
    process.execPath,
    [cliEntrypoint, 'status', '--json', '--project', projectPath],
    { encoding: 'utf-8' },
  )

  const statusPayload = parseLastJsonLine(statusResult.stdout) as {
    success: boolean
    result?: {
      projectPath: string
      unityVersion: string
      pluginVersion: string
      activeScene: string
      playMode: string
    }
    error?: string
  }

  assert.equal(statusPayload.success, true)
  assert.equal(statusPayload.result?.projectPath, projectPath)
  assert.equal(typeof statusPayload.result?.unityVersion, 'string')
  assert.equal(typeof statusPayload.result?.pluginVersion, 'string')
  assert.equal(typeof statusPayload.result?.activeScene, 'string')
  assert.equal(typeof statusPayload.result?.playMode, 'string')

  const sceneResult = await execFileAsync(
    process.execPath,
    [cliEntrypoint, 'scene', 'active', '--json', '--project', projectPath],
    { encoding: 'utf-8' },
  )

  const scenePayload = parseLastJsonLine(sceneResult.stdout) as {
    success: boolean
    result?: {
      scene: {
        name: string
        path: string
        isDirty: boolean
      }
    }
    error?: string
  }

  assert.equal(scenePayload.success, true)
  assert.equal(typeof scenePayload.result?.scene.name, 'string')
  assert.equal(typeof scenePayload.result?.scene.path, 'string')
  assert.equal(typeof scenePayload.result?.scene.isDirty, 'boolean')
})
