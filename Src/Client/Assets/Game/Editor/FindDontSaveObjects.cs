using UnityEngine;
using UnityEditor;
using Common;

public class FindDontSaveObjects
{
    [MenuItem("Tools/Find DontSaveInEditor Objects")]
    static void FindAll()
    {
        var all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in all)
        {
            if ((go.hideFlags & HideFlags.DontSaveInEditor) != 0)
            {
                Log.WarningFormat($"Found DontSaveInEditor Object: {go.name}", go);
            }
        }
    }
}