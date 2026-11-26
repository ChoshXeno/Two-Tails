"""Create isolated first-pass actions from the original Two Tails FBX rigs.

Run with Blender 5.3 in background mode. Original FBX files are read only.
"""
import math
import os
import sys

import bpy
from mathutils import Vector


ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
OUT = os.path.join(ROOT, "Assets", "Art", "AnimationTests")
PREVIEW = os.path.join(ROOT, "Docs", "AnimationPreviews")


def pose(arm, frame, rotations, offsets=None):
    bpy.context.scene.frame_set(frame)
    for name, angles in rotations.items():
        bone = arm.pose.bones.get(name)
        if bone is None:
            continue
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = tuple(math.radians(v) for v in angles)
        bone.keyframe_insert(data_path="rotation_euler", frame=frame, group=name)
    for name, position in (offsets or {}).items():
        bone = arm.pose.bones.get(name)
        if bone is None:
            continue
        bone.location = position
        bone.keyframe_insert(data_path="location", frame=frame, group=name)


def camera_and_lights():
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 24
    scene.render.resolution_x = 640
    scene.render.resolution_y = 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("Preview World")
    scene.world.color = (0.20, 0.24, 0.30)
    bpy.ops.object.camera_add(location=(3.2, -4.4, 2.3))
    camera = bpy.context.object
    target = Vector((0, 0, 0.08))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 2.45
    scene.camera = camera
    for location, power, size in [((2, -3, 4), 450, 4), ((-3, 1, 3), 300, 3)]:
        bpy.ops.object.light_add(type="AREA", location=location)
        lamp = bpy.context.object
        lamp.data.energy = power
        lamp.data.shape = "DISK"
        lamp.data.size = size
        lamp.rotation_euler = (target - lamp.location).to_track_quat("-Z", "Y").to_euler()


def author(species, source, action_name, frames, poses, previews):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT, source))
    arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
    mesh = next(o for o in bpy.data.objects if o.type == "MESH")
    for bone in arm.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0, 0, 0)
        bone.location = (0, 0, 0)
    if arm.animation_data:
        arm.animation_data.action = None
    action = bpy.data.actions.new(action_name)
    arm.animation_data_create()
    arm.animation_data.action = action
    scene = bpy.context.scene
    scene.render.fps = 24
    scene.frame_start, scene.frame_end = frames
    for frame, rotations, offsets in poses:
        pose(arm, frame, rotations, offsets)
    for slot in action.slots:
        for bag in action.layers[0].strips[0].channelbags:
            for curve in bag.fcurves:
                for key in curve.keyframe_points:
                    key.interpolation = "BEZIER"
    scene.frame_set(frames[0])
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(PREVIEW, exist_ok=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, species + "_test_v1.blend"))
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    mesh.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(OUT, species + "_" + action_name + "_test_v1.fbx"),
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=False,
        bake_anim_step=1.0,
        path_mode="COPY",
        embed_textures=True,
    )
    camera_and_lights()
    for frame in previews:
        scene.frame_set(frame)
        scene.render.filepath = os.path.join(PREVIEW, f"{species}_{action_name}_{frame:02d}.png")
        bpy.ops.render.render(write_still=True)
    print("CREATED", species, action_name, len(arm.data.bones), len(mesh.data.vertices))


if __name__ == "__main__":
    fox_base = {"chest": (0, 0, 0), "neck_01": (0, 0, 0), "head": (0, 0, 0),
                "frontUpr.L": (0, 0, 0), "frontUpr.R": (0, 0, 0),
                "frontLwr.L": (0, 0, 0), "frontLwr.R": (0, 0, 0),
                "tail_01": (0, 0, 0)}
    def fox(chest, neck, head, left, right, tail):
        return {**fox_base, "chest": (chest, 0, 0), "neck_01": (neck, 0, 0),
                "head": (head, 0, 0), "frontUpr.L": (left, 0, 0),
                "frontUpr.R": (right, 0, 0), "frontLwr.L": (-left*.55, 0, 0),
                "frontLwr.R": (-right*.55, 0, 0), "tail_01": (tail, 0, 0)}
    author("Fox", "Assets/Prefabs/Fox/Fox_character.fbx", "Interact", (1, 18), [
        (1, fox(0, 0, 0, 0, 0, 0), None),
        (5, fox(-5, 8, 4, -16, -6, 4), None),
        (10, fox(5, -9, -8, 25, 3, -5), None),
        (14, fox(2, -4, -3, 9, 2, 2), None),
        (18, fox(0, 0, 0, 0, 0, 0), None),
    ], (1, 5, 10, 14, 18))
    def monkey(chest, left, right, rear_left, rear_right, tail):
        return {"chest": (chest, 0, 0), "clavicle.L": (0, 0, left*.35),
                "clavicle.R": (0, 0, right*.35),
                "frontLwr.L": (left, 0, 0), "frontLwr.R": (right, 0, 0),
                "frontHorselink.L": (-left*.35, 0, 0),
                "frontHorselink.R": (-right*.35, 0, 0),
                "rearUpr.L": (rear_left, 0, 0), "rearUpr.R": (rear_right, 0, 0),
                "rearLwr.L": (-rear_left*.4, 0, 0),
                "rearLwr.R": (-rear_right*.4, 0, 0),
                "tail_01": (tail, 0, 0), "tail_02": (-tail*.7, 0, 0)}
    author("Monkey", "Assets/Prefabs/Monkey.fbx", "Climb_Loop", (1, 25), [
        (1, monkey(5, -27, 22, 18, -12, 9), None),
        (7, monkey(-3, -2, 34, 2, -25, -7), None),
        (13, monkey(5, 22, -27, -12, 18, 9), None),
        (19, monkey(-3, 34, -2, -25, 2, -7), None),
        (25, monkey(5, -27, 22, 18, -12, 9), None),
    ], (1, 7, 13, 19, 25))
