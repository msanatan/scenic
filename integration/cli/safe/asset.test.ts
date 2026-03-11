import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'
import { TEMP_DIR, makeAssetFixtures } from '../../helpers/asset-fixtures.ts'

describe('CLI: asset', () => {
  const createdAssets: string[] = []

  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    for (const p of createdAssets) {
      try {
        await runCli('asset', 'delete', p)
      } catch {
        // ignore cleanup failures
      }
    }
  })

  const { findCopyableAsset, createTempAsset } = makeAssetFixtures(
    {
      copy: async (src, dest) => {
        const payload = (await runCli('asset', 'copy', src, dest)) as { success: boolean }
        assert.equal(payload.success, true)
      },
    },
    createdAssets,
  )

  it('asset find returns paginated results', async () => {
    const payload = (await runCli('asset', 'find', '--limit', '5', '--offset', '0')) as {
      success: boolean
      result?: {
        assets: Array<{
          assetPath: string
          guid: string
          type: string
          name: string
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
    assert.ok(Array.isArray(payload.result?.assets))
  })

  it('asset find filters by type', async () => {
    const payload = (await runCli('asset', 'find', '--type', 'Material', '--limit', '5')) as {
      success: boolean
      result?: {
        assets: Array<{ type: string }>
      }
    }

    assert.equal(payload.success, true)
    for (const asset of payload.result?.assets ?? []) {
      assert.equal(asset.type, 'Material')
    }
  })

  it('asset get returns metadata', async () => {
    const findPayload = (await runCli('asset', 'find', '--limit', '1')) as {
      success: boolean
      result?: { assets: Array<{ assetPath: string }> }
    }
    assert.ok((findPayload.result?.assets.length ?? 0) > 0)

    const assetPath = findPayload.result!.assets[0].assetPath
    const payload = (await runCli('asset', 'get', assetPath)) as {
      success: boolean
      result?: {
        assetPath: string
        guid: string
        type: string
        name: string
        labels: string[]
        dependencies: string[]
      }
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.assetPath, assetPath)
    assert.ok(payload.result!.guid.length > 0)
    assert.ok(payload.result!.type.length > 0)
    assert.ok(payload.result!.name.length > 0)
    assert.ok(Array.isArray(payload.result?.labels))
    assert.ok(Array.isArray(payload.result?.dependencies))
  })

  it('asset copy, move, and delete lifecycle', async () => {
    const { assetPath: source, ext } = findCopyableAsset()
    const sourceGetPayload = (await runCli('asset', 'get', source)) as {
      success: boolean
      result?: { guid: string }
    }
    const sourceGuid = sourceGetPayload.result!.guid

    const copyPath = `${TEMP_DIR}/CliCopy_${Date.now()}${ext}`
    const movePath = `${TEMP_DIR}/CliMove_${Date.now()}${ext}`
    createdAssets.push(movePath)

    const copyPayload = (await runCli('asset', 'copy', source, copyPath)) as {
      success: boolean
      result?: { sourcePath: string; newPath: string; guid: string }
    }
    assert.equal(copyPayload.success, true)
    assert.equal(copyPayload.result?.sourcePath, source)
    assert.equal(copyPayload.result?.newPath, copyPath)
    assert.notEqual(copyPayload.result?.guid, sourceGuid)

    const copyGuid = copyPayload.result!.guid
    const movePayload = (await runCli('asset', 'move', copyPath, movePath)) as {
      success: boolean
      result?: { oldPath: string; newPath: string; guid: string }
    }
    assert.equal(movePayload.success, true)
    assert.equal(movePayload.result?.oldPath, copyPath)
    assert.equal(movePayload.result?.newPath, movePath)
    assert.equal(movePayload.result?.guid, copyGuid)

    const deletePayload = (await runCli('asset', 'delete', movePath)) as {
      success: boolean
      result?: { assetPath: string; deleted: boolean }
    }
    assert.equal(deletePayload.success, true)
    assert.equal(deletePayload.result?.deleted, true)
  })

  it('asset import reimports an asset', async () => {
    const findPayload = (await runCli('asset', 'find', '--limit', '1')) as {
      success: boolean
      result?: { assets: Array<{ assetPath: string }> }
    }
    assert.ok((findPayload.result?.assets.length ?? 0) > 0)

    const assetPath = findPayload.result!.assets[0].assetPath
    const payload = (await runCli('asset', 'import', assetPath)) as {
      success: boolean
      result?: { assetPath: string; importerType: string }
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.assetPath, assetPath)
    assert.ok(payload.result!.importerType.includes('Importer'))
  })

  it('asset import-settings get reads importer properties', async () => {
    const findPayload = (await runCli('asset', 'find', '--limit', '1')) as {
      success: boolean
      result?: { assets: Array<{ assetPath: string }> }
    }
    assert.ok((findPayload.result?.assets.length ?? 0) > 0)

    const assetPath = findPayload.result!.assets[0].assetPath
    const payload = (await runCli('asset', 'import-settings', 'get', assetPath)) as {
      success: boolean
      result?: { assetPath: string; importerType: string; properties: Record<string, unknown> }
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.assetPath, assetPath)
    assert.ok(payload.result!.importerType.includes('Importer'))
    assert.ok(payload.result!.properties !== null && typeof payload.result!.properties === 'object')
  })

  it('asset labels add and remove manage labels', async () => {
    const assetPath = await createTempAsset('CliLabelTarget')
    const label = `ScenicCliTest_${Date.now()}`

    const addPayload = (await runCli('asset', 'labels', 'add', assetPath, label)) as {
      success: boolean
      result?: { assetPath: string; labels: string[]; added: string[] }
    }
    assert.equal(addPayload.success, true)
    assert.ok(addPayload.result!.added.includes(label))
    assert.ok(addPayload.result!.labels.includes(label))

    const removePayload = (await runCli('asset', 'labels', 'remove', assetPath, label)) as {
      success: boolean
      result?: { assetPath: string; labels: string[]; removed: string[] }
    }
    assert.equal(removePayload.success, true)
    assert.ok(removePayload.result!.removed.includes(label))
    assert.ok(!removePayload.result!.labels.includes(label))
  })
})
