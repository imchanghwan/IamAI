# Environment

- Node v24 used for setup; `engines.node >=20`, `.nvmrc` = 22. npm 11 workspaces (no pnpm).
- Workspaces: `shared`, `client`, `server`. TypeScript project references (`tsc -b`) with `tsconfig.base.json`.
- TypeScript is pinned to `~6.0.3` because typescript-eslint supports `<6.1.0` (TS 7 is unsupported by it).
- Colyseus: latest stable is 0.18.x (`@colyseus/core`, `@colyseus/ws-transport`). Server-side `@colyseus/schema` comes transitively; the client SDK (`colyseus.js`) is not installed yet.
- Vitest 5 runs via root `vitest.config.ts` `projects`; each package has its own config and a smoke test.
- ESLint flat config in `eslint.config.js`, with eslint-config-prettier last.
- Ports: Vite 5173. Server port TBD.

## Not installed yet (roadmap)

Electron (Steam), Capacitor (iOS/Android), Nginx and PM2 (deploy). `ecosystem.config.cjs` is a placeholder pointing at `server/dist/index.js`.

## Placeholders

`client/src/main.ts` and `server/src/index.ts` only log a line; `smoke.test.ts` files assert true. Replace when implementation begins.
