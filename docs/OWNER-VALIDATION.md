# Sprint 3 Owner Validation Procedure

## Status and artifact

**Status:** Passed and owner accepted on 2026-08-15

**Purpose:** Judge only the live DSP behavior that deterministic, build, shim,
and package inspection cannot establish.

Use these exact development artifacts produced by the Sprint 3 package gate:

- DLL: `artifacts/package/0.1.310/DSPRecipeTracker.dll`
- package: `artifacts/package/0.1.310/DSPRecipeTracker-0.1.310.zip`
- build identity: `artifacts/package/0.1.310/build-info.json`

Confirm that `build-info.json` reports semantic version `0.1.310`, source
revision `cb37a0acbd23e77306137dc695d92980f1c686cc`, diagnostic label
`0.1.310.cb37a0acbd23`, and `referenceMode` `Local`. This is a development test
build, not a supported release or publication candidate.

**Recorded result:** The owner reported `PASS` for groups A through D on
2026-08-15. This satisfied Behavioral-ready for the identified build and
conditions. A subsequent direct-build workshop refined presentation and
lifecycle behavior; the owner accepted final direct build `0.1.318`. Source-
ready and Package-inspected then passed for the clean committed result at
`14bbe8e046e32333bd7cf68f35b8f22bc04dd47f`, also using build number 318.
Publication approval and release candidacy were not inferred.

The formal steps below preserve the procedure actually run against build
`0.1.310`. The accepted workshop progression and compromise rationale are
recorded in the archived
[`Sprint 3 Prototype Roadmap`](archive/PROTOTYPE-ROADMAP.md).

## Prerequisites and safe setup

1. Use Dyson Sphere Program `0.10.34.28529` with BepInEx `5.4.17.0` and no
   older DSP Recipe Tracker DLL present.
2. Use a disposable or backed-up save with the Replicator unlocked and at
   least one ordinary hand-craftable recipe plus one unlocked recipe that DSP
   identifies as requiring a production machine.
3. Set the display to 3840 by 2160 and DSP UI Layout Reference Height to
   `Auto`. No other runtime resolution or UI-scale case is part of this gate.
4. Copy only the test `DSPRecipeTracker.dll` to
   `BepInEx/plugins/DSPRecipeTracker/`. Do not copy a shim, dependency, report,
   or other artifact into the game.
5. Start DSP normally and load the prepared save. Do not use automation or a
   substitute runtime.

## A. Native pinning and live recipe presentation

1. Open the Replicator and left-click an unlocked hand-craftable recipe.
   Confirm native selection changes and no pin is created.
2. Right-click that recipe. Confirm native selection still behaves normally,
   the recipe becomes pinned, and its native grid cell receives restrained
   pinned treatment. The tracker remains hidden while the Replicator or
   Inventory is open.
3. Close the Replicator and Inventory. Confirm one complete tracker row
   appears and shows the native product icon, every direct ingredient in
   native order, `inventory / required` values, and readable sufficient or
   insufficient treatment. Confirm the tracker icons themselves do nothing
   when clicked.
4. Add or remove one direct ingredient through ordinary game controls. Confirm
   any opened major interface is closed, then confirm the displayed inventory
   count and sufficiency update shortly afterward without re-pinning or moving
   the row.
5. Reopen the Replicator, pin one unlocked machine-only recipe, then close the
   Replicator and Inventory. Confirm its row identifies the native production
   category without offering crafting or navigation behavior.
6. Reopen the Replicator and right-click the first pinned recipe again.
   Confirm native selection remains usable, then close the Replicator and
   Inventory and confirm the corresponding tracker row is removed.

Expected result: native Replicator behavior coexists with pinning, cell
treatment is clear but restrained, rows are complete and current, and the
tracker does not alter inventory or expose another interaction surface.

## B. Dragging, bounds, and contained input

1. Drag the tracker by its background. Confirm movement follows the pointer at
   the selected 4K/Auto setting and the complete panel remains reachable at
   every screen edge.
2. Place the panel over an innocuous clickable HUD area. Click and drag the
   tracker background and row content. Confirm the covered HUD control and the
   game world do not receive those pointer actions.

Expected result: dragging is usable at 3840-by-2160/Auto, the fixed panel stays
within its effective 1920-by-1080 UI layout, and tracker-owned surfaces contain
their input.

## C. Automatic and manual visibility

1. With at least one pin present, use the panel Hide control. Confirm the panel
   hides and the paired global Show/Hide control restores it.
2. Leave the panel requested visible. Open and close each of these interfaces
   one at a time: Tech Tree, Dyson Sphere Editor, Inventory, Replicator,
   Statistics, and Dashboard.
3. For each interface, confirm the tracker hides while it is open and returns
   after it closes. Confirm a manual Hide remains respected rather than being
   overwritten by opening or closing an interface.

Expected result: the paired controls preserve the player's choice, and only
the contracted six major interfaces apply automatic hiding.

## D. Lifecycle and cleanup symptoms

1. Close and reopen the Replicator during the session. Confirm pinning and the
   tracker remain responsive and no duplicate panel or control appears.
2. Exit DSP normally. Confirm shutdown does not hang or display an error.
3. Start DSP once more and load the same save. Confirm there is one tracker
   control, no duplicate UI, and no pins restored from the previous session.

Expected result: ordinary close, reopen, shutdown, and fresh-session behavior
show no duplicate listeners, leaked UI, stale callbacks, or persisted MVP
state.

## Reporting

Report one line for each group:

```text
A Native pinning and presentation: PASS | FAIL | UNEXPECTED - notes
B Dragging and contained input: PASS | FAIL | UNEXPECTED - notes
C Visibility: PASS | FAIL | UNEXPECTED - notes
D Lifecycle and cleanup: PASS | FAIL | UNEXPECTED - notes
```

If every group passes, no screenshot or log is needed. For a failure or
unexpected result, describe the exact step and visible behavior. Only then,
if diagnosis requires it, retain the relevant `DSP-Recipe-Tracker` and
`tracker-` lines from `BepInEx/LogOutput.log` or one screenshot showing the
specific visual/input problem; do not provide the full log, save, or unrelated
player data.
