using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class DialogueEditorWindowOpener : AssetModificationProcessor
{
    [OnOpenAsset(1)]
    public static bool OnOepnAsset(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);

        if (obj is DialogueSO)
        {
            DialogueEditorWindow.ShowWindow((DialogueSO)obj);
            return true;
        }

        return false;
    }
}