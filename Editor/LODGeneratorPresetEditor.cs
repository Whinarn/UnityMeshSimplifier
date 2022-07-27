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
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityMeshSimplifier.Editor
{
    [CustomEditor(typeof(LODGeneratorPreset))]
    internal sealed class LODGeneratorPresetEditor : UnityEditor.Editor
    {
        private const string FadeModeFieldName = "fadeMode";
        private const string AnimateCrossFadingFieldName = "animateCrossFading";
        private const string SimplificationOptionsFieldName = "simplificationOptions";
        private const string LevelsFieldName = "levels";
        private const string LevelScreenRelativeHeightFieldName = "screenRelativeTransitionHeight";
        private const string LevelFadeTransitionWidthFieldName = "fadeTransitionWidth";
        private const string LevelQualityFieldName = "quality";
        private const string LevelCombineMeshesFieldName = "combineMeshes";
        private const string LevelCombineSubMeshesFieldName = "combineSubMeshes";
        private const string LevelRenderersFieldName = "renderers";
        private const string SimplificationOptionsEnableSmartLinkFieldName = "EnableSmartLink";
        private const string SimplificationOptionsVertexLinkDistanceFieldName = "VertexLinkDistance";
        private const float RemoveLevelButtonSize = 20f;

        private SerializedProperty fadeModeProperty = null;
        private SerializedProperty animateCrossFadingProperty = null;
        private SerializedProperty simplificationOptionsProperty = null;
        private SerializedProperty levelsProperty = null;

        private bool[] settingsExpanded = null;
        private LODGeneratorPreset lodGeneratorPreset = null;

        private static readonly GUIContent createLevelButtonContent = new GUIContent("Create Level", "Creates a new LOD level.");
        private static readonly GUIContent deleteLevelButtonContent = new GUIContent("X", "Deletes this LOD level.");
        private static readonly GUIContent settingsContent = new GUIContent("Settings", "The settings for the LOD level.");
        private static readonly Color removeColor = new Color(1f, 0.6f, 0.6f, 1f);

        private void OnEnable()
        {
            fadeModeProperty = serializedObject.FindProperty(FadeModeFieldName);
            animateCrossFadingProperty = serializedObject.FindProperty(AnimateCrossFadingFieldName);
            simplificationOptionsProperty = serializedObject.FindProperty(SimplificationOptionsFieldName);
            levelsProperty = serializedObject.FindProperty(LevelsFieldName);

            lodGeneratorPreset = target as LODGeneratorPreset;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawView();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawView()
        {
            EditorGUILayout.PropertyField(fadeModeProperty);
            var fadeMode = (LODFadeMode)fadeModeProperty.intValue;

            bool hasCrossFade = (fadeMode == LODFadeMode.CrossFade || fadeMode == LODFadeMode.SpeedTree);
            if (hasCrossFade)
            {
                EditorGUILayout.PropertyField(animateCrossFadingProperty);
            }

            DrawSimplificationOptions();

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

            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
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
    }
}
