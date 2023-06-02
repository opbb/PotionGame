using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;

/*
 * PlayerInventoryView defines the functionality for the players inventory UI. It does not track the inventory itself.
 */
public sealed class PlayerInventoryView : MonoBehaviour
{

    [SerializeField] private readonly string dragButton = "Fire1";
    [SerializeField] private KeyCode inventoryKey = KeyCode.E;

    // A static variable allowing any script to access the inventory UI.
    [HideInInspector] public static PlayerInventoryView Instance;

    private HashSet<StoredItem> looseItems;

    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;
    private VisualElement m_WholeScreen;
    private VisualElement m_LooseCenter;
    private MouseTracker mouseTracker;
    //private static Label m_ItemDetailHeader;
    //private static Label m_ItemDetailBody;
    //private static Label m_ItemDetailPrice;
    private bool m_IsInventoryReady;
    public static Dimensions SlotDimension { get; private set; }

    private VisualElement m_Telegraph;

    private bool isDragging = false;
    private StoredItem draggedItem = null;


    // =======================================
    // ========= Configuration/Setup =========
    // =======================================

    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one PlayerInventoryView
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

    // Methods below copied from tutorial, I will finish them later
    // TODO: Implement inventory
    private async void Configure()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        m_WholeScreen = m_Root.Q<VisualElement>("WholeScreen");
        m_LooseCenter = m_Root.Q<VisualElement>("LooseCenter");
        looseItems = new HashSet<StoredItem>();
        
        //VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        //m_ItemDetailHeader = itemDetails.Q<Label>("Header");
        //m_ItemDetailBody = itemDetails.Q<Label>("Body");
        //m_ItemDetailPrice = itemDetails.Q<Label>("SellPrice");
        //await UniTask.WaitForEndOfFrame();

        ConfigureInventoryTelegraph();
        ConfigureMouseTracker();

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

    private static void ConfigureInventoryItem(StoredItem item, ItemVisual visual)
    {
        item.RootVisual = visual;
        visual.style.visibility = Visibility.Visible;
    }

    private void ConfigureMouseTracker()
    {
        mouseTracker = new MouseTracker();
        mouseTracker.AddToClassList("mouse-tracker");
        mouseTracker.BringToFront();
        mouseTracker.pickingMode = PickingMode.Ignore;
        m_WholeScreen.Add(mouseTracker);
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

    // Is called by PlayerInventory.LoadInventory() once it finishes.
    public async void LoadInventory()
    {
        await UniTask.WaitUntil(() => m_IsInventoryReady);
        List<StoredItem> invList = PlayerInventory.Instance.GetItemsInInventory();

        foreach (StoredItem loadedItem in invList)
        {
            ItemVisual inventoryItemVisual = new ItemVisual(loadedItem);

            AddItemToInventoryGrid(inventoryItemVisual);
            SetItemPosition(inventoryItemVisual, new Vector2(SlotDimension.Width * loadedItem.position.x,
                    SlotDimension.Height * loadedItem.position.y));
            ConfigureInventoryItem(loadedItem, inventoryItemVisual);
        }
        DisableInventoryView();// Disable the inventory once it's loaded
    }

 
    // ================================
    // ========= Player Input =========
    // ================================

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            draggedItem.RootVisual.MoveToMouse(mouseTracker.mousePosition);
            if (!Input.GetButton(dragButton))
            {
                isDragging = false;
                draggedItem.RootVisual.TryPlace();
                mouseTracker.pickingMode = PickingMode.Ignore;
            }
        }

        if (Input.GetKeyDown(inventoryKey))
        {
            if (m_Root.style.display == DisplayStyle.Flex)
            {
                DisableInventoryView();
            }
            else
            {
                EnableInventoryView();
            }
        }
    }

    // ========================================
    // ========= Inventory Management =========
    // ========================================

    public void EnableInventoryView()
    {
        if (Instance != null)
        {
            m_Root.style.display = DisplayStyle.Flex;
            MouseLook.isUIActive = true;
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            SwitchLooseCenterItemsToGrid();
        }
    }

    public void DisableInventoryView()
    {
        if (Instance != null)
        {
            m_Root.style.display = DisplayStyle.None;
            MouseLook.isUIActive = false;
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            ClearLooseItems();
        }
    }

    // Loads adds an item to the left panel, outside of the inventory grid
    public void AddLooseItem(StoredItem item)
    {
        ItemVisual looseItemVisual = new ItemVisual(item);

        AddItemToLooseCenter(looseItemVisual);
        SetItemPosition(looseItemVisual, new Vector2(0,0));
        ConfigureInventoryItem(item, looseItemVisual);
        looseItems.Add(item);
    }

    public void ClearLooseItems()
    {
        foreach(StoredItem item in looseItems)
        {
            RemoveItemFromInventoryGrid(item.RootVisual);
        }
        looseItems.Clear();
    }

    public void RemoveItem(StoredItem item)
    {
        looseItems.Remove(item);
        RemoveItemFromInventoryGrid(item.RootVisual);
    }

    public void MakeItemLoose(StoredItem item)
    {
        looseItems.Add(item);
    }

    public void MakeItemNotLoose(StoredItem item)
    {
        looseItems.Remove(item);
    }

    // ===========================
    // ========= Helpers =========
    // ===========================

    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    private void AddItemToInventoryGrid(VisualElement item) => m_InventoryGrid.Add(item);
    private void RemoveItemFromInventoryGrid(VisualElement item) => m_InventoryGrid.Remove(item);
    private void AddItemToLooseCenter(VisualElement item) => m_LooseCenter.Add(item);
    private void RemoveItemFromLooseCenter(VisualElement item) => m_LooseCenter.Remove(item);

    public void StartDragging(StoredItem item)
    {
        if (!isDragging)
        {
            isDragging = true;
            draggedItem = item;
            mouseTracker.pickingMode = PickingMode.Position;

            // So that the item doesn't teleport to the old mouse position before the mouse position updates
            mouseTracker.mousePosition = draggedItem.RootVisual.GetCenterPosition();
        }
    }

    // Changes the parent of every child of loose center to be a child of inventory grid
    private async void SwitchLooseCenterItemsToGrid()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        List<VisualElement> looseCenterChildren = new List<VisualElement>(m_LooseCenter.Children());

        foreach(VisualElement child in looseCenterChildren)
        {
            RemoveItemFromLooseCenter(child);
            AddItemToInventoryGrid(child);
            SetItemPosition(child, m_LooseCenter.layout.position -m_InventoryGrid.parent.layout.position);
        }
    }

    // WIP
    /*
    public void ShowPlacementTarget(ItemVisual draggedItem)
    {

        int xPos = Mathf.RoundToInt(layout.x) + (PlayerInventoryView.SlotDimension.Width / 2);
        int yPos = Mathf.RoundToInt(layout.y) + (PlayerInventoryView.SlotDimension.Height / 2);

        // Integer devision to chop off decimal
        int xSlot = xPos / PlayerInventoryView.SlotDimension.Width;
        int ySlot = yPos / PlayerInventoryView.SlotDimension.Height;

        if (PlayerInventory.InvPos.IsValid(xSlot, ySlot))
        {
            // Set up telegraph to proper shape and size
            m_Telegraph.style.width = draggedItem.style.width;
            m_Telegraph.style.height = draggedItem.style.height;
            // Set up texture
            m_Telegraph.

            m_Telegraph.style.visibility = Visibility.Visible;
        }
        else
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
        }
    }
    */
}

