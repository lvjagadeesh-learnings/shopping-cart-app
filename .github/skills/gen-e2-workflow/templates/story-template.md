---
ticket-id: '[ticket-id]'
ticket-title: '[Short ticket title]'
created: '[YYYY-MM-DD]'
status: draft
---

# [Short ticket title]

## Description

<!--
Restate the ticket in your own words: what problem exists today, what the user/business
needs, and why it matters. 2-5 sentences. Do not just copy-paste the raw ticket verbatim —
clarify anything ambiguous in the original text.
-->

## Acceptance Criteria

<!--
Each item must be independently testable/verifiable — a reviewer or a test should be able to
confirm pass/fail without more context. Prefer Given/When/Then or short bullet statements.
Number them (AC1, AC2, ...) so the implementation plan can reference them directly.
-->

- **AC1**: Given [state], when [action], then [observable outcome]
- **AC2**: ...

## Definition of Done

<!--
Concrete, project-specific checklist — not generic filler. Cover at minimum: tests written
and passing for every Acceptance Criterion above, code reviewed, no regressions in existing
functionality, docs updated if user-facing behavior changed, and merged per this repo's
branching convention. Add ticket-specific items as needed (e.g. accessibility check,
performance budget, migration script run).
-->

- [ ] All Acceptance Criteria above are covered by passing tests
- [ ] Code reviewed and approved
- [ ] No regressions in existing functionality
- [ ] Documentation updated (if applicable)
- [ ] Changes merged per the repo's branching convention
