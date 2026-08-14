# DSP Recipe Tracker - MVP Roadmap

## Status and authority

**Status:** Authorized planning baseline

**Planning horizon:** Concept to working MVP prototype

**Active sprint:**
[`Sprint 1 - Executable Panel Foundation`](../ROADMAP.md)

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
- Runtime or visual work requires an installed-game checkpoint; compilation
  alone cannot complete it.
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

**Goal:** Establish a real BepInEx plugin and independently validate its panel
boundary before adding recipe behavior.

**Epics:**

1. Build authority, stable identity, and version continuity.
2. Installed lifecycle, HUD host, and native resource acquisition.
3. Panel composition, dragging, input containment, and visibility policy.
4. Retirement of the placeholder package path.

**Exit result:** A real local build loads under the pinned BepInEx runtime and
can display a native-compatible panel through a diagnostic fixture. The normal
plugin remains hidden with no rows. No Replicator, recipe, inventory,
pin-state, or major-window adapter exists yet, and the repository no longer
emits a zero-byte dummy mod package after real source is introduced.

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

**Goal:** Complete material presentation and validate the installed MVP
prototype.

**Epics:**

1. Native-composed product and direct-ingredient rows.
2. Inventory counts, sufficiency state, and machine-only warnings.
3. Tracker navigation, only if promoted at the Sprint 3 entry gate.
4. Runtime lifecycle and supported display-matrix validation.
5. Final real-package and clean-install validation.

**Exit result:** The installed prototype demonstrates the core loop:

```text
right-click recipe -> pin state -> recipe row -> inventory comparison
                   -> visibility controls -> unpin or FILO eviction
```

Tracker navigation has a confirmed native path but remains decision-gated.
Before Sprint 3 becomes Ready, the owner must include or exclude it. Inclusion
adds a bounded navigation story; exclusion does not block the core tracker and
must remain reflected in public documentation.

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

The MVP prototype is ready for owner review only when it:

- builds with zero release errors against complete, authority-aligned
  references;
- loads and shuts down cleanly under the pinned BepInEx installation;
- satisfies the applicable acceptance outline in `PROJECT.md`;
- demonstrates runtime and visual behavior in the supported installed game;
- changes no inventory, crafting, factory, or save state;
- produces a real version-aligned package without incomplete compile shims;
  and
- retains technical evidence without treating it as owner acceptance or
  publication approval.

Persistence, quantity scaling, recursive ingredients, automatic replication,
animation, resizing, speculative compatibility layers, and broader
localization remain outside this roadmap.

## Packaging boundary

The zero-byte placeholder is valid only while no executable plugin project
exists. Sprint 1 must retire it when real source is introduced and make the
local package consume the real versioned build output.

If complete, hash-aligned references are available to GitHub-hosted runners,
the hosted workflow may build and validate the real assembly. Otherwise it
must report executable building as blocked and emit no dummy mod package. It
may continue source and documentation checks that do not claim executable or
runtime validation. Partial game, Unity, or BepInEx shims are prohibited.

Sprint 3 revalidates the completed feature package and clean installation; it
does not postpone removal of the placeholder. This boundary follows
[`THUNDERSTORE-PACKAGE.md`](../THUNDERSTORE-PACKAGE.md) and
[`RUNTIME-AUTHORITY.md`](../RUNTIME-AUTHORITY.md).
