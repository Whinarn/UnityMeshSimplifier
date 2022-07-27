# UnityMeshSimplifier

[![openupm](https://img.shields.io/npm/v/com.whinarn.unitymeshsimplifier?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.whinarn.unitymeshsimplifier/)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/3c6b55bddfe64912b56e6759c642939d)](https://www.codacy.com/manual/Whinarn/UnityMeshSimplifier?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Whinarn/UnityMeshSimplifier&amp;utm_campaign=Badge_Grade)
[![CircleCI](https://img.shields.io/circleci/build/gh/Whinarn/UnityMeshSimplifier?label=circle-ci)](https://circleci.com/gh/Whinarn/UnityMeshSimplifier/tree/master)
[![GitHub Release Status](https://img.shields.io/github/workflow/status/Whinarn/UnityMeshSimplifier/Release?label=release)](https://github.com/Whinarn/UnityMeshSimplifier/actions?query=workflow%3ARelease)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/Whinarn/UnityMeshSimplifier/blob/master/LICENSE.md)
[![semantic-release](https://img.shields.io/badge/%20%20%F0%9F%93%A6%F0%9F%9A%80-semantic--release-e10079.svg)](https://github.com/semantic-release/semantic-release)

Mesh simplification for [Unity](https://unity3d.com/). The project is deeply based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm, but rewritten entirely in C# and released under the MIT license.

Because of the fact that this project is entirely in C# it *should* work on all platforms that Unity officially supports, as well as both in the editor and at runtime in builds.

## Up for adoption

Hello there! Are you interested in mesh simplification and would like to take this package further?

I'm currently looking for someone to take over this repository since I feel like I'm not the right person to do it, nor do I have enough time to spend on it. Or if you'd just like to be a maintainer. Preferably someone with a better knowledge and interest in mesh simplification.

If you think that's you or your organization, please open up an issue to start a discussion.

## Compatibility

Because this project is now using Unity packages, you should use a Unity version from 2018.1 and beyond.
Although some scripts have been tested and confirmed working as far back as Unity 5.6, it will no longer be officially supported.
Unity introduced the package manager in Unity 2017.2, but at a very early state.

The code will only (at the time of writing) compile with the *.NET Standard 2.0* or *.NET 4.X* scripting runtime versions. The legacy scripting runtime is no longer supported.
Read more about this [here](https://docs.unity3d.com/2019.1/Documentation/Manual/dotnetProfileSupport.html).

## Installation into Unity project

You will find different methods of installing this library into your Unity project [here](https://github.com/Whinarn/UnityMeshSimplifier/wiki#installing).

## How to use

If you are not a programmer look into the [LOD Generator Helper component](https://github.com/Whinarn/UnityMeshSimplifier/wiki/LOD-Generator-Helper-component).

For programmers you will find documentation for the [Mesh Simplifier API](https://github.com/Whinarn/UnityMeshSimplifier/wiki/Mesh-Simplifier-API) and the [LOD Generator API](https://github.com/Whinarn/UnityMeshSimplifier/wiki/LOD-Generator-API) useful.

## The Smart Linking feature

In order to solve artifacts in the mesh simplification process where holes or other serious issues could arise, a new feature called smart linking has been introduced. This feature is enabled by default but can be disabled through the `EnableSmartLink` field on the `SimplificationOptions` struct appled to the `MeshSimplifier` class through the `SimplificationOptions` property. Disabling this could give you a minor performance gain in cases where you do not need this.

The `VertexLinkDistance` field on the `SimplificationOptions` struct could be also be used to change the maximum distance between two vertices for the linking. The default value is `double.Epsilon`. This value maximum distance is intended to be very small, but you might need to increase it for large scale meshes.

## Potential problems and solutions

### I'm missing XML documentation in my project

If you have installed this library through the Git URL in the Unity package manager, then you will likely see this problem.
The solution is quite simple however. Open up your Unity Editor, go to `Edit` > `Preferences` and then under `External Tools` you will have multiple options under `Generate .csproj files for:`. Now simply make sure that Git packages and whatever other kind of package is selected here. You should now see the XML documentation for the API show up in your code editor after having also clicked `Regenerate project files`.

### My decimated meshes have holes

The original algorithm that was ported from C++ did not support situations where multiple vertices shared the same position, instead of being treated as one vertex they were treated individually. This would then end up creating visible holes in the mesh where the vertices were not connected through triangles.

There are several ways to solve this problem. The smart linking feature (mentioned above) is enabled by default and should take care of most of these problems for you. But there are also options to preserve borders, seams and UV foldovers. The properties *PreserveBorders*, *PreserveSeams* and *PreserveFoldovers* will preserve some vertices from being decimated, strongly limiting the decimation algorithm, but should prevent holes in most situations.

The recommendation is to use the smart linking feature that is enabled by default, but the options for preservation exists in those cases where you may want it.

### Some objects are not animated correctly after I have generated LOD Groups

The most probable cause for this is that you have objects that are parented under bones that will move with the animations. Currently there is no code to deal with this, and the best way to do this is to use nested LOD Groups. Any such object that you know is parented under a bone should have its own LOD Group.

### The Unity-generated Visual Studio solution file appears broken

This can be a problem because of an assembly definition provided with this repository, if you are using Unity 2017.3 or above. Make sure that you have the latest version of [Visual Studio Tools for Unity](https://www.visualstudio.com/vs/unity-tools/). If you are using Visual Studio 2017, make sure that Visual Studio is up to date and that you have installed the *Game development with Unity* component. For other versions of Visual Studio you would have to download a separate installer. Please go to the [Microsoft Documentation](https://docs.microsoft.com/en-us/visualstudio/cross-platform/getting-started-with-visual-studio-tools-for-unity) for more information.

## How to contribute

Please read [CONTRIBUTING](https://github.com/Whinarn/UnityMeshSimplifier/blob/master/CONTRIBUTING.md) regarding how to contribute.
