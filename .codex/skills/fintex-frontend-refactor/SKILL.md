---
name: fintex-frontend-refactor
description: Use when refactoring or extending the Fintex Next.js frontend. Enforces lower-case dashed folder names, modular feature folders, axios-based API utilities, hook-safe React structure, files under 300 lines where practical, and readable provider/component boundaries.
---

# Fintex Frontend Refactor

Use this skill for any frontend work inside `Frontend/nextjs`.

## Goals

- Keep folders lower-case and dashed when multi-word.
- Keep files small and easy to scan. Prefer staying under `300` lines.
- Use React hooks only at the top level of components or custom hooks.
- Keep provider `index.tsx` files lean. Move effect-heavy logic into local hooks when possible.
- Use `axios` through the shared instance in [utils/axios-instance.ts](../../../../Frontend/nextjs/utils/axios-instance.ts).
- Prefer modular feature folders over large one-file screens.

## Structure rules

- Shared UI goes in `components/`.
- Shared hooks go in `hooks/`.
- Shared API clients and normalizers go in `utils/`.
- Shared types go in `types/`.
- Large features should use a feature folder, for example:

```text
components/dashboard/paper-trading-panel/
  index.tsx
  types.ts
  use-controller.ts
  accounts-modal.tsx
  trade-modal.tsx
```

- Route files such as `app/**/page.tsx` should stay thin and delegate to feature components.

## API rules

- Build API calls with `getAxiosInstance()`.
- Keep request builders and response normalizers out of components.
- Prefer one API utility per domain, such as:
  - `utils/paper-trading-api.ts`
  - `utils/live-trading-api.ts`
  - `utils/market-data-api.ts`

## Hook rules

- Never call hooks inside conditions, loops, or nested functions.
- Custom hooks must start with `use`.
- If a component has many handlers, state values, and effects, extract a custom hook.
- If a provider needs polling, subscriptions, or hydration, prefer:
  - `providers/<feature>-provider/index.tsx` for wiring
  - `providers/<feature>-provider/use-<feature>-provider.ts` for side effects

## Component rules

- Keep presentational components mostly declarative.
- Push formatting, mapping, and orchestration into helpers or hooks.
- Add short comments only when the intent is not obvious from the code.
- Use explicit prop types.

## Refactor workflow

1. Inspect the feature and identify oversized files, mixed responsibilities, and hook/API issues.
2. Create or reuse a feature folder with lower-case dashed naming.
3. Extract API access to axios-based utilities.
4. Extract orchestration into custom hooks.
5. Split JSX into focused subcomponents.
6. Run `npm run lint` and `npm run build`.

## Preferred outcomes

- A senior engineer should be able to find:
  - where state lives
  - where API calls happen
  - where UI rendering happens
  - where shared types/helpers live

- The final structure should feel predictable, shallow, and easy to extend.
