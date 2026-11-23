# Prototype serialized Inspector inventory

Read from Unity assets. Null fields may be intentional or filled at runtime. Values listed here are serialized overrides and values, before Play Mode.

### Prototype_MainMenu — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · Main Menu, Menu Camera
- `Main Menu` / `PrototypeTestMenu` · mainMenu=True
### Prototype_SplitScreen — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · Greybox, Directional Light, P1 Fox, Main Camera, EventSystem, Prototype Debug Players, P2 Camera, Split Screen, Animal Test Geometry, Level 1 Night Test, Cooperative Test Areas, Animal Mechanisms, Monkey Rope Test, Grip Block Test
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Lever` / `PrototypeLever` · interactionPoint=InteractionPoint · handle=Handle · pullAngle=65 · door=DoorPanel · debounce=0.15
- `PressurePlateDoor` / `PrototypePlateGate` · plateA=Plate A · plateB=Plate B · door=DoorPanel
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Plate A` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Plate B` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `P1 Fox` / `PrototypeMotor` · visual=Visual · movementCamera=Main Camera · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `P1 Fox` / `PrototypeLocalInput` · actions=PrototypeSplitInput · actionMap=Gameplay · interactor=P1 Fox
- `P1 Fox` / `PrototypeInteractor` · interactionOrigin=InteractionOrigin · maximumDistance=1.5 · maximumAngle=45 · visibilityMask=Everything · levers.Array.data[0]=Lever
- `P1 Fox` / `PrototypeAnimal` · species=0
- `P1 Fox` / `AnimalMechanicsActor` · 
- `P1 Fox` / `PrototypeSwimmer` · swimSpeed=3 · diveSpeed=2 · surfaceSpeed=2 · surfaceOffset=0.8 · floorClearance=0.15
- `Main Camera` / `PrototypeFollowCamera` · target=P1 Fox · spawnFacing=Visual · pivotHeight=1.2 · distance=5 · minimumDistance=2 · maximumDistance=8 · zoomTime=0.12 · wheelSensitivity=0.5 · pinchSensitivity=6 · initialPitch=25 · minimumPitch=-10 · maximumPitch=65 · mouseSensitivity=0.22 · touchSensitivity=0.16 · invertY=False · smoothTime=0.16 · rotationTime=0.075 · obstructionMask=Default · obstructionRadius=0.2 · obstructionPadding=0.08 · recoveryTime=0.22
- `P2 Monkey` / `PrototypeMotor` · visual=Visual · movementCamera=Main Camera · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `P2 Monkey` / `PrototypeLocalInput` · actions=PrototypeSplitInput · actionMap=DebugPlayer2 · interactor=P2 Monkey
- `P2 Monkey` / `PrototypeInteractor` · interactionOrigin=InteractionOrigin · maximumDistance=1.5 · maximumAngle=45 · visibilityMask=Everything · levers.Array.data[0]=Lever
- `P2 Monkey` / `PrototypeAnimal` · species=1
- `P2 Monkey` / `AnimalMechanicsActor` · 
- `P2 Monkey` / `PrototypeSwimmer` · swimSpeed=3 · diveSpeed=2 · surfaceSpeed=2 · surfaceOffset=0.8 · floorClearance=0.15
- `P2 Camera` / `PrototypeFollowCamera` · target=P1 Fox · spawnFacing=Visual · pivotHeight=1.2 · distance=5 · minimumDistance=2 · maximumDistance=8 · zoomTime=0.12 · wheelSensitivity=0.5 · pinchSensitivity=6 · initialPitch=25 · minimumPitch=-10 · maximumPitch=65 · mouseSensitivity=0.22 · touchSensitivity=0.16 · invertY=False · smoothTime=0.16 · rotationTime=0.075 · obstructionMask=Default · obstructionRadius=0.2 · obstructionPadding=0.08 · recoveryTime=0.22
- `Split Screen` / `PrototypeSplitScreen` · players.Array.data[0]=P1 Fox · players.Array.data[1]=P2 Monkey · cameras.Array.data[0]=Main Camera · cameras.Array.data[1]=P2 Camera · viewports.Array.data[0].x=0 · viewports.Array.data[0].y=0 · viewports.Array.data[0].width=0.5 · viewports.Array.data[0].height=1 · viewports.Array.data[1].x=0.5 · viewports.Array.data[1].y=0 · viewports.Array.data[1].width=0.5 · viewports.Array.data[1].height=1 · lookUp=92 · lookDown=86 · lookLeft=88 · lookRight=90 · zoomIn=93 · zoomOut=87 · lookSpeed=90 · zoomSpeed=3
- `Split Screen` / `PrototypeTestMenu` · mainMenu=False
- `Vine Wall` / `PrototypeClimbSurface` · front=(0.00, 0.00, -1.00) · front.x=0 · front.y=0 · front.z=-1 · landing=Landing
- `Level 1 Night Test` / `PrototypeNightEnvironment` · nightShader=Two Tails/Prototype Animal Vision · visibleRadius=3 · fadeWidth=3 · localLightFadeWidth=0.2 · nightBrightness=0.28 · nightTint=RGBA(0.380, 0.520, 0.750, 1.000) · foxBrightness=1.4 · foxGamma=1.15 · transparentShader=Two Tails/Prototype Visible Transparent · daylight=Directional Light · moonlight=Weak Moonlight · moonDirection=(65.00, -35.00, 0.00) · moonDirection.x=65 · moonDirection.y=-35 · moonDirection.z=0 · moonColor=RGBA(0.340, 0.460, 0.700, 1.000) · moonIntensity=0.12 · moonShadows=2 · suppressRealtimeShadowsAtNight=True · suppressScreenSpaceAmbientOcclusionAtNight=True
- `Air Catch Vine` / `PrototypeClimbSurface` · front=(0.00, 0.00, -1.00) · front.x=0 · front.y=0 · front.z=-1 · landing=Air Catch Landing
- `TT-P05 Switchback Gates` / `CooperativePuzzle` · kind=0 · networked=False · plateA=Plate A · plateB=Plate B · resetPad=Reset Tile · movingA=Gate A · movingB=Gate B · exitGate=null · rampA=null · rampB=null · plateVisualA=Plate A Visual · plateVisualB=Plate B Visual · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `TT-P01 Bridge Handoff` / `CooperativePuzzle` · kind=1 · networked=False · plateA=Plate A · plateB=Plate B · resetPad=Reset Tile · movingA=Front Bridge · movingB=Rear Bridge · exitGate=null · rampA=null · rampB=null · plateVisualA=Plate A Visual · plateVisualB=Plate B Visual · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `TT-P02 Counterweight Lift` / `CooperativePuzzle` · kind=2 · networked=False · plateA=Left Load · plateB=Right Load · resetPad=Reset Tile · movingA=Left Lift · movingB=Right Lift · exitGate=Balance Exit Gate · rampA=Loading Ramp 0 · rampB=Loading Ramp 1 · plateVisualA=Left Lift · plateVisualB=Right Lift · boxes.Array.data[0]=Weight 2 Crate 0 · boxes.Array.data[1]=Weight 1 Crate 1 · boxes.Array.data[2]=Weight 1 Crate 2 · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `Weight 2 Crate 0` / `CooperativeWeightBox` · weight=2
- `Weight 1 Crate 1` / `CooperativeWeightBox` · weight=1
- `Weight 1 Crate 2` / `CooperativeWeightBox` · weight=1
- `Animal Mechanisms` / `AnimalMechanicsWorld` · heavyPushSpeed=1.5 · heavyFallHeight=-5 · heavyRespawnDelay=0.8 · savedRabbit=Rabbit Display · savedElephant=Elephant Display · savedStrikeBoard=Strike Board · savedRabbitVisual=Visual · savedElephantVisual=Visual · savedMonkey=Monkey Display · savedMonkeyVisual=Visual · savedHeavyBlock=Heavy Gap Block · networked=False · lobby=Safe Swap Lobby · displays.Array.data[0]=Otter Display · displays.Array.data[1]=Beaver Display · displayVisuals.Array.data[0]=Visual · displayVisuals.Array.data[1]=Visual · logs.Array.data[0]=Collectible Log 0 · logs.Array.data[1]=Collectible Log 1 · logs.Array.data[2]=Collectible Log 2 · dam=Dam · resetPoint=Animal Test Reset · damMarker=Dam Marker · installedLogs.Array.data[0]=Installed Log 0 · installedLogs.Array.data[1]=Installed Log 1 · installedLogs.Array.data[2]=Installed Log 2 · reservoir=Rising Reservoir · damLabel=Label · riseSpeed=0.3 · interactionDistance=1.5 · interactionAngle=45 · swapDuration=0.8
- `Swimming Pool` / `PrototypeWaterVolume` · bottom=-4 · initialLevel=-0.6 · targetLevel=-0.6 · waterVisual=Water · safeShore=Safe Shore
- `Rising Reservoir` / `PrototypeWaterVolume` · bottom=-5 · initialLevel=-4 · targetLevel=0 · waterVisual=Water · safeShore=Safe Shore
- `Elephant Free Push Test Block` / `ElephantFreePushBlock` · pushSpeed=1.5 · fallHeight=-5 · respawnDelay=0.8 · useInitialTransform=True · respawnPoint=null
- `Climb Rope` / `PrototypeSwingRope` · online=False · length=5 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Rope A` / `PrototypeSwingRope` · online=False · length=5.6 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Rope B` / `PrototypeSwingRope` · online=False · length=5.6 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Weight 2` / `PrototypeGripBlock` · weight=2 · online=False
- `Weight 3` / `PrototypeGripBlock` · weight=3 · online=False
### Prototype_OnlineTest — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · Greybox, Directional Light, Main Camera, Prototype HUD, EventSystem, Network Session, Test Menu Return, Animal Test Geometry, Level 1 Night Test, Cooperative Test Areas, Animal Mechanisms, Monkey Rope Test, Grip Block Test
- `LeverDoor` / `PrototypeNetworkDoor` · panel=DoorPanel · passage=Server Passage · playerLayers=Ignore Raycast
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Lever` / `PrototypeLever` · interactionPoint=InteractionPoint · handle=Handle · pullAngle=65 · door=null · debounce=0.15
- `Lever` / `PrototypeNetworkLever` · door=LeverDoor · maximumDistance=1.5 · maximumAngle=45 · serverCooldown=0.25 · visibilityMask=Default
- `PressurePlateDoor` / `PrototypeNetworkDoor` · panel=DoorPanel · passage=Server Passage · playerLayers=Ignore Raycast
- `PressurePlateDoor` / `PrototypeNetworkPlateGate` · plateA=Plate A · plateB=Plate B · playerLayers=Ignore Raycast
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Plate A` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Plate B` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Main Camera` / `PrototypeFollowCamera` · target=null · spawnFacing=null · pivotHeight=1.2 · distance=5 · minimumDistance=2 · maximumDistance=8 · zoomTime=0.12 · wheelSensitivity=0.5 · pinchSensitivity=6 · initialPitch=25 · minimumPitch=-10 · maximumPitch=65 · mouseSensitivity=0.22 · touchSensitivity=0.16 · invertY=False · smoothTime=0.16 · rotationTime=0.075 · obstructionMask=Default · obstructionRadius=0.2 · obstructionPadding=0.08 · recoveryTime=0.22
- `World Orbit Input` / `PrototypeOrbitDrag` · orbit=Main Camera
- `SafeArea` / `PrototypeSafeArea` · mobileFrameRate=30
- `SafeArea` / `PrototypeSwimmingHud` · session=Network Session · jumpLabel=Text · surface=Surface
- `StickHandle` / `PrototypeScreenStick` · m_MovementRange=68 · m_DynamicOriginRange=100 · m_ControlPath=<Gamepad>/leftStick · m_Behaviour=0 · m_UseIsolatedInputActions=False · m_PointerDownAction.m_Name= · m_PointerDownAction.m_Type=0 · m_PointerDownAction.m_ExpectedControlType= · m_PointerDownAction.m_Id=29f13f25-85eb-49d9-9d34-738187c97ea6 · m_PointerDownAction.m_Processors= · m_PointerDownAction.m_Interactions= · m_PointerDownAction.m_Flags=0 · m_PointerDownAction.m_Priority=0 · m_PointerMoveAction.m_Name= · m_PointerMoveAction.m_Type=0 · m_PointerMoveAction.m_ExpectedControlType= · m_PointerMoveAction.m_Id=2210ca19-eb72-4977-a5c4-2c2319310a58 · m_PointerMoveAction.m_Processors= · m_PointerMoveAction.m_Interactions= · m_PointerMoveAction.m_Flags=0 · m_PointerMoveAction.m_Priority=0
- `Connection` / `PrototypeNetworkHud` · session=Network Session · followCamera=Main Camera · orbit=World Orbit Input · stick=StickBase · address=Address · port=Port · host=Start Host · client=Start Client · disconnect=Disconnect · status=Text · players=Text · online=Network Session · directControls=Direct Test Controls · onlineControls=Online Relay Controls · switchMode=Direct Test · modeLabel=Text · title=Text · joinCode=Join Code · createRoom=Create Room · joinRoom=Join Room · copyCode=Copy Code · leaveRoom=Leave Room · leaveLabel=Text · invite=Text · identity=Text · touchHeight=120
- `Network Puzzle HUD` / `PrototypeNetworkPuzzleHud` · session=Network Session · interact=Network Interact · prompt=Interaction Prompt · gate=PressurePlateDoor · plateStatus=Text · jump=Jump Button
- `Jump Button` / `PrototypeScreenButton` · m_ControlPath=<Gamepad>/buttonWest
- `Surface` / `PrototypeScreenButton` · m_ControlPath=<Gamepad>/buttonNorth
- `Network Session` / `PrototypeNetworkSession` · spawnPoints.Array.data[0]=Spawn 1 · spawnPoints.Array.data[1]=Spawn 2 · connectionTimeout=8 · swapAnimals=False
- `Network Session` / `PrototypeOnlineRoom` · environmentName=production · operationTimeout=30
- `Test Menu Return` / `PrototypeTestMenu` · mainMenu=False
- `Vine Wall` / `PrototypeClimbSurface` · front=(0.00, 0.00, -1.00) · front.x=0 · front.y=0 · front.z=-1 · landing=Landing
- `Level 1 Night Test` / `PrototypeNightEnvironment` · nightShader=Two Tails/Prototype Animal Vision · visibleRadius=1 · fadeWidth=3 · localLightFadeWidth=0.2 · nightBrightness=0.28 · nightTint=RGBA(0.380, 0.520, 0.750, 1.000) · foxBrightness=1.4 · foxGamma=1.15 · transparentShader=Two Tails/Prototype Visible Transparent · daylight=Directional Light · moonlight=Weak Moonlight · moonDirection=(65.00, -35.00, 0.00) · moonDirection.x=65 · moonDirection.y=-35 · moonDirection.z=0 · moonColor=RGBA(0.340, 0.460, 0.700, 1.000) · moonIntensity=0.12 · moonShadows=2 · suppressRealtimeShadowsAtNight=True · suppressScreenSpaceAmbientOcclusionAtNight=True
- `Air Catch Vine` / `PrototypeClimbSurface` · front=(0.00, 0.00, -1.00) · front.x=0 · front.y=0 · front.z=-1 · landing=Air Catch Landing
- `TT-P05 Switchback Gates` / `CooperativePuzzle` · kind=0 · networked=True · plateA=Plate A · plateB=Plate B · resetPad=Reset Tile · movingA=Gate A · movingB=Gate B · exitGate=null · rampA=null · rampB=null · plateVisualA=Plate A Visual · plateVisualB=Plate B Visual · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `TT-P05 Switchback Gates` / `CooperativePuzzleNetwork` · 
- `TT-P01 Bridge Handoff` / `CooperativePuzzle` · kind=1 · networked=True · plateA=Plate A · plateB=Plate B · resetPad=Reset Tile · movingA=Front Bridge · movingB=Rear Bridge · exitGate=null · rampA=null · rampB=null · plateVisualA=Plate A Visual · plateVisualB=Plate B Visual · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `TT-P01 Bridge Handoff` / `CooperativePuzzleNetwork` · 
- `TT-P02 Counterweight Lift` / `CooperativePuzzle` · kind=2 · networked=True · plateA=Left Load · plateB=Right Load · resetPad=Reset Tile · movingA=Left Lift · movingB=Right Lift · exitGate=Balance Exit Gate · rampA=Loading Ramp 0 · rampB=Loading Ramp 1 · plateVisualA=Left Lift · plateVisualB=Right Lift · boxes.Array.data[0]=Weight 2 Crate 0 · boxes.Array.data[1]=Weight 1 Crate 1 · boxes.Array.data[2]=Weight 1 Crate 2 · status=State · travelSpeed=1.7 · liftRange=0.35 · liftSpeed=0.25 · resetHold=2 · targetWeight=4 · pushSpeed=2
- `TT-P02 Counterweight Lift` / `CooperativePuzzleNetwork` · 
- `Weight 2 Crate 0` / `CooperativeWeightBox` · weight=2
- `Weight 1 Crate 1` / `CooperativeWeightBox` · weight=1
- `Weight 1 Crate 2` / `CooperativeWeightBox` · weight=1
- `Animal Mechanisms` / `AnimalMechanicsWorld` · heavyPushSpeed=1.5 · heavyFallHeight=-5 · heavyRespawnDelay=0.8 · savedRabbit=Rabbit Display · savedElephant=Elephant Display · savedStrikeBoard=Strike Board · savedRabbitVisual=Visual · savedElephantVisual=Visual · savedMonkey=Monkey Display · savedMonkeyVisual=Visual · savedHeavyBlock=Heavy Gap Block · networked=True · lobby=Safe Swap Lobby · displays.Array.data[0]=Otter Display · displays.Array.data[1]=Beaver Display · displayVisuals.Array.data[0]=Visual · displayVisuals.Array.data[1]=Visual · logs.Array.data[0]=Collectible Log 0 · logs.Array.data[1]=Collectible Log 1 · logs.Array.data[2]=Collectible Log 2 · dam=Dam · resetPoint=Animal Test Reset · damMarker=Dam Marker · installedLogs.Array.data[0]=Installed Log 0 · installedLogs.Array.data[1]=Installed Log 1 · installedLogs.Array.data[2]=Installed Log 2 · reservoir=Rising Reservoir · damLabel=Label · riseSpeed=0.3 · interactionDistance=1.5 · interactionAngle=45 · swapDuration=0.8
- `Animal Mechanisms` / `AnimalMechanicsNetwork` · 
- `Swimming Pool` / `PrototypeWaterVolume` · bottom=-4 · initialLevel=-0.6 · targetLevel=-0.6 · waterVisual=Water · safeShore=Safe Shore
- `Rising Reservoir` / `PrototypeWaterVolume` · bottom=-5 · initialLevel=-4 · targetLevel=0 · waterVisual=Water · safeShore=Safe Shore
- `Elephant Free Push Test Block` / `ElephantFreePushBlock` · pushSpeed=1.5 · fallHeight=-5 · respawnDelay=0.8 · useInitialTransform=True · respawnPoint=null
- `Climb Rope` / `PrototypeSwingRope` · online=True · length=5 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Rope A` / `PrototypeSwingRope` · online=True · length=5.6 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Rope B` / `PrototypeSwingRope` · online=True · length=5.6 · pushForce=2.4 · damping=0.35 · maxAngle=58 · climbSpeed=2 · visual=Rope Visual
- `Weight 2` / `PrototypeGripBlock` · weight=2 · online=True
- `Weight 3` / `PrototypeGripBlock` · weight=3 · online=True
### Prototype_Greybox — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · Greybox, Directional Light, PlayerPrototype, Main Camera, Prototype HUD, EventSystem, Prototype Debug Players
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Lever` / `PrototypeLever` · interactionPoint=InteractionPoint · handle=Handle · pullAngle=65 · door=DoorPanel · debounce=0.15
- `PressurePlateDoor` / `PrototypePlateGate` · plateA=Plate A · plateB=Plate B · door=DoorPanel
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Plate A` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Plate B` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `PlayerPrototype` / `PrototypeMotor` · visual=Visual · movementCamera=Main Camera · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `PlayerPrototype` / `PrototypeLocalInput` · actions=TwoTailsInput · actionMap=Gameplay · interactor=PlayerPrototype
- `PlayerPrototype` / `PrototypeInteractor` · interactionOrigin=InteractionOrigin · maximumDistance=1.5 · maximumAngle=45 · visibilityMask=Everything · levers.Array.data[0]=Lever
- `Main Camera` / `PrototypeFollowCamera` · target=PlayerPrototype · spawnFacing=Visual · pivotHeight=1.2 · distance=5 · minimumDistance=2 · maximumDistance=8 · zoomTime=0.12 · wheelSensitivity=0.5 · pinchSensitivity=6 · initialPitch=25 · minimumPitch=-10 · maximumPitch=65 · mouseSensitivity=0.22 · touchSensitivity=0.16 · invertY=False · smoothTime=0.16 · rotationTime=0.075 · obstructionMask=Default · obstructionRadius=0.2 · obstructionPadding=0.08 · recoveryTime=0.22
- `World Orbit Input` / `PrototypeOrbitDrag` · orbit=Main Camera
- `SafeArea` / `PrototypeSafeArea` · mobileFrameRate=30
- `StickHandle` / `PrototypeScreenStick` · m_MovementRange=68 · m_DynamicOriginRange=100 · m_ControlPath=<Gamepad>/leftStick · m_Behaviour=0 · m_UseIsolatedInputActions=False · m_PointerDownAction.m_Name= · m_PointerDownAction.m_Type=0 · m_PointerDownAction.m_ExpectedControlType= · m_PointerDownAction.m_Id=29f13f25-85eb-49d9-9d34-738187c97ea6 · m_PointerDownAction.m_Processors= · m_PointerDownAction.m_Interactions= · m_PointerDownAction.m_Flags=0 · m_PointerDownAction.m_Priority=0 · m_PointerMoveAction.m_Name= · m_PointerMoveAction.m_Type=0 · m_PointerMoveAction.m_ExpectedControlType= · m_PointerMoveAction.m_Id=2210ca19-eb72-4977-a5c4-2c2319310a58 · m_PointerMoveAction.m_Processors= · m_PointerMoveAction.m_Interactions= · m_PointerMoveAction.m_Flags=0 · m_PointerMoveAction.m_Priority=0
- `InteractButton` / `PrototypeScreenButton` · m_ControlPath=<Gamepad>/buttonSouth
- `Player1Prompt` / `PrototypeInteractionPrompt` · interactor=PlayerPrototype · debugPlayer=False
- `Player2Prompt` / `PrototypeInteractionPrompt` · interactor=DebugPlayer2 · debugPlayer=True
- `Prototype Debug Players` / `PrototypeDebugPlayers` · enablePlayer2=True · player2=DebugPlayer2
- `DebugPlayer2` / `PrototypeMotor` · visual=Visual · movementCamera=Main Camera · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `DebugPlayer2` / `PrototypeLocalInput` · actions=TwoTailsInput · actionMap=DebugPlayer2 · interactor=DebugPlayer2
- `DebugPlayer2` / `PrototypeInteractor` · interactionOrigin=InteractionOrigin · maximumDistance=1.5 · maximumAngle=45 · visibilityMask=Everything · levers.Array.data[0]=Lever
### Prototype_Network — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · Greybox, Directional Light, Main Camera, Prototype HUD, EventSystem, Network Session
- `LeverDoor` / `PrototypeNetworkDoor` · panel=DoorPanel · passage=Server Passage · playerLayers=Ignore Raycast
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Lever` / `PrototypeLever` · interactionPoint=InteractionPoint · handle=Handle · pullAngle=65 · door=null · debounce=0.15
- `Lever` / `PrototypeNetworkLever` · door=LeverDoor · maximumDistance=1.5 · maximumAngle=45 · serverCooldown=0.25 · visibilityMask=Default
- `PressurePlateDoor` / `PrototypeNetworkDoor` · panel=DoorPanel · passage=Server Passage · playerLayers=Ignore Raycast
- `PressurePlateDoor` / `PrototypeNetworkPlateGate` · plateA=Plate A · plateB=Plate B · playerLayers=Ignore Raycast
- `DoorPanel` / `PrototypeDoor` · openHeight=3 · openDuration=0.6 · characterLayers=Ignore Raycast
- `Plate A` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Plate B` / `PrototypePressurePlate` · visual=Visual · pressDepth=0.025 · inactiveColor=RGBA(1.000, 0.780, 0.080, 1.000) · activeColor=RGBA(0.160, 0.840, 0.260, 1.000) · occupantLayers=Everything
- `Main Camera` / `PrototypeFollowCamera` · target=null · spawnFacing=null · pivotHeight=1.2 · distance=5 · minimumDistance=2 · maximumDistance=8 · zoomTime=0.12 · wheelSensitivity=0.5 · pinchSensitivity=6 · initialPitch=25 · minimumPitch=-10 · maximumPitch=65 · mouseSensitivity=0.22 · touchSensitivity=0.16 · invertY=False · smoothTime=0.16 · rotationTime=0.075 · obstructionMask=Default · obstructionRadius=0.2 · obstructionPadding=0.08 · recoveryTime=0.22
- `World Orbit Input` / `PrototypeOrbitDrag` · orbit=Main Camera
- `SafeArea` / `PrototypeSafeArea` · mobileFrameRate=30
- `StickHandle` / `PrototypeScreenStick` · m_MovementRange=68 · m_DynamicOriginRange=100 · m_ControlPath=<Gamepad>/leftStick · m_Behaviour=0 · m_UseIsolatedInputActions=False · m_PointerDownAction.m_Name= · m_PointerDownAction.m_Type=0 · m_PointerDownAction.m_ExpectedControlType= · m_PointerDownAction.m_Id=29f13f25-85eb-49d9-9d34-738187c97ea6 · m_PointerDownAction.m_Processors= · m_PointerDownAction.m_Interactions= · m_PointerDownAction.m_Flags=0 · m_PointerDownAction.m_Priority=0 · m_PointerMoveAction.m_Name= · m_PointerMoveAction.m_Type=0 · m_PointerMoveAction.m_ExpectedControlType= · m_PointerMoveAction.m_Id=2210ca19-eb72-4977-a5c4-2c2319310a58 · m_PointerMoveAction.m_Processors= · m_PointerMoveAction.m_Interactions= · m_PointerMoveAction.m_Flags=0 · m_PointerMoveAction.m_Priority=0
- `Connection` / `PrototypeNetworkHud` · session=Network Session · followCamera=Main Camera · orbit=World Orbit Input · stick=StickBase · address=Address · port=Port · host=Start Host · client=Start Client · disconnect=Disconnect · status=Text · players=Text · online=Network Session · directControls=Direct Test Controls · onlineControls=Online Relay Controls · switchMode=Direct Test · modeLabel=Text · title=Text · joinCode=Join Code · createRoom=Create Room · joinRoom=Join Room · copyCode=Copy Code · leaveRoom=Leave Room · leaveLabel=Text · invite=Text · identity=Text · touchHeight=120
- `Network Puzzle HUD` / `PrototypeNetworkPuzzleHud` · session=Network Session · interact=Network Interact · prompt=Interaction Prompt · gate=PressurePlateDoor · plateStatus=Text · jump=null
- `Network Session` / `PrototypeNetworkSession` · spawnPoints.Array.data[0]=Spawn 1 · spawnPoints.Array.data[1]=Spawn 2 · connectionTimeout=8 · swapAnimals=False
- `Network Session` / `PrototypeOnlineRoom` · environmentName=production · operationTimeout=30
### PrototypePlayer — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · PrototypePlayer
- `PrototypePlayer` / `PrototypeMotor` · visual=Visual · movementCamera=null · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `PrototypePlayer` / `PrototypeLocalInput` · actions=TwoTailsInput · actionMap=Gameplay · interactor=PrototypePlayer
- `PrototypePlayer` / `PrototypeInteractor` · interactionOrigin=InteractionOrigin · maximumDistance=1.5 · maximumAngle=45 · visibilityMask=Everything
### NetworkPlayerPrototype — missing scripts 0, broken prototype references 0, repeated MonoBehaviour types 0
Roots · NetworkPlayerPrototype
- `NetworkPlayerPrototype` / `PrototypeMotor` · visual=Visual · movementCamera=null · speed=4 · turnSpeed=720 · gravity=24 · resetHeight=-5 · jumpHeight=1.2 · climbDistance=0.4 · climbAngle=45 · climbSpeed=2 · rabbitJumpMultiplier=2 · scaleRabbitPlaceholder=True
- `NetworkPlayerPrototype` / `PrototypeLocalInput` · actions=TwoTailsInput · actionMap=Gameplay · interactor=null
- `NetworkPlayerPrototype` / `NetworkPlayerPrototype` · capsule=Visual · hostMaterial=PrototypeBlue · clientMaterial=PrototypeRed · serverProxy=Server Puzzle Proxy
- `NetworkPlayerPrototype` / `PrototypeNetworkInteraction` · interactionOrigin=InteractionOrigin
- `NetworkPlayerPrototype` / `PrototypeAnimal` · species=0
- `NetworkPlayerPrototype` / `AnimalMechanicsActor` · 
- `NetworkPlayerPrototype` / `PrototypeSwimmer` · swimSpeed=3 · diveSpeed=2 · surfaceSpeed=2 · surfaceOffset=0.8 · floorClearance=0.15
