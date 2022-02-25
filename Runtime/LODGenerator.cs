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

namespace UnityMeshSimplifier
{
    /// <summary>
    /// Contains methods for generating LODs (level of details) for game objects.
    /// </summary>
    public static class LODGenerator
    {
        #region Static Read-Only
        /// <summary>
        /// The name of the game object where generated LODs are parented under.
        /// </summary>
        public static readonly string LODParentGameObjectName = "_UMS_LODs_";

        /// <summary>
        /// The default parent path for generated LOD assets.
        /// </summary>
        public static readonly string LODAssetDefaultParentPath = "Assets/UMS_LODs/";

        /// <summary>
        /// The root assets path.
        /// </summary>
        public static readonly string AssetsRootPath = "Assets/";

        /// <summary>
        /// The user data applied to created LOD assets.
        /// </summary>
        public static readonly string LODAssetUserData = "UnityMeshSimplifierLODAsset";
        #endregion

        #region Nested Types
        private struct RendererInfo
        {
            public string name;
            public bool isStatic;
            public bool isNewMesh;
            public Transform transform;
            public Mesh mesh;
            public Material[] materials;
            public Transform rootBone;
            public Transform[] bones;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates the LODs and sets up a LOD Group for the LOD generator helper component.
        /// </summary>
        /// <param name="generatorHelper">The LOD generator helper.</param>
        /// <returns>The generated LOD Group.</returns>
        public static LODGroup GenerateLODs(LODGeneratorHelper generatorHelper)
        {
            if (generatorHelper == null)
                throw new System.ArgumentNullException(nameof(generatorHelper));

            var gameObject = generatorHelper.gameObject;
            var levels = generatorHelper.Levels;
            bool autoCollectRenderers = generatorHelper.AutoCollectRenderers;
            var simplificationOptions = generatorHelper.SimplificationOptions;
            string saveAssetsPath = generatorHelper.SaveAssetsPath;

            var lodGroup = GenerateLODs(gameObject, levels, autoCollectRenderers, simplificationOptions, saveAssetsPath);
            if (lodGroup == null)
                return null;

            lodGroup.animateCrossFading = generatorHelper.AnimateCrossFading;
            lodGroup.fadeMode = generatorHelper.FadeMode;
            return lodGroup;
        }

        /// <summary>
        /// Generates the LODs and sets up a LOD Group for the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to set up.</param>
        /// <param name="levels">The LOD levels to set up.</param>
        /// <param name="autoCollectRenderers">If the renderers under the game object and any children should be automatically collected.
        /// Enabling this will ignore any renderers defined under each LOD level.</param>
        /// <param name="simplificationOptions">The mesh simplification options.</param>
        /// <returns>The generated LOD Group.</returns>
        public static LODGroup GenerateLODs(GameObject gameObject, LODLevel[] levels, bool autoCollectRenderers, SimplificationOptions simplificationOptions)
        {
            return GenerateLODs(gameObject, levels, autoCollectRenderers, simplificationOptions, null);
        }

        /// <summary>
        /// Generates the LODs and sets up a LOD Group for the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to set up.</param>
        /// <param name="levels">The LOD levels to set up.</param>
        /// <param name="autoCollectRenderers">If the renderers under the game object and any children should be automatically collected.
        /// Enabling this will ignore any renderers defined under each LOD level.</param>
        /// <param name="simplificationOptions">The mesh simplification options.</param>
        /// <param name="saveAssetsPath">The path to where the generated assets should be saved. Can be null or empty to use the default path.</param>
        /// <returns>The generated LOD Group.</returns>
        public static LODGroup GenerateLODs(GameObject gameObject, LODLevel[] levels, bool autoCollectRenderers, SimplificationOptions simplificationOptions, string saveAssetsPath)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException(nameof(gameObject));
            else if (levels == null)
                throw new System.ArgumentNullException(nameof(levels));

            var transform = gameObject.transform;
            var existingLodParent = transform.Find(LODParentGameObjectName);
            if (existingLodParent != null)
                throw new System.InvalidOperationException("The game object already appears to have LODs. Please remove them first.");

            var existingLodGroup = gameObject.GetComponent<LODGroup>();
            if (existingLodGroup != null)
                throw new System.InvalidOperationException("The game object already appears to have a LOD Group. Please remove it first.");

            MeshSimplifier.ValidateOptions(simplificationOptions);

            saveAssetsPath = ValidateSaveAssetsPath(saveAssetsPath);

            var lodParentGameObject = new GameObject(LODParentGameObjectName);
            var lodParent = lodParentGameObject.transform;
            ParentAndResetTransform(lodParent, transform);

            var lodGroup = gameObject.AddComponent<LODGroup>();

            Renderer[] allRenderers = null;
            if (autoCollectRenderers)
            {
                // Collect all enabled renderers under the game object
                allRenderers = GetChildRenderersForLOD(gameObject);
            }

            var renderersToDisable = new List<Renderer>((allRenderers != null ? allRenderers.Length : 10));
            var lods = new LOD[levels.Length];
            for (int levelIndex = 0; levelIndex < levels.Length; levelIndex++)
            {
                var level = levels[levelIndex];
                var levelGameObject = new GameObject(string.Format("Level{0:00}", levelIndex));
                var levelTransform = levelGameObject.transform;
                ParentAndResetTransform(levelTransform, lodParent);

                Renderer[] originalLevelRenderers = allRenderers ?? level.Renderers;
                var levelRenderers = new List<Renderer>((originalLevelRenderers != null ? originalLevelRenderers.Length : 0));

                if (originalLevelRenderers != null && originalLevelRenderers.Length > 0)
                {
                    var meshRenderers = (from renderer in originalLevelRenderers
                                         let meshFilter = renderer.GetComponent<MeshFilter>()
                                         where renderer.enabled && renderer as MeshRenderer != null
                                         && meshFilter != null
                                         && meshFilter.sharedMesh != null
                                         select renderer as MeshRenderer).ToArray();
                    var skinnedMeshRenderers = (from renderer in originalLevelRenderers
                                                where renderer.enabled && renderer as SkinnedMeshRenderer != null
                                                && (renderer as SkinnedMeshRenderer).sharedMesh != null
                                                select renderer as SkinnedMeshRenderer).ToArray();

                    RendererInfo[] staticRenderers;
                    RendererInfo[] skinnedRenderers;
                    if (level.CombineMeshes)
                    {
                        staticRenderers = CombineStaticMeshes(transform, levelIndex, meshRenderers);
                        skinnedRenderers = CombineSkinnedMeshes(transform, levelIndex, skinnedMeshRenderers);
                    }
                    else
                    {
                        staticRenderers = GetStaticRenderers(meshRenderers);
                        skinnedRenderers = GetSkinnedRenderers(skinnedMeshRenderers);
                    }

                    if (staticRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < staticRenderers.Length; rendererIndex++)
                        {
                            var renderer = staticRenderers[rendererIndex];
                            var levelRenderer = CreateLevelRenderer(gameObject, levelIndex, level, levelTransform, rendererIndex, renderer, simplificationOptions, saveAssetsPath);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    if (skinnedRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedRenderers[rendererIndex];
                            var levelRenderer = CreateLevelRenderer(gameObject, levelIndex, level, levelTransform, rendererIndex, renderer, simplificationOptions, saveAssetsPath);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    foreach (var renderer in originalLevelRenderers)
                    {
                        if (!renderersToDisable.Contains(renderer))
                        {
                            renderersToDisable.Add(renderer);
                        }
                    }
                }

                lods[levelIndex] = new LOD(level.ScreenRelativeTransitionHeight, levelRenderers.ToArray());
            }

            CreateBackup(gameObject, renderersToDisable.ToArray());
            foreach (var renderer in renderersToDisable)
            {
                renderer.enabled = false;
            }

            lodGroup.animateCrossFading = false;
            lodGroup.SetLODs(lods);
            return lodGroup;
        }

        /// <summary>
        /// Destroys the generated LODs and LOD Group for the LOD generator helper component.
        /// </summary>
        /// <param name="generatorHelper">The LOD generator helper.</param>
        /// <returns>If the LODs were successfully destroyed.</returns>
        public static bool DestroyLODs(LODGeneratorHelper generatorHelper)
        {
            if (generatorHelper == null)
                throw new System.ArgumentNullException(nameof(generatorHelper));

            return DestroyLODs(generatorHelper.gameObject);
        }

        /// <summary>
        /// Destroys the generated LODs and LOD Group for the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to destroy LODs for.</param>
        /// <returns>If the LODs were successfully destroyed.</returns>
        public static bool DestroyLODs(GameObject gameObject)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException(nameof(gameObject));

            RestoreBackup(gameObject);

            var transform = gameObject.transform;
            var lodParent = transform.Find(LODParentGameObjectName);
            if (lodParent == null)
                return false;

#if UNITY_EDITOR
            // Destroy LOD assets
            DestroyLODAssets(lodParent);
#endif

            // Destroy the LOD parent
            DestroyObject(lodParent.gameObject);

            // Destroy the LOD Group if there is one
            var lodGroup = gameObject.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                DestroyObject(lodGroup);
            }

            return true;
        }
        #endregion

        #region Private Methods
        private static RendererInfo[] GetStaticRenderers(MeshRenderer[] renderers)
        {
            var newRenderers = new List<RendererInfo>(renderers.Length);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh filter and was ignored.", renderer);
                    continue;
                }

                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new RendererInfo
                {
                    name = renderer.name,
                    isStatic = true,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials
                });
            }
            return newRenderers.ToArray();
        }

        private static RendererInfo[] GetSkinnedRenderers(SkinnedMeshRenderer[] renderers)
        {
            var newRenderers = new List<RendererInfo>(renderers.Length);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];

                var mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new RendererInfo
                {
                    name = renderer.name,
                    isStatic = false,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials,
                    rootBone = renderer.rootBone,
                    bones = renderer.bones
                });
            }
            return newRenderers.ToArray();
        }

        private static RendererInfo[] CombineStaticMeshes(Transform transform, int levelIndex, MeshRenderer[] renderers)
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<RendererInfo>(renderers.Length);

            Material[] combinedMaterials;
            var combinedMesh = MeshCombiner.CombineMeshes(transform, renderers, out combinedMaterials);
            combinedMesh.name = string.Format("{0}_static{1:00}", transform.name, levelIndex);
            string rendererName = string.Format("{0}_combined_static", transform.name);
            newRenderers.Add(new RendererInfo
            {
                name = rendererName,
                isStatic = true,
                isNewMesh = true,
                transform = null,
                mesh = combinedMesh,
                materials = combinedMaterials,
                rootBone = null,
                bones = null
            });

            return newRenderers.ToArray();
        }

        private static RendererInfo[] CombineSkinnedMeshes(Transform transform, int levelIndex, SkinnedMeshRenderer[] renderers)
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<RendererInfo>(renderers.Length);
            var blendShapeRenderers = (from renderer in renderers
                                       where renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0
                                       select renderer);
            var renderersWithoutMesh = (from renderer in renderers
                                        where renderer.sharedMesh == null
                                        select renderer);
            var combineRenderers = (from renderer in renderers
                                    where renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount == 0
                                    select renderer).ToArray();

            // Warn about renderers without a mesh
            foreach (var renderer in renderersWithoutMesh)
            {
                Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
            }

            // Don't combine meshes with blend shapes
            foreach (var renderer in blendShapeRenderers)
            {
                newRenderers.Add(new RendererInfo
                {
                    name = renderer.name,
                    isStatic = false,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = renderer.sharedMesh,
                    materials = renderer.sharedMaterials,
                    rootBone = renderer.rootBone,
                    bones = renderer.bones
                });
            }

            if (combineRenderers.Length > 0)
            {
                Material[] combinedMaterials;
                Transform[] combinedBones;
                var combinedMesh = MeshCombiner.CombineMeshes(transform, combineRenderers, out combinedMaterials, out combinedBones);
                combinedMesh.name = string.Format("{0}_skinned{1:00}", transform.name, levelIndex);

                var rootBone = FindBestRootBone(transform, combineRenderers);
                string rendererName = string.Format("{0}_combined_skinned", transform.name);
                newRenderers.Add(new RendererInfo
                {
                    name = rendererName,
                    isStatic = false,
                    isNewMesh = false,
                    transform = null,
                    mesh = combinedMesh,
                    materials = combinedMaterials,
                    rootBone = rootBone,
                    bones = combinedBones
                });
            }

            return newRenderers.ToArray();
        }

        private static void ParentAndResetTransform(Transform transform, Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static void ParentAndOffsetTransform(Transform transform, Transform parentTransform, Transform originalTransform)
        {
            transform.position = originalTransform.position;
            transform.rotation = originalTransform.rotation;
            transform.localScale = originalTransform.lossyScale;
            transform.SetParent(parentTransform, true);
        }

        private static Renderer CreateLevelRenderer(GameObject gameObject, int levelIndex, in LODLevel level, Transform levelTransform, int rendererIndex, in RendererInfo renderer, in SimplificationOptions simplificationOptions, string saveAssetsPath)
        {
            var mesh = renderer.mesh;

            // Simplify the mesh if necessary
            if (level.Quality < 1f)
            {
                mesh = SimplifyMesh(mesh, level.Quality, simplificationOptions);

#if UNITY_EDITOR
                SaveLODMeshAsset(mesh, gameObject.name, renderer.name, levelIndex, mesh.name, saveAssetsPath);
#endif

                if (renderer.isNewMesh)
                {
                    DestroyObject(renderer.mesh);
                }
            }

            if (renderer.isStatic)
            {
                string rendererName = string.Format("{0:000}_static_{1}", rendererIndex, renderer.name);
                return CreateStaticLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials, level);
            }
            else
            {
                string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, renderer.name);
                return CreateSkinnedLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials, renderer.rootBone, renderer.bones, level);
            }
        }

        private static MeshRenderer CreateStaticLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, in LODLevel level)
        {
            var levelGameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            var levelTransform = levelGameObject.transform;
            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var meshFilter = levelGameObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = levelGameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            SetupLevelRenderer(meshRenderer, level);
            return meshRenderer;
        }

        private static SkinnedMeshRenderer CreateSkinnedLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, Transform rootBone, Transform[] bones, in LODLevel level)
        {
            var levelGameObject = new GameObject(name, typeof(SkinnedMeshRenderer));
            var levelTransform = levelGameObject.transform;
            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var skinnedMeshRenderer = levelGameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;
            SetupLevelRenderer(skinnedMeshRenderer, level);
            return skinnedMeshRenderer;
        }

        private static Transform FindBestRootBone(Transform transform, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return null;

            Transform bestBone = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || skinnedMeshRenderers[i].rootBone == null)
                    continue;

                var rootBone = skinnedMeshRenderers[i].rootBone;
                var distance = (rootBone.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestBone = rootBone;
                    bestDistance = distance;
                }
            }

            return bestBone;
        }

        private static void SetupLevelRenderer(Renderer renderer, in LODLevel level)
        {
            renderer.shadowCastingMode = level.ShadowCastingMode;
            renderer.receiveShadows = level.ReceiveShadows;
            renderer.motionVectorGenerationMode = level.MotionVectorGenerationMode;
            renderer.lightProbeUsage = level.LightProbeUsage;
            renderer.reflectionProbeUsage = level.ReflectionProbeUsage;

            var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.quality = level.SkinQuality;
                skinnedMeshRenderer.skinnedMotionVectors = level.SkinnedMotionVectors;
            }
        }

        private static Renderer[] GetChildRenderersForLOD(GameObject gameObject)
        {
            var resultRenderers = new List<Renderer>();
            CollectChildRenderersForLOD(gameObject.transform, resultRenderers);
            return resultRenderers.ToArray();
        }

        private static void CollectChildRenderersForLOD(Transform transform, List<Renderer> resultRenderers)
        {
            // Collect the rendererers of this transform
            var childRenderers = transform.GetComponents<Renderer>();
            resultRenderers.AddRange(childRenderers);

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                // Skip children that are not active
                var childTransform = transform.GetChild(i);
                if (!childTransform.gameObject.activeSelf)
                    continue;

                // If the transform have the identical name as to our LOD Parent GO name, then we also skip it
                if (string.Equals(childTransform.name, LODParentGameObjectName))
                    continue;

                // Skip children that has a LOD Group or a LOD Generator Helper component
                if (childTransform.GetComponent<LODGroup>() != null)
                    continue;
                else if (childTransform.GetComponent<LODGeneratorHelper>() != null)
                    continue;

                // Continue recursively through the children of this transform
                CollectChildRenderersForLOD(childTransform, resultRenderers);
            }
        }

        private static Mesh SimplifyMesh(Mesh mesh, float quality, in SimplificationOptions options)
        {
            var meshSimplifier = new MeshSimplifier();
            meshSimplifier.SimplificationOptions = options;
            meshSimplifier.Initialize(mesh);
            meshSimplifier.SimplifyMesh(quality);

            var simplifiedMesh = meshSimplifier.ToMesh();
            simplifiedMesh.bindposes = mesh.bindposes;
            return simplifiedMesh;
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException(nameof(obj));

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj, false);
            }
#else
            Object.Destroy(obj);
#endif
        }

        private static void CreateBackup(GameObject gameObject, Renderer[] originalRenderers)
        {
            var backupComponent = gameObject.AddComponent<LODBackupComponent>();
            backupComponent.hideFlags = HideFlags.HideInInspector;
            backupComponent.OriginalRenderers = originalRenderers;
        }

        private static void RestoreBackup(GameObject gameObject)
        {
            var backupComponents = gameObject.GetComponents<LODBackupComponent>();
            foreach (var backupComponent in backupComponents)
            {
                var originalRenderers = backupComponent.OriginalRenderers;
                if (originalRenderers != null)
                {
                    foreach (var renderer in originalRenderers)
                    {
                        if (renderer != null)
                        {
                            renderer.enabled = true;
                        }
                    }
                }
                DestroyObject(backupComponent);
            }
        }

        private static string ValidateSaveAssetsPath(string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(saveAssetsPath))
                return null;

#if UNITY_EDITOR
            return IOUtils.MakeSafeRelativePath(saveAssetsPath);
#else
            Debug.LogWarning("Unable to save assets when not running in the Unity Editor.");
            return null;
#endif
        }

        #region Editor Functions
#if UNITY_EDITOR
        internal static string GetFinalSaveAssetsPath(string gameObjectName, string rendererName, string saveAssetsPath)
        {
            if (!string.IsNullOrEmpty(saveAssetsPath))
            {
                return string.Format("{0}{1}", AssetsRootPath, saveAssetsPath);
            }
            else
            {
                // If there is no save assets path, we create a default one
                return string.Format("{0}{1}/{2}", LODAssetDefaultParentPath, gameObjectName, rendererName);
            }
        }

        private static void SaveLODMeshAsset(Object asset, string gameObjectName, string rendererName, int levelIndex, string meshName, string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(meshName))
                meshName = "unnamed";

            gameObjectName = IOUtils.MakeSafeFileName(gameObjectName);
            rendererName = IOUtils.MakeSafeFileName(rendererName);
            meshName = IOUtils.MakeSafeFileName(meshName);
            meshName = string.Format("{0:00}_{1}", levelIndex, meshName);

            string finalSaveAssetsPath = GetFinalSaveAssetsPath(gameObjectName, rendererName, saveAssetsPath);
            string saveAssetPath = string.Format("{0}/{1}.mesh", finalSaveAssetsPath, meshName);
            SaveAsset(asset, saveAssetPath);
        }

        private static void SaveAsset(Object asset, string path)
        {
            IOUtils.CreateParentDirectory(path);

            // Make sure that there is no asset with the same path already
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);

            UnityEditor.AssetDatabase.CreateAsset(asset, path);

            var assetImporter = UnityEditor.AssetImporter.GetAtPath(path);
            if (assetImporter != null)
            {
                assetImporter.userData = LODAssetUserData;
                assetImporter.SaveAndReimport();
            }
            else
            {
                Debug.LogWarningFormat(asset, "Could not find asset importer for recently created asset, so could not mark it properly: {0}", path);
            }
        }

        private static void DestroyLODAssets(Transform transform)
        {
            var renderers = transform.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var meshRenderer = renderer as MeshRenderer;
                var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;

                if (meshRenderer != null)
                {
                    var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        DestroyLODAsset(meshFilter.sharedMesh);
                    }
                }
                else if (skinnedMeshRenderer != null)
                {
                    DestroyLODAsset(skinnedMeshRenderer.sharedMesh);
                }

                foreach (var material in renderer.sharedMaterials)
                {
                    DestroyLODMaterialAsset(material);
                }
            }

            // Delete any empty LOD asset directories
            IOUtils.DeleteEmptyDirectory(LODAssetDefaultParentPath.TrimEnd('/'));
        }

        private static void DestroyLODMaterialAsset(Material material)
        {
            if (material == null)
                return;

            var shader = material.shader;
            if (shader == null)
                return;

            // We find all texture properties of materials and delete those assets also
            int propertyCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                var propertyType = UnityEditor.ShaderUtil.GetPropertyType(shader, propertyIndex);
                if (propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, propertyIndex);
                    var texture = material.GetTexture(propertyName);
                    DestroyLODAsset(texture);
                }
            }

            DestroyLODAsset(material);
        }

        private static void DestroyLODAsset(Object asset)
        {
            if (asset == null)
                return;

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                return;

            var assetImporter = UnityEditor.AssetImporter.GetAtPath(assetPath);
            if (assetImporter == null)
                return;

            // We only delete assets that we have automatically generated
            if (string.Equals(assetImporter.userData, LODAssetUserData))
            {
                UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            }
        }
#endif
        #endregion
        #endregion
    }
}
