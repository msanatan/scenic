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

describe('client.status', () => {
  it('returns parsed status result', async () => {
    const mockSend = mock.fn(async (req: { id: string }) => ({
      id: req.id,
      success: true,
      result: {
        projectPath: '/tmp/project',
        unityVersion: '6000.3.5f2',
        pluginVersion: '0.2.0',
        activeScene: 'Assets/Main.unity',
        playMode: 'edit',
      },
    }))

    const client = createClientForTests({ send: mockSend })
    const result = await client.status()
    assert.equal(result.projectPath, '/tmp/project')
    assert.equal(result.playMode, 'edit')
  })

  it('rejects malformed status payload', async () => {
    const mockSend = mock.fn(async (req: { id: string }) => ({
      id: req.id,
      success: true,
      result: { bad: true },
    }))

    const client = createClientForTests({ send: mockSend })
    await assert.rejects(client.status(), /ValiError/)
  })
})
