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

## Rabbit production pass — 2026-11-26

Sampled seven takes from Bunny_animated_All_animation.fbx and the rest pose from Bunny_character.fbx on their own skins. No Rabbit test clip or previous finished animation was found. The original Walk is unassigned because the motor has no separate walk state. IdleLieDown and IdleAlertLookAround are unused alternative resting poses. Grip explicitly excludes Rabbit. The source animation imports showed substantial floor-height differences, so selected motions were baked as copies using bone rotations and grounded visual height adjustment where appropriate. Root motion is off.

| Source → state | Seconds | Loop |
| --- | ---: | --- |
| rabbit_Idle → Idle | 0.667 | Yes |
| New sitting pose → Sit | 2.000 | Yes |
| rabbit_ANIM_Rabbit_Gallop → Run | 0.458 | Yes |
| rabbit_ANIM_Rabbit_1Hop early phase → Jump_Start | 0.160 | No |
| rabbit_ANIM_Rabbit_1Hop mid phase → Jump_Air | 0.360 | Yes |
| rabbit_ANIM_Rabbit_1Hop recovery phase → Jump_Land | 0.240 | No |
| New head anticipation and push → Interact | 0.500 | No |
| rabbit_ANIM_Rabbit_Death → Death | 0.708 | No |
| rabbit_ANIM_Rabbit_1Hop compressed push → Rejump | 0.160 | No |

Delivery: Assets/Art/Finished Animation/Rabbit/Resources/TwoTailsRabbitVisual.prefab with Rabbit.controller, valid Generic RabbitAvatar.asset and Bunny_animated_All_animation Base Map. Runtime uses Jump_Start briefly on takeoff, then Jump_Air through the remainder of flight. An accepted second jump briefly reports Rejump and restarts Jump_Start at frame zero. Rabbit_Jump_Air remains looped (0.36 s); its stable airborne pose and seam still need adjustment in Unity. The controller remains authoritative for movement and jump count. Lever use retains its existing accepted-action timing, with no animation event.

SplitScreen verified Lobby Sit and actual request to swap Fox to Rabbit. CharacterController height was 1.0 m. Earlier controlled presentation probes selected Jump_Start, Jump_Air, Rejump and Jump_Land directly; these predate the current routing. The motor requests Jump_Air after the takeoff window and replays Jump_Start for the second impulse, but this routing and the revised airborne clip have not been exercised in a live Play Mode trace. Actual double-jump and ledge-fall input, lever contact, death/reset, small-hole traversal, rendered shape and independent Host/Client remain pending.

## Beaver production pass — 2026-11-26

### Proportion and grip follow-up

The animated source was proportionally shorter than Beaver_character in its unanimated skin (baked length-to-height about 1.50 versus 1.59). The prefab now uses Beaver_character with unchanged uniform proportions. Motion deltas were rebaked onto its rest skeleton and existing standalone clip GUIDs were retained. Grounded clips add vertical skeletal correction only. Mesh minima relative to the visual anchor sampled from -0.006 to +0.016 m, including Idle, Run and both grip blends. The Player root stayed at unit scale.

The former controller referenced Beaver Push and Pull and had no Override Controller; the reported swim playback was not reproduced. New grips blend hold and stepping on the existing GripSpeed parameter. A controlled Animator probe selected the expected Beaver hold and step at 0 and 1.2 m/s. SplitScreen accepted the lobby exchange and later reported only Beaver_character with the Beaver controller. Controlled wood handlers produced Pickup/Place/Pickup/Place, inventory 1/0/1/0 and dam 0/1/0/1. This did not exercise actual pushing and pulling, rendered floor contact, or repeat Lobby Sit.

Recursive scan found Beaver_character.fbx (rest only) and Beaver_animated_All_animation.fbx (six gameplay candidates). No matching test FBX was found. Output: Assets/Art/Finished Animation/Beaver/Resources/TwoTailsBeaverVisual.prefab and Beaver.controller. The prefab uses the animated source skin, a valid independent Generic Avatar, the Beaver Preview_URP material and Beaver_animated_All_animation Base Map. Root motion is disabled.

| Source → state | Seconds | Delivery |
| --- | ---: | --- |
| wolf_idle → Idle | 1.667 | Looping copy |
| fox_mon_xiaohuli_01_run → Run | 0.533 | Looping copy |
| wolf_jump_start → Jump_Start | 0.267 | Direct |
| wolf_jump_loop → Jump_Air | 0.467 | Looping copy |
| wolf_jump_end → Jump_Land | 0.667 | Direct |
| wolf_dead → Death | 1.267 | Direct |
| Beaver_Sit → Sit | 2.200 | New loop |
| Beaver_Interact → Interact | 0.620 | New one-shot |
| Beaver_Wood_Pickup → Wood_Pickup | 0.620 | New one-shot for collecting and reclaiming |
| Beaver_Wood_Place → Wood_Place | 0.620 | New one-shot for deposit |
| Beaver_Push / Beaver_Pull → Push / Pull | 0.680 each | New directional loops |

Walk has no distinct gameplay state. Unity sampled nine baked skin poses for each state and confirmed clip flags. No rendered judgment was made on the bandage. SplitScreen observed Lobby Sit and successful Fox-to-Beaver exchange with playable Idle. The first timed wood probe hit the wait gate. A reset of only runtime test lock/cooldown allowed the normal successful handlers to run: collect → place → reclaim → place produced Pickup, Place, Pickup, Place requests with inventory 1/0/1/0 and dam progress 0/1/0/1. The cues originate after authoritative success; inventory, slot visuals and water keep their existing update timing. Hand-to-object and exact slot contact timing, rendered animation switching for all four cues, complete dam water rise, land locomotion/jump/ledge fall, ordinary interaction, grip, death and independent Host/Client remain unverified.

## Otter production pass — 2026-11-26

### Fox locomotion transfer, ascent and grip gait follow-up

Otter Run, Jump_Start, Jump_Air and Jump_Land now use sampled Fox rest-relative rotations on 25 matching Otter nodes, with reduced tail and lower limb amplitude. Unity verified their original Fox durations of 0.533, 0.267, 0.467 and 0.667 seconds and their loop flags. All four are Otter-owned rotation-only clips. Existing Otter clip GUIDs were retained. No Fox clip is directly referenced by Otter.

Fox and Otter Push and Pull each use a hold/step blend tree at 0 and 1.2 m/s. Negative grip intent selects Pull and neutral input retains the active direction. Measured planar displacement controls blending so a blocked actor can settle into the hold. Otter Dive_Loop uses measured upward travel to pitch the visual and eases back to its neutral mount at the surface. Unity confirmed the four blend trees and controlled pool descent to Y=-3.0. The ascent trace produced no samples. Short-hop, moving landing, full push and pull cycles, contact and slide checks, and ascent alignment still require interactive scene verification.

Source inventory: Assets/Prefabs/Otter.fbx imports rest plus the Unity-generated preview of rest. No Otter test FBX was found. All clips below are newly authored Generic rotation curves on the original Otter hierarchy, using Fox run timing and interaction rhythm as references. No imported Fox motion was copied onto the Otter rig. Original rig, importer and source material files were preserved.

| New Otter clip | Seconds | Loop |
| --- | ---: | --- |
| Idle | 2.00 | Yes |
| Sit | 2.40 | Yes |
| Run | 0.54 | Yes |
| Jump_Start | 0.25 | No |
| Jump_Air | 0.54 | Yes |
| Jump_Land | 0.36 | No |
| Interact | 0.67 | No |
| Death | 0.65 | No |
| Swim_Start | 0.32 | No |
| Swim_Loop | 0.68 | Yes |
| Swim_Stop_Idle | 1.70 | Yes |
| Dive_Loop | 0.63 | Yes |
| Dive_Stop_Idle | 1.90 | Yes |
| Push | 0.68 | Yes |
| Pull | 0.68 | Yes |

Delivery: Assets/Art/Finished Animation/Otter/Resources/TwoTailsOtterVisual.prefab, Otter.controller and OtterAvatar.asset. Existing Preview_URP links Otter BakedMaterial_baseColor. Root motion is disabled. Walk has no separate gameplay speed or state. Water exit blends to the existing land/air motion, with no dedicated exit mechanic. Interact uses a forepaw reach; actual lever handle contact remains unverified.

Unity sampled nine baked mesh frames per clip and reported the expected loop flags. This does not establish rendered pose or scarf quality. In SplitScreen, a controlled species assignment and move into Swimming Pool produced Swim_Start; subsequent Animator playback confirmed Otter_Swim_Stop_Idle. Scripted motor and swim input requested Swim_Loop, Dive_Loop at Y=-2.20, Dive_Stop_Idle, and Swim_Stop_Idle at Y=-1.40 on surfacing. Lobby exchange attempt returned Already playing Fox; normal swap and Lobby Sit, rendered land and water sequences, exit, rapid starts and stops, forepaw contact and Host/Client verification remain pending.

## Fox production pass — 2026-11-26

### Paw lever and blend follow-up

The controller has no Any State transitions and the Fox driver already skips duplicate requests. Rapid alternating Idle/Run requests could interrupt each other's fades. The Fox driver now lets an in-flight locomotion blend settle before switching direction, with 0.13 s general and 0.07 s takeoff/landing fades. Actual jump and locomotion playback still require a trace with a spawned player.

The existing Fox_Interact asset is the underlying weight-shift pose. Accepted local or network lever interaction now carries the lever reference. Fox presentation reaches toward the handle grip centered 0.45 m above the pivot using rotation-only foreleg IK. The handle moves from 0.19 to 0.43 s into the visual gesture. Door demand stays on the original accepted-use path. Unity loaded the scripts, but no player was spawned in the measured OnlineTest session. Reach, penetration, phase timing, lobby exchange and video remain unverified.

### Scale and state follow-up

The prefab root was scaled uniformly to 1.22 in the visual asset. Side-by-side instantiated prefab bounds in the OnlineTest Play Mode scene measured Fox 0.789 × 1.281 × 1.651 m and Monkey 0.860 × 1.180 × 1.703 m. Both mesh bases were at Y≈0 with their visual anchors at Y=0. These are geometric probes, not a rendered comparison or lobby check.

The previous Interact source matched the production hierarchy paths but included 129 position and 129 scale curves. The new Fox_Interact.anim transfers relative bone rotations to the production rest pose. Across 13 samples, local bone translation and scale deltas were zero and rotation reached 23.6 degrees. Visual blending and normal interaction still require gameplay inspection.

Fox previously used controller grounding to choose airborne motion during grip follow-through, with no grip-specific Motion. Active grips now select Fox_Push even at zero movement. Fox uses 0.08 s takeoff and landing fades and 0.11 s for other transitions. Landing can interrupt Jump_Start, while walking off an edge directly requests Jump_Air. Push contact, short-hop timing, Bite_Hold and independent Host/Client acceptance remain pending. Online Host failed to start due to the Netcode missing NetworkManager error and an alternate port retry timed out.

Recursive Prefabs search found Fox_character.fbx (rest only) and Fox_animated_All_animation.fbx (nine named takes). The latter supplies the skin and six selected source takes plus Jump_Air. Idle1 and xiuwen are unused candidates. The independent Resources/TwoTailsFoxVisual.prefab, Fox.controller and Generic FoxAvatar.asset are under Assets/Art/Finished Animation/Fox. The existing Fox Preview_URP material provides the Base Map. Root motion is off.

| State | Take | Seconds | Loop and delivery |
| --- | --- | ---: | --- |
| Idle | fox_mon_xiaohuli_01_idle | 1.667 | Looping copy |
| Sit | fox_mon_xiaohuli_01_sit | 1.333 | Looping copy, lobby |
| Run | fox_mon_xiaohuli_01_run | 0.533 | Looping copy |
| Jump_Start | wolf_jump_start | 0.267 | Direct, once |
| Jump_Air | wolf_jump_loop | 0.467 | Looping copy |
| Jump_Land | wolf_jump_end | 0.667 | Direct, once |
| Interact | Interact from Fox_Interact_test_v1.fbx | 0.708 | Direct, once |
| Death | wolf_dead | 1.267 | Direct, once. Existing reset is immediate and presentation window is 0.65 s |

No distinct walk state. Fox is eligible for the common grip mechanics, but dedicated Push and Pull poses are pending. The current lever interaction has no Fox bite hold/release state or mouth contact target, so Bite_Hold remains pending.

Unity generated a valid Generic Avatar and eight controller states. Verified OnlineTest localhost Host spawn as Fox, actual Idle playback, the bound Fox Base Map and a skinned bounds minimum of Y=0.032 against Y=0.040 for the player anchor. The subsequent exchange probes returned Already playing Fox and Please wait, so lobby Sit and a successful round trip are unverified. The remaining takes require posed and rendered inspection, including scarf appearance, and the full Run, jump, ledge fall, Interact, grip and death sequences plus independent Client playback still require scene verification.

## Monkey production pass — 2026-11-26

### Grip pose and Mixamo mantle follow-up

Monkey, Fox and Otter Push/Pull states use 0.68 s hold and step motions. Push lowers the head; Pull raises the head and uses reverse stepping. Local presentation reads actual gripped box displacement, with actor displacement as the fallback when there is no local grip target. Unity generated the clip and state references. After reconnecting the Editor bridge, a controlled probe at the real Weight 2 box confirmed all three species could grip, request Push and Pull, and release to Idle. The probe supplied the pull intent directly. Interactive box movement, stop and restart gait timing remain unverified.

Assets/Extra/Ch14_nonPBR@Braced Hang To Crouch.fbx imports a 1.133 s take. Seven pose samples showed braced hang, hips rising while the lead hand initially stays close to its start, then legs coming up into crouch. The reference starts in braced hang and has no separate preceding wall-climb cycle. The standalone Monkey_Climb_End.anim is a hand-authored 0.84 s rotation-only adaptation of those phases on the original Monkey rig. The existing state references it. CharacterController now stages rise, traversal and settle after the grab-height and clearance checks, with forepaw IK targeting the edge during the support phase.

Controlled SplitScreen Play Mode tests attached Monkey at the grab height on the real Air Catch Vine and Vine Wall, then invoked the normal mantle checks and advanced the controller at 0.02 s per step. Each started and completed in 43 steps with a 0.010 m landing offset. This verifies the root path under the tested setup. Normal climb-to-mantle input, side-view hand slip, mesh intersection, Animator return and occupied landing checks remain pending.

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

`nRabbit Jump_Air follow-up (2026-11-27): froze source frame 3 at 60 fps into a 0.6 s constant airborne hold across all 168 curves using Unity AnimationUtility. Loop enabled, existing asset GUID retained. Removes pose oscillation and extra vertical animation. Gameplay rendering remains pending.

## Elephant first pass — 2026-11-27

Recursive Prefabs scan found Elephant.fbx only, with rest (0.033 s) and editor preview, no gameplay actions. All requested actions were missing. Authored on original rest skeleton, with unchanged weights, hierarchy and FBX GUID. No source action reused. Native scale measured 1.480 m high; visual child scaled uniformly 1.1149 for 1.65 m reference height and rest mesh floor aligned, without editing Player root/collider.

| New clip / state | Seconds | Loop |
| --- | ---: | --- |
| Idle, Sit | 2.4 each | Yes |
| Run | 0.9 | Yes |
| Jump_Start | 0.27 | No |
| Jump_Air | 0.65 | Yes |
| Jump_Land | 0.4 | No |
| Interact | 0.65 | No |
| Death | 0.65 | No |
| Push, Push_Hold, Pull, Pull_Hold | 0.9 each | Yes |
| Strike | 0.8 | No |

Delivery candidate: Assets/Art/Finished Animation/Elephant/Resources/TwoTailsElephantVisual.prefab; Elephant.controller; ElephantAvatar.asset; 13 Elephant-owned clips. Uses existing external Elephant Preview_URP.mat and baseColor, leaves BakedMaterial importer untouched. Generic Avatar valid; root motion false. Walk has no dedicated motor tier. Existing motor allows Elephant jumping. GripSpeed blends hold at 0 with step at 1.2 m/s using existing actual block displacement input. Strike uses a new appended motion enum, requested on successful board use; board effect remains immediate. Remote Strike cue synchronization not verified/complete.

Controlled Play Mode Animator selection confirmed Idle/Sit/Run/three jumps/Interact/Death/Push/Pull/Strike all reference Elephant motions. This was a temporary prefab probe, not actual player input. Actual lobby swap, contact and deformation rendering, grounded gait, push/pull sequences, death and Host/Client remain unverified. These are first-pass assets awaiting acceptance. Sit leg placement and animated mesh floor require visual correction if necessary; do not infer acceptance from successful binding. The normal player sequence probe was inconclusive after runtime species/state changed. Build authoring source: AgentScripts/BuildElephant.cs.

Elephant torso follow-up: six clips updated in place with hips/chest/neck curves and local hips compression; Run local heave 0–0.036 m, Interact -0.022 m, Strike -0.025 m, takeoff -0.055 m, landing -0.065 m; Jump_Air fixed height. No scale curves or gameplay displacement changes. Unity authoring succeeded; rendered contact and deformation acceptance still pending. Authoring source: AgentScripts/TuneElephantBody.cs.

Monkey rope grip follow-up (2026-11-27): added RopeGripUp tangent accessor and post-animation-request HoldRope forelimb targeting in MonkeyPresentation. Visual-only alignment and hand-centre correction keep the hands at the runtime grip across the climb/hang transition. Unity compilation passed; rendered contact, transition smoothness and remote parity remain pending.

Night visibility fix (2026-11-27): skinned animals now registered initially and at species visual instantiation; runtime night material preserves Base Map and shader samples UV texture. OnlineTest developer/night vision disabled, runtime binding checked; all six controlled prefab preparations retained Base Map. Shader compile clean. Rendered dark/near/local-light/Fox/split/Host-Client acceptance remains pending; see PROJECT_STATUS.
