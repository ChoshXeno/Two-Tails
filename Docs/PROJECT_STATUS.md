# Two Tails project status

Last updated November 10, 2026 (Malaysia time).

## Goals and development environment

Two Tails is a mobile two-player cooperative animal puzzle game. Players combine complementary animal abilities to solve environmental puzzles and rescue wildlife. The [Project Roadmap](https://app.notion.com/p/4120be84637f81958a18f28aabcaf369) describes the prototype scope and task order. The [Agent Operating Guide](https://app.notion.com/p/4120be84637f8140aa04c1aea2b376a7) describes where to save project decisions and development journals.

- **Verified in the repository.** Unity `6000.6.8f1`, revision `237b6593792b`, in `ProjectSettings/ProjectVersion.txt`. The project uses Universal RP.
- **Reported by the user.** Development on Windows 11 with JetBrains Rider. The available test phone is an iPhone. The Rider package is verified below. Rider configuration and the iPhone deployment path have yet to be verified.

### Package inventory

Rechecked `Packages/manifest.json` and `Packages/packages-lock.json` on November 10, 2026; no packages changed. Transport, Services Core, and Authentication resolve transitively.

| Package | Status |
| --- | --- |
| Input System (`com.unity.inputsystem`) | Installed `1.20.1` |
| Universal RP (`com.unity.render-pipelines.universal`) | Installed `17.6.0` |
| JetBrains Rider integration (`com.unity.ide.rider`) | Installed `3.1.0` |
| Unity Pipeline (`com.unity.pipeline`) | Installed `0.10.0-exp.1` |
| Netcode for GameObjects (`com.unity.netcode.gameobjects`) | Installed `2.13.3` |
| Unity Transport (`com.unity.transport`) | Resolved `6.6.0` |
| Multiplayer Services (`com.unity.services.multiplayer`) | Installed `2.4.1` |
| Services Core (`com.unity.services.core`) | Resolved `1.18.0` |
| Authentication (`com.unity.services.authentication`) | Resolved `3.8.0` |
| Multiplayer Play Mode (`com.unity.multiplayer.playmode`) | Installed `3.0.0` |

## Completed work and evidence

**T01 — Project foundation: reported complete by the user.**

- **Implemented in the repository.** The Unity project, URP settings, initial scene, and Git history are present. `ProjectSettings/EditorBuildSettings.asset` includes `Assets/Scenes/SampleScene.unity` as an enabled scene.
- **Verified in the November 8 live Editor check.** Unity Pipeline returned `SampleScene` as the active scene, with `Main Camera`, `Directional Light`, and `Global Volume` as roots. The Editor was idle, reported no script compilation failure, and showed zero current Console errors.
- **Verified in the Editor log.** `Logs/Editor.log` recorded a successful Windows player build at 10:02 PM Malaysia time on November 8. Launching that build and opening a Unity script in Rider were not independently tested during these checks.
- **Git reference.** Local `main` and the cached `origin/main` reference both pointed to `f2640f5` (`Add: Added folders for organization`). The remote was not queried.

## Current task and outstanding work

**Current: T07-A developer split-screen and friends test package.** Engineering changes and Windows Xcode export are complete within the checks recorded below; native installation, physical input/network tests and mobile performance remain pending.

**Resolved T04 localhost connection follow-up (November 9, 2026).** The Windows player log identified the T03 executable, and the Editor Host logged `NetworkConfig mismatch`. The old network scene disabled NGO scene management and the current T04 scene enables it. A matching independent T04 Windows QA client connected to the live Editor Host on `127.0.0.1:7777`; both processes reported two players. The current T04 Windows player was opened for manual use. See [T04 network puzzles](T04_Network_Puzzles.md) for the executable path and rebuild steps.

**T02 — Local greybox.** Reported complete by the user. The scene has independent lever and two-plate doors, a P1 orbit and zoom camera, an English HUD, and an Editor/Development-only P2 test character. See [Prototype Greybox](Prototype_Greybox.md) for configuration, asset conventions, and acceptance checks.

- **Implemented.** The lever toggles its door and the plates use distinct-character AND occupancy to control a separate door. Both doors reverse smoothly. The camera supports wheel and pinch zoom without changing FOV.
- **Verified.** Current Play Mode probes passed the plate truth table and occupancy cleanup, lever input and reversal, independent doors, panel collider recovery, and desktop wheel zoom with UI isolation and obstruction recovery. A prior non-development Windows player launched the prototype with only one active character and no active P2.
- **Awaiting testing on a physical device.** Confirm the iPhone build, signing, and installation path. Verify pinch, simultaneous touches, the safe area, and background/resume.
- The prototype uses installed Input System `1.20.1` and uGUI `2.6.0`.

**T03 — Local Host/Client movement.** Reported fully accepted by the user. NGO automatically spawns one player per approved connection. Owners drive movement, the root position and visual yaw replicate, and the server assigns both player slots. See [T03 local networking](T03_Network.md) for setup and validation.

- **Verified.** Editor Host and independent headless Windows Development Players passed three connection, disconnect, and rejoin cycles. Both owners' movement and visible facing replicated. Late-join position, color, independent camera state, capacity rejection, wrong-port recovery, and local greybox regression passed.
- **Historical evidence scope.** The recorded T03 automation used headless Windows Development Players. It did not independently test physical-device touch.

**T04 — Network puzzles.** Reported complete by the user. [T04 network puzzles](T04_Network_Puzzles.md) records the implementation and acceptance tests. The independent Lever and two-plate doors use server-write state. A server-only trigger proxy lets the Host detect the remote player's plate and passage occupancy.

- **Verified.** Editor Host and independent headless Windows Development Client passed the Lever and two-plate tests, including rejection/cooldown, late join, disconnection cleanup, three rejoin rounds, and synchronized anti-crush pause/resume. Editor input and local Lever regressions passed.
- **Awaiting interactive and physical-device testing.** Two visible windows, touch controls, and a join during an in-progress door transition remain manual checks.

**T05 — Online Relay rooms.** Implemented in the network prototype. Online Relay creates private two-player Sessions and joins by Session code. It uses separate cached test identities, gates the gameplay UI on a connected local NGO player, and shares the existing player and puzzle authority. Direct Test remains available from the idle menu. See [T05 online rooms](T05_Online_Rooms.md).

- **Verified in the live Editor.** The existing Two Tails cloud project initialized against `production`, signed in anonymously, created a room with a code and one NGO player using `RelayUnityTransport`, and returned to Ready after deleting the room.
- **Verified with independent processes on one PC.** The Editor and a headless Windows Development Player completed three two-player Relay rounds with distinct identities. Copy, bad and deleted codes, capacity, rejoin, Host process loss, cancellation, and Direct after Relay passed. Scripted poses verified owner replication, both Lever owners, the two-plate gate, passage blocking, occupancy cleanup, and late-join door state.
- **Verified delivery build.** `Builds/T05_Windows/TwoTailsOnline.exe` was built from Windows Network Prototype without the temporary probe, with zero errors and warnings. The online scene has zero Missing Scripts.
- **Awaiting interactive and physical-device testing.** Repeat the input and camera checks in two visible windows. Test physical network interruption and two separate networks. iPhone deployment remains pending.

## T06-A — Mobile readiness

**Implemented:** existing room UI enlarged for touch; input editing isolates movement/camera, pause clears local input and virtual controls, foreground checks reuse existing network cleanup. Landscape-only IL2CPP/ARM64 iOS Development profile and export menu added. Mobile FPS target is 30; desktop configuration retained. See [mobile readiness and Mac handoff](T06_Mobile_Readiness.md).

**Verified:** pre-export C# compilation had zero errors/warnings; live scene had no Missing Scripts. Four aspect layouts, compact input layout, synthetic pause/pinch/reset probes, Direct Host and Relay create/leave/Direct smoke tests passed within the documented scope. Windows exported an Xcode project to ignored `Builds/iOS/Prototype`. Export is not native compilation or installation.

**Reported by the user:** iPhone 15 Pro running iOS 26.5; no Mac. T03/T04 and T05 core/same-PC Relay have user acceptance. **Awaiting testing on a physical device:** iOS native build/install, multi-touch, keyboard, clipboard, background networking, cross-network and performance. Baseline commit `38f6ba4`; T06 changes are uncommitted.

## Next steps and acceptance criteria

1. Follow [T06 mobile readiness](T06_Mobile_Readiness.md) for Mac/Xcode signing and iPhone checks.
2. Run remaining physical-device and interactive checks in [T04 network puzzles](T04_Network_Puzzles.md).
3. Run the remaining T05 interactive and cross-network checks with the same Windows Development Build on both machines. iPhone installation and physical input validation remain separate.

## T07-A delivery (2026-11-10)

Implemented separate MainMenu, SplitScreen and OnlineTest scenes, a Windows Friends Test profile, development-only offline entry, two independent cameras and per-viewport HUD, reserved Jump bindings, and cleanup back to menu. Baseline scenes, network ownership and puzzle validation code remain unchanged. See [friends test instructions](T07_Friends_Test.md).

Verified: zero-error/warning compile, successful Windows x64 Development Build; initial headless executable startup log contained no exception; final build was regenerated after the camera adapter adjustment (not an interactive graphical test). Live Editor measured two actors, 50/50 camera rectangles, preserved 54-degree FOV, one AudioListener, no network manager in Local. W/arrow state injection isolated motors. Distinct actors on separate plates opened the door; P1 and P2 validated Lever interactions succeeded. P2 camera probe rotated only P2 after explicitly resuming the background-paused Editor. Local -> menu -> Relay Connected (one player) -> menu -> Local completed, then another local re-entry kept two actors/one listener/zero Missing Scripts. No UGS/Relay operation is called by Local, but no physical internet-disconnection test was performed.

Distribution: `Builds/TwoTails_Friends_T07A_Windows.zip`, full runnable dependencies and README; private cache folders and Unity DontShip backup omitted. Not sent to anyone.

Pending: interactive mouse/resize and keyboard rollover, physical P2 interaction-key input and full plate truth table regression, actual non-development executable entry denial, two-instance Relay join/rejoin/puzzle regression on this build, different-network friends acceptance, Android physical device tests. Historical T03-T05 results do not substitute for these checks. No jump/climb/night vision/new puzzles implemented. No commit/push.

Follow-up verification (2026-11-10): final rebuilt Windows executable launched headless; startup log had no exception. Main Menu screenshot inspected in Editor: Local Play, Online Play, Quit and version label visible without overlap. This does not verify interactive executable input or two-instance networking. Test processes stopped; Editor left out of Play Mode.

## T07-B (2026-11-10)

T07-A is now **reported complete by the user**. **Implemented:** Fox/Monkey identity (Weight 2), server-slot identity assignment, jump inputs/mobile button, shared Motor jump/climb, explicit vine surface, test geometry, optional motion events and network presentation state. **Verified:** compile and Windows build; local component edge-case probes and injected Space/Enter callbacks; independent Windows Client Monkey climb observed on Editor Host. See [animal ability evidence](T07_Animal_Abilities.md). **Pending:** complete interactive/Relay regression, friends across networks and Android physical-device tests. Existing owner movement authority is retained; no movement anti-cheat claim. No commit/push.

## T07-C (2026-11-11)

**Implemented:** per-follow-camera Fox/Monkey world-space night visibility in SplitScreen and OnlineTest, ordinary shadowed spot lighting, development-only local daylight toggle, suspended vine with airborne catch, and corrected online touch Jump creation/binding. Baseline scenes and package versions retained. T07-B's previous mobile-button claim was not supported by the actual OnlineTest hierarchy; corrected in this task.

**Verified:** Editor night/day same-position screenshots; ascent/descent climb component probes clear vertical speed; ground/Fox/back/side rejection and reset; saved scenes have zero Missing Scripts. Windows Development build succeeded. Independent headless Client jumped/caught/climbed to y=3.93 and Host observed its final position; this was Direct, not Relay. Build has an existing DEVELOPMENT_BUILD deprecation warning; Client logged Unity diagnostic endpoint timeouts. See [T07-C setup and evidence](T07_Night_Vision.md).

**Pending:** full human-control regression, rendered two-window identity/night comparison, Relay/cross-network regression, actual non-development executable check, Android/iPhone rendering and multi-touch. Future opposed tablet layout is recorded only. No commit/push. Next: manually accept the T07-C checklist and physical touch behavior; do not treat component probes as device acceptance.

## T07-C supplement / T07-D / T07-E (2026-11-11)

**Implemented:** URP Lit/PBR night pass with low cool shape-dependent fill, independent development Fox A/B and local daylight; three cooperative areas in actual SplitScreen/OnlineTest entrances. Switchback and Bridge lock after distinct-player handoff. Counterweight uses explicit 2/1/1 crates, controlled pushing/riding and 4/4 lock. Shared offline/server rule engine plus persistent network snapshots; independent reset tiles. Existing baseline scenes/inputs/abilities retained; no version upgrade or commit/push.

**Verified:** zero-error compile; matched-view Editor night A/B screenshots; automatic actual-controller traversal of both relay routes; interrupt/reset/retry; crate pushing, weight imbalance, passenger movement and balance lock. Gate anti-crush and per-area reset isolation. Existing Lever/AND/jump/air-catch/pause/exit regression and two cameras/one listener. Windows Development build succeeded. Two independent Client runs through real Sessions Relay completed all three key Client operations, with Host Fox/Client Monkey then swapped identities. Developer light states remained process-local. Disconnect timeout cleaned occupancy/attempts, new room/rejoin kept two actors, normal Host leave returned Ready/idle.

**Pending / awaiting physical device testing:** graphical Windows executable automation was blocked by Computer Use app-approval timeout; headless results are not graphical acceptance. Different-network friends tests, Android touch/performance, rendered swapped-role Client view, full late-join/reset matrix and exhaustive lag/crush cases remain pending. Mobile URP additional-light shadows are disabled; desktop visual evidence does not validate mobile parity. See [cooperative tests and exact evidence](T07_Cooperative_Tests.md) and [lighting diagnosis](T07_Night_Vision.md). Next: manual graphical and physical-device acceptance, using the final complete Builds/CooperativeTest folder.

Final evidence: final Windows rebuild after label-spacing changes succeeded; current running SplitScreen audit found 0 Missing Scripts, 0 broken serialized references, 0 unsupported materials and exactly 3 puzzle controllers. Editor was left in the observed Play Mode state to avoid interrupting possible user testing. Base commit d289c71; no new commit. Decision journal: https://app.notion.com/p/4140be84637f81219726d6a77589e090
