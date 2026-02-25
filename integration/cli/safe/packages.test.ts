import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: packages', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns paginated package results', async () => {
    const payload = (await runCli('packages', 'get', '--limit', '5', '--offset', '0', '--include-indirect')) as {
      success: boolean
      result?: {
        packages: Array<{
          name: string
          displayName: string
          version: string
          source: string
          isDirectDependency: boolean
        }>
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 5)
    assert.equal(payload.result?.offset, 0)
    assert.equal(typeof payload.result?.total, 'number')
    assert.ok((payload.result?.total ?? 0) > 0)
    assert.ok((payload.result?.packages.length ?? 0) <= 5)

    const first = payload.result?.packages[0]
    assert.equal(typeof first?.name, 'string')
    assert.equal(typeof first?.displayName, 'string')
    assert.equal(typeof first?.version, 'string')
    assert.equal(typeof first?.source, 'string')
    assert.equal(typeof first?.isDirectDependency, 'boolean')
  })

  it('supports includeIndirect toggle', async () => {
    const directPayload = (await runCli('packages', 'get', '--limit', '500', '--offset', '0')) as {
      success: boolean
      result?: { total: number }
      error?: string
    }
    const indirectPayload = (await runCli('packages', 'get', '--limit', '500', '--offset', '0', '--include-indirect')) as {
      success: boolean
      result?: { total: number }
      error?: string
    }

    assert.equal(directPayload.success, true)
    assert.equal(indirectPayload.success, true)
    assert.ok((indirectPayload.result?.total ?? 0) >= (directPayload.result?.total ?? 0))
  })

  it('supports search filter on name or display name', async () => {
    const baselinePayload = (await runCli('packages', 'get', '--limit', '200', '--offset', '0', '--include-indirect')) as {
      success: boolean
      result?: {
        packages: Array<{
          name: string
          displayName: string
        }>
      }
      error?: string
    }

    assert.equal(baselinePayload.success, true)
    assert.ok((baselinePayload.result?.packages.length ?? 0) > 0)

    const seed = baselinePayload.result?.packages.find((pkg) => pkg.name.length >= 4 || pkg.displayName.length >= 4)
      ?? baselinePayload.result?.packages[0]
    const seedText = (seed?.name.length ?? 0) >= 4 ? seed?.name : seed?.displayName
    const query = seedText?.slice(0, Math.min(4, seedText.length)) ?? ''
    assert.ok(query.length > 0)

    const filteredPayload = (await runCli('packages', 'get', '--limit', '200', '--offset', '0', '--include-indirect', '--search', query)) as {
      success: boolean
      result?: {
        total: number
        packages: Array<{
          name: string
          displayName: string
        }>
      }
      error?: string
    }

    assert.equal(filteredPayload.success, true)
    assert.ok((filteredPayload.result?.total ?? 0) > 0)

    const normalized = query.toLowerCase()
    for (const pkg of filteredPayload.result?.packages ?? []) {
      const inName = pkg.name.toLowerCase().includes(normalized)
      const inDisplayName = pkg.displayName.toLowerCase().includes(normalized)
      assert.equal(inName || inDisplayName, true)
    }
  })
})
