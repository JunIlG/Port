using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class DialogueNode : Node
{
    public string ID;
    public string dialogueText;
    public Vector2 nodePosition;
    public bool isEntry = false;
}
