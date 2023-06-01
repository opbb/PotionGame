using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeManager : MonoBehaviour
{
    public List<Button> recipeButtons;
    public List<Recipe> recipes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MouseLook.isUIActive = false;
            Cursor.visible = false;

            toggleUIButtons(false);
        }

    }

    public void displayRecipies()
    {
        MouseLook.isUIActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        toggleUIButtons(true);
    }

    public void toggleUIButtons(bool isActive)
    {
        foreach (Button b in recipeButtons)
        {
            b.gameObject.SetActive(isActive);
        }
    }

    public void BrewPotion(int recipeIndex)
    {
        Recipe recipe = recipes[recipeIndex];


        if (CheckInventory(recipe.requiredIngredients))
        {
            RemoveIngredientsFromInventory(recipe.requiredIngredients);

            StartBrewingPotion(recipe);

        }
        else
        {
            Debug.Log("Missing ingredients! Cannot brew potion.");
        }
    }

    private bool CheckInventory(List<string> requiredIngredients)
    {
        // Implement the logic to check if the player has the required ingredients in their inventory
        // Return true if the player has all the required ingredients, otherwise return false
        print("checking inventory");
        return true;
    }

    private void RemoveIngredientsFromInventory(List<string> requiredIngredients)
    {
        // Implement the logic to remove the required ingredients from the player's inventory
        // Modify the player's inventory accordingly
        print("removing from inventory");
    }

    private void StartBrewingPotion(Recipe recipe)
    {
        // Implement the logic to start brewing the potion
        // Use the properties of the recipe object to determine brewing time, effects, etc.
        Debug.Log("brewing " + recipe.name);
    }
}
