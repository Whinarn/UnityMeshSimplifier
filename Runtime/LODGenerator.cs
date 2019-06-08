#region License
/*
MIT License

Copyright(c) 2019 Mattias Edlund

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

using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A LOD (level of detail) generator.
    /// </summary>
    [AddComponentMenu("Rendering/LOD Generator")]
    public sealed class LODGenerator : MonoBehaviour
    {
        #region LOD Level
        /// <summary>
        /// A LOD (level of detail) level.
        /// </summary>
        [System.Serializable]
        public struct Level
        {
            [SerializeField, Range(0f, 1f), Tooltip("The desired quality for this level.")]
            private float quality;

            /// <summary>
            /// Gets or sets the quality of this level between 0 and 1.
            /// </summary>
            public float Quality
            {
                get { return quality; }
                set { quality = Mathf.Clamp01(value); }
            }

            /// <summary>
            /// Creates a new LOD level.
            /// </summary>
            /// <param name="quality">The quality of this level between 0 and 1.</param>
            public Level(float quality)
            {
                this.quality = Mathf.Clamp01(quality);
            }
        }
        #endregion

        #region Fields
        [SerializeField, Tooltip("The LOD levels.")]
        private Level[] levels = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the LOD levels for this generator.
        /// </summary>
        public Level[] Levels
        {
            get { return levels; }
            set { levels = value; }
        }
        #endregion
    }
}