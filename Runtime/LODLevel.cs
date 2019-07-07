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

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A LOD (level of detail) level.
    /// </summary>
    [Serializable]
    public struct LODLevel
    {
        #region Fields
        [SerializeField, Range(0f, 1f), Tooltip("The screen relative height to use for the transition.")]
        private float screenRelativeTransitionHeight;
        [SerializeField, Range(0f, 1f), Tooltip("The width of the cross-fade transition zone (proportion to the current LOD's whole length).")]
        private float fadeTransitionWidth;
        [SerializeField, Range(0f, 1f), Tooltip("The desired quality for this level.")]
        private float quality;
        [SerializeField, Tooltip("If all renderers and meshes under this level should be combined into one, where possible.")]
        private bool combineMeshes;
        [SerializeField, Tooltip("If all sub-meshes should be combined into one, where possible.")]
        private bool combineSubMeshes;

        [SerializeField, Tooltip("The renderers used in this level.")]
        private Renderer[] renderers;

        [SerializeField, Tooltip("The skin quality to use for renderers on this level.")]
        private SkinQuality skinQuality;
        [SerializeField, Tooltip("The shadow casting mode for renderers on this level.")]
        private ShadowCastingMode shadowCastingMode;
        [SerializeField, Tooltip("If renderers on this level should receive shadows.")]
        private bool receiveShadows;
        [SerializeField, Tooltip("The motion vector generation mode for renderers on this level.")]
        private MotionVectorGenerationMode motionVectorGenerationMode;
        [SerializeField, Tooltip("If renderers on this level should use skinned motion vectors.")]
        private bool skinnedMotionVectors;
        [SerializeField, Tooltip("The light probe usage for renderers on this level.")]
        private LightProbeUsage lightProbeUsage;
        [SerializeField, Tooltip("The reflection probe usage for renderers on this level.")]
        private ReflectionProbeUsage reflectionProbeUsage;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the screen relative height to use for the transition [0-1].
        /// </summary>
        public float ScreenRelativeTransitionHeight
        {
            get { return screenRelativeTransitionHeight; }
            set { screenRelativeTransitionHeight = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// Gets or sets the width of the cross-fade transition zone (proportion to the current LOD's whole length) [0-1]. Only used if it's not animated.
        /// </summary>
        public float FadeTransitionWidth
        {
            get { return fadeTransitionWidth; }
            set { fadeTransitionWidth = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// Gets or sets the quality of this level [0-1].
        /// </summary>
        public float Quality
        {
            get { return quality; }
            set { quality = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// Gets or sets if all renderers and meshes under this level should be combined into one, where possible.
        /// </summary>
        public bool CombineMeshes
        {
            get { return combineMeshes; }
            set { combineMeshes = value; }
        }

        /// <summary>
        /// Gets or sets if all sub-meshes should be combined into one, where possible.
        /// NOTE: This is only used if <see cref="CombineMeshes"/> is true.
        /// </summary>
        public bool CombineSubMeshes
        {
            get { return combineSubMeshes; }
            set { combineSubMeshes = value; }
        }

        /// <summary>
        /// Gets or sets the renderers used in this level.
        /// These will have no purpose if automatic collection is used for the LOD generator.
        /// </summary>
        public Renderer[] Renderers
        {
            get { return renderers; }
            set { renderers = value; }
        }

        /// <summary>
        /// Gets or sets the skin quality to use for renderers on this level.
        /// </summary>
        public SkinQuality SkinQuality
        {
            get { return skinQuality; }
            set { skinQuality = value; }
        }

        /// <summary>
        /// Gets or sets the shadow casting mode for renderers on this level.
        /// </summary>
        public ShadowCastingMode ShadowCastingMode
        {
            get { return shadowCastingMode; }
            set { shadowCastingMode = value; }
        }

        /// <summary>
        /// Gets or sets if renderers on this level should receive shadows.
        /// </summary>
        public bool ReceiveShadows
        {
            get { return receiveShadows; }
            set { receiveShadows = value; }
        }

        /// <summary>
        /// Gets or sets the motion vector generation mode for renderers on this level.
        /// </summary>
        public MotionVectorGenerationMode MotionVectorGenerationMode
        {
            get { return motionVectorGenerationMode; }
            set { motionVectorGenerationMode = value; }
        }

        /// <summary>
        /// Gets or sets if renderers on this level should use skinned motion vectors.
        /// </summary>
        public bool SkinnedMotionVectors
        {
            get { return skinnedMotionVectors; }
            set { skinnedMotionVectors = value; }
        }

        /// <summary>
        /// Gets or sets the light probe usage for renderers on this level.
        /// </summary>
        public LightProbeUsage LightProbeUsage
        {
            get { return lightProbeUsage; }
            set { lightProbeUsage = value; }
        }

        /// <summary>
        /// Gets or sets the reflection probe usage for renderers on this level.
        /// </summary>
        public ReflectionProbeUsage ReflectionProbeUsage
        {
            get { return reflectionProbeUsage; }
            set { reflectionProbeUsage = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new LOD level.
        /// </summary>
        /// <param name="screenRelativeTransitionHeight">The screen relative height to use for the transition [0-1].</param>
        /// <param name="quality">The quality of this level [0-1].</param>
        public LODLevel(float screenRelativeTransitionHeight, float quality)
            : this(screenRelativeTransitionHeight, 0f, quality, false, false, null)
        {

        }

        /// <summary>
        /// Creates a new LOD level.
        /// </summary>
        /// <param name="screenRelativeTransitionHeight">The screen relative height to use for the transition [0-1].</param>
        /// <param name="fadeTransitionWidth">The width of the cross-fade transition zone (proportion to the current LOD's whole length) [0-1]. Only used if it's not animated.</param>
        /// <param name="quality">The quality of this level [0-1].</param>
        /// <param name="combineMeshes">If all renderers and meshes under this level should be combined into one, where possible.</param>
        /// <param name="combineSubMeshes">If all sub-meshes should be combined into one, where possible.</param>
        public LODLevel(float screenRelativeTransitionHeight, float fadeTransitionWidth, float quality, bool combineMeshes, bool combineSubMeshes)
            : this(screenRelativeTransitionHeight, fadeTransitionWidth, quality, combineMeshes, combineSubMeshes, null)
        {

        }

        /// <summary>
        /// Creates a new LOD level.
        /// </summary>
        /// <param name="screenRelativeTransitionHeight">The screen relative height to use for the transition [0-1].</param>
        /// <param name="fadeTransitionWidth">The width of the cross-fade transition zone (proportion to the current LOD's whole length) [0-1]. Only used if it's not animated.</param>
        /// <param name="quality">The quality of this level [0-1].</param>
        /// <param name="combineMeshes">If all renderers and meshes under this level should be combined into one, where possible.</param>
        /// <param name="combineSubMeshes">If all sub-meshes should be combined into one, where possible.</param>
        /// <param name="renderers">The renderers used in this level.</param>
        public LODLevel(float screenRelativeTransitionHeight, float fadeTransitionWidth, float quality, bool combineMeshes, bool combineSubMeshes, Renderer[] renderers)
        {
            this.screenRelativeTransitionHeight = Mathf.Clamp01(screenRelativeTransitionHeight);
            this.fadeTransitionWidth = fadeTransitionWidth;
            this.quality = Mathf.Clamp01(quality);
            this.combineMeshes = combineMeshes;
            this.combineSubMeshes = combineSubMeshes;

            this.renderers = renderers;

            this.skinQuality = SkinQuality.Auto;
            this.shadowCastingMode = ShadowCastingMode.On;
            this.receiveShadows = true;
            this.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
            this.skinnedMotionVectors = true;
            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        }
        #endregion
    }
}
