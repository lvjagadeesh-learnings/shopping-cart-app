# Execution Guidelines

Detailed guidance for Phase 3 (`templates/execution-template.md`). Read this before starting
work on `execution.md`. This phase is the actual coding work, done with the Copilot agent in
VS Code, guided by `implementation-plan.md`.

## Working the plan

- Work through `implementation-plan.md`'s `## Implementation Steps` in the order and
  dependency notes it specifies.
- After completing a step, check it off in `implementation-plan.md` itself (`[ ]` → `[x]`) —
  the plan is the single source of truth for what's done.
- Append a short, dated entry to `execution.md`'s `## Execution Log` for each completed step
  (or a logical group of steps): what was done, which files changed. Keep entries terse —
  this is a log, not a narrative.

## When the codebase doesn't match the plan

If a step turns out to be wrong or incomplete once you're actually in the code (a referenced
file doesn't exist, an assumption was false, a dependency was missed):

- For a trivial, obviously-correct adaptation (e.g. a file was renamed but the intent is
  identical), just adapt and note it briefly in `## Deviations & Decisions`.
- For anything that changes the *approach* described in the plan, stop, record it in
  `## Deviations & Decisions` (or `## Blockers` if it stops progress entirely), and check
  with the user before improvising further. Never silently reinterpret the plan.

## Running verification as you go

Run the tests/checks listed in `implementation-plan.md`'s `## Verification` section after the
step(s) they apply to, not only at the very end — catching a broken assumption early is
cheaper than discovering it after every step is "done".

## Finishing: Completion Summary

Once every step in `implementation-plan.md` is checked off:

1. Re-read `story.md`'s Acceptance Criteria and confirm each one against the actual result
   (not against the plan's *intent* — against what was actually built).
2. Re-read the Definition of Done (present in both `story.md` and `implementation-plan.md`,
   verbatim) and confirm each item.
3. Fill in `execution.md`'s `## Completion Summary` with the checked-off list and a short
   final status statement.
4. If any Acceptance Criterion or Definition of Done item cannot be checked off, do not mark
   the ticket done — report what's missing to the user instead.
