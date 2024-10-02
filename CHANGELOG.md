# Changelog

## [2.0.3] - 2024-10-02

### Changed

- Suppress Harmony warnings while checking for optional methods to improve the signal-to-noise ratio in logs ([`ae62dca`](https://github.com/toebeann/Tobey.UnityAudio/commit/ae62dcaa77af1d591c61d9a3527f0d6f2549dbc5))

## [2.0.2] - 2023-12-22

### Fixed

- Reset default value for `Patching.ExcludedAssemblies` config value to `string.Empty` ([`856acbe`](https://github.com/toebeann/Tobey.UnityAudio/commit/856acbeb6b687925c21ef38f87d5c955db417600))
- Correct a typo in the description of the `RuntimeTesting.PlayAudioFileKeyboardShortcut` config value ([`4eb82c5`](https://github.com/toebeann/Tobey.UnityAudio/commit/4eb82c5f2c276a29e4153c0dbd5abe8a409e87ce))

## [2.0.1] - 2023-12-22

### Changed

- Add sensible defaults for `Patching.ExcludedAssemblies` config value ([`7ab989e`](https://github.com/toebeann/Tobey.UnityAudio/commit/7ab989e422caeca8f5b347e4a32fbfc133d2c3cd))

## [2.0.0] - 2023-09-01

### Changed

- **Breaking:** Rename `Tobey.UnityAudio.Core.dll` -> `Tobey.UnityAudio.Shared.dll` ([`2390ecb`](https://github.com/toebeann/Tobey.UnityAudio/commit/2390ecb8b4282985cdf2c43d607a60990131217f))
- Don't use a cache file anymore ([`ca138aa`](https://github.com/toebeann/Tobey.UnityAudio/commit/ca138aaf3312a12383a692e847e053b1b84f9727))

### Removed

- **Breaking:** Remove ILmerged dependency on `Newtonsoft.Json` ([`ca138aa`](https://github.com/toebeann/Tobey.UnityAudio/commit/ca138aaf3312a12383a692e847e053b1b84f9727))

## [1.0.3] - 2023-04-08

### Fixed

- Fix corrupted `classdata.tpk` ([`2e3e05c`](https://github.com/toebeann/Tobey.UnityAudio/commit/2e3e05c2b4640f721df0065a62e77747484bc39a))

## [1.0.2] - 2023-04-08

### Fixed

- Ignore non-.NET assemblies when using automatic patching ([`1651e99`](https://github.com/toebeann/Tobey.UnityAudio/commit/1651e9988a1aba724246feff56302c03d1b5c0f0))

## [1.0.1] - 2023-04-03

### Fixed

- Merge `System.Runtime.Serialization` into the `Patcher` assembly to prevent issues when the game does not include it ([`3dfd18b`](https://github.com/toebeann/Tobey.UnityAudio/commit/3dfd18b2fad34b0cdd2ccf01e92bd0f6b5c8683a))

## [1.0.0] - 2023-04-02

Initial release 🚀

[2.0.3]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v2.0.3
[2.0.2]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v2.0.2
[2.0.1]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v2.0.1
[2.0.0]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v2.0.0
[1.0.3]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v1.0.3
[1.0.2]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v1.0.2
[1.0.1]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v1.0.1
[1.0.0]: https://github.com/toebeann/Tobey.UnityAudio/releases/tag/v1.0.0
