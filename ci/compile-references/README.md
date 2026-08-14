# Hosted compile-reference boundary

This directory is the fixed source root for declaration-only DSP and Unity
compile-reference shims used by hosted builds.

S1-02 adds only the Unity declaration needed to resolve BepInEx's
`BaseUnityPlugin` inheritance chain. No production source directly consumes a
game or Unity member yet, so no DSP game shim exists.

Shims contain declarations only. They are never runtime substitutes, are never
installed, and are excluded from every package. Every consumed DSP or Unity
symbol must be represented in `surface-inventory.json` and checked against the
compiled production assembly as required by
[`THUNDERSTORE-PACKAGE.md`](../../docs/THUNDERSTORE-PACKAGE.md).
