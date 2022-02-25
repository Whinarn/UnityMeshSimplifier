# [3.1.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v3.0.1...v3.1.0) (2022-02-25)


### Features

* **simplifier:** added options validation ([55264ce](https://github.com/Whinarn/UnityMeshSimplifier/commit/55264ce8a28f6755a2a920f25aa2312f2d185e72))

## [3.0.1](https://github.com/Whinarn/UnityMeshSimplifier/compare/v3.0.0...v3.0.1) (2021-06-25)


### Bug Fixes

* support Unity 2021 ([2f2eb3b](https://github.com/Whinarn/UnityMeshSimplifier/commit/2f2eb3bee2ac1d1691373200983ac64ae507dfc8))

# [3.0.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.6...v3.0.0) (2021-03-27)


### Bug Fixes

* **lods:** prevent empty mesh names ([ec6cf87](https://github.com/Whinarn/UnityMeshSimplifier/commit/ec6cf87c4c652aca953cb782d8796749037322a6))
* **simplifier:** obsolete properties that moved to options struct ([84664d1](https://github.com/Whinarn/UnityMeshSimplifier/commit/84664d1852da93e5206f335d17db7025c7be5281))
* **simplifier:** options for uv component count ([7f7b773](https://github.com/Whinarn/UnityMeshSimplifier/commit/7f7b77370442f5e49dd74b2e8c39a427b21980ab))
* **simplifier:** removed obsolete properties ([24ccc56](https://github.com/Whinarn/UnityMeshSimplifier/commit/24ccc5663c1281e2feadf6c03e947481a68844c2))


### Features

* **lods:** save assets path is now less restricted ([380bda9](https://github.com/Whinarn/UnityMeshSimplifier/commit/380bda999c8b95baed6a6533b0b66bfb4eb8d2c9))
* **package:** bump minimum required version of Unity to 2018.1 ([8f37f81](https://github.com/Whinarn/UnityMeshSimplifier/commit/8f37f81a7688c2ff3a895c9616e4b171cd54a633))
* **simplifier:** set manual uv component count ([9795860](https://github.com/Whinarn/UnityMeshSimplifier/commit/9795860c6354b87924b97b354688e2b59098eda7))


### BREAKING CHANGES

* **simplifier:** removed obsolete properties
* **lods:** save assets paths are now related to Assets/ rather
than Assets/UMS_LODS/

## [2.3.6](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.5...v2.3.6) (2020-11-20)


### Bug Fixes

* **simplifier:** prevent near zero denominator ([469272a](https://github.com/Whinarn/UnityMeshSimplifier/commit/469272ae031774383eb13253e7e2d5abbaafe79c))

## [2.3.5](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.4...v2.3.5) (2020-10-10)


### Bug Fixes

* **lod-generator-helper:** mark scene as dirty ([7554f8e](https://github.com/Whinarn/UnityMeshSimplifier/commit/7554f8ecb7cd105aacdad4a07d32eaf081cf7ae3))
* **unity:** increase support for unity 2020 ([7a28614](https://github.com/Whinarn/UnityMeshSimplifier/commit/7a286148aecef6f7918abce34f3e7000f2856f4a))

## [2.3.4](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.3...v2.3.4) (2020-08-25)


### Bug Fixes

* **tests:** compile errors on 2018.4.26f1 ([#38](https://github.com/Whinarn/UnityMeshSimplifier/issues/38)) ([91b5ad7](https://github.com/Whinarn/UnityMeshSimplifier/commit/91b5ad7de7d6f77d29275fc69d3e7506df6a586f))

## [2.3.3](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.2...v2.3.3) (2020-05-01)


### Bug Fixes

* **mesh:** bug related to degenerated triangle when using smart linking ([e9d5def](https://github.com/Whinarn/UnityMeshSimplifier/commit/e9d5def2eb6e18eed7e9f86943e5d32bf0721d60))

## [2.3.2](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.1...v2.3.2) (2020-04-30)


### Bug Fixes

* **meshutils:** GetUsedUVComponents should now be able to return zero ([af60914](https://github.com/Whinarn/UnityMeshSimplifier/commit/af6091481d212f5c98bf1f8f16cf922367f0a08d))

## [2.3.1](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.3.0...v2.3.1) (2020-04-14)


### Reverts

* temporarily allow refactor commits to trigger a release ([fd8d697](https://github.com/Whinarn/UnityMeshSimplifier/commit/fd8d69751038f0d8a6fc9880c3159d3718d6a2ee))

## [2.3.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.2.0...v2.3.0) (2020-04-14)

### Added

* mesh simplifier uses the simplification options struct ([87d3fa8](https://github.com/Whinarn/UnityMeshSimplifier/commit/87d3fa81419c4fce2d360572290bfecee7a3fbf9))

## [v2.2.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.1.0...v2.2.0) (2020-03-24)


### Added

* An option for preserving surface curvature.

### Fixed

* A bug with calculating vertex positions when combining meshes.

## [v2.1.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.0.1...v2.1.0) (2020-03-16)


### Added

* A button to copy visibility changes from the LOD Group.

### Fixed

* Fixed an incorrect tooltip for the MaxIterationCount of the SimplificationOptions struct.
* Fixed potential problems of destroying LODs after some renderers no longer exists.
* Meshes are always readable in editor.
* Fixed a warning that is printed when a folder that doesn't exist is attempted to be removed.
* Fixed bugs where mismatching bindposes resulted in incorrectly combined meshes.

## [v2.0.1](https://github.com/Whinarn/UnityMeshSimplifier/compare/v2.0.0...v2.0.1) (2019-07-12)


### Fixed

* Fixed compilation errors in Unity 2018

## [v2.0.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.3-legacy...v2.0.0) (2019-07-07)


### Added

* Unity package manifest file.
* LOD generator.
* Component to assist with LOD generation.
* Added support to interpolate blend shapes.
* Added support for up to 8 UV channels.

### Removed

* Removed the long obsolete KeepBorders property on the MeshSimplifier class.

### Changed

* Reorganized the project layout to match the Unity convention.
* The vertex attributes are now interpolated using barycentric coordinates.

## [v1.0.3](https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.2-legacy...v1.0.3-legacy) (2018-10-20)


### Fixed

* The maximum hash distance is now calculated based on the VertexLinkDistanceSqr property value instead of being hardcoded to 1.
* Fixed an issue with the vertex hashes not using the entire integer range, but instead was using only half of it.

## [v1.0.2](https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.1-legacy...v1.0.2-legacy) (2018-07-05)


### Fixed

* Fixed a documentation mistake with the VertexLinkDistanceSqr property on the MeshSimplifier class.

## [v1.0.1](https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.0-legacy...v1.0.1-legacy) (2018-06-03)


### Fixed

* Added more exception throwing on invalid parameters that wasn't previously handled.
* Added assertions when getting the triangle indices for a sub-mesh, to detect a faulty state more easily.
* Optimized the retrieving of sub-mesh triangles when having a large number of sub-meshes.
* Heavily optimized the initialization and simplification process.

## [v1.0.0](https://github.com/Whinarn/UnityMeshSimplifier/compare/v0.1.0-legacy...v1.0.0-legacy) (2018-05-12)


### Added

* Unity assembly definition file.
* Feature to change the maximum iteration count for the mesh simplification.

### Fixed

* Better support for skinned meshes.
* Support for Unity 2017.4 and 2018.X

## [v0.1.0](https://github.com/Whinarn/UnityMeshSimplifier/releases/tag/v0.1.0-legacy) (2018-04-01)


### Added

* A mesh simplification algorithm based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm.
* A feature (Smart Linking) that attempts to solve problems where holes could appear in simplified meshes.
* Support for static and skinned meshes.
* Support for 2D, 3D and 4D UVs.
