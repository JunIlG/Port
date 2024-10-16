using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueSaveUtility
{
    // Save target
    private DialogueSO dialogue;

    // Controller element
    private DialogueEditor editor;

    // View elements
    private DialogueEditorView editorView;

    // Model elements
    private List<Edge> edges => editorView.edges.ToList();
    private List<DialogueNode> nodes => editorView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static DialogueSaveUtility GetInstance(DialogueEditor inEditor, DialogueEditorView inEditorView, DialogueSO inDialogue)
    {
        return new DialogueSaveUtility
        {

            dialogue = inDialogue,
            editor = inEditor,
            editorView = inEditorView
        };
    }

    #region Save
    public void SaveDialogue()
    {
        if (ConfigureEntireDialogue() == false)
        {
            EditorUtility.DisplayDialog("Error", "The dialogue entry node must exist to a snippet", "OK");
        }

        EditorUtility.SetDirty(dialogue);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Configure the dialogue model based on the current state of the editor view
    /// If the snippet is not connected to anything, it will be ignored
    /// </summary>
    private bool ConfigureEntireDialogue()
    {
        DialogueNode startNode = nodes.FirstOrDefault(x => x.isEntry);

        if (startNode == null)
        {
            return false;
        }

        Queue<DialogueNode> queue = new Queue<DialogueNode>();
        HashSet<DialogueNode> visitedNodes = new HashSet<DialogueNode>();
        queue.Enqueue(startNode);
        visitedNodes.Add(startNode);

        // Store snippets
        dialogue.dialogueSnippets.Clear();
        while (queue.Count > 0)
        {
            DialogueNode currentNode = queue.Dequeue();

            DialogueSnippet snippet = new DialogueSnippet()
            {
                ID = currentNode.ID,
                dialogueText = currentNode.dialogueText
            };

            foreach (Edge edge in edges.Where(x => x.output.node == currentNode))
            {
                DialogueNode nextNode = edge.input.node as DialogueNode;

                DialogueChoice choice = new DialogueChoice()
                {
                    choiceText = edge.output.portName,
                    nextSnippetID = nextNode.ID
                };
                snippet.choices.Add(choice);

                if (!visitedNodes.Contains(nextNode))
                {
                    queue.Enqueue(nextNode);
                    visitedNodes.Add(nextNode);
                }
            }

            dialogue.dialogueSnippets.Add(snippet);
        }

        // Store node datas
        dialogue.dialogueNodeData.Clear();
        foreach (DialogueNode node in nodes)
        {
            dialogue.dialogueNodeData.Add(new DialogueNodeData
            {
                ID = node.ID,
                dialogueText = node.dialogueText,
                nodePosition = node.GetPosition().position,
                isEntry = node.isEntry
            });
        }

        return true;
    }
    #endregion

    #region Load
    public void LoadDialogue()
    {
        if (dialogue.dialogueNodeData.Count == 0)
        {
            return;
        }

        CreateNodes();
        ConnectNodes();
        LoadExposedProperties();
    }

    private void CreateNodes()
    {

        DialogueNodeData entryNodeData = dialogue.dialogueNodeData.FirstOrDefault(x => x.isEntry);
        editor.CreateEntryNode(entryNodeData.nodePosition, entryNodeData == null ? string.Empty : entryNodeData.ID);

        foreach (DialogueNodeData nodeData in dialogue.dialogueNodeData)
        {
            if (nodeData.isEntry)
            {
                continue;
            }

            var tempNode = editor.CreateNode(nodeData.nodePosition, nodeData.dialogueText);
            tempNode.ID = nodeData.ID;
            tempNode.isEntry = nodeData.isEntry;

            foreach (DialogueChoice choice in dialogue.dialogueSnippets.FirstOrDefault(x => x.ID == tempNode.ID).choices)
            {
                editor.AddChoicePort(tempNode, choice.choiceText);
            }
        }
    }

    private void ConnectNodes()
    {
        List<DialogueNode> tempNodes = nodes;
        foreach (DialogueNode node in tempNodes)
        {
            DialogueSnippet snippet = dialogue.dialogueSnippets.FirstOrDefault(x => x.ID == node.ID);
            
            int i = 0;
            foreach (DialogueChoice choice in snippet.choices)
            {
                DialogueNode nextNode = tempNodes.FirstOrDefault(x => x.ID == choice.nextSnippetID);

                LinkNode(node.outputContainer[i].Q<Port>(), (Port)nextNode.inputContainer[0]);
                ++i;
            }
        }
    }

    private void LinkNode(Port output, Port input)
    {
        Edge tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        editorView.Add(tempEdge);
    }
    
    private void LoadExposedProperties()
    {
        ExposedProperty[] properties = dialogue.exposedProperties.ToArray();
        dialogue.exposedProperties.Clear();
        foreach (ExposedProperty property in properties)
        {
            editor.AddPropertyToBlackboard(property);
        }
    }
    #endregion
}