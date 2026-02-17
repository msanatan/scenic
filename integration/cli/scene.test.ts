import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../helpers/cli-runner.ts'

describe('CLI: scene', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns the active scene', async () => {
    const payload = (await runCli('scene', 'active')) as {
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

    assert.equal(payload.success, true)
    assert.equal(typeof payload.result?.scene.name, 'string')
    assert.equal(typeof payload.result?.scene.path, 'string')
    assert.equal(typeof payload.result?.scene.isDirty, 'boolean')
  })
})
