import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: logs', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('returns paginated logs with severity filtering', async () => {
    const marker = `sdk-logs-${Date.now()}`
    await client.execute(`UnityEngine.Debug.LogWarning("${marker}")`)

    const page = await client.logs({ severity: 'warn', limit: 200, offset: 0 })
    assert.equal(typeof page.total, 'number')
    assert.equal(page.limit, 200)
    assert.equal(page.offset, 0)
    assert.ok(page.total > 0)
    assert.ok(page.logs.every((entry) => entry.severity === 'warn'))
    assert.ok(page.logs.some((entry) => entry.message.includes(marker)))
  })
})
