# Hosted compile-reference boundary

This directory is the fixed source root for declaration-only DSP and Unity
compile-reference shims used by hosted builds.

The `UnityEngine` facade supplies the transitive `MonoBehaviour` declaration
required by BepInEx. The Unity shims declare the exact panel, Replicator,
image, text, and font surface consumed by production. `DSPGame.Reference`
declares only the consumed Replicator, visibility, recipe, item-icon, player,
Icarus-inventory, overlay-canvas, pointer-drag, and parent-rectangle surface
needed through S3-05.
Private reflection bindings are inventoried separately and confirmed through
bounded read-only inspection; they are not misrepresented as public
compile-time members.

Shims contain declarations only. They are never runtime substitutes, are never
installed, and are excluded from every package. Every consumed DSP or Unity
symbol and inheritance edge must be represented in
`surface-inventory.json`. The validator checks every shim together against
both real-reference and hosted-reference production builds as required by
[`THUNDERSTORE-PACKAGE.md`](../../docs/THUNDERSTORE-PACKAGE.md).
