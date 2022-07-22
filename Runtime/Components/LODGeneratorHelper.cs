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
    /// <summary>
    /// A LOD (level of detail) generator helper.
    /// </summary>
    [AddComponentMenu("Rendering/LOD Generator Helper")]
    public sealed class LODGeneratorHelper : MonoBehaviour
    {
        #region Fields
        [SerializeField, Tooltip("The LOD Generator preset to use.")]
        private LODGeneratorPreset lodGeneratorPreset = null;

        [SerializeField, Tooltip("Whether to enable customization of preset-derived generation settings.")]
        private bool customizeSettings = true;

        [SerializeField, Tooltip("The fade mode used by the created LOD group.")]
        private LODFadeMode fadeMode = LODFadeMode.None;
        [SerializeField, Tooltip("If the cross-fading should be animated by time.")]
        private bool animateCrossFading = false;

        [SerializeField, Tooltip("If the renderers under this game object and any children should be automatically collected.")]
        private bool autoCollectRenderers = true;

        [SerializeField, Tooltip("The simplification options.")]
        private SimplificationOptions simplificationOptions = SimplificationOptions.Default;

        [SerializeField, Tooltip("The path within the assets directory to save the generated assets. Leave this empty to use the default path.")]
        private string saveAssetsPath = string.Empty;

        [SerializeField, Tooltip("The LOD levels.")]
        private LODLevel[] levels = null;

        [SerializeField]
        private bool isGenerated = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a LOD generator preset. Presets can be used to drive simplification options and levels in a sharable way.
        /// </summary>
        public LODGeneratorPreset LodGeneratorPreset
        {
            get { return lodGeneratorPreset; }
            set { lodGeneratorPreset = value; }
        }

        /// <summary>
        /// Gets or sets if the simplification options and levels should be customizable, versus driven by the specified preset.
        /// </summary>
        public bool CustomizeSettings
        {
            get { return customizeSettings; }
            set { customizeSettings = value; }
        }

        /// <summary>
        /// Gets or sets the fade mode used by the created LOD group.
        /// </summary>
        public LODFadeMode FadeMode
        {
            get { return fadeMode; }
            set
            {
                if (!customizeSettings)
                {
                    fadeMode = value;
                }
                else
                {
                    WarnDisabledCustomization();
                }
            }
        }

        /// <summary>
        /// Gets or sets if the cross-fading should be animated by time. The animation duration
        /// is specified globally as crossFadeAnimationDuration.
        /// </summary>
        public bool AnimateCrossFading
        {
            get { return animateCrossFading; }
            set
            {
                if (!customizeSettings)
                {
                    animateCrossFading = value;
                }
                else
                {
                    WarnDisabledCustomization();
                }
            }
        }

        /// <summary>
        /// Gets or sets if the renderers under this game object and any children should be automatically collected.
        /// </summary>
        public bool AutoCollectRenderers
        {
            get { return autoCollectRenderers; }
            set { autoCollectRenderers = value; }
        }

        /// <summary>
        /// Gets or sets the simplification options.
        /// </summary>
        public SimplificationOptions SimplificationOptions
        {
            get { return simplificationOptions; }
            set
            {
                if (!customizeSettings)
                {
                    simplificationOptions = value;
                }
                else
                {
                    WarnDisabledCustomization();
                }
            }
        }

        /// <summary>
        /// Gets or sets the path within the project to save the generated assets.
        /// Leave this empty to use the default path.
        /// </summary>
        public string SaveAssetsPath
        {
            get { return saveAssetsPath; }
            set { saveAssetsPath = value; }
        }

        /// <summary>
        /// Gets or sets the LOD levels for this generator.
        /// </summary>
        public LODLevel[] Levels
        {
            get { return levels; }
            set
            {
                if (!customizeSettings)
                {
                    levels = value;
                }
                else
                {
                    WarnDisabledCustomization();
                }
            }
        }

        /// <summary>
        /// Gets if the LODs have been generated.
        /// </summary>
        public bool IsGenerated
        {
            get { return isGenerated; }
        }
        #endregion

        #region Unity Events
        private void Reset()
        {
            autoCollectRenderers = true;
            ResetPresetDerivedSettings();
        }

        private void OnValidate()
        {
            if (!customizeSettings)
            {
                UpdateSettingsFromPreset();
            }
        }
        #endregion

        private void WarnDisabledCustomization()
        {
            Debug.LogWarning($"Attempted to set a preset-driven property on a {typeof(LODGeneratorHelper)} while customization is disabled. Enable customization first.");
        }

        private void ResetPresetDerivedSettings()
        {
            fadeMode = LODFadeMode.None;
            animateCrossFading = false;
            simplificationOptions = SimplificationOptions.Default;
            levels = LODLevel.GetDefaultLevels();
        }

        private void UpdateSettingsFromPreset()
        {
            // Retain copy of levels so any specified renderers can survive reset
            LODLevel[] previousLevels = (LODLevel[])levels.Clone();

            // Copy settings from preset, or use defaults if no preset specified
            if (lodGeneratorPreset != null)
            {
                fadeMode = lodGeneratorPreset.FadeMode;
                animateCrossFading = lodGeneratorPreset.AnimateCrossFading;
                simplificationOptions = lodGeneratorPreset.SimplificationOptions;
                levels = (LODLevel[])lodGeneratorPreset.Levels.Clone();
            }
            else
            {
                ResetPresetDerivedSettings();
            }

            // Copy specified renderers over
            int rendererCopyCount = Mathf.Min(levels.Length, previousLevels.Length);
            for(int idx = 0; idx < rendererCopyCount; idx++)
            {
                levels[idx].Renderers = (Renderer[])previousLevels[idx].Renderers.Clone();
            }
        }
    }
}
