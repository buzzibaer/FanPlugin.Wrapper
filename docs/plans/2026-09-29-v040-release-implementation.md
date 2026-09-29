# Version 0.4.0 Release Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Release the current wrapper as GitHub release `v0.4.0` with a matching `0.4.0.0` DLL version.

**Architecture:** Update the assembly metadata on `main`, validate a Release build and tests, then create a signed-off Git tag and GitHub release that uploads only the compiled DLL.

**Tech Stack:** .NET Framework 4.7.2, MSTest, Git, GitHub CLI.

---

### Task 1: Version the assembly

**Files:**
- Modify: `Properties/AssemblyInfo.cs:36-37`
- Test: `tests/FanPlugin.Wrapper.Tests/FanVersionTests.cs`

**Step 1: Write the failing version test**

Add an MSTest that loads the wrapper assembly and asserts:

```csharp
Assert.AreEqual("0.4.0.0", typeof(Fan).Assembly.GetName().Version.ToString());
```

**Step 2: Run the test to verify it fails**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter FanVersionTests`

Expected: FAIL because the assembly version is `1.0.0.0`.

**Step 3: Set matching assembly metadata**

Set both attributes:

```csharp
[assembly: AssemblyVersion("0.4.0.0")]
[assembly: AssemblyFileVersion("0.4.0.0")]
```

**Step 4: Verify tests and Release build**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore`

Expected: PASS.

Run: `dotnet build FanPlugin.Wrapper.csproj -c Release --no-restore`

Expected: Build succeeded with 0 warnings and 0 errors; `bin\\Release\\FanPlugin.Wrapper.dll` exists.

**Step 5: Commit and push**

```bash
git add Properties/AssemblyInfo.cs tests/FanPlugin.Wrapper.Tests/FanVersionTests.cs
git commit -m "Bump assembly version to 0.4.0"
git push origin main
```

### Task 2: Create and verify the GitHub release

**Files:**
- Upload: `bin/Release/FanPlugin.Wrapper.dll`

**Step 1: Confirm release inputs**

Run: `git status --short`

Expected: no output.

Run: `git log -1 --oneline`

Expected: the version-bump commit is `HEAD` and has been pushed to `origin/main`.

**Step 2: Create tag and release**

Run:

```bash
gh release create v0.4.0 bin/Release/FanPlugin.Wrapper.dll --target main --title "FanPlugin Version 0.4.0" --notes "## Highlights
- Add Fan20320 SD-card playback support.
- Add configurable endpoints and network timeouts.
- Keep the selected fan instance alive across PupScript events.
- Validate video IDs before connecting to the fan.

## Hardware validation
FanV3 and Fan20320 have not yet been verified with physical hardware."
```

**Step 3: Verify GitHub release**

Run: `gh release view v0.4.0 --json tagName,name,targetCommitish,assets`

Expected: tag `v0.4.0`, title `FanPlugin Version 0.4.0`, target `main`, and asset `FanPlugin.Wrapper.dll`.
