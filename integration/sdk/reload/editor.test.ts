import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

async function waitForPlayMode(client: ScenicClient, expected: 'edit' | 'playing' | 'paused'): Promise<void> {
  const timeoutMs = 20_000
  const intervalMs = 250
  const start = Date.now()

  while (Date.now() - start < timeoutMs) {
    const status = await client.status()
    if (status.playMode === expected) {
      return
    }
    await new Promise((resolve) => setTimeout(resolve, intervalMs))
  }

  const finalStatus = await client.status()
  throw new Error(`Timed out waiting for play mode '${expected}'. Last mode: '${finalStatus.playMode}'.`)
}

describe('SDK: editor', () => {
  let client: ScenicClient

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    await client.editorStop()
    await waitForPlayMode(client, 'edit')
    client.close()
  })

  it('controls play, pause, and stop', async () => {
    await client.editorStop()
    await waitForPlayMode(client, 'edit')

    const play = await client.editorPlay()
    assert.ok(play.playMode === 'playing' || play.playMode === 'paused')
    await waitForPlayMode(client, 'playing')

    const pause = await client.editorPause()
    assert.equal(pause.playMode, 'paused')
    await waitForPlayMode(client, 'paused')

    const stop = await client.editorStop()
    assert.ok(stop.playMode === 'edit' || stop.playMode === 'playing')
    await waitForPlayMode(client, 'edit')
  })
})
