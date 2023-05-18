using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AssetUsageChecker
{
    [MenuItem("Assets/Check Selected Assets Usage and Delete Unused", false, 20)]
    private static void CheckSelectedAssetsUsageAndDeleteUnused()
    {
        Object[] selectedObjects = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogError("Please select one or more assets in the Project window.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"The object \"{obj.name}\" is not a valid asset.");
                continue;
            }

            if (IsAssetUsed(assetPath))
            {
                Debug.Log($"The asset \"{assetPath}\" is being used in the scenes. Cannot delete it.");
            }
            else
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
                Debug.Log($"The asset \"{assetPath}\" has been deleted.");
            }
        }
    }

    [MenuItem("Assets/Check Selected Assets Usage and Delete Unused", true)]
    private static bool ValidateCheckSelectedAssetsUsageAndDeleteUnused()
    {
        return Selection.GetFiltered<Object>(SelectionMode.DeepAssets).Length > 0;
    }

    private static bool IsAssetUsed(string assetPath)
    {
        string[] allScenePaths = UnityEditor.AssetDatabase.GetAllAssetPaths();

        foreach (string scenePath in allScenePaths)
        {
            if (scenePath.EndsWith(".unity"))
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath);

                GameObject[] allObjects = scene.GetRootGameObjects();

                foreach (GameObject obj in allObjects)
                {
                    Component[] components = obj.GetComponentsInChildren<Component>(true);

                    foreach (Component component in components)
                    {
                        if (component != null)
                        {
                            SerializedObject serializedObject = new SerializedObject(component);
                            SerializedProperty prop = serializedObject.GetIterator();

                            while (prop.NextVisible(true))
                            {
                                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    if (prop.objectReferenceValue != null)
                                    {
                                        Object objRef = prop.objectReferenceValue;

                                        string objPath = AssetDatabase.GetAssetPath(objRef);

                                        if (objPath == assetPath)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
