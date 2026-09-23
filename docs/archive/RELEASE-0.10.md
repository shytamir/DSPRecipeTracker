# Release 0.10 - Completion Record

**Recorded:** 2026-09-24 (Europe/Madrid)

**Status:** Requested work complete; release `0.10.33` tagged and published;
repository returned to maintenance mode at the owner's request.

This historical record closes the work performed on 2026-09-23 and the
publication confirmed on 2026-09-24. It does not activate a story, validation
gate, or further implementation. Current authority remains in
[`PROJECT.md`](../PROJECT.md) and [`ROADMAP.md`](../ROADMAP.md).

## Completed work

| Commit | Result |
| --- | --- |
| `fab76d6` | Recorded full support for DSP `0.10.35.29057` after the owner reported that all existing validations passed and the mod worked unchanged. Original game-version and feasibility evidence was preserved. |
| `57903b7` | Promoted the release line from `0.9` to `0.10`; GitHub Actions run 32 produced and validated `0.10.32`. |
| `a184433` | Preserved the compiled DLL's modification time in the package so BepInEx can invalidate cached plugin metadata. Added a fixed-timestamp rejection and regression case. The minor version remained `0.10`; run 33 produced `0.10.33`. |

The timestamp correction addresses the owner's report that the chainloader
announced `0.9.30` while the loaded plugin logged `0.10.32.57903b702cc8`.
BepInEx `5.4.17` caches metadata by DLL path and modification time; the previous
package assigned every DLL the same 1980 timestamp. The correction uses native
cache invalidation without changing the loader or accessing its cache.

## Validation

- [Actions run 32](https://github.com/shytamir/DSPRecipeTracker/actions/runs/35921596823)
  passed for `57903b702cc836290bfdba9e3a1c94cbe1464d31`. Downloaded package,
  manifest, plugin metadata, assembly/file versions, diagnostic label, build
  information, and ZIP/DLL hashes agreed on `0.10.32`.
- The timestamp fix passed a Hosted Release build with zero warnings/errors,
  deterministic tests, compile-reference coverage, and package regression
  cases. The old run-32 ZIP was rejected specifically for its fixed timestamp.
- [Actions run 33](https://github.com/shytamir/DSPRecipeTracker/actions/runs/35924611079)
  passed for `a1844332a5481593123ea91b4ba3de97c5f7f09e`. Independent inspection
  verified `0.10.33`, assembly/file version `0.10.33.0`, diagnostic label
  `0.10.33.a1844332a548`, matching hashes, and a changed extracted DLL timestamp.
- Agent checks were static, deterministic, build, package, and repository-local
  extraction checks. The owner supplied game-update validation; no separate
  owner restart result for the timestamp correction was recorded.

## Publication and closeout

The owner confirmed publication and requested this closeout on 2026-09-24.
Public checks established:

- [GitHub release/tag `0.10.33`](https://github.com/shytamir/DSPRecipeTracker/releases/tag/0.10.33)
  points to `a1844332a5481593123ea91b4ba3de97c5f7f09e` and publishes
  `DSPRecipeTracker-0.10.33.zip`. The existing `0.10` tag points to the same
  source commit, which includes the timestamp correction.
- [Thunderstore](https://thunderstore.io/c/dyson-sphere-program/p/DSPRecipeTracker/DSPRecipeTracker/)
  serves `DSPRecipeTracker-DSPRecipeTracker-0.10.33`.
- Both public ZIPs exactly match the validated run-33 artifact (SHA-256
  `976aa39647c925cd8872eb4a9909e3d57962d2dd852864ffa859a7ee2f13f6fe`). The
  downloaded store package passed `PackageValidator` again for semantic
  version `0.10.33`, assembly/file version `0.10.33.0`, diagnostic label
  `0.10.33.a1844332a548`, plugin identity, contents, hashes, and DLL timestamp.

The source correction and its publication are complete. The repository is in
maintenance mode with no active story or validation gate. `VERSION` remains
`MAJOR=0`, `MINOR=10`; no `0.11` promotion or new roadmap is activated.
