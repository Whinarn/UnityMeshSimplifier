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
        #region Consts
        private const string LODParentGameObjectName = "_UMS_LODs_";
        #endregion

        #region Structs
        private struct StaticRenderer
        {
            public string name;
            public bool isNewMesh;
            public Mesh mesh;
            public Material[] materials;
        }

        private struct SkinnedRenderer
        {
            public string name;
            public bool isNewMesh;
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

            var lodGroup = GenerateLODs(gameObject, levels, autoCollectRenderers, simplificationOptions);
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
            if (gameObject == null)
                throw new System.ArgumentNullException(nameof(gameObject));
            else if (levels == null)
                throw new System.ArgumentNullException(nameof(levels));


            var transform = gameObject.transform;
            var existingLodParent = transform.Find(LODParentGameObjectName);
            if (existingLodParent != null)
            {
                DisplayError("The game object already has LODs!", "The game object already appears to have LODs. Please remove them first.", "OK", existingLodParent);
                return null;
            }

            var existingLodGroup = gameObject.GetComponent<LODGroup>();
            if (existingLodGroup != null)
            {
                DisplayError("The game object already has LODs!", "The game object already appears to have a LOD Group. Please remove it first.", "OK", existingLodGroup);
                return null;
            }

            var lodParentGameObject = new GameObject(LODParentGameObjectName);
            var lodParent = lodParentGameObject.transform;
            ParentAndResetTransform(lodParent, transform);

            var lodGroup = gameObject.AddComponent<LODGroup>();

            Renderer[] allRenderers = (autoCollectRenderers ? gameObject.GetComponentsInChildren<Renderer>() : null);
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

                if (originalLevelRenderers != null)
                {
                    var meshRenderers = (from renderer in originalLevelRenderers
                                        where renderer as MeshRenderer != null
                                        select renderer as MeshRenderer).ToArray();
                    var skinnedMeshRenderers = (from renderer in originalLevelRenderers
                                                where renderer as SkinnedMeshRenderer != null
                                                select renderer as SkinnedMeshRenderer).ToArray();

                    StaticRenderer[] staticRenderers;
                    SkinnedRenderer[] skinnedRenderers;
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
                            var mesh = renderer.mesh;

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality, simplificationOptions);
                                SaveAsset(mesh);

                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }

                            string rendererName = string.Format("{0:000}_static_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateLevelRenderer(rendererName, levelTransform, mesh, renderer.materials, ref level);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    if (skinnedRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedRenderers[rendererIndex];
                            var mesh = renderer.mesh;

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality, simplificationOptions);
                                SaveAsset(mesh);

                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }

                            string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, mesh, renderer.materials, renderer.rootBone, renderer.bones, ref level);
                            levelRenderers.Add(levelRenderer);
                        }
                    }
                }

                foreach (var renderer in originalLevelRenderers)
                {
                    if (!renderersToDisable.Contains(renderer))
                    {
                        renderersToDisable.Add(renderer);
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
        /// Destroys the generated LODs and LOD Group for the specified game object.
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

            // Destroy the LOD parent
            DestroyObject(lodParent.gameObject);

            // Destroy the LOD Group if there is one
            var lodGroup = gameObject.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                DestroyObject(lodGroup);
            }

            // TODO: Clean up asset files?

            return true;
        }
        #endregion

        #region Private Methods
        private static StaticRenderer[] GetStaticRenderers(MeshRenderer[] renderers)
        {
            var newRenderers = new List<StaticRenderer>(renderers.Length);
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

                newRenderers.Add(new StaticRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    mesh = mesh,
                    materials = renderer.sharedMaterials
                });
            }
            return newRenderers.ToArray();
        }

        private static SkinnedRenderer[] GetSkinnedRenderers(SkinnedMeshRenderer[] renderers)
        {
            var newRenderers = new List<SkinnedRenderer>(renderers.Length);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];

                var mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new SkinnedRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    mesh = mesh,
                    materials = renderer.sharedMaterials,
                    rootBone = renderer.rootBone,
                    bones = renderer.bones
                });
            }
            return newRenderers.ToArray();
        }

        private static StaticRenderer[] CombineStaticMeshes(Transform transform, int levelIndex, MeshRenderer[] renderers)
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<StaticRenderer>(renderers.Length);

            Material[] combinedMaterials;
            var combinedMesh = MeshCombiner.CombineMeshes(transform, renderers, out combinedMaterials);
            combinedMesh.name = string.Format("{0}_static{1:00}", transform.name, levelIndex);
            string rendererName = string.Format("{0}_combined_static", transform.name);
            newRenderers.Add(new StaticRenderer()
            {
                name = rendererName,
                isNewMesh = true,
                mesh = combinedMesh,
                materials = combinedMaterials
            });

            return newRenderers.ToArray();
        }

        private static SkinnedRenderer[] CombineSkinnedMeshes(Transform transform, int levelIndex, SkinnedMeshRenderer[] renderers)
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<SkinnedRenderer>(renderers.Length);
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
                newRenderers.Add(new SkinnedRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
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
                newRenderers.Add(new SkinnedRenderer()
                {
                    name = rendererName,
                    isNewMesh = false,
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

        private static MeshRenderer CreateLevelRenderer(string name, Transform parentTransform, Mesh mesh, Material[] materials, ref LODLevel level)
        {
            var gameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            ParentAndResetTransform(gameObject.transform, parentTransform);

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            SetupLevelRenderer(meshRenderer, ref level);
            return meshRenderer;
        }

        private static SkinnedMeshRenderer CreateSkinnedLevelRenderer(string name, Transform parentTransform, Mesh mesh, Material[] materials, Transform rootBone, Transform[] bones, ref LODLevel level)
        {
            var gameObject = new GameObject(name, typeof(SkinnedMeshRenderer));
            ParentAndResetTransform(gameObject.transform, parentTransform);

            var skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;
            SetupLevelRenderer(skinnedMeshRenderer, ref level);
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

        private static void SetupLevelRenderer(Renderer renderer, ref LODLevel level)
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

        private static Mesh SimplifyMesh(Mesh mesh, float quality, SimplificationOptions options)
        {
            var meshSimplifier = new MeshSimplifier();
            meshSimplifier.PreserveBorderEdges = options.PreserveBorderEdges;
            meshSimplifier.PreserveUVSeamEdges = options.PreserveUVSeamEdges;
            meshSimplifier.PreserveUVFoldoverEdges = options.PreserveUVFoldoverEdges;
            meshSimplifier.EnableSmartLink = options.EnableSmartLink;
            meshSimplifier.VertexLinkDistance = options.VertexLinkDistance;
            meshSimplifier.MaxIterationCount = options.MaxIterationCount;
            meshSimplifier.Agressiveness = options.Agressiveness;

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

            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
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
                        renderer.enabled = true;
                    }
                }
                DestroyObject(backupComponent);
            }
        }

        private static void SaveAsset(Object asset)
        {
            // TODO: Save asset!
        }

        private static void DisplayError(string title, string message, string ok, Object context)
        {
            Debug.LogErrorFormat(context, "{0}\n{1}", title, message);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(title, message, ok);
#endif
        }
        #endregion
    }
}
