# CloudGameShare frontend

Start PostgreSQL using the Catalog API's configured connection string. From the repository root, start the API in a separate terminal.

```powershell
dotnet run --project src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi --launch-profile http
```

From this directory, run `npm install` and `npm run dev`. Vite proxies `/api/games` to `http://localhost:5001`. Set `CATALOG_API_TARGET` in the Vite process environment if your API runs at a different address.

When hosting the built frontend, route `/api/games` on the same origin to the Catalog API. The Add Game form saves to that API, and the catalog uses `coverImageUrl` for card images.

With Steam selected as the launcher, enter a title or numeric App ID in Add Game to retrieve metadata. Select a name match to fill the form. Choose English, Portuguese, or Brazilian Portuguese for Steam lookups. Steam's PC minimum requirements fill RAM, storage, and explicitly stated CPU core counts; fractional GB round up to whole GB. Expand Steam hardware requirements to review the original minimum and recommended details for PC and Linux. Review the runtime and allocations, including the GPU count, before saving; processor models, clock speeds, and graphics memory do not specify pod core or GPU counts.

The Catalog API exposes `GET /api/games/steam/search?name=Stardew%20Valley` and `GET /api/games/steam/413150`. Both accept an optional `language` query parameter. Steam lookups use the Store endpoints through Catalog's infrastructure service, with a 10-second timeout and bounded in-memory caching. These Store endpoints have an undocumented contract.

## React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is enabled on this template. See [this documentation](https://react.dev/learn/react-compiler) for more information.

Note: This will impact Vite dev & build performances.
You can also try [the experimental native React Compiler support in plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react/README.md#rust-react-compiler) by using `compiler: true` in the plugin options instead of using the Babel plugin.

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

You can also install [eslint-plugin-react-x](https://npmx.dev/package/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://npmx.dev/package/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```
