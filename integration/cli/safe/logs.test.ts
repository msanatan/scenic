import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: logs', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns paginated logs with severity filtering', async () => {
    const marker = `cli-logs-${Date.now()}`
    await runCli('execute', `UnityEngine.Debug.LogWarning("${marker}")`)

    const payload = (await runCli('logs', '--severity', 'warn', '--limit', '200', '--offset', '0')) as {
      success: boolean
      result?: {
        logs: Array<{
          timestamp: string
          severity: 'info' | 'warn' | 'error'
          message: string
          stackTrace?: string
        }>
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 200)
    assert.equal(payload.result?.offset, 0)
    assert.equal(typeof payload.result?.total, 'number')
    assert.ok(payload.result?.total > 0)
    assert.ok(payload.result?.logs.every((entry) => entry.severity === 'warn'))
    assert.ok(payload.result?.logs.some((entry) => entry.message.includes(marker)))
  })
})
