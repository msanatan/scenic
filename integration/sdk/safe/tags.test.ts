import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: tags', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })

  after(() => {
    client.close()
  })

  it('returns tags including built-in markers', async () => {
    const result = await client.tagsGet()

    assert.equal(typeof result.total, 'number')
    assert.ok(Array.isArray(result.tags))
    assert.equal(result.total, result.tags.length)
    assert.ok(result.total > 0)

    const untagged = result.tags.find((tag) => tag.name === 'Untagged')
    assert.ok(untagged != null)
    assert.equal(untagged?.isBuiltIn, true)
  })
})
