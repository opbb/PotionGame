using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    public int recipeIndex;
    private RecipeManager recipeManager;

    private void Start()
    {
        recipeManager = FindObjectOfType<RecipeManager>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(BrewPotion);
    }

    private void BrewPotion()
    {
        recipeManager.BrewPotion(recipeIndex);
    }
}