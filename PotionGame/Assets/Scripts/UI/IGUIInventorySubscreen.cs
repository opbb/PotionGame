using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IGUIInventorySubscreen : IGUIScreen
{
    // Adds the given item to the subscreen, or returns false if the item could not be added. Also, sends a return action which should tell the subscreen what to do
    // if it wants to send an item back to the inventory.
    public bool MoveItemFromInventoryToSubscreen(ItemDefinition item, Action<ItemDefinition> returnAction);
}
