import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: test', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('lists tests with pagination', async () => {
    const page = await client.testList({ mode: 'edit', limit: 10, offset: 0 })
    assert.equal(page.limit, 10)
    assert.equal(page.offset, 0)
    assert.equal(typeof page.total, 'number')
    assert.ok(Array.isArray(page.tests))
    if (page.tests.length > 0) {
      assert.equal(typeof page.tests[0].fullName, 'string')
      assert.equal(page.tests[0].mode, 'edit')
    }
  })

  it('runs filtered tests and returns summary', async () => {
    const result = await client.testRun({
      mode: 'edit',
      filter: 'DomainReloadCommandHandlerTests.Route_DomainReload_ReturnsTriggeredTrue',
      limit: 10,
      offset: 0,
    })

    assert.equal(result.limit, 10)
    assert.equal(result.offset, 0)
    assert.equal(typeof result.total, 'number')
    assert.equal(typeof result.passed, 'number')
    assert.equal(typeof result.failed, 'number')
    assert.equal(typeof result.durationMs, 'number')
    assert.ok(result.total >= 1)
    assert.ok(result.tests.length >= 1)
  })
})
