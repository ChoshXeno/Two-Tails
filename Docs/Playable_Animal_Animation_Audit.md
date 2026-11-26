# Playable animal animation audit and first-pass direction

26 November 2026 (Malaysia time). Source: [Notion animation handoff](https://app.notion.com/p/4240be84637f8156afd0f4b3f24024ca), inspected project files, Blender 5.3 FBX imports, and Unity 6000.6.8f1 Editor import checks. This is an asset and mechanism audit, not gameplay animation acceptance.

## Status key and scope

- **已有可用**: a matching clip, loop configuration, and playable presentation verified in this project. None yet.
- **需修复**: a source candidate or new test action exists, but needs retiming, visual correction, importer setup, or gameplay wiring before it is usable.
- **需制作**: no matching action found on that animal's own rig.
- `—`: no current gameplay trigger; create only if the mechanic is approved or implemented.

The two saved player Prefabs (`Assets/prototype/Prefabs/PrototypePlayer.prefab`, `NetworkPlayerPrototype.prefab`) still use a capsule `Visual` and contain no Animator. `PrototypeAnimal.MotionChanged` and the replicated `PrototypeMotion` byte have no Animator listener. The current enum is Idle, Run, Jump, Fall, Climb, Interact, Ability, Rope; the motor emits Idle/Run/Jump/Fall/Climb/Rope. `Interact` and `Ability` are not emitted by the inspected interaction code. No walk speed tier exists: `Run` means any movement input at a configured 4 m/s. Swimming, grip pushing, and pulling currently have no distinct motion enum. Root position is driven by `CharacterController`; new clips are in place.

All six FBX imports have one skinned mesh and one `purebind_armature`, are imported as Generic, and the original importers use `NoAvatar`. Fox (43 bones), Monkey (49), Otter (29), Beaver (41), Rabbit (41), and Elephant (41) each have a 1–2-frame `rest` source Action. There are no original `.blend`, standalone `.anim`, or `.controller` files in the repository. The source FBX files contain packed textures, but external texture names referenced by Blender are absent for Fox/Monkey/Otter/Elephant. The new test FBXs embed their packed textures. Meshes retain one material; skin weights were present through the Armature modifier. No source file, original importer, scene, or playable Prefab was modified.

## Clip-by-clip inventory

The table uses `修` = 需修复, `制` = 需制作, `—` = trigger absent. Every `修` entry below still fails the stricter **已有可用** definition above.

| Clip | Fox | Monkey | Otter | Beaver | Rabbit | Elephant |
| --- | --- | --- | --- | --- | --- | --- |
| Idle | 制 | 制 | 制 | 修: `wolf_idle` | 修: `rabbit_Idle`, alert/lie variants | 制 |
| Walk/Move | 制 | 制 | 制 | 制 | 修: `rabbit_ANIM_Rabbit_Walk` | 制 |
| Run | 制 | 制 | 制 | 修: `fox_mon_xiaohuli_01_run` | 修: `rabbit_ANIM_Rabbit_Gallop` | 制 |
| Jump_Start | 制 | 制 | 制 | 修: `wolf_jump_start` | 修: split `rabbit_ANIM_Rabbit_1Hop` | 制 |
| Jump_Air | 制 | 制 | 制 | 修: `wolf_jump_loop` | 修: split `rabbit_ANIM_Rabbit_1Hop` | 制 |
| Jump_Land | 制 | 制 | 制 | 修: `wolf_jump_end` | 修: split `rabbit_ANIM_Rabbit_1Hop` | 制 |
| Interact | 修: isolated test imported; not wired | 制 | 制 | 制 | 制 | 制 |
| Death | 制 | 制 | 制 | 修: `wolf_dead` | 修: `rabbit_ANIM_Rabbit_Death` | 制 |
| Push | 制 | 制 | 制 | 制 | —: grip excludes Rabbit | 制 |
| Pull | 制 | 制 | 制 | 制 | —: grip excludes Rabbit | 制 |

| Exclusive clip | Status | Evidence and issue |
| --- | --- | --- |
| Fox `Bite_Hold` | 需制作 | The Fox hierarchy has no jaw bone. A closed-mouth hold plus a separately parented gameplay mouth socket may work; real opening needs a carefully isolated rig or shape-key change. The vertical lever/bite trigger is not present in code. |
| Monkey `Wall_Hang_Idle` | 需制作 | Wall approach and climb are implemented, but the motion enum has no hang/edge idle state. |
| Monkey `Climb_Loop` | 需修复 | New isolated first-pass clip imports and loops, but the Blender preview reads too much like quadruped stepping. Wall alignment and gameplay wiring need work. |
| Monkey `Climb_End` | 需制作 | The motor can leave the wall edge or jump release; there is no end timing event. |
| Monkey `Vine_Hang_Idle` | 需制作 | Rope grab and swing exist; rope uses one broad `Rope` motion state. |
| Monkey `Vine_Climb` | 需制作 | Rope depth movement exists; its climb speed/zero-input distinction is not exposed to animation. Swing needs a separate visual decision. |
| Otter `Swim_Start` | 需制作 | Water entry is detected, but not exposed to animation. |
| Otter `Swim_Loop` | 需制作 | Horizontal swimming works; no swim motion state. |
| Otter `Swim_Stop_Idle` | 需制作 | Surface floating is tracked by `PrototypeSwimmer`. |
| Otter `Dive_Stop_Idle` | 需制作 | Underwater depth hold is implemented, without an animation signal. |
| Otter `Dive_Loop` | 需制作 | Held Dive changes depth; no animation signal. |
| Beaver `Wood_Pickup` | 需制作 | Pickup is an authoritative Interact result. |
| Beaver `Wood_Place` | 需制作 | Deposit and reclaim use authoritative Interact results. |
| Elephant `Strike` | 需制作 | Interact near `Strike Board` breaks a brown wooden barricade in the Rabbit crawlspace course. Result is immediate, without a strike timing event. |

Beaver's six animated FBX Actions retain `wolf_*` and fox/monkey source names. The matching bone hierarchy does not establish animal-specific quality. Rabbit has seven imported actions, including one whole hop and a walk; none have loop flags set in Unity. They remain candidates to inspect and repair, not accepted clips. Fox, Monkey, Otter, and Elephant have only a rest pose in their original FBX. Fox's rendered test shows ears, tail, and scarf; Monkey's rendered test shows a long tail, and its hierarchy has jaw bones. Beaver's requested right rear bandage still needs visual deformation checks.

## Proposed key poses and trigger points

These are first-pass directions without reference video. Keep feet and contact points on the mesh's own rig; gameplay moves the root.

| Action | Key poses | Trigger and timing |
| --- | --- | --- |
| Idle, all six | Balanced four-paw stance, subtle ribcage breath, small ear/head/tail motion; Beaver's bandaged right hind leg carries slightly less weight. | Grounded with no movement, interaction, or grip. Loop quietly. |
| Walk/Move, all six | Alternating diagonal contacts; Rabbit shorter forepaw placement and stronger hind extension; Elephant slow planted foot roll. | A future low-speed tier, if introduced. Current binary movement needs a blend-speed decision; do not substitute Run. |
| Run, all six | Fox/Monkey/Otter/Beaver springing four-beat gait, Rabbit bounding gallop, Elephant shorter heavy stride; return cycle at a matching foot contact. | Current nonzero movement currently maps to `Run`; speed variation/blend requires new parameter. |
| Jump_Start, all six | Hindquarter compression, forepaw lift, hind-leg push. Rabbit pushes strongly with both hind legs and reuses a shorter version on double jump. | On accepted jump input at the same frame as motor impulse; anticipation must not delay physics. |
| Jump_Air, all six | Forelegs gather, back lengthens, hind legs trail then prepare to tuck; ears/tails follow. Rabbit has distinct second-jump extension. | While airborne, including apex/downward phase, with a vertical-speed blend. |
| Jump_Land, all six | Forepaw touch, hind-leg catch, brief shoulder/hip compression, head settle. Elephant compresses less and absorbs weight through trunk/head. | On grounded transition; interrupt safely if another jump is accepted. |
| Interact, all six | Head locates target, near forepaw reaches/touches, recoil to stance; Elephant uses trunk contact for ordinary interaction. | On accepted server/local interaction result, aligned to contact frame. Fox test action is this proposal. |
| Death, all six | Brief destabilization, paws lose support, body folds, then stillness; avoid violent flips. | Only if a death hold is introduced before return to spawn. Current out-of-safety reset is immediate. |
| Push, eligible animals | Front paws press, shoulders lean, hind feet step forward in place; Elephant lowers head/shoulder. | While gripping and forward input actually moves a block, or while Elephant heavy push is accepted. |
| Pull, eligible animals | Front paws brace/grip, body shifts back, hind paws retreat in place. | While gripping and backward input actually moves a block; Rabbit is excluded by current grip rule. |
| Fox Bite_Hold | Muzzle approaches vertical rod, mouth meets socket, forelegs brace, neck and tail settle into small hold loop. | Only after a lever bite interaction and held state exist. Current rig cannot open the mouth independently. |
| Monkey Wall_Hang_Idle | Forepaws at wall grips, hind paws lightly braced, shoulders carrying weight, tail counterbalances. | On wall attach with no climb input. |
| Monkey Climb_Loop | Alternating forepaw reach and hind-paw push, torso rises without root translation, brief four-point contact before side swap. | While attached to a wall and climbing; reverse or retime for descent. Current test needs stronger vertical body orientation. |
| Monkey Climb_End | One forepaw finds top, chest clears ledge, hind legs step over, settle to four-paw stance. | On motor top-edge exit, not merely loss of wall collision. |
| Monkey Vine_Hang_Idle | Both forepaws grip rope, hind feet tuck and separate, body follows pendulum with tail delay. | Rope attached, no depth input; swing angle can drive additive sway. |
| Monkey Vine_Climb | Alternating hand-over-hand pulls, hind feet pinch rope, tail balances. | Rope depth actively changes; distinguish from pendulum drive. |
| Otter Swim_Start | Forepaws enter, body levels, hind feet sweep and tail follows. | Water entry, first surface travel beat. |
| Otter Swim_Loop | Side-to-side tail propulsion with alternating hind feet and low forepaw strokes. | Surface movement. |
| Otter Swim_Stop_Idle | Body floats level, slow bob, tail and paws make small station-keeping motion. | Surface, no horizontal input. |
| Otter Dive_Stop_Idle | Body level below surface, tiny tail and paw corrections. | Underwater, no horizontal or vertical input. |
| Otter Dive_Loop | Head and shoulders tip downward, spine follows, tail/feet drive depth; adapt sign for rising. | Held Dive while depth falls; current held Surface needs either reverse/variant. |
| Beaver Wood_Pickup | Nose locates log, forepaws clasp, chest lifts slightly, head returns. | Successful unique-log pickup. Inventory remains gameplay state. |
| Beaver Wood_Place | Forepaws lower log into slot, brief push to seat it, release and settle. | Successful dam deposit; reclaim needs a reverse or separate event. |
| Elephant Strike | Front feet plant, shoulder and head/trunk draw back, short forward blow, recoil and settle. | Accepted Interact on `Strike Board`; break effect should align with contact frame. |

## First two isolated actions and observed checks

- `Assets/Art/AnimationTests/Fox_test_v1.blend` and `Fox_Interact_test_v1.fbx`: 18 frames at 24 fps. Poses at 1/5/10/14/18. The near front paw reaches while head and chest dip, then returns. Blender rendered five frames in `Docs/AnimationPreviews`. Unity imported one named `Interact` take as Generic, generated a valid Avatar, loop off, 0.71 s. Sampling a temporary Unity model instance changed its left front-foot world position by 0.170 m between start and midpoint. Blender image inspection found the scarf and body intact. **Implemented/verified in asset import only**; gameplay effect not verified because the playable Prefabs still display capsules.
- `Assets/Art/AnimationTests/Monkey_test_v1.blend` and `Monkey_Climb_Loop_test_v1.fbx`: 25 frames at 24 fps, frame 25 matches frame 1. Alternating limb and tail keys at 1/7/13/19/25. Blender rendered five frames. Unity imported one named `Climb_Loop` take as Generic, valid Avatar, loop on, 1.00 s. Unity sample changed the left front-foot position by 0.293 m between start and midpoint. The preview shows a legible alternating beat but too little upright wall posture. **Needs visual repair before gameplay integration**.
- The Unity Console had one pre-existing error at the captured cursor and no new error entries during these imports. Import and sampling do not prove animation during Play Mode. No phone check was performed.

Preview frames: [Fox frame 1](AnimationPreviews/Fox_Interact_01.png), [Fox contact](AnimationPreviews/Fox_Interact_10.png), [Monkey frame 1](AnimationPreviews/Monkey_Climb_Loop_01.png), [Monkey opposite reach](AnimationPreviews/Monkey_Climb_Loop_13.png), [Monkey frame 19](AnimationPreviews/Monkey_Climb_Loop_19.png). Each sequence also includes intermediate/end frames in that folder.

## Decisions still needed

1. Is Run intended to be a distinct sprint, and what speed threshold should select Walk versus Run? Present motor has one movement speed and a misnamed `Run` state.
2. Should a death visual delay the current immediate reset on falling below `resetHeight` or deep-water return? If so, for how long and under whose network authority?
3. Should Fox's Bite_Hold be a closed-mouth hold with a mouth socket, or should the rig be extended to create a jaw opening? What exact lever object and held/release trigger are intended?
4. Should Monkey's rope visual include separate swing and depth-climb Actions, and should wall idle use a vertical body orientation or retain the four-paw wall scramble silhouette? The current test preview needs this direction confirmed.
5. Should Otter Surface use reversed Dive_Loop or a separate rise clip? Should Beaver reclaim reuse pickup? Does Elephant break on the animation contact frame or immediately on input?
6. Confirm the intended model scale, forward axis, and whether the six FBX models should replace the capsule visuals in both modes now or after the full animation set is approved.

Next: review the two preview directions, especially Monkey's wall silhouette. Then revise Monkey, wire an Animator presentation layer and matching visual Prefab copies, verify each animation in SplitScreen and OnlineTest, and only then produce the remaining clips in batches. Preserve the original FBXs and current prototype Prefabs throughout.

## Preparation follow-up — 2026-11-26 (supersedes missing-clip claims above)

Current scope retains capsule Player Visuals and gameplay wiring. No animation production or integration is authorized in this preparation pass.

Fox_animated_All_animation.fbx and its .meta are now present, untracked. No restore was performed. Unity finds nine non-rest clips; see Animation_Source_Inventory.txt for exact names, durations, loops and Avatar settings across every Prefabs FBX and both test FBXs. All newly discovered original actions are **已发现、待预览**, not missing and not yet visually accepted. The former audit table's Fox missing claims are superseded. The prior Fox Interact sampled-preview conclusion and Monkey Climb_Loop needs-repair conclusion remain. No clips are 已接入游戏.

Search scope: recursive Assets/Prefabs, Unity model asset database, Git all-ref Fox path history, and recursive Downloads filenames matching Fox/Meshy. Git history adds Fox_character and textures in fb5e8c5 and their metas in b559848; no tracked animated Fox FBX deletion was found. Downloads/CharacterAssets/Fox has the character and textures; Downloads also has Meshy_AI_Fluffy_Foxling_20261125_generate.blend. These were not copied or modified. Existing animated Fox provenance cannot be established from Git; preserve it and ask for the original download only if provenance is required. Normal Git status hit an LFS subprocess permission failure; filter-disabled status is not evidence that LFS-managed files really changed.

Material diagnosis: every inspected original Renderer uses URP/Lit with a NULL Base Map. The Elephant message supplied by the user describes an embedded material being assigned where an external material is required; complete Console stack was not captured this pass. Original importers and GUIDs were not changed. Packed Base Color/normal/metallic/roughness images were extracted from Elephant, Monkey and Otter into Assets/Art/MaterialPreparation/{animal}. Fox/Beaver/Rabbit previews reference their existing same-animal color PNGs. Independent Preview_URP.mat and MaterialPreview.prefab assets exist for all six; Unity verified a non-null Base Map for each. Original FBXs still have their original untextured material configuration. Rendered Unity color/scarf acceptance and disappearance of the reported message remain pending; do not call the visual repair accepted yet. Normal/roughness maps are preserved but not assigned in these initial preview materials.

Assets/Art/Finished Animation/{Fox,Monkey,Otter,Beaver,Rabbit,Elephant} is prepared and empty: no experimental asset is classified as finished. Subsequent animal tasks read this section, Animation_Source_Inventory.txt, that animal's FBX/material preview, then relevant gameplay code only. Recheck import inventory when files/settings change. First remaining preparation check: render the independent material previews, compare scarf/colors, capture the complete Console message and confirm source clip/Avatar counts unchanged. Then wait for the user's named animal instruction.

## Monkey production pass — 2026-11-26

### Runtime contact follow-up

- MonkeyPresentation uses the active climb collider and its outward normal for orientation and visual spacing. The existing clips still need individual hand and foot contact validation. Climb_End and vine and rope clips have not been revised.
- PrototypeMotor now chooses Push when an active grip has neutral movement input, and clears the previous wall or rope attachment on grip entry. Head-to-box posing remains pending.
- PrototypeGripBlock used horizontal-only Transform movement. It now checks support and sweeps downward for landing on the Host or offline. OnlineTest localhost Host swapped to Monkey through the lobby. Both authored boxes fell from a scripted Y=2.550 to Y=0.551 on the existing floor. This probe did not exercise pushing over an edge or an independent Client.

All six Lobby Animal Displays require a stable looping Sit with light breathing, separate from standing Idle. Reuse suitable same-animal clips. Fox source fox_mon_xiaohuli_01_sit → Lobby Sit is marked existing/reuse from the user handover. Beaver source fox_mon_xiaohuli_01_run → Run is the intended mapping, pending playback inspection. Keep original source names. No other animal animation was produced in this pass.

Monkey source Assets/Prefabs/Monkey.fbx contains rest (0.033 s, non-loop). The existing Climb_Loop test remains separate and is superseded for playback because its silhouette resembled ground locomotion. Editable Blender copy and generator live in Tools/Animation (Monkey_Actions_v1.blend and author_monkey.py). Delivery is Assets/Art/Finished Animation/Monkey: Monkey_Actions.fbx, Monkey.controller, Resources/TwoTailsMonkeyVisual.prefab. The original FBX, skinned mesh hierarchy, test assets and material texture are preserved. Delivery uses the Generic Avatar from Monkey_Actions.fbx and the existing Monkey Preview_URP material with the extracted Monkey Base Color.

| FBX source take → Animator state (same name) | Seconds | Loop | Presentation trigger |
| --- | ---: | --- | --- |
| Monkey_Idle | 2.000 | Yes | Grounded, no move input |
| Monkey_Run | 0.583 | Yes | Existing ground movement; no separate Walk or sprint tier |
| Monkey_Jump_Start | 0.292 | No | Motor ascent |
| Monkey_Jump_Air | 0.667 | Yes | Sustained airtime or falling |
| Monkey_Jump_Land | 0.375 | No | Ground after air |
| Monkey_Interact | 0.667 | No | Successful lever operation |
| Monkey_Death | 1.000 | No | Fall reset; starts at the respawn location, with current 0.65 s display window |
| Monkey_Sit | 2.000 | Yes | Lobby slot currently holds Monkey |
| Monkey_Wall_Hang_Idle | 1.500 | Yes | Attached to wall, no input |
| Monkey_Climb_Loop | 1.000 | Yes | Attached to wall with movement input |
| Monkey_Climb_End | 0.542 | No | Motor reaches the top landing |
| Monkey_Vine_Hang_Idle | 1.500 | Yes | Rope attached without depth input, including swing |
| Monkey_Vine_Climb | 1.000 | Yes | Rope attached with depth input |
| Monkey_Push | 0.750 | Yes | Grip held and movement toward the block |
| Monkey_Pull | 0.833 | Yes | Grip held and movement away |

Blender sampled previews: Sit, Jump_Air, Climb_Loop and Vine_Climb images in Docs/AnimationPreviews. Observed the green scarf and colored fur, an upright airborne silhouette and upright climbing silhouettes. The existing rig lacks separate finger chains. Hand and foot contact with actual geometry, transition intersections and loop seam visual quality still need a focused visual check. The poses use authored bone keys without a live contact solver.

Unity import: 15 named clips and valid Generic Avatar. Runtime model uses the existing species entry point in local and network players and disables root motion. Existing Lobby exchange slots gain the sit visual only while their identity is Monkey. SplitScreen Play Mode observed Lobby Sit with advancing normalized time, Monkey-only Animator, real motor Run with 1.59 m of travel, return Idle, Jump_Start/Air/Land, wall hold, wall climb and real top-edge Climb_End. Rope probe observed auto-grab and hold, 0.80 m ascent with Vine_Climb, and return to hang during swing input. See Monkey_PlayMode_Check.txt for the timed release probe and Direct Host result. Direct localhost Host exchanged to Monkey through the lobby interaction and showed Monkey Idle with the expected Base Map. Independent Client/Relay, visual contact in gameplay, dedicated lever/Death/Push/Pull and hardware validation remain pending.

Six new .gitkeep files under Finished Animation appeared in Git status alongside the existing Unity directory metas. No other eligible empty leaf directory was found by the workspace scan. No commit or push.

Additional Unity import verification: all ten looping clips returned a maximum start/near-end local rotation difference of 0.000 degrees on the imported model at the reported precision. The rope release was observed before the regrab cooldown. No new Console errors since the pre-existing cursor.

