using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshSimplifierEditor : EditorWindow
{
    private SkinnedMeshRenderer[] selectedMeshRenderers;
    private float quality = 0.5f;

    [MenuItem("SimplifyMesh Editor/Simplify Mesh")]
    static void Init()
    {
        MeshSimplifierEditor window = (MeshSimplifierEditor)EditorWindow.GetWindow(typeof(MeshSimplifierEditor));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Mesh Simplification", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Select Skinned Mesh Renderers:");

        // If the array is null, initialize it
        if (selectedMeshRenderers == null)
        {
            selectedMeshRenderers = new SkinnedMeshRenderer[0];
        }

        // Display the selection field for each element in the array
        for (int i = 0; i < selectedMeshRenderers.Length; i++)
        {
            selectedMeshRenderers[i] = EditorGUILayout.ObjectField(
                $"Element {i + 1}",
                selectedMeshRenderers[i],
                typeof(SkinnedMeshRenderer),
                true
            ) as SkinnedMeshRenderer;
        }

        // Display an "Add" button to add more elements to the array
        if (GUILayout.Button("Add Mesh Renderer"))
        {
            ArrayUtility.Add(ref selectedMeshRenderers, null);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Remove Mesh Renderer") && selectedMeshRenderers.Length > 0)
        {
            ArrayUtility.RemoveAt(ref selectedMeshRenderers, selectedMeshRenderers.Length - 1);
        }


        EditorGUILayout.Space();

        quality = EditorGUILayout.Slider("Quality", quality, 0.01f, 1f);

        if (GUILayout.Button("Begin Simplification"))
        {
            SimplifyMeshes();
        }
    }

    void SimplifyMeshes()
    {
        if (selectedMeshRenderers == null || selectedMeshRenderers.Length == 0)
        {
            Debug.LogWarning("No Skinned Mesh Renderers selected.");
            return;
        }
        else
        {
            foreach (var meshRenderer in selectedMeshRenderers)
            {
                if (meshRenderer == null)
                {
                    Debug.LogWarning("One of the selected mesh renderers is null.");
                    continue;
                }

                // The rest of the code remains unchanged
                var originalMesh = meshRenderer.sharedMesh;
                var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
                meshSimplifier.Initialize(originalMesh);
                meshSimplifier.SimplifyMesh(quality);
                var destMesh = meshSimplifier.ToMesh();

                string folderPath = "Assets/Simplified Meshes";
                string meshName = meshRenderer.transform.name.Replace(":", "_");
                string fileName = $"mesh_{meshName}_{meshRenderer.transform.GetSiblingIndex()}.asset";

                string filePath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                AssetDatabase.CreateAsset(destMesh, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                meshRenderer.sharedMesh = destMesh;

                Debug.Log($"Mesh simplified and saved at: {filePath}");
            }
        }
    }
}