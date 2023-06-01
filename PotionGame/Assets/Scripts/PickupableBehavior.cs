using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableBehavior : MonoBehaviour
{
    [SerializeField] private ItemDefinition thisItemDef;
    private void OnDestroy()
    {
        PlayerInventory.Instance.OpenInventoryWithItem(thisItemDef);
    }
}
