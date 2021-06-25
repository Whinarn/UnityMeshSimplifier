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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using NUnit.Framework;

namespace UnityMeshSimplifier.Editor.Tests
{
    public class MeshUtilsTest
    {
        [Test]
        public void ShouldApplyBlendShapes()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[2];

            var frameWeights = new float[]
            {
                0.2f,
                0.48f
            };
            var deltaVertices = new Vector3[][]
            {
                new Vector3[] {
                    new Vector3(15f, 13.7f, 8889.91f),
                    new Vector3(761.717f, -76.225f, 889.1f)
                },
                new Vector3[]
                {
                    new Vector3(-15f, 13.7f, -8889.91f),
                    new Vector3(761.717f, 76.225f, -889.1f)
                }
            };
            var deltaNormals = new Vector3[][]
            {
                new Vector3[]
                {
                    deltaVertices[0][0].normalized,
                    deltaVertices[0][1].normalized
                },
                new Vector3[]
                {
                    deltaVertices[1][0].normalized,
                    deltaVertices[1][1].normalized
                }
            };
            var deltaTangents = new Vector3[][]
            {
                new Vector3[]
                {
                    Vector3.Cross(deltaNormals[0][0], Vector3.forward).normalized,
                    Vector3.Cross(deltaNormals[0][1], Vector3.forward).normalized,
                },
                new Vector3[]
                {
                    Vector3.Cross(deltaNormals[1][0], Vector3.forward).normalized,
                    Vector3.Cross(deltaNormals[1][1], Vector3.forward).normalized
                }
            };

            var blendShape = new BlendShape
            {
                ShapeName = "test",
                Frames = new BlendShapeFrame[]
                {
                    new BlendShapeFrame(frameWeights[0], deltaVertices[0], deltaNormals[0], deltaTangents[0]),
                    new BlendShapeFrame(frameWeights[1], deltaVertices[1], deltaNormals[1], deltaTangents[1])
                }
            };
            var blendShapes = new BlendShape[]
            {
                blendShape
            };

            MeshUtils.ApplyMeshBlendShapes(mesh, blendShapes);

            Assert.AreEqual(1, mesh.blendShapeCount);
            Assert.AreEqual(blendShape.ShapeName, mesh.GetBlendShapeName(0));
            Assert.AreEqual(blendShape.Frames.Length, mesh.GetBlendShapeFrameCount(0));

            for (int i = 0; i < blendShape.Frames.Length; i++)
            {
                var frame = blendShape.Frames[i];
                Assert.AreEqual(frame.FrameWeight, mesh.GetBlendShapeFrameWeight(0, i));

                var resultVertices = new Vector3[2];
                var resultNormals = new Vector3[2];
                var resultTangents = new Vector3[2];
                mesh.GetBlendShapeFrameVertices(0, i, resultVertices, resultNormals, resultTangents);
                Assert.AreEqual(frame.DeltaVertices, resultVertices);
                Assert.AreEqual(frame.DeltaNormals, resultNormals);
                Assert.AreEqual(frame.DeltaTangents, resultTangents);
            }
        }

        [Test]
        public void ShouldConvertUVsTo2D()
        {
            var uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 1f, 1f),
                new Vector4(1f, 1f, 1f, 1f),
                new Vector4(0f, 1f, 0f, 1f)
            };
            var expectedUVs = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            var convertedUVs = MeshUtils.ConvertUVsTo2D(uvs);
            Assert.AreEqual(expectedUVs, convertedUVs);
        }

        [Test]
        public void ShouldConvertUVsTo3D()
        {
            var uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 1f, 1f),
                new Vector4(1f, 1f, 1f, 1f),
                new Vector4(0f, 1f, 0f, 1f)
            };
            var expectedUVs = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 1f),
                new Vector3(1f, 1f, 1f),
                new Vector3(0f, 1f, 0f)
            };
            var convertedUVs = MeshUtils.ConvertUVsTo3D(uvs);
            Assert.AreEqual(expectedUVs, convertedUVs);
        }

        [Test]
        public void ShouldCreateMesh()
        {
            var vertices = new Vector3[]
            {
                new Vector3(15f, 13.7f, 8889.91f),
                new Vector3(761.717f, -76.225f, 889.1f),
                new Vector3(-15f, 13.7f, -8889.91f),
                new Vector3(761.717f, 76.225f, -889.1f)
            };
            var indices = new int[][]
            {
                new int[] { 0, 1, 2, 3, 1, 0 },
                new int[] { 3, 1, 0, 2, 1, 0 }
            };
            var normals = new Vector3[]
            {
                vertices[0].normalized,
                vertices[1].normalized,
                vertices[2].normalized,
                vertices[3].normalized
            };
            var tangents = new Vector4[]
            {
                Vector3.Cross(normals[0], Vector3.forward).normalized,
                Vector3.Cross(normals[1], Vector3.forward).normalized,
                Vector3.Cross(normals[2], Vector3.forward).normalized,
                Vector3.Cross(normals[3], Vector3.forward).normalized
            };
            var colors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.yellow,
                Color.magenta
            };
            var boneWeights = new BoneWeight[]
            {
                new BoneWeight
                {
                    boneIndex0 = 0,
                    boneIndex1 = 1,
                    boneIndex2 = 2,
                    boneIndex3 = 3,
                    weight0 = 0.1f,
                    weight1 = 0.2f,
                    weight2 = 0.3f,
                    weight3 = 0.4f
                },
                new BoneWeight
                {
                    boneIndex0 = 0,
                    boneIndex1 = 1,
                    boneIndex2 = 2,
                    boneIndex3 = 3,
                    weight0 = 0.5f,
                    weight1 = 0.6f,
                    weight2 = 0.7f,
                    weight3 = 0.8f
                },
                new BoneWeight
                {
                    boneIndex0 = 3,
                    boneIndex1 = 2,
                    boneIndex2 = 1,
                    boneIndex3 = 0,
                    weight0 = 0.1f,
                    weight1 = 0.2f,
                    weight2 = 0.3f,
                    weight3 = 0.4f
                },
                new BoneWeight
                {
                    boneIndex0 = 3,
                    boneIndex1 = 2,
                    boneIndex2 = 1,
                    boneIndex3 = 0,
                    weight0 = 0.5f,
                    weight1 = 0.6f,
                    weight2 = 0.7f,
                    weight3 = 0.8f
                }
            };
            var uvs2D = new List<Vector2>[]
            {
                new List<Vector2> { new Vector2(0f, 1f), new Vector2(0.1f, 1.1f), new Vector2(0.2f, 1.2f), new Vector2(0.3f, 1.3f) },
                null,
                null,
                new List<Vector2> { new Vector2(1.1f, 2.1f), new Vector2(1.2f, 2.2f), new Vector2(1.3f, 2.3f), new Vector2(1.4f, 2.4f) }
            };
            var uvs3D = new List<Vector3>[]
            {
                null,
                new List<Vector3> { new Vector3(0f, 1f, 2f), new Vector3(0.1f, 1.1f, 2.1f), new Vector3(0.2f, 1.2f, 2.2f), new Vector3(0.3f, 1.3f, 2.3f) },
                null,
                null
            };
            var uvs4D = new List<Vector4>[]
            {
                null,
                null,
                new List<Vector4> { new Vector4(0f, 1f, 2f, 3f), new Vector4(0.1f, 1.1f, 2.1f, 3.1f), new Vector4(0.2f, 1.2f, 2.2f, 3.2f), new Vector4(0.3f, 1.3f, 2.3f, 3.3f) },
                null
            };
            var bindposes = new Matrix4x4[]
            {
                Matrix4x4.identity,
                Matrix4x4.Rotate(Quaternion.Euler(13.4f, 91.1f, -17.9f)),
                Matrix4x4.Rotate(Quaternion.Euler(-74.9f, 2.817f, 41.99f)),
                Matrix4x4.Rotate(Quaternion.Euler(14.55f, 23.69f, -73.82f))
            };
            var blendShapes = new BlendShape[0];

            var mesh = MeshUtils.CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, uvs2D, uvs3D, uvs4D, bindposes, blendShapes);
            Assert.IsNotNull(mesh);
            Assert.AreEqual(vertices, mesh.vertices);

            for (int i = 0; i < indices.Length; i++)
            {
                Assert.AreEqual(indices[i], mesh.GetIndices(i));
            }

            Assert.AreEqual(normals, mesh.normals);
            Assert.AreEqual(tangents, mesh.tangents);
            Assert.AreEqual(colors, mesh.colors);
            Assert.AreEqual(boneWeights, mesh.boneWeights);

            var resultUV2D = new List<Vector2>();
            mesh.GetUVs(0, resultUV2D);
            Assert.AreEqual(uvs2D[0], resultUV2D.ToArray());

            var resultUV3D = new List<Vector3>();
            mesh.GetUVs(1, resultUV3D);
            Assert.AreEqual(uvs3D[1], resultUV3D.ToArray());

            var resultUV4D = new List<Vector4>();
            mesh.GetUVs(2, resultUV4D);
            Assert.AreEqual(uvs4D[2], resultUV4D.ToArray());

            resultUV2D.Clear();
            mesh.GetUVs(3, resultUV2D);
            Assert.AreEqual(uvs2D[3], resultUV2D.ToArray());

            Assert.AreEqual(bindposes, mesh.bindposes);
        }

        [Test]
        public void ShouldGetMeshBlendShapes()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[2];

            string shapeName = "test";
            var frameWeights = new float[]
            {
                0.2f,
                0.48f
            };
            var deltaVertices = new Vector3[][]
            {
                new Vector3[] {
                    new Vector3(15f, 13.7f, 8889.91f),
                    new Vector3(761.717f, -76.225f, 889.1f)
                },
                new Vector3[]
                {
                    new Vector3(-15f, 13.7f, -8889.91f),
                    new Vector3(761.717f, 76.225f, -889.1f)
                }
            };
            var deltaNormals = new Vector3[][]
            {
                new Vector3[]
                {
                    deltaVertices[0][0].normalized,
                    deltaVertices[0][1].normalized
                },
                new Vector3[]
                {
                    deltaVertices[1][0].normalized,
                    deltaVertices[1][1].normalized
                }
            };
            var deltaTangents = new Vector3[][]
            {
                new Vector3[]
                {
                    Vector3.Cross(deltaNormals[0][0], Vector3.forward).normalized,
                    Vector3.Cross(deltaNormals[0][1], Vector3.forward).normalized,
                },
                new Vector3[]
                {
                    Vector3.Cross(deltaNormals[1][0], Vector3.forward).normalized,
                    Vector3.Cross(deltaNormals[1][1], Vector3.forward).normalized
                }
            };

            mesh.AddBlendShapeFrame(shapeName, frameWeights[0], deltaVertices[0], deltaNormals[0], deltaTangents[0]);
            mesh.AddBlendShapeFrame(shapeName, frameWeights[1], deltaVertices[1], deltaNormals[1], deltaTangents[1]);

            var blendShapes = MeshUtils.GetMeshBlendShapes(mesh);
            Assert.IsNotNull(blendShapes);
            Assert.AreEqual(1, blendShapes.Length);

            var blendShape = blendShapes[0];
            Assert.IsNotNull(blendShape);
            Assert.AreEqual(shapeName, blendShape.ShapeName);
            Assert.IsNotNull(blendShape.Frames);
            Assert.AreEqual(2, blendShape.Frames.Length);

            for (int i = 0; i < blendShape.Frames.Length; i++)
            {
                var frame = blendShape.Frames[i];
                Assert.AreEqual(frameWeights[i], frame.FrameWeight);
                Assert.AreEqual(deltaVertices[i], frame.DeltaVertices);
                Assert.AreEqual(deltaNormals[i], frame.DeltaNormals);
                Assert.AreEqual(deltaTangents[i], frame.DeltaTangents);
            }
        }

        [Test]
        public void ShouldGetMeshUVs()
        {
            var uvs = new Vector4[][]
            {
                new Vector4[]
                {
                    new Vector4(0f, 0f, 0f, 0f),
                    new Vector4(1f, 0f, 1f, 1f),
                    new Vector4(1f, 1f, 1f, 1f),
                    new Vector4(0f, 1f, 0f, 1f)
                },
                new Vector4[]
                {
                    new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
                    new Vector4(1.5f, 0.5f, 1.5f, 1.5f),
                    new Vector4(1.5f, 1.5f, 1.5f, 1.5f),
                    new Vector4(0.5f, 1.5f, 0.5f, 1.5f)
                }
            };
            var mesh = new Mesh();
            mesh.vertices = new Vector3[4];
            for (int i = 0; i < uvs.Length; i++)
            {
#if UNITY_2019_3_OR_NEWER
                mesh.SetUVs(i, uvs[i]);

#else
                mesh.SetUVs(i, uvs[i].ToList());
#endif
            }

            var allUVs = MeshUtils.GetMeshUVs(mesh);
            Assert.AreEqual(uvs, allUVs.Take(2).Select(x => x.ToArray()).ToArray());

            for (int i = 0; i < uvs.Length; i++)
            {
                var channelUVs = MeshUtils.GetMeshUVs(mesh, i);
                Assert.AreEqual(uvs[i], channelUVs.ToArray());
            }
        }

        [Test]
        public void ShouldGetSubMeshIndexMinMax()
        {
            IndexFormat indexFormat;
            var indices = new int[][]
            {
                new int[]
                {
                    0,
                    1,
                    2,
                    5,
                    9,
                    3,
                    4,
                    7,
                    6
                },
                new int[]
                {
                    9999,
                    500,
                    6000,
                    4716,
                    1818,
                    1177
                }
            };
            var expectedMinMaxIndices = new Vector2Int[]
            {
                new Vector2Int(0, 9),
                new Vector2Int(500, 9999)
            };
            var minMaxIndices = MeshUtils.GetSubMeshIndexMinMax(indices, out indexFormat);
            Assert.AreEqual(expectedMinMaxIndices, minMaxIndices);
            Assert.AreEqual(IndexFormat.UInt16, indexFormat);

            indices = new int[][]
            {
                new int[]
                {
                    ushort.MaxValue + 1,
                    ushort.MaxValue / 2,
                    0
                },
                new int[]
                {
                    1,
                    ushort.MaxValue / 2,
                    0
                }
            };
            expectedMinMaxIndices = new Vector2Int[]
            {
                new Vector2Int(0, ushort.MaxValue + 1),
                new Vector2Int(0, ushort.MaxValue / 2)
            };
            minMaxIndices = MeshUtils.GetSubMeshIndexMinMax(indices, out indexFormat);
            Assert.AreEqual(expectedMinMaxIndices, minMaxIndices);
            Assert.AreEqual(IndexFormat.UInt32, indexFormat);
        }

        [Test]
        public void ShouldGetUsedUVComponents()
        {
            var uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 1f, 1f),
                new Vector4(1f, 1f, 1f, 1f),
                new Vector4(0f, 1f, 0f, 1f)
            };
            int usedComponents = MeshUtils.GetUsedUVComponents(uvs);
            Assert.AreEqual(4, usedComponents);

            uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 1f, 0f),
                new Vector4(1f, 1f, 1f, 0f),
                new Vector4(0f, 1f, 0f, 0f)
            };
            usedComponents = MeshUtils.GetUsedUVComponents(uvs);
            Assert.AreEqual(3, usedComponents);

            uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(1f, 1f, 0f, 0f),
                new Vector4(0f, 1f, 0f, 0f)
            };
            usedComponents = MeshUtils.GetUsedUVComponents(uvs);
            Assert.AreEqual(2, usedComponents);

            uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(0f, 0f, 0f, 0f)
            };
            usedComponents = MeshUtils.GetUsedUVComponents(uvs);
            Assert.AreEqual(1, usedComponents);

            uvs = new List<Vector4>
            {
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(0f, 0f, 0f, 0f),
                new Vector4(0f, 0f, 0f, 0f)
            };
            usedComponents = MeshUtils.GetUsedUVComponents(uvs);
            Assert.AreEqual(0, usedComponents);
        }
    }
}
