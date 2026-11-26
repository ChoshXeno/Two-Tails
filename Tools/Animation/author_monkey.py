"""Author Monkey actions on a private copy of the source rig (Blender 5.3)."""
import math
import os

import bpy
from mathutils import Matrix, Quaternion, Vector

from author_test_clips import ROOT

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT, "Assets/Prefabs/Monkey.fbx"))
arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
scene = bpy.context.scene
scene.render.fps = 24
arm.animation_data.action = None
bones = arm.pose.bones
for bone in bones:
    bone.rotation_mode = "QUATERNION"

def rotate(name, axis, degrees):
    bone = bones[name]
    local_axis = bone.bone.matrix_local.to_3x3().inverted() @ Vector(axis)
    bone.rotation_quaternion = Quaternion(local_axis, math.radians(degrees))

def aim(name, child, direction):
    bpy.context.view_layer.update()
    bone = bones[name]
    delta = (bones[child].head - bone.head).rotation_difference(Vector(direction))
    matrix = delta.to_matrix().to_4x4() @ bone.matrix.to_3x3().to_4x4()
    matrix.translation = bone.head
    bone.matrix = matrix

def pose(kind, phase, strength):
    for bone in bones:
        bone.matrix_basis = Matrix.Identity(4)
    beat = math.sin(phase)
    lift = math.cos(phase)
    pitch = {"Wall":72,"Vine":75,"Sit":64,"Air":69,"End":65,"Death":10}.get(kind, 0)
    rotate("hips", (1,0,0), -pitch * strength)
    if kind == "Death":
        rotate("hips", (0,0,1), 82 * strength)
    rotate("tail_01", (0,1,0), beat * 10)
    rotate("neck_01", (1,0,0), (18 if kind in ("Wall","Vine") else 3) * strength + lift * 2)
    for side, sign in (("L",1),("R",-1)):
        suffix = "." + side
        if kind in ("Wall","Vine","Air","Sit","End"):
            high = sign * beat
            width = {"Wall":.55,"Vine":.12,"Air":.8,"Sit":.18,"End":.35}[kind]
            reach = .65 if kind in ("Wall","Vine","End") else .24
            height = {"Wall":.65,"Vine":.95,"Air":.6,"Sit":-.85,"End":.75}[kind]
            aim("clavicle"+suffix, "frontLwr"+suffix, (sign*width,height+high*.55,reach))
            aim("frontLwr"+suffix, "frontHorselink"+suffix,
                (-sign*.30 if kind=="Vine" else sign*width*.35,height+high*.45,reach))
            aim("rearUpr"+suffix, "rearLwr"+suffix,(sign*.5,-.55-high*.35,.48))
            aim("rearLwr"+suffix, "rearHorselink"+suffix,(-sign*.15,-.8+high*.25,.42))
        else:
            swing = sign * beat * strength
            rotate("clavicle"+suffix,(1,0,0),swing*33)
            rotate("frontLwr"+suffix,(1,0,0),-max(0,swing)*38)
            rotate("rearUpr"+suffix,(1,0,0),-swing*36)
            rotate("rearLwr"+suffix,(1,0,0),max(0,-swing)*35)
    bpy.context.view_layer.update()

specs = [
    ("Idle",48,"Ground",.035,True),("Run",14,"Ground",1,True),
    ("Jump_Start",7,"Start",.8,False),("Jump_Air",16,"Air",1,True),
    ("Jump_Land",9,"Land",.75,False),("Interact",16,"Touch",.85,False),
    ("Death",24,"Death",1,False),("Sit",48,"Sit",1,True),
    ("Wall_Hang_Idle",36,"Wall",1,True),("Climb_Loop",24,"Wall",1,True),
    ("Climb_End",13,"End",1,False),("Vine_Hang_Idle",36,"Vine",1,True),
    ("Vine_Climb",24,"Vine",1,True),("Push",18,"Ground",.48,True),
    ("Pull",20,"Ground",.42,True),
]
actions = []
for name, duration, kind, amount, loop in specs:
    action = bpy.data.actions.new("Monkey_" + name)
    action.use_fake_user = True
    arm.animation_data.action = action
    for i in range(9):
        t = i/8
        frame = 1 + t*duration
        phase = t*2*math.pi
        strength = amount
        if "Hang_Idle" in name or name in ("Sit","Jump_Air"):
            phase = .6 + math.sin(phase)*.08
        if not loop:
            strength *= t if name == "Death" else (1-t if name=="Climb_End" else math.sin(math.pi*t))
            if name == "Climb_End" and t == 1:
                kind = "Ground"
        pose(kind,phase,strength)
        if name in ("Jump_Start","Jump_Land"):
            rotate("chest",(1,0,0),strength*(18 if name=="Jump_Start" else -16))
            rotate("rearUpr.L",(1,0,0),strength*44)
            rotate("rearUpr.R",(1,0,0),strength*44)
        elif name == "Interact":
            rotate("clavicle.L",(1,0,0),-strength*62)
        for bone in bones:
            bone.keyframe_insert(data_path="rotation_quaternion",frame=frame,group=bone.name)
    actions.append(action)
arm.animation_data.action = None
for action in actions:
    track = arm.animation_data.nla_tracks.new()
    track.name = action.name
    track.strips.new(action.name,1,action)
scene.frame_set(1)
folder = os.path.join(ROOT,"Assets","Art","Finished Animation","Monkey")
bpy.context.preferences.filepaths.save_version = 0
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,"Tools","Animation","Monkey_Actions_v1.blend"))
bpy.ops.export_scene.fbx(filepath=os.path.join(folder,"Monkey_Actions.fbx"),
    object_types={"ARMATURE","MESH"}, add_leaf_bones=False,
    bake_anim_use_nla_strips=True,bake_anim_use_all_actions=False,
    bake_anim_simplify_factor=0.0)
