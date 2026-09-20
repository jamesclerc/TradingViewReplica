import { apiGet } from './httpClient'
import type { ScreenerDefinition, ScreenerResult } from '../types/market'

export function fetchScreenerDefinitions(): Promise<ScreenerDefinition[]> {
  return apiGet<ScreenerDefinition[]>('/api/screener/definitions')
}

export function fetchScreener(screenerId: string, count = 25): Promise<ScreenerResult> {
  return apiGet<ScreenerResult>(`/api/screener/${encodeURIComponent(screenerId)}?count=${count}`)
}
