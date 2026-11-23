import { useEffect, useRef, useState } from 'react'
import { changePublication, deleteGame, getGame } from './catalogApi'
import type { CatalogGame, GameDetails } from './catalogApi'

function safeLink(value: string | null) {
  if (!value) return undefined
  try {
    const url = new URL(value)
    return ['https:', 'http:'].includes(url.protocol) ? url.href : undefined
  } catch { return undefined }
}

export default function GameDetailsDialog({ game, blocked, sessionActive, onClose, onEdit, onPlay, onChanged }: {
  game: CatalogGame
  blocked: boolean
  sessionActive: boolean
  onClose: () => void
  onEdit: (game: GameDetails) => void
  onPlay: (game: CatalogGame) => void
  onChanged: (message: string) => void
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [details, setDetails] = useState<GameDetails | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    dialog.current?.showModal()
    const controller = new AbortController()
    getGame(game.id, controller.signal).then(
      (value) => { if (!controller.signal.aborted) { setDetails(value); setError('') } },
      (cause) => { if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : 'Unable to load game details.') },
    )
    return () => controller.abort()
  }, [game.id, version])

  async function mutate(action: 'publish' | 'delete') {
    if (!details || busy) return
    setBusy(true)
    setError('')
    try {
      if (action === 'delete') {
        await deleteGame(details.id)
        onChanged(`${details.name} deleted.`)
        dialog.current?.close()
      } else {
        await changePublication(details.id, !details.isPublished)
        setDetails({ ...details, isPublished: !details.isPublished })
        onChanged(`${details.name} ${details.isPublished ? 'unpublished' : 'published'}.`)
      }
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Unable to update the game.')
    } finally { setBusy(false) }
  }

  return (
    <dialog ref={dialog} className="add-dialog details-dialog" aria-labelledby="details-title" onClose={onClose} onCancel={(event) => { if (busy) event.preventDefault() }}>
      <div className="dialog-heading">
        <h2 id="details-title">{game.name}</h2>
        <button className="close-button" aria-label="Close game details" disabled={busy} onClick={() => dialog.current?.close()}>×</button>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
      {!details && !error && <p role="status">Loading game details…</p>}
      {!details && error && <button className="button button-cream" onClick={() => setVersion((value) => value + 1)}>Retry details</button>}
      {details && <>
        <p className="publication-status">{details.isPublished ? 'Published' : 'Draft'} · {details.genre}</p>
        {details.description && <p className="game-description">{details.description}</p>}
        <dl className="details-grid">
          <div><dt>Publisher</dt><dd>{details.publisher}</dd></div>
          <div><dt>Developer</dt><dd>{details.developer}</dd></div>
          <div><dt>Release date</dt><dd>{new Date(details.releaseDate).toLocaleDateString(undefined, { timeZone: 'UTC' })}</dd></div>
          <div><dt>Launcher</dt><dd>{details.launchConfig.launcher} · {details.launchConfig.appId}</dd></div>
          <div><dt>Runtime</dt><dd>{details.launchConfig.runtime} {details.launchConfig.protonVersion}</dd></div>
          <div><dt>Hardware</dt><dd>{details.requirements.cpuCores} CPU cores · {details.requirements.memoryGb} GB RAM · {details.requirements.gpuCount} GPU · {details.requirements.storageGb} GB storage</dd></div>
        </dl>
        <div className="card-actions">
          {safeLink(details.gameUrl) && <a className="button button-cream" href={safeLink(details.gameUrl)} target="_blank" rel="noreferrer">Game website</a>}
          {safeLink(details.trailerUrl) && <a className="button button-cream" href={safeLink(details.trailerUrl)} target="_blank" rel="noreferrer">Watch trailer</a>}
        </div>
        <p id="details-playback-note">{blocked ? 'Mobile gameplay is currently unsupported. Use a desktop to play.' : !details.isPublished ? 'Publish this game to enable play.' : sessionActive ? 'Your current session is shown on the catalog page.' : 'Request a cloud gaming session for this game.'}</p>
        <div className="card-actions">
          <button className="button button-pink" disabled={busy || blocked || !details.isPublished || sessionActive} aria-describedby="details-playback-note" onClick={() => { onPlay(details); dialog.current?.close() }}>Play</button>
          <button className="button button-cream" disabled={busy} onClick={() => { onEdit(details); dialog.current?.close() }}>Edit game</button>
          <button className="button button-cream" disabled={busy} onClick={() => void mutate('publish')}>{busy ? 'Saving…' : details.isPublished ? 'Unpublish' : 'Publish'}</button>
          <button className="button button-cream danger" disabled={busy} onClick={() => setConfirmDelete(true)}>Delete game</button>
        </div>
        {confirmDelete && <div className="delete-confirmation">
          <p>Delete {details.name} permanently? This cannot be undone.</p>
          <div className="card-actions">
            <button className="button button-cream" disabled={busy} onClick={() => setConfirmDelete(false)}>Keep game</button>
            <button className="button button-pink" disabled={busy} onClick={() => void mutate('delete')}>{busy ? 'Deleting…' : 'Confirm delete'}</button>
          </div>
        </div>}
      </>}
    </dialog>
  )
}
