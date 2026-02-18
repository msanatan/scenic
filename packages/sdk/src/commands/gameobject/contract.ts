import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const GameObjectDimensionSchema = v.picklist(['2d', '3d'])
const PrimitiveSchema = v.picklist(['cube', 'sphere', 'capsule', 'cylinder', 'plane', 'quad'])
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

export const GameObjectCreateInputSchema = v.object({
  name: v.string(),
  parent: v.optional(v.string()),
  dimension: v.optional(GameObjectDimensionSchema),
  primitive: v.optional(PrimitiveSchema),
  transform: v.optional(CreateTransformSchema),
})

export const GameObjectCreateResultSchema = v.object({
  name: v.string(),
  path: v.string(),
  isActive: v.boolean(),
  siblingIndex: v.number(),
})

export const gameObjectCreateCommand = defineCommand({
  method: 'gameObjectCreate',
  wire: 'gameobject.create',
  params: (input: GameObjectCreateInput) => ({
    name: input.name,
    parent: input.parent,
    dimension: input.dimension,
    primitive: input.primitive,
    transform: input.transform,
  }),
  result: GameObjectCreateResultSchema,
})

export type GameObjectDimension = v.InferOutput<typeof GameObjectDimensionSchema>
export type PrimitiveTypeName = v.InferOutput<typeof PrimitiveSchema>
export type TransformSpace = v.InferOutput<typeof TransformSpaceSchema>
export type Vector3Value = v.InferOutput<typeof Vector3Schema>
export type CreateTransform = v.InferOutput<typeof CreateTransformSchema>
export type GameObjectCreateInput = v.InferOutput<typeof GameObjectCreateInputSchema>
export type GameObjectCreateResult = InferResult<typeof gameObjectCreateCommand>
