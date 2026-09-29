# Fan Hardware Validation Note Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Clearly mark untested `FanV3` and `Fan20320` variants without expanding the README into a separate setup guide.

**Architecture:** Change only the existing README hardware-selection section. Keep the current concise `Fan20320` configuration description and add a neutral shared validation note.

**Tech Stack:** Markdown, Git.

---

### Task 1: Add the hardware-validation note

**Files:**
- Modify: `README.md:128-131`

**Step 1: Inspect the existing hardware list**

Confirm it already documents `Fan`, `FanV3`, and `Fan20320`, including the `Fan20320` endpoint and file-ID behavior.

**Step 2: Add the minimal note**

Immediately after the three hardware bullets, add:

```markdown
`FanV3` and `Fan20320` have not yet been verified with physical hardware.
```

Do not add a setup section, network guide, troubleshooting material, or implementation-origin explanation.

**Step 3: Verify the rendered-source content**

Run: `rg -n "FanV3.*Fan20320.*physical hardware|20320.*192\.168\.4\.1:20320" README.md`

Expected: both the existing `Fan20320` description and the new neutral validation note are found.

**Step 4: Commit**

```bash
git add README.md
git commit -m "Document untested fan hardware variants"
```
