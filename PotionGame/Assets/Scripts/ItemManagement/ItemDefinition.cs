using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]

/*
 * ItemDefinition defines the properties of an item, which can be stored in the player inventory.
 */
public class ItemDefinition : ScriptableObject
{
    public string ID = Guid.NewGuid().ToString();
    public string CommonName;
    public string Description;
    public Sprite Icon;
    private Sprite PlacementTelegraph;
    public Dimensions SlotDimension;
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
    //Coordinate array defining what areas the item DOESNT occupy within its height and width dimensions.
    //public (int, int)[] EmptySpaces;
}