# Goal 3.5 — implementation progress

Base verified: `352afd2`, clean worktree before changes. Goal 3 is not being repeated.

## Verified so far

- Domain NameParser now treats three components as one given name and two surnames.
- Particles de/del/la/las/los attach to the following word (including De la Hoz).
- Angular preview uses the same heuristic. No global case conversion.
- 14 targeted NameParser tests passed, including all eight named examples and whitespace normalization.
- BusinessName validation now identifies the business name independently from the extended legal name.
- Advanced typed criteria are wired into the existing HTTP search/use case/repository;
  migration `20260907170000_AddAdvancedCustomerSearch` adds a typed routine without
  removing existing routines. Migration has NOT yet been applied or runtime-tested.
- Backend build after these changes passes with zero warnings/errors.
- Assistant contracts define model interpretation, responses, context and server-owned
  pending actions. Provider, storage and use-case implementations are still pending.

The advanced routine currently returns at most 101 rows, intended as a 100-row display
limit plus overflow indicator. Count handling must explicitly address overflow before
the feature can be considered complete.

Name parsing remains ambiguous: Juan Carlos Pérez defaults to Juan / Carlos, Pérez;
Ana María Pérez-Gómez defaults to Ana / María, Pérez-Gómez. Editable name components
allow correction. The heuristic preserves accents, case, hyphens and apostrophes.

## Remaining scope (not complete)

Advanced reusable search and migration; assistant Application orchestration, typed
interpretations, OpenRouter adapter, confirmation actions; Angular chat; API-based
idempotent demo seed; backend/fake-model tests; real provider smoke if configured;
full-stack and new-surface browser QA; README/assistant architecture/PROJECT_STATE;
safe secret scan and final commit only after validation.

Initial environment-name-only checks found no OPENROUTER-named process, user or
machine variables. Further safe configuration discovery remains pending. No secret
values have been printed. No deployment or push is authorized.
