# Cooperative greybox tests (T07-D / T07-E)

2026-11-11 — Unity 6000.6.8f1, URP 17.6.0, NGO 2.13.3, Transport 6.6.0, MPS 2.4.1. No package upgrades.

## Entry and layout

Open `Assets/prototype/Scenes/Prototype_MainMenu.unity`, then Local Play or Online Play. Actual scenes remain Prototype_SplitScreen and Prototype_OnlineTest; the older Prototype_Network and Greybox baselines are retained. Walk north from the existing room to the arrival plaza. The original NorthWall is disabled, not deleted; its GUID/references and the other baseline objects are retained.

- Left, origin (-22,0,20): **TT-P05 Switchback Gates**. A is in the left lane at local (-2.5,0,0); B enters the right lane and reaches the second tile at (2.5,0,8). A holds while B crosses. A and a different B occupying the two tiles locks both gates open until reset. If A leaves early, the first gate closes when its passage is clear. Meet at local z=19. Walls/gates are 4 m tall and have no climb surface.
- Center, origin (0,0,20): **TT-P01 Bridge Handoff**. A tile (-2,0,1), B tile (0,0,14), exit z=26. Gaps are 8 m, beyond the current jump distance. B reaches the island while A holds; a distinct B locks both bridges. Early release retracts the front bridge after riders clear it. A failed fall uses existing player reset; the marked reset tile clears the attempt. The seesaw variant is not implemented.
- Right, origin (22,0,20): **TT-P02 Counterweight Lift**. Deck centers x=-3.5/+3.5, z=3. One Weight 2 crate starts left; two Weight 1 crates start at right loading positions (2.5,0.5,-2)/(4.5,0.5,-2). Both players weigh 2. Push into the crates using ordinary movement; load is counted only when feet/crate center is on the deck. The ramps follow the controlled lift height. Heavy side descends, light side rises. Distinct players and exactly 4/4 latch alignment and open the exit barrier. After latching, subsequent unloading does not undo completion; reset starts another trial. Fallen crates recover at their original loading positions.

Each zone has a marked **RESET** tile at local (-6,0,-3). Stand there 2 seconds, then leave before another request. Only actors inside that zone are moved to its safe entry. It resets that zone's state, travel and crates. In online mode the server validates tile occupancy; no untrusted reset-state payload is accepted. A disconnection during a latched attempt resets that attempt, avoiding stale cooperative state.

## Shared rules and authority

`CooperativePuzzle` samples foot-level volumes and deduplicates by PrototypeAnimal root. Offline uses enabled local motors. Online additionally requires a currently connected spawned PlayerObject through the existing NetworkPuzzleOccupancy validation. Static decoration and old debug identities cannot count online.

`CooperativePuzzleNetwork` owns a server-written persistent NetworkVariable snapshot: latch, travel, weights, tile flags, reset revision and all three crate positions. Scene NetworkObjects use the existing NGO scene-management path; clients never spawn the same puzzle independently. Remote clients apply snapshots, including after joining. No NetworkTransform also writes these puzzle transforms.

Crate contact requests are emitted only by locally owned moving characters. The server resolves the actual sender's PlayerObject, checks proximity/facing, rate limits and validates the candidate box volume/support. Push magnitude is server configured. Player movement remains owner-authoritative; received position/facing are not full anti-cheat validation. Remote platform snapshots carry only the local enabled controller, while the server remains the sole puzzle-state writer.

## Inspector

On each zone root: Kind, Networked, plate/reset references, moving transforms, ramp references, exit barrier, crate list, state label; Travel Speed 1.7 normalized/s, Lift Range 0.35 m, Lift Speed 0.25 m/s, Reset Hold 2 s, Target Weight 4, Push Speed 2 m/s. Weight is explicit on CooperativeWeightBox. Do not change the player identity weight by color/name.

NetworkManager / PrototypeNetworkSession: **Swap Animals** (development only, set before starting). It changes server-assigned animal identity but not spawn-slot allocation. Each endpoint's camera reads its followed animal. Default remains Host Fox / Client Monkey.

Each area's Route Light is a bounded spot; per-light shadow tier is 512. Shared PC/Mobile URP assets are not modified. The mobile asset currently disables additional-light shadows, so mobile shadow parity is pending a separate target-specific configuration/validation.

## Executed offline checks

- C# compilation completed with zero errors/warnings before the Windows build.
- Actual running components: Switchback A-only opens B gate; distinct B latches; A leaving preserves passage. Bridge has the same handoff persistence.
- Both puzzles: early release returned both travel values to zero; successful handoff then 2-second physical reset-tile occupancy cleared the latch.
- Continuous existing CharacterController movement traversed both complete routes: B reached the second tile, then A and B each reached the exit. Actors were initially positioned by the test harness; this is automated motor traversal, not keyboard usability acceptance.
- Lift with both actors and left crate measured 4/2, travel -0.35/+0.35, passengers moved to the corresponding heights. Actual motor contact pushed both right crates up the ramps and triggered the balance lock/alignment. Continuing to push past the platform reduced displayed load, while completed lock correctly persisted.
- Earlier ramp-edge blocking was reproduced and corrected with support-height/candidate-volume validation.

## Manual acceptance matrix

Record each platform separately. Pending checks are not implied by compilation or earlier task acceptance.

1. Night / Fox A/B / shadows: follow T07_Night_Vision.md. Compare same position, orbit/zoom and swapped online animal identity. Verify local Developer Lights does not change the other instance.
2. Each puzzle: complete once, interrupt once, reset, complete again. Try wrong player/one player covering both plates; check no gate crush. Test Jump and Monkey climbing cannot bypass route barriers.
3. Lift: push each crate onto and off deck; jump off; ride both directions; verify 4/2, 4/3, 4/4; reset while moving; disconnect while occupied. Check latency collision limits with both players close together.
4. Online: host/create and independent client/join; compare all travel/load/box/latch states. Client must do handoff and crate push. Leave/rejoin, repeat with Swap Animals enabled. Reset should not change adjacent zones.
5. Baseline: Lever, AND door, Jump, suspended vine catch, camera drag/zoom, UI input exclusion, menu return.
6. Windows executable graphical UI and split view; friends across different networks; Android touch/performance remain distinct results.

Do not distribute only the exe. Include the entire `Builds/CooperativeTest` runtime folder, excluding DontShip/debug-symbol backups if packaging for friends. No authentication cache or administrative keys are added by this task.

## Independent Relay and final regression evidence

Two actual Sessions/Relay runs used Editor Host plus `Builds/CooperativeTest/TwoTails.exe` as a separate headless Client with `-authProfile coop_client -coopProbe -joinCode <session-code>`. The Host confirmed RelayUnityTransport; no direct-IP fallback was used. First run: Host Fox / Client Monkey. Second: Host Monkey / Client Fox using the development Swap Animals field before creating. Each run had exactly two PlayerObjects.

The Client used its existing owner motor to cross both handoff routes and to push both right-hand crates. Logs `Temp/coop-client.log` and `Temp/coop-swapped.log` recorded both relay latches, right load 3 then 4, balanced 4/4 and zero lift offsets. Host read matching final completion and box positions. First run crates ended at local (2.5,0.5,2.9)/(4.5,0.5,3.2); second at (2.5,0.5,2.3)/(4.5,0.5,2.3). After Client walked off, load appropriately reduced to 4/2 without undoing completion. Host developer lights=True while the rejoined Client reported False. This proves local state separation; the headless Client cannot verify rendered night appearance.

Client process termination cleaned up after the existing 30-second transport timeout: server player count=1, all three latched attempts reset to false with revision increment. Recreated Relay session accepted the same independent Client profile without duplicate actors. Normal Host Leave returned Ready and NetworkIdle=True. These runs are same-PC Relay, not different-network acceptance.

Final local regression: validated Lever interaction; distinct-actor AND open; jump off cleared plate occupancy; airborne catch entered with vertical speed 0; zero input paused climb and reverse input exited. Two enabled cameras / one listener remained. Gate passage blocker held B at full open after early release, then closing completed when blocker left. Lift reset during unequal loading restored its crates/attempt without clearing the adjacent latched Switchback. The left crate begins descending again immediately after reset because its initial Weight 2 remains intentionally unbalanced.

Windows build succeeded. Initial attempt while Unity was compiling was rejected; retry after compilation succeeded. Existing ability-probe DEVELOPMENT_BUILD directive emits UAC0009 deprecation warning. Unity cloud diagnostic endpoints timed out in Client logs, while Sessions Relay and gameplay succeeded. No new gameplay exception was observed in either Client log.

Graphical executable automation: Computer Use app launch approval timed out. Therefore Windows build *rendered UI/split-screen* acceptance remains pending, separately from Editor screenshots and headless two-instance success. Android device touch/performance, friends across networks, graphical swapped-role night view, explicit Client-view reset/late-join snapshot matrix and exhaustive latency/crush cases remain pending. Do not relabel these as passed.

## Modified resources

- Scenes: Prototype_SplitScreen, Prototype_OnlineTest (Editor API; existing GUIDs retained).
- New code: CooperativePuzzle, CooperativePuzzleState, CooperativeWeightBox, CooperativePuzzleNetwork, opt-in CooperativePuzzleProbe, Editor CooperativePuzzleSetup.
- Changed existing code: PrototypeMotor (controlled push/ride/warp integration), PrototypeNetworkSession and NetworkPlayerPrototype (development animal assignment), PrototypeNightEnvironment and PrototypeAnimalVision shader (Lit pass and Fox A/B).
- New URP Lit materials: PrototypeCoopFloor, PrototypeCoopWall, PrototypeCoopAccent. Existing source materials retained; runtime copies implement animal vision.
- Lights: previous local lamp tuned, Lever Lamp and three bounded Route Lights added. Shared URP renderer/assets and package versions unchanged. Per-light shadow tier 512, existing PC shadow distance 50. Static geometry is layer 0, players layer 2, controlled crates layer 7 (excluded from support-ray self hits).
- Docs: T07_Night_Vision.md, this file, PROJECT_STATUS.md. Notion journal records decisions/evidence; code remains in repository.

Final audit: final Windows rebuild after sign-spacing changes succeeded. Current running Prototype_SplitScreen read-only reference/material audit is recorded separately below; the Editor unexpectedly re-entered Play Mode during final cleanup, so it was preserved rather than forcibly exited or switched. Base commit remains d289c71; all existing user changes remain uncommitted.

Read-only final result: running Prototype_SplitScreen had 0 Missing Scripts, 0 broken non-null serialized references, 0 missing/unsupported renderer materials, and exactly 3 CooperativePuzzle components. This is not an Android shader-variant or graphical Windows Player audit. No final forced scene switch was performed.
