import path from 'node:path'
import { createClient } from '../../packages/sdk/src/client.ts'

export const projectPath = path.resolve(import.meta.dirname, '../../TestProjects/UniBridge3Dv6.3')

export function createTestClient() {
  return createClient({
    projectPath,
    enableExecute: true,
    connectTimeout: 10_000,
    commandTimeout: 20_000,
  })
}
