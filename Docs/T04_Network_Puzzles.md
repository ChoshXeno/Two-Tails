# T04 network puzzles

## Run

Open `Assets/prototype/Scenes/Prototype_Network.unity`. With T05 or later, select **Direct Test** on both instances to use this loopback test. In the Editor select **Start Host**. In a Windows Development Player from **Windows Network Prototype**, select **Start Client**. Use `127.0.0.1:7777` on both. Use E or **Interact** beside the lever. Stand on separate plates to request the pressure gate to open.

The scene is configured. **Tools > Two Tails > Configure Network Puzzles** applies the setup to an older saved network scene. The local greybox and its Windows profile remain the baseline.

If the Host logs `NetworkConfig mismatch`, check the Client executable path in the first line of its Unity player log. T04 enables NGO scene management for the networked puzzles. A T03 player built with scene management disabled cannot join a T04 Host. Use `Temp/T04Network/TwoTailsPuzzles.exe` for the current local build, or rebuild from **Windows Network Prototype** into a new output folder. Start the Host before clicking **Start Client**. If UDP port 7777 is already bound by the intended Host, use **Start Client** in the second instance.

## Wiring and authority

- The local input raises one interaction event per press. The owner player sends a NGO 2.13.3 server RPC with a `NetworkObjectReference`. The server checks the real sender against the connection's current PlayerObject, resolves the lever, and checks range, horizontal Visual facing, line of sight, and the lever's cooldown. Rejections target only the requester. The Host's own interaction takes the same path.
- The Lever changes only `LeverDoor`'s server-write door state. Its handle takes the pose implied by that state. The two-plate AND gate writes only `PressurePlateDoor` and replicates one bit per occupied plate.
- A door stores its requested state, blocked flag, starting progress, and server start time in a `NetworkVariable`. Each peer computes travel from NGO server time and applies it to the same panel and BoxCollider. Scene-spawned objects apply the initial state and subscribe for updates. Despawn removes subscriptions and resets the local visuals.
- Closing occupancy is computed by the server. The demand becomes closed immediately when the AND result is false. The panel freezes at its current travel while a character occupies the passage, and resumes from that progress when clear. The stable frame holds the NetworkObject and the panel has no NetworkTransform.
- The Host CharacterController and a trigger-only server proxy for the remote Client are sampled using physics overlaps. Plate occupancy is rebuilt each fixed step, with colliders deduplicated by current PlayerObject ID. One player touching both volumes still counts as one player.

The Host validates against the position and Visual yaw it has received from the owner's NetworkTransforms. This may lag the owner's view. It does not provide server-authoritative locomotion, reconciliation, or anti-cheat. The sensor proxy does not add solid player-to-player collisions.

## Inspector defaults

| Object | Position and settings |
| --- | --- |
| Lever | `(-2, 0, 3.5)`, chest-to-point range 1.5 m, horizontal cone 45°, server cooldown 0.25 s. LOS includes Default (layer 0) and ignores triggers and the actor and lever hierarchies. |
| LeverDoor | `(0, 0, 5)`, raises panel 3 m with 0.6 s full travel. |
| Plate A and Plate B | `(3.2, 0, 3.35)` and `(6.4, 0, 3.35)`. Trigger size `(1.5, 0.44, 1.5)` m, centered 0.18 m above the floor. Yellow/green mesh with 0.025 m visual depression. |
| PressurePlateDoor | `(4.8, 0, 4.8)`, same travel settings as LeverDoor. |
| Passage and player proxy | Each passage is `(2.4, 2.4, 1.2)` m, centered 1.2 m above the frame origin. Plate and passage masks include Ignore Raycast (layer 2). The server-only remote trigger capsule copies the owner's controller dimensions. |

## Acceptance

### T04-A

- Host opens and Client closes. Check range, facing, occlusion, near-simultaneous requests, and feedback isolation.
- Join and rejoin while the lever is open. Verify the correct initial panel position and handle pose.
- Check the original local Lever independently.

### T04-B

- Solo on either plate, both players on one plate, and one player's multiple colliders on both plates all keep the gate closed. Different players on separate plates open it, including after swapping.
- Leave a plate, quit the Client process while on it, and reconnect. Verify that old occupancy clears. Watch both visuals and both door positions.
- Reverse during travel. Occupy the passage when the gate loses demand, then leave. Both peers should freeze and resume at matching progress.
- Reopen the session three times. Verify the independent Lever state, current plate state, and no stale occupancy or duplicated scene objects.

## Evidence

- **T04-A, November 9, 2026.** The live Editor Host and an independent headless Windows Development Client verified Host open, Client close, matching panel and lever poses, rejection by range/facing/occlusion, requester-only feedback, the cooldown, late join, Client rejoin, and Host shutdown. The relocation probe waited for the Host to receive the Client's pose before pressing. Local Lever and DebugPlayer2 passed in the original greybox.
- **T04-B, November 9, 2026.** The two processes verified solo A and B, two actors on A, separate and swapped occupants, one actor with colliders on both plates, and removal of an extra collider. Both peers agreed on the yellow/green plate visuals and gate target. A remote actor in the passage paused both panels at matching progress while the gate's demand was closed. Leaving resumed the close. A fast exit reversed the gate from mid-travel.
- Quitting the Client process on a plate cleared the old occupancy. A new process joined with the current plate and door states. Three disconnect/rejoin rounds agreed on both independent mechanisms. Two Host restarts reset both puzzle targets and all occupancy.
- **Editor input regression.** Injected Input System events verified WASD, on-screen stick movement without camera rotation, world orbit, wheel zoom, and holding E without repeated toggles on the network Host. The independent Windows probes used headless players and scripted positions. Physical-device touch, two visible windows, and joining during an in-progress transition remain manual checks.
- **Final scene scan.** Zero Missing Scripts and zero dangling serialized references in the network scene and player prefab. Scene management is enabled. The three in-scene NetworkObjects are Lever, LeverDoor, and PressurePlateDoor. The network scene has zero enabled local puzzle writers. The original greybox still opens its independent doors and has no NetworkObjects.
- **Clean Windows Development Build.** The final build from the existing Windows Network Prototype profile succeeded with zero errors and warnings after removing the temporary probe. Local output for this Editor session is `Temp/T04Network/TwoTailsPuzzles.exe`.
