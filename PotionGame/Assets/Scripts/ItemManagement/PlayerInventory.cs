using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;

public sealed class PlayerInventory : MonoBehaviour
{
    // A static variable allowing any script to access the inventory.
    [HideInInspector] public static PlayerInventory Instance;

    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;
    private static Label m_ItemDetailHeader;
    private static Label m_ItemDetailBody;
    private static Label m_ItemDetailPrice;
    private bool m_IsInventoryReady;
    public static Dimensions SlotDimension { get; private set; }


    public List<StoredItem> StoredItems = new List<StoredItem>();
    public Dimensions InventoryDimensions;

    private VisualElement m_Telegraph;


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
        LoadInventory();
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
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        //VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        //m_ItemDetailHeader = itemDetails.Q<Label>("Header");
        //m_ItemDetailBody = itemDetails.Q<Label>("Body");
        //m_ItemDetailPrice = itemDetails.Q<Label>("SellPrice");
        //await UniTask.WaitForEndOfFrame();

        ConfigureInventoryTelegraph();

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        ConfigureSlotDimensions();
        m_IsInventoryReady = true;
    }
    private void ConfigureSlotDimensions()
    {
        VisualElement firstSlot = m_InventoryGrid.Children().First();
        SlotDimension = new Dimensions
        {
            Width = Mathf.RoundToInt(firstSlot.worldBound.width),
            Height = Mathf.RoundToInt(firstSlot.worldBound.height)
        };
    }
    private async Task<bool> GetPositionForItem(VisualElement newItem)
    {
        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                //try position
                SetItemPosition(newItem, new Vector2(SlotDimension.Width * x,
                    SlotDimension.Height * y));
                //await UniTask.WaitForEndOfFrame();
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                StoredItem overlappingItem = StoredItems.FirstOrDefault(s =>
                    s.RootVisual != null &&
                    s.RootVisual.layout.Overlaps(newItem.layout));
                //Nothing is here! Place the item.
                if (overlappingItem == null)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    private async void LoadInventory()
    {
        await UniTask.WaitUntil(() => m_IsInventoryReady);
        foreach (StoredItem loadedItem in StoredItems)
        {
            ItemVisual inventoryItemVisual = new ItemVisual(loadedItem.Details);

            AddItemToInventoryGrid(inventoryItemVisual);
            bool inventoryHasSpace = await GetPositionForItem(inventoryItemVisual);
            if (!inventoryHasSpace)
            {
                Debug.Log("No space - Cannot pick up the item");
                RemoveItemFromInventoryGrid(inventoryItemVisual);
                continue;
            }
            ConfigureInventoryItem(loadedItem, inventoryItemVisual);
        }
    }
    private void AddItemToInventoryGrid(VisualElement item) => m_InventoryGrid.Add(item);
    private void RemoveItemFromInventoryGrid(VisualElement item) => m_InventoryGrid.Remove(item);
    private static void ConfigureInventoryItem(StoredItem item, ItemVisual visual)
    {
        item.RootVisual = visual;
        visual.style.visibility = Visibility.Visible;
    }

    private void ConfigureInventoryTelegraph()
    {
        m_Telegraph = new VisualElement
        {
            name = "Telegraph",
            style =
        {
            position = Position.Absolute,
            visibility = Visibility.Hidden
        }
        };
        m_Telegraph.AddToClassList("slot-icon-highlighted");
        AddItemToInventoryGrid(m_Telegraph);
    }

    public (bool canPlace, Vector2 position) ShowPlacementTarget(ItemVisual draggedItem)
    {
        if (!m_InventoryGrid.layout.Contains(new Vector2(draggedItem.localBound.xMax,
            draggedItem.localBound.yMax)))
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
            return (canPlace: false, position: Vector2.zero);
        }
        VisualElement targetSlot = m_InventoryGrid.Children().Where(x =>
            x.layout.Overlaps(draggedItem.layout) && x != draggedItem).OrderBy(x =>
            Vector2.Distance(x.worldBound.position,
            draggedItem.worldBound.position)).First();
        m_Telegraph.style.width = draggedItem.style.width;
        m_Telegraph.style.height = draggedItem.style.height;
        SetItemPosition(m_Telegraph, new Vector2(targetSlot.layout.position.x,
            targetSlot.layout.position.y));
        m_Telegraph.style.visibility = Visibility.Visible;
        var overlappingItems = StoredItems.Where(x => x.RootVisual != null &&
            x.RootVisual.layout.Overlaps(m_Telegraph.layout)).ToArray();
        if (overlappingItems.Length > 1)
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
            return (canPlace: false, position: Vector2.zero);
        }
        return (canPlace: true, targetSlot.worldBound.position);
    }

    [Serializable]
    public class StoredItem
    {
        public ItemDefinition Details;
        public ItemVisual RootVisual;
    }
}