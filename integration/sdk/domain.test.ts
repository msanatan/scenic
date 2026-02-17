import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../packages/sdk/src/index.ts'
import { createTestClient } from '../helpers/sdk-client.ts'

describe('SDK: domain', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('triggers a domain reload', async () => {
    const result = await client.domainReload()
    assert.equal(result.triggered, true)
  })
})
