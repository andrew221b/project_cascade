# CASCADE — Unity project

Unity: **6000.3.8f1** (`1c7db571dde0`). Initial template: local **3D URP**
(`com.unity.template.urp-blank`, `17.0.14`).

Open this directory with the pinned Editor. The package root one directory above
contains the authoritative `AGENTS.md`, `TASKS.md`, `Docs/`, `Prompts/`, `Data/`,
`Tools/` and `Reports/`. Read those instructions before making changes.
Keep those source documents outside `Assets/`; do not duplicate them here.

Game source belongs in `Assets/_Cascade/`. P00 establishes the environment only;
no CASCADE gameplay or custom visual assets have been implemented yet.
`Assets/Scenes/SampleScene.unity` is the unchanged template scene, not a playable.

Shapes, Words and Mixed share a pure C# SymbolId core. English is the default,
with Russian and Ukrainian supported. One persistent row moves between floors
and must exactly match the receiver. No matching, clearing, new-piece spawning,
grid gameplay or gameplay physics.

ShapeMania (`shape_sorting-final`) is the user-confirmed visual reference.
Inspect its actual assets during P12 before deciding what can be reused.
Its mechanics, paid packages, service settings and advertising IDs are not
CASCADE source material.
