using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]
public class ItemDefinition : ScriptableObject
{
    public string ID = Guid.NewGuid().ToString();
    public string CommonName;
    public string Description;
    public Sprite Icon;
    public Dimensions SlotDimension;
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
}