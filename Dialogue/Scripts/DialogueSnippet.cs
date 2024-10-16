using System;
using System.Collections.Generic;

[Serializable]
public class DialogueSnippet
{
    public string ID;
    public string dialogueText;
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public bool IsEntry()
    {
        return dialogueText.Equals("@Entry");
    }
}