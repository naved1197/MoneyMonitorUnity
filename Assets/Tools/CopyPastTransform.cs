#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class CopyPastTransform : Editor
{
    private Transform transformCopy;
    private Editor defaultEditor;

    private void OnEnable()
    {
        // Cache the default editor
        defaultEditor = Editor.CreateEditor(target, GetEditorType());
    }

    private void OnDisable()
    {
        // Clean up the default editor
        if (defaultEditor != null)
        {
            DestroyImmediate(defaultEditor);
            defaultEditor = null;
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        if (defaultEditor != null)
        {
            defaultEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button("Paste Components"))
        {
            PasteComponents();
        }

        GUI.enabled = Selection.activeTransform != null;

        if (GUILayout.Button("Copy Components"))
        {
            CopyComponents();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void CopyComponents()
    {
        UnityEditorInternal.ComponentUtility.CopyComponent(Selection.activeTransform);

    }

    private void PasteComponents()
    {
        UnityEditorInternal.ComponentUtility.PasteComponentValues(Selection.activeTransform);
        //if (transformCopy != null)
        //{
        //    Component[] components = transformCopy.GetComponents<Component>();

        //    Undo.RecordObject(Selection.activeTransform, "Paste Components");

        //    foreach (Component component in components)
        //    {
        //        if (component != null && !(component is Transform))
        //        {
        //            UnityEditorInternal.ComponentUtility.CopyComponent(component);
        //            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(Selection.activeTransform.gameObject);
        //        }
        //    }

        //    Debug.Log("Components pasted.");
        //}
    }

    private System.Type GetEditorType()
    {
        // Get the type of the default Transform editor
        System.Type editorType = typeof(Editor).Assembly.GetType("UnityEditor.TransformInspector");

        if (editorType == null)
        {
            // Fallback to the default editor if the TransformInspector type is not found
            editorType = typeof(Editor);
        }

        return editorType;
    }
}
#endif