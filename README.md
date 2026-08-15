# DSP Recipe Tracker

DSP Recipe Tracker is a working validated MVP prototype for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It lets players pin recipes from the Replicator and compare each recipe's
direct material requirements with the contents of Icarus's inventory.

The product, implementation, and validation contracts and Sprints 1 through 3
are owner accepted. The final workshop behavior passed owner validation and
acceptance on 2026-08-15. Source-ready and Package-inspected passed for clean
commit `14bbe8e046e32333bd7cf68f35b8f22bc04dd47f` using build number 318.
The repository is planning-pending for publication refinement; no supported or
published release exists.

## Validated MVP prototype

- Pin and unpin recipes by right-clicking the Replicator's existing recipe
  grid. Left-click remains selection-only, while right-click preserves native
  selection and toggles the pin.
- Mark pinned native Replicator cells with restrained green corner brackets;
  unpinned cells remain neutral and DSP's original state buffer is untouched.
- Show up to three pinned recipes; a fourth pin replaces the bottom entry.
- Display product and direct-ingredient icons without recursively expanding
  the recipe tree.
- Show each ingredient as `amount in inventory / amount required`.
- Use clear insufficient and sufficient states at the exact required
  threshold.
- Keep pins after crafting for the current play session.
- Identify the required production building for a pinned machine-only recipe.
- Keep product and ingredient icons non-interactive; tracker navigation is
  outside the MVP.
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

The current planning-pending state is recorded in
[docs/ROADMAP.md](docs/ROADMAP.md). Completed Sprint records are archived as
[Sprint 3](docs/archive/PROTOTYPE-ROADMAP.md),
[Sprint 2](docs/archive/UI_INTEGRATION_ROADMAP.md), and
[Sprint 1](docs/archive/BOOTSTRAP-ROADMAP.md). The historical MVP roadmap and
original concept foundation remain under `docs/planning/`.

The governing UI-extension approach is recorded in
[docs/PRODUCT-PRINCIPLES.md](docs/PRODUCT-PRINCIPLES.md). The initial
[feasibility findings](docs/FEASIBILITY.md) document the matching assembly and
runtime evidence separately from product decisions.

## Requirements

The prototype runtime requires:

- Dyson Sphere Program;
- BepInEx 5 installed in the game directory.

The initial feasibility baseline is DSP `0.10.34.28529`, Steam build
`23109513`, and Unity `2022.3.62f3c1`. Automated geometry validation covers
1920-by-1080/Auto at UI layout height 1080 and 2560-by-1440/Auto at UI layout
height 1440. Owner-performed runtime display validation is limited to
3840-by-2160/Auto, which calculates to a 1080-pixel UI layout height. Game,
Unity, and BepInEx assemblies remain external dependencies and are not
redistributed in this repository.

## Runtime authority

Installed DSP and Unity assemblies may be inspected read-only for the exact
surface consumed by the product. Story validators check that narrow contract,
not the broader runtime's binary identity. See
[docs/RUNTIME-AUTHORITY.md](docs/RUNTIME-AUTHORITY.md) for the authority,
inspection, and repository-boundary rules. Licensed binaries and generated
runtime datasets are never redistributed.

## BepInEx conformance

The supported plugin-manager contract is the installed BepInEx `5.4.17.0`,
mapped to source tag `v5.4.17`. The plugin uses only BepInEx 5 lifecycle,
identity, logging, and any explicitly needed configuration surface. HarmonyX
`2.5.5` is available but will be referenced only when a confirmed game hook
requires patching.

The MVP pin path uses the Replicator's existing `PointerDown` event and does
not require HarmonyX. Its pinned-cell treatment uses tracker-owned,
non-raycasting green corner brackets.

The project does not modify BepInEx, target the source checkout's BepInEx 6
`master`, or promise compatibility with other loader versions. See
[docs/BEPINEX-CONFORMANCE.md](docs/BEPINEX-CONFORMANCE.md) for exact commits,
the single hosted-download integrity boundary, accepted conclusions, and the
fixed-version policy.

The approved BepInEx plugin GUID is `dsprecipetracker`; its loader display name
is `DSP-Recipe-Tracker`.

## Build

S1-02 introduced the minimal BepInEx plugin skeleton. It established
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
plugin. Hosted CI supplies its own explicit BepInEx compile reference.

Build and inspect the real development package in both reference modes with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-03.ps1 `
  -GameRoot '<DSP installation>' `
  -BuildNumber 2 `
  -SourceRevision $revision
```

The package, retained DLL, build information, and validation report are written
only beneath ignored `artifacts/package/`. The validator checks the exact ZIP
allowlist, manifest, README, PNG, managed DLL identity and versions, BepInEx
metadata, and DLL hash. It also runs required fail-closed package cases. The
package is not installed or loaded.

## Versioning and packaging

`VERSION` supplies the major and minor release line. The real package pipeline
uses the GitHub Actions run number as the patch value and derives:

```text
Package/plugin version: M.m.N
Semantic version:       M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.<short-commit>
```

The current pre-release line is `0.9`. Pushes to `main` and manual dispatches
run the same hosted build, shim-coverage check, package construction, and static
inspection in GitHub Actions. The resulting development artifact is retained
for 30 days; the workflow does not publish it.

## Project status

Sprints 1 through 3 and all required exit gates are complete and owner
accepted. The resulting prototype implements the complete transient pinning,
direct-ingredient presentation, visibility, dragging, lifecycle, and package
loop with bounded Debug diagnostics. The owner passed the formal four-group
procedure and accepted the refined workshop build. The clean committed result
then passed Source-ready and Package-inspected as build 318.

The repository has no active story. Its next state is publication refinement,
which requires a new owner-authorized roadmap. The validated prototype does
not imply publication approval, compatibility beyond the recorded baseline,
or a supported release.

## Repository layout

```text
.
|-- AGENTS.md
|-- .gitignore
|-- .github/
|   `-- workflows/
|       `-- build-package.yml
|-- ci/
|   `-- compile-references/
|       |-- README.md
|       |-- DSPGame.Reference/
|       |-- Unity.Reference/
|       |   |-- UnityEngine/
|       |   |-- UnityEngine.CoreModule/
|       |   |-- UnityEngine.TextRenderingModule/
|       |   |-- UnityEngine.UIModule/
|       |   `-- UnityEngine.UI/
|       `-- surface-inventory.json
|-- Directory.Build.props
|-- Directory.Build.targets
|-- DSPRecipeTracker.sln
|-- global.json
|-- docs/
|   |-- archive/
|   |   |-- BOOTSTRAP-ROADMAP.md
|   |   |-- PROTOTYPE-ROADMAP.md
|   |   `-- UI_INTEGRATION_ROADMAP.md
|   |-- BEPINEX-CONFORMANCE.md
|   |-- FEASIBILITY.md
|   |-- IMPLEMENTATION-CONTRACT.md
|   |-- planning/
|   |   |-- MVP-ROADMAP.md
|   |   `-- PINNED-RECIPE-TRACKER-CONCEPT.md
|   |-- PRODUCT-PRINCIPLES.md
|   |-- PROJECT.md
|   |-- OWNER-VALIDATION.md
|   |-- ROADMAP.md
|   |-- RUNTIME-AUTHORITY.md
|   |-- THUNDERSTORE-PACKAGE.md
|   `-- VALIDATION-CONTRACT.md
|-- scripts/
|   |-- build/
|   |   |-- Build-S1-02.ps1
|   |   `-- Build-S1-03.ps1
|   `-- validate/
|       |-- CompileReferenceValidator/
|       |-- PackageValidator/
|       |-- S1-01.BuildContract.proj
|       |-- Validate-S1-01.ps1
|       |-- Validate-S1-02.ps1
|       |-- Validate-S1-03.ps1
|       |-- Validate-S1-04.ps1
|       |-- Validate-S1-05.ps1
|       |-- Validate-S1-06.ps1
|       |-- Validate-S2-01.ps1
|       |-- Validate-S2-02.ps1
|       |-- Validate-S2-03.ps1
|       |-- Validate-S2-04.ps1
|       |-- Validate-S2-05.ps1
|       |-- Validate-S2-06.ps1
|       |-- Validate-S3-01.ps1
|       |-- Validate-S3-02.ps1
|       |-- Validate-S3-03.ps1
|       |-- Validate-S3-04.ps1
|       |-- Validate-S3-05.ps1
|       |-- Validate-S3-06.ps1
|       |-- Validate-Sprint3-SourceReady.ps1
|       `-- Validate-Sprint3-PackageInspected.ps1
|-- packaging/
|   |-- icon.png
|   |-- manifest.json
|   `-- README.md
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

Implementation files follow the adopted
[source, test, CI-reference, validation, and package layout](docs/IMPLEMENTATION-CONTRACT.md#8-adopted-repository-layout)
Future implementation requires an active owner-authorized roadmap; placeholder
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
