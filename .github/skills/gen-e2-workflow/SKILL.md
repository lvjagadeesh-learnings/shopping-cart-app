---
name: gen-e2-workflow
description: 'Palo IT''s Gen e2 ticket-driven workflow. Turns a pasted ticket into a story.md (Description, Acceptance Criteria, Definition of Done), generates a codebase-grounded implementation-plan.md from that story, then executes the plan in VS Code Copilot agent mode while logging progress in execution.md. Use when: create a story from a ticket, write acceptance criteria, define definition of done, generate implementation plan from story, create implementation plan, execute implementation plan, work a ticket, start ticket implementation, Gen e2 workflow, story to plan to code.'
---

# Gen e2 Workflow: Story → Implementation Plan → Execution

Palo IT's "Gen e2" workflow turns a raw ticket into three linked, consistently-formatted
markdown artifacts for one work item, then drives the actual coding through the last one:

1. **Story** — what the ticket asks for, made explicit and testable.
2. **Implementation Plan** — how it will actually be built, grounded in this codebase.
3. **Execution** — the log of doing the work with the Copilot agent, checked off against the plan.

## When to Use This Skill

- The user pastes or describes a ticket/work item and asks to create a story from it.
- The user asks to turn a story into an implementation plan.
- The user asks to execute/work an implementation plan or "start on this ticket".
- The user asks for the Gen e2 workflow by name.

## Prerequisites

- The raw ticket text (pasted or described in chat) — title plus enough detail to write a
  Description, Acceptance Criteria, and Definition of Done. No ticketing-system integration
  is required or used; everything comes from what the user provides in chat.

## Ticket ID (used in every file path — read this first)

Derive one `ticket-id` slug and reuse it verbatim across all three phases:

- If the ticket text contains an explicit ID (e.g. `PROJ-123`, `#456`), lowercase it and
  strip any leading `#` → `proj-123`, `456`.
- Otherwise, slugify the title: lowercase, strip punctuation, collapse whitespace to single
  hyphens, cap at ~50 chars (e.g. "Add cart discount codes" → `add-cart-discount-codes`).
- Confirm the derived slug with the user before creating any files if it's ambiguous.

## Procedure

### Phase 1 — Create the Story

1. Read `references/story-guidelines.md` for how to write testable Acceptance Criteria and a
   concrete Definition of Done.
2. Copy `templates/story-template.md` to `docs/stories/<ticket-id>/story.md` in the **target
   project** (not this skill folder).
3. Fill in the frontmatter (`ticket-id`, `title`, `created`) and the three sections:
   `## Description`, `## Acceptance Criteria`, `## Definition of Done`.
4. Show the filled story to the user before moving to Phase 2.

### Phase 2 — Create the Implementation Plan

1. Read `references/implementation-plan-guidelines.md` for how to ground steps in the real
   codebase and build the Requirement Mapping list.
2. Read the `story.md` from Phase 1 — every Acceptance Criterion must be traceable to at
   least one plan step.
3. Explore the actual codebase (relevant files, existing patterns, applicable
   `.github/instructions/*.instructions.md` files) before writing steps — do not write
   generic advice.
4. Copy `templates/implementation-plan-template.md` to
   `docs/plans/<ticket-id>/implementation-plan.md`, filling in `## Objective`, `## Scope`,
   `## Requirement Mapping`, `## Implementation Steps`, `## Verification`, and
   `## Definition of Done` (copied verbatim from `story.md`).
5. Show the plan to the user for review before Phase 3 — this is the last checkpoint before
   real code changes happen.

### Phase 3 — Execute the Implementation Plan

1. Read `references/execution-guidelines.md` for how to work the plan with the Copilot agent.
2. Copy `templates/execution-template.md` to `docs/plans/<ticket-id>/execution.md`.
3. Work through `implementation-plan.md`'s steps in order. As each step completes: check it
   off in `implementation-plan.md`, and append a dated entry to `execution.md`'s
   `## Execution Log`.
4. Log anything non-obvious (deviations from the plan, blockers, decisions) in `execution.md`
   under `## Deviations & Decisions` / `## Blockers` — never silently improvise past what the
   plan says.
5. When all steps are done, verify the result against every item in `story.md`'s Definition
   of Done and record the outcome in `execution.md`'s `## Completion Summary`.

## Gotchas

- **Reuse the same `ticket-id` everywhere.** A mismatched slug between `docs/stories/` and
  `docs/plans/` breaks the traceability this whole workflow exists for.
- **Don't start Phase 3 before the user has reviewed the plan from Phase 2.** Phase 3 makes
  real code changes; Phases 1–2 are just documents.
- **Acceptance Criteria must be independently testable**, not vague prose — otherwise the
  Requirement Mapping list in the implementation plan has nothing concrete to trace to.
- **Definition of Done is written once**, in `story.md`, and copied verbatim into the plan
  and the execution completion summary. Don't redefine "done" at each phase.
- **If the codebase doesn't match what the plan assumed** (e.g. a referenced file doesn't
  exist), stop and record it in `execution.md`'s Deviations section rather than silently
  reinterpreting the plan — unless it's a trivial, obviously-correct adaptation.

## Output

For one ticket, this skill produces (in the target project, not this skill folder):

- `docs/stories/<ticket-id>/story.md`
- `docs/plans/<ticket-id>/implementation-plan.md`
- `docs/plans/<ticket-id>/execution.md`
