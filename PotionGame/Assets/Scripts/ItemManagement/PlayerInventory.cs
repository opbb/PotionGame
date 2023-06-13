using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * PlayerInventory manages the mechanics of the inventory, storing the data, making it accessible
 * to other scripts, and ensuring the Tetrising happens correctly.
 */
public class PlayerInventory : MonoBehaviour, IGUIScreen
{
    public static Dimensions InventoryDimensions { get; private set; }

    public List<ItemDefinition> startingItems;

    // The number of slots in the 
    private readonly int InventoryWidthSlotCount = 7;
    private readonly int InventoryHeightSlotCount = 8;

    // The array representing the grid of the inventory.
    private StoredItem[,] inventory;

    // A static variable allowing any script to access the inventory UI.
    [HideInInspector] public static PlayerInventory Instance;


    // =======================================
    // ========= Configuration/Setup =========
    // =======================================

    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one PlayerInventory
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Configure();

        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadInventory();
    }

    private void Configure()
    {
        ConfigureInventoryDimensions();
        ConfigureInventoryArray();
    }

    private void ConfigureInventoryDimensions()
    {
        InventoryDimensions = new Dimensions
        {
            Width = InventoryWidthSlotCount,
            Height = InventoryHeightSlotCount
        };
    }

    private void ConfigureInventoryArray()
    {
        inventory = new StoredItem[InventoryWidthSlotCount, InventoryHeightSlotCount];
    }



    private void LoadInventory()
    {
        foreach(ItemDefinition item in startingItems)
        {
            StoredItem initItem = new StoredItem();
            initItem.Details = item;
            bool wasAdded = AutoAddItem(initItem);
            if(!wasAdded)
            {
                Debug.Log("Could not add item \"" + item.CommonName + "\" as it would not fit.");
            }
        }
        
        if(PlayerInventoryView.Instance != null) 
        {
            PlayerInventoryView.Instance.LoadInventory();
        }
        
    }

    // Adds the given item to the first open slot in the inventory. Notably does not add any visual for the item. Used for initial setup.
    private bool AutoAddItem(StoredItem newItem)
    {
        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                //try position
                bool successful = AddToInventory(new InvPos(x, y), newItem);

                if (successful)
                {
                    return true;
                }
            }
        }

        return false;
    }

    

    // ================================
    // ========= Safe For Use =========
    // ================================

    // Note: These methods are friendly in that they maintain the sync between the PlayerInventory and the PlayerInventoryView,
    //       so they should be safe to use as you please.

    // Checks that all the slots occupied by the given dimensions at the given position are empty.
    public bool AreSlotsOpen(InvPos position, Dimensions dimensions)
    {
        Func<int, int, bool> lambda = (x, y) => inventory[x, y] == null;

        ApplyToSlotsResults results = ApplyToItemSlots(position, dimensions, lambda);

        return results == ApplyToSlotsResults.LambdaSuccess;
    }

    // OVERLOAD: Checks that all the slots occupied by the given item at the given position are empty.
    public bool AreSlotsOpen(InvPos position, StoredItem item)
    {
        Dimensions dim = item.Details.SlotDimension;
        return AreSlotsOpen(position, dim);
    }

    public bool AreSlotsItem(InvPos position, StoredItem item)
    {
        Dimensions dim = item.Details.SlotDimension;

        Func<int, int, bool> lambda = (x, y) => ReferenceEquals(inventory[x, y], item);

        ApplyToSlotsResults results = ApplyToItemSlots(position, dim, lambda);

        return results == ApplyToSlotsResults.LambdaSuccess;
    }

    public List<StoredItem> GetItemsInInventory()
    {
        List<StoredItem> invList = new List<StoredItem>();

        StoredItem currentItem = null;

        Predicate<StoredItem> sameAsCurrent = (itemInList) => ReferenceEquals(itemInList, currentItem);

        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                currentItem = inventory[x, y];
                if (currentItem != null && !invList.Exists(sameAsCurrent))
                {
                    invList.Add(currentItem);
                }
            }
        }

        return invList;
    }

    // Opens the inventory with the given item in the lefthand side, ready to be stored
    public void OpenInventoryWithItem(ItemDefinition itemDef)
    {
        StoredItem item = new StoredItem();
        item.Details = itemDef;
        PlayerInventoryView.Instance.AddLooseItem(item);
        PlayerInventoryView.Instance.EnableInventoryView();
    }
    
    // Opens the inventory with the given item in the lefthand side, ready to be stored
    public void OpenInventoryWithItems(List<ItemDefinition> itemDefs)
    {
        foreach (ItemDefinition itemDef in itemDefs)
        {
            StoredItem item = new StoredItem();
            item.Details = itemDef;
            PlayerInventoryView.Instance.AddLooseItem(item);
        }
        PlayerInventoryView.Instance.EnableInventoryView();
    }

    // If this type of item is present in the inventory then it will be removed, and the method will return true.
    // If it is not present, then the method will return false.
    public bool TryTakeOutItem(ItemDefinition itemDef)
    {
        StoredItem currentItem;

        // Check every cell for the desired item
        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                currentItem = inventory[x, y];

                if (currentItem != null && currentItem.Details.ID.Equals(itemDef.ID))
                {
                    // If we find it, remove it and return true
                    RemoveFromInventory(currentItem);
                    PlayerInventoryView.Instance.RemoveItem(currentItem);
                    return true;
                }
            }
        }

        // If we don't find it, return false
        return false;
    }

    // ==========================================
    // ========= Inventory Manipulation =========
    // ==========================================

    // Note: These methods are unfriendly in that they DO NOT maintain the sync between the PlayerInventory
    //       and the PlayerInventoryView, so proceeed with caution.

    // Locks the unsafe method behind a call to "UnsafeMethods" so people at least know they're unsafe
    public static class UnsafeMethods
    {

        public static bool AddToInventory(InvPos position, StoredItem item)
        {
            return PlayerInventory.Instance.AddToInventory(position, item);
        }

        public static void RemoveFromInventory(StoredItem item)
        {
            PlayerInventory.Instance.RemoveFromInventory(item);
        }
    }

    // Adds the given item to the inventory at the given position. Returns true if the item was added successfully, and false otherwise.
    private bool AddToInventory(InvPos position, StoredItem item)
    {
        Dimensions dim = item.Details.SlotDimension;

        if (AreSlotsOpen(position, dim))
        {
            Action<int, int> lambda = (x, y) =>
            {
                inventory[x,y] = item;
            };

            ApplyToSlotsResults results = ApplyToItemSlots(position, dim, lambda);
            item.position = position;

            return true;
        } else
        {
            return false;
        }
    }

    

    // Removes the given item, throws an error if the item is not in the inventory or if a slot being cleared does not contain the item
    private void RemoveFromInventory(StoredItem item)
    {
        Dimensions dim = item.Details.SlotDimension;

        if (item.position == null)
        {
            throw new InvalidOperationException("The given item's position is null. It is probably not in the inventory.");
        }

        if (AreSlotsItem(item.position, item))
        {
            Action<int, int> lambda = (x, y) =>
            {
                inventory[x, y] = null;
            };

            ApplyToSlotsResults results = ApplyToItemSlots(item.position, dim, lambda);
            item.position = null; // Item is no longer in inventory, so remove position
        } else
        {
            throw new InvalidOperationException("The given item's position is occupied by other items.");
        }
    }

    //Helpers

    // Applies the given lambda function to all the slots for the given item.
    // Returns ApplyToSlotsResults.OutOfBounds if the given position and dimensions were outside the inventory.
    // Otherwise, stores the boolean output of the lambda, and if there was a false, returns ApplyToSlotsResults.LambdaError
    // If there were only trues, returns ApplyToSlotsResults.LambdaSuccess
    private ApplyToSlotsResults ApplyToItemSlots(InvPos position, Dimensions dimensions, Func<int, int, bool> lambda)
    {
        if (position.x + dimensions.Width - 1 >= PlayerInventory.InventoryDimensions.Width ||
           position.y + dimensions.Height - 1 >= PlayerInventory.InventoryDimensions.Height) // Subtrack one from each to account for indexing from 0
        {
            return ApplyToSlotsResults.OutOfBounds;
        }

        bool lambdaSuccess = true;

        // Iterate through all affected slots
        for (int i = position.x; i < (position.x + dimensions.Width); i++)
        {
            for (int j = position.y; j < (position.y + dimensions.Height); j++)
            {
                lambdaSuccess = lambdaSuccess && lambda(i, j);
            }
        }

        if (lambdaSuccess)
        {
            return ApplyToSlotsResults.LambdaSuccess;
        }
        else
        {
            return ApplyToSlotsResults.LambdaError;
        }
    }

    // Overload to allow Actions (ie. lambda's that return void)
    private ApplyToSlotsResults ApplyToItemSlots(InvPos position, Dimensions dimensions, Action<int, int> action)
    {
        Func<int, int, bool> lambda = (x, y) =>
        {
            action(x, y);
            return true;
        };
        return ApplyToItemSlots(position, dimensions, lambda);
    }

    private enum ApplyToSlotsResults
    {
        OutOfBounds,
        LambdaError,
        LambdaSuccess
    }


    // ======================================
    // ========= Inventory Position =========
    // ======================================

    // A class storing a position within the player inventory. Ensures that the position is within bounds.
    public class InvPos
    {
        public readonly int x;
        public readonly int y;

        public InvPos(int x, int y)
        {
            // Check that the position is within bounds
            if(!IsValid(x,y))
            {
                throw new ArgumentException("The given coordinates (" + x + "," + y + ") are outside of the bounds of the inventory (" +
                    PlayerInventory.InventoryDimensions.Width  + "," + PlayerInventory.InventoryDimensions.Height  + ".");
            }

            this.x = x;
            this.y = y;
        }

        public static bool IsValid(int x, int y) =>
            x >= 0 && y >= 0 && x < PlayerInventory.InventoryDimensions.Width && y < PlayerInventory.InventoryDimensions.Height;
    }

    // =============================================
    // ========= IGUIScreen Implementation =========
    // =============================================

    public bool isGUIActive()
    {
        return PlayerInventoryView.Instance.isGUIActive();
    }

    public void activateGUI()
    {
        PlayerInventoryView.Instance.activateGUI();
    }

    public void deactivateGUI()
    {
        PlayerInventoryView.Instance.deactivateGUI();
    }


    //For testing
    public void LogInventory()
    {
        string outString = "";
        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                if(inventory[x,y] == null)
                {
                    outString = outString + " _ ";
                } else
                {
                    outString = outString + " " + inventory[x,y].Details.CommonName.Substring(0,1) + " ";
                }
            }
            outString = outString + "\n";
        }
        Debug.Log(outString);
    }
}

[Serializable]
public class StoredItem
{
    public PlayerInventory.InvPos position; // Should be null if the item is not in the inventory
    public ItemDefinition Details;
    public ItemVisual RootVisual;
}
