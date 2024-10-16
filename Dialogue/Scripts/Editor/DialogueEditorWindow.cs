using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DialogueEditorWindow : EditorWindow
{
    // Window for reopening the window
    private static DialogueEditorWindow window;

    // Editor graph view element
    private DialogueEditorView editorView;

    // Controller for the dialogue editor UI.
    private DialogueEditor editor;

    // State variables
    private DialogueSO targetDialogue;

    public static void ShowWindow(DialogueSO obj)
    {
        if (window != null)
        {
            window.Close();
            window = null;
        }

        DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>();
        wnd.targetDialogue = obj;
        wnd.titleContent = new GUIContent($"{wnd.targetDialogue.name}");

        wnd.GenerateEditor();
        wnd.RegisterShortcuts();
        wnd.GenerateBlackboard();
        wnd.editor.LoadDialogue();

        window = wnd;

        wnd.Show();
    }

    /// <summary>
    /// Generate the dialogue editor controller and Add its view to this window
    /// </summary>
    private void GenerateEditor()
    {
        editor = new DialogueEditor(this, targetDialogue);

        editorView = editor.GetView();

        rootVisualElement.Add(editorView);
        editorView.StretchToParentSize();
    }

   /// <summary>
   /// Generate Blackboard for the dialogue editor view.
   /// </summary>
    private void GenerateBlackboard()
    {
        Blackboard blackboard = new Blackboard(editorView);
        blackboard.Add(new BlackboardSection{title = "Exposed Properties"});
        
        blackboard.addItemRequested = x => editor.AddPropertyToBlackboard(new ExposedProperty());

        blackboard.editTextRequested = (bb, element, newValue) =>
        {
            string oldPropertyKey = ((BlackboardField)element).text;
            if (targetDialogue.exposedProperties.Any(p => p.propertyKey == newValue))
            {
                EditorUtility.DisplayDialog("Error", "A property with this key already exists.", "OK");
                return;
            }

            targetDialogue.exposedProperties.FirstOrDefault(p => p.propertyKey == oldPropertyKey).propertyKey = newValue;
            ((BlackboardField)element).text = newValue;
        };
        
        blackboard.SetPosition(new Rect(10, 30, 200, 300));
        blackboard.scrollable = true;

        editor.blackboard = blackboard;
        editorView.Add(blackboard);
    }

    #region Register Shortcut
    /// <summary>
    /// Register shortcut keys to the Dialogue Editor View
    /// </summary>
    /// <param name="view"></param>
    private void RegisterShortcuts()
    {
        editorView.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                editor.SaveDialogue();
                evt.StopPropagation();
            }
        });
    }

    public void RequestCreateNode()
    {
        editor.CreateNode(Event.current.mousePosition);
    }
    #endregion
}