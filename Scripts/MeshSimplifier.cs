#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

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

#region Original License
/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// The mesh simplifier.
    /// Deeply based on https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification but rewritten completely in C#.
    /// </summary>
    public sealed class MeshSimplifier
    {
        #region Consts
        private const double DoubleEpsilon = 1.0E-3;
        private const int UVChannelCount = 4;
        #endregion

        #region Classes
        #region Triangle
        private struct Triangle
        {
            #region Fields
            public int v0;
            public int v1;
            public int v2;
            public int subMeshIndex;

            public int va0;
            public int va1;
            public int va2;

            public double area;

            public double err0;
            public double err1;
            public double err2;
            public double err3;

            public bool deleted;
            public bool dirty;
            public Vector3d n;
            #endregion

            #region Properties
            public int this[int index]
            {
                get
                {
                    return (index == 0 ? v0 : (index == 1 ? v1 : v2));
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            v0 = value;
                            break;
                        case 1:
                            v1 = value;
                            break;
                        case 2:
                            v2 = value;
                            break;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
            }
            #endregion

            #region Constructor
            public Triangle(int v0, int v1, int v2, int subMeshIndex)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                this.subMeshIndex = subMeshIndex;

                this.va0 = v0;
                this.va1 = v1;
                this.va2 = v2;

                area = 0;
                err0 = err1 = err2 = err3 = 0;
                deleted = dirty = false;
                n = new Vector3d();
            }
            #endregion

            #region Public Methods
            public void GetAttributeIndices(int[] attributeIndices)
            {
                attributeIndices[0] = va0;
                attributeIndices[1] = va1;
                attributeIndices[2] = va2;
            }

            public void SetAttributeIndex(int index, int value)
            {
                switch (index)
                {
                    case 0:
                        va0 = value;
                        break;
                    case 1:
                        va1 = value;
                        break;
                    case 2:
                        va2 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            public void GetErrors(double[] err)
            {
                err[0] = err0;
                err[1] = err1;
                err[2] = err2;
            }
            #endregion
        }
        #endregion

        #region Vertex
        private struct Vertex
        {
            public Vector3d p;
            public int tstart;
            public int tcount;
            public SymmetricMatrix q;
            public bool border;
            public bool seam;
            public bool foldover;

            public Vertex(Vector3d p)
            {
                this.p = p;
                this.tstart = 0;
                this.tcount = 0;
                this.q = new SymmetricMatrix();
                this.border = true;
                this.seam = false;
                this.foldover = false;
            }
        }
        #endregion

        #region Ref
        private struct Ref
        {
            public int tid;
            public int tvertex;

            public void Set(int tid, int tvertex)
            {
                this.tid = tid;
                this.tvertex = tvertex;
            }
        }
        #endregion

        #region UV Channels
        private class UVChannels<TVec>
        {
            private ResizableArray<TVec>[] channels = null;
            private TVec[][] channelsData = null;

            public TVec[][] Data
            {
                get
                {
                    for (int i = 0; i < UVChannelCount; i++)
                    {
                        if (channels[i] != null)
                        {
                            channelsData[i] = channels[i].Data;
                        }
                        else
                        {
                            channelsData[i] = null;
                        }
                    }
                    return channelsData;
                }
            }

            /// <summary>
            /// Gets or sets a specific channel by index.
            /// </summary>
            /// <param name="index">The channel index.</param>
            public ResizableArray<TVec> this[int index]
            {
                get { return channels[index]; }
                set { channels[index] = value; }
            }

            public UVChannels()
            {
                channels = new ResizableArray<TVec>[UVChannelCount];
                channelsData = new TVec[UVChannelCount][];
            }

            /// <summary>
            /// Resizes all channels at once.
            /// </summary>
            /// <param name="capacity">The new capacity.</param>
            /// <param name="trimExess">If exess memory should be trimmed.</param>
            public void Resize(int capacity, bool trimExess = false)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (channels[i] != null)
                    {
                        channels[i].Resize(capacity, trimExess);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Fields
        private bool preserveBorders = false;
        private bool preserveSeams = false;
        private bool preserveFoldovers = false;
        private bool enableSmartLink = true;
        private double agressiveness = 7.0;
        private bool verbose = false;

        private double vertexLinkDistanceSqr = double.Epsilon;

        private int subMeshCount = 0;
        private ResizableArray<Triangle> triangles = null;
        private ResizableArray<Vertex> vertices = null;
        private ResizableArray<Ref> refs = null;

        private ResizableArray<Vector3> vertNormals = null;
        private ResizableArray<Vector4> vertTangents = null;
        private UVChannels<Vector2> vertUV2D = null;
        private UVChannels<Vector3> vertUV3D = null;
        private UVChannels<Vector4> vertUV4D = null;
        private ResizableArray<Color> vertColors = null;
        private ResizableArray<BoneWeight> vertBoneWeights = null;

        // Pre-allocated buffers
        private double[] errArr = new double[3];
        private int[] attributeIndexArr = new int[3];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if borders should be preserved.
        /// </summary>
        [Obsolete("Use the 'MeshSimplifier.PreserveBorders' property instead.", false)]
        public bool KeepBorders
        {
            get { return preserveBorders; }
            set { preserveBorders = value; }
        }

        /// <summary>
        /// Gets or sets if borders should be preserved.
        /// </summary>
        public bool PreserveBorders
        {
            get { return preserveBorders; }
            set { preserveBorders = value; }
        }

        /// <summary>
        /// Gets or sets if seams should be preserved.
        /// </summary>
        public bool PreserveSeams
        {
            get { return preserveSeams; }
            set { preserveSeams = value; }
        }

        /// <summary>
        /// Gets or sets if foldovers should be preserved.
        /// </summary>
        public bool PreserveFoldovers
        {
            get { return preserveFoldovers; }
            set { preserveFoldovers = value; }
        }

        /// <summary>
        /// Gets or sets if a feature for smarter vertex linking should be enabled, reducing artifacts in the
        /// decimated result at the cost of a slightly more expensive initialization by treating vertices at
        /// the same position as the same vertex while separating the attributes.
        /// </summary>
        public bool EnableSmartLink
        {
            get { return enableSmartLink; }
            set { enableSmartLink = value; }
        }

        /// <summary>
        /// Gets or sets the agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.
        /// </summary>
        public double Agressiveness
        {
            get { return agressiveness; }
            set { agressiveness = value; }
        }

        /// <summary>
        /// Gets or sets if verbose information should be printed to the console.
        /// </summary>
        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }

        /// <summary>
        /// Gets or sets the maximum squared distance between two vertices in order to link them.
        /// Note that this value is only used if PreventHoles is true.
        /// </summary>
        public double VertexLinkDistanceSqr
        {
            get { return vertexLinkDistanceSqr; }
            set { vertexLinkDistanceSqr = value; }
        }

        /// <summary>
        /// Gets or sets the vertex positions.
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                int vertexCount = this.vertices.Length;
                var vertices = new Vector3[vertexCount];
                var vertArr = this.vertices.Data;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = (Vector3)vertArr[i].p;
                }
                return vertices;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                vertices.Resize(value.Length);
                var vertArr = vertices.Data;
                for (int i = 0; i < value.Length; i++)
                {
                    vertArr[i] = new Vertex(value[i]);
                }
            }
        }

        /// <summary>
        /// Gets the count of sub-meshes.
        /// </summary>
        public int SubMeshCount
        {
            get { return subMeshCount; }
        }

        /// <summary>
        /// Gets or sets the vertex normals.
        /// </summary>
        public Vector3[] Normals
        {
            get { return (vertNormals != null ? vertNormals.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertNormals, "normals");
            }
        }

        /// <summary>
        /// Gets or sets the vertex tangents.
        /// </summary>
        public Vector4[] Tangents
        {
            get { return (vertTangents != null ? vertTangents.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertTangents, "tangents");
            }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 1.
        /// </summary>
        public Vector2[] UV1
        {
            get { return GetUVs2D(0); }
            set { SetUVs(0, value); }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 2.
        /// </summary>
        public Vector2[] UV2
        {
            get { return GetUVs2D(1); }
            set { SetUVs(1, value); }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 3.
        /// </summary>
        public Vector2[] UV3
        {
            get { return GetUVs2D(2); }
            set { SetUVs(2, value); }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 4.
        /// </summary>
        public Vector2[] UV4
        {
            get { return GetUVs2D(3); }
            set { SetUVs(3, value); }
        }

        /// <summary>
        /// Gets or sets the vertex colors.
        /// </summary>
        public Color[] Colors
        {
            get { return (vertColors != null ? vertColors.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertColors, "colors");
            }
        }

        /// <summary>
        /// Gets or sets the vertex bone weights.
        /// </summary>
        public BoneWeight[] BoneWeights
        {
            get { return (vertBoneWeights != null ? vertBoneWeights.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertBoneWeights, "boneWeights");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        public MeshSimplifier()
        {
            triangles = new ResizableArray<Triangle>(0);
            vertices = new ResizableArray<Vertex>(0);
            refs = new ResizableArray<Ref>(0);
        }

        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        /// <param name="mesh">The original mesh to simplify.</param>
        public MeshSimplifier(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            triangles = new ResizableArray<Triangle>(0);
            vertices = new ResizableArray<Vertex>(0);
            refs = new ResizableArray<Ref>(0);

            Initialize(mesh);
        }
        #endregion

        #region Private Methods
        #region Initialize Vertex Attribute
        private void InitializeVertexAttribute<T>(T[] attributeValues, ref ResizableArray<T> attributeArray, string attributeName)
        {
            if (attributeValues != null && attributeValues.Length == vertices.Length)
            {
                if (attributeArray == null)
                {
                    attributeArray = new ResizableArray<T>(attributeValues.Length, attributeValues.Length);
                }
                else
                {
                    attributeArray.Resize(attributeValues.Length);
                }

                var arrayData = attributeArray.Data;
                Array.Copy(attributeValues, 0, arrayData, 0, attributeValues.Length);
            }
            else
            {
                if (attributeValues != null && attributeValues.Length > 0)
                {
                    Debug.LogErrorFormat("Failed to set vertex attribute '{0}' with {1} length of array, when {2} was needed.", attributeName, attributeValues.Length, vertices.Length);
                }
                attributeArray = null;
            }
        }
        #endregion

        #region Calculate Error
        private double VertexError(ref SymmetricMatrix q, double x, double y, double z)
        {
            return q.m0 * x * x + 2 * q.m1 * x * y + 2 * q.m2 * x * z + 2 * q.m3 * x + q.m4 * y * y
                + 2 * q.m5 * y * z + 2 * q.m6 * y + q.m7 * z * z + 2 * q.m8 * z + q.m9;
        }

        private double CalculateError(int i0, int i1, out Vector3d result, out int resultIndex)
        {
            // compute interpolated vertex
            var vertices = this.vertices.Data;
            Vertex v0 = vertices[i0];
            Vertex v1 = vertices[i1];
            SymmetricMatrix q = v0.q + v1.q;
            bool border = (v0.border & v1.border);
            double error = 0.0;
            double det = q.Determinant1();
            if (det != 0.0 && !border)
            {
                // q_delta is invertible
                result = new Vector3d(
                    -1.0 / det * q.Determinant2(),  // vx = A41/det(q_delta)
                    1.0 / det * q.Determinant3(),   // vy = A42/det(q_delta)
                    -1.0 / det * q.Determinant4()); // vz = A43/det(q_delta)
                error = VertexError(ref q, result.x, result.y, result.z);
                resultIndex = 2;
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3d p1 = v0.p;
                Vector3d p2 = v1.p;
                Vector3d p3 = (p1 + p2) * 0.5f;
                double error1 = VertexError(ref q, p1.x, p1.y, p1.z);
                double error2 = VertexError(ref q, p2.x, p2.y, p2.z);
                double error3 = VertexError(ref q, p3.x, p3.y, p3.z);
                error = MathHelper.Min(error1, error2, error3);
                if (error == error3)
                {
                    result = p3;
                    resultIndex = 2;
                }
                else if (error == error2)
                {
                    result = p2;
                    resultIndex = 1;
                }
                else if (error == error1)
                {
                    result = p1;
                    resultIndex = 0;
                }
                else
                {
                    result = p3;
                    resultIndex = 2;
                }
            }
            return error;
        }
        #endregion

        #region Flipped
        /// <summary>
        /// Check if a triangle flips when this edge is removed
        /// </summary>
        private bool Flipped(Vector3d p, int i0, int i1, ref Vertex v0, ResizableArray<bool> deleted)
        {
            int tcount = v0.tcount;
            var refs = this.refs.Data;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v0.tstart + k];
                Triangle t = triangles[r.tid];
                if (t.deleted)
                    continue;

                int s = r.tvertex;
                int id1 = t[(s + 1) % 3];
                int id2 = t[(s + 2) % 3];
                if (id1 == i1 || id2 == i1)
                {
                    deleted[k] = true;
                    continue;
                }

                Vector3d d1 = vertices[id1].p - p;
                d1.Normalize();
                Vector3d d2 = vertices[id2].p - p;
                d2.Normalize();
                double dot = Vector3d.Dot(ref d1, ref d2);
                if (System.Math.Abs(dot) > 0.999)
                    return true;

                Vector3d n;
                Vector3d.Cross(ref d1, ref d2, out n);
                n.Normalize();
                deleted[k] = false;
                dot = Vector3d.Dot(ref n, ref t.n);
                if (dot < 0.2)
                    return true;
            }

            return false;
        }
        #endregion

        #region Calculate Area
        private double CalculateArea(int i0, int i1, int i2)
        {
            var vertices = this.vertices.Data;
            return MathHelper.TriangleArea(ref vertices[i0].p, ref vertices[i1].p, ref vertices[i2].p);
        }
        #endregion

        #region Update Triangles
        /// <summary>
        /// Update triangle connections and edge error after a edge is collapsed.
        /// </summary>
        private void UpdateTriangles(int i0, int ia0, ref Vertex v, ResizableArray<bool> deleted, ref int deletedTriangles)
        {
            Vector3d p;
            int pIndex;
            int tcount = v.tcount;
            var triangles = this.triangles.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v.tstart + k];
                int tid = r.tid;
                Triangle t = triangles[tid];
                if (t.deleted)
                    continue;

                if (deleted[k])
                {
                    triangles[tid].deleted = true;
                    ++deletedTriangles;
                    continue;
                }

                t[r.tvertex] = i0;
                if (ia0 != -1)
                {
                    t.SetAttributeIndex(r.tvertex, ia0);
                }

                t.dirty = true;
                //t.area = CalculateArea(t.v0, t.v1, t.v2);
                t.err0 = CalculateError(t.v0, t.v1, out p, out pIndex);
                t.err1 = CalculateError(t.v1, t.v2, out p, out pIndex);
                t.err2 = CalculateError(t.v2, t.v0, out p, out pIndex);
                t.err3 = MathHelper.Min(t.err0, t.err1, t.err2);
                triangles[tid] = t;
                refs.Add(r);
            }
        }
        #endregion

        #region Move/Merge Vertex Attributes
        private void MoveVertexAttributes(int i0, int i1)
        {
            if (vertNormals != null)
            {
                vertNormals[i0] = vertNormals[i1];
            }
            if (vertTangents != null)
            {
                vertTangents[i0] = vertTangents[i1];
            }
            if (vertUV2D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV2D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = vertUV[i1];
                    }
                }
            }
            if (vertUV3D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV3D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = vertUV[i1];
                    }
                }
            }
            if (vertUV4D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV4D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = vertUV[i1];
                    }
                }
            }
            if (vertColors != null)
            {
                vertColors[i0] = vertColors[i1];
            }
        }

        private void MergeVertexAttributes(int i0, int i1)
        {
            if (vertNormals != null)
            {
                vertNormals[i0] = (vertNormals[i0] + vertNormals[i1]) * 0.5f;
            }
            if (vertTangents != null)
            {
                vertTangents[i0] = (vertTangents[i0] + vertTangents[i1]) * 0.5f;
            }
            if (vertUV2D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV2D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = (vertUV[i0] + vertUV[i1]) * 0.5f;
                    }
                }
            }
            if (vertUV3D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV3D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = (vertUV[i0] + vertUV[i1]) * 0.5f;
                    }
                }
            }
            if (vertUV4D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV4D[i];
                    if (vertUV != null)
                    {
                        vertUV[i0] = (vertUV[i0] + vertUV[i1]) * 0.5f;
                    }
                }
            }
            if (vertColors != null)
            {
                vertColors[i0] = (vertColors[i0] + vertColors[i1]) * 0.5f;
            }
        }
        #endregion

        #region Are UVs The Same
        private bool AreUVsTheSame(int channel, int indexA, int indexB)
        {
            if (vertUV2D != null)
            {
                var vertUV = vertUV2D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV3D != null)
            {
                var vertUV = vertUV3D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV4D != null)
            {
                var vertUV = vertUV4D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            return false;
        }
        #endregion

        #region Remove Vertex Pass
        /// <summary>
        /// Remove vertices and mark deleted triangles
        /// </summary>
        private void RemoveVertexPass(int startTrisCount, int targetTrisCount, double threshold, ResizableArray<bool> deleted0, ResizableArray<bool> deleted1, ref int deletedTris)
        {
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            var vertices = this.vertices.Data;

            Vertex v0, v1;
            Vector3d p;
            int pIndex;
            for (int i = 0; i < triangleCount; i++)
            {
                var t = triangles[i];
                if (t.dirty || t.deleted || t.err3 > threshold)
                    continue;

                t.GetErrors(errArr);
                t.GetAttributeIndices(attributeIndexArr);
                for (int j = 0; j < 3; j++)
                {
                    if (errArr[j] > threshold)
                        continue;

                    int k = ((j + 1) % 3);
                    int i0 = t[j];
                    int i1 = t[k];
                    v0 = vertices[i0];
                    v1 = vertices[i1];

                    // Border check
                    if (v0.border != v1.border)
                        continue;
                    // Seam check
                    else if (v0.seam != v1.seam)
                        continue;
                    // Foldover check
                    else if (v0.foldover != v1.foldover)
                        continue;
                    // If borders should be preserved
                    else if (preserveBorders && v0.border)
                        continue;
                    // If seams should be preserved
                    else if (preserveSeams && v0.seam)
                        continue;
                    // If foldovers should be preserved
                    else if (preserveFoldovers && v0.foldover)
                        continue;

                    // Compute vertex to collapse to
                    CalculateError(i0, i1, out p, out pIndex);
                    deleted0.Resize(v0.tcount); // normals temporarily
                    deleted1.Resize(v1.tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(p, i0, i1, ref v0, deleted0))
                        continue;
                    if (Flipped(p, i1, i0, ref v1, deleted1))
                        continue;

                    int ia0 = attributeIndexArr[j];

                    // Not flipped, so remove edge
                    v0.p = p;
                    v0.q += v1.q;
                    vertices[i0] = v0;

                    if (pIndex == 1)
                    {
                        // Move vertex attributes from ia1 to ia0
                        int ia1 = attributeIndexArr[k];
                        MoveVertexAttributes(ia0, ia1);
                    }
                    else if (pIndex == 2)
                    {
                        // Merge vertex attributes ia0 and ia1 into ia0
                        int ia1 = attributeIndexArr[k];
                        MergeVertexAttributes(ia0, ia1);
                    }

                    if (v0.seam)
                    {
                        ia0 = -1;
                    }

                    int tstart = refs.Length;
                    UpdateTriangles(i0, ia0, ref v0, deleted0, ref deletedTris);
                    UpdateTriangles(i0, ia0, ref v1, deleted1, ref deletedTris);

                    int tcount = refs.Length - tstart;
                    if (tcount <= v0.tcount)
                    {
                        // save ram
                        if (tcount > 0)
                        {
                            var refsArr = refs.Data;
                            Array.Copy(refsArr, tstart, refsArr, v0.tstart, tcount);
                        }
                    }
                    else
                    {
                        // append
                        vertices[i0].tstart = tstart;
                    }

                    vertices[i0].tcount = tcount;
                    break;
                }

                // Check if we are already done
                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;
            }
        }
        #endregion

        #region Update Mesh
        /// <summary>
        /// Compact triangles, compute edge error and build reference list.
        /// </summary>
        /// <param name="iteration">The iteration index.</param>
        private void UpdateMesh(int iteration)
        {
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;

            int triangleCount = this.triangles.Length;
            int vertexCount = this.vertices.Length;
            if (iteration > 0) // compact triangles
            {
                int dst = 0;
                for (int i = 0; i < triangleCount; i++)
                {
                    var triangle = triangles[i];
                    if (!triangle.deleted)
                    {
                        if (dst != i)
                        {
                            triangles[dst] = triangle;
                        }
                        dst++;
                    }
                }
                this.triangles.Resize(dst);
                triangles = this.triangles.Data;
                triangleCount = dst;
            }

            UpdateReferences();

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                var refs = this.refs.Data;

                var vcount = new List<int>(8);
                var vids = new List<int>(8);
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].border = false;
                    vertices[i].seam = false;
                    vertices[i].foldover = false;
                }

                int ofs;
                int id;
                int borderVertexCount = 0;
                for (int i = 0; i < vertexCount; i++)
                {
                    var vertex = vertices[i];
                    vcount.Clear();
                    vids.Clear();

                    int tcount = vertex.tcount;
                    for (int j = 0; j < tcount; j++)
                    {
                        int k = refs[vertex.tstart + j].tid;
                        Triangle t = triangles[k];
                        for (k = 0; k < 3; k++)
                        {
                            ofs = 0;
                            id = t[k];
                            while (ofs < vcount.Count)
                            {
                                if (vids[ofs] == id)
                                    break;

                                ++ofs;
                            }

                            if (ofs == vcount.Count)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                            }
                            else
                            {
                                ++vcount[ofs];
                            }
                        }
                    }

                    int vcountCount = vcount.Count;
                    for (int j = 0; j < vcountCount; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            id = vids[j];
                            vertices[id].border = true;
                            ++borderVertexCount;
                        }
                    }
                }

                if (enableSmartLink)
                {
                    // First find all border vertices
                    var borderIndices = new int[borderVertexCount];
                    int borderIndexCount = 0;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var v0 = vertices[i];
                        if (!v0.border)
                            continue;

                        borderIndices[borderIndexCount++] = i;
                    }

                    // Then find identical border vertices and bind them together as one
                    for (int i = 0; i < borderIndexCount; i++)
                    {
                        var myIndex = borderIndices[i];
                        if (myIndex == -1)
                            continue;

                        var myVertex = vertices[myIndex];
                        for (int j = i + 1; j < borderIndexCount; j++)
                        {
                            var otherIndex = borderIndices[j];
                            if (otherIndex == -1)
                                continue;

                            var otherVertex = vertices[otherIndex];
                            if ((myVertex.p - otherVertex.p).MagnitudeSqr <= vertexLinkDistanceSqr)
                            {
                                borderIndices[j] = -1;
                                vertices[myIndex].border = false;
                                vertices[otherIndex].border = false;

                                if (AreUVsTheSame(0, myIndex, otherIndex))
                                {
                                    vertices[myIndex].foldover = true;
                                    vertices[otherIndex].foldover = true;
                                }
                                else
                                {
                                    vertices[myIndex].seam = true;
                                    vertices[otherIndex].seam = true;
                                }

                                for (int k = 0; k < otherVertex.tcount; k++)
                                {
                                    var r = refs[otherVertex.tstart + k];
                                    triangles[r.tid][r.tvertex] = myIndex;
                                }
                            }
                        }
                    }

                    // Update the references again
                    UpdateReferences();
                }

                // Init Quadrics by Plane & Edge Errors
                //
                // required at the beginning ( iteration == 0 )
                // recomputing during the simplification is not required,
                // but mostly improves the result for closed meshes
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                Vector3d n, p0, p1, p2, p10, p20, dummy;
                int dummy2;
                SymmetricMatrix sm;
                for (int i = 0; i < triangleCount; i++)
                {
                    var triangle = triangles[i];
                    var vert0 = vertices[triangle.v0];
                    var vert1 = vertices[triangle.v1];
                    var vert2 = vertices[triangle.v2];
                    p0 = vert0.p;
                    p1 = vert1.p;
                    p2 = vert2.p;
                    p10 = p1 - p0;
                    p20 = p2 - p0;
                    Vector3d.Cross(ref p10, ref p20, out n);
                    n.Normalize();
                    triangles[i].n = n;

                    sm = new SymmetricMatrix(n.x, n.y, n.z, -Vector3d.Dot(ref n, ref p0));
                    vert0.q += sm;
                    vert1.q += sm;
                    vert2.q += sm;
                    vertices[triangle.v0] = vert0;
                    vertices[triangle.v1] = vert1;
                    vertices[triangle.v2] = vert2;
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    // Calc Edge Error
                    var triangle = triangles[i];
                    //triangle.area = CalculateArea(triangle.v0, triangle.v1, triangle.v2);
                    triangle.err0 = CalculateError(triangle.v0, triangle.v1, out dummy, out dummy2);
                    triangle.err1 = CalculateError(triangle.v1, triangle.v2, out dummy, out dummy2);
                    triangle.err2 = CalculateError(triangle.v2, triangle.v0, out dummy, out dummy2);
                    triangle.err3 = MathHelper.Min(triangle.err0, triangle.err1, triangle.err2);
                    triangles[i] = triangle;
                }
            }
        }
        #endregion

        #region Update References
        private void UpdateReferences()
        {
            int triangleCount = this.triangles.Length;
            int vertexCount = this.vertices.Length;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;

            // Init Reference ID list
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = vertices[i];
                vertex.tstart = 0;
                vertex.tcount = 0;
                vertices[i] = vertex;
            }

            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                ++vertices[triangle.v0].tcount;
                ++vertices[triangle.v1].tcount;
                ++vertices[triangle.v2].tcount;
            }

            int tstart = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = vertices[i];
                vertex.tstart = tstart;
                tstart += vertex.tcount;
                vertex.tcount = 0;
                vertices[i] = vertex;
            }

            // Write References
            this.refs.Resize(tstart);
            var refs = this.refs.Data;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                var vert0 = vertices[triangle.v0];
                var vert1 = vertices[triangle.v1];
                var vert2 = vertices[triangle.v2];

                refs[vert0.tstart + vert0.tcount].Set(i, 0);
                refs[vert1.tstart + vert1.tcount].Set(i, 1);
                refs[vert2.tstart + vert2.tcount].Set(i, 2);
                ++vert0.tcount;
                ++vert1.tcount;
                ++vert2.tcount;

                vertices[triangle.v0] = vert0;
                vertices[triangle.v1] = vert1;
                vertices[triangle.v2] = vert2;
            }
        }
        #endregion

        #region Compact Mesh
        /// <summary>
        /// Finally compact mesh before exiting.
        /// </summary>
        private void CompactMesh()
        {
            int dst = 0;
            var vertices = this.vertices.Data;
            int vertexCount = this.vertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tcount = 0;
            }

            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                if (!triangle.deleted)
                {
                    if (triangle.va0 != triangle.v0)
                    {
                        vertices[triangle.va0].p = vertices[triangle.v0].p;
                        triangle.v0 = triangle.va0;
                    }
                    if (triangle.va1 != triangle.v1)
                    {
                        vertices[triangle.va1].p = vertices[triangle.v1].p;
                        triangle.v1 = triangle.va1;
                    }
                    if (triangle.va2 != triangle.v2)
                    {
                        vertices[triangle.va2].p = vertices[triangle.v2].p;
                        triangle.v2 = triangle.va2;
                    }
                    triangles[dst++] = triangle;

                    vertices[triangle.v0].tcount = 1;
                    vertices[triangle.v1].tcount = 1;
                    vertices[triangle.v2].tcount = 1;
                }
            }

            this.triangles.Resize(dst);
            triangles = this.triangles.Data;
            triangleCount = dst;

            var vertNormals = (this.vertNormals != null ? this.vertNormals.Data : null);
            var vertTangents = (this.vertTangents != null ? this.vertTangents.Data : null);
            var vertUV2D = (this.vertUV2D != null ? this.vertUV2D.Data : null);
            var vertUV3D = (this.vertUV3D != null ? this.vertUV3D.Data : null);
            var vertUV4D = (this.vertUV4D != null ? this.vertUV4D.Data : null);
            var vertColors = (this.vertColors != null ? this.vertColors.Data : null);
            var vertBoneWeights = (this.vertBoneWeights != null ? this.vertBoneWeights.Data : null);

            dst = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                var vert = vertices[i];
                if (vert.tcount > 0)
                {
                    vert.tstart = dst;
                    vertices[i] = vert;

                    if (dst != i)
                    {
                        vertices[dst].p = vert.p;
                        if (vertNormals != null) vertNormals[dst] = vertNormals[i];
                        if (vertTangents != null) vertTangents[dst] = vertTangents[i];
                        if (vertUV2D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV2D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV3D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV3D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV4D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV4D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertColors != null) vertColors[dst] = vertColors[i];
                        if (vertBoneWeights != null) vertBoneWeights[dst] = vertBoneWeights[i];
                    }
                    ++dst;
                }
            }

            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                triangle.v0 = vertices[triangle.v0].tstart;
                triangle.v1 = vertices[triangle.v1].tstart;
                triangle.v2 = vertices[triangle.v2].tstart;
                triangles[i] = triangle;
            }

            this.vertices.Resize(dst);
            if (vertNormals != null) this.vertNormals.Resize(dst, true);
            if (vertTangents != null) this.vertTangents.Resize(dst, true);
            if (vertUV2D != null) this.vertUV2D.Resize(dst, true);
            if (vertUV3D != null) this.vertUV3D.Resize(dst, true);
            if (vertUV4D != null) this.vertUV4D.Resize(dst, true);
            if (vertColors != null) this.vertColors.Resize(dst, true);
            if (vertBoneWeights != null) this.vertBoneWeights.Resize(dst, true);
        }
        #endregion
        #endregion

        #region Public Methods
        #region Sub-Meshes
        /// <summary>
        /// Returns the triangle indices for a specific sub-mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <returns>The triangle indices.</returns>
        public int[] GetSubMeshTriangles(int subMeshIndex)
        {
            // First get the sub-mesh offsets
            int triangleCount = this.triangles.Length;
            var triArr = this.triangles.Data;
            int[] subMeshOffsets = new int[subMeshCount];
            int lastSubMeshOffset = -1;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triArr[i];
                if (triangle.subMeshIndex >= subMeshIndex && triangle.subMeshIndex != lastSubMeshOffset)
                {
                    for (int j = lastSubMeshOffset + 1; j < triangle.subMeshIndex; j++)
                    {
                        subMeshOffsets[j] = i - 1;
                    }
                    subMeshOffsets[triangle.subMeshIndex] = i;
                    lastSubMeshOffset = triangle.subMeshIndex;
                    if (lastSubMeshOffset >= (subMeshIndex + 1))
                        break;
                }
            }
            for (int i = lastSubMeshOffset + 1; i < subMeshCount; i++)
            {
                subMeshOffsets[i] = triangleCount;
            }

            int startOffset = subMeshOffsets[subMeshIndex];
            int endOffset = ((subMeshIndex + 1) < subMeshCount ? subMeshOffsets[subMeshIndex + 1] : triangleCount) - 1;
            int subMeshTriangleCount = endOffset - startOffset + 1;
            if (subMeshTriangleCount < 0) subMeshTriangleCount = 0;
            int[] subMeshIndices = new int[subMeshTriangleCount * 3];

            for (int triangleIndex = startOffset; triangleIndex <= endOffset; triangleIndex++)
            {
                var triangle = triArr[triangleIndex];
                int offset = (triangleIndex - startOffset) * 3;
                subMeshIndices[offset] = triangle.v0;
                subMeshIndices[offset + 1] = triangle.v1;
                subMeshIndices[offset + 2] = triangle.v2;
            }

            return subMeshIndices;
        }

        /// <summary>
        /// Clears out all sub-meshes.
        /// </summary>
        public void ClearSubMeshes()
        {
            subMeshCount = 0;
            triangles.Resize(0);
        }

        /// <summary>
        /// Adds a sub-mesh triangle indices for a specific sub-mesh.
        /// </summary>
        /// <param name="triangles">The triangle indices.</param>
        public void AddSubMeshTriangles(int[] triangles)
        {
            if (triangles == null)
                throw new ArgumentNullException("triangles");
            else if ((triangles.Length % 3) != 0)
                throw new ArgumentException("The index array length must be a multiple of 3 in order to represent triangles.", "triangles");

            int subMeshIndex = subMeshCount++;
            int triangleIndex = this.triangles.Length;
            int subMeshTriangleCount = triangles.Length / 3;
            this.triangles.Resize(this.triangles.Length + subMeshTriangleCount);
            var trisArr = this.triangles.Data;
            for (int i = 0; i < subMeshTriangleCount; i++)
            {
                int offset = i * 3;
                int v0 = triangles[offset];
                int v1 = triangles[offset + 1];
                int v2 = triangles[offset + 2];
                trisArr[triangleIndex + i] = new Triangle(v0, v1, v2, subMeshIndex);
            }

            triangleIndex += subMeshTriangleCount;
        }

        /// <summary>
        /// Adds several sub-meshes at once with their triangle indices for each sub-mesh.
        /// </summary>
        /// <param name="triangles">The triangle indices for each sub-mesh.</param>
        public void AddSubMeshTriangles(int[][] triangles)
        {
            if (triangles == null)
                throw new ArgumentNullException("triangles");

            int totalTriangleCount = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                if ((triangles[i].Length % 3) != 0)
                    throw new ArgumentException(string.Format("The index array length at index {0} must be a multiple of 3 in order to represent triangles.", i), "triangles");

                totalTriangleCount += triangles[i].Length / 3;
            }

            int triangleIndex = this.triangles.Length;
            this.triangles.Resize(this.triangles.Length + totalTriangleCount);
            var trisArr = this.triangles.Data;

            for (int i = 0; i < triangles.Length; i++)
            {
                int subMeshIndex = subMeshCount++;
                var subMeshTriangles = triangles[i];
                int subMeshTriangleCount = subMeshTriangles.Length / 3;
                for (int j = 0; j < subMeshTriangleCount; j++)
                {
                    int offset = j * 3;
                    int v0 = subMeshTriangles[offset];
                    int v1 = subMeshTriangles[offset + 1];
                    int v2 = subMeshTriangles[offset + 2];
                    trisArr[triangleIndex + j] = new Triangle(v0, v1, v2, subMeshIndex);
                }

                triangleIndex += subMeshTriangleCount;
            }
        }
        #endregion

        #region UV Sets
        #region Getting
        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector2[] GetUVs2D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (vertUV2D != null && vertUV2D[channel] != null)
            {
                return vertUV2D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (3D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector3[] GetUVs3D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (vertUV3D != null && vertUV3D[channel] != null)
            {
                return vertUV3D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (4D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector4[] GetUVs4D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (vertUV4D != null && vertUV4D[channel] != null)
            {
                return vertUV4D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector2> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            else if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (vertUV2D != null && vertUV2D[channel] != null)
            {
                var uvData = vertUV2D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (3D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector3> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            else if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (vertUV3D != null && vertUV3D[channel] != null)
            {
                var uvData = vertUV3D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (4D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            else if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (vertUV4D != null && vertUV4D[channel] != null)
            {
                var uvData = vertUV4D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }
        #endregion

        #region Setting
        /// <summary>
        /// Sets the UVs (2D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector2[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV2D == null)
                    vertUV2D = new UVChannels<Vector2>();

                int uvCount = uvs.Length;
                var uvSet = vertUV2D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector2>(uvCount, uvCount);
                    vertUV2D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV2D != null)
                {
                    vertUV2D[channel] = null;
                }
            }

            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (3D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector3[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV3D == null)
                    vertUV3D = new UVChannels<Vector3>();

                int uvCount = uvs.Length;
                var uvSet = vertUV3D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector3>(uvCount, uvCount);
                    vertUV3D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV3D != null)
                {
                    vertUV3D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (4D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector4[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV4D == null)
                    vertUV4D = new UVChannels<Vector4>();

                int uvCount = uvs.Length;
                var uvSet = vertUV4D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector4>(uvCount, uvCount);
                    vertUV4D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV4D != null)
                {
                    vertUV4D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (2D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector2> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV2D == null)
                    vertUV2D = new UVChannels<Vector2>();

                int uvCount = uvs.Count;
                var uvSet = vertUV2D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector2>(uvCount, uvCount);
                    vertUV2D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV2D != null)
                {
                    vertUV2D[channel] = null;
                }
            }

            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (3D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector3> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV3D == null)
                    vertUV3D = new UVChannels<Vector3>();

                int uvCount = uvs.Count;
                var uvSet = vertUV3D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector3>(uvCount, uvCount);
                    vertUV3D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV3D != null)
                {
                    vertUV3D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (4D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV4D == null)
                    vertUV4D = new UVChannels<Vector4>();

                int uvCount = uvs.Count;
                var uvSet = vertUV4D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector4>(uvCount, uvCount);
                    vertUV4D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV4D != null)
                {
                    vertUV4D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
        }
        #endregion
        #endregion

        #region Initialize
        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void Initialize(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            this.Vertices = mesh.vertices;
            this.Normals = mesh.normals;
            this.Tangents = mesh.tangents;
            this.UV1 = mesh.uv;
            this.UV2 = mesh.uv2;
            this.UV3 = mesh.uv3;
            this.UV4 = mesh.uv4;
            this.Colors = mesh.colors;
            this.BoneWeights = mesh.boneWeights;

            ClearSubMeshes();

            int subMeshCount = mesh.subMeshCount;
            int[][] subMeshTriangles = new int[subMeshCount][];
            for (int i = 0; i < subMeshCount; i++)
            {
                subMeshTriangles[i] = mesh.GetTriangles(i);
            }
            AddSubMeshTriangles(subMeshTriangles);
        }
        #endregion

        #region Simplify Mesh
        /// <summary>
        /// Simplifies the mesh to a desired quality.
        /// </summary>
        /// <param name="quality">The target quality (between 0 and 1).</param>
        public void SimplifyMesh(float quality)
        {
            quality = Mathf.Clamp01(quality);

            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(20);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(20);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;
            int targetTrisCount = Mathf.RoundToInt(triangleCount * quality);

            for (int iteration = 0; iteration < 100; iteration++)
            {
                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;

                // Update mesh once in a while
                if ((iteration % 5) == 0)
                {
                    UpdateMesh(iteration);
                    triangles = this.triangles.Data;
                    triangleCount = this.triangles.Length;
                    vertices = this.vertices.Data;
                }

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = 0.000000001 * Math.Pow(iteration + 3, agressiveness);

                if (verbose)
                {
                    Debug.LogFormat("iteration {0} - triangles {1} threshold {2}", iteration, (startTrisCount - deletedTris), threshold);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, targetTrisCount, threshold, deleted0, deleted1, ref deletedTris);
            }

            CompactMesh();

            if (verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", this.triangles.Length);
            }
        }

        /// <summary>
        /// Simplifies the mesh without losing too much quality.
        /// </summary>
        public void SimplifyMeshLossless()
        {
            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(0);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(0);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;

            for (int iteration = 0; iteration < 9999; iteration++)
            {
                // Update mesh constantly
                UpdateMesh(iteration);
                triangles = this.triangles.Data;
                triangleCount = this.triangles.Length;
                vertices = this.vertices.Data;

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = DoubleEpsilon;

                if (verbose)
                {
                    Debug.LogFormat("Lossless iteration {0} - triangles {1}", iteration, triangleCount);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, 0, threshold, deleted0, deleted1, ref deletedTris);

                if (deletedTris <= 0)
                    break;

                deletedTris = 0;
            }

            CompactMesh();

            if (verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", this.triangles.Length);
            }
        }
        #endregion

        #region To Mesh
        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public Mesh ToMesh()
        {
            var vertices = this.Vertices;
            var normals = this.Normals;
            var tangents = this.Tangents;
            var colors = this.Colors;
            var boneWeights = this.BoneWeights;

            var newMesh = new Mesh();

#if UNITY_2017_3
            // TODO: Use baseVertex if all submeshes are within the ushort.MaxValue range even though the total vertex count is above
            bool use32BitIndex = (vertices.Length > ushort.MaxValue);
            newMesh.indexFormat = (use32BitIndex ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
#endif

            newMesh.subMeshCount = subMeshCount;
            newMesh.vertices = this.Vertices;
            if (normals != null) newMesh.normals = normals;
            if (tangents != null) newMesh.tangents = tangents;

            if (vertUV2D != null)
            {
                List<Vector2> uvSet = null;
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (vertUV2D[i] != null)
                    {
                        if (uvSet == null)
                            uvSet = new List<Vector2>(vertUV2D[i].Length);

                        GetUVs(i, uvSet);
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            if (vertUV3D != null)
            {
                List<Vector3> uvSet = null;
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (vertUV3D[i] != null)
                    {
                        if (uvSet == null)
                            uvSet = new List<Vector3>(vertUV3D[i].Length);

                        GetUVs(i, uvSet);
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            if (vertUV4D != null)
            {
                List<Vector4> uvSet = null;
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (vertUV4D[i] != null)
                    {
                        if (uvSet == null)
                            uvSet = new List<Vector4>(vertUV4D[i].Length);

                        GetUVs(i, uvSet);
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            if (colors != null) newMesh.colors = colors;
            if (boneWeights != null) newMesh.boneWeights = boneWeights;

            for (int i = 0; i < subMeshCount; i++)
            {
                var subMeshTriangles = GetSubMeshTriangles(i);
                newMesh.SetTriangles(subMeshTriangles, i, false);
            }

            newMesh.RecalculateBounds();
            return newMesh;
        }
        #endregion
        #endregion
    }
}