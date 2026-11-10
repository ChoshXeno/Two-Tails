# T07-B animal identity, jump and automatic climb

Updated 2026-11-10. T07-A acceptance is reported complete by the user. No Unity/package upgrades, commit or push.

## Open and configure

Use Windows Friends Test profile and Prototype_MainMenu. Local Play loads Prototype_SplitScreen; Online Play loads Prototype_OnlineTest. Both existing entry scenes were updated through Editor APIs. `Tools > Two Tails > Configure Animal Test` installs missing test geometry/UI without replacing existing geometry. Existing .meta GUIDs were preserved. Baseline Greybox/Network scenes were not rebuilt.

## Changed files

- PrototypeAnimal.cs: Fox/Monkey identity, Weight 2, motion enum and event for optional model presentation. An Animator is not required. Idle/Run/Jump/Fall/Climb are driven; Interact/Ability are reserved presentation states, with the existing input interaction event available for animation wiring.
- PrototypeClimbSurface.cs: explicit front normal and top landing marker. Green is presentation only.
- PrototypeMotor.cs: jump queue, gravity/head collision, grounded-only climb entry, pause/reverse/top clearance, collision-respecting landing Move and reset cleanup. Same motor serves offline and owned online players.
- PrototypeLocalInput.cs: Jump action callbacks, focus/menu/pause/disable input clearing.
- NetworkPlayerPrototype.cs: server slot assigns Fox (slot 0) or Monkey (slot 1); owner movement presentation state replicates. Server retains sole identity assignment; owner retains movement authority. Client already receives Monkey by default, so a second instance joining is the Client-Monkey test path. No host migration or movement anti-cheat was added.
- TwoTailsInput.inputactions and PrototypeSplitInput.inputactions: Gameplay Space + Gamepad buttonWest, local P2 Enter/numpadEnter. Existing Move/Interact bindings retained.
- NetworkPlayerPrototype.prefab: identity component added. Logic root, CharacterController and Visual remain separate.
- Prototype_SplitScreen / Prototype_OnlineTest: animal test geometry; local identity; online Jump button reuses pointer-owned PrototypeScreenButton and buttonWest action.
- PrototypeAbilitySetup.cs: idempotent Editor configuration.
- PrototypeAbilityProbe.cs: development-only, command-line opt-in `-abilityProbe`; normal gameplay never runs it. Used for independent Client QA.

## Inspector and geometry

On each PrototypeMotor (including network prefab): Jump Height 1.2m, gravity 24m/s², Climb Distance 0.4m beyond controller front edge, Climb Angle 45 degrees, Climb Speed 2m/s. Visual is the existing independent rotation/model child. On PrototypeAnimal, select offline Species; Weight is fixed at 2. Online assignment overrides offline selection.

Vine Wall centre (-6,1.5,1), size (2.5,3,0.5), outward local front -Z. Landing feet marker (-6,3.03,1.9). Top Platform centre (-6,2.75,2.5), size (2.5,0.5,2.5). Normal Wall centre (6,1.5,1), same size. Jump Step centre (5,0.4,-3), size (2,0.8,2). All static geometry uses existing default solid collision layer. Three-metre platform exceeds 1.2m jump height, even from the isolated 0.8m step.

Climb only enters grounded while Monkey faces the surface front and moves inward. Release holds height; reverse releases into gravity. Jump is ignored during climbing. Target capsule space and supporting floor must exist; transfer uses CharacterController.Move, so an intermediate obstacle also blocks transfer. Reset/disable clears climb and queued input. No coyote time, jump buffering, wall jumping or arbitrary animal skills were added.

## Observed verification

- Compilation: zero errors and warnings; matching Windows Development build succeeded with zero build errors.
- Local component probes: jump apex rise about 1.22m; registered ceiling stopped upward velocity with maximum feet height 0.21m. Monkey entered, held height with zero input and exited on reverse. Fox could not climb; normal wall and vine back-face rejected. Blocked top held climbing and allowed reverse exit. Falling below reset height cleared climb.
- Input: actual InputAction callbacks from injected Space and Enter each performed once; held state did not repeat. Editor keyboard device had to be enabled/focused for automated input. Direct Motor calls were not treated as key verification. Physical keyboard rollover and touchscreen remain manual.
- Plate: after jump and real physics updates, actor held at about 0.81m had zero plate occupancy. Same-frame repeated motor calls produced stale overlap queries and were not accepted as evidence.
- Scene: zero Missing Scripts and one AudioListener observed in split screen.
- Independent instances: Editor Direct Host plus Windows Development Client; Client assigned Monkey, entered climb and reached landing at (-6,3.03,1.9). Host observed the same Client landing and identity. This is same-PC Direct network evidence, not cross-network or Relay acceptance.

## Reproduce independent Client test

Build scenes Menu/SplitScreen/OnlineTest together. The QA build is `Builds/AnimalTest/TwoTails.exe` (Git ignored). Start Direct Test Host in Editor using OnlineTest, port 7777. Launch matching player with `-batchmode -nographics -abilityProbe -logFile <absolute-log-path>`. Probe joins as Client, drives the owner motor toward the vine and logs entry/maximum height/landing; it does not bypass NGO spawning or ownership. Close the QA process afterward. Logs used here are in ignored Temp/animal-client-landing.log.

## Remaining manual/physical acceptance

- Interactive hold/release, Space/Enter/numpadEnter, oblique approaches and camera-relative input at arbitrary orbit angles.
- Actual mobile Jump + stick simultaneous touches, Android installation and performance.
- Both players jumping/falling and all climb edge cases through Relay, then friends on different networks.
- Full Lever/two-plate/anti-crush and menu-return regression on the final build (existing authority code remains unchanged).
- Replace placeholder models and bind optional presentation events; no animation clips supplied.

Owner-authoritative movement is still trusted for replicated positions and presentation state. This is not server-side movement validation or anti-cheat. No night vision, development lighting or new puzzle mechanics were implemented.

Final collision-transfer build was retested: Client entered climb, reached y=3.03 and exited climbing; Host observed (-6,3.03,1.9). QA processes stopped, Editor returned to Main Menu outside Play Mode. Temporary focus/input/time overrides were restored.
