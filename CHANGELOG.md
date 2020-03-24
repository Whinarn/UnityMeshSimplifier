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
