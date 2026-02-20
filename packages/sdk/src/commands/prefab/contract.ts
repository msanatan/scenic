import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const TransformSpaceSchema = v.picklist(['local', 'world'])

const Vector3Schema = v.object({
  x: v.number(),
  y: v.number(),
  z: v.number(),
})

const CreateTransformSchema = v.object({
  space: v.optional(TransformSpaceSchema),
  position: v.optional(Vector3Schema),
  rotation: v.optional(Vector3Schema),
  scale: v.optional(Vector3Schema),
})

const TransformSnapshotSchema = v.object({
  position: Vector3Schema,
  rotation: Vector3Schema,
  scale: Vector3Schema,
})

export const PrefabInstantiateInputSchema = v.object({
  prefabPath: v.string(),
  parentPath: v.optional(v.string()),
  parentInstanceId: v.optional(v.number()),
  transform: v.optional(CreateTransformSchema),
})

export const PrefabInstantiateResultSchema = v.object({
  prefabPath: v.string(),
  name: v.string(),
  path: v.string(),
  instanceId: v.number(),
  siblingIndex: v.number(),
  isActive: v.boolean(),
  transform: TransformSnapshotSchema,
})

export const prefabInstantiateCommand = defineCommand({
  method: 'prefabInstantiate',
  wire: 'prefab.instantiate',
  params: (input: PrefabInstantiateInput) => ({
    prefabPath: input.prefabPath,
    parentPath: input.parentPath,
    parentInstanceId: input.parentInstanceId,
    transform: input.transform,
  }),
  result: PrefabInstantiateResultSchema,
})

export const PrefabSaveInputSchema = v.object({
  prefabPath: v.string(),
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
})

export const PrefabSaveResultSchema = v.object({
  prefabPath: v.string(),
  sourceName: v.string(),
  sourcePath: v.string(),
  sourceInstanceId: v.number(),
})

export const prefabSaveCommand = defineCommand({
  method: 'prefabSave',
  wire: 'prefab.save',
  params: (input: PrefabSaveInput) => ({
    prefabPath: input.prefabPath,
    path: input.path,
    instanceId: input.instanceId,
  }),
  result: PrefabSaveResultSchema,
})

export type TransformSpace = v.InferOutput<typeof TransformSpaceSchema>
export type Vector3Value = v.InferOutput<typeof Vector3Schema>
export type CreateTransform = v.InferOutput<typeof CreateTransformSchema>
export type PrefabInstantiateInput = v.InferOutput<typeof PrefabInstantiateInputSchema>
export type PrefabInstantiateResult = InferResult<typeof prefabInstantiateCommand>
export type PrefabSaveInput = v.InferOutput<typeof PrefabSaveInputSchema>
export type PrefabSaveResult = InferResult<typeof prefabSaveCommand>
