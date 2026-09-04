---
ticket-id: '[ticket-id]'
story: '../../stories/[ticket-id]/story.md'
created: '[YYYY-MM-DD]'
status: draft
---

# Implementation Plan: [Short ticket title]

## Objective

<!-- One or two sentences restating the goal from story.md's Description, in delivery terms. -->

## Scope

**In scope:**

- ...

**Out of scope:**

- ...

## Requirement Mapping

<!--
Every Acceptance Criterion from story.md must appear here at least once. This list is the
traceability backbone of the whole workflow — don't skip rows.
-->

- **AC1** → Step 2, Step 3 — Verification: Unit test: ...
- **AC2** → Step 4 — Verification: Manual check: ...

## Implementation Steps

<!--
Numbered, concrete, file-aware. Ground these in the actual codebase (explore it first) —
not generic advice. Note dependencies/ordering between steps where they matter.
-->

1. [ ] [Step description] — files: `path/to/file.ext`
2. [ ] [Step description] — files: `path/to/file.ext`
3. [ ] ...

## Verification

<!-- How the whole change gets validated once all steps are done: tests to run, commands, manual checks. -->

- ...

## Definition of Done

<!-- Copied verbatim from story.md — do not redefine "done" here. -->

- [ ] All Acceptance Criteria above are covered by passing tests
- [ ] Code reviewed and approved
- [ ] No regressions in existing functionality
- [ ] Documentation updated (if applicable)
- [ ] Changes merged per the repo's branching convention
