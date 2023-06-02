using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe", order = 1)]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public int brewingTime;
    public List<StoredItem> requiredIngredients;
}