# UnityMeshSimplifier
Mesh simplification for [Unity](https://unity3d.com/). The project is deeply based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm, but rewritten entirely in C# and released under the MIT license.

Because of the fact that this project is entirely in C# it *should* work on all platforms that Unity officially supports, as well as both in the editor and at runtime in builds.

## Compatibility
These scripts have been tested and confirmed working with Unity 5.6.0f3, Unity 2017.1.0f3, Unity 2017.2.1f1, Unity 2017.3.0f3, Unity 2017.4.0f1 and Unity 2018.1.2f1.

## Installation into Unity project
1. Copy the contents of this repository into a folder named *UnityMeshSimplifier* in your Assets directory within your Unity project.
2. Done!

## How do I use this?
```c#
float quality = 0.5f;
var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
meshSimplifier.Initialize(sourceMesh);
meshSimplifier.SimplifyMesh(quality);
var destMesh = meshSimplifier.ToMesh();
```

or

```c#
float quality = 0.5f;
var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
meshSimplifier.Vertices = vertices;
meshSimplifier.AddSubMeshTriangles(indices);
meshSimplifier.SimplifyMesh(quality);
var newVertices = meshSimplifier.Vertices;
var newIndices = meshSimplifier.GetSubMeshTriangles(0);
```

## The Smart Linking feature
In order to solve artifacts in the mesh simplification process where holes or other serious issues could arise, a new feature called smart linking has been introduced. This feature is enabled by default but can be disabled through the *EnableSmartLink* property on the *MeshSimplifier* class. Disabling this could give you a minor performance gain in cases where you do not need this.

The *VertexLinkDistanceSqr* property on the *MeshSimplifier* class could be used to change the maximum squared distance between two vertices for the linking. The default value is *double.Epsilon*.

## My decimated meshes have holes, what is wrong?
The original algorithm that was ported from C++ did not support situations where multiple vertices shared the same position, instead of being treated as one vertex they were treated individually. This would then end up creating visible holes in the mesh where the vertices were not connected through triangles.

There are several ways to solve this problem. The smart linking feature (mentioned above) is enabled by default and should take care of most of these problems for you. But there are also options to preserve borders, seams and UV foldovers. The properties *PreserveBorders*, *PreserveSeams* and *PreserveFoldovers* will preserve some vertices from being decimated, strongly limiting the decimation algorithm, but should prevent holes in most situations.

The recommendation is to use the smart linking feature that is enabled by default, but the options for preservation exists in those cases where you may want it.

## The Unity-generated Visual Studio solution file appears broken
This can be a problem because of an assembly definition provided with this repository, if you are using Unity 2017.3 or above. Make sure that you have the latest version of [Visual Studio Tools for Unity](https://www.visualstudio.com/vs/unity-tools/). If you are using Visual Studio 2017, make sure that Visual Studio is up to date and that you have installed the *Game development with Unity* component. For other versions of Visual Studio you would have to download a separate installer. Please go to the [Microsoft Documentation](https://docs.microsoft.com/en-us/visualstudio/cross-platform/getting-started-with-visual-studio-tools-for-unity) for more information.

## Other projects
If you are interested in mesh simplification in .NET outside of Unity you can visit my other project [MeshDecimator](https://github.com/Whinarn/MeshDecimator).
