# Implementation Plan Guidelines

Detailed guidance for Phase 2 (`templates/implementation-plan-template.md`). Read this before
filling in an `implementation-plan.md`.

## Ground the plan in the real codebase — don't write generic advice

Before writing a single implementation step, explore the actual project:

- Search for existing code/patterns that already solve similar problems (naming, folder
  structure, error handling style) and follow them rather than inventing a new pattern.
- Check for applicable `.github/instructions/*.instructions.md` files (matched by `applyTo`
  glob against the files you expect to touch) and follow their conventions.
- Identify the exact files that will need to change. A step like "update the cart service"
  without a file path is not specific enough — resolve it to `path/to/CartService.cs` (or
  the actual project's equivalent) before writing it down.

If you cannot find enough context to write a concrete, file-aware step, say so and ask the
user rather than guessing.

## Building the Requirement Mapping list

Every Acceptance Criterion (`AC1`, `AC2`, ...) from `story.md` must appear in this list at
least once, mapped to:

- The implementation step number(s) that satisfy it.
- A concrete verification method (a specific unit/integration test name or a precise manual
  check) — not just "test it".

If an Acceptance Criterion has no corresponding step, the plan is incomplete — add the
missing step(s) rather than leaving a gap.

## Sizing and ordering steps

- Each step should be small enough to verify independently (roughly: one logical change, one
  or a few related files).
- Note dependencies between steps explicitly when order matters (e.g. "Step 3 depends on the
  migration added in Step 1").
- Prefer the natural build order of the codebase (e.g. data model → service/business logic →
  API/controller → UI) unless the ticket dictates otherwise.

## Scope

Be explicit about what is **out of scope** as well as in scope. This prevents the execution
phase from silently expanding the ticket (scope creep) or from missing something the
Acceptance Criteria actually require.

## Before moving to Phase 3

Show the completed plan to the user. Phase 3 makes real code changes — the plan is the last
point where the *approach* can be corrected cheaply, before the *implementation* begins.
