"""Builds Reactor Watch into Assets/Scenes/SampleScene.unity (pass 2, with the skills).

Starts from the scene backup taken before any Reactor Watch object existed, so it can run again.
Reuses the guid of every asset that already has a .meta; new assets get a new guid.
Everything is built in memory and written once at the end (unity-scene-yaml rule 3).
Usage: python Tools/GeneratorScripts/build_reactor_watch.py <base scene backup>
"""
import glob, math, os, sys, uuid

PROJECT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
os.chdir(PROJECT)
BASE_SCENE = sys.argv[1]
SCENE = "Assets/Scenes/SampleScene.unity"
R = "Assets/ReactorWatch"
UI = R + "/UI"
PS_SAMPLE = (r"C:\Program Files\Unity\Hub\Editor\6000.3.5f2\Editor\Data\Resources\PackageManager\BuiltInPackages"
             r"\com.unity.ui.test-framework\Samples~\DocCodeSamples.Tests\Runtime\Resources\UITestFrameworkDocSamplePanelSettings.asset")
BUILTIN = "0000000000000000e000000000000000"
LIT_MAT = "31321ba15b8f8eb4c954353edc038b1d"      # URP Lit.mat (package)
UXML_ID = 9197481963319205126
TSS_ID = -4733365628477956816

files = {}  # path -> text, written at the end


def read_guid(meta_path):
    for line in open(meta_path, encoding="utf-8"):
        if line.startswith("guid: "):
            return line.split()[1]
    raise AssertionError("no guid in " + meta_path)


def asset_guid(path, meta_text_for):
    """guid of an existing asset, or a new guid plus a new .meta to write."""
    if os.path.exists(path + ".meta"):
        return read_guid(path + ".meta")
    guid = uuid.uuid4().hex
    files[path + ".meta"] = meta_text_for(guid)
    return guid


def meta_cs(guid):
    return (f"fileFormatVersion: 2\nguid: {guid}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n"
            "  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


def meta_native(main):
    return lambda guid: (f"fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: {main}\n"
                         "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


def meta_scripted(script, extra=""):
    return lambda guid: (f"fileFormatVersion: 2\nguid: {guid}\nScriptedImporter:\n  internalIDToNameTable: []\n  externalObjects: {{}}\n"
                         f"  serializedVersion: 2\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
                         f"  script: {{fileID: {script}, guid: {BUILTIN}, type: 0}}\n" + extra)


def meta_folder(guid):
    return (f"fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


# ---------- assets ----------
for folder in [R, R + "/Scripts", R + "/Data", R + "/Materials", UI]:
    assert os.path.isdir(folder), folder
    asset_guid(folder, meta_folder)

cs = {}
for path in [R + "/Scripts/ReactorConfig.cs", R + "/Scripts/Valve.cs", R + "/Scripts/ReactorGame.cs",
             R + "/Scripts/PlayerController.cs", UI + "/ReactorWatchHUD.cs", UI + "/ValveWorldLabel.cs"]:
    assert os.path.exists(path), path
    cs[os.path.basename(path)[:-3]] = asset_guid(path, meta_cs)

# DA_Reactor: every game number (written once in pass 1, kept as is)
DA = R + "/Data/DA_Reactor.asset"
assert os.path.exists(DA)
da_guid = read_guid(DA + ".meta")
assert f"guid: {cs['ReactorConfig']}" in open(DA, encoding="utf-8").read()
core_mat = read_guid(R + "/Materials/M_Core.mat.meta")
valve_mat = read_guid(R + "/Materials/M_Valve.mat.meta")

uxml = {}
for name in ["WBP_HUD", "WBP_PauseMenu", "WBP_EndScreen"]:
    p = f"{UI}/{name}.uxml"
    assert os.path.exists(p), p
    uxml[name] = asset_guid(p, meta_scripted(13804))
assert os.path.exists(UI + "/ReactorWatch.uss")
asset_guid(UI + "/ReactorWatch.uss", lambda g: f"fileFormatVersion: 2\nguid: {g}\n")

TSS = UI + "/ReactorWatchTheme.tss"
if not os.path.exists(TSS):
    files[TSS] = '@import url("unity-theme://default");\nVisualElement {}\n'
tss_guid = asset_guid(TSS, meta_scripted(12388, "  disableValidation: 0\n  unsupportedSelectorAction: 0\n"))

# PanelSettings (ui-hud-kit rule 6): scale with screen height, 1280x720 reference
PS = UI + "/PS_ReactorWatchHUD.asset"
ps = open(PS_SAMPLE, encoding="utf-8").read()
for a, b in [("m_Name: UITestFrameworkDocSamplePanelSettings", "m_Name: PS_ReactorWatchHUD"),
             ("themeUss: {fileID: 0}", f"themeUss: {{fileID: {TSS_ID}, guid: {tss_guid}, type: 3}}"),
             ("m_ScaleMode: 0", "m_ScaleMode: 2"),
             ("m_ReferenceResolution: {x: 1200, y: 800}", "m_ReferenceResolution: {x: 1280, y: 720}"),
             ("m_ScreenMatchMode: 0", "m_ScreenMatchMode: 0"),
             ("m_Match: 0", "m_Match: 1")]:
    assert ps.count(a) == 1, a
    ps = ps.replace(a, b)
files[PS] = ps
ps_guid = asset_guid(PS, meta_native(11400000))

# ---------- scene ----------
s = open(BASE_SCENE, encoding="utf-8", newline="").read()
assert "\r\n" not in s and "m_Name: Valve_" not in s and "m_Name: Main Camera" in s
nid = [2051390000]


def nxt():
    nid[0] += 1
    assert f"&{nid[0]}" not in s
    return nid[0]


HDR = "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
COLL = ("  m_Material: {fileID: 0}\n  m_IncludeLayers:\n    serializedVersion: 2\n    m_Bits: 0\n  m_ExcludeLayers:\n"
        "    serializedVersion: 2\n    m_Bits: 0\n  m_LayerOverridePriority: 0\n  m_IsTrigger: 0\n  m_ProvidesContacts: 0\n  m_Enabled: 1\n")
blocks, roots = [], []


def go(gid, name, comps):
    return (f"--- !u!1 &{gid}\nGameObject:\n{HDR}  serializedVersion: 6\n  m_Component:\n"
            + "".join(f"  - component: {{fileID: {c}}}\n" for c in comps)
            + f"  m_Layer: 0\n  m_Name: {name}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n"
              "  m_StaticEditorFlags: 0\n  m_IsActive: 1\n")


def tr(tid, gid, pos, scale=(1, 1, 1), father=0, children=(), yaw=0):
    h = math.radians(yaw) / 2
    ch = "  m_Children: []\n" if not children else "  m_Children:\n" + "".join(f"  - {{fileID: {c}}}\n" for c in children)
    return (f"--- !u!4 &{tid}\nTransform:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n  serializedVersion: 2\n"
            f"  m_LocalRotation: {{x: 0, y: {round(math.sin(h), 7)}, z: 0, w: {round(math.cos(h), 7)}}}\n"
            f"  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}\n"
            f"  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}\n  m_ConstrainProportionsScale: 0\n"
            f"{ch}  m_Father: {{fileID: {father}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: {yaw}, z: 0}}\n")


def mf(i, gid, mesh):
    return f"--- !u!33 &{i}\nMeshFilter:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n  m_Mesh: {{fileID: {mesh}, guid: {BUILTIN}, type: 0}}\n"


def mr(i, gid, material, shadows=1):
    return (f"--- !u!23 &{i}\nMeshRenderer:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_CastShadows: {shadows}\n"
            f"  m_ReceiveShadows: {shadows}\n  m_DynamicOccludee: 1\n  m_StaticShadowCaster: 0\n  m_MotionVectors: 1\n"
            "  m_LightProbeUsage: 1\n  m_ReflectionProbeUsage: 1\n  m_RayTracingMode: 2\n  m_RayTraceProcedural: 0\n"
            "  m_RenderingLayerMask: 1\n  m_RendererPriority: 0\n  m_Materials:\n"
            f"  - {material}\n  m_StaticBatchInfo:\n    firstSubMesh: 0\n    subMeshCount: 0\n"
            "  m_StaticBatchRoot: {fileID: 0}\n  m_ProbeAnchor: {fileID: 0}\n  m_LightProbeVolumeOverride: {fileID: 0}\n"
            "  m_ScaleInLightmap: 1\n  m_ReceiveGI: 1\n  m_PreserveUVs: 0\n  m_IgnoreNormalsForChartDetection: 0\n"
            "  m_ImportantGI: 0\n  m_StitchLightmapSeams: 1\n  m_SelectedEditorRenderState: 3\n  m_MinimumChartSize: 4\n"
            "  m_AutoUVMaxDistance: 0.5\n  m_AutoUVMaxAngle: 89\n  m_LightmapParameters: {fileID: 0}\n  m_SortingLayerID: 0\n"
            "  m_SortingLayer: 0\n  m_SortingOrder: 0\n  m_AdditionalVertexStreams: {fileID: 0}\n")


def mat_ref(guid): return f"{{fileID: 2100000, guid: {guid}, type: 2}}"


FONT_MAT = f"{{fileID: 10100, guid: {BUILTIN}, type: 0}}"   # verified in URP sample 2Podium.prefab


def box(i, gid):
    return (f"--- !u!65 &{i}\nBoxCollider:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n{COLL}  serializedVersion: 3\n"
            "  m_Size: {x: 1, y: 1, z: 1}\n  m_Center: {x: 0, y: 0, z: 0}\n")


def capsule(i, gid):
    return (f"--- !u!136 &{i}\nCapsuleCollider:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n{COLL}  serializedVersion: 2\n"
            "  m_Radius: 0.5\n  m_Height: 2\n  m_Direction: 1\n  m_Center: {x: 0, y: 0, z: 0}\n")


def rgba(r, g, b, a=255): return (a << 24) | (b << 16) | (g << 8) | r


def textmesh(i, gid, text, size, bold, color):
    return (f"--- !u!102 &{i}\nTextMesh:\n  serializedVersion: 3\n{HDR}  m_GameObject: {{fileID: {gid}}}\n  m_Text: {text}\n"
            "  m_OffsetZ: 0\n  m_CharacterSize: 0.06\n  m_LineSpacing: 1\n  m_Anchor: 4\n  m_Alignment: 1\n  m_TabSize: 4\n"
            f"  m_FontSize: {size}\n  m_FontStyle: {1 if bold else 0}\n  m_RichText: 1\n"
            f"  m_Font: {{fileID: 10102, guid: {BUILTIN}, type: 0}}\n  m_Color:\n    serializedVersion: 2\n    rgba: {color}\n")


def mono(i, gid, script, fields, ident=""):
    return (f"--- !u!114 &{i}\nMonoBehaviour:\n{HDR}  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {script}\n  m_Name: \n  m_EditorClassIdentifier: {ident}\n" + fields)


def script(name): return f"{{fileID: 11500000, guid: {cs[name]}, type: 3}}"


def mesh_obj(name, pos, scale, mesh, material, father, collider="box", yaw=0, extra=(), children=()):
    gid, tid, f, r, c = nxt(), nxt(), nxt(), nxt(), nxt()
    blocks.extend([go(gid, name, [tid, f, r, c, *extra]), tr(tid, gid, pos, scale, father, children, yaw),
                   mf(f, gid, mesh), mr(r, gid, material),
                   box(c, gid) if collider == "box" else capsule(c, gid)])
    return gid, tid


# Ids decided up front for the objects that others point to
gm_g, gm_t, gm_m = nxt(), nxt(), nxt()

# Room: 2000 x 2000 cm floor, walls 4 m high, no ceiling (the sun lights it)
room_g, room_t = nxt(), nxt()
blocks += [go(room_g, "Room", [room_t]), None]
room_tr_at = len(blocks) - 1
kids = [mesh_obj(n, p, sc, 10202, mat_ref(LIT_MAT), room_t)[1] for n, p, sc in [
    ("Floor", (0, -0.1, 0), (20, 0.2, 20)),
    ("Wall_North", (0, 2, 10.1), (20.4, 4, 0.2)), ("Wall_South", (0, 2, -10.1), (20.4, 4, 0.2)),
    ("Wall_East", (10.1, 2, 0), (0.2, 4, 20.4)), ("Wall_West", (-10.1, 2, 0), (0.2, 4, 20.4))]]
blocks[room_tr_at] = tr(room_t, room_g, (0, 0, 0), children=kids)
roots.append(room_t)

# Core: glowing cylinder, 3 m wide and 4 m high
roots.append(mesh_obj("Core", (0, 2, 0), (3, 2, 3), 10206, mat_ref(core_mat), 0, collider="capsule")[1])

# Valves A (north), B (east), C (west), each with a world label 1.3 m above it (ui-hud-kit rule 4)
valve_monos = []
for vid, pos, yaw in [("A", (0, 1.3, 9.75), 0), ("B", (9.75, 1.3, 0), 90), ("C", (-9.75, 1.3, 0), -90)]:
    v_mono = nxt()
    lab_g, lab_t, lab_r, lab_tm, lab_m = nxt(), nxt(), nxt(), nxt(), nxt()
    st_g, st_t, st_r, st_tm = nxt(), nxt(), nxt(), nxt()
    v_g, v_t = mesh_obj(f"Valve_{vid}", pos, (1, 1, 0.5), 10202, mat_ref(valve_mat), 0, yaw=yaw,
                        extra=(v_mono,), children=(lab_t,))
    blocks.append(mono(v_mono, v_g, script("Valve"),
                       f"  valveId: {vid}\n  config: {{fileID: 11400000, guid: {da_guid}, type: 2}}\n"
                       f"  valveRenderer: {{fileID: {v_g + 3}}}\n"))
    # Label in front of the wall (local z -1 = 0.5 m because the valve is 0.5 deep), letter on top, status under it
    blocks += [go(lab_g, "Label", [lab_t, lab_r, lab_tm, lab_m]),
               tr(lab_t, lab_g, (0, 1.3, -1), father=v_t, children=(st_t,)),
               mr(lab_r, lab_g, FONT_MAT, shadows=0),
               textmesh(lab_tm, lab_g, vid, 96, True, rgba(255, 255, 255)),
               mono(lab_m, lab_g, script("ValveWorldLabel"),
                    f"  game: {{fileID: {gm_m}}}\n  valve: {{fileID: {v_mono}}}\n  statusText: {{fileID: {st_tm}}}\n"),
               go(st_g, "Status", [st_t, st_r, st_tm]),
               tr(st_t, st_g, (0, -0.4, 0), father=lab_t),
               mr(st_r, st_g, FONT_MAT, shadows=0),
               textmesh(st_tm, st_g, "READY", 48, False, rgba(110, 210, 125))]
    valve_monos.append(v_mono)
    roots.append(v_t)
valve_list = "".join(f"  - {{fileID: {m}}}\n" for m in valve_monos)

# Game: heat, time, pause, end
blocks += [go(gm_g, "ReactorGame", [gm_t, gm_m]), tr(gm_t, gm_g, (0, 0, 0)),
           mono(gm_m, gm_g, script("ReactorGame"),
                f"  config: {{fileID: 11400000, guid: {da_guid}, type: 2}}\n  valves:\n{valve_list}")]
roots.append(gm_t)

# Player with the Main Camera as its child, at eye height
CAM_T = 330585546
pl_g, pl_t, pl_cc, pl_m = nxt(), nxt(), nxt(), nxt()
blocks += [go(pl_g, "Player", [pl_t, pl_cc, pl_m]), tr(pl_t, pl_g, (0, 1, -7), children=[CAM_T]),
           f"--- !u!143 &{pl_cc}\nCharacterController:\n{HDR}  m_GameObject: {{fileID: {pl_g}}}\n{COLL}  serializedVersion: 3\n"
           "  m_Height: 2\n  m_Radius: 0.4\n  m_SlopeLimit: 45\n  m_StepOffset: 0.3\n  m_SkinWidth: 0.08\n"
           "  m_MinMoveDistance: 0.001\n  m_Center: {x: 0, y: 0, z: 0}\n",
           mono(pl_m, pl_g, script("PlayerController"),
                f"  game: {{fileID: {gm_m}}}\n  cameraPivot: {{fileID: {CAM_T}}}\n  valves:\n{valve_list}")]
roots.append(pl_t)

# HUD: UIDocument in the scene (ui-hud-kit rule 1)
hud_g, hud_t, hud_doc, hud_m = nxt(), nxt(), nxt(), nxt()
blocks += [go(hud_g, "HUD", [hud_t, hud_doc, hud_m]), tr(hud_t, hud_g, (0, 0, 0)),
           mono(hud_doc, hud_g, f"{{fileID: 19102, guid: {BUILTIN}, type: 0}}",
                f"  m_PanelSettings: {{fileID: 11400000, guid: {ps_guid}, type: 2}}\n  m_ParentUI: {{fileID: 0}}\n"
                f"  sourceAsset: {{fileID: {UXML_ID}, guid: {uxml['WBP_HUD']}, type: 3}}\n  m_SortingOrder: 0\n  m_Position: 0\n"
                "  m_WorldSpaceSizeMode: 1\n  m_WorldSpaceWidth: 1920\n  m_WorldSpaceHeight: 1080\n  m_PivotReferenceSize: 0\n"
                "  m_Pivot: 0\n  m_WorldSpaceCollider: {fileID: 0}\n",
                ident="UnityEngine.dll::UnityEngine.UIElements.UIDocument"),
           mono(hud_m, hud_g, script("ReactorWatchHUD"),
                f"  document: {{fileID: {hud_doc}}}\n  game: {{fileID: {gm_m}}}\n  player: {{fileID: {pl_m}}}\n"
                f"  pauseMenuAsset: {{fileID: {UXML_ID}, guid: {uxml['WBP_PauseMenu']}, type: 3}}\n"
                f"  endScreenAsset: {{fileID: {UXML_ID}, guid: {uxml['WBP_EndScreen']}, type: 3}}\n")]
roots.append(hud_t)

# Move the Main Camera under the player, looking straight ahead
a = s.index(f"--- !u!4 &{CAM_T}\n")
b = s.index("--- ", a + 5)
cam = s[a:b]
for old, new in [("m_LocalRotation: {x: 0.08715574, y: 0, z: 0, w: 0.9961947}", "m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}"),
                 ("m_LocalPosition: {x: 0, y: 1, z: -5}", "m_LocalPosition: {x: 0, y: 0.6, z: 0}"),
                 ("m_Father: {fileID: 0}", f"m_Father: {{fileID: {pl_t}}}"),
                 ("m_LocalEulerAnglesHint: {x: 10, y: 0, z: 0}", "m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}")]:
    assert cam.count(old) == 1, old
    cam = cam.replace(old, new)
s = s[:a] + cam + s[b:]
assert s.count(f"  - {{fileID: {CAM_T}}}\n") == 1
s = s.replace(f"  - {{fileID: {CAM_T}}}\n", "")

marker = "--- !u!1660057539 &9223372036854775807"
assert s.count(marker) == 1 and s.endswith("\n") and None not in blocks
s = s.replace(marker, "".join(blocks) + marker) + "".join(f"  - {{fileID: {t}}}\n" for t in roots)
assert s.count("m_Name: Valve_") == 3 and s.count("m_Name: Label\n") == 3 and s.count("m_Name: HUD\n") == 1

# ---------- write everything once ----------
for p, t in files.items():
    os.makedirs(os.path.dirname(p), exist_ok=True)
    open(p, "w", encoding="utf-8", newline="\n").write(t)
open(SCENE, "w", encoding="utf-8", newline="").write(s)
print("wrote", len(files), "asset files + scene")
for p in sorted(files):
    print("  ", p)
