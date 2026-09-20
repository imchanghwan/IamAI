# IamAI

A multiplayer web game. **Status: development environment only - no game implementation yet.**

## Stack

TypeScript, Phaser 4 (rendering), Vite (bundler), Colyseus 0.18 (multiplayer server), ESLint (flat config + typescript-eslint), Prettier, Vitest, Git/GitHub.

## Setup

Requires Node >= 20 (`.nvmrc` pins 22) and npm.

```sh
npm install
npm run dev        # client (Vite, http://localhost:5173) + server (tsx watch)
```

## Scripts

| Command             | Purpose                                   |
| ------------------- | ----------------------------------------- |
| `npm run dev`       | Run client and server concurrently        |
| `npm run build`     | Build shared, server, then client         |
| `npm run lint`      | ESLint over the whole repo                |
| `npm run format`    | Prettier write (`format:check` to verify) |
| `npm run typecheck` | `tsc -b` across project references        |
| `npm test`          | Vitest (workspace projects)               |

## Structure

```
client/   Vite + TypeScript + Phaser (browser app)
server/   Colyseus + TypeScript (run with tsx in dev)
shared/   Shared TypeScript types, consumed as @iamai/shared
docs/     Environment and tooling notes
```

## Deployment roadmap (not set up yet)

- Web: Nginx serving `client/dist`, reverse-proxying WebSocket to the server
- Server process: PM2 (`ecosystem.config.cjs` placeholder)
- Desktop / Steam: Electron (not installed)
- iOS / Android: Capacitor (not installed)
