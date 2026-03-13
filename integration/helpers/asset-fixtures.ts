import path from 'node:path'

export const TEMP_DIR = 'Assets/__TempTests__'
export const FIXTURE_TEXT_PATH = 'Assets/TestAssets/TextFileForTesting.txt'
export const FIXTURE_MATERIAL_PATH = 'Assets/TestAssets/RedMaterial.mat'

export type AssetFixtureOps = {
  copy: (src: string, dest: string) => Promise<void>
}

export function makeAssetFixtures(ops: AssetFixtureOps, createdAssets: string[]) {
  function findCopyableAsset(): { assetPath: string; ext: string } {
    return { assetPath: FIXTURE_TEXT_PATH, ext: path.extname(FIXTURE_TEXT_PATH) }
  }

  async function createTempAsset(prefix = 'TempAsset'): Promise<string> {
    const { assetPath: source, ext } = findCopyableAsset()
    const tempPath = `${TEMP_DIR}/${prefix}_${Date.now()}${ext}`
    await ops.copy(source, tempPath)
    createdAssets.push(tempPath)
    return tempPath
  }

  return { findCopyableAsset, createTempAsset }
}
