using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueEditor
{
    // View elements
    private DialogueEditorView editorView;
    public Blackboard blackboard;

    // View properties
    private readonly Vector2 defaultNodeSize = new Vector2(200, 300);

    // Model elements
    private DialogueSO dialogue;

    public DialogueEditor(DialogueEditorWindow wnd, DialogueSO inDialogue)
    {
        dialogue = inDialogue;

        GenerateEditorView(wnd);

        // Create a entry node if none exists
        if (dialogue.dialogueSnippets.Count == 0)
        {
            CreateEntryNode(new Vector2(100, 100));
        }
    }

    #region View
    public DialogueEditorView GetView()
    {
        return editorView;
    }

    public DialogueEditorView GenerateEditorView(DialogueEditorWindow wnd)
    {
        editorView = new DialogueEditorView(this, wnd);

        return editorView;
    }
    #endregion

    #region Node
    /// <summary>
    /// Create a entry node and Add it to the graph view
    /// </summary>
    public DialogueNode CreateEntryNode(Vector2 position, string nodeID = "")
    {
        DialogueNode node = new DialogueNode
        {
            ID = nodeID == string.Empty ? Guid.NewGuid().ToString() : nodeID,
            title = "EntryNode",
            dialogueText = "@Entry",
            isEntry = true
        };
        node.capabilities &= ~Capabilities.Deletable;
        node.capabilities &= ~Capabilities.Copiable;

        node.SetPosition(new Rect(position, new Vector2(150, 200)));

        Port port = CreatePort(node, Direction.Output);
        port.portName = "Start";

        node.outputContainer.Add(port);
        node.RefreshExpandedState();
        node.RefreshPorts();

        editorView.AddElement(node);

        return node;
    }

    /// <summary>
    /// Create a new node and Add it to the view
    /// </summary>
    public DialogueNode CreateNode(Vector2 position, string dialogueText = "")
    {
        DialogueNode node = new DialogueNode
        {
            ID = Guid.NewGuid().ToString(),
            title = dialogueText,
            dialogueText = dialogueText
        };

        node.SetPosition(new Rect(position, defaultNodeSize));

        Button addChoiceButton = new Button(() => AddChoicePort(node));
        addChoiceButton.text = "+";
        node.titleButtonContainer.Add(addChoiceButton);

        Port port = CreatePort(node, Direction.Input, Port.Capacity.Multi);
        port.portName = "Input";
        node.inputContainer.Add(port);

        TextField dialogueTextField = new TextField(string.Empty);
        dialogueTextField.RegisterValueChangedCallback(evt =>
        {
            node.title = evt.newValue;
            node.dialogueText= evt.newValue;
        });
        dialogueTextField.SetValueWithoutNotify(dialogueText);
        dialogueTextField.multiline = true;
        node.mainContainer.Add(dialogueTextField);

        node.RefreshExpandedState();
        node.RefreshPorts();

        editorView.AddElement(node);

        return node;
    }
    #endregion

    #region Port
    private Port CreatePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(Node));
    }

    public void AddChoicePort(DialogueNode node, string portName = "")
    {
        Port port = CreatePort(node, Direction.Output);
        port.name = "Output";
        port.portName = portName;

        Label oldLabel = port.contentContainer.Q<Label>();
        port.contentContainer.Remove(oldLabel);

        Label newLabel = new Label("Output");
        port.contentContainer.Add(newLabel);

        TextField choiceTextField = new TextField
        {
            value = portName
        };
        choiceTextField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
        port.contentContainer.Add(choiceTextField);

        Button deleteButton = new Button(() => RemovePort(node, port))
        {
            text = "X"
        };
        port.contentContainer.Add(deleteButton);

        node.outputContainer.Add(port);
        node.RefreshExpandedState();
        node.RefreshPorts();
    }

    private void RemovePort(DialogueNode node, Port port)
    {
        Edge edge = editorView.edges.ToList().FirstOrDefault(e => e.output == port);

        if (edge != null)
        {
            edge.input.Disconnect(edge);
            editorView.RemoveElement(edge);
        }

        node.outputContainer.Remove(port);
        node.RefreshExpandedState();
        node.RefreshPorts();
    }
    #endregion

    #region Blackboard
    public void AddPropertyToBlackboard(ExposedProperty newProperty)
    {
        string localPropertyKey = newProperty.propertyKey;
        string localPropertyValue = newProperty.propertyValue;

        while (dialogue.exposedProperties.Any(p => p.propertyKey == localPropertyKey))
        {
            localPropertyKey += "_";
        }

        ExposedProperty property = new ExposedProperty
        {
            propertyKey = localPropertyKey,
            propertyValue = localPropertyValue
        };
        dialogue.exposedProperties.Add(property);

        VisualElement container = new VisualElement();
        BlackboardField blackboardField = new BlackboardField
        {
            text = property.propertyKey,
            typeText = "string"
        };
        container.Add(blackboardField);

        TextField valueField = new TextField("Value: ")
        {
            value = property.propertyValue
        };
        valueField.RegisterValueChangedCallback(evt =>
        {
            property.propertyValue = evt.newValue;
        });

        BlackboardRow blackboardRow = new BlackboardRow(blackboardField, valueField);
        container.Add(blackboardRow);

        blackboard.Add(container);
    }
    #endregion

    #region SaveAndLoad
    public void SaveDialogue()
    {
        DialogueSaveUtility.GetInstance(this, editorView, dialogue).SaveDialogue();
    }

    public void LoadDialogue()
    {
        DialogueSaveUtility.GetInstance(this, editorView, dialogue).LoadDialogue();
    }
    #endregion
}