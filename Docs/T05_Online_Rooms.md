# T05 online rooms

## Cloud and packages

The Editor is Unity 6000.6.8f1. The project resolves Multiplayer Services 2.4.1, Services Core 1.18.0, Authentication 3.8.0, NGO 2.13.3, and Unity Transport 6.6.0. Multiplayer Services is the only new direct package dependency.

The existing Unity Cloud link identifies **Two Tails**, organization `shentan2013`, project ID `f34ec8c6-da2d-4559-93a0-73aa6364f096`. The Network Session object's **Prototype Online Room** component explicitly uses the `production` environment and a 30-second startup timeout. Both peers must use the same saved scene and build. To verify setup, compare the Editor's **Project Settings > Services** with the existing project's **Development > Settings** and **Environments** in the [Unity Dashboard](https://cloud.unity.com/). Under that same project check **Player Authentication** and **Multiplayer** access for the selected environment, including the services used by Sessions and Relay. See [Authentication setup](https://docs.unity.com/en-us/authentication/get-started) and [MPS SDK setup](https://docs.unity.com/en-us/mps-sdk/install-and-upgrade).

## Local two-instance test

Use the complete Windows build folder at `Builds/T05_Windows` and run `TwoTailsOnline.exe`. The scene is already wired. **Tools > Two Tails > Configure Online Rooms** applies the setup to an older saved network scene.

1. Open `Assets/prototype/Scenes/Prototype_Network.unity` in Unity and enter Play Mode. Use **Online Relay**. In the Editor select **Create Room** and then **Copy Code** once it shows **Connected** and **Players: 1/2**.
2. In **File > Build Profiles > Windows Network Prototype**, make a Windows Development Build. Launch it using the command below, paste the room code into **Join Code**, and select **Join Room**. Verify that both screens show **Players: 2/2** and distinct short **Player ID** values. The identity uses the Authentication SDK's cached credentials for the chosen profile.
3. On the Client select **Leave Room** and rejoin the same code. On the Host select **Leave Room**, then confirm that the Client returns to the menu and the old code cannot join. Both sides can create or join again after shutdown.
4. Select **Direct Test** from the offline menu to run the previous loopback workflow on `127.0.0.1:7777`. Select **Online Relay** to return. This toggle is locked while either mode is starting, connected, or leaving.

The Editor defaults to `twotails_editor`, a Development Build defaults to `twotails_development`, and a regular build uses `twotails`. Override the profile before initialization via `-authProfile`. The profile names select cached identities, independent of whether the player hosts or joins. Use different profiles when running two Windows players on one machine:

```powershell
Start-Process -FilePath '.\TwoTailsOnline.exe' -ArgumentList '-authProfile test_host'
Start-Process -FilePath '.\TwoTailsOnline.exe' -ArgumentList '-authProfile test_client'
```

## Session lifecycle

Create uses a private two-player MPS Session with DTLS Relay. Join uses its case-preserved, trimmed Session code. The MPS SDK starts and stops the existing NGO NetworkManager and configures its Unity Transport. The online controller checks the configured Relay protocol and waits for both the NGO connection and the spawned local player before publishing **Connected**. NGO still approves at most two players and assigns their spawn slots. T03 movement and T04 puzzle authority use the same objects in both modes.

Cancel or timeout cancels the SDK request. The UI keeps the connection mode reserved until the request and any cleanup finish. Leaving invokes Session deletion for the creator and membership leave for the guest, then waits for NGO shutdown. Host loss ends the game. A cloud request may fail to confirm cleanup during an outage, and automatic game host migration is outside this milestone. A rate-limit response imposes a three-second cooldown before manual retry.

## Acceptance and cross-network test

- Create and copy a code. Join it from another identity, reject a third identity, and retry after a bad or deleted code.
- Leave as Client and rejoin. Leave as Host and verify the Client returns to the menu. Repeat for three rounds, including a new room after each Host exit.
- Quit the Host process during a room and verify recovery on the Client. Try offline, restore the connection, and retry.
- On both peers, verify independent input and camera, owner position and facing, each peer's Lever, two-player plate occupancy, passage obstruction, disconnect cleanup, and late-join state. Repeat the Direct test afterward.
- Zip the complete Windows Development Build folder, including the executable, its matching `_Data` folder, Unity player DLLs, and other files Unity generated. Extract the same build on both PCs. Use a home connection on PC A and a separate connection such as an iPhone data hotspot on PC B. Start both with distinct identities, keep the same Cloud Project and environment, and test the code flow. No port forwarding or firewall changes are needed.
- Cross-network testing requires two independent networks. iPhone deployment and touch validation require an actual installation.

## Evidence

- November 9, 2026. The live Editor signed in and created a room on the linked Two Tails project in `production`. In three Editor Host and independent headless Windows Development Client rounds, the two distinct identities used `RelayUnityTransport`, both peers reached two spawned players, and both recovered after each Host deleted the room.
- Verified Copy Code, invalid-code feedback followed by a successful retry, rejection of a third player as full, Client leave and rejoin, and rejection of the deleted room's code.
- Over Relay, a scripted owner pose replicated to the remote peer. Both owners operated the Lever. Separate plate occupancy opened its gate, occupancy in the passage blocked closing, Client leave cleared plate occupancy, and a rejoining Client received the current Lever door state.
- The Editor also joined a Windows Host. Killing that Host process returned the Editor Client to an actionable menu with **Host left the room**. Cancel and a deliberately shortened startup deadline returned the room UI to an actionable state. After Relay cleanup, the same two processes connected again over Direct using the `UnityTransport` protocol. The Direct Test menu locked mode switching while the Host was active.
- The probes used independent processes, the real services, and scripted pose changes and interactions. Two visible windows with keyboard, touch, and camera input, physical network loss and recovery, different networks, and iPhone deployment remain manual checks.
- The network scene contains one NetworkManager and one EventSystem and has zero Missing Scripts. The Windows Network Prototype Development Build succeeded with zero errors and warnings after the temporary QA probe was removed.
