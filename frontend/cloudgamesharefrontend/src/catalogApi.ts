export type CatalogGame = {
  id: string
  name: string
  genre: string
  description: string | null
  coverImageUrl: string | null
  isPublished: boolean
}

export type CreateGame = {
  name: string
  genre: string
  description: string | null
  coverImageUrl: string | null
  trailerUrl: string | null
  gameUrl: string | null
  publisher: string
  developer: string
  releaseDate: string
  requirements: { cpuCores: number; memoryGb: number; gpuCount: number; storageGb: number }
  launchConfig: { launcher: string; appId: string; runtime: string; protonVersion: string | null }
}

export type GameDetails = CreateGame & { id: string; isPublished: boolean }

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
type CatalogError = { title?: string; detail?: string; errors?: { error: string }[] | Record<string, string[]> }

const gamesUrl = '/api/games'

export async function requestApi(path: string, options?: RequestInit) {
  const response = await fetch(path, options)
  if (response.ok) return response

  const problem: CatalogError | null = await response.json().catch(() => null)
  const errors = problem?.errors
  const details = Array.isArray(errors) ? errors.map(({ error }) => error).join(' ') : Object.values(errors ?? {}).flat().join(' ')
  const fallback = response.status === 401 ? 'Sign in is required by the server to perform this action.' : `API returned HTTP ${response.status}.`
  throw new Error(details || problem?.detail || problem?.title || fallback)
}

async function getPage(pageNumber: number, signal: AbortSignal): Promise<CatalogPage> {
  const response = await requestApi(`${gamesUrl}?pageNumber=${pageNumber}&pageSize=100`, { signal })
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
  await requestApi(`${gamesUrl}/create`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(game),
  })
}

export async function searchSteamGames(name: string, language: string, signal: AbortSignal): Promise<SteamGameSummary[]> {
  const query = new URLSearchParams({ name, language })
  const response = await requestApi(`${gamesUrl}/steam/search?${query}`, { signal })
  return response.json()
}

export async function getSteamGame(appId: number, language: string, signal: AbortSignal): Promise<SteamGameDetails> {
  const response = await requestApi(`${gamesUrl}/steam/${appId}?language=${language}`, { signal })
  return response.json()
}

export async function getGame(id: string, signal: AbortSignal): Promise<GameDetails> {
  return (await requestApi(`${gamesUrl}/${encodeURIComponent(id)}`, { signal })).json()
}

export async function updateGame(id: string, game: CreateGame): Promise<void> {
  await requestApi(`${gamesUrl}/${encodeURIComponent(id)}`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(game),
  })
}

export async function changePublication(id: string, published: boolean): Promise<void> {
  await requestApi(`${gamesUrl}/${encodeURIComponent(id)}/${published ? 'publish' : 'unpublish'}`, { method: 'POST' })
}

export async function deleteGame(id: string): Promise<void> {
  await requestApi(`${gamesUrl}/${encodeURIComponent(id)}`, { method: 'DELETE' })
}
