# Runtime Authority and Provenance

This document defines the evidence hierarchy for Dyson Sphere Program runtime
facts used by DSP Recipe Tracker. It records the recovered authoritative source
snapshot and prevents derived exports, inferred API shapes, or stale game
files from being treated as interchangeable.

**Evidence disposition:** The former retained authority files were removed.
The owner accepts the recorded identities, hashes, coverage, and conclusions
in this document as the recovered baseline. This record does not claim that
the former local evidence remains available or reproducible.

## Authority hierarchy

Use sources in this order:

1. The installed game's `Assembly-CSharp.dll` with the exact SHA-256 recorded
   below is the primary authority for types, members, signatures, inheritance,
   method bodies, IL call sites, and runtime API behavior encoded in that
   assembly.
2. The recorded conclusions from the removed, hash-linked Phase 1 export are
   accepted authority for the runtime Proto records and graph relationships
   explicitly preserved in this document and `FEASIBILITY.md`.
3. Direct inspection or a new focused export from the same assembly is
   required for a new fact that is absent, ambiguous, summarized, or deferred
   in the accepted record.
4. Project documentation and implementation are consumers of those sources;
   they do not override them.

Community documentation, wiki content, names inferred from UI presentation,
and source-defined compile shims may help investigation, but they do not
establish runtime authority.

BepInEx loader and Harmony authority is maintained separately in
[`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md). Do not infer loader support
from the game-assembly authority chain.

## Current assembly snapshot

| Field | Value |
| --- | --- |
| Installed source | `C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll` |
| Former retained copy | Removed |
| Size | 7,830,016 bytes |
| SHA-256 | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |
| Assembly identity | `Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null` |

The removed retained assembly was recorded as an unmodified copy of the
installed source whose hash matched the value above. That conclusion is
accepted as part of the recovered baseline. No retained copy is currently
available from this repository.

## Recorded derived export snapshot

| Field | Value |
| --- | --- |
| Supplied bundle | `dsp_phase1_end_products_v1_0.zip` |
| Former retained bundle | Removed |
| Former extracted files | Removed |
| Bundle size | 97,354 bytes |
| Attached bundle SHA-256 | `282720387dffc1e5fea4bffd72f0aa405392c545c05989e554603059e45ee3ae` |
| Export time recorded by bundle | `2026-07-19T01:51:31.9958668Z` |
| Validation status | `PASS`, with no errors or warnings |
| Assembly SHA-256 recorded by bundle | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |

The recorded bundle assembly hash matched the installed and former retained
assembly. The owner accepts the conclusion that the export and direct assembly
inspection referred to the same runtime binary.

The validation document also records
`source_zip_sha256=6253ab971c284f19cd1b7bb0e5727a83197fcf75bbcf90cd847d67d27ff6a6f5`.
That value identifies the export process's upstream source ZIP; it is not the
SHA-256 of the attached six-file bundle. Keep the two hashes distinct.

## Recorded bundle contents and coverage

The removed bundle was recorded as containing:

- `dsp_phase1_canonical_dataset_v1_0.json`;
- `dsp_phase1_validation_v1_0.json`;
- `dsp_phase1_nodes_v1_0.csv`;
- `dsp_phase1_edges_v1_0.csv`;
- `dsp_phase1_milestone_seed_graph_v1_0.csv`; and
- `dsp_phase1_reconstruction_report_v1_0.md`.

The former validation recorded all six extracted files as matching their
archive entries. It reported 314 technologies, 161 recipes, 174 items, 25
themes, and 14 vein types, with unique IDs, resolved graph references, and an
acyclic formal technology graph. Those conclusions are accepted as given.

For this project, the recorded export conclusions are the accepted baseline
for recipe identities, direct inputs and outputs, unlock relationships, item
identities, and other explicitly preserved fields. Practical-interpretation
edges remain separate from formal game-data edges and must not be promoted to
runtime facts by inference.

The former bundle did not contain a dedicated IL-disassembly or exhaustive
call-site inventory. Direct assembly inspection is required when implementation
needs IL references, exact member signatures, overload selection, control flow,
or a call pattern not established by the accepted conclusions.

## Use and refresh rules

- Treat the recorded conclusions as accepted recovery inputs, not as retained
  evidence that can be reopened.
- Verify the installed assembly identity before making a new runtime claim
  against the current installation.
- If the installed hash differs, treat this snapshot as historical. Do not mix
  facts from the new assembly with exports tied to the old hash.
- Regenerate or supplement exports from the changed assembly before adopting
  new recipe or API claims.
- Record exact types, members, signatures, and inspected call sites for every
  runtime integration decision.
- Preserve unknowns as unknowns. Do not fill export gaps from intuition,
  partial shims, or unrelated game versions.
- Compile-time references must satisfy the integrity rules in
  [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md).
- Technical validation against these sources remains separate from in-game
  owner acceptance.

## Sprint 1 compile-reference snapshot

Sprint 1 uses the following complete installed assemblies. This is the
authorized reference set for the plugin foundation; implementation must add a
new authority entry before consuming another Unity module.

| Relative to the game root | Assembly identity | SHA-256 |
| --- | --- | --- |
| `DSPGAME_Data\Managed\Assembly-CSharp.dll` | `Assembly-CSharp, Version=0.0.0.0` | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |
| `BepInEx\core\BepInEx.dll` | `BepInEx, Version=5.4.17.0` | `dc1cb6b58b962bda5aaa1d6b5f9ae14ec174f61836a1a1f96c1a040c7e8381f7` |
| `DSPGAME_Data\Managed\UnityEngine.dll` | `UnityEngine, Version=0.0.0.0` | `72cc73eef0036530abe21f82971ff06002cee37effdd4dd7d5d4ec8df3911f8d` |
| `DSPGAME_Data\Managed\UnityEngine.CoreModule.dll` | `UnityEngine.CoreModule, Version=0.0.0.0` | `e2b5ae2fd12646d03fc3d04d1a37d522572a3b97022fe1b95bbf2a2f2b04853a` |
| `DSPGAME_Data\Managed\UnityEngine.UIModule.dll` | `UnityEngine.UIModule, Version=0.0.0.0` | `c37bb3eace97302fb3aa9e17eac4446f8b010307637aa9cc5cb84c59828b0ab2` |
| `DSPGAME_Data\Managed\UnityEngine.UI.dll` | `UnityEngine.UI, Version=1.0.0.0` | `54953ebd7c9b4b39279876b37109f0f503938847f2a7be4a22d62e9b94c347eb` |

The table records the reference set used to establish the Sprint 1 source
baseline. The build pipeline may reference documented external assemblies
without copying or redistributing them. This authority record does not
authorize copying a plugin into the game installation or launching DSP,
BepInEx, Unity, or a substitute runtime harness.

### S1-06 exact Unity UI compile surface

On 2026-08-14, the six installed hashes above were rechecked and matched this
record before S1-06 metadata inspection. Mono.Cecil read the matching assembly
definitions without loading DSP, Unity, BepInEx, or the product assembly as a
runtime. The resulting compile surface is:

| Assembly | Declaring type | Exact consumed member |
| --- | --- | --- |
| `UnityEngine.CoreModule` | `UnityEngine.Object` | static `System.Void Destroy(UnityEngine.Object)` |
| `UnityEngine.CoreModule` | `UnityEngine.GameObject` | instance `System.Void .ctor(System.String)` |
| `UnityEngine.CoreModule` | `UnityEngine.GameObject` | instance `UnityEngine.Component AddComponent(System.Type)` |
| `UnityEngine.CoreModule` | `UnityEngine.GameObject` | instance `System.Void SetActive(System.Boolean)` |
| `UnityEngine.CoreModule` | `UnityEngine.Transform` | instance `System.Void SetParent(UnityEngine.Transform, System.Boolean)` |
| `UnityEngine.CoreModule` | `UnityEngine.RectTransform` | instance setters for `anchorMin`, `anchorMax`, `pivot`, `anchoredPosition`, and `sizeDelta`, each `System.Void(UnityEngine.Vector2)` |
| `UnityEngine.CoreModule` | `UnityEngine.Vector2` | instance `System.Void .ctor(System.Single, System.Single)` |
| `UnityEngine.UI` | `UnityEngine.UI.Graphic` | instance `System.Void set_raycastTarget(System.Boolean)` |
| `UnityEngine.UI` | `UnityEngine.UI.Image` | instance `System.Void set_sprite(UnityEngine.Sprite)` |

The inventory also preserves the exact type identities and inheritance needed
to compile those calls: `Object -> Component -> Behaviour -> MonoBehaviour`,
`Component -> Transform -> RectTransform`, and
`MonoBehaviour -> UIBehaviour -> Graphic -> MaskableGraphic -> Image`.
`Sprite` and `Component` are consumed as parameter/return identities. The
legacy `UnityEngine` facade continues to supply only the transitive
`MonoBehaviour` identity expected by BepInEx.

S1-06 consumes no `Assembly-CSharp` member, performs no reflection or dynamic
lookup, and adds no Unity member outside this recorded surface. The tracked
shims are declarations derived from these signatures, not runtime substitutes
or authority sources.

### S2-02 Replicator input surface

On 2026-08-14, the recorded `Assembly-CSharp`, `UnityEngine.CoreModule`, and
`UnityEngine.UI` hashes were rechecked before read-only Mono.Cecil inspection.
The accepted input adapter consumes this exact surface:

| Access | Declaring type | Exact member |
| --- | --- | --- |
| private reflection | `UIReplicatorWindow` | `UnityEngine.EventSystems.EventTrigger evtRecipe` |
| private reflection | `UIReplicatorWindow` | `RecipeProto[] recipeProtoArray` |
| private reflection | `UIReplicatorWindow` | `System.Int32 mouseRecipeIndex` |
| public compile | `Proto` | instance field `System.Int32 ID` |
| public compile | `EventTrigger` | instance getter `List<EventTrigger.Entry> triggers` |
| public compile | `EventTrigger.Entry` | instance fields `EventTriggerType eventID` and `EventTrigger.TriggerEvent callback` |
| public compile | `UnityEvent<BaseEventData>` | instance `AddListener` and `RemoveListener` |
| public compile | `PointerEventData` | instance getter `InputButton button` |

`UIReplicatorWindow._OnCreate()` creates the shared recipe `PointerDown` entry,
adds `OnRecipeMouseDown(BaseEventData)` first, and then stores that entry in
`evtRecipe.triggers`. The native handler reads `mouseRecipeIndex` and
`recipeProtoArray`, rejects invalid or null entries, and calls
`SetSelectedRecipeIndex(index, true)`. S2-02 therefore appends one listener to
that existing callback and uses the same index and populated array; it neither
replaces the native listener nor creates an independent eligibility rule.

The declaration-only shims cover every public type and member consumed by the
compiled product. The three private fields remain explicit `runtimeBindings`
in `surface-inventory.json` and are checked against hash-matched installed
metadata by `Validate-S2-02.ps1`; they are not falsely exposed as public shim
members. This inspection did not load DSP, Unity, BepInEx, or the plugin as a
runtime.

### S2-03 independent recipe-grid treatment surface

On 2026-08-14, S2-03 reused the same hash-matched installed metadata and
accepted feasibility calibration. Read-only inspection reconfirmed that
`UIReplicatorWindow._OnCreate()` allocates 120-entry `RecipeProto[]` and
`System.UInt32[]` arrays and a 120-by-4-byte `ComputeBuffer` for the native
recipe grid. `SetMaterialProps()` binds the private native `recipeStateBuffer`
to the private native `recipeBgMat` through `_StateBuffer`.

The tracker does not consume those private native state or material members.
It consumes the following independent construction surface:

| Assembly | Declaring type | Exact use |
| --- | --- | --- |
| `Assembly-CSharp` | `UIReplicatorWindow` | public `Image recipeBg`, public `RawImage recipeIcons`, and private-read `RecipeProto[] recipeProtoArray` |
| `UnityEngine.CoreModule` | `UnityEngine.Object` | generic `Instantiate<T>(T, Transform, Boolean)` and `Destroy(Object)` |
| `UnityEngine.CoreModule` | `Component` / `Transform` | object, transform, parent, component, and sibling access used to clone and order the non-raycasting layer |
| `UnityEngine.CoreModule` | `Material` | clone constructor, `SetBuffer`, and `SetColor` |
| `UnityEngine.CoreModule` | `ComputeBuffer` | 120-by-4 constructor, `SetData(Array)`, and `Release()` |
| `UnityEngine.UI` | `Graphic` | material get/set, color set, and raycast-target set |

The tracker-owned material uses `_StateBuffer`, `_FilterColor`, and
`_BansColor`. The accepted calibration is filter mask `0x2` with green
`(0.20, 0.75, 0.25, 1.00)`, banned mask `0x8` with red
`(0.78, 0.22, 0.22, 0.45)`, and image opacity `0.08`. These are source inputs,
not a live-appearance claim; visual suitability remains for the later owner
gate.

`Validate-S2-03.ps1` confirms the native private fields as forbidden original
resources and rejects any production reflection or direct-name path to
`recipeStateArray`, `recipeStateBuffer`, or `recipeBgMat`. The exhaustive
validator covers every public external member consumed by both Local and
Hosted builds. No runtime was loaded for this validation.

### S2-04 exact major-interface visibility surface

On 2026-08-14, S2-04 used the same hash-matched installed
`Assembly-CSharp.dll` as a read-only metadata authority. Inspection confirmed
the exact direct surface below:

| Declaring type | Member | Exact shape |
| --- | --- | --- |
| `UIGame` | `techTree` | public `UITechTree` field |
| `UIGame` | `dysonEditor` | public `UIDysonEditor` field |
| `UIGame` | `inventoryWindow` | public `UIInventoryWindow` field |
| `UIGame` | `replicator` | public `UIReplicatorWindow` field |
| `UIGame` | `statWindow` | public `UIStatisticsWindow` field |
| `UIGame` | `dashboard` | public `UIDashboard` field |
| `ManualBehaviour` | `active` | public `System.Boolean` getter; non-public setter |

Each of the six window types directly inherits `ManualBehaviour`, which
inherits `UnityEngine.MonoBehaviour`. Production caches those six public
references and consumes only the inherited `active` getter. It does not use
`UIGame.isAnyFunctionWindowActive`, `UIGame.uiPanelActiveMask`, reflection,
Harmony, or per-window lifecycle patches.

`Validate-S2-04.ps1` checks the field types, accessibility, inheritance, and
property accessors against the authority assembly. The declaration-only shims
and consumed-surface inventory represent every public external type, member,
and inheritance edge consumed by production; exhaustive validation passed for
both Local and Hosted builds. This establishes source compatibility with the
recorded surface, not live window behavior.

### S2-05 direct recipe-icon lookup surface

On 2026-08-15, S2-05 reused the same hash-matched installed
`Assembly-CSharp.dll` and documented Unity assemblies as read-only metadata
authorities. Inspection confirmed this exact lookup chain:

| Declaring type | Member or inheritance | Exact shape |
| --- | --- | --- |
| `LDB` | `recipes` | public static getter returning `RecipeProtoSet` |
| `RecipeProtoSet` | base type | `ProtoSet<RecipeProto>` |
| `ProtoSet<T>` | `Select` | public instance `T Select(System.Int32)` |
| `RecipeProto` | `iconSprite` | public instance getter returning `UnityEngine.Sprite` |
| `ProtoTable` | base type | `UnityEngine.ScriptableObject` |
| `UnityEngine.ScriptableObject` | base type | `UnityEngine.Object` |

Through S2-05, production performed `LDB.recipes.Select(recipeId)` and then
read the returned recipe's `iconSprite`. S3-02 extends that read-only surface
to the exact recipe, item, player, and storage members recorded in the Sprint 3
authority refresh below. Production still does not consume
`RecipeProto.Results` or `ResultCounts`, use `ItemProto.maincraft`, or attach
navigation or pinning behavior to tracker icons.

The Unity slot layer reuses the already recorded `GameObject`, `RectTransform`,
`Image`, `Image.sprite`, `Graphic.raycastTarget`, and object-destruction
surfaces. The declaration-only shims add the exact recipe-set and
`ScriptableObject` inheritance needed to compile the direct lookup.
`Validate-S2-05.ps1` verifies the runtime signatures and rejects output/item
reinterpretation and interaction paths. Exhaustive Local and Hosted coverage
passed. This establishes static source compatibility and ownership boundaries,
not live icon composition, dragging, containment, or cleanup behavior.

### S2-06 startup and visibility-control surface

On 2026-08-15, S2-06 reused the same hash-matched installed game and Unity
assemblies as read-only metadata authorities. Inspection confirmed the exact
startup and native-control chain below:

| Declaring type | Member | Exact shape |
| --- | --- | --- |
| `UIRoot` | `instance` | public static getter returning `UIRoot` |
| `UIRoot` | `uiGame` | public `UIGame` field; its transform is the panel host |
| `UIGame` | `gameMenu` | public `UIGameMenu` field |
| `UIGameMenu` | `buttonS` | public `Button` field; 46-by-46 screenshot template |
| `UIGameMenu` | `button3` | public `Button` field whose callback maps to `UIGame.On_F_Switch`, the Replicator action |
| `Button` | `onClick` | public get/set `ButtonClickedEvent` property |
| `Graphic` | `raycastTarget` | public getter used to identify the non-raycasting native icon image |
| `UIButton` | `tips` and format fields | public tracker-reset tooltip surface |
| `Component` | `GetComponentsInChildren<T>(bool)` | public localization-removal surface |

Production waits until `UIRoot.instance.uiGame` is available, then caches the
native hosts and resources. It clones `buttonS`, replaces the entire inherited
`onClick` event, removes cloned `Localizer` components, resets the exposed
tooltip content, and assigns tracker-owned listeners. The chosen global-control
icon is the sprite on `button3`'s non-raycasting descendant image, avoiding an
asset-name dependency and the raycasting background/circle images.

`Validate-S2-06.ps1` verifies these signatures and the source ownership rules;
the exhaustive validator covers every additional public external member in
the declaration-only shims. This establishes source compatibility and cleanup
paths, not live icon suitability, interaction, layout, or shutdown behavior.

### Sprint 3 planning authority refresh

On 2026-08-15, the owner-supplied
`dsp_phase1_end_products_v1_0.zip` was available for read-only reinspection.
Its SHA-256 was
`282720387dffc1e5fea4bffd72f0aa405392c545c05989e554603059e45ee3ae`,
matching the recorded bundle identity. Its canonical dataset again reported a
passing validation and assembly SHA-256
`ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85`.
The installed `Assembly-CSharp.dll` and the recorded Unity module set were also
rehashed and matched this document before metadata and IL inspection. No game,
Unity, BepInEx, plugin, or substitute runtime was loaded.
This reinspection does not reinstate the bundle as a retained repository or
shared project input.

The 161 recipe nodes and 441 exact `recipe_input` edges establish this direct-
ingredient cardinality for the supported snapshot:

| Direct inputs | Recipe count |
| ---: | ---: |
| 1 | 27 |
| 2 | 43 |
| 3 | 42 |
| 4 | 44 |
| 5 | 4 |
| 6 | 1 |

All 161 recipes have at least one direct input and no recipe/item pair is
duplicated. The sole six-input maximum is recipe ID `75`, Universe Matrix,
with exact quantity-one inputs `6001`, `6002`, `6003`, `6004`, `6005`, and
`1122`. The four five-input recipes are IDs `39`, `72`, `119`, and `128`.
Sprint 3 row modeling and layout must therefore cover one through six inputs;
four is not the supported maximum.

Read-only metadata inspection confirmed the presentation-data surface used by
S3-02:

| Declaring type | Exact public member |
| --- | --- |
| `RecipeProto` | fields `ERecipeType Type`, `System.Boolean Handcraft`, `System.Int32[] Items`, and `System.Int32[] ItemCounts` |
| `RecipeProto` | getters `UnityEngine.Sprite iconSprite` and `System.String madeFromString` |
| `LDB` | static getter `ItemProtoSet items` |
| `ItemProtoSet` | base type `ProtoSet<ItemProto>` |
| `ItemProto` | getter `UnityEngine.Sprite iconSprite` |
| `GameMain` | static getter `Player mainPlayer` |
| `Player` | getter `StorageComponent package` |
| `StorageComponent` | `System.Int32 GetItemCount(System.Int32)` |

`RecipeProto.madeFromString` switches on `RecipeProto.Type`, returns `"-"` for
the none value, returns localized labels for Smelt, Chemical, Refine, Assemble,
Particle, Exchange, PhotonStore, Fractionate, and Research, and returns a
localized Unknown label for other values. Every machine-only recipe in the
Phase 1 dataset uses one of Smelt, Chemical, Refine, Assemble, Particle,
Fractionate, or Research. The supported baseline therefore needs no independent
production-category table or tracker-owned machine-category fallback.

The matching assembly exposes public static
`UICanvasScalerHandler.GetSuggestUILayoutHeight()` and
`GetSuggestUILayoutHeight(System.Int32)`. Inspection of the integer overload's
IL confirms these selected Auto cases:

| Resolution height | Auto UI layout height | Resulting height scale |
| ---: | ---: | ---: |
| 1080 | 1080 | 1 |
| 1440 | 1440 | 1 |
| 2160 | 1080 | 2 |

`UICanvasScalerHandler.SetCanvas()` applies the selected layout height to a
height-matched `CanvasScaler` reference resolution. The retained feasibility
probe independently recorded `Screen.height = 2160` and
`UIRoot.overlayCanvas.scaleFactor = 2`, consistent with the inspected Auto
calculation.

The narrow drag and bounds surface is public and requires no new private DSP
binding or Harmony patch:

| Assembly | Declaring type | Exact member |
| --- | --- | --- |
| `UnityEngine.UI` | `PointerEventData` | getter `UnityEngine.Vector2 delta` |
| `UnityEngine.UI` | `EventTriggerType` | literal `Drag = 5` |
| `UnityEngine.UI` | `EventTrigger.Entry` | public constructor and public fields `EventTriggerType eventID` and `EventTrigger.TriggerEvent callback` |
| `UnityEngine.UI` | `EventTrigger.TriggerEvent` | public constructor; inherits `UnityEvent<BaseEventData>` |
| `UnityEngine.UIModule` | `Canvas` | getter `System.Single scaleFactor` |
| `UnityEngine.CoreModule` | `RectTransform` | getter `UnityEngine.Rect rect` |
| `UnityEngine.CoreModule` | `Rect` | getters `System.Single width` and `System.Single height` |

Production can convert screen-pixel pointer deltas through the live overlay-
canvas scale factor and clamp against the actual parent rectangle in UI-layout
units. It does not need to reproduce DSP's Auto algorithm or subscribe to a
global resolution event. The existing tracker-owned panel object can own and
release its drag callback with the rest of its Unity resources.

## Repository boundary

The former retained authority inputs were removed. Tracked documentation may
record their identities, hashes, provenance, coverage, and accepted
conclusions, but it must not claim that the files remain locally available or
embed or redistribute licensed game binaries or large generated datasets.
