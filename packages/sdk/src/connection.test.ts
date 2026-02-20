import { afterEach, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import net from 'node:net'
import fs from 'node:fs'
import { PipeConnection } from './connection.ts'

const SOCK_PATH = '/tmp/unibridge-test.sock'

function createMockServer(handler: (data: Buffer) => Buffer) {
  const sockets = new Set<net.Socket>()
  const server = net.createServer((socket) => {
    sockets.add(socket)
    socket.on('close', () => {
      sockets.delete(socket)
    })
    let buffer = Buffer.alloc(0)
    socket.on('data', (chunk: Buffer | string) => {
      const data = typeof chunk === 'string' ? Buffer.from(chunk) : chunk
      buffer = Buffer.concat([buffer, data])
      while (buffer.length >= 4) {
        const len = buffer.readUInt32BE(0)
        if (buffer.length < 4 + len) break
        const msg = buffer.subarray(4, 4 + len)
        buffer = buffer.subarray(4 + len)
        const response = handler(msg)
        const frame = Buffer.alloc(4 + response.length)
        frame.writeUInt32BE(response.length, 0)
        response.copy(frame, 4)
        socket.write(frame)
      }
    })
  })

  try {
    fs.unlinkSync(SOCK_PATH)
  } catch {
    // ignore missing socket
  }

  server.listen(SOCK_PATH)
  const closeWithClients = server.close.bind(server)
  ;(server as net.Server & { close: net.Server['close'] }).close = ((callback?: (err?: Error) => void) => {
    for (const socket of sockets) {
      socket.destroy()
    }
    sockets.clear()
    return closeWithClients(callback)
  }) as net.Server['close']
  return server
}

describe('PipeConnection', () => {
  let server: net.Server | undefined
  let conn: PipeConnection | undefined

  afterEach(async () => {
    conn?.disconnect()
    if (server) {
      await new Promise<void>((resolve) => server?.close(() => resolve()))
    }
    try {
      fs.unlinkSync(SOCK_PATH)
    } catch {
      // ignore missing socket
    }
  })

  it('connects and sends/receives a message', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'hello',
      }))
    })

    conn = new PipeConnection()
    await conn.connect(SOCK_PATH)
    const res = await conn.send({
      id: 'cmd-1',
      command: 'execute',
      params: { code: 'test' },
    })

    assert.equal(res.success, true)
    assert.equal(res.result, 'hello')
  })

  it('matches responses to requests by ID', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: req.id,
      }))
    })

    conn = new PipeConnection()
    await conn.connect(SOCK_PATH)
    const [a, b] = await Promise.all([
      conn.send({ id: 'cmd-a', command: 'execute', params: {} }),
      conn.send({ id: 'cmd-b', command: 'execute', params: {} }),
    ])

    assert.equal(a.result, 'cmd-a')
    assert.equal(b.result, 'cmd-b')
  })

  it('reconnects for new commands after server restarts', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({ id: req.id, success: true, result: 'ok' }))
    })

    conn = new PipeConnection({ connectTimeout: 5000 })
    await conn.connect(SOCK_PATH)

    await new Promise<void>((resolve) => server?.close(() => resolve()))
    try {
      fs.unlinkSync(SOCK_PATH)
    } catch {
      // ignore missing socket
    }

    await new Promise((resolve) => setTimeout(resolve, 300))
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'reconnected',
      }))
    })

    const res = await conn.send({
      id: 'cmd-2',
      command: 'execute',
      params: {},
    })

    assert.equal(res.result, 'reconnected')
  })

  it('re-sends pending commands after reconnect', async () => {
    let commandCount = 0
    server = createMockServer((msg) => {
      commandCount++
      const req = JSON.parse(msg.toString())
      if (commandCount === 1) {
        setTimeout(() => {
          server?.close()
          try {
            fs.unlinkSync(SOCK_PATH)
          } catch {
            // ignore missing socket
          }
          server = createMockServer((nextMsg) => {
            commandCount++
            const nextReq = JSON.parse(nextMsg.toString())
            return Buffer.from(JSON.stringify({
              id: nextReq.id,
              success: true,
              result: 'recovered',
            }))
          })
        }, 50)
        return Buffer.alloc(0)
      }

      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'unexpected',
      }))
    })

    conn = new PipeConnection({ commandTimeout: 2000, connectTimeout: 2000 })
    await conn.connect(SOCK_PATH)

    const response = await conn.send({
      id: 'cmd-reload',
      command: 'execute',
      params: {},
    })

    assert.equal(response.success, true)
    assert.equal(response.result, 'recovered')
    assert.equal(commandCount, 2)
  })

  it('fires connect timeout when no server', async () => {
    conn = new PipeConnection({ connectTimeout: 500 })
    await assert.rejects(conn.connect('/tmp/nonexistent.sock'), /Connect timeout/i)
  })

  it('times out when the response is lost', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      if (req.command === 'execute') {
        return Buffer.alloc(0)
      }

      return Buffer.from(JSON.stringify({
        id: req.id,
        success: false,
        error: 'unexpected command',
      }))
    })

    conn = new PipeConnection({ commandTimeout: 1000 })
    await conn.connect(SOCK_PATH)

    await assert.rejects(
      conn.send({
        id: 'cmd-timeout',
        command: 'execute',
        params: { code: 'Debug.Log("x")' },
      }),
      /Command timeout/i,
    )
  })
})
