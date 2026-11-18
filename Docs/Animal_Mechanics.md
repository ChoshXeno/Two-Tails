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
