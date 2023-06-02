using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeManager : MonoBehaviour
{
    public List<Button> recipeButtons;
    public List<Recipe> recipes;

    public PlayerInventory inventory;
    public Text brewFailedText; 

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
            brewFailedText.gameObject.SetActive(false);
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
            b.gameObject.gameObject.SetActive(isActive);
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
            brewFailedText.gameObject.SetActive(true);
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

        // Check if the inventory has all the required ingredients
        foreach (StoredItem requiredItem in requiredIngredients)
        {
            if (!inventoryCount.ContainsKey(requiredItem.Details.CommonName) || inventoryCount[requiredItem.Details.CommonName] == 0)
            {
               
                return false;
            }

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
           
            print("Removing " + requiredItem.Details.CommonName + " from inventory");
        }
    }

    private void StartBrewingPotion(Recipe recipe)
    {
        // Hide the crafting buttons
        toggleUIButtons(false);

        // show the inventory
        inventory.OpenInventoryWithItem(recipe.resultingItem.Details);
        Debug.Log("brewing " + recipe.name);
    }
}
