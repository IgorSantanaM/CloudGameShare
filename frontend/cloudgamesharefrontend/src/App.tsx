import { useCallback, useEffect, useRef, useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import controllerArt from './assets/arcade-controller.png'
import defaultCover from './assets/default-game-cover.png'
import { createGame, getCatalog, updateGame } from './catalogApi'
import type { CatalogGame, GameDetails, SteamGameDetails } from './catalogApi'
import SteamAutofill from './SteamAutofill'
import GameDetailsDialog from './GameDetailsDialog'
import SessionPanel from './SessionPanel'
import { isPlaybackBlocked, usePlaybackBlocked } from './playback'
import './App.css'

type CatalogState =
  | { status: 'loading' }
  | { status: 'ready'; games: CatalogGame[] }
  | { status: 'error' }

const iconPaths = {
  controller: 'M4 4h8v1h2v2h1v5h-1v1h-3v-1H5v1H2v-1H1V7h1V5h2zm1 2v2H3v1h2v2h1V9h2V8H6V6zm6 0v2h2V6zm-1 3v2h2V9z',
  plus: 'M7 2h2v5h5v2H9v5H7V9H2V7h5z',
  arrow: 'M8 2h2v2h2v2h2v2h-2v2h-2v2H8v-2h2V8H2V6h8V4H8z',
  search: 'M3 1h7v2h2v7h-2v2H3v-2H1V3h2zm1 2v1H3v5h1v1h5V9h1V4H9V3zm8 8h2v2h2v2h-2v-2h-2z',
  close: 'M3 2h2v2h2v2h2V4h2V2h2v3h-2v2H9v2h2v2h2v3h-2v-2H9v-2H7v2H5v2H3v-3h2V9h2V7H5V5H3z',
  grid: 'M2 2h5v5H2zm7 0h5v5H9zM2 9h5v5H2zm7 0h5v5H9z',
}

function PixelIcon({ name }: { name: keyof typeof iconPaths }) {
  return (
    <svg aria-hidden="true" viewBox="0 0 16 16" fill="currentColor" width="1em" height="1em" shapeRendering="crispEdges">
      <path d={iconPaths[name]} />
    </svg>
  )
}

function GameCover({ imageUrl }: { imageUrl: string | null }) {
  const [failedUrl, setFailedUrl] = useState<string>()
  const url = imageUrl?.trim()

  return url && failedUrl !== url ? (
    <img className="game-cover" src={url} alt="" loading="lazy" onError={() => setFailedUrl(url)} />
  ) : (
    <img className="game-cover default-cover" src={defaultCover} alt="" loading="lazy" />
  )
}

function App() {
  const [catalog, setCatalog] = useState<CatalogState>({ status: 'loading' })
  const [catalogVersion, setCatalogVersion] = useState(0)
  const [query, setQuery] = useState('')
  const [filter, setFilter] = useState<string | null>(null)
  const [announcement, setAnnouncement] = useState('')
  const [formError, setFormError] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const [isAddOpen, setIsAddOpen] = useState(false)
  const [steamQuery, setSteamQuery] = useState('')
  const [editingGame, setEditingGame] = useState<GameDetails | null>(null)
  const [selectedGame, setSelectedGame] = useState<CatalogGame | null>(null)
  const [sessionGame, setSessionGame] = useState<CatalogGame | null>(null)
  const blocked = usePlaybackBlocked()
  const addDialog = useRef<HTMLDialogElement>(null)
  const addForm = useRef<HTMLFormElement>(null)
  const games = catalog.status === 'ready' ? catalog.games : []
  const genres = [...new Set(games.map((game) => game.genre))].sort((first, second) => first.localeCompare(second))
  const searchTerm = query.trim().toLowerCase()
  const visibleGames = games.filter((game) =>
    (filter === null || game.genre === filter) &&
    game.name.toLowerCase().includes(searchTerm),
  )

  useEffect(() => {
    const controller = new AbortController()
    getCatalog(controller.signal).then(
      (games) => {
        if (!controller.signal.aborted) setCatalog({ status: 'ready', games })
      },
      () => {
        if (!controller.signal.aborted) setCatalog({ status: 'error' })
      },
    )
    return () => controller.abort()
  }, [catalogVersion])

  useEffect(() => {
    if (!announcement) return

    const timer = window.setTimeout(() => setAnnouncement(''), 4200)
    return () => window.clearTimeout(timer)
  }, [announcement])

  const applySteamGame = useCallback((game: SteamGameDetails) => {
    const values = {
      name: game.name,
      description: game.description ?? '',
      genre: game.genres.join(', '),
      publisher: game.publishers.join(', '),
      developer: game.developers.join(', '),
      releaseDate: game.releaseDate ?? '',
      imageUrl: game.coverImageUrl ?? '',
      appId: String(game.appId),
      launcher: 'Steam',
    }
    for (const [name, value] of Object.entries(values)) {
      const field = addForm.current?.elements.namedItem(name)
      if (field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement || field instanceof HTMLSelectElement) {
        field.value = value
      }
    }
  }, [])

  function lookupSteam(event: ChangeEvent<HTMLInputElement>) {
    const launcher = event.currentTarget.form?.elements.namedItem('launcher')
    if (launcher instanceof HTMLSelectElement && launcher.value === 'Steam') {
      setSteamQuery(event.currentTarget.value)
    }
  }

  function clearFilters() {
    setQuery('')
    setFilter(null)
  }

  function reloadCatalog() {
    setCatalog({ status: 'loading' })
    setCatalogVersion((version) => version + 1)
  }

  function editGame(game: GameDetails) {
    setEditingGame(game)
    setFormError('')
    setIsAddOpen(true)
    const values = {
      ...game,
      ...game.requirements,
      ...game.launchConfig,
      imageUrl: game.coverImageUrl ?? '',
      releaseDate: game.releaseDate.slice(0, 10),
    }
    for (const [name, value] of Object.entries(values)) {
      const field = addForm.current?.elements.namedItem(name)
      if (field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement || field instanceof HTMLSelectElement) {
        field.value = String(value ?? '')
      }
    }
    addDialog.current?.showModal()
  }

  function playGame(game: CatalogGame) {
    if (isPlaybackBlocked() || !game.isPublished || sessionGame) return
    setSessionGame(game)
  }

  async function addGame(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSaving) return

    const form = event.currentTarget
    const fields = new FormData(form)
    const value = (field: string) => String(fields.get(field) ?? '').trim()
    const name = value('name')
    const date = value('releaseDate')

    setFormError('')
    setIsSaving(true)
    try {
      const payload = {
        name,
        genre: value('genre'),
        publisher: value('publisher'),
        developer: value('developer'),
        description: value('description') || null,
        releaseDate: `${date}T00:00:00Z`,
        coverImageUrl: value('imageUrl') || null,
        trailerUrl: value('trailerUrl') || null,
        gameUrl: value('gameUrl') || null,
        requirements: {
          cpuCores: Number(value('cpuCores')),
          memoryGb: Number(value('memoryGb')),
          gpuCount: Number(value('gpuCount')),
          storageGb: Number(value('storageGb')),
        },
        launchConfig: {
          launcher: value('launcher'),
          appId: value('appId'),
          runtime: value('runtime'),
          protonVersion: value('protonVersion') || null,
        },
      }
      if (editingGame) await updateGame(editingGame.id, payload)
      else await createGame(payload)
      form.reset()
      addDialog.current?.close()
      clearFilters()
      setAnnouncement(`${name} ${editingGame ? 'updated' : 'added to the catalog'}.`)
      reloadCatalog()
    } catch (error) {
      setFormError(error instanceof Error ? error.message : 'Unable to save the game.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <>
      <header className="site-header" id="top">
        <div className="page-width header-inner">
          <a className="brand" href="#top" aria-label="CloudGameShare home">
            <span className="brand-icon"><PixelIcon name="controller" /></span>
            <span>CloudGameShare</span>
          </a>
          <nav className="site-nav" aria-label="Main navigation">
            <a href="#catalog"><PixelIcon name="grid" /> Catalog</a>
          </nav>
          <button className="button button-pink header-add" type="button" onClick={() => {
            setEditingGame(null)
            addForm.current?.reset()
            setFormError('')
            setIsAddOpen(true)
            addDialog.current?.showModal()
          }}>
            <PixelIcon name="plus" /> Add Game
          </button>
        </div>
      </header>

      <main className="page-width">
        <section className="hero" aria-labelledby="hero-title">
          <div className="hero-copy">
            <h1 id="hero-title">The Game<br /><span>Library.</span></h1>
            <p>Find your next game or add one to the collection.</p>
            <a className="button button-cream" href="#catalog">
              Browse Games
            </a>
          </div>
          <img className="hero-art" src={controllerArt} alt="" width="1536" height="1024" />
        </section>

        <section className="catalog" id="catalog" aria-labelledby="catalog-title">
          {blocked && <p className="mobile-notice" id="mobile-playback-note">Mobile gameplay is currently unsupported. You can browse and manage games here. Use a desktop to play.</p>}
          {sessionGame && <SessionPanel key={sessionGame.id} game={sessionGame} onDismiss={() => setSessionGame(null)} />}
          <div className="catalog-heading">
            <h2 id="catalog-title">Game Catalog</h2>
            <label className="search">
              <PixelIcon name="search" />
              <span className="sr-only">Search games by title</span>
              <input type="search" placeholder="Search games…" value={query} onChange={(event) => setQuery(event.target.value)} />
            </label>
          </div>

          {games.length > 0 && <div className="catalog-tools">
            <div className="filters" role="group" aria-label="Filter by genre">
              <button className="filter" type="button" aria-pressed={filter === null} onClick={() => setFilter(null)}>All games</button>
              {genres.map((genre) => (
                <button key={genre} className="filter" type="button" aria-pressed={filter === genre} onClick={() => setFilter(genre)}>
                  {genre}
                </button>
              ))}
            </div>
            <p className="result-count" aria-live="polite">{visibleGames.length} {visibleGames.length === 1 ? 'game' : 'games'}</p>
          </div>}

          {catalog.status === 'loading' ? (
            <div className="empty" role="status"><p>Loading the catalog…</p></div>
          ) : catalog.status === 'error' ? (
            <div className="empty">
              <p role="alert">Couldn’t load the catalog.</p>
              <button type="button" className="button button-cream" onClick={reloadCatalog}>Try Again</button>
            </div>
          ) : visibleGames.length > 0 ? (
            <ul className="game-grid">
              {visibleGames.map((game) => (
                <li key={game.id}>
                  <article className="game-card">
                    <div className="game-art">
                      <GameCover imageUrl={game.coverImageUrl} />
                      <span className="genre-tag">{game.genre}</span>
                    </div>
                    <div className="game-info">
                      <h3>{game.name}</h3>
                      <span className="publication-status">{game.isPublished ? 'Published' : 'Draft'}</span>
                      {game.description && <p>{game.description}</p>}
                      <div className="card-actions">
                        <button className="button button-cream" onClick={() => setSelectedGame(game)}>Details</button>
                        <button className="button button-pink" disabled={blocked || !game.isPublished || !!sessionGame} aria-describedby={blocked ? 'mobile-playback-note' : undefined} onClick={() => playGame(game)}>Play</button>
                      </div>
                      {!blocked && !game.isPublished && <p className="playback-note">Publish from Details to enable play.</p>}
                    </div>
                  </article>
                </li>
              ))}
            </ul>
          ) : games.length > 0 ? (
            <div className="empty">
              <PixelIcon name="search" />
              <p>No games match your search.</p>
              <button type="button" className="button button-cream" onClick={clearFilters}>Clear Filters</button>
            </div>
          ) : <div className="empty"><p>No games in the catalog yet.</p></div>}
        </section>
      </main>

      <footer className="site-footer">
        <div className="page-width footer-inner">
          <span>CloudGameShare</span>
        </div>
      </footer>

      <p className="announcement" role="status" hidden={!announcement}>{announcement}</p>

      {selectedGame && <GameDetailsDialog key={selectedGame.id} game={selectedGame} blocked={blocked} sessionActive={!!sessionGame} onClose={() => setSelectedGame(null)} onEdit={editGame} onPlay={playGame} onChanged={(message) => { setAnnouncement(message); reloadCatalog() }} />}

      <dialog ref={addDialog} className="add-dialog" aria-labelledby="add-title" onClose={() => {
        setIsAddOpen(false)
        setSteamQuery('')
      }} onCancel={(event) => {
        if (isSaving) event.preventDefault()
      }}>
        <div className="dialog-heading">
          <h2 id="add-title">{editingGame ? 'Edit Game' : 'Add Game'}</h2>
          <button type="button" className="close-button" aria-label="Close game form" disabled={isSaving} onClick={() => addDialog.current?.close()}>
            <PixelIcon name="close" />
          </button>
        </div>
        <form ref={addForm} onSubmit={addGame}>
          {isAddOpen && <SteamAutofill query={steamQuery} disabled={isSaving} onApply={applySteamGame} />}
          <fieldset className="add-fields" disabled={isSaving}>
            <div className="form-grid">
              <label>Game title <input name="name" placeholder="e.g. Stardew Valley or 413150" required pattern=".*\S.*" autoFocus onChange={lookupSteam} /></label>
              <label>Genre <input name="genre" list="catalog-genres" required pattern=".*\S.*" /></label>
              <datalist id="catalog-genres">{genres.map((genre) => <option key={genre} value={genre} />)}</datalist>
              <label>Publisher <input name="publisher" required pattern=".*\S.*" /></label>
              <label>Developer <input name="developer" required pattern=".*\S.*" /></label>
              <label>Release date <input type="date" name="releaseDate" required /></label>
              <label>
                <span>Cover image URL <span className="optional">(optional)</span></span>
                <input type="url" name="imageUrl" placeholder="https://example.com/cover.png" />
              </label>
              <label>Trailer URL (optional)<input type="url" name="trailerUrl" /></label>
              <label>Game website URL (optional)<input type="url" name="gameUrl" /></label>
            </div>
            <label>
              <span>Description <span className="optional">(optional)</span></span>
              <textarea name="description" rows={2} />
            </label>
            <fieldset className="form-group">
              <legend>Launch configuration</legend>
              <div className="form-grid launch-fields">
                <label>
                  Launcher
                  <select name="launcher" defaultValue="Steam" onChange={() => setSteamQuery('')}>
                    <option>Steam</option>
                    <option value="EpicGames">Epic Games</option>
                    <option value="Gog">GOG</option>
                    <option value="EaApp">EA App</option>
                    <option value="UbisoftConnect">Ubisoft Connect</option>
                    <option value="BattleNet">Battle.net</option>
                    <option value="RiotClient">Riot Client</option>
                    <option value="RockstarGames">Rockstar Games</option>
                    <option value="XboxPc">Xbox PC</option>
                    <option value="AmazonGames">Amazon Games</option>
                    <option value="ItchIo">itch.io</option>
                    <option>Standalone</option>
                  </select>
                </label>
                <label>App ID <input name="appId" required pattern=".*\S.*" onChange={lookupSteam} /></label>
                <label>
                  Runtime
                  <select name="runtime" defaultValue="LinuxNative">
                    <option value="LinuxNative">Linux native</option>
                    <option>Proton</option>
                    <option>Wine</option>
                    <option value="WindowsNative">Windows native</option>
                  </select>
                </label>
                <label>Proton version (optional)<input name="protonVersion" /></label>
              </div>
            </fieldset>
            <fieldset className="form-group">
              <legend>Hardware requirements</legend>
              <div className="form-grid hardware-fields">
                <label>CPU cores <input type="number" name="cpuCores" min="1" required /></label>
                <label>Memory (GB) <input type="number" name="memoryGb" min="1" required /></label>
                <label>GPU count <input type="number" name="gpuCount" min="1" required /></label>
                <label>Storage (GB) <input type="number" name="storageGb" min="1" required /></label>
              </div>
            </fieldset>
          </fieldset>
          {formError && <p className="form-error" role="alert">{formError}</p>}
          <div className="dialog-actions">
            <button type="button" className="button button-cream" disabled={isSaving} onClick={() => addDialog.current?.close()}>Cancel</button>
            <button className="button button-pink" type="submit" disabled={isSaving}><PixelIcon name="plus" /> {isSaving ? 'Saving…' : editingGame ? 'Save Changes' : 'Add Game'}</button>
          </div>
        </form>
      </dialog>
    </>
  )
}

export default App
