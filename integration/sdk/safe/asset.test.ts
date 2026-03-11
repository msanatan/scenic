import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'
import { TEMP_DIR, makeAssetFixtures } from '../../helpers/asset-fixtures.ts'

describe('SDK: asset', () => {
  let client: ScenicClient
  const createdAssets: string[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const p of createdAssets) {
      try {
        await client.assetDelete({ assetPath: p })
      } catch {
        // ignore cleanup failures
      }
    }
    client.close()
  })

  const { findCopyableAsset, createTempAsset } = makeAssetFixtures(
    {
      copy: async (src, dest) => { await client.assetCopy({ assetPath: src, newPath: dest }) },
    },
    createdAssets,
  )

  it('assetFind returns paginated results', async () => {
    const result = await client.assetFind({ limit: 5, offset: 0 })

    assert.equal(result.limit, 5)
    assert.equal(result.offset, 0)
    assert.equal(typeof result.total, 'number')
    assert.ok(Array.isArray(result.assets))

    if (result.assets.length > 0) {
      const first = result.assets[0]
      assert.equal(typeof first.assetPath, 'string')
      assert.equal(typeof first.guid, 'string')
      assert.equal(typeof first.type, 'string')
      assert.equal(typeof first.name, 'string')
    }
  })

  it('assetFind filters by type', async () => {
    const result = await client.assetFind({ type: 'Material', limit: 5 })

    assert.ok(Array.isArray(result.assets))
    for (const asset of result.assets) {
      assert.equal(asset.type, 'Material')
    }
  })

  it('assetGet returns metadata for an existing asset', async () => {
    const findResult = await client.assetFind({ limit: 1 })
    assert.ok(findResult.assets.length > 0, 'Need at least one asset')

    const assetPath = findResult.assets[0].assetPath
    const result = await client.assetGet({ assetPath })

    assert.equal(result.assetPath, assetPath)
    assert.ok(result.guid.length > 0)
    assert.ok(result.type.length > 0)
    assert.ok(result.name.length > 0)
    assert.ok(Array.isArray(result.labels))
    assert.ok(Array.isArray(result.dependencies))
  })

  it('assetCopy creates a copy at the new path with a new GUID', async () => {
    const { assetPath: source, ext } = findCopyableAsset()
    const sourceGet = await client.assetGet({ assetPath: source })
    const newPath = `${TEMP_DIR}/SdkCopiedAsset_${Date.now()}${ext}`
    createdAssets.push(newPath)

    const result = await client.assetCopy({ assetPath: source, newPath })

    assert.equal(result.sourcePath, source)
    assert.equal(result.newPath, newPath)
    assert.notEqual(result.guid, sourceGet.guid)
  })

  it('assetMove renames an asset preserving GUID', async () => {
    const { assetPath: source, ext } = findCopyableAsset()
    const createPath = `${TEMP_DIR}/SdkMoveSource_${Date.now()}${ext}`
    await client.assetCopy({ assetPath: source, newPath: createPath })
    const getResult = await client.assetGet({ assetPath: createPath })
    const originalGuid = getResult.guid

    const destPath = `${TEMP_DIR}/SdkMoveDest_${Date.now()}${ext}`
    createdAssets.push(destPath)

    const result = await client.assetMove({ assetPath: createPath, newPath: destPath })

    assert.equal(result.oldPath, createPath)
    assert.equal(result.newPath, destPath)
    assert.equal(result.guid, originalGuid)
  })

  it('assetDelete removes an asset', async () => {
    const { assetPath: source, ext } = findCopyableAsset()
    const copyPath = `${TEMP_DIR}/SdkDeleteTarget_${Date.now()}${ext}`
    await client.assetCopy({ assetPath: source, newPath: copyPath })

    const result = await client.assetDelete({ assetPath: copyPath })

    assert.equal(result.assetPath, copyPath)
    assert.equal(result.deleted, true)
  })

  it('assetImport reimports an asset', async () => {
    const findResult = await client.assetFind({ limit: 1 })
    assert.ok(findResult.assets.length > 0)

    const assetPath = findResult.assets[0].assetPath
    const result = await client.assetImport({ assetPath })

    assert.equal(result.assetPath, assetPath)
    assert.ok(result.importerType.includes('Importer'))
  })

  it('assetImportSettingsGet reads importer properties', async () => {
    const findResult = await client.assetFind({ limit: 1 })
    assert.ok(findResult.assets.length > 0)

    const assetPath = findResult.assets[0].assetPath
    const result = await client.assetImportSettingsGet({ assetPath })

    assert.equal(result.assetPath, assetPath)
    assert.ok(result.importerType.includes('Importer'))
    assert.ok(result.properties !== null && typeof result.properties === 'object')
  })

  it('assetLabelsAdd, assetLabelsGet, and assetLabelsRemove manage the full labels lifecycle', async () => {
    const assetPath = await createTempAsset('SdkLabelTarget')
    const label = `ScenicTest_${Date.now()}`

    const initial = await client.assetLabelsGet({ assetPath })
    assert.ok(!initial.labels.includes(label))

    const addResult = await client.assetLabelsAdd({ assetPath, labels: [label] })
    assert.ok(addResult.added.includes(label))
    assert.ok(addResult.labels.includes(label))

    const afterAdd = await client.assetLabelsGet({ assetPath })
    assert.ok(afterAdd.labels.includes(label))

    const removeResult = await client.assetLabelsRemove({ assetPath, labels: [label] })
    assert.ok(removeResult.removed.includes(label))
    assert.ok(!removeResult.labels.includes(label))

    const afterRemove = await client.assetLabelsGet({ assetPath })
    assert.ok(!afterRemove.labels.includes(label))
  })
})
