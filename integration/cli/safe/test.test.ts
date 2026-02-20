import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../../helpers/cli-runner.ts'

describe('CLI: test', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('lists tests with pagination', async () => {
    const payload = (await runCli('test', 'list', '--mode', 'edit', '--limit', '10', '--offset', '0')) as {
      success: boolean
      result?: {
        tests: Array<{
          name: string
          fullName: string
          mode: 'edit' | 'play'
          assembly: string
        }>
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 10)
    assert.equal(payload.result?.offset, 0)
    assert.equal(typeof payload.result?.total, 'number')
    assert.ok(Array.isArray(payload.result?.tests))
  })

  it('runs filtered tests and returns summary', async () => {
    const payload = (await runCli(
      'test',
      'run',
      '--mode',
      'edit',
      '--filter',
      'DomainReloadCommandHandlerTests.Route_DomainReload_ReturnsTriggeredTrue',
      '--limit',
      '10',
      '--offset',
      '0',
    )) as {
      success: boolean
      result?: {
        tests: Array<{
          name: string
          fullName: string
          mode: 'edit' | 'play'
          status: 'passed' | 'failed' | 'skipped' | 'inconclusive'
          durationMs: number
        }>
        passed: number
        failed: number
        skipped: number
        inconclusive: number
        durationMs: number
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 10)
    assert.equal(payload.result?.offset, 0)
    assert.equal(typeof payload.result?.durationMs, 'number')
    assert.ok((payload.result?.total ?? 0) >= 1)
    assert.ok((payload.result?.tests.length ?? 0) >= 1)
  })
})
