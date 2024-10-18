using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public UIDocument uIDocument;
    private ListView chatContainer;
    private TextField inputTextView;
    private string inputText;
    public VisualTreeAsset chatTemplate;
    
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
        // Initialize UI elements
        uIDocument = GetComponent<UIDocument>();
        chatContainer = uIDocument.rootVisualElement.Q<ListView>("ChatContainer");
        inputTextView = uIDocument.rootVisualElement.Q<TextField>("InputText");
        inputTextView.RegisterValueChangedCallback(evt => inputText = evt.newValue);
        inputTextView.RegisterCallback<NavigationSubmitEvent>(OnTextSend, TrickleDown.TrickleDown);
        inputTextView.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return)
        {
            evt.StopImmediatePropagation();
        }

        inputTextView.Focus();
    }

    private void OnTextSend(NavigationSubmitEvent evt)
    {
        using (Packet packet = new Packet(PacketType.Message))
        {
            packet.Write(inputText);
            ClientSend.SendTcpData(packet);
        }

        inputTextView.value = "";

        evt.StopImmediatePropagation();
        inputTextView.Focus();
    }

    public void AddChatMessage(string message)
    {
        VisualElement newChatItem = chatTemplate.CloneTree();
        newChatItem.Q<Label>("MessageText").text = message;
        chatContainer.hierarchy.Add(newChatItem);
        chatContainer.Rebuild();
    }
    
}
