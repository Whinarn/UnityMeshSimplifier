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

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A blend shape.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct BlendShape
    {
        /// <summary>
        /// The name of the blend shape.
        /// </summary>
        public string ShapeName;
        /// <summary>
        /// The blend shape frames.
        /// </summary>
        public BlendShapeFrame[] Frames;

        /// <summary>
        /// Creates a new blend shape.
        /// </summary>
        /// <param name="shapeName">The name of the blend shape.</param>
        /// <param name="frames">The blend shape frames.</param>
        public BlendShape(string shapeName, BlendShapeFrame[] frames)
        {
            this.ShapeName = shapeName;
            this.Frames = frames;
        }
    }

    /// <summary>
    /// A blend shape frame.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct BlendShapeFrame
    {
        /// <summary>
        /// The weight of the blend shape frame.
        /// </summary>
        public float FrameWeight;
        /// <summary>
        /// The delta vertices of the blend shape frame.
        /// </summary>
        public Vector3[] DeltaVertices;
        /// <summary>
        /// The delta normals of the blend shape frame.
        /// </summary>
        public Vector3[] DeltaNormals;
        /// <summary>
        /// The delta tangents of the blend shape frame.
        /// </summary>
        public Vector3[] DeltaTangents;

        /// <summary>
        /// Creates a new blend shape frame.
        /// </summary>
        /// <param name="frameWeight">The weight of the blend shape frame.</param>
        /// <param name="deltaVertices">The delta vertices of the blend shape frame.</param>
        /// <param name="deltaNormals">The delta normals of the blend shape frame.</param>
        /// <param name="deltaTangents">The delta tangents of the blend shape frame.</param>
        public BlendShapeFrame(float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            this.FrameWeight = frameWeight;
            this.DeltaVertices = deltaVertices;
            this.DeltaNormals = deltaNormals;
            this.DeltaTangents = deltaTangents;
        }
    }
}
