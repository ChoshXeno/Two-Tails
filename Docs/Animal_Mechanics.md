# Animal mechanisms — development test

Scope: animal mechanics only. Further T07-D puzzle expansion and the T08 level flow are paused. Unity 6000.6.8f1 and installed packages are retained. All player-facing text is English.

## Entrances and assets

Use `Prototype_MainMenu`: Local Play opens `Prototype_SplitScreen`; Online Play opens `Prototype_OnlineTest`. Both contain the same dry, walkable swap lobby and animal test extension south of the original room. Existing puzzles, airborne vine catching, cameras and controls remain in place.

`Tools/Two Tails/Configure Animal Mechanisms` configures the two existing scenes and network player prefab through Editor APIs. It preserves existing assets and skips an already-created Animal Mechanisms root. The checked-in scenes are ready; running the menu is not required. Do not rebuild the older baseline scene builders over these scenes.

## Controls

| Action | P1 / online owner | Local P2 | Touch |
|---|---|---|---|
| Move | WASD | Arrow keys | Existing joystick |
| Interact | E | 0 / Numpad 0 | Interact, upper right |
| Jump / held Dive in water | Space | Enter / Numpad Enter | Jump / Dive, lower left |
| Held Surface | Q | Right Shift | Surface above Dive, only while swimming |

Dive and Surface together produce no vertical movement. Released underwater controls hold depth. Focus, pause, warp and disabled input clear held actions. The Surface action is independently configurable in the existing input assets. Existing desktop split-screen remains a shared-keyboard development mode; opposed tablet touch is still deferred.

## Layout and Inspector parameters

- `Animal Mechanisms/Safe Swap Lobby`: center `(0,1.5,-4.5)`, size `(16,3,5)`. Otter display `(-5,.04,-5)`, Beaver display `(5,.04,-5)`. Displays exchange their species and position with the player, so subsequent locations change.
- `AnimalMechanicsWorld`: interaction distance 1.5m, Visual-facing angle 45 degrees, display reservation .8s, Required Logs 3, water rise .3m/s. Three logs have unique bit identities. Inventory is per real player; displays have no player/weight identity.
- `PrototypeSwimmer` on the local actors and network player prefab: swim 3m/s, Dive/Surface 2m/s, surface feet offset .8m, bottom clearance .15m. Swimming uses the existing CharacterController; no dynamic Rigidbody is added.
- Ordinary pool center `(-14,0,-23)`, 12 x 16m, bottom -4m, surface -.6m. Safe shore `(-14,.1,-12)`. Entry ramp descends from `(-14,0,-13)`.
- Reservoir center `(12,0,-25)`, 12 x 20m, bottom -5m, initial surface -4m, target 0m. Dam `(12,-4.5,-25.5)`. Logs follow the entry ramp near x13, z -16/-19/-22. Exit ramp leads to the platform around `(12,0,-40)`.
- Reset marker `(0,.04,-6)`: developer-only Interact target. Restores logs, empty inventories, dam progress, low water and safe actor positions; clears swimming. Animal choices are retained.
- `PrototypeNightEnvironment`: radius 3m + fade 3m, Fox brightness 1.4 (range .25–3), gamma 1.15 (range .5–2). N toggles Fox A/B only; P toggles local developer daylight. Both reset on scene entry. Water and world labels use the same visibility policy; screen HUD is unaffected.

## Authority and limitations

A spawned `AnimalMechanicsNetwork` scene object stores display species/positions, reservation times, picked/deposited log bitmasks and water level in a server-write NetworkVariable. The existing owner player RPC validates the real sender and current PlayerObject, then checks distance, actual Visual facing, line of sight and safe exchange positions on the Host. The player NetworkObject and ownership never change. Animal identity and held inventory are server-write player state. Disconnect returns unspent logs; deposited logs remain in the dam. Late join reads current persistent state.

Swimming remains owner movement, matching the existing movement architecture. The Host validates interactions using received owner poses; this is not server-authoritative movement or anti-cheat. Rising water and detection share the same authoritative level. Non-Otters in deep water temporarily return to the configured dry shore; this is a prototype recovery rule, not a finished drowning mechanic.

The Monkey near-fill obstruction test currently handles BoxColliders (the actual greybox walls, doors, ramps and platforms), with 64 nearby blockers per camera. Local light visibility also uses URP shadow attenuation. New arbitrary meshes need an appropriate visibility blocker or a future generalized occlusion solution. This shader cost needs physical-device profiling. Sequential URP cameras were targeted; XR/custom renderer integrations are not claimed tested.

## Manual acceptance

1. In Local Play, approach and face each display, Interact, then exchange back. Check color, camera, HUD and per-animal abilities. Try a blocked landing and carrying logs while swapping.
2. With developer lights off, compare Fox/Monkey at the same position. Check exact-black unlit far geometry/labels/water, wall occlusion, shadowed lit regions and Fox-only brightness/gamma. Repeat after swapping online identities.
3. Otter: walk down the ordinary pool ramp, Dive, release, Surface, both actions, press into walls/floor, then leave by the ramp. Repeat with touch joystick plus action fingers on a real device.
4. Beaver: walk down the reservoir ramp, collect three logs, deposit three separate presses, walk back up as water rises. Otter crosses the raised reservoir and exits via the far ramp.
5. Repeat with Client performing the operations; try simultaneous/duplicate pickup and swap requests. Compare both peers' inventory effects, progress and water. Leave carrying a log, rejoin, and ensure it is collectible again.
6. Reset during swimming and after completion, twice. Check no duplicate logs, stale inventory or swimming state. Return to menu and re-enter.
7. Regress jumping, Monkey airborne vine catch, Lever and distinct-player plate gate. Different-network and Android touch/performance results remain separate from same-PC tests.

## Executed checks

Results are updated after the current test run; do not treat the acceptance list above as passed tests.

### Verification evidence (2026-11-14)

- C# compilation and Windows x64 Development Build succeeded. Unity's build reported zero errors. The CLI recompile request occasionally returned an HTTP 400 during domain reload; subsequent compilation/builds succeeded. This transport response was not treated as a C# compilation result.
- Local Play component probes: Fox -> Otter and Monkey -> Beaver swaps passed the real interaction/safe-position checks; three distinct logs were collected and deposited; water reached target 0. Otter floated at y=-1.4 in the ordinary pool, descended to y=-3.85 (bottom + .15), held released depth, treated both vertical buttons as neutral, and surfaced back to -1.4. A submerged Beaver returned to `(12,.04,-12)`.
- A Windows headless Development Client joined an Editor Host via **Sessions + Relay**, with a separate `animals_client` authentication profile. `Logs/AnimalClient_Diagnostic.log` ended `COMPLETE failed=False`, progress 3, water 0. It used the production owner motor and request RPC. It walked the ramp, collected three unique logs (including duplicate requests), deposited three times, walked out as water rose, swapped back to Monkey and then to Otter, tested Dive/hold/both/Surface, and reached the far bank at approximately `(12,.15,-38.32)`. Host observation matched Client progress, water, animal and remote position. This is mechanism execution evidence, not just a two-player connection check.
- The probe uses scripted approach warps for exchanges and initial pool positioning. It does not prove a human completed every route or validate touchscreen input. Two earlier Relay automated swap-back attempts were rejected after long-distance positioning; a focused Direct swap-back and the subsequent full Relay diagnostic passed. The exact earlier rejection reason was not captured, so do not describe it as a proven fixed gameplay defect.
- Host validation rejected excessive distance, back-facing and an inserted line-of-sight blocker with `Move closer`, `Face the target`, and `Interaction blocked`.
- Runtime inspection found zero Missing Scripts, one online Camera/AudioListener/EventSystem, and no active non-text mesh with shadow casting disabled.
- Initial visual comparison: `Temp/Animals_A.png` inspected in Editor, with matched Fox/Monkey poses; unlit far world/labels disappeared on Monkey while Fox retained the scene. Further final visual checks are recorded below.

Observed and corrected during testing: MaterialPropertyBlock creation moved from a MonoBehaviour initializer to Awake; overlapping walkway/platform geometry was shortened to unblock ramps; underwater ramp gaps received solid walls; initial server animal allocation now handles the scene NetworkObject spawn order.

Observed existing room limitation: killing the Client process caused NGO to remove it after timeout, but an immediate same-identity Session join returned `SessionConflict` due to stale cloud membership. Host Leave Room closes that room. This task does not add seamless recovery or rewrite Sessions. Normal Leave Room is tested separately below.

Physical-device acceptance is still pending: Android/iPhone multitouch, soft UI behavior, frame rate/temperature, and real different-network testing. A headless Client does not verify its rendered graphics. Shared mobile URP currently lacks the PC profile's additional-light shadow support; mobile visual parity must be checked/configured before claiming phone acceptance.

### Changed files

- Scenes: `Prototype_SplitScreen.unity`, `Prototype_OnlineTest.unity`; existing GUIDs retained. Main Menu entry names unchanged.
- Prefab: `NetworkPlayerPrototype.prefab`; added animal actor/swimmer components.
- Input: `TwoTailsInput.inputactions`, `PrototypeSplitInput.inputactions`; added Surface without replacing existing action IDs.
- Runtime additions: `AnimalMechanicsActor`, `AnimalMechanicsState`, `AnimalMechanicsWorld`, `AnimalMechanicsNetwork`, `AnimalNameLabel`, `PrototypeWaterVolume`, `PrototypeSwimmer`, `PrototypeSwimmingHud`.
- Runtime adapters: `PrototypeAnimal`, `PrototypeMotor`, `PrototypeLocalInput`, `PrototypeInteractionPrompt`, `PrototypeSplitScreen`, `NetworkPlayerPrototype`, `PrototypeNetworkInteraction`, `PrototypeNightEnvironment`.
- Rendering: existing `PrototypeAnimalVision.shader`, new `PrototypeVisibility.hlsl` and `PrototypeVisibleTransparent.shader`; new BasinStone/DamWood/Water/DamMarker materials. Shared URP assets were not changed.
- Editor setup: `AnimalMechanicsSetup`; opt-in development QA: `AnimalMechanicsProbe`.
- The pre-existing uncommitted `PrototypeNetworkSession.cs` identity-assignment change is retained, not attributed to this task. No commit/push or package/Editor upgrade.

### Repeatable independent-client probe

Build with `Tools/Two Tails/Build Windows Friends Test`. Start an Editor Online Play room, then run the complete build folder's executable with:

```powershell
.\TwoTails.exe -batchmode -nographics -animalProbe -joinCode YOUR_CODE -authProfile animals_client -logFile AnimalClient.log
```

Add `-animalCycle` for normal leave/rejoin, persistent state, repeated reset and unspent-log return checks. `-animalSwapOnly` isolates two display exchanges. `-animalObserve` only joins and reports persistence. Omitting `-joinCode` uses the existing `127.0.0.1:7777` Direct Test route. These explicit QA bootstraps are compiled only in Editor/Development Builds. Ordinary launches never run them.


### Final lifecycle, regression and visual checks

`Logs/AnimalLifecycle.log` ended `COMPLETE failed=False`. The independent Relay Client executed the full dam/swim path, then normal Sessions Leave/Join twice. It observed completed dam/water state on late join, retained the free Otter identity without duplicating a display animal, issued two synchronized resets, collected an unspent log as Beaver, left, and rejoined with empty inventory and the world log restored. Host observations matched the replicated state.

Additional actual checks:

- Two local actors requested the same display in one frame: first accepted; second rejected with `Display is busy`.
- Beaver acquired a real log, then attempted a valid lobby swap: rejected with `Use your logs before swapping`, inventory remained 1.
- Existing network Lever request opened its original door. Two actual network PlayerObjects on separate original plates yielded `Gate: Open`; moving Host away yielded `Gate: Closed`. These are component/RPC regression probes, not human playthroughs.
- Existing local Jump executed and landed; Monkey airborne catch entered Climb and reached approximately y=3.94. CLI-induced frame stalls affected measured jump apex, so no jump-height tuning claim is made.
- `Temp/Animals_Final_AB.png` versus `Temp/Animals_Final_Bright.png`: changing only Fox brightness 1.4 -> 2.2 and gamma 1.15 -> 1.5 produced mean sampled RGB difference 38.59 in Fox's world crop, **0 in Monkey's equivalent crop**. A distant black sample was RGB (0,0,0). The captured matched-pose test deliberately hid one overlapping placeholder renderer; these runtime poses/brightness changes were discarded.
- Both custom shaders reported zero shader messages; no runtime mesh had a missing/error material. Final resized label layout was inspected in `Temp/Animals_Final_Entry.png`.
- Final saved SplitScreen and OnlineTest scans: zero Missing Scripts, zero broken serialized object references. Final Local -> menu -> Local run: 2 actors, 2 cameras, 1 AudioListener, default Fox vision enabled, fresh Console 0 errors / 0 warnings.
- Final Windows Development Build succeeded after the last label-size/release-reset-marker changes. The full runnable folder is `Builds/FriendsTest/` (`TwoTails.exe`, Data and dependencies); it is Git-ignored. The final cosmetic revision was not rerun through the entire network probe; the mechanism code path was unchanged apart from hiding the reset marker in non-development builds.

Still awaiting user validation: interactive mouse/keyboard and touch HUD ergonomics; both rendered online windows (the automated Client was headless); strict blackout/shadows across every camera angle and material; simultaneous two-peer display requests over real latency; physical mobile graphics/multitouch/performance; different networks. Existing mobile URP shadow limitations and abrupt-client SessionConflict are not marked passed.

Suggested commit: `feat: add animal swapping, otter swimming and beaver dam tests`

Development journal saved according to AGENTS.md: [Notion animal mechanisms journal](https://app.notion.com/p/4170be84637f8132a139c93a4efd7f19). Repository documentation remains the implementation/configuration reference.

## November 18 — Beaver and ordinary-animal tweaks

The moving dark silhouettes came from the old reveal ray, which started at the player's position and intersected every nearby solid box. These were changes in reveal coverage. The live SplitScreen audit found no Light under any actor or Camera and no active Volume. PC URP has a screen-space ambient occlusion feature. Its settings were retained. The pending workspace correction limits reveal rays to walls, dividers and doors and applies fixed diffuse near fill independently of lit PBR output. Fox uses its separate camera brightness and gamma. The transparent shader applies the same visibility and real-light exception to world labels, water and slot outlines. The outlines depth-test against opaque geometry and cast no shadows.

Both test scenes retain the existing dam root and slot references. The dam moved to the dry apron at (12, 0.04, -10) so Beaver can reclaim pieces at full water. Three slots sit at local X -1.05, 0 and 1.05. Approach from the south and face the desired piece. The basin and ramp geometry still provide collision. The dam pieces themselves are visual and have no enabled colliders, including optional replacement prefabs.

In the Animal Mechanisms Inspector, **Installed Logs** provides the slot boxes. Each valid entry's order is its ID, so append new entries or rebuild both peers after editing the order. **Slot Prefabs** optionally replaces the solid presentation for each source index. Use the authored slot box to set preview position, rotation and size. The preview is a runtime visibility-aware transparent wireframe with depth testing. The requirement is derived from the valid entries. The prototype snapshot encodes up to 16 slot entries and 15 unique authored source logs. There is no player carry-capacity rule. Add enough source logs for each new slot. **Interaction Distance**, **Interaction Angle**, and **Rise Speed** remain on Animal Mechanics. Rise Speed governs both directions. Initial and target heights remain on the Dam Reservoir's Prototype Water Volume. Ordinary view radius, fade and Fox controls remain on Prototype Night Environment.

Use E / touch Interact or Pickup to collect or reclaim. P2 uses 0 / Numpad 0. Use Q for P1 or online Build, Right Shift for P2, or the visible Build button. The online auxiliary button shares the existing Surface input and becomes Build only for a Beaver within dam range. Otter Surface remains a held action. Out-of-range and blocked targets are excluded from Pickup selection. The server serializes transactions and rejects a request based on an older interaction revision. The replicated snapshot stores the exact slot-to-original-log identity. Inventory updates continue through the existing per-player authoritative variable. Make a new matching build for each peer because the snapshot layout changed.

Verify manually by collecting three loose logs on the reservoir entry ramp and returning to the dry apron. Build three times and watch the previews disappear in order. Once full, face and reclaim each possible slot in turn, verify the relevant preview returns and watch water drain, then rebuild and observe the reversed direction. Repeat with the other peer, leave and rejoin, then reset. Check camouflaged outlines behind walls and in darkness at varied angles, view isolation, and physical touch. Existing status history above refers to older builds unless specifically identified as November 18 evidence.

## November 18 — night shadow and light-edge follow-up

### Cause and implementation

The live SplitScreen scene contains eight enabled realtime lights. Its route, pool, Lever, dam and lit-area spot lights all had `LightShadows.Soft`; the small source shape (`m_ShapeRadius: 0.025`) still produced conspicuous geometric shadow maps. The scene has no assigned LightingDataAsset (`m_LightingDataAsset` is the built-in null reference), so baked shadows were not the cause. `PrototypeNightEnvironment`'s wall/door reveal blocker mask also does not draw projected shadows. The PC URP Renderer additionally had an active Screen Space Ambient Occlusion feature (`Intensity 0.4`, `Radius 0.3`), which contributed contact darkening but not the large directional silhouettes.

`PrototypeNightEnvironment` now saves the original light shadow modes and PC SSAO feature state, disables those shadow contributions during night, and restores their original values when Developer Lights is enabled or the component is disabled. Direct local-light illumination, PBR surface orientation, visible object geometry, collision and the camera-local wall reveal remain. This is runtime-only; the shared PC/Mobile Renderer assets and scene light settings are unchanged. Scene lights are local to each game instance; in SplitScreen both views share the same night policy, while Online Play changes only that instance's local rendering.

The local-light visibility mask now eases through `smoothstep` rather than using a binary near-zero contribution gate. Transparent water and world-space marker/text rendering use the same eased reveal. The setting is per scene camera environment, not a change to Unity's lamp range/angle.

### Inspector and live verification

On `Level 1 Night Test` > `PrototypeNightEnvironment` (in both `Prototype_SplitScreen` and `Prototype_OnlineTest`):

- `Local Light Fade Width`: **0.2** by default (range 0.01–1). Increase it to soften the lit-to-black mask transition.
- `Suppress Realtime Shadows At Night`: enabled by default. Original per-light modes return with Developer Lights.
- `Suppress Screen Space Ambient Occlusion At Night`: enabled by default. The saved Renderer Feature remains enabled and is restored with Developer Lights.
- Existing `Visible Radius` **3m** and `Fade Width` **3m** remain separate Monkey proximity controls.
- Fox Night Vision brightness/gamma and the player-centered near-fill are unchanged.

Live Unity Editor checks, Unity 6000.6.8f1, November 18:

- Runtime inspection observed all eight lights at `None` and PC SSAO inactive in night mode; Developer Lights restored all eight light modes to `Soft` and SSAO to active. Turning Developer Lights off reapplied the night values.
- Compared live SplitScreen Game View captures in night mode before and after the compiled changes. The conspicuous ground-cast hard silhouettes were removed. Closed and open LeverDoor states were captured; the door panel BoxCollider stayed enabled in both states. Object surfaces and direct lit regions remain shaded.
- Regressed existing local Beaver handlers in temporary Play Mode: 3 unique logs collected, 3 ordered slots built, middle slot reclaimed with its outline restored, water target changed from 0 back to -4, then the slot was rebuilt and target returned to 0. Play Mode was stopped; those test positions/state were discarded.
- `git diff --check` and Unity script compile passed. No errors were reported; Unity emitted two existing `UAC0009` warnings from the development probe scripts. Fresh Console status after the Play Mode checks was 0 errors / 0 warnings.
- Unity was returned to `Prototype_MainMenu`, outside Play Mode. Saved split/online scenes and shared Renderer assets were not edited by this lighting pass.

Still awaiting user acceptance: keyboard-controlled walking around a fixed prop with both split views visible, actual interactive wall/spotlight-edge inspection at several positions, Relay rendered-view parity, Windows player/build visual parity, and mobile URP parity. No physical-device test was performed. The local fade width is a normalized light-mask contribution width rather than a world-distance measurement.

### Observed verification for the November 18 tweaks

- Unity Play Mode split-screen passed 14 scripted checks through the actual world, actor and HUD components. The checks included swap, one viewport's inventory, three Pickups, three ordered Builds, exact middle-slot preview restoration, rejection of duplicate reclaim, rising water, drainage, rebuilding and reset. A screen capture showed the outlines and a disabled Build button at zero inventory.
- Online HUD in a running Host verified Pickup at a loose log, actual button click pickup, live count, enabled Build with wood, disabled Build after spending it, Surface for a swimming Otter, and reset clearing swim state with empty slots and water at -4. Input binding inspection confirmed Q, Right Shift and gamepad buttonNorth without changes to the Input Actions assets.
- Controlled rendered reveal comparison used a fixed camera and stationary box, with all real lights off and the ordinary reveal active. Moving the reveal origin from X -1.7 to +1.7 changed zero of 2,820 visible samples in the shared near region. An outer sample was RGB (0,0,0) and a fade sample changed from (24,37,50) to (1,1,2). A separate fixed spotlight cast a visible shadow. Captures are in Temp/BeaverChecks. Both shaders reported zero shader messages. This samples one controlled geometry setup and does not cover every wall, Fox angle or material.
- Windows x64 Development Build succeeded with zero errors and three warnings at Builds/BeaverTweaks/TwoTails.exe. Cached C# runtime and Editor compilation also succeeded. A pure C# executable passed 10 assertions for ordered fill, partial and full reclaim, duplicate reclaim, completed-dam inventory preservation, exact rebuild and reset. Diff whitespace check passed.
- The first Direct Host launch hit NGO duplicate message registration with Domain Reload disabled. A fresh domain reload restored Host startup for testing. The original Editor options were restored. Independent matching Windows Client log: Logs/BeaverClient_VerifiedHost.log, COMPLETE failed=False. The Client completed the three unique pickups, three builds, duplicate reclaim rejection, exact slot restoration, drainage and renewed rise, swapped back, swam and reached the far bank. Host snapshot observed progress 3, slots 801, water 0 and two peers. The runtime Host scene had zero missing scripts. Immediate rejoin after terminating the Client failed once. A fresh Client after the Host released the old slot observed progress 3, water 0 and empty inventory. This run used Direct on the same machine.
- Pending: human controller and simultaneous touch use, rendered remote view including Fox and wall/transparent occlusion at varied angles, fresh Relay/cross-network run, real-device performance, and concurrent requests from two peers under latency. Duplicate requests from the independent Client were exercised. The ordinary gameplay roster assigns one Beaver at a time.

Changed implementation includes both prototype test scenes and AnimalMechanicsSetup; PrototypeAnimalVision and PrototypeNightEnvironment; AnimalMechanicsWorld, AnimalMechanicsState, AnimalMechanicsActor, DamSlotPresentation and AnimalMechanicsProbe; PrototypeLocalInput, PrototypeNetworkInteraction, PrototypeNetworkPuzzleHud, PrototypeSwimmingHud, PrototypeSplitScreen and BeaverInventoryHud. Existing asset GUIDs remain in use and Unity/package versions were retained. Base 8c6a1b2.

## Rabbit and Elephant course — 2026-11-19

Implemented in the existing SplitScreen and OnlineTest entries. The Animal Mechanics world creates the course when the scene starts, so the saved scenes and existing asset GUIDs remain intact. Swap at the two added lobby displays, then follow the center of the southern animal-test walkway between the pool and reservoir. The Rabbit course is on the left and the heavy block and blue barricade are on the right. The existing lobby Reset restores the block and board, clears push input and returns players to the safe lobby while retaining their animal identities. Use matching builds because the animal snapshot changed.

- Rabbit Weight 1. Authored standing baseline H 2 m, W 0.8 m, J 1.2 m. Rabbit controller H 1 m, W 0.4 m. Prototype Motor exposes Rabbit Jump Multiplier (2) and Scale Rabbit Placeholder. The latter should be disabled once the visual has its own Rabbit dimensions. Jump uses the existing bindings, with two presses available until confirmed landing. Both jump impulses target 2.4 m from their respective takeoff positions.
- High platform rise 3 m. Equal-height far gap 4.2 m. Ideal continuous-physics comparison at speed 4 m/s and gravity 24 m/s² is 3.58 m for one jump and 6.11 m for a second jump at the first apex, excluding collision and discrete stepping. These are calculated values, not Play Mode measurements.
- Tunnel and board frame have 1.30 m headroom and 0.46 m width. Tunnel is 4 m long. Elephant Weight 3. Approach the block from its east shelf and hold movement west. Heavy Push Speed is on Animal Mechanics (1.5 m/s); guide travel is 1.5 m along world -X. Strike uses Interact and the existing interaction distance/angle (defaults 1.5 m/45°). Host validates current player, species, pose and range and checks the block's swept volume before advancing it. The player pose still comes from owner-authoritative movement.
- Verification: cached Unity Roslyn C# compile exit 0, arithmetic geometry assertions, and git diff --check. The Editor was unreachable via Pipeline. Batch startup exited 1 before import after a refused licensing IPC connection and WMI assertions. NGO postprocessing, live collision clearance, ability rejection, duplicate Strike, Reset, all manual jumps, Host/Client parity and existing gameplay regressions are pending. See Logs/RabbitElephantCompile.log.

## Elephant free push test — November 19, 2026

The original Heavy Gap Block remains on its 1.5 m guided path for the existing course. A separate `Elephant Free Push Test Block` is saved under Animal Mechanisms in both prototype scenes at (0, 0.55, -10), on the wide 48 m by 6 m walkway. It is a 1.6 × 1.1 × 1.6 m cube with a BoxCollider and kinematic Rigidbody. The player collision handler requests movement only for Elephant. `ElephantFreePushBlock` sweeps against solid colliders, applies horizontal movement, simulated vertical gravity, and an individual fall respawn. It has no route endpoint. Inspector controls are Push Speed 1.5 m/s, Fall Height -5 m, Respawn Delay 0.8 s, Use Initial Transform, and an optional Respawn Point. The old Heavy Gap Block and Strike Board are separate.

The dam slot outline runtime material now uses white. The user-modified PrototypeDamMarker material was retained. Unity Editor compilation reported no errors. Edit Mode inspection found one named block in each saved scene. The inspected SplitScreen instance had an enabled BoxCollider and kinematic Rigidbody. Manual multi-direction pushing, edge fall/respawn, standing on the moving block, existing course regression, and online synchronization remain unverified. This free block has no network transform or authoritative snapshot yet, so online movement should not be considered synchronized.

## November 22 — Elephant block support fix

The original gap block is a scene BoxCollider moved by the Animal Mechanics snapshot. Its previous push path accepted only the east face and clamped HeavyOffset at 1.5 m, so the bridge position was the hard endpoint. It now accepts pushes from both X faces, updates signed offset in the same Host snapshot, and checks the actual support under its centre. Once unsupported, the snapshot carries a vertical drop and returns the block to its own saved starting pose after it falls below Heavy Fall Height for Heavy Respawn Delay. Reset also clears the vertical state. The block is still a scripted scene collider, not a Rigidbody. The local client and Host should use the same updated build because the network state layout changed.

The separate free push block has a solid BoxCollider and kinematic Rigidbody. Its previous downward SweepTest could miss the floor while the block rested flush against it, leading the script's gravity to move it through the walkway. A short ray from inside the bottom footprint now detects the solid ground each physics step, while the existing horizontal sweep checks walls. It remains Elephant-only, and its individual starting transform is the respawn pose. The actual walkway has a non-trigger BoxCollider, top at y=0. The free block starts at (0, 0.55, -10), with its bottom at y=0.

Unity script compilation completed with no errors and git diff --check passed. The active SplitScreen scene was dirty in Edit Mode, so no Play Mode physics run was performed in this pass. The guided gap block has no Rigidbody; the free block has a kinematic Rigidbody and an enabled non-trigger BoxCollider. Full passage, reverse push, edge fall, Rabbit standing and two-instance synchronization remain to be observed in Play Mode.

## Monkey rope course — 2026-11-23

Both Prototype_SplitScreen and Prototype_OnlineTest now contain an editable Monkey Rope Test root. Follow the animal walkway east (+X) past x=24. Climb Rope is at (25.4, 6, -7.4), Rope A at (28.9, 7.5, -10), and Rope B at (35, 7.5, -10). The blueprints use the existing materials and safety floor. The existing OnlineTest night radius edit of 1 m was preserved.

Monkey can grab automatically while airborne. In P1 and Online, hold E/Q for Up/Down and use Space to release. In P2, hold 0/Right Shift and use Enter. Movement remains camera-relative. Online touch uses the existing L-shaped button group with Up above Down and Jump left of Down. Otter now holds Jump/Space (P2 Enter) for Surface and the auxiliary action for Dive. Beaver retains contextual Pickup and Build. SplitScreen retains its existing keyboard-specific layout and adds current-player rope guidance.

PrototypeSwingRope exposes Length, Push Force (2.4), Damping (0.35), Max Angle (58 degrees) and Climb Speed (2 m/s). The shared swing state is server-written online on scene NetworkObjects. Character movement and rope pose marker use existing owner movement and motion replication. The motor exposes RopeGripPoint for animation. Network peers need the same scene assets.

Observed: Unity compilation completed with no errors. Both scenes had three saved ropes. SplitScreen Play Mode probes rejected Fox, grabbed as Monkey, climbed up about 0.91 m and down 0.80 m, released without immediate regrab and inherited the sampled tangential direction. In a fixed-step comparison, single-direction peak was 27.8 degrees and reversing at the turning points reached 41.2 degrees. A scripted run starting already near Rope A grabbed A, transferred to B and reached (41.61, 1.34, -10), at the landing platform. Direct Host spawned all three ropes and showed Up/Down/Jump, Surface/Dive in water, Jump after leaving, and Build for Beaver near the dam. The Host was stopped after the check.

Pending: full human run from the launch platform, climbing both end stops, physical touch and gamepad, active swimming hold behavior and building with the new button layout, and an independent network Client including transfer and rejoin.
## Monkey lobby display — 2026-11-23

`Prototype_OnlineTest` and `Prototype_SplitScreen` now each save one `Animal Mechanisms/Monkey Display` in the walkable lobby at `(0, 0.04, -3.2)`. It duplicates the existing Otter display construction and uses a separate yellow `PrototypeMonkeyDisplay` material. The display is editable in Edit Mode. The fifth serialized display state holds its species, position and swap reservation for late joiners. The existing 1.5 m / 45 degree interaction, line-of-sight, safe landing and Host-controlled exchange paths apply; exchanging to Monkey calls the existing animal ability selection, including vines and ropes. The default first player remains Fox.

Player species are unique: the server rejects a display exchange into a species held by the other current player. A second player prefers Monkey when available; if the first player already controls Monkey, the second player receives Fox. A display may still show a species held by a player, but its exchange is refused while that player holds it. This avoids changing the existing two-player spawn path or hiding an authored display.

Observed: Unity compiled with zero errors; both saved scenes contain the display and serialized references. Play Mode loaded the split scene with two actors and Monkey display state, but the unfocused Editor remained at frame 2. Actual Interact input, Host-alone swap, rope traversal and join/rejoin identity synchronization need interactive acceptance. Open `Prototype_OnlineTest`, create a room alone, swap Fox to Monkey, then check Up / Down and Jump on the nearby ropes. Join a second instance and confirm it controls Fox. Also test the reverse order, with P2 Monkey first, and confirm P1's attempt to take the occupied Monkey is rejected.

## Vine, segmented rope and moonlight — 2026-11-23

- Monkey vine movement now uses the existing screen-relative input axes: W/S or joystick Y changes height, while A/D or joystick X moves along the wall tangent. The combined input is clamped. A BoxCollider-local edge check accounts for transform scale and the controller radius. Jump leaves the vine with a short reattach delay. The landing remains subject to the existing standing-space and ground checks.
- The rope retains its host-authored pendulum drive at the grabbed height. Eight equal-length visual links and grasp geometry now read one shared chain. Links below the held point use Verlet gravity, inertia, damping and length constraints; the held portion follows the pendulum. The host sends a persistent eight-point rope snapshot alongside angular state so a joining Client can reconstruct the current curve. W/S changes arm a short impulse that waits for swing velocity to align, rather than spending it against the previous swing before the turn. Inspector settings remain on each `PrototypeSwingRope`: Length, Push Force, Damping, Max Angle and Climb Speed. The straight `Rope Visual` child is the Edit Mode template; Play Mode creates the visible links from it.
- `PrototypeNightEnvironment` has an Inspector-linked `Weak Moonlight` Directional Light in both saved scenes. Defaults: rotation `(65,-35,0)`, blue color `(0.34,0.46,0.70)`, intensity `0.12`, Soft shadows. The existing night shadow suppression continues to remove other light shadows; the moon alone keeps a shadow map. The ordinary animal visibility shader includes the shadowed main-light contribution as a light exception outside near reveal. Developer Lights turns the moon off, then restores it when returning to night. `Level 1 Night Test/Moonlight Roof Test` contains a solid roof and three cave walls centered near `(-5, 0, 5)`; open ground and cave center are the comparison positions. PC and Mobile URP assets already support main-light shadows; neither asset was changed.

**Observed:** Unity compiled with zero errors; current Console had zero errors. Both saved scenes reopened with the moon and roof objects and zero Missing Scripts. In SplitScreen Play Mode, all three ropes had eight runtime links and the night moon was enabled with Soft shadows. A controlled same-depth, 440-step rope simulation measured a 1.37 m maximum grab-point displacement with continuous W versus 1.86 m with direction changes timed to motion; a 260-step probe measured tail curvature up to 0.973 and link-length error up to 0.001 m. A controlled live component call entered the vine, moved 0.56 m laterally, rose 0.34 m during diagonal input, and exited with Jump. Center lateral motion was 0.20 m and edge motion 0.00 m after fixing the scaled-collider radius. A ray toward the saved moon from the cave center hit `Solid Cave Roof`; the comparison ray from exposed ground was clear. Developer Lights disabled the moon and restored it with Soft shadows. OnlineTest Direct Host connected with one Fox player, all three rope NetworkObjects spawned and no current Console errors.

**Pending interactive acceptance:** actual keyboard and joystick climb, repeated high-point timing by a player, Rope A → Rope B → platform traversal, rendered moon shadow/visibility comparison at the eave, cave mouth and interior with ordinary animal and Fox, Local SplitScreen visual isolation, and independent Host/Client rope movement and position parity. The direct Host smoke test does not establish Client sync. No Windows build or physical touch test ran for this change. The chain's upper portion intentionally stays straight from anchor to grip; only the free tail bends, and Client visual smoothing can leave a short-lived position difference under latency.
