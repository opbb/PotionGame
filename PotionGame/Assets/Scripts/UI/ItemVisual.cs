using UnityEngine;
using UnityEngine.UIElements;
using System;
public class ItemVisual : VisualElement
{
    private readonly ItemDefinition m_Item;

    // Movement elements
    private Vector2 m_OriginalPosition;
    private bool m_IsDragging;
    private (bool canPlace, Vector2 position) m_PlacementResults;
    private readonly StoredItem thisItem;


    public ItemVisual(StoredItem item)
    {
        thisItem = item;
        m_Item = item.Details;
        name = $"{m_Item.CommonName}";
        style.height = m_Item.SlotDimension.Height *
            PlayerInventoryView.SlotDimension.Height;
        style.width = m_Item.SlotDimension.Width *
            PlayerInventoryView.SlotDimension.Width;
        style.visibility = Visibility.Hidden;
        VisualElement icon = new VisualElement
        {
            style = { backgroundImage = m_Item.Icon.texture }
        };
        icon.AddToClassList("visual-icon");
        Add(icon);
        AddToClassList("visual-icon-container");

        RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
    }

    ~ItemVisual()
    {
        UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }

    public void TryPlace()
    {
        int xPos = Mathf.RoundToInt(layout.x) + (PlayerInventoryView.SlotDimension.Width / 2);
        int yPos = Mathf.RoundToInt(layout.y) + (PlayerInventoryView.SlotDimension.Height / 2);

        // Integer devision to chop off decimal
        int xSlot = xPos / PlayerInventoryView.SlotDimension.Width;
        int ySlot = yPos / PlayerInventoryView.SlotDimension.Height;

        // First, if the item is currently in the inventory, remove it and record its position in case we have to put it back.
        PlayerInventory.InvPos originalPosition = null;
        bool startedInInventory = false;
        if (thisItem.position != null)
        {
            originalPosition = thisItem.position;
            startedInInventory = true;
            PlayerInventory.UnsafeMethods.RemoveFromInventory(thisItem);
        }

        // Next, if the item is within the inventory, deal with the inventory
        if (PlayerInventory.InvPos.IsValid(xSlot,ySlot))
        {
            // Try to place the item
            PlayerInventory.InvPos position = new PlayerInventory.InvPos(xSlot, ySlot);
            bool successful = PlayerInventory.UnsafeMethods.AddToInventory(position, thisItem);
            
            if(successful)
            {
                // Place item visual
                MoveToGridSlot(xSlot, ySlot);
                PlayerInventoryView.Instance.MakeItemNotLoose(thisItem);
            } else
            {
                // Return item visual to original position
                SetPosition(new Vector2(m_OriginalPosition.x, m_OriginalPosition.y));

                // If the item was originally in the inventory, re-add it
                if (startedInInventory)
                {
                    successful = PlayerInventory.UnsafeMethods.AddToInventory(originalPosition, thisItem);

                    if(!successful)
                    {
                        throw new InvalidOperationException("The inventory item was unable to be re-added after it couldn't be placed.");
                    }
                }
            }
        } else
        {
            // If the item is outside of the inventory, then move it to the subscreen
            PlayerInventoryView.Instance.MoveItemToSubscreen(thisItem);
        }

        // This will show you what the actual PlayerInventory array looks like so you can confirm that it matches the visuals
        //PlayerInventory.Instance.LogInventory();
    }

    public void MoveToMouse(Vector2 mousePosition)
    {
        SetPosition(GetMousePosition(mousePosition));
        //m_PlacementResults = PlayerInventoryView.Instance.ShowPlacementTarget(this);
    }

    // Translates a position in screen space into a position relative to the parent such that
    // if you placed the element there, the mouse would be at the center
    public Vector2 GetMousePosition(Vector2 mousePosition) {
        float xPos = mousePosition.x - (layout.width / 2) - parent.worldBound.position.x;
        float yPos = mousePosition.y - (layout.height / 2) - parent.worldBound.position.y;
        return new Vector2(xPos,yPos);
        }

    public Vector2 GetCenterPosition()
    {
        float xPos = parent.worldBound.position.x + layout.x + (layout.width / 2);
        float yPos = parent.worldBound.position.y + layout.y + (layout.height / 2);
        return new Vector2(xPos, yPos);
    }

    private void OnMouseDownEvent(MouseDownEvent mouseEvent)
    {
        if (mouseEvent.button == 0) {
            StartDrag();
        } else if (mouseEvent.button == 1 && m_Item.effect != null) {
            ApplyEffect();
        }
    }

    private void StartDrag()
    {
        m_OriginalPosition = worldBound.position - parent.worldBound.position;
        BringToFront();
        PlayerInventoryView.Instance.StartDragging(thisItem);
    }

    private void ApplyEffect()
    {
        PotionEffectManager.Instance.ApplyEffect(m_Item.effect);
        PlayerInventory.Instance.TryTakeOutItem(m_Item);
    }

    public void MoveToGridSlot(int x, int y)
    {
        style.left = PlayerInventoryView.SlotDimension.Width * x;
        style.top = PlayerInventoryView.SlotDimension.Height * y;
    }
}