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
        private const string LODParentGameObjectName = "_LOD_";
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

            var lodGroup = GenerateLODs(gameObject, levels, autoCollectRenderers);
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
        /// <returns>The generated LOD Group.</returns>
        public static LODGroup GenerateLODs(GameObject gameObject, LODLevel[] levels, bool autoCollectRenderers)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException(nameof(gameObject));
            else if (levels == null)
                throw new System.ArgumentNullException(nameof(levels));

            DestroyLODs(gameObject);

            var transform = gameObject.transform;
            var lodParentGameObject = new GameObject(LODParentGameObjectName);
            var lodParent = lodParentGameObject.transform;
            ParentAndResetTransform(lodParent, transform);

            var lodGroup = gameObject.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = gameObject.AddComponent<LODGroup>();
            }

            Renderer[] allRenderers = (autoCollectRenderers ? gameObject.GetComponentsInChildren<Renderer>() : null);
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

                    if (level.CombineMeshes)
                    {
                        if (meshRenderers.Length > 0)
                        {
                            Material[] combinedMaterials;
                            var combinedMesh = MeshCombiner.CombineMeshes(transform, meshRenderers, out combinedMaterials);
                            combinedMesh.name = string.Format("{0}_static{1:00}", gameObject.name, levelIndex);

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                var simplifiedMesh = SimplifyMesh(combinedMesh, level.Quality);
                                DestroyObject(combinedMesh); // We delete the combined mesh since it's no longer to be used
                                combinedMesh = simplifiedMesh;
                            }

                            // TODO: Save asset file!

                            string rendererName = string.Format("{0}_combined_static", gameObject.name);
                            var combinedRenderer = CreateLevelRenderer(rendererName, levelTransform, combinedMesh, combinedMaterials, ref level);
                            levelRenderers.Add(combinedRenderer);
                        }

                        if (skinnedMeshRenderers.Length > 0)
                        {
                            Material[] combinedMaterials;
                            Transform[] combinedBones;
                            var combinedMesh = MeshCombiner.CombineMeshes(transform, skinnedMeshRenderers, out combinedMaterials, out combinedBones);
                            combinedMesh.name = string.Format("{0}_skinned{1:00}", gameObject.name, levelIndex);

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                var simplifiedMesh = SimplifyMesh(combinedMesh, level.Quality);
                                DestroyObject(combinedMesh); // We delete the combined mesh since it's no longer to be used
                                combinedMesh = simplifiedMesh;
                            }

                            // TODO: Save asset file!

                            var rootBone = FindBestRootBone(transform, skinnedMeshRenderers);
                            string rendererName = string.Format("{0}_combined_skinned", gameObject.name);
                            var combinedRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, combinedMesh, combinedMaterials, rootBone, combinedBones, ref level);
                            levelRenderers.Add(combinedRenderer);
                        }
                    }
                    else
                    {
                        for (int rendererIndex = 0; rendererIndex < meshRenderers.Length; rendererIndex++)
                        {
                            var renderer = meshRenderers[rendererIndex];
                            var meshFilter = renderer.GetComponent<MeshFilter>();
                            if (meshFilter == null)
                                continue;

                            var mesh = meshFilter.sharedMesh;
                            if (mesh == null)
                                continue;

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality);

                                // TODO: Save asset file!
                            }

                            string rendererName = string.Format("{0:000}_static_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateLevelRenderer(rendererName, levelTransform, mesh, renderer.sharedMaterials, ref level);
                            levelRenderers.Add(levelRenderer);
                        }

                        for (int rendererIndex = 0; rendererIndex < skinnedMeshRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedMeshRenderers[rendererIndex];
                            var mesh = renderer.sharedMesh;
                            if (mesh == null)
                                continue;

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality);

                                // TODO: Save asset file!
                            }

                            string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, mesh, renderer.sharedMaterials, renderer.rootBone, renderer.bones, ref level);
                            levelRenderers.Add(levelRenderer);
                        }
                    }
                }

                lods[levelIndex] = new LOD(level.ScreenRelativeTransitionHeight, levelRenderers.ToArray());
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

        private static Mesh SimplifyMesh(Mesh mesh, float quality)
        {
            var meshSimplifier = new MeshSimplifier();
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
        #endregion
    }
}
