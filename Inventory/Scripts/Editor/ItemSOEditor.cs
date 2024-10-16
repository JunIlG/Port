using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        ItemSO item = (ItemSO)target;

        DrawDefaultInspector();

        
        GUILayout.Label("Grid", EditorStyles.boldLabel);

        for (int i = 0; i < 4; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                item[i, j] = GUILayout.Toggle(item[i, j], GUIContent.none, GUILayout.Width(20));
            }
            GUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}
