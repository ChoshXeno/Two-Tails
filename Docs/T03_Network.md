# T03 local networking

## Open and run

1. Open `Assets/prototype/Scenes/Prototype_Network.unity` in Unity `6000.6.8f1`. The saved scene is ready to run. **Tools > Two Tails > Build Network Prototype** creates missing assets from the saved local greybox.
2. Open **File > Build Profiles**, select **Windows Network Prototype**, and make a Development Build in an output folder outside `Assets`. The separate **Windows** profile still points to the local greybox.
3. Enter Play Mode in the network scene and click **Start Host** with Address `127.0.0.1` and Port `7777`. Launch the Windows Player and click **Start Client** with the same destination. The Player runs windowed and continues to process networking in the background.
4. Use WASD or the on-screen stick in the focused instance. Drag the world to orbit and scroll to zoom. Use **Disconnect** to leave, then wait for the start buttons to become available.

## Package versions

| Component | Version |
| --- | --- |
| Unity Editor | `6000.6.8f1` |
| Netcode for GameObjects | `2.13.3` |
| Unity Transport | `6.6.0`, resolved transitively by Package Manager |
| Multiplayer Play Mode | `3.0.0` |

NGO also resolves Mono.Cecil `1.11.6`. Unity Package Manager generated the NGO default prefab list and Multiplayer Play Mode configuration. T03 uses direct loopback UDP, with no Multiplayer Services setup. The Client connects to the Host port from a transport-assigned local endpoint.

## Authority and scene boundaries

- One NGO `NetworkManager` uses client-server topology and Unity Transport. Approval reserves one of two server slots before accepting the request, including the Host. NGO's automatic `PlayerPrefab` path creates one PlayerObject per approved connection. Slots release on disconnect.
- The server assigns blue/red materials through a server-write slot variable and picks spawns at `(-1.5, 0.1, -4.6)` and `(1.5, 0.1, -4.6)`.
- Two interpolated owner-authoritative `NetworkTransform` components synchronize root XYZ and the Visual's local yaw. Only the owner enables input, the motor, and its CharacterController. A single scene camera binds to the local owner and returns to its menu pose when that owner despawns. No scene player or DebugPlayer2 is present.
- The network scene retains static greybox geometry and puzzle placeholders. Lever, plate, and door gameplay scripts are absent from this copy. Puzzle state replication belongs to T04.
- Only scene geometry collides with the local controller in the network scene. Remote avatars have no collision proxy, so network avatars may pass through each other. Owner-authoritative movement is for this trusted local test. The Host ends the session when it leaves.

## Acceptance checklist

- [ ] Host alone has one blue player. Joining gives both processes two players with matching positions, colors, and visible yaw.
- [ ] Move each owner independently, and rotate and zoom each camera. Only that instance's owner and camera receive its input. Verify the unfocused process keeps networking and its controls stay neutral.
- [ ] Disconnect and rejoin as Client. Check that the Host despawns the departed player and the replacement gets one red player. Stop the Host and verify Client can start again. Repeat three times.
- [ ] Attempt a third connection while both slots are occupied. Verify **Session Full** and a usable UI after rejection.
- [ ] Try a different port with no Host. Verify connection failure or timeout and retry on the correct port.
- [ ] Reopen the local greybox. Verify P1, DebugPlayer2, Lever, and the pressure-plate gate still work.

## Verification recorded

- On November 9, 2026, an Editor Host and a Windows x64 Development Player communicated over `127.0.0.1:7777`. A second Windows process was rejected with **Session Full**. The Windows processes ran in headless mode for this automated probe.
- The Host had one blue player before joining. Both peers reported the same two colors and ownership after joining. A late Client received the Host's already-changed position. Queued keyboard events moved only the corresponding owner. The other process received matching position and Visual yaw after interpolation. Scripted orbit and zoom changes stayed local to each process.
- Three Host/Client cycles passed. Each included Client disconnect, Host despawn, a fresh Client rejoin in the red slot, and Host shutdown. Both processes ended each cycle with zero players and the menu camera restored. The Client recovered after a wrong-port attempt before connecting on the correct port.
- Local greybox regression in the live Editor found two actors including DebugPlayer2. Both plates opened their door, leaving a plate closed it, and the Lever door operated independently.
- Final Editor inspection found zero Missing Scripts and dangling references. The network scene has one NetworkManager, camera, AudioListener and EventSystem, with no scene player or active puzzle components. The final cleanup probe destroyed the camera before disabling the HUD, with no Console error.
- A clean build from **Windows Network Prototype**, after removing the temporary probe, succeeded with zero build errors and warnings. Its local output is `Temp/T03Network/TwoTailsNetwork.exe` for this Editor session. Rebuild from the profile if Unity clears `Temp`.
- Pending interactive checks include two visible windows, real foreground/background input switching, and actual on-screen stick and pointer use in the network scene. Physical-device networking and touch are untested.
