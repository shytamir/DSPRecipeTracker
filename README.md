# DSP Recipe Tracker

DSP Recipe Tracker is a planned in-game recipe companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It will let players pin recipes from the Replicator and compare each recipe's
direct material requirements with the contents of Icarus's inventory.

The recovered product, implementation, and validation contracts are owner
accepted. S1-01 is owner accepted, and S1-02 is implemented and technically
validated pending owner acceptance. Runtime execution and
runtime validation remain outside the current source-only authorization.
Future in-game validation is performed only by the owner from testable builds,
after non-runtime checks are exhausted and only at meaningful gates. No
installable package or supported release exists yet.

## Planned MVP

- Pin and unpin recipes by right-clicking the Replicator's existing recipe
  grid. Left-click remains selection-only, while right-click preserves native
  selection and toggles the pin.
- Show restrained green and red filtering on native Replicator recipe cells
  for unpinned and pinned state without adding a separate Pin button or
  modifying DSP's original cell-state buffer.
- Show up to three pinned recipes; a fourth pin replaces the bottom entry.
- Display product and direct-ingredient icons without recursively expanding
  the recipe tree.
- Show each ingredient as `amount in inventory / amount required`.
- Use clear insufficient and sufficient states at the exact required
  threshold.
- Keep pins after crafting for the current play session.
- Identify the required production building for a pinned machine-only recipe.
- If promoted during feature development, navigate from product and ingredient
  icons to their Replicator recipes without creating another pinning surface.
- Provide a fixed-size, draggable, screen-bounded tracker that captures its
  own pointer input.
- Hide the panel when empty, when the player requests it, and while major game
  interfaces are open.
- Restore a manually hidden panel from a Show/Hide control added to the game's
  existing bottom-right game-menu group, inheriting that group's visibility.

## Product boundaries

The mod owns only transient FILO pin state, presentation, and visibility
orchestration. DSP remains authoritative for Replicator population, technology
lock, ordinary recipe interaction, recipe data, and inventory quantities.

The MVP reads Icarus's inventory only. It does not count materials in storage,
logistics stations, or the wider planet, and it does not alter inventory,
crafting, factory, or save state. The tracker panel is not another pinning
surface.

Quantity scaling, saved pins, optional automatic replication, animation, and
panel resizing are possible follow-up features rather than initial
commitments. Automatic building placement and other factory automation are
separate project ideas.

The authoritative product scope is maintained in
[docs/PROJECT.md](docs/PROJECT.md). Readiness and acceptance criteria are
maintained in [docs/VALIDATION-CONTRACT.md](docs/VALIDATION-CONTRACT.md).

The active Sprint 1 implementation record is
[docs/ROADMAP.md](docs/ROADMAP.md). The authorized long-range MVP roadmap and
original concept foundation remain under `docs/planning/`.

The governing UI-extension approach is recorded in
[docs/PRODUCT-PRINCIPLES.md](docs/PRODUCT-PRINCIPLES.md). The initial
[feasibility findings](docs/FEASIBILITY.md) document the matching assembly and
runtime evidence separately from product decisions.

## Requirements

The planned runtime requires:

- Dyson Sphere Program;
- BepInEx 5 installed in the game directory.

The initial feasibility baseline is DSP `0.10.34.28529`, Steam build
`23109513`, Unity `2022.3.62f3c1`, and the exact game-assembly hash recorded in
the runtime authority contract. Supported resolutions and UI scales will be
selected through later initialized in-game validation. Game, Unity, and
BepInEx assemblies remain external dependencies and are not redistributed in
this repository.

## Runtime authority

Runtime recipe facts and API integration are grounded in a SHA-256-identified
installed `Assembly-CSharp.dll` and a validation-passing export derived from
that same binary. The assembly is authoritative for IL and exact API shapes;
the export is authoritative for the Proto and graph fields it preserves.

See [docs/RUNTIME-AUTHORITY.md](docs/RUNTIME-AUTHORITY.md) for the recovered
source identities, accepted conclusions, evidence hierarchy, and refresh
rules. The former retained authority files were removed; licensed binaries and
generated datasets are never redistributed.

## BepInEx conformance

The supported plugin-manager contract is the installed BepInEx `5.4.17.0`,
mapped to source tag `v5.4.17`. The plugin will use only BepInEx 5 lifecycle,
identity, logging, and any explicitly needed configuration surface. HarmonyX
`2.5.5` is available but will be referenced only when a confirmed game hook
requires patching.

The confirmed MVP pin path uses the Replicator's existing `PointerDown` event
and does not require HarmonyX. Its green/red treatment uses a non-raycasting
clone of the native grid material with an independent state buffer.

The project does not modify BepInEx, target the source checkout's BepInEx 6
`master`, or promise compatibility with other loader versions. See
[docs/BEPINEX-CONFORMANCE.md](docs/BEPINEX-CONFORMANCE.md) for exact commits,
recorded binary hashes, accepted conclusions, and the fixed-version policy.

The approved BepInEx plugin GUID is `dsprecipetracker`; its loader display name
is `DSP-Recipe-Tracker`.

## Build

S1-02 provides a minimal, nonfunctional BepInEx plugin skeleton. It establishes
the approved identity, loader lifecycle/logging boundary, and versioned source
build; it does not implement recipe tracking or authorize installation.

The S1-01 repository and external-reference contract is validated with:

```powershell
.\scripts\validate\Validate-S1-01.ps1 -GameRoot '<DSP installation>'
```

The command reads the explicitly supplied installation only to confirm the
documented compile inputs exist. It does not copy, install, load, or execute
them.

Validate both the explicit local-reference build and the hosted shim build
with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-02.ps1 `
  -GameRoot '<DSP installation>' `
  -BuildNumber 1 `
  -SourceRevision $revision
```

This produces ignored repository-local build output, runs deterministic
identity tests, checks assembly/file/diagnostic versions, and validates the
exact hosted Unity compile-reference surface. It does not install or load the
plugin. Hosted CI supplies its own explicit BepInEx compile reference; S1-03
will add package construction and workflow automation.

The repository intentionally contains no placeholder DLL or dummy package
pipeline. Sprint 1 will create the first package pipeline from the real plugin
build output according to
[docs/THUNDERSTORE-PACKAGE.md](docs/THUNDERSTORE-PACKAGE.md).

## Versioning and packaging

`VERSION` supplies the major and minor release line. The real package pipeline
will use the GitHub Actions run number as the patch value and derive:

```text
Package/plugin version: M.m.N
Semantic version:       M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.<short-commit>
```

The current pre-release line is `0.1`. The package contract defines the
single-DLL archive layout and static validation boundary.

## Project status

S1-01, repository hygiene, is complete and owner accepted. S1-02 has produced
the minimal source build and is technically validated; it remains Active
pending explicit owner acceptance, while later stories remain Proposed.
Historical assembly and isolated-runtime feasibility conclusions identify the
Replicator input surface, recipe and inventory APIs, native HUD host, exact
major-interface visibility signals, and reusable presentation resources. Those
accepted conclusions do not authorize new runtime execution. The plugin
skeleton contains no recipe-tracking behavior; gameplay, layout, input, and
display-scale behavior remain unvalidated.

## Repository layout

```text
.
|-- AGENTS.md
|-- .gitignore
|-- ci/
|   `-- compile-references/
|       |-- README.md
|       |-- Unity.Reference/
|       |   `-- UnityEngine/
|       `-- surface-inventory.json
|-- Directory.Build.props
|-- Directory.Build.targets
|-- DSPRecipeTracker.sln
|-- docs/
|   |-- BEPINEX-CONFORMANCE.md
|   |-- FEASIBILITY.md
|   |-- IMPLEMENTATION-CONTRACT.md
|   |-- planning/
|   |   |-- MVP-ROADMAP.md
|   |   `-- PINNED-RECIPE-TRACKER-CONCEPT.md
|   |-- PRODUCT-PRINCIPLES.md
|   |-- PROJECT.md
|   |-- ROADMAP.md
|   |-- RUNTIME-AUTHORITY.md
|   |-- THUNDERSTORE-PACKAGE.md
|   `-- VALIDATION-CONTRACT.md
|-- scripts/
|   |-- build/
|   |   `-- Build-S1-02.ps1
|   `-- validate/
|       |-- CompileReferenceValidator/
|       |-- S1-01.BuildContract.proj
|       |-- Validate-S1-01.ps1
|       `-- Validate-S1-02.ps1
|-- src/
|   `-- DSPRecipeTracker/
|-- tests/
|   `-- DSPRecipeTracker.Tests/
|-- VERSION
|-- LICENSE
`-- README.md
```

Before contributing, read [AGENTS.md](AGENTS.md). It defines the repository's
scope, safety, validation, and Git expectations.

Implementation files will follow the adopted
[source, test, CI-reference, validation, and package layout](docs/IMPLEMENTATION-CONTRACT.md#adopted-repository-layout)
as their respective Sprint 1 stories become Active. Later-story placeholder
projects and directories are not created in advance.

The [original concept foundation](docs/planning/PINNED-RECIPE-TRACKER-CONCEPT.md)
is retained as non-authoritative planning context. [The project contract](docs/PROJECT.md)
remains authoritative for current scope and accepted decisions.

## License

DSP Recipe Tracker is licensed under the
[Apache License 2.0](LICENSE).

## Disclaimer

This is an unofficial community project. Dyson Sphere Program and its assets
belong to their respective owners. BepInEx and the game are required but are
not included.
