using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeManager : MonoBehaviour
{
    public List<Button> recipeButtons;
    public List<Recipe> recipes;

    public PlayerInventory inventory;
   

    // Start is called before the first frame update
    void Start()
    {
        if (inventory == null)
        {
            inventory = gameObject.GetComponent<PlayerInventory>();
        }
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

    private bool CheckInventory(List<StoredItem> requiredIngredients)
    {
        List<StoredItem> invList = inventory.GetItemsInInventory();

        // Create a dictionary to store the count of each ingredient in the inventory
        Dictionary<string, int> inventoryCount = new Dictionary<string, int>();

        // Count the occurrences of each ingredient in the inventory
        foreach (StoredItem item in invList)
        {
            if (inventoryCount.ContainsKey(item.Details.CommonName))
            {
                inventoryCount[item.Details.CommonName]++;
            }
            else
            {
                inventoryCount[item.Details.CommonName] = 1;
            }
        }


        // Print all elements and their counts
        foreach (KeyValuePair<string, int> pair in inventoryCount)
        {
            Debug.Log("Item: " + pair.Key + ", Count: " + pair.Value);
        }

        // Check if the inventory has all the required ingredients
        foreach (StoredItem requiredItem in requiredIngredients)
        {
            if (!inventoryCount.ContainsKey(requiredItem.Details.CommonName) || inventoryCount[requiredItem.Details.CommonName] == 0)
            {
                // Required ingredient not found in inventory or not enough quantity
                return false;
            }

            // Decrement the count of the required ingredient in the inventory
            inventoryCount[requiredItem.Details.CommonName]--;
        }

        // All required ingredients found in the inventory
        return true;
    }

    private void RemoveIngredientsFromInventory(List<StoredItem> requiredIngredients)
    {
        foreach (StoredItem requiredItem in requiredIngredients)
        {
            inventory.TryTakeOutItem(requiredItem.Details);
            // Implement the logic to remove the required ingredient from the player's inventory
            // Modify the player's inventory accordingly
            // ...
            print("Removing " + requiredItem.Details.CommonName + " from inventory");
        }
    }

    private void StartBrewingPotion(Recipe recipe)
    {
        // TODO figure out how to add item
        //inventory.AutoAddItem(recipe.resultingItem);
        // Implement the logic to start brewing the potion
        // Use the properties of the recipe object to determine brewing time, effects, etc.
        Debug.Log("brewing " + recipe.name);
    }
}
