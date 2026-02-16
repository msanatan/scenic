import { describe, it, mock } from 'node:test'
import assert from 'node:assert/strict'
import { createClientForTests } from './client.ts'

describe('client.execute', () => {
  it('returns the result value on success', async () => {
    const mockSend = mock.fn(async (req: { id: string }) => ({
      id: req.id,
      success: true,
      result: 'Player',
    }))
    const client = createClientForTests({ send: mockSend })
    const result = await client.execute('Selection.activeGameObject?.name')
    assert.equal(result, 'Player')
  })

  it('sends correctly shaped command', async () => {
    const mockSend = mock.fn(async (req: { id: string }) => ({
      id: req.id,
      success: true,
      result: null,
    }))
    const client = createClientForTests({ send: mockSend })
    await client.execute('Debug.Log("hi")')

    const sentRequest = mockSend.mock.calls[0]?.arguments[0] as {
      command: string
      params: { code: string }
      id: string
    }

    assert.equal(sentRequest.command, 'execute')
    assert.equal(sentRequest.params.code, 'Debug.Log("hi")')
    assert.ok(sentRequest.id)
  })

  it('throws UniBridgeError on failure', async () => {
    const mockSend = mock.fn(async (req: { id: string }) => ({
      id: req.id,
      success: false,
      error: 'Compilation failed',
    }))
    const client = createClientForTests({ send: mockSend })

    await assert.rejects(client.execute('badCode'), /Compilation failed/)
  })
})
