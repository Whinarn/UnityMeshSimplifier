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

                area = 0;
                err0 = err1 = err2 = err3 = 0;
                deleted = dirty = false;
                n = new Vector3d();
            }
            #endregion

            #region Public Methods
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
            public bool linked;

            public Vertex(Vector3d p)
            {
                this.p = p;
                this.tstart = 0;
                this.tcount = 0;
                this.q = new SymmetricMatrix();
                this.border = true;
                this.linked = false;
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
        #endregion

        #region Fields
        private bool keepBorders = false;
        private double agressiveness = 7.0;
        private bool verbose = false;

        private int subMeshCount = 0;
        private ResizableArray<Triangle> triangles = null;
        private ResizableArray<Vertex> vertices = null;
        private ResizableArray<Ref> refs = null;

        private ResizableArray<Vector3> vertNormals = null;
        private ResizableArray<Vector4> vertTangents = null;
        private ResizableArray<Vector2> vertUV1 = null;
        private ResizableArray<Vector2> vertUV2 = null;
        private ResizableArray<Vector2> vertUV3 = null;
        private ResizableArray<Vector2> vertUV4 = null;
        private ResizableArray<Color> vertColors = null;
        private ResizableArray<BoneWeight> vertBoneWeights = null;

        // Pre-allocated buffer for error values
        private double[] errArr = new double[3];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if borders should be kept.
        /// </summary>
        public bool KeepBorders
        {
            get { return keepBorders; }
            set { keepBorders = value; }
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
                InitializeVertexAttribute(value, ref vertNormals);
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
                InitializeVertexAttribute(value, ref vertTangents);
            }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 1.
        /// </summary>
        public Vector2[] UV1
        {
            get { return (vertUV1 != null ? vertUV1.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertUV1);
            }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 2.
        /// </summary>
        public Vector2[] UV2
        {
            get { return (vertUV2 != null ? vertUV2.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertUV2);
            }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 3.
        /// </summary>
        public Vector2[] UV3
        {
            get { return (vertUV3 != null ? vertUV3.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertUV3);
            }
        }

        /// <summary>
        /// Gets or sets the vertex UV set 4.
        /// </summary>
        public Vector2[] UV4
        {
            get { return (vertUV4 != null ? vertUV4.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertUV4);
            }
        }

        /// <summary>
        /// Gets or sets the vertex colors.
        /// </summary>
        public Color[] Colors
        {
            get { return (vertColors != null ? vertColors.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertColors);
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
                InitializeVertexAttribute(value, ref vertBoneWeights);
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
        private void InitializeVertexAttribute<T>(T[] attributeValues, ref ResizableArray<T> attributeArray)
        {
            if (attributeValues != null && attributeValues.Length == vertices.Length)
            {
                if (attributeArray == null)
                {
                    attributeArray = new ResizableArray<T>(attributeValues.Length);
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

        private double CalculateError(int i0, int i1, out Vector3d result)
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
                    result = p3;
                else if (error == error2)
                    result = p2;
                else if (error == error1)
                    result = p1;
                else
                    result = p3;
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
        private void UpdateTriangles(int i0, ref Vertex v, ResizableArray<bool> deleted, ref int deletedTriangles)
        {
            Vector3d p;
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
                t.dirty = true;
                t.area = CalculateArea(t.v0, t.v1, t.v2);
                t.err0 = CalculateError(t.v0, t.v1, out p);
                t.err1 = CalculateError(t.v1, t.v2, out p);
                t.err2 = CalculateError(t.v2, t.v0, out p);
                t.err3 = MathHelper.Min(t.err0, t.err1, t.err2);
                triangles[tid] = t;
                refs.Add(r);
            }
        }
        #endregion

        #region Merge Vertices
        private void MergeVertices(int i0, int i1)
        {
            if (vertNormals != null)
            {
                vertNormals[i0] = (vertNormals[i0] + vertNormals[i1]) * 0.5f;
            }
            if (vertTangents != null)
            {
                vertTangents[i0] = (vertTangents[i0] + vertTangents[i1]) * 0.5f;
            }
            if (vertUV1 != null)
            {
                vertUV1[i0] = (vertUV1[i0] + vertUV1[i1]) * 0.5f;
            }
            if (vertUV2 != null)
            {
                vertUV2[i0] = (vertUV2[i0] + vertUV2[i1]) * 0.5f;
            }
            if (vertUV3 != null)
            {
                vertUV3[i0] = (vertUV3[i0] + vertUV3[i1]) * 0.5f;
            }
            if (vertUV4 != null)
            {
                vertUV4[i0] = (vertUV4[i0] + vertUV4[i1]) * 0.5f;
            }
            if (vertColors != null)
            {
                vertColors[i0] = (vertColors[i0] + vertColors[i1]) * 0.5f;
            }
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
            for (int i = 0; i < triangleCount; i++)
            {
                var t = triangles[i];
                if (t.dirty || t.deleted || t.err3 > threshold)
                    continue;

                t.GetErrors(errArr);
                for (int j = 0; j < 3; j++)
                {
                    if (errArr[j] > threshold)
                        continue;

                    int i0 = t[j];
                    int i1 = t[(j + 1) % 3];
                    v0 = vertices[i0];
                    v1 = vertices[i1];

                    // Border check
                    if (v0.border != v1.border)
                        continue;

                    // If borders should be kept
                    if (keepBorders && (v0.border || v1.border))
                        continue;

                    // Compute vertex to collapse to
                    CalculateError(i0, i1, out p);
                    deleted0.Resize(v0.tcount); // normals temporarily
                    deleted1.Resize(v1.tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(p, i0, i1, ref v0, deleted0))
                        continue;
                    if (Flipped(p, i1, i0, ref v1, deleted1))
                        continue;

                    // Not flipped, so remove edge
                    v0.p = p;
                    v0.q += v1.q;
                    vertices[i0] = v0;
                    MergeVertices(i0, i1);

                    int tstart = refs.Length;
                    UpdateTriangles(i0, ref v0, deleted0, ref deletedTris);
                    UpdateTriangles(i0, ref v1, deleted1, ref deletedTris);

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

            // Init Quadrics by Plane & Edge Errors
            //
            // required at the beginning ( iteration == 0 )
            // recomputing during the simplification is not required,
            // but mostly improves the result for closed meshes
            if (iteration == 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                Vector3d n, p0, p1, p2, p10, p20, dummy;
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
                    triangle.err0 = CalculateError(triangle.v0, triangle.v1, out dummy);
                    triangle.err1 = CalculateError(triangle.v1, triangle.v2, out dummy);
                    triangle.err2 = CalculateError(triangle.v2, triangle.v0, out dummy);
                    triangle.err3 = MathHelper.Min(triangle.err0, triangle.err1, triangle.err2);
                    triangles[i] = triangle;
                }
            }

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

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                List<int> vcount = new List<int>();
                List<int> vids = new List<int>();
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].border = false;
                }

                int ofs;
                int id;
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
                        }
                    }
                }
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
            var vertUV1 = (this.vertUV1 != null ? this.vertUV1.Data : null);
            var vertUV2 = (this.vertUV2 != null ? this.vertUV2.Data : null);
            var vertUV3 = (this.vertUV3 != null ? this.vertUV3.Data : null);
            var vertUV4 = (this.vertUV4 != null ? this.vertUV4.Data : null);
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
                        if (vertUV1 != null) vertUV1[dst] = vertUV1[i];
                        if (vertUV2 != null) vertUV2[dst] = vertUV2[i];
                        if (vertUV3 != null) vertUV3[dst] = vertUV3[i];
                        if (vertUV4 != null) vertUV4[dst] = vertUV4[i];
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

            this.vertices.Resize(dst, true);
            if (vertNormals != null) this.vertNormals.Resize(dst, true);
            if (vertTangents != null) this.vertTangents.Resize(dst, true);
            if (vertUV1 != null) this.vertUV1.Resize(dst, true);
            if (vertUV2 != null) this.vertUV2.Resize(dst, true);
            if (vertUV3 != null) this.vertUV3.Resize(dst, true);
            if (vertUV4 != null) this.vertUV4.Resize(dst, true);
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
            var uv1 = this.UV1;
            var uv2 = this.UV2;
            var uv3 = this.UV3;
            var uv4 = this.UV4;
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
            if (uv1 != null) newMesh.uv = uv1;
            if (uv2 != null) newMesh.uv2 = uv2;
            if (uv3 != null) newMesh.uv3 = uv3;
            if (uv4 != null) newMesh.uv4 = uv4;
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