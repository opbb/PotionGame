using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "New StringToPrefabMap", menuName = "Data/StringToPrefabMap")]

public class StringToItemDefMat
{
    [SerializeField] private ItemDefinition[] allItemDefinitions;
    private Hashtable stringToItemDefMap;

    public StringToItemDefMat()
    {

    }
}
