# Fan20320 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a COM-visible `Fan20320` hardware implementation that selects an existing zero-padded numeric SD-card file by ID through the Android app's port-20320 protocol.

**Architecture:** Keep `playVideoWithId(string)` as the public compatibility API. `Fan20320` owns a persistent `TcpClient`, sends the Android command handshake, parses the initial file-list frame into ordered names, converts a requested decimal ID into its six-digit filename, and sends the file-list index as the `B` command payload. Extract frame construction and parsing into an internal protocol helper so byte-level behavior can be unit tested without a fan.

**Tech Stack:** C# 7.3, .NET Framework 4.7.2, TCP sockets, MSTest 3.6.4, COM automation, PupScript JScript.

---

### Task 1: Add protocol-frame tests

**Files:**
- Create: `tests/FanPlugin.Wrapper.Tests/Fan20320ProtocolTests.cs`
- Create: `Fan20320Protocol.cs`

**Step 1: Write the failing test**

Create tests for the exact byte representation recovered from `MainActivity.java`:

```csharp
[TestMethod]
public void BuildCommandFrame_UsesMarkersAndEncodedLength()
{
    CollectionAssert.AreEqual(
        Encoding.ASCII.GetBytes("C0EEB7C9BAA3")
            .Concat(new byte[] { 98, 99, 99 })
            .Concat(new byte[] { (byte)'e' })
            .Concat(Encoding.ASCII.GetBytes("C0EEBDF9E5B7"))
            .ToArray(),
        Fan20320Protocol.BuildCommandFrame(new byte[] { (byte)'e' }));
}

[TestMethod]
public void BuildCommandFrame_WritesFileIndexAsRawByte()
{
    byte[] frame = Fan20320Protocol.BuildCommandFrame(new byte[] { (byte)'B', 1 });

    Assert.AreEqual(1, frame[17]);
}

[TestMethod]
public void TryParseFileList_ParsesOrderedNamesAndStatusSuffix()
{
    byte[] frame = CreateFileListFrame(new[] { "000001.bin", "000005.bin" }, 1);
    string[] files;
    Assert.IsTrue(Fan20320Protocol.TryParseFileList(frame, out files));
    CollectionAssert.AreEqual(new[] { "000001.bin", "000005.bin" }, files);
}

[TestMethod]
public void TryParseFileList_RejectsInvalidFrame()
{
    string[] files;
    Assert.IsFalse(Fan20320Protocol.TryParseFileList(new byte[0], out files));
}
```

Implement the test-only `CreateFileListFrame` helper in the same test class. It must build the app's `i` response: start marker, three encoded length bytes, command byte `i`, length-prefixed GB2312 names, sixteen status bytes, and end marker.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320ProtocolTests`

Expected: FAIL because `Fan20320Protocol` does not exist.

**Step 3: Write minimal implementation**

Create internal static `Fan20320Protocol` with:

```csharp
internal static readonly byte[] StartMarker = Encoding.ASCII.GetBytes("C0EEB7C9BAA3");
internal static readonly byte[] EndMarker = Encoding.ASCII.GetBytes("C0EEBDF9E5B7");

internal static byte[] BuildCommandFrame(byte[] command)
{
    if (command == null || command.Length > 65534) throw new ArgumentOutOfRangeException("command");
    int length = command.Length;
    return StartMarker.Concat(new[] {
        (byte)(length / 323),
        (byte)(((length / 17) % 19) + 99),
        (byte)((length % 17) + 98)
    }).Concat(command).Concat(EndMarker).ToArray();
}
```

Implement `TryParseFileList(byte[] frame, out string[] files)` without `Substring` or string length calculations. Validate markers and frame length by reversing the Android formula:

```csharp
int payloadLength = (frame[12] * 323) + ((frame[13] - 99) * 17) + (frame[14] - 98);
```

Require command byte `i`, then parse from offset 16 until the final sixteen status bytes. Each name starts with an unsigned byte length and is decoded with `Encoding.GetEncoding("GB2312")`. Return false on malformed boundaries, an invalid length, or invalid markers.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320ProtocolTests`

Expected: PASS.

**Step 5: Commit**

```bash
git add Fan20320Protocol.cs tests/FanPlugin.Wrapper.Tests/Fan20320ProtocolTests.cs FanPlugin.Wrapper.csproj
git commit -m "Add Fan20320 protocol framing"
```

### Task 2: Implement and test ID-to-file-list mapping

**Files:**
- Modify: `tests/FanPlugin.Wrapper.Tests/Fan20320ProtocolTests.cs`
- Modify: `Fan20320Protocol.cs`

**Step 1: Write the failing test**

Add mapping tests:

```csharp
[TestMethod]
public void TryFindFileIndex_UsesPaddedFilenameRatherThanIdAsIndex()
{
    int index;
    Assert.IsTrue(Fan20320Protocol.TryFindFileIndex(
        new[] { "000001.bin", "000005.bin", "000010.bin" }, 5, out index));
    Assert.AreEqual(1, index);
}

[TestMethod]
public void TryFindFileIndex_ReturnsFalseForAbsentFile()
{
    int index;
    Assert.IsFalse(Fan20320Protocol.TryFindFileIndex(new[] { "000001.bin" }, 5, out index));
}

[TestMethod]
public void TryFindFileIndex_RejectsIndexOutsideSingleByteRange()
{
    int index;
    Assert.IsFalse(Fan20320Protocol.TryFindFileIndex(CreateNames(257), 256, out index));
}
```

`CreateNames` should generate valid six-digit `.bin` names and ensure the requested name occurs at index 256.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320ProtocolTests`

Expected: FAIL because `TryFindFileIndex` does not exist.

**Step 3: Write minimal implementation**

Add this helper:

```csharp
internal static bool TryFindFileIndex(string[] files, int videoId, out int index)
{
    index = -1;
    string expectedName = videoId.ToString("D6") + ".bin";
    for (int i = 0; i < files.Length; i++)
    {
        if (string.Equals(files[i], expectedName, StringComparison.OrdinalIgnoreCase))
        {
            if (i > byte.MaxValue) return false;
            index = i;
            return true;
        }
    }
    return false;
}
```

Keep the existing `FanVideoId` range validation (`0..99`) in the public class. The index limit is separate because the device selection command can carry only one byte.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320ProtocolTests`

Expected: PASS.

**Step 5: Commit**

```bash
git add Fan20320Protocol.cs tests/FanPlugin.Wrapper.Tests/Fan20320ProtocolTests.cs
git commit -m "Map Fan20320 IDs to file list entries"
```

### Task 3: Add persistent Fan20320 connection and playback tests

**Files:**
- Create: `Fan20320.cs`
- Create: `tests/FanPlugin.Wrapper.Tests/Fan20320Tests.cs`
- Modify: `FanPlugin.Wrapper.csproj`

**Step 1: Write the failing test**

Use a loopback `TcpListener` to emulate one command session. It must verify the initial empty framed command, return the file-list response, then verify the `B` command contains index `1` for ID `5`.

```csharp
[TestMethod]
public void PlayVideoWithId_MapsIdToReturnedFileListIndex()
{
    // Server reads the handshake, sends 000001.bin/000005.bin response,
    // reads the selection frame, and captures its raw B payload.
    Assert.AreEqual("Command successfull", fan.playVideoWithId("5"));
    CollectionAssert.AreEqual(new byte[] { (byte)'B', 1 }, server.SelectionCommand);
}

[TestMethod]
public void PlayVideoWithId_ReturnsNotFoundWithoutSendingSelection()
{
    // Server returns a list without 000005.bin.
    Assert.AreEqual("Video ID 5 not found on fan.", fan.playVideoWithId("5"));
    Assert.IsFalse(server.ReceivedSelection);
}

[TestMethod]
public void PlayVideoWithId_RejectsInvalidIdBeforeNetworkAccess()
{
    var fan = new Fan20320 { ServerIp = "", ServerPort = 0 };
    Assert.AreEqual("Invalid videoID", fan.playVideoWithId("abc"));
}

[TestMethod]
public void Fan20320_DefaultsAreComConfigurable()
{
    AssertComConfiguration(typeof(Fan20320), 20320);
}
```

Move the existing private `AssertComConfiguration` helper from `FanTcpTransportTests` to a shared internal test helper, or duplicate only the minimal assertions. Confirm `ServerIp`, `ServerPort`, `ConnectTimeoutMs`, and `SocketTimeoutMs` are readable and writable COM-visible instance properties.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320Tests`

Expected: FAIL because `Fan20320` does not exist.

**Step 3: Write minimal implementation**

Create a public `Fan20320` class with these defaults and properties:

```csharp
public const string DefaultServerIp = "192.168.4.1";
public const int DefaultServerPort = 20320;
public string ServerIp { get; set; } = DefaultServerIp;
public int ServerPort { get; set; } = DefaultServerPort;
public int ConnectTimeoutMs { get; set; } = 3000;
public int SocketTimeoutMs { get; set; } = 3000;
```

Maintain private `TcpClient client`, `NetworkStream stream`, and `string[] files`. In `playVideoWithId`:

1. Validate `videoID` using `FanVideoId.TryParse`; return `Invalid videoID` on failure.
2. Call `EnsureConnected`. It connects with the same bounded-connect approach as `FanTcpTransport`, configures stream timeouts, sends `BuildCommandFrame(new byte[0])`, and reads complete frames until it receives an `i` file-list frame.
3. Use `TryFindFileIndex`; return `Video ID {id} not found on fan.` if absent.
4. Send `BuildCommandFrame(new byte[] { (byte)'B', (byte)index })` and return `Command successfull`.
5. On socket, IO, timeout, invalid-frame, or disconnect errors, close and clear all session state before returning an existing-style `Network error:` or `Network timeout:` message.

Implement a private read loop that accumulates bytes until both markers and the length-derived frame size are present. Do not assume one `NetworkStream.Read` call equals one complete protocol frame. Limit the receive buffer to a documented safe bound such as 64 KiB and fail cleanly if the announced frame exceeds it.

Add `<Compile Include="Fan20320.cs" />` and `<Compile Include="Fan20320Protocol.cs" />` to the project file.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320Tests`

Expected: PASS.

**Step 5: Commit**

```bash
git add Fan20320.cs Fan20320Protocol.cs FanPlugin.Wrapper.csproj tests/FanPlugin.Wrapper.Tests/Fan20320Tests.cs tests/FanPlugin.Wrapper.Tests/FanTcpTransportTests.cs
git commit -m "Add Fan20320 SD card playback"
```

### Task 4: Expose the new hardware in PupScript and documentation

**Files:**
- Modify: `pupscript/pupscript_js.pup:3-35`
- Modify: `README.md:120-142`
- Modify: `tests/FanPlugin.Wrapper.Tests/Fan20320Tests.cs`

**Step 1: Write the failing test**

Add a source-level regression test that reads the repository PupScript and asserts it contains:

```text
FAN_HARDWARE_VERSION == 20320
FanPlugin.Wrapper.Fan20320
```

This matches the project’s existing script verification approach while remaining executable under .NET tests.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore --filter Fan20320Tests`

Expected: FAIL because the PupScript has no `20320` branch.

**Step 3: Write minimal implementation**

Extend `CreateFan()`:

```js
} else if (FAN_HARDWARE_VERSION == 20320) {
    fan = new ActiveXObject("FanPlugin.Wrapper.Fan20320");
}
```

Keep all existing endpoint and timeout override assignments unchanged; the new class has compatible property names. Update README hardware selection documentation with:

- `20320` selects the Android-app protocol on `192.168.4.1:20320`.
- It chooses existing files by the numeric ID in a six-digit filename, e.g. `playVideoWithId(5)` maps to `000005.bin`.
- The fan receives the list position internally; callers must not pass the list position.
- Media upload is not implemented.

**Step 4: Run verification**

Run: `dotnet test tests/FanPlugin.Wrapper.Tests/FanPlugin.Wrapper.Tests.csproj --no-restore`

Expected: PASS, including existing tests and all new `Fan20320` tests.

Run: `dotnet build FanPlugin.Wrapper.csproj --no-restore`

Expected: Build succeeded with 0 warnings and 0 errors.

Run: `cscript.exe //nologo //E:JScript pupscript\pupscript_js.pup`

Expected: exit code 0. The script parser accepts the new branch; it will not instantiate a fan unless a Pup event calls `CreateFan`.

**Step 5: Commit**

```bash
git add pupscript/pupscript_js.pup README.md tests/FanPlugin.Wrapper.Tests/Fan20320Tests.cs
```
