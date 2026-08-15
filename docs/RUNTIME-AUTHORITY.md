# Runtime Authority Policy

## Status and purpose

**Status:** Current runtime-authority policy

This document governs how runtime facts are established and used. It is not an
inspection diary, evidence archive, compatibility database, or runtime gate.
Product behavior remains governed by [`PROJECT.md`](PROJECT.md), while
[`FEASIBILITY.md`](FEASIBILITY.md) retains the accepted findings that still
constrain implementation.

## Authority hierarchy

Use sources in this order:

1. Installed DSP and Unity assemblies may be inspected as read-only inputs for
   exact types, members, signatures, inheritance, and narrowly required method
   behavior.
2. Owner-accepted findings in `FEASIBILITY.md` govern the runtime conclusions
   they preserve until directly superseded.
3. [`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md) separately governs the
   supported loader and plugin-manager surface.
4. Community documentation, UI-derived names, and compile-reference shims may
   guide investigation but do not establish runtime authority.

Confirmed facts must remain distinct from inference. When evidence does not
establish a required claim, record the uncertainty and fail the affected
integration softly instead of inventing compatibility.

## Tracked consumed-surface contract

`ci/compile-references/surface-inventory.json` is the tracked inventory of DSP,
Unity, and BepInEx symbols consumed by production. Declaration-only hosted
shims must cover that inventory exhaustively and must not declare unrelated
runtime behavior.

Tracked contracts retain only current product-facing conclusions and consumed
surfaces. Detailed assembly identities, inspection transcripts, derived
datasets, story-by-story evidence, and historical provenance remain outside
the tracked repository and are not required build or validation inputs.

## Inspection and validation rules

- Inspect installed assemblies only when an active story requires unresolved
  runtime knowledge.
- Ordinary story validators must not pin or compare game or Unity assembly
  hashes.
- Validators may confirm the presence and exact shape of consumed members and,
  where the story depends on it, narrowly inspect relevant method behavior.
- A supplied assembly passes the source gate when the consumed contract remains
  compatible; its broader binary identity is irrelevant to that gate.
- If a consumed member or required behavior changes, stop the affected work,
  report the mismatch, and re-establish only the necessary contract.
- Update production access, the consumed-surface inventory, declaration shims,
  focused tests, and current documentation together when that contract changes.

Static inspection and compilation establish source compatibility only. They do
not establish installed behavior, interaction, presentation, performance, or
compatibility beyond the consumed surface. Runtime validation remains owner
performed under [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md).

## Runtime and repository boundary

Product code must not locate, hash, or validate authority assemblies during
runtime execution. Runtime adapters consume the narrow documented surface and
fail softly when expected objects or members are unavailable.

Do not copy or commit game, Unity, or BepInEx binaries, generated runtime
datasets, player data, saves, logs, or environment-derived evidence. External
working evidence may assist later investigation, but it is not repository
authority and must never become an implicit prerequisite for building,
validating, or reviewing the project.
