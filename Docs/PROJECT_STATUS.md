# Two Tails project status

Last updated November 26, 2026 (Malaysia time).

## Animal night visibility registration — 2026-11-27

Fixed PrototypeNightEnvironment initialization to prepare MeshRenderer and SkinnedMeshRenderer, including inactive children. MonkeyPresentation registers all renderer descendants when a species visual is instantiated (player, remote presentation and lobby share this path). Runtime cloned night materials now preserve Base Map, tiling, offset and culling; PrototypeAnimalVision shader samples UV/Base Map. Previously skinned animals bypassed the night shader while ordinary MeshRenderer geometry used camera-specific visibility. No renderer enabled toggles, shared source material edits or layer changes were introduced; existing per-camera constants and visibility/light functions are reused.

Verified in OnlineTest Play Mode with Developer Lights=false and FoxNightVision=false: active skinned renderer used Two Tails/Prototype Animal Vision, layer 0/rendering layer 1. Controlled runtime preparation of all six prefabs retained their same-animal Base Maps and enabled renderers. C# compilation passed and ShaderUtil reported no shader errors. These are runtime binding checks, not screenshot/pixel acceptance. No interactive bright-dark-bright path, actual species exchange, Fox Vision rendered toggle, independent Host/Client or split-screen divergent-camera render acceptance completed. Next: complete those visual scenarios, including accessories and local-light occlusion. No commit/push.
## Monkey rope grip follow-up — 2026-11-27

Implemented runtime hand targeting while Monkey is on a rope. Previously Vine_Hang_Idle only supplied an authored pose with no continuous rope contact constraint. Presentation now aligns the visual to the rope tangent, moves only the visual so the mean hand position reaches RopeGripPoint, then solves both forelimbs toward adjacent grip points. The solver runs after the animation request in presentation, including climb-to-hang transitions; it stops automatically when IsOnRope clears. Player collision, rope momentum and release mechanics are unchanged. Unity compilation completed with no errors. Rendered swing transitions, palm orientation, collision clearance and Host/Client remain unverified. Next: exercise real grab/climb/swing/release and inspect side views.
## Elephant torso tuning — 2026-11-27

Updated Run, Interact, Jump_Start, Jump_Air, Jump_Land and Strike in place using AgentScripts/TuneElephantBody.cs. Added model-axis hips/chest/neck rotation and hips translation without scale curves. Run has 3.6 cm authored local heave and small pitch/roll; interaction and kick have weight shift; takeoff/landing have 5.5/6.5 cm local compression. Air stays at fixed height with light chest motion. Original gameplay movement and clip GUIDs retained. Unity authoring executed successfully; rendered gameplay and foot-contact acceptance remain pending. Body curves are rebuilt from rest each pass to avoid accumulating offsets.
## Elephant first animation pass — 2026-11-27

Implemented independent Elephant visual prefab, Generic Avatar, controller and 13 authored clips under Assets/Art/Finished Animation/Elephant. Only rest exists in Assets/Prefabs/Elephant.fbx; source FBX/importer untouched. Uses existing same-animal Preview_URP material and BakedMaterial.001_baseColor. Shared presentation supports Elephant player and lobby. Motor selects grip hold/step blends and successful local Strike requests the kick cue without delaying board destruction. Build script is AgentScripts/BuildElephant.cs.

Verified: Unity compilation completed without errors. Controlled Play Mode Animator selection returned all 11 requested states with Elephant-only clips, valid Avatar, root motion off and correct non-null Base Map; Push/Pull selected hold clips at zero GripSpeed. This is binding/playback verification, not visual acceptance. The normal runtime player probe did not produce a reliable sequence (species/state changed before follow-up sampling). Lobby exchange, live gait/jump/lever/Strike contact, pushing/pulling against actual boxes, death, skin floor/contact, scarf rendering and independent Host/Client remain unverified. Strike cue currently originates on authority; remote cue parity needs further work. Generated clips are first-pass candidates, not accepted finished animation despite requested delivery directory. Next: preview each pose, calibrate ground/contact, complete real gameplay and network checks before accepting delivery.
## Rabbit stable airborne follow-up — 2026-11-27

Restored Rabbit's intended state order: Jump_Start on takeoff, Jump_Air after the short takeoff window, and Jump_Start replayed once for an accepted second jump. Inspection of Rabbit_Jump_Air.anim confirms a 0.36 s rotation-only clip with Loop Time enabled. User specified keeping source frames 2–4 and looping that segment. The Editor is currently unreachable, so the clip has not yet been cropped or seam-checked. Completed follow-up: Unity API froze all 168 curves at source frame 3 (60 fps), holding the selected airborne pose in a 0.6 s loop with no animated vertical motion. GUID retained. Editor connection succeeded outside the sandbox; prior unreachable reports were sandbox-related. Rendered gameplay acceptance remains pending.

## Rabbit jump presentation — 2026-11-26

Rabbit briefly plays Jump_Start on takeoff, then returns to Jump_Air; an accepted second jump reports Rejump briefly and restarts Rabbit_Jump_Start once. Other species' jump routing is unchanged. The runtime selection was code-checked, but a live Play Mode jump trace has not yet been observed. See the 2026-11-27 follow-up for the current stable-air pose work.

## Rabbit Idle reference skeleton — 2026-11-26

Replaced the adjusted seated pose in Rabbit_Seated_Idle with the original Bunny_character neutral pose across 41 matching bones. Added only a 0.4-degree lower spine breathing cycle and visual floor compensation. Unity sampled the 2.6 s loop with less than 0.4 mm difference between sampled front and rear toe heights and no head rotation change. Source skin weights and ribbon-area upper-body pose were retained. Gameplay rendering remains unchecked.

## Rabbit seated posture and ribbon — 2026-11-26

Adjusted Rabbit_Seated_Idle to raise the head and straighten the back using coordinated torso motion, retaining the breathing loop. Unity pose sampling measured about 0.11 m of head lift before floor compensation. No ribbon bones or weights were edited. A UV-based upper rear ribbon proxy measured approximately 2.4% edge-length change at the 95th percentile, with isolated edges around 15%. The proxy does not confirm zero stretch or in-game clearance. Side-view visual acceptance remains pending.

## Rabbit seated gameplay idle — 2026-11-26

Replaced the former standalone Rabbit_Idle clip with Rabbit_Seated_Idle, a 2.6 s forward-facing seated breathing loop. Verified the existing Rabbit Idle state plays the new clip and removed the old standalone asset. Unity sampled 25 poses and measured near-zero floor deviation. Lobby Sit remains separate.

## Rabbit standing and landing pose — 2026-11-26

Adjusted Rabbit_Idle and Rabbit_Jump_Land to lift the head and reach the forepaws down and slightly forward. Recalibrated the animated skin floor after the edit. Unity midpoint samples had mesh floor Y=0.000 and front-to-rear toe bone differences of 0.048 m in Idle and 0.032 m in Jump_Land. The newly enlarged presentation scale was retained. Rendered gameplay remains unchecked.

## Rabbit visual size — 2026-11-26

Increased Rabbit prefab scale to 1.3 and compensated the player-only half-size placeholder mount in presentation. Recalibrated six grounded clips against skinned vertex heights. The player-mount Idle comparison measured Rabbit 0.384 × 0.976 × 0.984 m and Fox 0.634 × 1.785 × 1.664 m. Rabbit mesh min Y was 0.000 m at the visual anchor. CharacterController settings were unchanged. Interactive crawlspace visibility remains unchecked.

## Rabbit visual, animations and double-jump cue — 2026-11-26

Delivered Rabbit prefab, Generic Avatar, textured material binding and nine action states. SplitScreen confirmed Lobby Sit and the normal Fox-to-Rabbit exchange with a 1.0 m collider. A controlled Play Mode presentation probe confirmed Idle, Run, Jump_Start, Jump_Air, Rejump and Jump_Land playback and a zero world offset between visual anchor and Player Root. The natural input trace did not capture a successful jump. See the audit for gameplay checks remaining.

## Otter limb and Beaver torso Run tuning — 2026-11-26

Adjusted rotation curves on 16 Otter forelimb and hindlimb nodes using Beaver stepping as a reference, leaving Otter torso and tail curves intact. Changed only Beaver hips and chest rotations. Both controllers retain the existing 0.533 s loops. Unity pose sampling measured a zero-degree Beaver seam and a 3.54-degree maximum whole-rig Otter seam. Rendered gameplay remains unverified.

## Beaver Run from Fox paw trajectories — 2026-11-26

Replaced the independent stride curves with sampled Fox paw trajectories and pelvis timing, baked through constrained Beaver leg chains. Kept the Beaver visual at 1.7 and the existing 0.533 s Run loop and GUID. Unity sampled 96 poses with mesh minima from 0.003 to 0.016 m relative to the anchor. The largest deviation from a requested paw target was 0.116 m under the joint limits. Rendered gameplay remains unverified.

## Beaver Run stride articulation — 2026-11-26

Refined the existing 0.533 s Beaver Run loop with larger strides and separate upper limb, lower limb and paw curves. The first and last keys share the same pose and slope. At visual scale 1.7, 96 Unity samples placed the lowest mesh point between -0.006 and +0.016 m relative to the visual anchor. Rendered gameplay was not checked.

## Beaver Run from Otter — 2026-11-26

Baked Otter_Run timing and joint motion onto 24 matching Beaver bones, centering the motion on Beaver's rest pose to avoid importing Otter's standing offsets. Kept the existing Run asset GUID and Beaver visual scale of 1.7. Unity verified the 0.533 s loop and sampled mesh minimum Y of -0.007 to +0.018 m across 64 frames. Rendered gameplay was not checked.

## Beaver visual scale and ground pose — 2026-11-26

Set TwoTailsBeaverVisual root scale to 1.7 uniformly. Rebuilt Idle and Run from Beaver's native rest pose after Unity found transferred rear leg deviations of 73° and 101°. New sampled maxima are 1.5° and 16°. Recalibrated visual-skeleton height for Idle, Run, Sit and grip actions. At the new scale, the sampled mesh floor offsets are +13 to +15 mm for Idle/Run and -31 to -21 mm for the grip steps, relative to the visual anchor. Rendered gameplay acceptance is pending.

## Beaver proportions, origin and grip blends — 2026-11-26

Rebuilt the existing Beaver visual prefab with the natural Beaver_character mesh and rig, baked motion onto that rest skeleton, and added grounded skeletal height compensation. Unity measured grounded clip mesh minima from -0.006 to +0.016 m relative to the visual anchor. Restored normal Lobby swap and observed the live Beaver_character Animator with Beaver controller. Forced Animator speed tests selected Beaver hold and step motions only. Controlled wood handlers passed pickup/place/reclaim/place. Interactive grip, side views and repeat Lobby Sit verification remain pending.

## Beaver visual, animation and dam cues — 2026-11-26

Delivered Beaver Generic Avatar, textured visual prefab, Animator and 12 states. Reused its six source actions and authored six missing actions. SplitScreen verified Lobby Sit, Fox-to-Beaver exchange and a controlled pickup/place/reclaim/place sequence through the actual gameplay handlers with correct inventory and dam progress and the expected animation requests. Rendered contact and independent Host/Client verification remain pending. See the animation audit.

## Monkey forepaw orientation — 2026-11-26

Unity pose sampling found backward-pointing forepaws early in Monkey Climb_End and upward-pointing forepaws in Push. Baked bilateral wrist compensation into the existing Climb_End and four Push/Pull clips, and added wrist alignment after mantle reach IK. At 27 samples per clip, both forepaw toe vectors had a forward component of at least 0.91. Side-view gameplay acceptance remains pending.

## Three-animal grip poses and Monkey mantle — 2026-11-26

Updated the existing Push/Pull states for Monkey, Fox and Otter with low-head push, raised-head pull and box-displacement blend input. Sampled the original Mixamo Braced Hang To Crouch FBX and hand-authored an in-place Monkey Climb_End. In SplitScreen, controlled top-edge tests on Air Catch Vine and Vine Wall each entered mantle and finished about 0.010 m from the authored landing in 43 steps of 0.02 s. At the real Weight 2 box, a controlled probe confirmed Push, Pull and release to Idle for all three species. Side-on paw contact and interactive gait sequences remain unverified. See the animation audit for scope.

## Otter locomotion and Fox/Otter grip gait — 2026-11-26

Baked Fox Run and three jump phases onto the Otter rest skeleton. Fox and Otter now blend grip poses and stepping from measured player displacement. Otter ascent pitches the visual along upward travel and smooths back at the surface. Unity inspected all four blend trees and rotation-only baked clips. A controlled SplitScreen pool dive reached Y=-3.0, but the ascent trace contained no visual samples. Full gameplay grip, locomotion and ascent visual acceptance remains pending.

## Otter animation and water presentation — 2026-11-26

Delivered 15 Otter-rig actions, the Otter Animator, Generic Avatar and textured prefab in Assets/Art/Finished Animation/Otter. The existing swimmer now reports surface and underwater motion through the normal species presentation. Unity sampled all clips and the SplitScreen pool probe observed entry, floating playback, movement and dive requests, submerged hold and return to the surface. The normal lobby exchange and rendered character acceptance remain pending. See the animation audit for timings and test scope.

## Fox paw lever and blend follow-up — 2026-11-26

Fox presentation now waits for an active Idle/Run blend before submitting the opposite locomotion request. Accepted lever use passes the actual lever to the paw solver. The ordinary interaction clip supplies the underlying pose and the left foreleg follows the moving handle grip. Door demand remains immediate. Unity loaded the revised scripts. The Play Mode scene returned to OnlineTest without a spawned player after the SplitScreen load attempt, so normal lever use, gameplay transition timing and recording remain unverified.

## Fox scale and animation corrections — 2026-11-26

Fox visual scale is 1.22. The controller uses a rotation-only Interact and a stable Push pose. Active grips override grounding-derived airborne motion; the Fox presentation can enter landing immediately from either airborne animation. In OnlineTest Play Mode a temporary side-by-side prefab probe measured near-zero visual floor offsets and sampled 13 Interact poses with zero bone position or scale change. Host start hit a Netcode missing NetworkManager exception and the alternate port reported timeout, so live player sequences and lobby swap remain unverified. See the audit.

## Fox visual and Animator — 2026-11-26

Implemented the independent Fox prefab, Generic Avatar and controller under Assets/Art/Finished Animation/Fox, with the existing player/lobby species presentation and network motion path. OnlineTest localhost Host showed the Fox Idle take, Fox Base Map, and mesh minimum Y=0.032 against player anchor Y=0.040. See the animation audit for clip mappings and outstanding gameplay checks.

## Monkey contact and grip block follow-up — 2026-11-26

Implemented wall-normal alignment and a visual standoff in MonkeyPresentation. PrototypeMotor clears wall and rope attachment when gripping and keeps Push selected with neutral input. PrototypeGripBlock now checks support, sweeps downward under gravity, releases grips while falling, replicates position and airborne state, and resets below its start height.

Verified Unity loaded the revised code and OnlineTest localhost Host swapped Fox to Monkey via the lobby display. Runtime diagnostic lifts of both original scene blocks from Y=0.550 to Y=2.550 settled at Y=0.551. Directional wall limb contact, mantle, vine and rope constraints, push head contact, player-driven edge and respawn checks, and independent Client acceptance remain pending. See the animation audit for scope.

## Monkey below-floor follow-up — 2026-11-26

**Implemented:** MonkeyPresentation now recalculates the model wrapper's local position from the Player root every frame. This handles changes to the capsule Visual mount position or scale after instantiation, including the swap transition, without moving the Player root or changing collision/network components.

**Verified in the live OnlineTest Host:** Before reload, Player root Y was 0.040 and CharacterController was grounded, but the Monkey wrapper local Y was -5.000 and its skinned feet were at Y=-3.960. Setting the wrapper to the root-relative Y=-1.000 immediately put the feet at Y=0.040. After leaving and restarting Play Mode, a new localhost Host exchanged Fox for Monkey through the Monkey Display (`Now playing Monkey`). The fresh source kept wrapper local Y=-1.000; subsequent samples showed the skinned feet matching the moving Player root (Y=3.761 and 3.420 during airborne motion). Animator Apply Root Motion remained false. The Player root, collision settings and network root were not edited. An independent Client view and an interactive on-ground screenshot remain unverified. No commit or push.

## Monkey swap visual correction — 2026-11-26

**Implemented:** `MonkeyPresentation` now places its model wrapper at the Player root in the Visual mount's local coordinates, keeps its local rotation aligned, and hides both the capsule renderer and child `FacingMarker` while Monkey is selected. The Player transform, CharacterController and network position are untouched.

**Verified in SplitScreen Play Mode:** The original regression measured Monkey model feet at the Player root Y and the yellow FacingMarker visible. After correction, initial Monkey feet and root both measured Y 0.04, with the marker hidden and Animator root motion disabled. Swapping Monkey to Otter restored the capsule and marker, and swapping back produced feet Y 0.04 with the marker hidden. The Player root displacement from the species swap was zero.

**Online limit:** An attempted Direct Host check in the current Editor did not reach a spawned player. The scene reported `CanStart=false`, and its Console then filled with unrelated `CooperativePuzzle.Paint` null references and `PrototypeOrbitDrag` EventSystem selected-object exceptions. Play Mode was stopped. Host/Client visual parity remains unverified in this correction; the same `MonkeyPresentation` component is used by both local and network Player prefabs through `PrototypeAnimal`. No commit or push.

## Playable animal animation audit and test actions — 2026-11-26

**Implemented in isolated assets:** Blender 5.3 generated `Fox_Interact_test_v1` and `Monkey_Climb_Loop_test_v1` on copies of their original rigs. Versioned `.blend` and FBX files are under `Assets/Art/AnimationTests`; sampled pose previews and an interactive frame viewer are under `Docs/AnimationPreviews`. The original FBXs, scenes, and playable Prefabs were not changed. See [the six-animal clip audit and proposed poses](Playable_Animal_Animation_Audit.md).

**Verified in Blender and the live Unity Editor:** Both actions rendered on their own skinned models. Unity imported each as Generic with a valid Avatar and one named clip. Fox Interact is nonlooping, 0.71 s; Monkey Climb_Loop is looping, 1.00 s, with a 0.000-degree sampled first/last front-foot pose difference. Sampling temporary Unity instances moved the left front foot by 0.170 m (Fox) and 0.293 m (Monkey). The captured Console cursor had one pre-existing error and no new entries during import. This confirms asset import and bone playback, not animation in the game.

**Needs repair and next step:** Monkey's first-pass climbing motion still reads too much like ground locomotion in the preview. The playable Prefabs currently use capsule Visuals and have no Animator; the motor's motion signal has no presentation listener. Review the preview direction and unresolved gameplay triggers before revising Monkey, wiring copied visuals/Animator presentation, or making the remaining clips. In-game animation, two-player presentation, and physical-device checks are pending. No Unity version/package change, commit, or push.

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

## Animation preparation follow-up — 2026-11-26
Fox animated FBX is now present with nine clips; no restoration performed. Full Unity subclip inventory saved to Docs/Animation_Source_Inventory.txt. Six independent URP materials and preview Prefabs use reliable same-animal Base Maps; Elephant/Monkey/Otter packed textures extracted separately. Original importers, models and Player Prefabs unchanged. Finished Animation animal directories prepared, empty. Rendered Unity material acceptance and full Console-message diagnosis remain pending. See audit follow-up before starting an animal. No commit/push.


## Monkey animation and Lobby presentation — 2026-11-26

Implemented 15 Monkey actions on a copy of its source rig, imported a valid Generic Avatar, added the Animator and routed the Monkey visual through existing local/network species identities. Lobby slots show Monkey Sit while holding Monkey. Finished Animation placeholders are Git-visible. SplitScreen checked Sit, Run/Idle, jump phases, wall hold/climb/top exit and rope hold/climb/swing input. Direct localhost Host used the existing exchange to select Monkey and confirmed its Idle Animator and Base Map. See the animation audit and raw Play Mode notes for scope. Independent Client/Relay, visual contact and loop inspection, and specific Interact/Death/Push/Pull gameplay checks remain pending. No commit or push.

Follow-up probes confirmed transient rope release before regrab and matching sampled start/near-end rotations for all ten looping Unity clips (0.000 degrees at reported precision).






