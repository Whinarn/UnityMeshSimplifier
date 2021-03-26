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
    /// Options for mesh simplification.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct SimplificationOptions
    {
        /// <summary>
        /// The default simplification options.
        /// </summary>
        public static readonly SimplificationOptions Default = new SimplificationOptions
        {
            PreserveBorderEdges = false,
            PreserveUVSeamEdges = false,
            PreserveUVFoldoverEdges = false,
            PreserveSurfaceCurvature = false,
            EnableSmartLink = true,
            VertexLinkDistance = double.Epsilon,
            MaxIterationCount = 100,
            Agressiveness = 7.0,
            ManualUVComponentCount = false,
            UVComponentCount = 2
        };

        /// <summary>
        /// If the border edges should be preserved.
        /// Default value: false
        /// </summary>
        [Tooltip("If the border edges should be preserved.")]
        public bool PreserveBorderEdges;
        /// <summary>
        /// If the UV seam edges should be preserved.
        /// Default value: false
        /// </summary>
        [Tooltip("If the UV seam edges should be preserved.")]
        public bool PreserveUVSeamEdges;
        /// <summary>
        /// If the UV foldover edges should be preserved.
        /// Default value: false
        /// </summary>
        [Tooltip("If the UV foldover edges should be preserved.")]
        public bool PreserveUVFoldoverEdges;
        /// <summary>
        /// If the discrete curvature of the mesh surface be taken into account during simplification. Taking surface curvature into account can result in good quality mesh simplification, but it can slow the simplification process significantly.
        /// Default value: false
        /// </summary>
        [Tooltip("If the discrete curvature of the mesh surface be taken into account during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.")]
        public bool PreserveSurfaceCurvature;
        /// <summary>
        /// If a feature for smarter vertex linking should be enabled, reducing artifacts in the
        /// decimated result at the cost of a slightly more expensive initialization by treating vertices at
        /// the same position as the same vertex while separating the attributes.
        /// Default value: true
        /// </summary>
        [Tooltip("If a feature for smarter vertex linking should be enabled, reducing artifacts at the cost of slower simplification.")]
        public bool EnableSmartLink;
        /// <summary>
        /// The maximum distance between two vertices in order to link them.
        /// Note that this value is only used if EnableSmartLink is true.
        /// Default value: double.Epsilon
        /// </summary>
        [Tooltip("The maximum distance between two vertices in order to link them.")]
        public double VertexLinkDistance;
        /// <summary>
        /// The maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.
        /// Sometimes a lower maximum count might be desired in order to lower the performance cost.
        /// Default value: 100
        /// </summary>
        [Tooltip("The maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.")]
        public int MaxIterationCount;
        /// <summary>
        /// The agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.
        /// Default value: 7.0
        /// </summary>
        [Tooltip("The agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.")]
        public double Agressiveness;
        /// <summary>
        /// If a manual UV component count should be used (set by UVComponentCount), instead of the automatic detection.
        /// Default value: false
        /// </summary>
        [Tooltip("If a manual UV component count should be used (set by UV Component Count below), instead of the automatic detection.")]
        public bool ManualUVComponentCount;
        /// <summary>
        /// The UV component count. The same UV component count will be used on all UV channels.
        /// Default value: 2
        /// </summary>
        [Range(0, 4), Tooltip("The UV component count. The same UV component count will be used on all UV channels.")]
        public int UVComponentCount;
    }
}
