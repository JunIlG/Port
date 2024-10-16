using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class DialogueEditorView : GraphView
{
    // Window reference
    private EditorWindow wnd;

    // Controller referece
    private DialogueEditor editor;

    public DialogueEditorView(DialogueEditor inEditor, EditorWindow inWnd)
    {
        wnd = inWnd;
        editor = inEditor;
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueEditorView"));
        AddGrid();
        AddManipulators();
    }

    private void AddGrid()
    {
        GridBackground grid = new GridBackground();
        Insert(0, grid);
    }

    private void AddManipulators()
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));
    }

    private void BuildContextMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Create Node", ContextCreateNode);
    }

    private void ContextCreateNode(DropdownMenuAction action)
    {
        editor.CreateNode(contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(port => port != startPort && port.node != startPort.node).ToList();
    }
}