import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: packages', () => {
  let client: ScenicClient

  before(() => {
    client = createTestClient()
  })

  after(() => {
    client.close()
  })

  it('returns paginated package results', async () => {
    const result = await client.packagesGet({ limit: 5, offset: 0, includeIndirect: true })

    assert.equal(result.limit, 5)
    assert.equal(result.offset, 0)
    assert.equal(typeof result.total, 'number')
    assert.ok(result.total > 0)
    assert.ok(result.packages.length <= 5)

    const first = result.packages[0]
    assert.equal(typeof first?.name, 'string')
    assert.equal(typeof first?.displayName, 'string')
    assert.equal(typeof first?.version, 'string')
    assert.equal(typeof first?.source, 'string')
    assert.equal(typeof first?.isDirectDependency, 'boolean')
  })

  it('supports includeIndirect toggle', async () => {
    const direct = await client.packagesGet({ limit: 500, offset: 0, includeIndirect: false })
    const withIndirect = await client.packagesGet({ limit: 500, offset: 0, includeIndirect: true })

    assert.ok(withIndirect.total >= direct.total)
  })

  it('supports search filter on name or display name', async () => {
    const baseline = await client.packagesGet({ limit: 200, offset: 0, includeIndirect: true })
    assert.ok(baseline.packages.length > 0)

    const seed = baseline.packages.find((pkg) => pkg.name.length >= 4 || pkg.displayName.length >= 4) ?? baseline.packages[0]
    const seedText = seed.name.length >= 4 ? seed.name : seed.displayName
    const query = seedText.slice(0, Math.min(4, seedText.length))
    assert.ok(query.length > 0)
    const filtered = await client.packagesGet({
      limit: 200,
      offset: 0,
      includeIndirect: true,
      search: query,
    })

    assert.ok(filtered.total > 0)
    for (const pkg of filtered.packages) {
      const inName = pkg.name.toLowerCase().includes(query.toLowerCase())
      const inDisplayName = pkg.displayName.toLowerCase().includes(query.toLowerCase())
      assert.equal(inName || inDisplayName, true)
    }
  })
})
