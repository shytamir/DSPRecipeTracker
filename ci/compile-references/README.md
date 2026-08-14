# Hosted compile-reference boundary

This directory is the fixed source root for declaration-only DSP and Unity
compile-reference shims used by hosted builds.

The `UnityEngine` facade supplies the transitive `MonoBehaviour` declaration
required by BepInEx. The Unity shims declare the exact panel and Replicator
event and rendering surface consumed by production. `DSPGame.Reference`
declares only the Replicator host, populated recipe type, recipe identity,
public native background/icon fields, the exact six-window active-state
surface, and direct recipe identity-to-icon lookup needed by S2-02 through
S2-05.
Private reflection bindings are inventoried separately and validated against
the hash-matched installed metadata; they are not misrepresented as public
compile-time members.

Shims contain declarations only. They are never runtime substitutes, are never
installed, and are excluded from every package. Every consumed DSP or Unity
symbol and inheritance edge must be represented in
`surface-inventory.json`. The validator checks every shim together against
both real-reference and hosted-reference production builds as required by
[`THUNDERSTORE-PACKAGE.md`](../../docs/THUNDERSTORE-PACKAGE.md).
