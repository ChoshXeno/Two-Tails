# Two Tails project status

Last updated November 23, 2026 (Malaysia time).

## Goals and development environment

Two Tails is a mobile two-player cooperative animal puzzle game. Players combine complementary animal abilities to solve environmental puzzles and rescue wildlife. The [Project Roadmap](https://app.notion.com/p/4120be84637f81958a18f28aabcaf369) describes the prototype scope and task order. The [Agent Operating Guide](https://app.notion.com/p/4120be84637f8140aa04c1aea2b376a7) describes where to save project decisions and development journals.

- **Verified in the repository.** Unity `6000.6.8f1`, revision `237b6593792b`, in `ProjectSettings/ProjectVersion.txt`. The project uses Universal RP.
- **Reported by the user.** Development on Windows 11 with JetBrains Rider. The available test phone is an iPhone. The Rider package is verified below. Rider configuration and the iPhone deployment path have yet to be verified.

### Package inventory

Rechecked `Packages/manifest.json` and `Packages/packages-lock.json` on November 14, 2026; no packages changed. Transport, Services Core, and Authentication resolve transitively.

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

**Current: animal mechanics pass (2026-11-14).** Implemented the requested visibility/UI corrections, walkable animal exchanges, Otter swimming and Beaver dam test areas. T07-D expansion is paused and T08 is deferred. See [animal mechanics and evidence](Animal_Mechanics.md) for verified scope and remaining acceptance.

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


## Animal mechanics pass — 2026-11-14

**Implemented:** strict Monkey PBR/transparent/world-label masking with greybox near-view occlusion; Fox-only brightness/gamma; corrected Jump/Interact placement and contextual Dive/Surface; server-validated lobby animal/position exchange; Otter swimming; Beaver unique-log inventory and dam; authoritative rising water; ordinary pool and ramp reservoir; synchronized developer reset. Existing scene/prefab/action GUIDs and the user's uncommitted NetworkSession identity code were preserved. Unity and package versions unchanged. Base commit inspected: `e55d69f`; no commit/push.

**Verified:** final compile and Windows Development Build; both saved scenes zero Missing Scripts/broken references; controlled Fox brightness change produced zero pixel difference in Monkey's matched world crop. Local swap/concurrency/inventory protection/swim probes passed. An independent Windows Client over Sessions + Relay completed the actual collect/deposit/raise-water/swim/exit path, normal Leave/Join persistence, repeated Client resets and unspent-log return. Existing network Lever/AND gate and local Jump/airborne Monkey catch probes passed. Final local menu re-entry had 2 actors, 2 cameras, 1 listener, fresh Console 0 errors/warnings. Detailed evidence and its limits are in [Animal_Mechanics.md](Animal_Mechanics.md).

**Known limits:** the automated Client was headless, not a rendered two-window graphics test. Two earlier auto-positioned swap-back requests were rejected; focused and subsequent complete runs passed, but the earlier rejection cause was not captured. Abrupt Client process termination can leave cloud membership temporarily conflicting (`SessionConflict`); normal Leave/Join passed. Mobile URP additional-light shadows differ from the PC profile. Owner-authoritative movement and BoxCollider-based near-view occlusion retain their documented limitations.

**Awaiting testing on a physical device:** Android/iPhone multi-touch, graphics/shadow parity, performance and real different-network play. **Next acceptance:** interactive controls/UI, rendered online views after swaps, all-angle blackout/wall occlusion, simultaneous peer exchanges under latency. Continue animal mechanism acceptance; do not resume T07-D expansion or T08 without a new request.

Build: `Builds/FriendsTest/TwoTails.exe` plus its complete sibling folder. Editor left at `Prototype_MainMenu`, outside Play Mode; QA processes and rooms closed.

## Near-view rendering follow-up (2026-11-18)

Implemented an independent fixed diffuse near reveal for ordinary animals. The original player-origin visibility rays used every nearby solid BoxCollider and therefore projected moving occlusion silhouettes, even without a player light. The prototype now restricts the ray blockers to wall, divider and door/gate objects using the current scene naming convention. Ordinary props still use camera depth and real light shadows. New room barriers must follow that convention or receive explicit blocker authoring in a future pass. The ordinary-animal PBR scene light contribution no longer scales with the player radius.

Verified: source inspection and git diff whitespace check. Unity CLI reported the Editor unreachable. Unity 6000.6.8f1 batch startup exited 1 before import with a refused licensing IPC connection and an ERROR_SUCCESS assertion (Logs/TweaksVerify.log). Shader compilation and rendered Local/Online acceptance remain pending. Contextual Beaver HUD, per-slot build/reclaim and reversible water are not present in this checkout. The untracked ACTIVE_TASK.md checklist describes work that could not be confirmed in the current source tree.

## Beaver and near-view tweaks — 2026-11-18

**Reported complete by the user:** the pre-tweak blackout and local lighting, lobby exchange, and Beaver pickup/deposit baseline. Rabbit and Elephant follow this acceptance round.

**Implemented:** retained the pending fixed diffuse near reveal and wall-only occlusion correction. Added per-view Beaver inventory, contextual Pickup and Build/Surface, persistent slot-to-log IDs, ordered build/reclaim, runtime depth-tested outlines and reversible authoritative water. The shared test dam now sits on the dry apron. Existing GUIDs were retained. Base 8c6a1b2.

**Verified after the earlier blocked startup:** live Editor shader inspection and fixed-camera rendered reveal comparison, local Play Mode swap and complete dam loop, online HUD and active-swimmer reset in an Editor Host, and successful Windows Development Build. An independent matching Windows Client over Direct completed the dam loop and duplicate-request checks. Host observed the same completed state. A fresh Client received the retained result after the old slot released. See [November 18 evidence and Inspector configuration](Animal_Mechanics.md#november-18--beaver-and-ordinary-animal-tweaks).

**Next acceptance:** interactive controls and rendered online view, Fox and wall/transparent occlusion at additional angles, Relay and different networks. **Awaiting testing on a physical device:** mobile touch and performance.

## Night lighting follow-up — 2026-11-18

**Implemented:** in `PrototypeNightEnvironment`, night mode temporarily disables realtime shadows on the scene lights and the PC URP Screen Space Ambient Occlusion feature. Developer Lights and component teardown restore their captured original modes. No scene or shared Renderer asset was edited. `Local Light Fade Width` (default 0.2) now smooths the visibility mask used by opaque/transparent night rendering. The existing camera-local near reveal, local lights, and Fox controls remain.

**Cause verified:** the loaded SplitScreen scene had eight realtime `Soft` shadow lights (including the Lever, dam, pool, route and lit-area lamps); the directional daylight was also `Soft` but is disabled at night. The scene has a null LightingDataAsset, ruling out assigned baked lightmaps. PC Renderer has an active SSAO feature (intensity 0.4, radius 0.3), explaining additional contact darkening. Live object-ray blocker filtering only handles the player-centered reveal and was not the floor projection source.

**Verified in the live Editor:** successful Unity script compile with 0 errors and two pre-existing `UAC0009` probe-script warnings; fresh Console after tests had 0 errors/0 warnings. Runtime reflection confirmed all eight night shadow modes `None`, PC SSAO inactive, Developer Lights restoration to `Soft`/active, and reapplication on return to night. Captured Game View with the LeverDoor open and closed; the moving panel collider stayed enabled. Existing Beaver flow was exercised through its handler: collected 3 logs, built all 3 slots, reclaimed the middle slot and restored its outline, observed water target reverse from 0 to -4, then rebuilt and restored target 0. Play Mode state was discarded; Editor returned to `Prototype_MainMenu`. See [night lighting evidence and settings](Animal_Mechanics.md#november-18--night-shadow-and-light-edge-follow-up).

**Awaiting user validation:** walking around a fixed object while observing both live split views, multiple local-light edge positions, rendered Online/Relay view isolation, a Windows Development player, and mobile URP parity. No physical-device, Relay or player-build visual test ran in this task. No commit/push.

## Rabbit and Elephant — 2026-11-19

**Implemented:** Rabbit collider and two-jump movement, Elephant weight, guided push and Strike, four lobby displays, replicated and resettable course state, and shared graybox geometry in SplitScreen and OnlineTest. See Animal_Mechanics.md for dimensions, controls and entrance.

**Verified:** cached Roslyn source compilation and analytical jump/geometry checks. **Pending:** full Unity import and NGO postprocessing, Play Mode measurement and gameplay regression, and Host/Client validation. Pipeline could not reach the Editor and batch mode stopped before import (Logs/RabbitElephantCompile.log). T07-D and T08 remain paused.

### Saved course and HUD lifecycle

Both prototype scenes now contain the two display objects and the course in Edit Mode. Animal Mechanisms binds serialized scene references, uses the Heavy Gap Block's saved position as the reset origin and keeps its existing network snapshot for offset and board state. The historical runtime builder remains available as fallback.

The local historical Editor log records PrototypeNetworkPuzzleHud.Update (old line 32), AnimalMechanicsActor.HasStrikeTarget (21), AnimalMechanicsWorld.FindTarget (121), then Component.transform on a destroyed actor. The HUD now clears its player bindings when there is no spawned local player and when disabled, and respects Unity destroyed-object checks. The exact historical trigger sequence was not reproduced.

Verified through the live Editor: saved and reopened both scenes and found all three children. In SplitScreen Play Mode there was exactly one of each; invoked Reset and requested exit. The Pipeline connection dropped during the subsequent online verification attempt. Online Host/Client, full animal gameplay checks and repeated exception regression remain pending.

## Elephant free push follow-up — 2026-11-19

Implemented a separately authored free push test block in both prototype scenes. The guided gap block remains available. The new kinematic Rigidbody and BoxCollider allow Elephant-only movement with an individual delayed fall respawn. Dam slot outlines now render white. Unity script compile succeeded with no errors and Edit Mode checks found one test block in each scene. Physical play and online synchronization are pending. Details and Inspector settings are in Docs/Animal_Mechanics.md.

## Elephant push corrections — 2026-11-22

Implemented bidirectional gap-block pushes, actual support sensing, falling and individual respawn in the authoritative Animal Mechanics snapshot. Replaced the free block's flush-ground SweepTest support probe with a downward ground ray. Unity scripts compiled with zero errors. Physics acceptance and Host/Client synchronization remain unverified. See Docs/Animal_Mechanics.md for root causes and parameters.

## Night reveal barriers — 2026-11-23

The old near reveal classified walls, dividers and named doors and gates by object name. On OnlineTest in Play Mode with Developer Lights off, a ray against the actual Vine Wall hit from one test position and missed from the other, for the same ground point at respective distances 1.20 m and 0.50 m. Both were inside the 3 m clear radius. The old shader returned zero reveal on the hit. The updated environment uses only explicitly assigned puzzle barriers and the observed scene reported zero authored and rendered blockers. Existing camera collision, local light terms, Fox vision and developer toggles retain their code paths. The runtime comparison was a ray and shader input check. Rendered screenshots, camera rotation, two-instance and split-screen visual acceptance remain pending.

## Monkey rope course — 2026-11-23

Implemented Monkey air grab, climbing, pendulum movement and momentum release through the existing local motor. Both saved prototype scenes have a climb rope and a two-rope course with safety floor. Online ropes use server-written swing state. Updated contextual HUD and Otter action order. Unity compile, local species/rope/transfer probes and Direct Host contextual labels were observed. Human route, device input, and independent Client remain pending. See Docs/Animal_Mechanics.md.

## Monkey lobby display — 2026-11-23

Implemented a saved fifth lobby display in both OnlineTest and SplitScreen. The networked animal snapshot now includes its species, position and reservation. The Host keeps player animal identities unique: a solo Fox can exchange for Monkey, while a later joiner is assigned Fox; an exchange into an animal held by the other player is rejected. Unity compilation passed with zero errors, and the saved display and scene references were observed in both scenes. The Play Mode scene loaded with two actors, but the unfocused Editor did not advance beyond frame 2. Host-alone Interact, rope use and two-instance join-order acceptance remain to be tested interactively. See Docs/Animal_Mechanics.md.

## Monkey vine, rope and weak moonlight — 2026-11-23

Implemented horizontal and diagonal vine climbing, scaled wall-edge limits and Jump release; an eight-link rope with a simulated free tail and server-written link snapshot; and a scene-saved weak moonlight plus solid roof test in both Prototype entries. The moon alone retains night shadow maps, and ordinary-animal far visibility uses its shadowed main-light contribution. Unity compilation and saved-scene Missing Script checks passed. Controlled Play Mode component probes observed lateral/diagonal climbing and Jump release, stronger timed rope oscillation than continuous input, transient tail bending and low link-length error. An exposed moon ray was clear while the cave-center ray hit its roof. Direct OnlineTest Host started with one player and all three rope NetworkObjects spawned; no current Console errors appeared. Interactive movement, rendered night views, two-instance Client parity, Windows build and device input remain pending. See Docs/Animal_Mechanics.md.
## Strict night visibility envelope — 2026-11-23

Implemented a shader-only correction: Fox no longer bypasses the distance envelope, and moon/local lights cannot reveal opaque or transparent world content beyond its fade end. Existing range parameters and moon/roof lighting configuration retained. Live Editor shader refresh reported zero shader messages and current Console zero errors. Rendered screenshot-route, split-screen and Host/Client acceptance remain pending; see Animal_Mechanics.md. No commit or push.
## Fox night vision regression fix — 2026-11-23

Restored full-view visibility only for the current camera's active Fox night vision; ordinary animals and Fox A/B-off keep strict distance masking. This supersedes the prior strict-distance requirement for Fox. Live split-camera bindings and A/B restoration were observed, both shader imports had zero diagnostics and current Console zero errors. Rendered dark-region comparison, swapped identities and independent Online views remain pending. No lighting settings changed; no commit/push.
## Distant local guide lights — 2026-11-23

Implemented additional-light-only far reveal for opaque surfaces; moon excluded from that exception. Transparent labels, water and dam outlines remain distance limited. Runtime Point/Spot sources receive a small depth-tested Unlit bulb tied to enabled state and intensity. Fox and Developer bypass remain. Unity C# compilation succeeded. Domain reload during the user's running Play Mode produced Input System assertions at PrototypeLocalInput.OnEnable line 45; interactive rendered checks were not completed. Local lights with shadows suppressed still have no wall-shadow rejection for their surface reveal: this remains an unresolved limitation requiring follow-up. Host/Client, split-screen visual acceptance and physical-device tests pending. No commit/push.
## Prototype hold-to-grip blocks — 2026-11-24

Saved Weight 2 and Weight 3 in Grip Block Test in Prototype_SplitScreen and Prototype_OnlineTest, at (17, 0.55, -10) and (20, 0.55, -10). Hold Interact facing the block and move toward or away from it. Release Interact to let go. Weight 2 accepts a non-Rabbit. Weight 3 requires Elephant or two eligible aligned moving players. The Host advances each block and replicates its position. The earlier gap course, free push block, and counterweight crates retain their original logic.

Verified Unity compilation and a scripted SplitScreen Play Mode check on Weight 3. Monkey alone produced no displacement and Elephant moved it 0.08 m in one step. Current Console reported zero errors. Pending interactive keyboard and touch, Weight 2 and Rabbit, two-player alignment and opposing input, release while moving, wall constraints, and independent Host/Client synchronization.
## Prototype migration audit — 2026-11-24

The user reported the current prototype mechanics accepted. See [migration audit](Prototype_Migration_Audit.md) and [actual Inspector inventory](Prototype_Inspector_Inventory.md). Unity inspected five scenes and two player prefabs with zero missing scripts, broken serialized Prototype references or repeated MonoBehaviour types on the same object. SplitScreen Play Mode confirmed separate Fox and Monkey camera targets. An unsaved runtime scene copy preserved the pool's shore reference and accepted a level change. Removed one unused shader helper and Unity reported zero diagnostics for both shaders. No scene or prefab edits. Independent Online Host/Client and visual regression remain unverified in this audit.
