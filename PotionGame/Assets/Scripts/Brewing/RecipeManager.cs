using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RecipeManager : MonoBehaviour, IBrewingInteractable, IGUIScreen
{
    [SerializeField] List<Button> recipeButtons;
    [SerializeField] List<Recipe> recipes;

    [SerializeField] PlayerInventory inventory;
    [SerializeField] TMP_Text brewFailedText;

    [HideInInspector] public static RecipeManager Instance;

    // Remembers whether this screen is currently active
    private bool isActive;
    private int currentPage = 0;

    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one RecipeManager
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }


    // IGUIScreen implementation
    public bool isGUIActive()
    {
        return isActive;
    }

    public void activateGUI()
    {
        isActive = true;
        gameObject.SetActive(true);
        brewFailedText.gameObject.SetActive(false);
        updateButtonText();
    }

    public void deactivateGUI()
    {
        isActive = false;
        gameObject.SetActive(false);
        brewFailedText.gameObject.SetActive(false);
    }

    public void updateButtonText()
    {
        int currentRecipe = currentPage * 4;
        // Determine the text each button should display based on the current page index
        foreach (Button b in recipeButtons)
        {
            string ingredientsText = "";
            TMP_Text buttonTextBox = b.GetComponentInChildren<TMP_Text>(true);
            
            // Get all Image components including nested ones
            Image[] buttonImages = b.GetComponentsInChildren<Image>(true);

            // Filter out the parent GameObject from the results
            Image buttonImage = buttonImages.FirstOrDefault(image => image.gameObject != b.gameObject);

            if (!(currentRecipe >= recipes.Count))
            {
                // Set the potion name
                ingredientsText = recipes[currentRecipe].recipeName + "\n";

                // Set the recipe ingredients
                int numberOfIngredients = recipes[currentRecipe].requiredIngredients.Count;

                for (int i = 0; i < numberOfIngredients; i++)
                {
                    ItemDefinition ingredient = recipes[currentRecipe].requiredIngredients[i].Details;
                    ingredientsText += ingredient.CommonName + "\n";
                }

                // Set the potion image
                Sprite potionSprite = recipes[currentRecipe].resultingItem.Details.Icon;
                buttonImage.sprite = potionSprite;

                // Enable the image component
                buttonImage.enabled = true;
            }
            else
            {
                // Disable the image component when there is no recipe
                buttonImage.enabled = false;
            }

            buttonTextBox.text = ingredientsText;
            currentRecipe++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (inventory == null)
        {
            inventory = PlayerInventory.Instance;
        }

        if (recipeButtons.Count <= 0)
        {
            Debug.Log("There are no buttons attached to the recipe manager");
        }

        gameObject.SetActive(false);

    }

    public void BrewPotion(int recipeIndex)
    {
        Recipe recipe = recipes[recipeIndex + (currentPage * 4)];
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
        deactivateGUI();

        // show the inventory
        inventory.OpenInventoryWithItem(recipe.resultingItem.Details);
        Debug.Log("brewing " + recipe.name);
    }

  
    // IBrewingInteractable implementation

    public void PrintTestMessage()
    {
        Debug.Log("Test message from RecipeManager.");
    }

    public void UpdatePageIndex(int amount)
    {

        if (((currentPage * 4) < recipes.Count - 4) && (amount > 0))
        {
            currentPage += amount;
            updateButtonText();
        }
        else if ((currentPage >= 1) && (amount < 0))
        {
            currentPage += amount;
            updateButtonText();
        }
    }
}
