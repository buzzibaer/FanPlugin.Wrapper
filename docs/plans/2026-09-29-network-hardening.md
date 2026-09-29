# Fan Network Hardening Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Reject invalid SD-card file IDs and ensure fan network operations have bounded, reportable failures for both Fan and FanV3.

**Architecture:** Keep protocol command construction in `Fan` and `FanV3`, while sharing transport timeout behavior in a small internal TCP helper. Expose per-instance timeout settings through COM just like the existing endpoint settings. Validate video IDs before shared-memory or network side effects, and only update playback history after a command is sent successfully.

**Tech Stack:** C# / .NET Framework 4.7.2, COM-visible wrapper classes, PupScript JScript, MSTest with local loopback TCP tests.

---

### Task 1: Add focused automated tests for video-ID validation

**Files:**
- Create: `tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`
- Create: `tests/FanPlugin.Wrapper.Tests/FanVideoIdTests.cs`
- Modify: `Properties/AssemblyInfo.cs` (grant test assembly internal visibility if the validator remains internal)

The test project targets `net472`, references `FanPlugin.Wrapper.csproj`, and uses `Microsoft.NET.Test.Sdk` 17.12.0 with `MSTest.TestAdapter` and `MSTest.TestFramework` 3.6.4.

**Step 1: Add tests for numeric format and supported range**

Cover `0` and `99` as accepted IDs, and null/empty, non-numeric, negative, and `100` as rejected IDs. Use real validation code, not a copy of its logic in the tests.

**Step 2: Run the new test project and verify RED**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: the tests fail because the shared validator does not exist yet.

**Step 3: Implement the minimal shared validator**

Add an internal helper that uses `int.TryParse` and accepts only values from `0` through `99`. Both hardware classes will use this same validation before touching shared memory.

**Step 4: Run the tests and verify GREEN**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: all validator tests pass.

### Task 2: Apply ID validation and truthful playback results to both variants

**Files:**
- Modify: `Fan.cs` (active Version 2 implementation)
- Modify: `FanV3.cs`
- Test: `tests/FanPlugin.Wrapper.Tests/FanPlaybackTests.cs`

**Step 1: Add tests for invalid IDs and transport failure propagation**

For both classes, verify invalid IDs return an invalid-ID result before shared-memory access or a network connection. Use a closed loopback port to verify that `playVideoWithId` returns the transport failure rather than a success-style `NEW ID` message.

**Step 2: Run the tests and verify RED**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: existing `int.Parse` throws for malformed IDs, and failed sends are currently hidden by `playVideoWithId`.

**Step 3: Implement validation and success-only history updates**

Call the shared validator before opening shared memory. Send the version-specific play command and propagate any transport error. Update `actual`/`last` only after the command write succeeds. Keep the existing successful result text for callers.

**Step 4: Run the tests and verify GREEN**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: invalid values have no shared-memory/network side effects and transport failures are returned to the caller for both variants.

### Task 3: Implement bounded TCP connect, read, and write operations

**Files:**
- Create: `FanTcpTransport.cs`
- Modify: `Fan.cs`
- Modify: `FanV3.cs`
- Modify: `FanPlugin.Wrapper.csproj`
- Modify: `tests/FanPlugin.Wrapper.Tests/FanTcpTransportTests.cs`

**Step 1: Add loopback tests for read timeout and connection failure**

Run a local `TcpListener`, accept a request, and intentionally do not send a response. Assert that a read operation returns a timeout failure within a bounded interval. Also test a closed loopback port for a prompt connection-refused result.

**Step 2: Run the tests and verify RED**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: the no-response case blocks beyond the desired bound with the current implementation.

**Step 3: Add a shared transport helper**

Use `TcpClient.BeginConnect` with a bounded wait, call `EndConnect` when it completes, and close the client when the wait expires. Set `NetworkStream.ReadTimeout` and `WriteTimeout`; dispose the client, stream, and async wait handle on every path. Catch `IOException` as well as socket errors because stream timeouts surface as I/O exceptions. Expose `ConnectTimeoutMs` and `SocketTimeoutMs` as public instance properties on both COM classes, with a 3000 ms default for each.

**Step 4: Route both hardware classes through the helper**

Retain each class's existing command strings and endpoint defaults. Delegate only TCP connection, write, and optional response read behavior to the shared helper. Preserve the existing string-returning COM methods, returning the helper's transport error when an operation fails.

**Step 5: Run the tests and verify GREEN**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: loopback timeout and connection-failure tests pass for Fan and FanV3.

### Task 4: Wire optional timeout overrides and document behavior

**Files:**
- Modify: `pupscript/pupscript_js.pup`
- Modify: `README.md`
- Test: `tests/FanPlugin.Wrapper.Tests/FanComConfigurationTests.cs`

**Step 1: Add COM configuration checks**

Verify `ServerIp`, `ServerPort`, `ConnectTimeoutMs`, and `SocketTimeoutMs` are public instance properties on both classes, and verify the documented defaults.

**Step 2: Run the configuration tests and verify RED**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Expected: timeout properties are not present yet.

**Step 3: Add optional PupScript timeout overrides**

Add `FAN_CONNECT_TIMEOUT_OVERRIDE` and `FAN_SOCKET_TIMEOUT_OVERRIDE`, defaulting to `0` to retain class defaults. Set the corresponding COM instance properties in `CreateFan()` only when the override is positive.

**Step 4: Document all variant defaults and overrides**

Update the hardware selection section with both hardware endpoints, the 3000 ms connect/read-write defaults, and the optional IP/port/timeout PupScript overrides. State that blank/zero overrides retain the selected class defaults.

**Step 5: Run all verification**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj`

Run: `dotnet build FanPlugin.Wrapper.csproj --no-restore`

Run: `cscript.exe //nologo //E:JScript pupscript/pupscript_js.pup`

Expected: all tests pass, the wrapper builds with no errors, and the PupScript parses as JScript.

## Scope notes

- Keep Version 2 and Version 3 protocol command construction separate.
- Preserve each class's documented default endpoint; overrides apply only to the selected instance.
- Do not depend on a physical fan for automated tests; use a local loopback listener.
- TCP has no message boundaries. This plan bounds the existing single response read but does not invent a new response framing rule; full response framing can be addressed when the device protocol's termination/length rules are established.
