import { useEffect, useRef, useState } from 'react'
import { getSteamGame, searchSteamGames } from './catalogApi'
import type { SteamGameDetails, SteamGameSummary } from './catalogApi'

type LookupState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'results'; games: SteamGameSummary[] }
  | { status: 'error'; message: string }
  | { status: 'filled'; game: SteamGameDetails }

type Props = {
  query: string
  disabled: boolean
  onApply: (game: SteamGameDetails) => void
}

export default function SteamAutofill({ query, disabled, onApply }: Props) {
  const [language, setLanguage] = useState('english')
  const [state, setState] = useState<LookupState>({ status: 'idle' })
  const activeRequest = useRef<AbortController | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    activeRequest.current?.abort()
    activeRequest.current = controller
    const input = query.trim()
    const timer = window.setTimeout(async () => {
      if (controller.signal.aborted) return
      if (disabled || input.length === 0 || (!/^\d+$/.test(input) && input.length < 2)) {
        setState({ status: 'idle' })
        return
      }

      setState({ status: 'loading' })
      try {
        if (/^\d+$/.test(input)) {
          const game = await getSteamGame(Number(input), language, controller.signal)
          if (controller.signal.aborted) return
          onApply(game)
          setState({ status: 'filled', game })
        } else {
          const games = await searchSteamGames(input, language, controller.signal)
          if (!controller.signal.aborted) setState({ status: 'results', games })
        }
      } catch (error) {
        if (!controller.signal.aborted) setState({
          status: 'error',
          message: error instanceof Error ? error.message : 'Unable to look up this game.',
        })
      }
    }, 500)

    return () => {
      window.clearTimeout(timer)
      controller.abort()
    }
  }, [query, language, disabled, onApply])

  async function selectGame(appId: number) {
    activeRequest.current?.abort()
    const controller = new AbortController()
    activeRequest.current = controller
    setState({ status: 'loading' })
    try {
      const game = await getSteamGame(appId, language, controller.signal)
      if (controller.signal.aborted) return
      onApply(game)
      setState({ status: 'filled', game })
    } catch (error) {
      if (!controller.signal.aborted) setState({
        status: 'error',
        message: error instanceof Error ? error.message : 'Unable to look up this game.',
      })
    }
  }

  useEffect(() => () => activeRequest.current?.abort(), [])

  return (
    <section className="steam-autofill" aria-label="Steam autofill">
      <div className="autofill-heading">
        <p>Enter a game title or Steam App ID to fill its details.</p>
        <label>
          Steam language
          <select value={language} disabled={disabled} onChange={(event) => setLanguage(event.target.value)}>
            <option value="english">English</option>
            <option value="portuguese">Português</option>
            <option value="brazilian">Português (Brasil)</option>
          </select>
        </label>
      </div>
      <div role="status" aria-live="polite">
        {state.status === 'loading' && <p>Looking up Steam…</p>}
        {state.status === 'filled' && <p>Details filled for {state.game.name}. Review before adding.</p>}
        {state.status === 'results' && state.games.length === 0 && <p>No Steam matches found. You can enter the details manually.</p>}
        {state.status === 'error' && <p>{state.message}</p>}
      </div>
      {state.status === 'filled' && (
        <>
          <p className="requirements-note">Hardware fields use available PC minimum values, rounded up to whole GB. Review core counts and set the GPU count for your session.</p>
          <details className="steam-requirements">
            <summary>Steam hardware requirements</summary>
            {([
              ['PC', state.game.pcRequirements],
              ['Linux', state.game.linuxRequirements],
            ] as const).map(([platform, requirements]) => requirements && (
              <section key={platform} aria-label={`${platform} requirements`}>
                <h3>{platform}</h3>
                {requirements.minimum && <><h4>Minimum</h4><p>{requirements.minimum.text}</p></>}
                {requirements.recommended && <><h4>Recommended</h4><p>{requirements.recommended.text}</p></>}
              </section>
            ))}
            {!state.game.pcRequirements && !state.game.linuxRequirements && <p>Steam has no published hardware requirements for this game.</p>}
          </details>
        </>
      )}
      {state.status === 'results' && state.games.length > 0 && (
        <ul className="steam-results">
          {state.games.map((game) => (
            <li key={game.appId}>
              <button type="button" disabled={disabled} onClick={() => void selectGame(game.appId)}>
                <span>{game.name}</span><small>App ID {game.appId}</small>
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
