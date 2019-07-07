# UnityMeshSimplifier
Mesh simplification for [Unity](https://unity3d.com/). The project is deeply based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm, but rewritten entirely in C# and released under the MIT license.

Because of the fact that this project is entirely in C# it *should* work on all platforms that Unity officially supports, as well as both in the editor and at runtime in builds.

## Compatibility
Because this project is now using Unity packages, you should use a Unity version from 2018.1 and beyond.
Although some scripts have been tested and confirmed working as far back as Unity 5.6, it will no longer be officially supported.
Unity introduced the package manager in Unity 2017.2, but at a very early state.

## Installation into Unity project
1. Read the instructions from the official Unity documentation: https://docs.unity3d.com/Manual/upm-dependencies.html#Git
2. Open up *manifest.json* inside the *Packages* directory in your Unity project using a text editor.
3. Under the dependencies section of this file, you should add the following line at the top:
```"com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git",```
4. You should now see something like this:
```
{
  "dependencies": {
    "com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git",
    "com.unity.burst": "1.0.4",
    "com.unity.mathematics": "1.0.1",
    "com.unity.package-manager-ui": "2.1.2",
    ...
  }
}
```
5. You can also specify to use a specific version of UnityMeshSimplifier if you wish by appending # to the Git URL followed by the package version. For example:
```"com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git#v1.1.0",```
6. Success! Start up Unity with your Unity project and you should see UnityMeshSimplifier appear in the Unity Package Manager.

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

## How do I contribute?
1. Create a new empty Unity project, or use an existing one if you wish.
2. Fork your own copy of this repository.
2. Clone your UnityMeshSimplifier fork into the *Packages* directory of your Unity project.
3. Start up your Unity project and you should see UnityMeshSimplifier appear in the Unity Package Manager.
4. Open the scripts inside of the Unity package as you would normally do with scripts in your *Assets* directory.
5. Make your changes inside a branch based on *master*.
6. Create a pull request to the official repository.
7. Success!

## The Smart Linking feature
In order to solve artifacts in the mesh simplification process where holes or other serious issues could arise, a new feature called smart linking has been introduced. This feature is enabled by default but can be disabled through the *EnableSmartLink* property on the *MeshSimplifier* class. Disabling this could give you a minor performance gain in cases where you do not need this.

The *VertexLinkDistanceSqr* property on the *MeshSimplifier* class could be used to change the maximum squared distance between two vertices for the linking. The default value is *double.Epsilon*.

## My decimated meshes have holes, what is wrong?
The original algorithm that was ported from C++ did not support situations where multiple vertices shared the same position, instead of being treated as one vertex they were treated individually. This would then end up creating visible holes in the mesh where the vertices were not connected through triangles.

There are several ways to solve this problem. The smart linking feature (mentioned above) is enabled by default and should take care of most of these problems for you. But there are also options to preserve borders, seams and UV foldovers. The properties *PreserveBorders*, *PreserveSeams* and *PreserveFoldovers* will preserve some vertices from being decimated, strongly limiting the decimation algorithm, but should prevent holes in most situations.

The recommendation is to use the smart linking feature that is enabled by default, but the options for preservation exists in those cases where you may want it.

## My animated meshes don't work, why?
This is most probably because there is currently no code for moving the [bindposes](https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html) over between the original and the simplified mesh. This can be easily resolved by copying over (no need to modify) the bindposes like this:

```c#
float quality = 0.5f;
var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
meshSimplifier.Initialize(sourceMesh);
meshSimplifier.SimplifyMesh(quality);
var destMesh = meshSimplifier.ToMesh();
destMesh.bindposes = sourceMesh.bindposes; // <-- this line should fix your issue
```

## How can I automatically generated LOD Groups?
There is a component named *LOD Generator Helper* that you add to the game object that you want to generate LODs for. You can customize, generate and destroy the LOD levels directly through the inspector. Any changes is saved within the component so that you can easily make the changes that you want without having to waste time reconfiguring everything again. Additional steps have been taken in order to protect your game objects from damage and makes sure that they can be restored back to their original state. Backups are always recommended however, to make sure that you do not ever lose any configuration that you have made.

There is also a static API at *UnityMeshSimplifier.LODGenerator* that you can use from code to generate and destroy LODs, both at runtime and in the editor.

## Some objects are not animated correctly after I have generated LOD Groups
The most probable cause for this is that you have objects that are parented under bones that will move with the animations. Currently there is no code to deal with this, and the best way to do this is to use nested LOD Groups. Any such object that you know is parented under a bone should have its own LOD Group.

## The Unity-generated Visual Studio solution file appears broken
This can be a problem because of an assembly definition provided with this repository, if you are using Unity 2017.3 or above. Make sure that you have the latest version of [Visual Studio Tools for Unity](https://www.visualstudio.com/vs/unity-tools/). If you are using Visual Studio 2017, make sure that Visual Studio is up to date and that you have installed the *Game development with Unity* component. For other versions of Visual Studio you would have to download a separate installer. Please go to the [Microsoft Documentation](https://docs.microsoft.com/en-us/visualstudio/cross-platform/getting-started-with-visual-studio-tools-for-unity) for more information.

## Other projects
If you are interested in mesh simplification in .NET outside of Unity you can visit my other project [MeshDecimator](https://github.com/Whinarn/MeshDecimator).
