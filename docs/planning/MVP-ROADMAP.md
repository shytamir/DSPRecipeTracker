# DSP Recipe Tracker - MVP Roadmap

## Status and authority

**Status:** Authorized planning baseline

**Planning horizon:** Concept to working MVP prototype

**Current Sprint 1 draft:**
[`Sprint 1 - Safe Source Foundation`](../ROADMAP.md)

The current Sprint 1 record is a sanitized source-only recovery plan with no
Active story. It overrides this planning baseline wherever this file mentions
installed, loaded-game, visual, or interaction validation. Those activities
require a separate owner-approved controlled-runtime record and are not
authorized merely because they appear in this long-range plan.

This record orders implementation; it does not create product authority.
[`PROJECT.md`](../PROJECT.md) governs scope,
[`PRODUCT-PRINCIPLES.md`](../PRODUCT-PRINCIPLES.md) governs the native-extension
approach, and [`FEASIBILITY.md`](../FEASIBILITY.md) governs confirmed runtime
evidence. The original concept remains non-authoritative history.

## Planning rules

- Each story offers one value and has an independently testable definition of
  done.
- Sprint closure checks are gates, not user stories.
- Runtime collection, tracker state, presentation modeling, and Unity UI stay
  separate.
- Compilation alone cannot establish runtime or visual behavior. Until a
  separate controlled-runtime record is explicitly approved, those checks
  remain unvalidated rather than becoming work for an agent to perform.
- Technical validation and owner acceptance remain separate states.
- Deferred features enter a sprint only through an explicit owner decision.
- Completed roadmaps are archived instead of remaining active work.

## Delivery states

| State | Meaning |
| --- | --- |
| Draft | Proposed content awaiting owner review. |
| Ready | Owner-approved scope with every entry decision resolved. |
| In progress | Implementation has begun. |
| Technically validated | Recorded evidence satisfies the definition of done. |
| Owner accepted | The owner has reviewed and accepted the demonstrated result. |
| Complete | Technical validation and required owner acceptance are recorded. |

## Roadmap

### Sprint 1 - Executable panel foundation

**Goal:** Establish a buildable BepInEx plugin and test its source-level panel
boundary before adding recipe behavior.

**Epics:**

1. Build authority, stable identity, and version continuity.
2. Compile-time lifecycle and fail-soft UI integration boundaries.
3. Panel composition, dragging, input containment, and visibility policy.
4. Creation of the first real package path from build output.

**Exit result:** A real local build and deterministic source tests pass without
installing or executing the plugin. The panel boundary compiles, but native
appearance, interaction, lifecycle, and cleanup remain unvalidated. No
Replicator, recipe, inventory, pin-state, or major-window adapter exists yet,
and the repository packages only real build output rather than introducing a
dummy mod artifact.

### Sprint 2 - Native integration and tracker state

**Goal:** Connect the panel boundary to the existing Replicator and HUD
surfaces.

**Epics:**

1. Native right-click pin gesture integration.
2. Independent Replicator cell-state treatment.
3. Transient FILO state and panel-slot synchronization.
4. Major-window visibility and paired Hide/Show controls.

**Exit result:** Right-clicking an eligible native recipe cell drives the
three-entry tracker, independent green/red treatment, exact six-window hide
rule, panel Hide control, and native game-menu restore control. Rows may still
use minimal recipe identity presentation.

### Sprint 3 - Recipe presentation and prototype hardening

**Goal:** Complete material presentation and prepare the MVP prototype for a
separately authorized runtime-validation decision.

**Epics:**

1. Native-composed product and direct-ingredient rows.
2. Inventory counts, sufficiency state, and machine-only warnings.
3. Tracker navigation, only if promoted at the Sprint 3 entry gate.
4. Controlled-runtime plan review, if separately commissioned by the owner.
5. Final real-package construction and static inspection.

**Exit result:** The source prototype implements the core loop:

```text
right-click recipe -> pin state -> recipe row -> inventory comparison
                   -> visibility controls -> unpin or FILO eviction
```

Tracker navigation has a confirmed native path but remains decision-gated.
Before Sprint 3 becomes Ready, the owner must include or exclude it. Inclusion
adds a bounded navigation story; exclusion does not block the core tracker and
must remain reflected in public documentation.

This roadmap does not authorize executing that loop in DSP. Runtime, visual,
interaction, cleanup, and compatibility claims remain unavailable unless a
separate controlled-runtime record is approved and completed.

## Dependency chain

```text
Sprint 1: authoritative plugin and independent panel boundary
    |
Sprint 2: Replicator, tracker state, and HUD visibility integration
    |
Sprint 3: live row content, hardening, and final package validation
    |
Working MVP prototype ready for owner review
```

Sprint 2 depends on the lifecycle, panel, and visibility boundaries established
in Sprint 1. Sprint 3 depends on stable recipe identities and slot transitions
from Sprint 2. Later work must not pull recipe or inventory rules backward
into the Sprint 1 shell.

## Cross-sprint definition of prototype

The source prototype is ready for owner review only when it:

- builds with zero release errors against complete, authority-aligned
  references;
- satisfies the applicable acceptance outline in `PROJECT.md`;
- passes deterministic source-level behavior tests without loading DSP,
  BepInEx, Unity, or a substitute harness;
- changes no inventory, crafting, factory, or save state;
- produces a real version-aligned package without incomplete compile shims;
  and
- retains technical evidence without treating it as owner acceptance or
  publication approval.

Installed lifecycle, runtime behavior, native appearance, interaction,
cleanup, coexistence, and supported display claims require their own
owner-approved controlled-runtime record. They cannot be inferred from this
source-prototype gate.

Persistence, quantity scaling, recursive ingredients, automatic replication,
animation, resizing, speculative compatibility layers, and broader
localization remain outside this roadmap.

## Packaging boundary

No placeholder DLL or dummy package pipeline exists, and Sprint 1 must not
create one. The first package path is introduced with real source and consumes
the real versioned build output.

If complete external references are lawfully available to GitHub-hosted
runners, the hosted workflow may build and validate the real assembly.
Otherwise it must report executable building as blocked and emit no mod
package. It may continue source and documentation checks that do not claim
executable or runtime validation. Partial game, Unity, or BepInEx shims are
prohibited.

Sprint 3 revalidates the completed feature package by static inspection; it
does not install the package. This boundary follows
[`THUNDERSTORE-PACKAGE.md`](../THUNDERSTORE-PACKAGE.md) and
[`RUNTIME-AUTHORITY.md`](../RUNTIME-AUTHORITY.md).
