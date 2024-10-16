using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class Inventory : MonoBehaviour
{
    // Inventory info
    [SerializeField] private Dimension inventoryDimension;
    [HideInInspector] public Dimension slotDimension;

    // Inventory grid slots
    private InventorySlot[,] slotGrids;

    // Visual tree references
    [SerializeField] private VisualTreeAsset slotTemplate;

    // Inventory ui elements
    public VisualElement gridContainer;

    // Inventory state
    private InventorySlot currentSlot = null;
    private bool readyForLoadItems = false;
    private InventoryItem pickedItem = null;
    private bool outOfRange = false;

    // Stored items
    [SerializeField] private List<ItemSO> startupItems = new List<ItemSO>();
    private List<InventoryItem> storedItems = new List<InventoryItem>();

    private async void Start() 
    {
        GenerateSlots();

        await UniTask.WaitUntil(() => readyForLoadItems);

        LoadAllItems();
    }

    private void Update() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pickedItem != null)
            {
                PlaceItem();
            }
            else
            {
                PickItem();
            }
        }


        // pickedItem icon follow mouse cursor
        if (pickedItem != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;
            Vector2 newIconPosition = mousePosition - (pickedItem.iconView.layout.size / 2f);
            pickedItem.UpdateIconPosition(newIconPosition);
        }
    }

    private async void GenerateSlots()
    {
        gridContainer = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Grid");

        slotGrids = new InventorySlot[inventoryDimension.height, inventoryDimension.width];
        for (int y = 0; y < inventoryDimension.height; y++)
        {
            for (int x = 0; x < inventoryDimension.width; x++)
            {
                VisualElement slotView = slotTemplate.Instantiate().Children().First();
                gridContainer.Add(slotView);
                slotGrids[y, x] = new InventorySlot(new Vector2Int(y, x), slotView, this);
            }
        }
        
        await UniTask.WaitForEndOfFrame();

        slotDimension = new Dimension() {
            width = Mathf.RoundToInt(slotGrids[0, 0].slotView.worldBound.width),
            height = Mathf.RoundToInt(slotGrids[0, 0].slotView.worldBound.height)
        };

        readyForLoadItems = true;
    }

    private void LoadAllItems()
    {
        foreach (var item in startupItems)
        {
            InventoryItem newItem = new InventoryItem(item, this);

            bool notPlaced = true;

            for (int y = 0; notPlaced && y < inventoryDimension.height; y++)
            {
                for (int x = 0; notPlaced && x < inventoryDimension.width; x++)
                {
                    var (canPlace, slotList) = CanPlaceItem(item, new Vector2Int(y, x));

                    if (canPlace)
                    {
                        notPlaced = false; // Break out of the loop

                        newItem.origianlSlotID = new Vector2Int(y, x);

                        // Place item in the grid
                        foreach (var slot in slotList)
                        {
                            slotGrids[slot.slotID.x, slot.slotID.y].storedItem = newItem;
                        }

                        newItem.UpdateIconPosition(slotGrids[y, x].slotView.worldBound.position);
                    }
                }
            }
            
            if (notPlaced == false)
            {
                storedItems.Add(newItem);
            }
        }
    }

    void PickItem()
    {
        if (currentSlot == null || currentSlot.storedItem == null)
        {
            return;
        }


        pickedItem = currentSlot.storedItem;

        var (_, slotList) = CanPlaceItem(pickedItem.item, pickedItem.origianlSlotID);

        foreach (var slot in slotList)
        {
            slot.storedItem = null;
        }

        OnEnterSlot(slotGrids[pickedItem.origianlSlotID.x, pickedItem.origianlSlotID.y]);

        pickedItem.iconView.BringToFront();
    }

    void PlaceItem()
    {
        if (currentSlot == null)
        {
            return;
        }

        var (canPlace, slotList) = CanPlaceItem(pickedItem.item, currentSlot.slotID);

        if (canPlace)
        {
            foreach (var slot in slotList)
            {
                slot.storedItem = pickedItem;
            }

            pickedItem.origianlSlotID = currentSlot.slotID;
            pickedItem.UpdateIconPosition(currentSlot.slotView.worldBound.position);
        }
        else
        {
            (canPlace, slotList) = CanPlaceItem(pickedItem.item, pickedItem.origianlSlotID);

            foreach (var slot in slotList)
            {
                slot.storedItem = pickedItem;
            }
            pickedItem.UpdateIconPosition(slotGrids[pickedItem.origianlSlotID.x, pickedItem.origianlSlotID.y].slotView.worldBound.position);
        }


        pickedItem = null;
    }

    public void OnEnterSlot(InventorySlot slot)
    {
        currentSlot = slot;

        if (pickedItem != null)
        {
            var (_, slotList) = CanPlaceItem(pickedItem.item, slot.slotID);

            foreach (var s in slotList)
            {
                if (s.storedItem == null && outOfRange == false)
                {
                    s.SetColor(InventorySlot.SlotState.Vacant);
                }
                else
                {
                    s.SetColor(InventorySlot.SlotState.Occupied);
                }
            }
        }
    }

    public void OnLeaveSlot(InventorySlot slot)
    {
        ClearGrid();

        if (currentSlot == slot)
        {
            currentSlot = null;
        }
    }

    /// <summary>
    /// Reset slots color
    /// </summary>
    private void ClearGrid()
    {
        for (int y = 0; y < inventoryDimension.height; y++)
        {
            for (int x = 0; x < inventoryDimension.width; x++)
            {
                slotGrids[y, x].SetColor(InventorySlot.SlotState.Default);
            }
        }
    }
    
    /// <summary>
    /// Check if the item can be placed in the grid at position (y, x).
    /// return bool and list of slot that is stored item
    /// </summary>
    private (bool, List<InventorySlot>) CanPlaceItem(ItemSO item, Vector2Int slotID)
    {
        bool canPlace = true;
        List<InventorySlot> slots = new List<InventorySlot>();
        outOfRange = false;

        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 4; ++x)
            {
                if (item[y, x] == false)
                {
                    continue;
                }

                int ny = y + slotID.x;
                int nx = x + slotID.y;

                if (ny < 0 || ny >= inventoryDimension.height || nx < 0 || nx >= inventoryDimension.width)
                {
                    canPlace = false;
                    outOfRange = true;
                    continue;
                }

                if (slotGrids[ny, nx].storedItem != null && slotGrids[ny, nx].storedItem.item != item)
                {
                    canPlace = false;
                }

                slots.Add(slotGrids[ny, nx]);
            }
        }

        return (canPlace, slots);
    }
}

[Serializable]
public struct Dimension
{
    public int width, height;
}