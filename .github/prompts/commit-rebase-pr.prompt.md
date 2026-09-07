---
description: "Commit staged changes with a Conventional Commit message, rebase onto latest main, push, and create or update the PR — keeps the branch conflict-free before merge."
name: "Commit, Rebase & PR"
argument-hint: "optional: commit type/scope/message override"
agent: agent
tools: ['runCommands', 'editFiles']
---
Commit, rebase, push, and open/update a pull request for the current feature branch. Do this as one sequential workflow — do not skip the rebase step, since it's what keeps the eventual PR merge conflict-free.

1. **Guard rails**: Run `git branch --show-current`. If it's `main`, `master`, or `develop`, stop and tell the user to create a feature branch first — never commit/push directly to these.

2. **Commit**: Run `git status --short` to see staged/unstaged changes. Draft a [Conventional Commits](https://www.conventionalcommits.org/) message: `type(scope): summary`, types limited to `feat|fix|docs|refactor|test|chore|ci|perf|security`, scope = the affected service/folder (e.g. `infra`, `order-service`, `frontend`). Add a body only if the change needs explaining beyond the summary. Stage all relevant changes (`git add -A` unless the user specified specific files) and commit.

3. **Rebase on latest `main`** (do this every time, even if you just branched recently):
   - `git fetch origin main`
   - `git rebase origin/main`
   - If conflicts occur: list the conflicting files, resolve them (or ask the user how to resolve if the resolution isn't obvious from context), `git add` the resolved files, `git rebase --continue`. If the user prefers to bail, `git rebase --abort` and warn that the PR may show conflicts.

4. **Push**:
   - If the branch has no upstream yet: `git push -u origin <branch>`.
   - If it does and a rebase happened: `git push --force-with-lease` (never plain `--force`).
   - If it does and no rebase happened (nothing to replay): plain `git push`.

5. **Create or update the PR**:
   - Check for an existing PR: `gh pr view --json url,number,state` (or `gh pr list --head <branch>`).
   - If `gh auth status` fails, tell the user to run `gh auth login` and stop.
   - **If no PR exists**: `gh pr create --base main --title "<same as commit summary>" --body "<commit body, or a short bullet summary of the diff>"`.
   - **If a PR exists**:
     - **If `state` is MERGED**: stop and tell the user: "PR #N is already merged. To create a new PR, check out a fresh feature branch (e.g. `git checkout -b <new-branch> origin/main`), make new changes, and run this prompt again."
     - **If `state` is CLOSED**: ask the user: "PR #N is closed. Reopen it with `gh pr reopen` and push again, or create a new PR instead?"
     - **If `state` is OPEN or DRAFT**: nothing further to do — the push above already updated it; just report its URL/number and state back to the user.

6. **Report**: Summarize branch name, commit hash(es), whether a rebase/conflict occurred, push result, the PR URL/number, and the final PR state (merged/closed/open/draft). If a PR was already merged, remind the user to create a new feature branch for any further changes.

Never force-push without having just rebased. Never skip step 3 — an un-rebased branch is exactly what causes PR merge conflicts.
