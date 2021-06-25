#region License
/*
MIT License

Copyright(c) 2017-2020 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

#if UNITY_2018_2_OR_NEWER
#define UNITY_8UV_SUPPORT
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// Contains utility methods for meshes.
    /// </summary>
    public static class MeshUtils
    {
        #region Static Read-Only
        /// <summary>
        /// The count of supported UV channels.
        /// </summary>
#if UNITY_8UV_SUPPORT
        public static readonly int UVChannelCount = 8;
#else
        public static readonly int UVChannelCount = 4;
#endif
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh sub-mesh indices.</param>
        /// <param name="normals">The mesh normals.</param>
        /// <param name="tangents">The mesh tangents.</param>
        /// <param name="colors">The mesh colors.</param>
        /// <param name="boneWeights">The mesh bone-weights.</param>
        /// <param name="uvs">The mesh 2D UV sets.</param>
        /// <param name="bindposes">The mesh bindposes.</param>
        /// <returns>The created mesh.</returns>
        public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector2>[] uvs, Matrix4x4[] bindposes, BlendShape[] blendShapes)
        {
            return CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, uvs, null, null, bindposes, blendShapes);
        }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh sub-mesh indices.</param>
        /// <param name="normals">The mesh normals.</param>
        /// <param name="tangents">The mesh tangents.</param>
        /// <param name="colors">The mesh colors.</param>
        /// <param name="boneWeights">The mesh bone-weights.</param>
        /// <param name="uvs">The mesh 4D UV sets.</param>
        /// <param name="bindposes">The mesh bindposes.</param>
        /// <returns>The created mesh.</returns>
        public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector4>[] uvs, Matrix4x4[] bindposes, BlendShape[] blendShapes)
        {
            return CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, null, null, uvs, bindposes, blendShapes);
        }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh sub-mesh indices.</param>
        /// <param name="normals">The mesh normals.</param>
        /// <param name="tangents">The mesh tangents.</param>
        /// <param name="colors">The mesh colors.</param>
        /// <param name="boneWeights">The mesh bone-weights.</param>
        /// <param name="uvs2D">The mesh 2D UV sets.</param>
        /// <param name="uvs3D">The mesh 3D UV sets.</param>
        /// <param name="uvs4D">The mesh 4D UV sets.</param>
        /// <param name="bindposes">The mesh bindposes.</param>
        /// <returns>The created mesh.</returns>
        public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector2>[] uvs2D, List<Vector3>[] uvs3D, List<Vector4>[] uvs4D, Matrix4x4[] bindposes, BlendShape[] blendShapes)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            else if (indices == null)
                throw new ArgumentNullException(nameof(indices));

            var newMesh = new Mesh();
            int subMeshCount = indices.Length;

            IndexFormat indexFormat;
            var indexMinMax = MeshUtils.GetSubMeshIndexMinMax(indices, out indexFormat);
            newMesh.indexFormat = indexFormat;

            if (bindposes != null && bindposes.Length > 0)
            {
                newMesh.bindposes = bindposes;
            }

            newMesh.subMeshCount = subMeshCount;
            newMesh.vertices = vertices;
            if (normals != null && normals.Length > 0)
            {
                newMesh.normals = normals;
            }
            if (tangents != null && tangents.Length > 0)
            {
                newMesh.tangents = tangents;
            }
            if (colors != null && colors.Length > 0)
            {
                newMesh.colors = colors;
            }
            if (boneWeights != null && boneWeights.Length > 0)
            {
                newMesh.boneWeights = boneWeights;
            }

            if (uvs2D != null)
            {
                for (int uvChannel = 0; uvChannel < uvs2D.Length; uvChannel++)
                {
                    if (uvs2D[uvChannel] != null && uvs2D[uvChannel].Count > 0)
                    {
                        newMesh.SetUVs(uvChannel, uvs2D[uvChannel]);
                    }
                }
            }

            if (uvs3D != null)
            {
                for (int uvChannel = 0; uvChannel < uvs3D.Length; uvChannel++)
                {
                    if (uvs3D[uvChannel] != null && uvs3D[uvChannel].Count > 0)
                    {
                        newMesh.SetUVs(uvChannel, uvs3D[uvChannel]);
                    }
                }
            }

            if (uvs4D != null)
            {
                for (int uvChannel = 0; uvChannel < uvs4D.Length; uvChannel++)
                {
                    if (uvs4D[uvChannel] != null && uvs4D[uvChannel].Count > 0)
                    {
                        newMesh.SetUVs(uvChannel, uvs4D[uvChannel]);
                    }
                }
            }

            if (blendShapes != null)
            {
                MeshUtils.ApplyMeshBlendShapes(newMesh, blendShapes);
            }

            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                var subMeshTriangles = indices[subMeshIndex];
                var minMax = indexMinMax[subMeshIndex];
                if (indexFormat == IndexFormat.UInt16 && minMax.y > ushort.MaxValue)
                {
                    int baseVertex = minMax.x;
                    for (int index = 0; index < subMeshTriangles.Length; index++)
                    {
                        subMeshTriangles[index] -= baseVertex;
                    }
                    newMesh.SetTriangles(subMeshTriangles, subMeshIndex, false, baseVertex);
                }
                else
                {
                    newMesh.SetTriangles(subMeshTriangles, subMeshIndex, false, 0);
                }
            }

            newMesh.RecalculateBounds();
            return newMesh;
        }

        /// <summary>
        /// Returns the blend shapes of a mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <returns>The mesh blend shapes.</returns>
        public static BlendShape[] GetMeshBlendShapes(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            int vertexCount = mesh.vertexCount;
            int blendShapeCount = mesh.blendShapeCount;
            if (blendShapeCount == 0)
                return null;

            var blendShapes = new BlendShape[blendShapeCount];

            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                string shapeName = mesh.GetBlendShapeName(blendShapeIndex);
                int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                var frames = new BlendShapeFrame[frameCount];

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float frameWeight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);

                    var deltaVertices = new Vector3[vertexCount];
                    var deltaNormals = new Vector3[vertexCount];
                    var deltaTangents = new Vector3[vertexCount];
                    mesh.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    frames[frameIndex] = new BlendShapeFrame(frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }

                blendShapes[blendShapeIndex] = new BlendShape(shapeName, frames);
            }

            return blendShapes;
        }

        /// <summary>
        /// Applies and overrides the specified blend shapes on the specified mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="blendShapes">The mesh blend shapes.</param>
        public static void ApplyMeshBlendShapes(Mesh mesh, BlendShape[] blendShapes)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            mesh.ClearBlendShapes();
            if (blendShapes == null || blendShapes.Length == 0)
                return;

            for (int blendShapeIndex = 0; blendShapeIndex < blendShapes.Length; blendShapeIndex++)
            {
                string shapeName = blendShapes[blendShapeIndex].ShapeName;
                var frames = blendShapes[blendShapeIndex].Frames;

                if (frames != null)
                {
                    for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
                    {
                        mesh.AddBlendShapeFrame(shapeName, frames[frameIndex].FrameWeight, frames[frameIndex].DeltaVertices, frames[frameIndex].DeltaNormals, frames[frameIndex].DeltaTangents);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the UV sets for a specific mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <returns>The UV sets.</returns>
        public static IList<Vector4>[] GetMeshUVs(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            var uvs = new IList<Vector4>[UVChannelCount];
            for (int channel = 0; channel < UVChannelCount; channel++)
            {
                uvs[channel] = GetMeshUVs(mesh, channel);
            }
            return uvs;
        }

        /// <summary>
        /// Returns the 2D UV list for a specific mesh and UV channel.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="channel">The UV channel.</param>
        /// <returns>The UV list.</returns>
        public static IList<Vector2> GetMeshUVs2D(Mesh mesh, int channel)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));
            else if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException(nameof(channel));

            var uvList = new List<Vector2>(mesh.vertexCount);
            mesh.GetUVs(channel, uvList);
            return uvList;
        }

        /// <summary>
        /// Returns the 3D UV list for a specific mesh and UV channel.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="channel">The UV channel.</param>
        /// <returns>The UV list.</returns>
        public static IList<Vector3> GetMeshUVs3D(Mesh mesh, int channel)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));
            else if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException(nameof(channel));

            var uvList = new List<Vector3>(mesh.vertexCount);
            mesh.GetUVs(channel, uvList);
            return uvList;
        }

        /// <summary>
        /// Returns the 4D UV list for a specific mesh and UV channel.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="channel">The UV channel.</param>
        /// <returns>The UV list.</returns>
        public static IList<Vector4> GetMeshUVs(Mesh mesh, int channel)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));
            else if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException(nameof(channel));

            var uvList = new List<Vector4>(mesh.vertexCount);
            mesh.GetUVs(channel, uvList);
            return uvList;
        }

        /// <summary>
        /// Returns the number of used UV components in a UV set.
        /// </summary>
        /// <param name="uvs">The UV set.</param>
        /// <returns>The number of used UV components.</returns>
        public static int GetUsedUVComponents(IList<Vector4> uvs)
        {
            if (uvs == null || uvs.Count == 0)
                return 0;

            int usedComponents = 0;
            foreach (var uv in uvs)
            {
                if (usedComponents < 1 && uv.x != 0f)
                {
                    usedComponents = 1;
                }
                if (usedComponents < 2 && uv.y != 0f)
                {
                    usedComponents = 2;
                }
                if (usedComponents < 3 && uv.z != 0f)
                {
                    usedComponents = 3;
                }
                if (usedComponents < 4 && uv.w != 0f)
                {
                    usedComponents = 4;
                    break;
                }
            }

            return usedComponents;
        }

        /// <summary>
        /// Converts a list of 4D UVs into 2D.
        /// </summary>
        /// <param name="uvs">The list of UVs.</param>
        /// <returns>The array of 2D UVs.</returns>
        public static Vector2[] ConvertUVsTo2D(IList<Vector4> uvs)
        {
            if (uvs == null)
                return null;

            var uv2D = new Vector2[uvs.Count];
            for (int i = 0; i < uv2D.Length; i++)
            {
                var uv = uvs[i];
                uv2D[i] = new Vector2(uv.x, uv.y);
            }
            return uv2D;
        }

        /// <summary>
        /// Converts a list of 4D UVs into 3D.
        /// </summary>
        /// <param name="uvs">The list of UVs.</param>
        /// <returns>The array of 3D UVs.</returns>
        public static Vector3[] ConvertUVsTo3D(IList<Vector4> uvs)
        {
            if (uvs == null)
                return null;

            var uv3D = new Vector3[uvs.Count];
            for (int i = 0; i < uv3D.Length; i++)
            {
                var uv = uvs[i];
                uv3D[i] = new Vector3(uv.x, uv.y, uv.z);
            }
            return uv3D;
        }

        /// <summary>
        /// Returns the minimum and maximum indices for each submesh along with the needed index format.
        /// </summary>
        /// <param name="indices">The indices for the submeshes.</param>
        /// <param name="indexFormat">The output index format.</param>
        /// <returns>The minimum and maximum indices for each submesh.</returns>
        public static Vector2Int[] GetSubMeshIndexMinMax(int[][] indices, out IndexFormat indexFormat)
        {
            if (indices == null)
                throw new ArgumentNullException(nameof(indices));

            var result = new Vector2Int[indices.Length];
            indexFormat = IndexFormat.UInt16;
            for (int subMeshIndex = 0; subMeshIndex < indices.Length; subMeshIndex++)
            {
                int minIndex, maxIndex;
                GetIndexMinMax(indices[subMeshIndex], out minIndex, out maxIndex);
                result[subMeshIndex] = new Vector2Int(minIndex, maxIndex);

                int indexRange = (maxIndex - minIndex);
                if (indexRange > ushort.MaxValue)
                {
                    indexFormat = IndexFormat.UInt32;
                }
            }
            return result;
        }
        #endregion

        #region Private Methods
        private static void GetIndexMinMax(int[] indices, out int minIndex, out int maxIndex)
        {
            if (indices == null || indices.Length == 0)
            {
                minIndex = maxIndex = 0;
                return;
            }

            minIndex = int.MaxValue;
            maxIndex = int.MinValue;

            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < minIndex)
                {
                    minIndex = indices[i];
                }
                if (indices[i] > maxIndex)
                {
                    maxIndex = indices[i];
                }
            }
        }
        #endregion
    }
}
