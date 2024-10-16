using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    // Singloton instance
    public static DialogueManager instance { get; private set; }

    // UI Elements
    private UIDocument ui;
    private Label textLabel;
    private Coroutine textAnim;
    private VisualElement selectButtonContainer;
    public VisualTreeAsset selectButtonTemplate;

    public DialogueSO dialogue;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        ui = GetComponent<UIDocument>();

        // Get UI Elements
        textLabel = ui.rootVisualElement.Q<Label>("TextLabel");
        selectButtonContainer = ui.rootVisualElement.Q<VisualElement>("SelectButtonContainer");

        ShowDialogue(dialogue.GetFirstSnippet());
    }

    IEnumerator RevealText(string labelText)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < labelText.Length; ++i)
        {
            sb.Append(labelText[i]);
            textLabel.text = sb.ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShowDialogue(DialogueSnippet currentSnippet)
    {
        if (textAnim != null)
        {
            StopCoroutine(textAnim);
        }

        textAnim = StartCoroutine(RevealText(dialogue.GetConvertText(currentSnippet.dialogueText)));

        selectButtonContainer.Clear();
        foreach (DialogueChoice choice in currentSnippet.choices)
        {
            Button button = selectButtonTemplate.CloneTree().Children().First() as Button;
            button.text = dialogue.GetConvertText(choice.choiceText);

            button.clicked += () => ShowDialogue(dialogue.GetSnippet(choice.nextSnippetID));

            selectButtonContainer.Add(button);
        }
    }
}
