using System;
using UnityEngine;
using UnityEngine.UIElements;

public class InventorySlot
{
    // Slot ID
    public Vector2Int slotID;

    // Slot visual element
    public VisualElement slotView;
    
    // Inventory reference
    private Inventory inventory;

    // Slot state and item info
    public bool isHovering = false;
    public InventoryItem storedItem;
    public enum SlotState { Default, Occupied, Vacant }

    public InventorySlot(Vector2Int id, VisualElement view, Inventory ownerInventory)
    {
        slotID = id;
        slotView = view;
        inventory = ownerInventory;

        // Add event listeners for hover and click events
        slotView.RegisterCallback<PointerEnterEvent>(OnHover);
        slotView.RegisterCallback<PointerLeaveEvent>(OnUnhover);
    }

    ~InventorySlot()
    {
        // Unregister callbacks when the slot is destroyed
        slotView.UnregisterCallback<PointerEnterEvent>(OnHover);
        slotView.UnregisterCallback<PointerLeaveEvent>(OnUnhover);
    }

    private void OnHover(PointerEnterEvent evt)
    {
        isHovering = true;

        inventory.OnEnterSlot(this);
    }
    
    private void OnUnhover(PointerLeaveEvent evt)
    {
        isHovering = false;

        inventory.OnLeaveSlot(this);
    }

    public void SetColor(SlotState state)
    {
        // Set color based on the current state of the slot
        switch(state)
        {
            case SlotState.Default:
            {
                slotView.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
                break;
            }
            case SlotState.Occupied:
            {
                slotView.style.backgroundColor = new StyleColor(new Color(219f / 255f, 9f / 255f, 0, 0.2f));
                break;
            }
            case SlotState.Vacant:
            {
                slotView.style.backgroundColor = new StyleColor(new Color(35f / 255f, 101f / 255f, 51f / 255f, 0.2f));
                break;
            }
        }
    }
}