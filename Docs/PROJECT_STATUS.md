# Two Tails project status

Last updated November 9, 2026 (Malaysia time).

## Goals and development environment

Two Tails is a mobile two-player cooperative animal puzzle game. Players combine complementary animal abilities to solve environmental puzzles and rescue wildlife. The [Project Roadmap](https://app.notion.com/p/4120be84637f81958a18f28aabcaf369) describes the prototype scope and task order. The [Agent Operating Guide](https://app.notion.com/p/4120be84637f8140aa04c1aea2b376a7) describes where to save project decisions and development journals.

- **Verified in the repository.** Unity `6000.6.8f1`, revision `237b6593792b`, in `ProjectSettings/ProjectVersion.txt`. The project uses Universal RP.
- **Reported by the user.** Development on Windows 11 with JetBrains Rider. The available test phone is an iPhone. The Rider package is verified below. Rider configuration and the iPhone deployment path have yet to be verified.

### Package inventory

Checked `Packages/manifest.json` and `Packages/packages-lock.json` on November 8, 2026. Installed versions match in both files.

| Package | Status |
| --- | --- |
| Input System (`com.unity.inputsystem`) | Installed `1.20.1` |
| Universal RP (`com.unity.render-pipelines.universal`) | Installed `17.6.0` |
| JetBrains Rider integration (`com.unity.ide.rider`) | Installed `3.1.0` |
| Unity Pipeline (`com.unity.pipeline`) | Installed `0.10.0-exp.1` |
| Netcode for GameObjects (`com.unity.netcode.gameobjects`) | Missing from both files |
| Unity Transport (`com.unity.transport`) | Missing from both files |
| Multiplayer Services (`com.unity.services.multiplayer`) | Missing from both files |
| Multiplayer Play Mode (`com.unity.multiplayer.playmode`) | Missing from both files |

## Completed work and evidence

**T01 — Project foundation: reported complete by the user.**

- **Implemented in the repository.** The Unity project, URP settings, initial scene, and Git history are present. `ProjectSettings/EditorBuildSettings.asset` includes `Assets/Scenes/SampleScene.unity` as an enabled scene.
- **Verified in the November 8 live Editor check.** Unity Pipeline returned `SampleScene` as the active scene, with `Main Camera`, `Directional Light`, and `Global Volume` as roots. The Editor was idle, reported no script compilation failure, and showed zero current Console errors.
- **Verified in the Editor log.** `Logs/Editor.log` recorded a successful Windows player build at 10:02 PM Malaysia time on November 8. Launching that build and opening a Unity script in Rider were not independently tested during these checks.
- **Git reference.** Local `main` and the cached `origin/main` reference both pointed to `f2640f5` (`Add: Added folders for organization`). The remote was not queried.

## Current task and outstanding work

**Prototype — Local greybox.** Implemented. The scene has independent lever and two-plate doors, a P1 orbit and zoom camera, an English HUD, and an Editor/Development-only P2 test character. See [Prototype Greybox](Prototype_Greybox.md) for configuration, asset conventions, and acceptance checks.

- **Implemented.** The lever toggles its door and the plates use distinct-character AND occupancy to control a separate door. Both doors reverse smoothly. The camera supports wheel and pinch zoom without changing FOV.
- **Verified.** Current Play Mode probes passed the plate truth table and occupancy cleanup, lever input and reversal, independent doors, panel collider recovery, and desktop wheel zoom with UI isolation and obstruction recovery. A prior non-development Windows player launched the prototype with only one active character and no active P2.
- **Awaiting testing on a physical device.** Confirm the iPhone build, signing, and installation path. Verify pinch, simultaneous touches, the safe area, and background/resume.
- The prototype uses installed Input System `1.20.1` and uGUI `2.6.0`. The missing multiplayer packages belong to later milestones.

## Next steps and acceptance criteria

1. Open Prototype Greybox and complete the manual acceptance checks in [Prototype Greybox](Prototype_Greybox.md).
2. Verify the physical iPhone controls and record the device and test results. T03 is a separate task.
