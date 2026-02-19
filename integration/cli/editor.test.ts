import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../helpers/cli-runner.ts'

async function waitForPlayMode(expected: 'edit' | 'playing' | 'paused'): Promise<void> {
  const timeoutMs = 20_000
  const intervalMs = 250
  const start = Date.now()

  while (Date.now() - start < timeoutMs) {
    const statusPayload = (await runCli('status')) as {
      success: boolean
      result?: {
        playMode: string
      }
    }
    if (statusPayload.success && statusPayload.result?.playMode === expected) {
      return
    }
    await new Promise((resolve) => setTimeout(resolve, intervalMs))
  }

  const finalPayload = (await runCli('status')) as {
    success: boolean
    result?: {
      playMode: string
    }
  }
  throw new Error(`Timed out waiting for play mode '${expected}'. Last mode: '${finalPayload.result?.playMode ?? 'unknown'}'.`)
}

describe('CLI: editor', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    await runCli('editor', 'stop')
    await waitForPlayMode('edit')
  })

  it('controls play, pause, and stop', async () => {
    await runCli('editor', 'stop')
    await waitForPlayMode('edit')

    const playPayload = (await runCli('editor', 'play')) as {
      success: boolean
      result?: {
        playMode: 'edit' | 'playing' | 'paused'
      }
    }
    assert.equal(playPayload.success, true)
    assert.ok(playPayload.result?.playMode === 'playing' || playPayload.result?.playMode === 'paused')
    await waitForPlayMode('playing')

    const pausePayload = (await runCli('editor', 'pause')) as {
      success: boolean
      result?: {
        playMode: 'edit' | 'playing' | 'paused'
      }
    }
    assert.equal(pausePayload.success, true)
    assert.equal(pausePayload.result?.playMode, 'paused')
    await waitForPlayMode('paused')

    const stopPayload = (await runCli('editor', 'stop')) as {
      success: boolean
      result?: {
        playMode: 'edit' | 'playing' | 'paused'
      }
    }
    assert.equal(stopPayload.success, true)
    assert.ok(stopPayload.result?.playMode === 'edit' || stopPayload.result?.playMode === 'playing')
    await waitForPlayMode('edit')
  })
})
