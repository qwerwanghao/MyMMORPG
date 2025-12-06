using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameViewHierarchyPicker
{
    [MenuItem("Tools/Pick UI Element %q")]
    static void PickUIElement()
    {
        var focused = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;
        if (focused == null || focused.GetType().FullName != "UnityEditor.GameView")
        {
            Debug.LogWarning("GameViewHierarchyPicker: 请将焦点切到 Game 视图再使用 Ctrl+Q。");
            return;
        }

        // Find EventSystem
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = Object.FindFirstObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            Debug.LogWarning("GameViewHierarchyPicker: No EventSystem found in the scene.");
            return;
        }

        // Prepare Pointer Data
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        // Raycast
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            // Pick the first valid result (top-most UI element)
            GameObject pickedObject = results[0].gameObject;

            // Select in Hierarchy
            Selection.activeGameObject = pickedObject;

            // Ping in Hierarchy to highlight
            EditorGUIUtility.PingObject(pickedObject);

            Debug.Log($"[GameViewHierarchyPicker] Picked: {pickedObject.name}", pickedObject);
        }
        else
        {
            Debug.Log("[GameViewHierarchyPicker] No UI element found under mouse.");
        }
    }
}
