# Story Guidelines

Detailed guidance for Phase 1 (`templates/story-template.md`). Read this before filling in a
`story.md`.

## Deriving the ticket-id

The `ticket-id` is used as a folder name under both `docs/stories/` and `docs/plans/`, and
must be identical across both.

- If the raw ticket text contains an explicit ID (e.g. `PROJ-123`, `JIRA-4567`, `#456`),
  reuse it verbatim, lowercased, with any leading `#` stripped: `proj-123`, `jira-4567`, `456`.
- Otherwise, slugify the ticket title: lowercase; strip punctuation; collapse runs of
  whitespace/punctuation into single hyphens; trim to roughly 50 characters at a word
  boundary. Example: "Add cart discount codes at checkout" → `add-cart-discount-codes`.
- If it's not obvious which to use (e.g. the ticket mentions multiple IDs, or the title is
  very generic), ask the user to confirm the slug before creating any folders.

## Writing the Description

- Restate the problem/need in your own words — 2–5 sentences. This forces you to actually
  understand the ticket rather than parroting it, and surfaces ambiguity early.
- If the raw ticket is vague or missing context needed to write testable Acceptance Criteria,
  ask the user rather than inventing details.

## Writing Acceptance Criteria

Each Acceptance Criterion must be **independently testable** — someone (or some test) should
be able to check it in isolation without needing the rest of the story for context.

- Prefer Given/When/Then phrasing for behavioral criteria:
  `Given <state>, when <action>, then <observable outcome>.`
- Bullet form is fine for simpler, declarative criteria (e.g. "Discount codes are
  case-insensitive").
- Number them (`AC1`, `AC2`, ...) — Phase 2's Requirement Mapping list references these
  numbers directly, so stable, sequential IDs matter more than clever grouping.
- Avoid criteria that describe *how* something is implemented (that belongs in the
  implementation plan) — describe observable *behavior* only.

## Writing the Definition of Done

The Definition of Done is written **once** here and copied verbatim into the implementation
plan and the execution completion summary in later phases — don't redefine it later.

- Always include: every Acceptance Criterion is covered by a passing test, code reviewed,
  no regressions, docs updated if user-facing behavior changed, merged per the repo's
  branching convention.
- Add ticket-specific items when relevant (e.g. accessibility pass, performance budget,
  data migration executed, feature flag configured) — a generic checklist that never
  changes between tickets is a sign the Definition of Done isn't being thought through.
