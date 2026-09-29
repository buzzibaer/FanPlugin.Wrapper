# Version 0.4.0 Release Design

## Goal

Publish the current Fan20320 and documentation changes as a manually created GitHub release with a DLL version that matches the release version.

## Versioning

- Git tag: `v0.4.0`
- GitHub release title: `FanPlugin Version 0.4.0`
- `AssemblyVersion`: `0.4.0.0`
- `AssemblyFileVersion`: `0.4.0.0`

## Release Asset

Build `FanPlugin.Wrapper.csproj` in the Release configuration and upload only `bin\\Release\\FanPlugin.Wrapper.dll`, consistent with the existing `v03` release.

## Release Notes

Summarize Fan20320 SD-card playback support, configurable network settings, ID validation, persistent PupScript fan instances, and the note that FanV3 and Fan20320 are not yet verified on physical hardware.
