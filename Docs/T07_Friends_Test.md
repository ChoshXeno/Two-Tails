# T07-A friends test

Two Tails - Friends Test / T07-A / 2026-11-10
Unity 6000.6.8f1; Windows x64 Development; app version 0.1.0.

1. Extract the ENTIRE archive to a writable folder. Keep TwoTails.exe,
   TwoTails_Data, UnityPlayer.dll and every other included dependency together.
2. Launch TwoTails.exe. Choose Online Play. One person chooses Create Room.
3. The other person chooses Online Play, enters the Join Code, then Join Room.
4. Test WASD/joystick movement, E/Interact lever, and the two pressure plates.
   Different players must occupy different plates. Leaving either closes the gate.
5. Leave Room / Main Menu; exchange Host roles and repeat. Either computer can host.
6. Report this build label, status/error text, steps, which player hosted, and a
   screenshot. Player.log is under %USERPROFILE%/AppData/LocalLow/<Company>/<Product>/.
   For a known log path, start: TwoTails.exe -logFile friends-test.log
   Never send authentication tokens or credential/cache files.
7. Different-network and Android-device testing have NOT been verified by this build.

Local Play is an offline development tool, not a second network Client.
P1: WASD, E, mouse drag on the left viewport, mouse wheel.
P2: arrows, 0/numpad0, numpad8/2/4/6 look, numpad9/3 zoom.
Space/Enter are reserved and do not jump yet. Only P1 supplies audio listening.
If simultaneous keys fail, report the exact keys and keyboard model; bindings can
be adjusted in the split input asset and Split Screen Inspector.

For two builds on ONE PC use different profiles:
TwoTails.exe -authProfile friend_a -logFile friend-a.log
TwoTails.exe -authProfile friend_b -logFile friend-b.log
Two separate PCs keep their own authentication caches; no cache is distributed.
No port forwarding or firewall disabling is required for Relay.

## Implementation

New scenes: Prototype_MainMenu, Prototype_SplitScreen and Prototype_OnlineTest under Assets/prototype/Scenes. The previous Greybox and Network scenes are unchanged. Select Windows Friends Test in Build Profiles before Editor scene transitions. Open Prototype_MainMenu. Build with Tools > Two Tails > Build Windows Friends Test. Create Test Entrances is non-destructive and skips existing scenes.

The offline scene reuses both existing local motors/interactors, Lever/Door/plate components; it never starts NGO or UGS. Stable local slots 0/1 correspond to Fox/Monkey. The online copy keeps existing ownership, Sessions startup, RPC validation and persistent puzzle state. No hybrid two-local-network-player mode exists.

Split Screen Inspector: players/cameras, normalized viewports, P2 Look/Zoom key bindings, lookSpeed 90 degrees/sec, zoomSpeed 3 metres/sec. Each FollowCamera retains pitch limits, distance limits and collision parameters. Vertical FOV remains 54 degrees and Camera.ResetAspect uses its actual viewport. HUD masks and mouse surface use those same viewport rectangles. TabletOpposed rotation and input transforms remain future work; no opposite-seat rendering is implemented.

P1 listens to audio for the development split mode; P2 has no AudioListener. Return waits for existing Session/NGO cleanup before loading the menu. Menu LocalAllowed guards the public entry and the offline scene itself in non-development builds. UI navigation is disabled, so arrows and Enter cannot also navigate/submit buttons. Shared keyboard actions do not use exclusive device pairing.

Reserved Jump bindings are in PrototypeSplitInput.inputactions, with no jump behavior. No climbing, night vision, weight puzzle or new puzzle layout was implemented.

## Verification

- Compile: zero errors/warnings observed before building. Windows Development Build succeeded.
- Local: observed two actors, two 50/50 cameras (aspect about 0.889 at 16:9), both FOV 54, one listener, no NetworkManager. Synthetic W and arrow input isolated correctly. Different actors on separate plates opened the gate; valid P1 and P2 Lever interactions accepted.
- Mode transitions: Local -> menu -> Relay Connected (one player) -> menu -> Local passed. Another local re-entry retained two actors, one listener and zero Missing Scripts. P2 camera probe advanced P2 yaw by 1.8 degrees at 0.02 seconds while P1 yaw stayed zero. The Editor had to be resumed from its background pause for this probe.
- Pending: physical shared-keyboard rollover combinations, mouse gestures at the midline, interactive window resizing, non-development executable gate test, complete two-instance Relay puzzle regression, different-network friends test, Android hardware. No physical network outage was induced; the offline path has no UGS or transport object.

No Unity/package upgrades or commits/pushes. Suggested commit: feat: add developer split-screen and friends test entry.

Archive: `Builds/TwoTails_Friends_T07A_Windows.zip`. Packaging checks CRC and includes executable, Data, native dependencies and README; Unity DontShip backup folders are excluded.

## File inventory

New runtime: `PrototypeTestMenu.cs`, `PrototypeSplitScreen.cs`. Extended shared components: `PrototypeFollowCamera.cs` (degree/metre input entry points), `PrototypeOrbitDrag.cs` and `PrototypeInteractionPrompt.cs` (explicit binding methods). New Editor builder: `PrototypeTestBuild.cs`. New split input asset and the three scenes/profile listed above include Unity-generated .meta files. Existing input asset, prefab, baseline scenes, package manifests and cloud settings are unchanged.

The initial executable was launched headless with no startup exception. The final executable was rebuilt successfully after the P2 adapter adjustment; its full interactive startup is pending.

Follow-up verification (2026-11-10): final rebuilt Windows executable launched headless; startup log had no exception. Main Menu screenshot inspected in Editor: Local Play, Online Play, Quit and version label visible without overlap. This does not verify interactive executable input or two-instance networking. Test processes stopped; Editor left out of Play Mode.
