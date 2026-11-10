# T06-A — Mobile readiness and iOS handoff

Updated 2026-11-10 (Malaysia). Baseline commit: `38f6ba4`.

## Scope and evidence labels

T03/T04 and T05 core/same-PC Relay acceptance are **reported complete by the user**. This task prepares the existing prototype for mobile; it does not implement host migration or automatic reconnection. Native compilation, signing, installation and physical-device testing are separate from Unity compilation and Xcode project export.

## Implementation and files

- `Assets/prototype/Scripts/Network/PrototypeNetworkHud.cs`: reuses the existing room controls; 1100-unit panel, configurable `touchHeight` default 120, connected-room compact layout, top-row invite entry/Join while typing. Standard ASCII-capable, single-line keyboard avoids autocorrect. Direct Test mode switch is Editor/Development only. Existing Copy Code and visible code/manual entry remain.
- `Assets/prototype/Scripts/Prototype/PrototypeSafeArea.cs`: existing screen-safe-area anchors still update on safe-area/resolution changes; mobile-only configurable 30 FPS target. Pause/focus loss cancels touchscreen and on-screen virtual controls; desktop target rate is unchanged. The keyboard does not move the joystick or Interact.
- `PrototypeLocalInput.cs`: explicit pause guard, immediate zero movement, neutral-input requirement on return. Each local input still owns its cloned actions.
- `PrototypeOrbitDrag.cs`: invite/address text editing clears/blocks rotation and zoom; existing pointer capture, touch IDs, pinch baselines and focus/pause cleanup remain.
- `PrototypeNetworkSession.cs` and `PrototypeOnlineRoom.cs`: on foreground resume, confirmed-invalid Direct/Session/NGO state uses existing cleanup. Desktop focus loss does not disconnect. A stale transport connection is still detected by the existing 30-second transport timeout; opening operations retain the 30-second bound. iOS background networking is not promised.
- `Assets/prototype/Editor/PrototypeIosBuild.cs`: `Tools > Two Tails > Export iOS Prototype`; exports the existing network scene, restores previous active platform and preloaded-assets state afterward.
- `Assets/Settings/Build Profiles/iOS Prototype.asset` plus `.meta`: independent iOS Development profile, one startup scene `Assets/prototype/Scenes/Prototype_Network.unity`, Deep Profiling and auto-connect Profiler disabled. Existing Windows profiles retained.
- `ProjectSettings/ProjectSettings.asset`: landscape autorotation Left/Right only; iOS IL2CPP/ARM64. No package, Cloud Project, Environment, scene or prefab GUID changes.
- `Docs/PROJECT_STATUS.md` and this document: evidence and handoff.

Runtime layout changes apply through the existing HUD component, so no scene rebuild menu is required. Existing Canvas Scaler is Scale With Screen Size, 1920×1080, match 0.5. Existing Interact and joystick remain in opposite bottom safe-area corners. No duplicate Canvas, camera, EventSystem, input or movement system was added.

## Input ownership review

The camera background only receives presses not claimed by foreground raycast targets. InputSystemUIInputModule retains multi-touch pointer behavior. A UI-origin drag remains captured by its originating control; it cannot become a background camera press after leaving its rectangle. Camera pinch only uses its own background touch list, so a joystick finger is not included. Single/pinch transitions rebuild baselines. No PlayerInput automatic control-scheme switching is used; scene OnScreenStick uses normal UI event routing (`useIsolatedInputActions = false`). Virtual Gamepad changes therefore do not switch action maps. Actual simultaneous fingers remain a device test.

## Build configuration

| Item | Observed value |
| --- | --- |
| Editor | 6000.6.8f1 (237b6593792b) |
| Input System / uGUI | 1.20.1 / 2.6.0 |
| NGO / Transport | 2.13.3 / 6.6.0 |
| Multiplayer Services / Play Mode | 2.4.1 / 3.0.0 |
| Services Core / Authentication | 1.18.0 / 3.8.0 |
| iOS Build Support | Installed; live BuildPipeline reports supported |
| Minimum iOS | Existing 15.0 retained |
| Bundle Identifier | Existing `com.Unity-Technologies.com.unity.template.urp-blank` retained |
| Build version | Existing 0.1.0 retained |
| Scripting / architecture | IL2CPP / ARM64, device (iphoneos) |
| Orientation | Landscape Left and Right; no portrait |
| Performance | Mobile target 30 FPS, Inspector configurable; desktop unchanged |

The existing template Bundle Identifier needs signing-team review before installation. It was not silently changed. Product Name, stripping settings and existing graphics quality are retained. Live scene inspection found one Directional Light and one Camera; no extra real-time lights or post-processing were introduced. This is preparation, not a measured mobile performance result.

Unity 6.6 documents iOS 15+ support: [system requirements](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements). Unity generates an Xcode project, while Xcode performs the native app build: [iOS setup](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/iphone/getting-started/ios-environment-setup). Confirm the borrowed Mac/Xcode combination against [Apple Xcode support](https://developer.apple.com/xcode/system-requirements/), particularly device support for the reported iOS 26.5.

## Executed checks

- **Verified:** C# compilation, zero project compile errors/warnings in the final pre-export recompile.
- **Verified:** running network scene has zero Missing Scripts; one Camera was observed. Play Mode frame counts advanced.
- **Verified:** 1920×1080, 2340×1080 (19.5:9), 2400×1080 (20:9), and 1600×1200 (4:3) menu captures inspected. Actual wide/4:3 Game View sizes were selected on Windows target; an initial iOS portrait-preview capture was rejected and replaced. This does not validate real notches.
- **Verified:** focused invite field leaves only Join active among room buttons; panel height becomes 144 reference units. This tests layout, not an iOS soft keyboard.
- **Verified:** connected 4:3 layout keeps room menu, joystick and Interact separate. Synthetic safe-area insets were applied in Play Mode; container shrank to 1708.8 units at 1920 width, with the 800-unit Direct panel inside. Actual `Screen.safeArea` on an iPhone remains pending.
- **Verified:** Direct Host starts with exactly one local player. Relay creates a room and reaches Connected with one player using `RelayUnityTransport`; leaving returns to Ready. Direct Host can start after Relay cleanup. No second-instance multiplayer acceptance is inferred from these smoke tests.
- **Verified (component probes):** pause clears motor input; pinch entry does not change distance; outward pinch decreases desired distance; pause clears tracked camera pointers. A virtual stick value was set and the pause reset returned it to zero. These are synthetic probes, not physical multi-touch tests.
- **Verified:** exported Info.plist contains only the two landscape orientations. Xcode project targets iphoneos/arm64 with deployment target 15.0. No microphone/camera/photo usage permission or blanket ATS bypass was added.
- **Pending:** complete two-instance T03/T04 regression after these changes, real keyboard/mouse interaction, physical network interruption and all device tests below.

Screenshots/probe scratch files are under ignored `Temp/T06_*`; they are not source assets.

## Export result and repeat steps

The final export reported `Succeeded, errors: 0`; the active target/profile returned to StandaloneWindows64 / Windows Network Prototype, with the network scene clean and Play Mode stopped. Windows successfully exported an Xcode project to `Builds/iOS/Prototype/Unity-iPhone.xcodeproj`. The output directory is Git-ignored. This is **Xcode project export success**, not an iOS App build or installation. No native Apple toolchain was run on Windows.

1. Exit Play Mode and save intentional scene edits.
2. Run `Tools > Two Tails > Export iOS Prototype`. The menu uses Development Build, exports the existing network scene and restores the preceding build target.
3. Alternatively open Build Profiles, choose `iOS Prototype`, verify its scene list, select Development Build without Deep Profiling, and Build to `Builds/iOS/Prototype/`.
4. Send/archive the **entire exported folder**, including Classes, Data, Libraries, Frameworks, Il2CppOutputProject and the Xcode project. Do not send only the `.xcodeproj` package.

## Device information and Mac handoff

**Reported by the user:** iPhone 15 Pro, iOS 26.5. No connected-device verification has occurred. No Mac is currently available.

1. Borrow a Mac with an Xcode release supporting that device OS; confirm macOS/Xcode compatibility before setup. Do not purchase cloud builds or membership as part of this task.
2. Copy the full export. Open `Unity-iPhone.xcodeproj` (or an existing workspace if a future dependency generates one).
3. Select the iPhone device and an authorized signing team. Review the retained template Bundle Identifier; authorize a unique identifier if signing requires one. Configure signing locally, never commit certificates, private keys, profiles or account credentials.
4. Connect/unlock/trust the phone and enable Developer Mode if Xcode requests it. Build the device target, record native compiler/linker errors separately from Unity export results, then install/run.
5. Confirm portrait is unavailable and both landscape orientations work. Test notch/Home Indicator spacing in both orientations, invite field keyboard/Join visibility, manual code entry and Copy Code.
6. Test joystick + camera drag, joystick + Interact, joystick + two other camera pinch fingers; UI-origin fingers never rotate/pinch. Verify pinch transition baselines, cancel/lift, focus and lock-screen cleanup.
7. Test iPhone Host and Client with a second independent identity. Repeat T03/T04 movement, facing, Lever, two-player plate gate, anti-crush and late join over Relay. Test a different network separately; same-PC Relay is not that test.
8. Lock/background each role, resume, check actual connection and recoverable UI on loss. No automatic host migration or seamless reconnection is expected.
9. Measure sustained FPS, memory and thermal behavior on device with profiling tools. The configured 30 FPS target is not a performance measurement.

## Remaining acceptance checklist

- [ ] Native Xcode compilation/linking and signing.
- [ ] iPhone installation and both landscape safe areas.
- [ ] Physical multi-touch, keyboard/autocorrect behavior and clipboard.
- [ ] Mobile Host/Client and existing puzzle regressions.
- [ ] Background/lock/resume with surviving and lost connections.
- [ ] Different-network Relay test.
- [ ] Sustained frame rate, temperature and memory.

No commit/push was performed. Suggested commit: `feat: prepare prototype mobile input and iOS development export`.
