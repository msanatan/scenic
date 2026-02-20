# Contributing to UniBridge

Thanks for your interest in UniBridge. This document explains how contributions work here \- please read it before opening an issue or pull request.

## Motivation

AI coding tools are excellent, we make heavy use of them. There's nothing against vibe coding, but you can't vibe think. Taking the shortest path to a new feature or fix has many merits, but those merits tend to fade away during a projec'ts long term maintenance.

Here's why we want discussions before PRs:

- Architectural changes should be aligned to the project's near, medium and long term goals. That contexts primarily exist with the maintainers.
- Code writing is easier, but the burden has shifted to code reading \- which is harder. Additionally: there's a lot PRs, there's a continuous stream of PRs, and a lot of code in each PR. Agents will be more autonmous but the human in the middle is ultimately responsible for the quality of the software. Figure out the right design before coding produces the best results.
- The more context we have for a change, the easier it is to give feedback and approve it.

## Issues First, Always

**Do not open a pull request without a linked, approved issue.**

The workflow is:

1. **Open an issue** using one of the provided templates (bug report or feature request).
2. **Wait for a maintainer response.** An open issue does not mean it's accepted — discussion happens first.
3. **Get explicit approval** before pushing any code. A maintainer will indicate when an issue is ready for a PR.
4. **Submit a focused PR** that links back to the approved issue.

PRs opened without a corresponding approved issue will be closed.

## Quality Standards

UniBridge has a small, intentional codebase. Contributions need to meet the same bar as the rest of the project:

- **Understand the code you're changing.** Read the surrounding context. Follow existing patterns and conventions.
- **Minimal dependencies** Speed and size are important to this repo, we do our best to add dependencies that are necessary, lightweight and well-maintained.
- **Keep changes focused.** One issue, one PR. Don't bundle unrelated changes.
- **Test your work.** Run existing tests and add new ones where appropriate.
- **Review your own code before submitting.** If you used AI tools to help write code, you are responsible for reviewing, understanding, and testing every line. Bulk AI-generated code that clearly wasn't reviewed will be closed without further discussion.

This repo has a test Unity project. Everything you need to test each component end-to-end is already here in the repo.

## What Gets Closed Immediately

- Pull requests without a linked, approved issue
- Unsolicited PRs (refactors, "improvements", dependency bumps nobody asked for)
- Code that doesn't follow existing project conventions
- Submissions that show no evidence of testing or understanding the codebase

## Communication

- Do NOT use AI tools to fill out the issue and PR templates
    - OSS is more than a license, it's a community - we want to connect with other humans.
- English is the primary language of communication
    - If you are not a native English speaker, fill out the templates in your native language. Our browsers have translation features, it should be fine.
    - Translating is a good use of AI, but do both. Use your native tongue and then use an LLM to translate to English if you would like.

## Questions?

If you're unsure about anything, open an issue and ask. That's exactly what the issue-first workflow is for.
