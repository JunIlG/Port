using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueSO")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueSnippet> dialogueSnippets = new List<DialogueSnippet>();

    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    
    // For Editor use only. Not used in game.
    #if UNITY_EDITOR
    public List<DialogueNodeData> dialogueNodeData = new List<DialogueNodeData>();
    #endif

    public DialogueSnippet GetSnippet(string ID)
    {
        return dialogueSnippets.FirstOrDefault(x => x.ID == ID);
    }

    /// <summary>
    /// Gets the text  replacing any exposed properties with their current values
    /// </summary>
    public string GetConvertText(string text)
    {
        Dictionary<string, string> propertyDict = new Dictionary<string, string>();
        exposedProperties.ForEach(p => propertyDict[p.propertyKey] = p.propertyValue);

        return Regex.Replace(text, @"\{(.+?)\}", match => 
        {
            string key = match.Groups[1].Value;

            return propertyDict.TryGetValue(key, out string value) ? value : match.Value;
        });
    }

    /// <summary>
    /// Get the dialogue snippet that is the entry point
    /// </summary>
    public DialogueSnippet GetFirstSnippet()
    {
        return GetSnippet(dialogueSnippets.FirstOrDefault(snippet => snippet.dialogueText == "@Entry").choices.FirstOrDefault()?.nextSnippetID);
    }
}