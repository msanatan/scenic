import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint, projectPath } from '../helpers/cli-runner.ts'

describe('CLI: status', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns project path, versions, scene, and play mode', async () => {
    const payload = (await runCli('status')) as {
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

    assert.equal(payload.success, true)
    assert.equal(payload.result?.projectPath, projectPath)
    assert.equal(typeof payload.result?.unityVersion, 'string')
    assert.equal(typeof payload.result?.pluginVersion, 'string')
    assert.equal(typeof payload.result?.activeScene, 'string')
    assert.equal(typeof payload.result?.playMode, 'string')
  })
})
