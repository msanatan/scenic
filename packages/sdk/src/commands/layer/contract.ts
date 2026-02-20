import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const LayerItemSchema = v.object({
  index: v.number(),
  name: v.string(),
  isBuiltIn: v.boolean(),
  isUserEditable: v.boolean(),
  isOccupied: v.boolean(),
})

export const LayersGetResultSchema = v.object({
  layers: v.array(LayerItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export interface LayersGetQuery {
  limit?: number
  offset?: number
}

export const layersGetCommand = defineCommand({
  method: 'layersGet',
  wire: 'layers.get',
  params: (query?: LayersGetQuery) => ({
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: LayersGetResultSchema,
})

export const LayersAddInputSchema = v.object({
  name: v.string(),
})

export const LayersAddResultSchema = v.object({
  layer: LayerItemSchema,
  added: v.boolean(),
  total: v.number(),
})

export const layersAddCommand = defineCommand({
  method: 'layersAdd',
  wire: 'layers.add',
  params: (input: LayersAddInput) => ({
    name: input.name,
  }),
  result: LayersAddResultSchema,
})

export const LayersRemoveInputSchema = v.object({
  name: v.string(),
})

export const LayersRemoveResultSchema = v.object({
  layer: LayerItemSchema,
  removed: v.boolean(),
  total: v.number(),
})

export const layersRemoveCommand = defineCommand({
  method: 'layersRemove',
  wire: 'layers.remove',
  params: (input: LayersRemoveInput) => ({
    name: input.name,
  }),
  result: LayersRemoveResultSchema,
})

export type LayerItem = v.InferOutput<typeof LayerItemSchema>
export type LayersGetResult = InferResult<typeof layersGetCommand>
export type LayersAddInput = v.InferOutput<typeof LayersAddInputSchema>
export type LayersAddResult = InferResult<typeof layersAddCommand>
export type LayersRemoveInput = v.InferOutput<typeof LayersRemoveInputSchema>
export type LayersRemoveResult = InferResult<typeof layersRemoveCommand>
