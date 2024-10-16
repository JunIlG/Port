using System;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryItem
{
    // Detail of the item
    public ItemSO item;

    // Anchor for UI display
    public Vector2Int origianlSlotID = Vector2Int.zero;

    // Item icon visual element
    public VisualElement iconView;

    public InventoryItem(ItemSO inItem, Inventory inventory)
    {
        item = inItem;

        Dimension dimension = item.GetItemDimension();
        // Create a new UI element for the item icon
        iconView = new VisualElement
        {
            name = "ItemIcon",
            style = {
                        backgroundImage = item.icon.texture,
                        backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f)),
                        width = inventory.slotDimension.width * dimension.width,
                        height = inventory.slotDimension.height * dimension.height,
                        position = Position.Absolute
                    },
            pickingMode = PickingMode.Ignore
        };
        inventory.gridContainer.Add(iconView);
    }

    /// <summary>
    /// Update icon view position to new pos
    /// </summary>
    public void UpdateIconPosition(Vector2 newPos)
    {
        iconView.style.left = newPos.x - iconView.parent.worldBound.position.x;
        iconView.style.top = newPos.y - iconView.parent.worldBound.position.y;
    }
}