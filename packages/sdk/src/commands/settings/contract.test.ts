import assert from 'node:assert/strict'
import { describe, it } from 'node:test'
import { invokeCommand } from '../define.ts'
import { settingsGetCommand, settingsUpdateCommand } from './contract.ts'

describe('settings command contracts', () => {
  it('settingsGet sends settings.get and parses response', async () => {
    let sentCommand = ''
    let sentParams: Record<string, unknown> = {}

    const result = await invokeCommand(
      settingsGetCommand,
      {
        async send(command, params) {
          sentCommand = command
          sentParams = params
          return { executeEnabled: true }
        },
        ensureExecuteEnabled() {
          throw new Error('execute guard should not be used for settings.get')
        },
      },
    )

    assert.equal(sentCommand, 'settings.get')
    assert.deepEqual(sentParams, {})
    assert.equal(result.executeEnabled, true)
  })

  it('settingsUpdate validates payload and sends settings.update', async () => {
    let sentCommand = ''
    let sentParams: Record<string, unknown> = {}

    const result = await invokeCommand(
      settingsUpdateCommand,
      {
        async send(command, params) {
          sentCommand = command
          sentParams = params
          return { executeEnabled: false }
        },
        ensureExecuteEnabled() {
          throw new Error('execute guard should not be used for settings.update')
        },
      },
      { executeEnabled: false },
    )

    assert.equal(sentCommand, 'settings.update')
    assert.deepEqual(sentParams, { executeEnabled: false })
    assert.equal(result.executeEnabled, false)

    await assert.rejects(
      invokeCommand(
        settingsUpdateCommand,
        {
          async send() {
            return { executeEnabled: true }
          },
          ensureExecuteEnabled() {
            return
          },
        },
        { executeEnabled: 'nope' } as unknown as { executeEnabled: boolean },
      ),
    )
  })
})
