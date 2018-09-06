# Change Log

## [v1.0.2] - 2018-07-05

### Fixed
- Fixed a documentation mistake with the VertexLinkDistanceSqr property on the MeshSimplifier class.

## [v1.0.1] - 2018-06-03

### Fixed
- Added more exception throwing on invalid parameters that wasn't previously handled.
- Added assertions when getting the triangle indices for a sub-mesh, to detect a faulty state more easily.
- Optimized the retrieving of sub-mesh triangles when having a large number of sub-meshes.
- Heavily optimized the initialization and simplification process.

## [v1.0.0] - 2018-05-12

### Added
- Unity assembly definition file.
- Feature to change the maximum iteration count for the mesh simplification.

### Fixed
- Better support for skinned meshes.
- Support for Unity 2017.4 and 2018.X

## v0.1.0 - 2018-04-01

### Added
- A mesh simplification algorithm based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm.
- A feature (Smart Linking) that attempts to solve problems where holes could appear in simplified meshes.
- Support for static and skinned meshes.
- Support for 2D, 3D and 4D UVs.

[v1.0.2]: https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.1...v1.0.2
[v1.0.1]: https://github.com/Whinarn/UnityMeshSimplifier/compare/v1.0.0...v1.0.1
[v1.0.0]: https://github.com/Whinarn/UnityMeshSimplifier/compare/v0.1.0...v1.0.0
