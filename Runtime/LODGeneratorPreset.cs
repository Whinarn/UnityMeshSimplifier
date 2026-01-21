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

using UnityEngine;

namespace UnityMeshSimplifier
{
    [CreateAssetMenu(
        fileName = "LOD Preset",
        menuName = "Mesh Simplifier/LOD Preset")]
    public sealed class LODGeneratorPreset : ScriptableObject
    {
        [SerializeField, Tooltip("The fade mode used by the created LOD group.")]
        private LODFadeMode fadeMode = LODFadeMode.None;

        [SerializeField, Tooltip("If the cross-fading should be animated by time.")]
        private bool animateCrossFading = false;

        [SerializeField, Tooltip("The simplification options.")]
        private SimplificationOptions simplificationOptions = SimplificationOptions.Default;

        [SerializeField, Tooltip("The LOD levels.")]
        private LODLevel[] levels = null;

        #region Properties
        /// <summary>
        /// Gets or sets the fade mode used by the created LOD group.
        /// </summary>
        public LODFadeMode FadeMode
        {
            get { return fadeMode; }
            set { fadeMode = value; }
        }

        /// <summary>
        /// Gets or sets if the cross-fading should be animated by time. The animation duration
        /// is specified globally as crossFadeAnimationDuration.
        /// </summary>
        public bool AnimateCrossFading
        {
            get { return animateCrossFading; }
            set { animateCrossFading = value; }
        }

        /// <summary>
        /// Gets or sets the simplification options.
        /// </summary>
        public SimplificationOptions SimplificationOptions
        {
            get { return simplificationOptions; }
            set { simplificationOptions = value; }
        }

        /// <summary>
        /// Gets or sets the LOD levels for this preset.
        /// </summary>
        public LODLevel[] Levels
        {
            get { return levels; }
            set { levels = value; }
        }
        #endregion

        #region Unity Events
        private void Reset()
        {
            fadeMode = LODFadeMode.None;
            animateCrossFading = false;
            simplificationOptions = SimplificationOptions.Default;
            levels = LODLevel.GetDefaultLevels();
        }
        #endregion
    }
}
