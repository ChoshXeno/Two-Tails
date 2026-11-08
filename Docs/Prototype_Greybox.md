# Prototype Greybox

## Open and run

1. Open the project in Unity `6000.6.8f1`. In the Project window, double-click `Assets/prototype/Scenes/Prototype_Greybox.unity`, enter Play Mode, and focus the Game view.
2. The saved scene can run directly. **Tools > Two Tails > Build Prototype Greybox** generates the scene if it is missing. A configured scene is left intact on repeat runs.
3. The Windows Build Profile uses the Prototype scene. The global scene list still points to `SampleScene`. For another platform's prototype build, select `Prototype_Greybox` in that platform's profile.

## Resource conventions

Keep experimental scenes, Prefabs, scripts, input, materials, UI, and Editor tools in their corresponding folders under `Assets/prototype`. Put formal level resources in the existing purpose-specific folders directly under `Assets`, such as `Editor`, `Input`, `Materials`, `Prefabs`, `Scenes`, and the existing scripts folder `Script`. Reuse the existing names and folders where they fit. Create a new folder only for a distinct responsibility without a suitable home.

Write all player-visible text in English, including HUD, buttons, prompts, statuses, errors, pause menus, and debug text shown in a test scene.

## Inspector settings

| Object or asset | Key configuration |
| --- | --- |
| Greybox | Ground at `(0, -0.25, 0)`, scale `(20, 0.5, 16)`. Four boundary walls are 2 m high and 1 m thick. The obstacles, frames, and panels use box colliders. |
| `PrototypePlayer.prefab` | Unit root scale, Character Controller height 2 m, radius 0.4 m, center `(0, 1, 0)`, skin width 0.04 m. P1 spawns at `(0, 0.1, -5)`. The red `DebugPlayer2` spawns at `(2, 0.1, -5)`. Both use the same Prefab and motor. Their visual capsules and markers have no colliders. |
| `PrototypeMotor` | Speed 4 m/s, visual turn rate 720°/s, gravity 24 m/s², reset below `y = -5`. `SetMoveInput(Vector2)` is the input entry point. |
| Lever | Root `(-2, 0, 3.5)`. `InteractionPoint` is 1.2 m above the floor. The handle tilts by 65° when the target is open. The shared toggle accepts at most one press per frame and has a 0.15 s debounce. |
| Interactor | Chest origin on the rotating visual. Maximum range 1.5 m and horizontal facing angle 45°. The line-of-sight ray ignores the actor and target colliders and triggers. When several levers qualify, the closest valid target wins. |
| Doors | Lever controls the red `LeverDoor` at `(0, 0, 5)`. The AND gate controls the blue `PressurePlateDoor` at `(4.8, 0, 4.8)`. Both travel 3 m with an approximately 0.6 s full open or close and can reverse during travel. |
| Pressure plates | Plate A at `(3.2, 0, 3.35)`, Plate B at `(6.4, 0, 3.35)`. Each trigger is `1.22 × 0.44 × 1.22 m` with center Y at 0.18 m. An occupied visual turns green and lowers by 0.025 m. A physics overlap sample rebuilds each plate's set of character roots every fixed step. |
| Main Camera | P1 pivot height 1.2 m, initial pitch 25°, pitch limits −10° to 65°. User distance starts at 5 m, clamps to 2–8 m, and smooths over 0.12 s. A wheel tick moves the target 0.5 m. Pinch moves it 6 m per screen short edge. Sphere radius 0.2 m, wall clearance 0.08 m, and obstruction recovery 0.22 s. A wall can shorten the actual distance below 2 m while preserving the selected distance. |
| `TwoTailsInput.inputactions` | `Gameplay` gives P1 WASD and left stick, plus E and Gamepad south. `DebugPlayer2` gives P2 arrow keys and both zero keys. Input System 1.20.1 names the main keyboard's zero control `<Keyboard>/0`. The keypad path is `<Keyboard>/numpad0`. |
| Prototype HUD | Screen Space Overlay, reference resolution 1920 × 1080, Match 0.5. The transparent world gesture surface sits behind the controls. Only touches starting on that surface can orbit or pinch. Foreground controls also exclude the mouse wheel. P1 and P2 have separate English prompts. |
| Prototype Debug Players | `Enable Player 2` defaults on for the Editor and Development Builds. The scene saves P2 inactive. Ordinary release players keep it inactive, along with its prompt. |

The HUD uses the built-in uGUI font and circle sprite. The room uses five URP/Lit color materials and one directional light.

## Manual acceptance

1. Hold WASD and an arrow key together. Confirm that each input moves only its own capsule and the two controllers stop against each other. Verify that joystick movement affects only P1.
2. With one character near the lever, try the other character's interaction keys. Approach with the correct character from more than 1.5 m, with its visual facing away, and with geometry in the line of sight. The door stays closed in all three cases.
3. Face the lever within range and toggle with E, Interact, main keyboard 0, or numpad 0 on the corresponding player. Check that holding the key does not repeat and that two presses on the same frame toggle only once. Toggle again after 0.15 s while the door is moving and watch it reverse without drifting.
4. Use the wheel over the world. Up zooms in, down zooms out, and the joystick and button block wheel events and camera drags. Move P1 near the south wall and back. The camera should shorten and recover to the same selected distance.
5. Test the pressure-door cases below. Watch the occupied visuals turn green and lower, then rise and turn yellow when vacated. Verify that each test area operates independently.
6. Use the Editor or a Development Build for both test characters. On a physical iPhone, check pinch in and out, one-finger orbit before and after pinch, pinch exclusion for fingers starting on controls, concurrent stick and orbit, safe-area placement, touch cancellation, and background/resume.

| Plate A | Plate B | Door target |
| --- | --- | --- |
| Empty | Empty | Closed |
| P1 | Empty | Closed |
| Empty | P2 | Closed |
| P1 | P2 | Open |
| P2 | P1 | Open |
| Both characters | Empty | Closed |
| Same character | Same character | Closed |

Leaving either plate, disabling or destroying its actor, or teleporting away must close the pressure door. Multiple colliders on one actor count once. A prop on either plate has no effect.

If a closing panel would intersect a character, the visual still closes and its collider temporarily yields. The collider returns when the panel volume is clear. During that overlap, the visually closed panel can be passed through.

## Verification recorded

- On November 9, 2026, the live Editor confirmed the migrated scene and Prefab GUIDs were unchanged. A scan found zero Missing Scripts and zero dangling serialized object references. The Windows Build Profile points to the renamed scene. Repeating the menu left all three asset hashes unchanged.
- Current Play Mode probes passed the pressure-door table, including one actor spanning both plates, two colliders belonging to one actor, non-character props, disabled and destroyed actors, and teleport cleanup. The visual changed to green and lowered while its trigger stayed fixed.
- Current Play Mode keyboard probes confirmed P1/P2 lever input, same-frame suppression, direction reversal, exact open and closed endpoints, and independent doors. In the doorway-overlap probe the collider yielded without ejecting the character and restored after departure.
- Queued Input System mouse events verified a 0.5 m change per wheel tick, the 2–8 m limits, unchanged FOV, and wheel and drag exclusion over the stick and button. The camera recovered to the selected 6 m after shortening to about 0.69 m near the south wall.
- The previous Windows x64 build with `BuildOptions.None` launched with `development=False`, `activeP2=False`, and one active motor.
- **Pending user verification.** Physical iPhone install, pinch and other two-finger gestures, safe-area behavior, and focus or touch cancellation on the device.
