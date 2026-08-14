# DSP Recipe Tracker

DSP Recipe Tracker is a planned in-game recipe companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It will let players pin recipes from the Replicator and compare each recipe's
direct material requirements with the contents of Icarus's inventory.

The previous project is being recovered into a safe source-first plan. The
sanitized Sprint 1 roadmap is a draft with no Active story, so implementation
and runtime validation are not yet authorized. Runtime authority, historical
feasibility findings, and UI-extension principles are documented, but no
installable plugin or supported release exists yet.

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

The authoritative product scope and acceptance outline are maintained in
[docs/PROJECT.md](docs/PROJECT.md).

The sanitized Sprint 1 draft is [docs/ROADMAP.md](docs/ROADMAP.md).
The authorized MVP skeleton and original concept foundation remain under
`docs/planning/`.

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

See [docs/RUNTIME-AUTHORITY.md](docs/RUNTIME-AUTHORITY.md) for the current
source identities, retained local paths, evidence hierarchy, and refresh
rules. Licensed binaries and generated datasets remain ignored under
`artifacts/` and are never redistributed.

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
binary hashes, retained evidence, and the fixed-version policy.

## Build and install

Build and installation instructions will be added with the first working
plugin skeleton. There is currently no functional DLL to build or install.

The repository intentionally contains no placeholder DLL or dummy package
pipeline. Sprint 1 will create the first package pipeline from the real plugin
build output; it must not emit an installable-looking artifact before real
source exists.

## Versioning and packaging

`VERSION` currently reserves the major and minor release line. Sprint 1 will
define the real package pipeline and make one repository-owned version source
derive:

```text
Package/plugin version: M.m.N
Semantic version:       M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.<short-commit>
```

The reserved pre-release line is `0.1`. Package layout and validation
documentation will be created with the real pipeline.

## Project status

Sprint 1 is awaiting owner review and has no Active story. Historical assembly
and isolated-runtime feasibility work identified the Replicator input surface,
recipe and inventory APIs, native HUD host, exact major-interface visibility
signals, and reusable presentation resources. Those findings do not authorize
new runtime execution. No functional plugin exists yet; gameplay, layout,
input, and display-scale behavior remains unvalidated.

## Repository layout

```text
.
|-- AGENTS.md
|-- .gitignore
|-- docs/
|   |-- BEPINEX-CONFORMANCE.md
|   |-- FEASIBILITY.md
|   |-- planning/
|   |   |-- MVP-ROADMAP.md
|   |   `-- PINNED-RECIPE-TRACKER-CONCEPT.md
|   |-- PRODUCT-PRINCIPLES.md
|   |-- ROADMAP.md
|   `-- RUNTIME-AUTHORITY.md
|-- VERSION
|-- LICENSE
`-- README.md
```

Before contributing, read [AGENTS.md](AGENTS.md). It defines the repository's
scope, safety, validation, and Git expectations.

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
