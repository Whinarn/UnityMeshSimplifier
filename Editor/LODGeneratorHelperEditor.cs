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
using UnityEditor;

namespace UnityMeshSimplifier.Editor
{
    [CustomEditor(typeof(LODGeneratorHelper))]
    internal sealed class LODGeneratorHelperEditor : UnityEditor.Editor
    {
        private const string FadeModeFieldName = "fadeMode";
        private const string AnimateCrossFadingFieldName = "animateCrossFading";
        private const string AutoCollectRenderersFieldName = "autoCollectRenderers";
        private const string SimplificationOptionsFieldName = "simplificationOptions";
        private const string SaveAssetsPathFieldName = "saveAssetsPath";
        private const string LevelsFieldName = "levels";
        private const string IsGeneratedFieldName = "isGenerated";
        private const string LevelScreenRelativeHeightFieldName = "screenRelativeTransitionHeight";
        private const string LevelFadeTransitionWidthFieldName = "fadeTransitionWidth";
        private const string LevelQualityFieldName = "quality";
        private const string LevelCombineMeshesFieldName = "combineMeshes";
        private const string LevelCombineSubMeshesFieldName = "combineSubMeshes";
        private const string LevelRenderersFieldName = "renderers";
        private const string SimplificationOptionsEnableSmartLinkFieldName = "EnableSmartLink";
        private const string SimplificationOptionsVertexLinkDistanceFieldName = "VertexLinkDistance";
        private const float RemoveLevelButtonSize = 20f;
        private const float RendererButtonWidth = 60f;
        private const float RemoveRendererButtonSize = 20f;

        private SerializedProperty fadeModeProperty = null;
        private SerializedProperty animateCrossFadingProperty = null;
        private SerializedProperty autoCollectRenderersProperty = null;
        private SerializedProperty simplificationOptionsProperty = null;
        private SerializedProperty saveAssetsPathProperty = null;
        private SerializedProperty levelsProperty = null;
        private SerializedProperty isGeneratedProperty = null;

        private bool overrideSaveAssetsPath = false;
        private bool[] settingsExpanded = null;
        private LODGeneratorHelper lodGeneratorHelper = null;

        private static readonly GUIContent createLevelButtonContent = new GUIContent("Create Level", "Creates a new LOD level.");
        private static readonly GUIContent deleteLevelButtonContent = new GUIContent("X", "Deletes this LOD level.");
        private static readonly GUIContent generateLODButtonContent = new GUIContent("Generate LODs", "Generates the LOD levels.");
        private static readonly GUIContent destroyLODButtonContent = new GUIContent("Destroy LODs", "Destroys the LOD levels.");
        private static readonly GUIContent settingsContent = new GUIContent("Settings", "The settings for the LOD level.");
        private static readonly GUIContent renderersHeaderContent = new GUIContent("Renderers:", "The renderers used for this LOD level.");
        private static readonly GUIContent removeRendererButtonContent = new GUIContent("X", "Removes this renderer.");
        private static readonly GUIContent addRendererButtonContent = new GUIContent("Add", "Adds a renderer to this LOD level.");
        private static readonly GUIContent overrideSaveAssetsPathContent = new GUIContent("Override Save Assets Path", "If you want to override the path where the generated assets are saved.");
        private static readonly Color removeColor = new Color(1f, 0.6f, 0.6f, 1f);

        private static readonly int ObjectPickerControlID = "LODGeneratorSelector".GetHashCode();

        private void OnEnable()
        {
            fadeModeProperty = serializedObject.FindProperty(FadeModeFieldName);
            animateCrossFadingProperty = serializedObject.FindProperty(AnimateCrossFadingFieldName);
            autoCollectRenderersProperty = serializedObject.FindProperty(AutoCollectRenderersFieldName);
            simplificationOptionsProperty = serializedObject.FindProperty(SimplificationOptionsFieldName);
            saveAssetsPathProperty = serializedObject.FindProperty(SaveAssetsPathFieldName);
            levelsProperty = serializedObject.FindProperty(LevelsFieldName);
            isGeneratedProperty = serializedObject.FindProperty(IsGeneratedFieldName);

            overrideSaveAssetsPath = (saveAssetsPathProperty.stringValue.Length > 0);
            lodGeneratorHelper = target as LODGeneratorHelper;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            bool isGenerated = isGeneratedProperty.boolValue;
            if (isGenerated)
            {
                DrawGeneratedView();
            }
            else
            {
                DrawNotGeneratedView();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneratedView()
        {
            if (GUILayout.Button(destroyLODButtonContent))
            {
                DestroyLODs();
            }
        }

        private void DrawNotGeneratedView()
        {
            EditorGUILayout.PropertyField(fadeModeProperty);
            var fadeMode = (LODFadeMode)fadeModeProperty.intValue;

            bool hasCrossFade = (fadeMode == LODFadeMode.CrossFade || fadeMode == LODFadeMode.SpeedTree);
            if (hasCrossFade)
            {
                EditorGUILayout.PropertyField(animateCrossFadingProperty);
            }

            EditorGUILayout.PropertyField(autoCollectRenderersProperty);
            DrawSimplificationOptions();

            bool newHasSaveAssetsPath = EditorGUILayout.Toggle(overrideSaveAssetsPathContent, overrideSaveAssetsPath);
            if (newHasSaveAssetsPath != overrideSaveAssetsPath)
            {
                overrideSaveAssetsPath = newHasSaveAssetsPath;
                saveAssetsPathProperty.stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }

            if (overrideSaveAssetsPath)
            {
                EditorGUILayout.PropertyField(saveAssetsPathProperty);
            }

            if (settingsExpanded == null || settingsExpanded.Length != levelsProperty.arraySize)
            {
                var newSettingsExpanded = new bool[levelsProperty.arraySize];
                if (settingsExpanded != null)
                {
                    System.Array.Copy(settingsExpanded, 0, newSettingsExpanded, 0, Mathf.Min(settingsExpanded.Length, newSettingsExpanded.Length));
                }
                settingsExpanded = newSettingsExpanded;
            }

            for (int levelIndex = 0; levelIndex < levelsProperty.arraySize; levelIndex++)
            {
                var levelProperty = levelsProperty.GetArrayElementAtIndex(levelIndex);
                DrawLevel(levelIndex, levelProperty, hasCrossFade);
            }

            if (GUILayout.Button(createLevelButtonContent))
            {
                CreateLevel();
            }

            if (GUILayout.Button(generateLODButtonContent))
            {
                GenerateLODs();
            }
        }

        private void DrawSimplificationOptions()
        {
            if (EditorGUILayout.PropertyField(simplificationOptionsProperty, false))
            {
                ++EditorGUI.indentLevel;

                var enableSmartLinkProperty = simplificationOptionsProperty.FindPropertyRelative(SimplificationOptionsEnableSmartLinkFieldName);

                var childProperties = simplificationOptionsProperty.GetChildProperties();
                foreach (var childProperty in childProperties)
                {
                    if (!enableSmartLinkProperty.boolValue && string.Equals(childProperty.name, SimplificationOptionsVertexLinkDistanceFieldName))
                        continue;

                    EditorGUILayout.PropertyField(childProperty, true);
                }

                --EditorGUI.indentLevel;
            }
        }

        private void DrawLevel(int index, SerializedProperty levelProperty, bool hasCrossFade)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(string.Format("Level {0}", index + 1), EditorStyles.boldLabel);

            var previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = removeColor;
            if (GUILayout.Button(deleteLevelButtonContent, GUILayout.Width(RemoveLevelButtonSize)))
            {
                DeleteLevel(index);
            }
            GUI.backgroundColor = previousBackgroundColor;
            EditorGUILayout.EndHorizontal();

            ++EditorGUI.indentLevel;

            var screenRelativeHeightProperty = levelProperty.FindPropertyRelative(LevelScreenRelativeHeightFieldName);
            EditorGUILayout.PropertyField(screenRelativeHeightProperty);

            var qualityProperty = levelProperty.FindPropertyRelative(LevelQualityFieldName);
            EditorGUILayout.PropertyField(qualityProperty);

            bool animateCrossFading = (hasCrossFade ? animateCrossFadingProperty.boolValue : false);
            settingsExpanded[index] = EditorGUILayout.Foldout(settingsExpanded[index], settingsContent);
            if (settingsExpanded[index])
            {
                ++EditorGUI.indentLevel;

                var combineMeshesProperty = levelProperty.FindPropertyRelative(LevelCombineMeshesFieldName);
                EditorGUILayout.PropertyField(combineMeshesProperty);

                if (combineMeshesProperty.boolValue)
                {
                    var combineSubMeshesProperty = levelProperty.FindPropertyRelative(LevelCombineSubMeshesFieldName);
                    EditorGUILayout.PropertyField(combineSubMeshesProperty);
                }

                var childProperties = levelProperty.GetChildProperties();
                foreach (var childProperty in childProperties)
                {
                    if (string.Equals(childProperty.name, LevelScreenRelativeHeightFieldName) || string.Equals(childProperty.name, LevelQualityFieldName) ||
                        string.Equals(childProperty.name, LevelCombineMeshesFieldName) || string.Equals(childProperty.name, LevelCombineSubMeshesFieldName) ||
                        string.Equals(childProperty.name, LevelRenderersFieldName))
                    {
                        continue;
                    }
                    else if ((!hasCrossFade || !animateCrossFading) && string.Equals(childProperty.name, LevelFadeTransitionWidthFieldName))
                    {
                        continue;
                    }

                    EditorGUILayout.PropertyField(childProperty, true);
                }

                --EditorGUI.indentLevel;
            }

            // Remove any null renderers
            var renderersProperty = levelProperty.FindPropertyRelative(LevelRenderersFieldName);
            for (int rendererIndex = renderersProperty.arraySize - 1; rendererIndex >= 0; rendererIndex--)
            {
                var rendererProperty = renderersProperty.GetArrayElementAtIndex(rendererIndex);
                var renderer = rendererProperty.objectReferenceValue as Renderer;
                if (renderer == null)
                {
                    renderersProperty.DeleteArrayElementAtIndex(rendererIndex);
                }
            }

            bool autoCollectRenderers = autoCollectRenderersProperty.boolValue;
            if (!autoCollectRenderers)
            {
                DrawRendererList(renderersProperty, EditorGUIUtility.currentViewWidth);
            }

            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
        }

        private void DrawRendererList(SerializedProperty renderersProperty, float availableWidth)
        {
            GUILayout.Label(renderersHeaderContent, EditorStyles.boldLabel);

            int rendererCount = renderersProperty.arraySize;
            int renderersPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / RendererButtonWidth));
            int rendererRowCount = Mathf.CeilToInt((float)(rendererCount + 1) / (float)renderersPerRow);

            var listPosition = GUILayoutUtility.GetRect(0f, rendererRowCount * RendererButtonWidth, GUILayout.ExpandWidth(true));
            GUI.Box(listPosition, GUIContent.none, EditorStyles.helpBox);

            var listInnerPosition = new Rect(listPosition.x + 3f, listPosition.y, listPosition.width - 6f, listPosition.height);
            float buttonWidth = listInnerPosition.width / (float)renderersPerRow;
            for (int rendererIndex = 0; rendererIndex < renderersProperty.arraySize; rendererIndex++)
            {
                int rowIndex = rendererIndex / renderersPerRow;
                int colIndex = rendererIndex % renderersPerRow;
                var rendererProperty = renderersProperty.GetArrayElementAtIndex(rendererIndex);
                var renderer = rendererProperty.objectReferenceValue as Renderer;

                var buttonPosition = new Rect(listInnerPosition.x + (colIndex * buttonWidth), listInnerPosition.y + (rowIndex * RendererButtonWidth) + 2f,
                    buttonWidth - 4f, RendererButtonWidth - 4f);
                DrawRendererButton(buttonPosition, renderersProperty, rendererIndex, renderer);
            }

            int addButtonRowIndex = rendererCount / renderersPerRow;
            int addButtonColIndex = rendererCount % renderersPerRow;
            var addButtonPosition = new Rect(listInnerPosition.x + (addButtonColIndex * buttonWidth), listInnerPosition.y + (addButtonRowIndex * RendererButtonWidth) + 2f,
                buttonWidth - 4f, RendererButtonWidth - 4f);
            HandleAddRenderer(addButtonPosition, listPosition, renderersProperty);
        }

        private void DrawRendererButton(Rect position, SerializedProperty renderersProperty, int rendererIndex, Renderer renderer)
        {
            var current = Event.current;
            var currentEvent = current.type;
            var removeButtonPosition = new Rect(position.xMax - RemoveRendererButtonSize, position.yMax - RemoveRendererButtonSize, RemoveRendererButtonSize, RemoveRendererButtonSize);

            if (currentEvent != EventType.Repaint)
            {
                if (currentEvent == EventType.MouseDown && current.button == 0)
                {
                    if (removeButtonPosition.Contains(current.mousePosition))
                    {
                        renderersProperty.DeleteArrayElementAtIndex(rendererIndex);
                        current.Use();
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                    else if (position.Contains(current.mousePosition))
                    {
                        Debug.Log("");
                        EditorGUIUtility.PingObject(renderer);
                        current.Use();
                    }
                }
            }
            else
            {
                if (renderer != null)
                {
                    GUIContent content = null;
                    var skinnedMeshRenderer = (renderer as SkinnedMeshRenderer);
                    if (skinnedMeshRenderer != null)
                    {
                        var meshPreview = AssetPreview.GetAssetPreview(skinnedMeshRenderer.sharedMesh);
                        content = new GUIContent(meshPreview, renderer.gameObject.name);
                    }
                    else
                    {
                        var meshFilter = renderer.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            var meshPreview = AssetPreview.GetAssetPreview(meshFilter.sharedMesh);
                            content = new GUIContent(meshPreview, renderer.gameObject.name);
                        }
                        else
                        {
                            string niceRendererTypeName = ObjectNames.NicifyVariableName(renderer.GetType().Name);
                            content = new GUIContent(niceRendererTypeName, renderer.gameObject.name);
                        }
                    }

                    var buttonPosition = new Rect(position.x + 2f, position.y + 2f, position.width - 4f, position.height - 4f);
                    GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
                    GUI.Box(buttonPosition, content);
                }
                else
                {
                    GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
                }

                var previousBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = removeColor;
                GUI.Box(removeButtonPosition, removeRendererButtonContent, EditorStyles.miniButton);
                GUI.backgroundColor = previousBackgroundColor;
            }
        }

        private void HandleAddRenderer(Rect position, Rect listArea, SerializedProperty renderersProperty)
        {
            if (GUI.Button(position, addRendererButtonContent))
            {
                EditorGUIUtility.ShowObjectPicker<Renderer>(null, true, string.Empty, ObjectPickerControlID);
                GUIUtility.ExitGUI();
            }

            var current = Event.current;
            var currentEvent = current.type;
            if (currentEvent == EventType.DragUpdated || currentEvent == EventType.DragPerform)
            {
                if (listArea.Contains(current.mousePosition))
                {
                    var dragObjects = DragAndDrop.objectReferences;
                    if (dragObjects != null && dragObjects.Length > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (currentEvent == EventType.DragPerform)
                        {
                            var draggedGameObjects = from go in dragObjects
                                                     where go as GameObject != null
                                                     select go as GameObject;
                            var draggedRenderers = from renderer in dragObjects
                                                   where renderer as Renderer != null
                                                   select renderer as Renderer;
                            var gameObjectRenderers = GetRenderers(draggedGameObjects, true);
                            AddRenderers(renderersProperty, draggedRenderers, true);
                            AddRenderers(renderersProperty, gameObjectRenderers, true);
                            DragAndDrop.AcceptDrag();
                        }
                    }

                    current.Use();
                }
            }
            else if (currentEvent == EventType.ExecuteCommand)
            {
                string commandName = current.commandName;
                if (string.Equals(commandName, "ObjectSelectorClosed") && EditorGUIUtility.GetObjectPickerControlID() == ObjectPickerControlID)
                {
                    var gameObject = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                    if (gameObject != null)
                    {
                        var gameObjectRenderers = GetRenderers(new GameObject[] { gameObject }, true);
                        AddRenderers(renderersProperty, gameObjectRenderers, true);
                    }
                    current.Use();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void AddRenderers(SerializedProperty renderersProperty, IEnumerable<Renderer> renderers, bool append)
        {
            if (!append)
            {
                renderersProperty.ClearArray();
            }

            var existingRendererList = new List<Renderer>(renderersProperty.arraySize);
            for (int i = 0; i < renderersProperty.arraySize; i++)
            {
                var rendererProperty = renderersProperty.GetArrayElementAtIndex(i);
                var renderer = rendererProperty.objectReferenceValue as Renderer;
                if (renderer != null)
                {
                    existingRendererList.Add(renderer);
                }
            }

            foreach (var renderer in renderers)
            {
                if (!existingRendererList.Contains(renderer))
                {
                    ++renderersProperty.arraySize;
                    var rendererProperty = renderersProperty.GetArrayElementAtIndex(renderersProperty.arraySize - 1);
                    rendererProperty.objectReferenceValue = renderer;
                    existingRendererList.Add(renderer);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateLevel()
        {
            int newIndex = levelsProperty.arraySize;
            levelsProperty.InsertArrayElementAtIndex(newIndex);
            var newLevelProperty = levelsProperty.GetArrayElementAtIndex(newIndex);
            var lastLevelProperty = (newIndex > 0 ? levelsProperty.GetArrayElementAtIndex(newIndex - 1) : null);
            var newScreenRelativeHeightProperty = newLevelProperty.FindPropertyRelative(LevelScreenRelativeHeightFieldName);
            var newQualityProperty = newLevelProperty.FindPropertyRelative(LevelQualityFieldName);

            if (lastLevelProperty != null)
            {
                var lastScreenRelativeHeightProperty = lastLevelProperty.FindPropertyRelative(LevelScreenRelativeHeightFieldName);
                var lastQualityProperty = lastLevelProperty.FindPropertyRelative(LevelQualityFieldName);
                newScreenRelativeHeightProperty.floatValue = lastScreenRelativeHeightProperty.floatValue * 0.5f;
                newQualityProperty.floatValue = lastQualityProperty.floatValue * 0.65f;
            }
            else
            {
                newScreenRelativeHeightProperty.floatValue = 0.6f;
                newQualityProperty.floatValue = 1f;
            }

            serializedObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }

        private void DeleteLevel(int index)
        {
            levelsProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }

        private void GenerateLODs()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating LODs", "Generating LODs...", 0f);
                var lodGroup = LODGenerator.GenerateLODs(lodGeneratorHelper);
                if (lodGroup != null)
                {
                    using (var serializedObject = new SerializedObject(lodGeneratorHelper))
                    {
                        var isGeneratedProperty = serializedObject.FindProperty(IsGeneratedFieldName);
                        serializedObject.UpdateIfRequiredOrScript();
                        isGeneratedProperty.boolValue = true;
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                DisplayError("Failed to generate LODs!", ex.Message, "OK", lodGeneratorHelper);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void DestroyLODs()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Destroying LODs", "Destroying LODs...", 0f);
                LODGenerator.DestroyLODs(lodGeneratorHelper);

                using (var serializedObject = new SerializedObject(lodGeneratorHelper))
                {
                    var isGeneratedProperty = serializedObject.FindProperty(IsGeneratedFieldName);
                    serializedObject.UpdateIfRequiredOrScript();
                    isGeneratedProperty.boolValue = false;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                DisplayError("Failed to destroy LODs!", ex.Message, "OK", lodGeneratorHelper);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private Renderer[] GetRenderers(IEnumerable<GameObject> gameObjects, bool searchChildren)
        {
            // Filter out game objects that aren't children of the generator
            var ourTransform = lodGeneratorHelper.transform;
            var childGameObjects = from go in gameObjects
                          where go.transform.IsChildOf(ourTransform)
                          select go;

            var notChildGameObjects = from go in gameObjects
                                      where !go.transform.IsChildOf(ourTransform)
#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019
                                         && !PrefabUtility.IsPartOfAnyPrefab(go)
#endif
                                      select go;

#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019
            var prefabGameObjects = from go in gameObjects
                                    where !go.transform.IsChildOf(ourTransform) &&
                                        PrefabUtility.IsPartOfAnyPrefab(go)
                                    select go;

            if (prefabGameObjects.Any())
            {
                EditorUtility.DisplayDialog("Invalid GameObjects", "Some objects are not children of the LODGenerator GameObject," + 
                    " as well as being part of a prefab. They will not be added.", "OK");
            }
#endif

            if (notChildGameObjects.Any())
            {
                if (EditorUtility.DisplayDialog("Reparent GameObjects", "Some objects are not children of the LODGenerator GameObject." +
                    " Do you want to reparent them and add them to the LODGenerator?", "Yes, Reparent", "No, Use Only Existing Children"))
                {
                    var relocatedList = new List<GameObject>();
                    foreach (var gameObject in notChildGameObjects)
                    {
                        gameObject.transform.SetParent(ourTransform, true);
                        relocatedList.Add(gameObject);
                    }

                    childGameObjects = childGameObjects.Union(relocatedList);
                }
            }

            var rendererList = new List<Renderer>();
            foreach (var gameObject in childGameObjects)
            {
                if (searchChildren)
                {
                    var renderers = gameObject.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        if (!rendererList.Contains(renderer))
                        {
                            rendererList.Add(renderer);
                        }
                    }
                }
                else
                {
                    var renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        rendererList.Add(renderer);
                    }
                }
            }

            return rendererList.ToArray();
        }

        private static void DisplayError(string title, string message, string ok, Object context)
        {
            EditorUtility.DisplayDialog(title, message, ok);
        }
    }
}
