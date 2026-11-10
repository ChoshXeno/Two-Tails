# T07-C: Level 1 night test and airborne wall catch

Updated 2026-11-11. Unity 6000.6.8f1; packages unchanged.

## Entry and configuration

Open `Assets/prototype/Scenes/Prototype_MainMenu.unity`. Local Play uses Prototype_SplitScreen; Online Play uses Prototype_OnlineTest. Both already contain the additions; no menu execution is required. `Tools/Two Tails/Configure Night Test` is the repeatable Editor setup entry. Baseline Greybox/Network scenes are retained.

On `Level 1 Night Test`, PrototypeNightEnvironment exposes Monkey Visible Radius (3 m), Fade Width (3 m), Night Brightness (0.8), shader and developer daylight reference. Each render camera uses its followed animal, not network role. World-space distance is measured from actor feet, independent of camera orbit/zoom. Normal additional lights contribute separately from animal ambient visibility, with URP shadows and normal geometry depth occlusion. Runtime material copies are confined to these test scenes; source materials and other scenes' ambient settings are not changed.

Editor/Development: P or Developer Lights switches both local split views; in Online it affects only this process. New scene instances start at night. Release runtime guards remove the control and reject Toggle calls. Actual release-executable testing remains pending.

Lamp: (6,5,-3), downward spot, range 9, intensity 14. Lit landmark: (7,0.75,-3). Unlit landmark: (-8,1.5,5). Suspended Air Catch Vine: (0,2.7,0), size (2,2.4,0.5), bottom 1.5. Landing: (0,3.93,1); platform top 3.9. Existing grounded vine and normal wall remain.

PrototypeMotor retains climb distance 0.4 beyond controller radius, angle 45 degrees, speed 2 m/s, and jump height 1.2. Airborne entry uses the existing explicit surface/front/facing/input checks, clears pending jump and vertical velocity, and reuses pause, reverse-exit, landing-clearance and reset rules. Owner movement and existing motion replication are unchanged; this is not server-authoritative movement validation.

## Touch HUD correction

The T07-B setup searched for an OnScreenButton, but this scene's Interact is a normal uGUI Button. Inspection found that Jump had not actually been created despite the earlier handover claim. This task adds/reuses one Jump Button using PrototypeScreenButton and Gameplay/Jump's `<Gamepad>/buttonWest`. It is shown only with the local player, shares SafeArea, and is offset (-190,+190) from Interact, with 174x174 hit area. No duplicated Interact click callback remains on Jump. Existing per-pointer ownership and camera UI exclusion are reused.

## Executed evidence

- Editor compilation: zero C# errors/warnings after environment changes. ShaderUtil reported no shader messages during rendering.
- Actual Play Mode rendered same-position/same-camera comparison: Fox sees the distant unlit scene; Monkey fades to black while separately lit geometry remains visible. Developer daylight makes both views clear. Captures: `Temp/T07C_compare_night.png` and `Temp/T07C_compare_day.png` (ignored QA output).
- Runtime component probes against actual scene colliders: ascent velocity +5 and descent -5 both entered Climb and became velocity 0. Reset cleared climbing. Suspended-wall entry from ground, Fox entry, back entry and side entry all returned false. These are component probes, not physical input or complete end-to-end traversal tests.

## Manual acceptance still required

- Mouse/touch Developer Lights toggling, return-to-menu night reset, camera orbit/zoom with the same landmark.
- Jump with joystick held; no accidental Interact or camera drag. Actual Android/iPhone multi-touch is unverified.
- Full airborne jump-to-wall, pause, reverse exit, blocked landing and reset with human controls on Host and Client.
- Relay, friends across networks, existing Lever/plate regression and platform rendering/performance. Earlier task acceptance is not a T07-C regression result.
- Android physical device: not run. iPhone physical device: not run.

## Limits and deferred tablet layout

The shader intentionally supports current opaque, plain-color MeshRenderer placeholders. Future textured/skinned/transparent art needs an explicit rendering integration pass. Fox night vision does not see through opaque geometry. No new network puzzle rules are introduced.

Future tablet only: left/right 50/50; P1 left rotates clockwise 90 degrees, P2 right counterclockwise 90 degrees. Each has Joystick, Interact and Jump; shared Menu at top center. HUD and touch coordinates must rotate consistently with each player's view. This task retains desktop split layout and does not implement tablet opposed controls.

## Files

New: PrototypeNightEnvironment.cs, PrototypeAnimalVision.shader, PrototypeNightSetup.cs and their Unity metadata.
Modified: PrototypeMotor.cs, PrototypeFollowCamera.cs, NetworkPlayerPrototype.cs, PrototypeNetworkPuzzleHud.cs, opt-in PrototypeAbilityProbe.cs, Prototype_SplitScreen.unity, Prototype_OnlineTest.unity, this document and PROJECT_STATUS.md. Existing uncommitted T07-B files remain intact.

## Windows independent-instance result (2026-11-11)

Windows x64 Development build succeeded at `Builds/NightTest/TwoTails.exe` (distribute the entire folder if needed, not just exe). Build emitted UAC0009 for the existing DEVELOPMENT_BUILD guard in the opt-in ability probe; it is a deprecation warning, not a build failure.

An Editor Direct Host and a separate headless Windows Client ran the same scenes/code. Client launched with `-abilityProbe -airCatchProbe`, moved toward the suspended wall, requested Jump and reached the landing: `entered=True maximumY=3.93`, `species=Monkey y=3.93 climbing=False`. Host inspection observed owner 1 Monkey at `(0,3.93,1)`, alongside owner 0 Fox. This verifies an independent Client traversal and final replicated position, not graphical rendering on the headless Client or every intermediate frame. Client log: `Temp/T07C_client.log`. Unity cloud diagnostic endpoints cdp.cloud.unity3d.com and config.uca.cloud.unity3d.com timed out; no ability exception was logged. This run did not use Relay.

Saved scenes each had zero Missing Scripts and exactly one night environment. Connected HUD inspection confirmed active Jump, buttonWest binding, 174x174 area, (-190,+190) offset and no inherited uGUI Button click callback. Physical multitouch remains pending. Temporary QA Client stopped, Editor returned to MainMenu outside Play Mode. No commit/push.

## T07-C lighting supplement (2026-11-11)

Confirmed root cause of the flat picture: the previous shader assigned `visibility * 0.8` equally to every surface before adding direct lights. Fox forced visibility=1, so its whole view received that large, normal-independent fill. Source geometry was converted at runtime to this shader, even when the saved material was URP Lit. It was not evidence that Unity had no lights. Observed scene: Directional Light intensity 1.25 (disabled in night by environment), spot intensity 14, skybox ambient intensity 1, no assigned Lighting Settings asset. The shader did not read ambient/reflection probes, emission or a fixed exposure, so those were not the confirmed source of its constant fill. No exposure workaround was introduced.

The same GUID shader now uses URP UniversalFragmentPBR, low cool normal-dependent hemisphere GI, direct lights and shadow reception. Night Brightness is 0.28, Night Tint (0.38,0.52,0.75); source materials remain intact and runtime copies preserve greybox colors. World-space fonts are excluded from material replacement. Existing spot intensity is 22; added Lever Lamp (-1,5,4), range 7, intensity 18. Cooperative route lamps use range 22/intensity 45. Five local shadow maps use per-light tier 512 rather than expanding the shared atlas. PC URP already supported additional shadows; Mobile URP does not, a documented remaining platform difference.

Development A/B: **N** or **Fox Vision A/B (N)** toggles only Fox's camera visibility rule. **P / Developer Lights** remains a separate local-only daylight override. Both reset on new scene entry; release guards reject both development toggles. At matched actor/camera positions, captures `Temp/T07_Lit_AB.png` and `Temp/T07_Lit_AB_off.png` show shape shading/shadows, Fox-only distant visibility when enabled, and matching base views when disabled. No camera-wide exposure or screen overlay is used. Camera-global shader uniforms are set at beginCameraRendering and cleared at end, per sequential URP camera render; XR/multiview rendering is not validated.

Actual test entry scenes and cooperative configuration are documented in [T07 cooperative tests](T07_Cooperative_Tests.md). The old T07-C screenshot is historical; use these Lit-pass captures for current appearance.
