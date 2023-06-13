using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIController : MonoBehaviour
{
    // The one instance of this class
    public static UIController Instance;

    // IGUIScreens to manage
    private PlayerInventory playerInventory;
    private QuestManager questManager;
    private RecipeManager recipeManager;
    private Herbarium herbariumManager;
    [SerializeField] private MortarAndPestleView mortarAndPestle;

    // Input keys
    [SerializeField] private KeyCode closeUIKey = KeyCode.Escape;
    [SerializeField] private KeyCode inventoryKey = KeyCode.E;
    [SerializeField] private KeyCode recipeKey = KeyCode.F;
    [SerializeField] private KeyCode herbariumKey = KeyCode.C;

    // Tells if any GUI screen is currently active
    private bool isAnyGUIActive { get => activeScreen != null; }
    private bool isAnyInventorySubscreenActive { get => activeInventorySubscreen != null; }
    // Remember the current GUI Screen which is active
    private IGUIScreen activeScreen;
    private IGUIInventorySubscreen activeInventorySubscreen;


    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one UIController
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

    // Start is called before the first frame update
    void Start()
    {
        playerInventory = PlayerInventory.Instance;
        mortarAndPestle.NestInsideInventoryLoose();
        questManager = GetComponent<QuestManager>();
        herbariumManager = Herbarium.Instance;
        questManager = GetComponent<QuestManager>();
        recipeManager = RecipeManager.Instance;
       
        if (playerInventory == null || questManager == null || recipeManager == null || herbariumManager == null || mortarAndPestle == null)
        {
            throw new InvalidOperationException("The UI Controller cant find all of the GUIScreens, " +
                "they are probably not attached to the same GameObject, which they should be.");
        }
        activeScreen = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnyGUIActive)
        {
            // If a UI is open, check if we should close it
            if (Input.GetKeyDown(closeUIKey))
            {
                // IF escape is pressed, close the UI no matter what
                DeactivateGUIScreen();
            }
            else if (playerInventory.isGUIActive() && Input.GetKeyDown(inventoryKey))
            {
                // If the inventory key is pressed while it is open, close it
                DeactivatePlayerInventory();
            }
        }
        else
        {
            if (Input.GetKeyDown(recipeKey) && InRange.isInRange)
            {
                InRange.nearestBrewingInteractableOrNull.OpenBrewingGUI();
            }
            else if (Input.GetKeyDown(inventoryKey))
            {
                ActivatePlayerInventory();
            }
            else if (Input.GetKeyDown(herbariumKey))
            {
                ActivateHerbarium();
            }
        } 
        
    }

    // ======= UI Specific Helpers =======

    // Activates the QuestManager GUI if it is safe to do so, returning true if it does.
    public bool ActivateQuestManager()
    {
        return ActivateGUIScreen(questManager);
    }

    // Deactivates the QuestManager if it is the active screen.
    public void DeactivateQuestManager()
    {
        if (ReferenceEquals(activeScreen, questManager))
        {
            DeactivateGUIScreen();
        }
    }

    // Activates the RecipeManager GUI if it is safe to do so, returning true if it does.
    public bool ActivateRecipeManager()
    {
        return ActivateGUIScreen(recipeManager);
    }

    // Deactivates the RecipeManager if it is the active screen.
    public void DeactivateRecipeManager()
    {
        if (ReferenceEquals(activeScreen, recipeManager))
        {
            DeactivateGUIScreen();
        }
    }

    // Activates the PlayerInventory GUI if it is safe to do so, returning true if it does.
    public bool ActivatePlayerInventory()
    {
        return ActivateGUIScreen(playerInventory);
    }

    // Deactivates the PlayerInventory if it is the active screen.
    public void DeactivatePlayerInventory()
    {
        if (ReferenceEquals(activeScreen, playerInventory))
        {
            DeactivateGUIScreen();
        }
    }

    public bool ActivateMortarAndPestle()
    {
        return ActivateInventorySubscreen(mortarAndPestle);
    }

    public void DeactivateMortarAndPestle()
    {
        if (ReferenceEquals(activeInventorySubscreen, mortarAndPestle))
        {
            DeactivateGUIScreen();
        }
    }

    // Activates the Herbarium GUI if it is safe to do so, returning true if it does.
    public bool ActivateHerbarium()
    {
        return ActivateGUIScreen(herbariumManager);
    }

    // Deactivates the PlayerInventory if it is the active screen.
    public void DeactivateHerbarium()
    {
        if (ReferenceEquals(activeScreen, playerInventory))
        {
            DeactivateGUIScreen();
        }
    }

    public IGUIInventorySubscreen GetActiveSubscreen()
    {
        return activeInventorySubscreen;
    }



    // ======= Generic Helpers =======


    private bool ActivateGUIScreen(IGUIScreen screen)
    {
        // Ensure other GUIs aren't active
        if (isAnyGUIActive)
        {
            // If this GUIScreen is already active, return true, otherwise return false
            return ReferenceEquals(screen, activeScreen);
        }

        // Set the new active screen
        activeScreen = screen;

        // Set up generic UI stuff
        EnterUIMode();

        // Activate the UI
        activeScreen.activateGUI();

        // Return succeess.
        return true;
    }

    private bool ActivateInventorySubscreen(IGUIInventorySubscreen subscreen)
    {

        if (ActivatePlayerInventory())
        {
            // Ensure other GUIs aren't active
            if (isAnyInventorySubscreenActive)
            {
                // If this GUIScreen is already active, return true, otherwise return false
                return ReferenceEquals(subscreen, activeInventorySubscreen);
            }

            // Set the new active screen
            activeInventorySubscreen = subscreen;

            // Activate the UI
            activeInventorySubscreen.activateGUI();

            // Return succeess.
            return true;
        } else
        {
            // If we can't open the inventory, return false.
            return false;
        }
    }

    private void DeactivateGUIScreen()
    {
        if(isAnyInventorySubscreenActive)
        {
            DeactivateInventorySubscreen();
        }
        activeScreen.deactivateGUI();
        activeScreen = null;
        ExitUIMode();
    }

    private void DeactivateInventorySubscreen()
    {
        activeInventorySubscreen.deactivateGUI();
        activeInventorySubscreen = null;
    }


    // Does the things that always acompany UI (activate mouse, deactivate movement, etc)
    private void EnterUIMode()
    {
        MouseLook.isUIActive = true;
        PlayerController.isUIActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Undoes the things that always acompany UI (deactivate mouse, activate movement, etc)
    private void ExitUIMode()
    {
        MouseLook.isUIActive = false;
        PlayerController.isUIActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool isUIActive()
    {
        return isAnyGUIActive;
    }
}
