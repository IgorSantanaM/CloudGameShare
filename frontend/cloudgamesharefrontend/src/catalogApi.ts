export type CatalogGame = {
  id: string
  name: string
  genre: string
  description: string | null
  coverImageUrl: string | null
}

export type CreateGame = {
  name: string
  genre: string
  description: string | null
  coverImageUrl: string | null
  publisher: string
  developer: string
  releaseDate: string
  requirements: { cpuCores: number; memoryGb: number; gpuCount: number; storageGb: number }
  launchConfig: { launcher: string; appId: string; runtime: string }
}

export type SteamGameSummary = { appId: number; name: string; coverImageUrl: string | null }
export type SteamGameDetails = {
  appId: number
  name: string
  description: string | null
  releaseDate: string | null
  genres: string[]
  publishers: string[]
  developers: string[]
  coverImageUrl: string | null
  supportsLinux: boolean
}

type CatalogPage = { items: CatalogGame[]; totalPages: number }
type CatalogError = { title?: string; errors?: { error: string }[] }

const gamesUrl = '/api/games'

async function requestCatalog(path: string, options?: RequestInit) {
  const response = await fetch(path, options)
  if (response.ok) return response

  const problem: CatalogError | null = await response.json().catch(() => null)
  const details = problem?.errors?.map(({ error }) => error).join(' ')
  throw new Error(details || problem?.title || `Catalog API returned HTTP ${response.status}.`)
}

async function getPage(pageNumber: number, signal: AbortSignal): Promise<CatalogPage> {
  const response = await requestCatalog(`${gamesUrl}?pageNumber=${pageNumber}&pageSize=100`, { signal })
  return response.json()
}

export async function getCatalog(signal: AbortSignal): Promise<CatalogGame[]> {
  const firstPage = await getPage(1, signal)
  const games = [...firstPage.items]

  for (let pageNumber = 2; pageNumber <= firstPage.totalPages; pageNumber++) {
    const page = await getPage(pageNumber, signal)
    games.push(...page.items)
  }

  return games
}

export async function createGame(game: CreateGame): Promise<void> {
  await requestCatalog(`${gamesUrl}/create`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(game),
  })
}

export async function searchSteamGames(name: string, language: string, signal: AbortSignal): Promise<SteamGameSummary[]> {
  const query = new URLSearchParams({ name, language })
  const response = await requestCatalog(`${gamesUrl}/steam/search?${query}`, { signal })
  return response.json()
}

export async function getSteamGame(appId: number, language: string, signal: AbortSignal): Promise<SteamGameDetails> {
  const response = await requestCatalog(`${gamesUrl}/steam/${appId}?language=${language}`, { signal })
  return response.json()
}
