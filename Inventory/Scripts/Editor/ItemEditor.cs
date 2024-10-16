using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEditor.UIElements;

public class ItemEditor : EditorWindow 
{
    private static List<ItemSO> itemList = new List<ItemSO>();

    private static VisualTreeAsset itemRowTemplate;

    private VisualElement itemListContainer;
    private ListView itemListView;

    private VisualElement itemEditorContainer;
    private VisualElement itemIcon;
    private Toggle[,] itemGridView = new Toggle[4, 4];

    private Sprite defaultItemIcon;

    private ItemSO selectedItem;

    [MenuItem("GridInven/Item")]
    private static void ShowWindow() 
    {
        var window = GetWindow<ItemEditor>();
        window.titleContent = new GUIContent("Item");
        window.minSize = new Vector2(400f, 500f);

        window.Show();
    }

    public void CreateGUI()
    {
        defaultItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/03_Copper_Bar_nobg.png");

        VisualTreeAsset mainTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ItemEditor.uxml");

        VisualElement root = mainTree.Instantiate();
        rootVisualElement.Add(root);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/ItemEditor.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        itemEditorContainer = rootVisualElement.Q<VisualElement>("ItemEditorContainer");
        itemEditorContainer.style.visibility = Visibility.Hidden;
        itemIcon = itemEditorContainer.Q<VisualElement>("Icon");

        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ItemRowTemplate.uxml");

        itemListContainer = rootVisualElement.Q<VisualElement>("ItemListContainer");

        rootVisualElement.Q<Button>("Btn_Add").clicked += MakeNewItem;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int captureI = i, captureJ = j;
                itemGridView[i, j] = root.Q<Toggle>($"Grid_{i}-{j}");
                itemGridView[i, j].RegisterValueChangedCallback(evt => ItemGridToggle(captureI, captureJ, evt.newValue));
            }
        }

        rootVisualElement.Q<Button>("Btn_Delete").clicked += DeleteItem;

        itemEditorContainer.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            selectedItem.itemName = evt.newValue;

            itemListView.Rebuild();
        });
        itemEditorContainer.Q<ObjectField>("IconPicker").RegisterValueChangedCallback(evt =>
        {
            Sprite newSprite = evt.newValue as Sprite;

            selectedItem.icon = newSprite == null ? defaultItemIcon : newSprite;
            itemIcon.style.backgroundImage = selectedItem.icon.texture;

            itemListView.Rebuild();
        });

        LoadItems();
        
        GenerateItemListView();
    }

    private void LoadItems()
    {
        itemList.Clear();

        string[] paths = Directory.GetFiles("Assets/Data/Items", "*.asset", SearchOption.AllDirectories);

        foreach (string path in paths)
        {
            string cleanPath = path.Replace("\\", "/");
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(cleanPath);
            if (item != null)
            {
                itemList.Add(item);
            }
        }
    }

    private void GenerateItemListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            e.Q<VisualElement>("ItemIcon").style.backgroundImage = itemList[i] == null ? defaultItemIcon.texture : itemList[i].icon.texture;
            e.Q<Label>("ItemName").text = itemList[i] == null ? "" : itemList[i].itemName;
        };

        itemListView = new ListView(itemList, 35f, makeItem, bindItem);
        itemListView.selectionType = SelectionType.Single;
        itemListView.style.height = itemList.Count * 40f;

        itemListContainer.Add(itemListView);

        itemListView.selectionChanged += ListView_SelectionChanged;
    }

    private void ListView_SelectionChanged(IEnumerable<object> selectedItems)
    {
        if (selectedItems.Count() == 0)
        {
            itemEditorContainer.style.visibility = Visibility.Hidden;
            return;
        }

        selectedItem = selectedItems.First() as ItemSO;

        SerializedObject so = new SerializedObject(selectedItem);
        itemEditorContainer.Bind(so);

        itemIcon.style.backgroundImage = selectedItem.icon.texture;
        itemEditorContainer.style.visibility = Visibility.Visible;

        // grid view set visual
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                itemGridView[i, j].SetValueWithoutNotify(selectedItem[i, j]);
            }
        }
    }

    private void ItemGridToggle(int i, int j, bool toggle)
    {
        selectedItem[i, j] = toggle;
    }

    private void MakeNewItem()
    {
        ItemSO newItem = CreateInstance<ItemSO>();

        newItem.itemName = $"New Item";
        newItem.icon = defaultItemIcon;

        AssetDatabase.CreateAsset(newItem, $"Assets/Data/Items/{newItem.id}.asset");

        itemList.Add(newItem);

        itemListView.Rebuild();

        itemListView.style.height = itemList.Count * 40f;
    }

    private void DeleteItem()
    {
        string path = AssetDatabase.GetAssetPath(selectedItem);
        AssetDatabase.DeleteAsset(path);

        itemList.Remove(selectedItem);

        itemListView.ClearSelection();
        itemListView.Rebuild();

        itemEditorContainer.style.visibility = Visibility.Hidden;
    }
}