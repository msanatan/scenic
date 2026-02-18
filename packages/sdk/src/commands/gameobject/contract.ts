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

const TransformSnapshotSchema = v.object({
  position: Vector3Schema,
  rotation: Vector3Schema,
  scale: Vector3Schema,
})

export const GameObjectCreateInputSchema = v.object({
  name: v.string(),
  parent: v.optional(v.string()),
  parentInstanceId: v.optional(v.number()),
  dimension: v.optional(GameObjectDimensionSchema),
  primitive: v.optional(PrimitiveSchema),
  transform: v.optional(CreateTransformSchema),
})

export const GameObjectCreateResultSchema = v.object({
  name: v.string(),
  path: v.string(),
  isActive: v.boolean(),
  siblingIndex: v.number(),
  instanceId: v.number(),
})

export const gameObjectCreateCommand = defineCommand({
  method: 'gameObjectCreate',
  wire: 'gameobject.create',
  params: (input: GameObjectCreateInput) => ({
    name: input.name,
    parent: input.parent,
    parentInstanceId: input.parentInstanceId,
    dimension: input.dimension,
    primitive: input.primitive,
    transform: input.transform,
  }),
  result: GameObjectCreateResultSchema,
})

export const GameObjectDestroyInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
})

export const GameObjectDestroyResultSchema = v.object({
  destroyed: v.boolean(),
  name: v.string(),
  path: v.string(),
  instanceId: v.number(),
})

export const gameObjectDestroyCommand = defineCommand({
  method: 'gameObjectDestroy',
  wire: 'gameobject.destroy',
  params: (input: GameObjectDestroyInput) => ({
    path: input.path,
    instanceId: input.instanceId,
  }),
  result: GameObjectDestroyResultSchema,
})

export const GameObjectUpdateInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  name: v.optional(v.string()),
  tag: v.optional(v.string()),
  layer: v.optional(v.string()),
  isStatic: v.optional(v.boolean()),
  transform: v.optional(CreateTransformSchema),
})

export const GameObjectUpdateResultSchema = v.object({
  name: v.string(),
  path: v.string(),
  instanceId: v.number(),
  tag: v.string(),
  layer: v.string(),
  isStatic: v.boolean(),
  transform: TransformSnapshotSchema,
})

export const gameObjectUpdateCommand = defineCommand({
  method: 'gameObjectUpdate',
  wire: 'gameobject.update',
  params: (input: GameObjectUpdateInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    name: input.name,
    tag: input.tag,
    layer: input.layer,
    isStatic: input.isStatic,
    transform: input.transform,
  }),
  result: GameObjectUpdateResultSchema,
})

export const GameObjectReparentInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  parentPath: v.optional(v.string()),
  parentInstanceId: v.optional(v.number()),
  toRoot: v.optional(v.boolean()),
  worldPositionStays: v.optional(v.boolean()),
})

export const GameObjectReparentResultSchema = v.object({
  name: v.string(),
  path: v.string(),
  instanceId: v.number(),
  parentPath: v.nullable(v.string()),
  siblingIndex: v.number(),
})

export const gameObjectReparentCommand = defineCommand({
  method: 'gameObjectReparent',
  wire: 'gameobject.reparent',
  params: (input: GameObjectReparentInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    parentPath: input.parentPath,
    parentInstanceId: input.parentInstanceId,
    toRoot: input.toRoot,
    worldPositionStays: input.worldPositionStays,
  }),
  result: GameObjectReparentResultSchema,
})

export const GameObjectGetInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
})

export const GameObjectGetResultSchema = v.object({
  name: v.string(),
  path: v.string(),
  instanceId: v.number(),
  isActive: v.boolean(),
  tag: v.string(),
  layer: v.string(),
  isStatic: v.boolean(),
  parentPath: v.nullable(v.string()),
  siblingIndex: v.number(),
  transform: TransformSnapshotSchema,
})

export const gameObjectGetCommand = defineCommand({
  method: 'gameObjectGet',
  wire: 'gameobject.get',
  params: (input: GameObjectGetInput) => ({
    path: input.path,
    instanceId: input.instanceId,
  }),
  result: GameObjectGetResultSchema,
})

export type GameObjectDimension = v.InferOutput<typeof GameObjectDimensionSchema>
export type PrimitiveTypeName = v.InferOutput<typeof PrimitiveSchema>
export type TransformSpace = v.InferOutput<typeof TransformSpaceSchema>
export type Vector3Value = v.InferOutput<typeof Vector3Schema>
export type CreateTransform = v.InferOutput<typeof CreateTransformSchema>
export type GameObjectCreateInput = v.InferOutput<typeof GameObjectCreateInputSchema>
export type GameObjectCreateResult = InferResult<typeof gameObjectCreateCommand>
export type GameObjectDestroyInput = v.InferOutput<typeof GameObjectDestroyInputSchema>
export type GameObjectDestroyResult = InferResult<typeof gameObjectDestroyCommand>
export type GameObjectUpdateInput = v.InferOutput<typeof GameObjectUpdateInputSchema>
export type GameObjectUpdateResult = InferResult<typeof gameObjectUpdateCommand>
export type GameObjectReparentInput = v.InferOutput<typeof GameObjectReparentInputSchema>
export type GameObjectReparentResult = InferResult<typeof gameObjectReparentCommand>
export type GameObjectGetInput = v.InferOutput<typeof GameObjectGetInputSchema>
export type GameObjectGetResult = InferResult<typeof gameObjectGetCommand>
