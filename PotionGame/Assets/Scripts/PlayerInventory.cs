using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    // A static variable allowing any script to access the inventory.
    [HideInInspector] public static PlayerInventory Instance;

    // Temporary way of storing inventory, for testing
    // TODO: Make real inventory
    private ArrayList invList;
    
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
        invList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StoreInInv(string itemName)
    {
        invList.Add(itemName);
    }

    public void ShowInv()
    {
        string invMsg = "Inventory: ";
        bool first = true;
        foreach(string itemName in invList)
        {
            if(!first)
            {
                invMsg = invMsg + ", ";
            } else
            {
                first = false;
            }
            invMsg = invMsg + itemName;
        }
        invMsg = invMsg + ".";
        Debug.Log(invMsg);
    }

    // Methods below copied from tutorial, I will finish them later
    // TODO: Implement inventory
    private async void Configure()
    {
        /*
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        m_ItemDetailHeader = itemDetails.Q<Label>("Header");
        m_ItemDetailBody = itemDetails.Q<Label>("Body");
        m_ItemDetailPrice = itemDetails.Q<Label>("SellPrice");
        await UniTask.WaitForEndOfFrame();
        ConfigureSlotDimensions();
        m_IsInventoryReady = true;
        */
    }
    private void ConfigureSlotDimensions()
    {
        /*
        VisualElement firstSlot = m_InventoryGrid.Children().First();
        SlotDimension = new Dimensions
        {
            Width = Mathf.RoundToInt(firstSlot.worldBound.width),
            Height = Mathf.RoundToInt(firstSlot.worldBound.height)
        };
        */
    }
}