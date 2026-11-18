# Active task — animal mechanics acceptance

**Status:** In progress. This carries forward the ongoing task recorded in `PROJECT_STATUS.md`; this tracker does not claim the task is complete.

## Goal

Finish acceptance of the existing animal swap, Otter swimming, Beaver dam, visibility, and reset mechanics without expanding into paused T07-D or T08 work.

## Scope

- Local and online interaction, controls, UI, animal identity/position exchange, swim/dam path, synchronized state and reset.
- Visual visibility/occlusion checks and regressions for jump, Monkey vine catch, Lever, and plate gate.
- Keep separate evidence for interactive, rendered online, physical-device, and different-network checks.

## Non-goals

- New puzzles, level flow, scope expansion, Unity/package upgrades, or unrequested gameplay changes.
- Treating headless runs or compilation as rendered, mobile, or real-network acceptance.

## Checkpoints

- [x] Implement ordinary-animal reveal and Beaver contextual controls, reversible dam slots, and dry access. C# compilation against cached Unity project references and static scene checks passed on November 18.
- [ ] Compare the new reveal against fixed scene lights and the Fox view in rendered gameplay.
- [ ] Verify the new loop locally, then with an independent client. Keep device and different-network checks separate.

- [x] Existing implementation and documented automated evidence reviewed; see [Animal Mechanics evidence](Animal_Mechanics.md).
- [ ] Complete local interactive controls/UI and animal exchange acceptance, including blocked landing and inventory restrictions.
- [ ] Inspect both rendered online views after exchanges; check concurrent peer requests, synchronized inventory/progress/water, and normal leave/rejoin.
- [ ] Check blackout, wall occlusion, labels/water visibility, and lit shadows across relevant camera angles and swapped identities.
- [ ] Run Android/iPhone touch, graphics/shadow, and performance checks on a physical device.
- [ ] Run the same relevant flow across different networks and regress jump, airborne vine catch, Lever, and distinct-player plate gate.

## Verified evidence

- **Passed:** C# compilation and final Windows Development Build; saved SplitScreen and OnlineTest scans had zero Missing Scripts/broken references; documented local mechanism probes and an independent Sessions + Relay headless Client lifecycle completed the dam/swim/reset path. Regression probes passed within their stated component/RPC scope.
- **Passed with scope limits:** matched-pose Editor image sampling showed Fox brightness changes did not affect Monkey's equivalent crop; custom shaders had zero shader messages; final local menu re-entry reported 2 actors, 2 cameras, 1 AudioListener, and fresh Console 0 errors/0 warnings.
- **Not run:** rendered two-window online acceptance, complete human-controlled playthrough, physical-device input/graphics/performance, and different-network play.
- **Failed/blocked:** No unresolved failed acceptance test is established. Two earlier automated Relay swap-back requests were rejected after long-distance positioning; the cause was not captured, and later focused and full runs passed. Computer-use approval timeout previously blocked graphical Windows executable automation; it is not evidence of a gameplay failure.

## Blockers

- November 18: Unity batch startup exited before project import. Unity shader import, Play Mode, and the updated Client probe still need to run. See [the tweak handoff](Animal_Mechanics.md#november-18-tweak-handoff).

- Physical-device and different-network acceptance remain pending; the available iPhone deployment path is not verified. Mobile additional-light shadow support differs from the PC profile.
- Abrupt Client process termination can leave temporary Sessions membership conflict (`SessionConflict`); normal Leave/Join passed. Seamless abrupt-disconnect recovery is outside this task.

## Next action

Open the project in Unity 6000.6.8f1 and import the changes. In Local Play, verify the night reveal and the Beaver pickup/build/reclaim loop, including walking the dry route at full water. Then repeat with a visible Host and Client and capture the two online views. Use the tweak handoff for the sequence and record outcomes before starting Rabbit or Elephant.
