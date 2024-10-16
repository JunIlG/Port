using System;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string ID;
    public string dialogueText;
    public Vector2 nodePosition;
    public bool isEntry = false;
}
