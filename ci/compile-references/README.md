# Hosted compile-reference boundary

This directory is the fixed source root for declaration-only DSP and Unity
compile-reference shims used by hosted builds.

The `UnityEngine` facade supplies the transitive `MonoBehaviour` declaration
required by BepInEx. S1-06 adds exact `UnityEngine.CoreModule` and
`UnityEngine.UI` declarations for tracker-owned panel creation, layout,
raycast containment, visibility, native-sprite assignment, and cleanup. No
production source consumes a DSP game member yet, so no DSP game shim exists.

Shims contain declarations only. They are never runtime substitutes, are never
installed, and are excluded from every package. Every consumed DSP or Unity
symbol and inheritance edge must be represented in
`surface-inventory.json`. The validator checks every shim together against
both real-reference and hosted-reference production builds as required by
[`THUNDERSTORE-PACKAGE.md`](../../docs/THUNDERSTORE-PACKAGE.md).
