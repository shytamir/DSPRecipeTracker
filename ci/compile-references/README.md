# Hosted compile-reference boundary

This directory is the fixed source root for declaration-only DSP and Unity
compile-reference shims used by hosted builds.

S1-01 establishes the boundary and its machine-readable surface inventory. It
does not create placeholder shim projects. The consuming implementation story
adds a shim project only when production source requires that external assembly
identity.

Shims contain declarations only. They are never runtime substitutes, are never
installed, and are excluded from every package. Every consumed DSP or Unity
symbol must be represented in `surface-inventory.json` and checked against the
compiled production assembly as required by
[`THUNDERSTORE-PACKAGE.md`](../../docs/THUNDERSTORE-PACKAGE.md).
