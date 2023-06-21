using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableBehavior : MonoBehaviour
{
    [SerializeField] private ItemDefinition thisItemDef;
    [SerializeField] private bool isIndestructible = false;

    private void OnDestroy()
    {
        PlayerInventory.Instance.OpenInventoryWithItem(thisItemDef);
    }

    public bool IsIndestructible()
    {
        return isIndestructible;
    }

    // Called for indestructable items 
    public void OpenInventoryWith()
    {
        PlayerInventory.Instance.OpenInventoryWithItem(thisItemDef);
    }
}
