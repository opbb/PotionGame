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

    // A static variable allowing any script to access the inventory UI.
    [HideInInspector] public static PlayerInventoryView Instance;

    private HashSet<StoredItem> looseItems;

    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;
    private VisualElement m_WholeScreen;
    private VisualElement m_Container;
    private VisualElement m_Loose;
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

    // Whether or not the GUI is active
    public bool isActive { get; private set; }


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
        m_Container = m_Root.Q<VisualElement>("Container");
        m_Loose = m_Root.Q<VisualElement>("Loose");
        m_LooseCenter = m_Root.Q<VisualElement>("LooseCenter");
        looseItems = new HashSet<StoredItem>();
        
        //VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        //m_ItemDetailHeader = itemDetails.Q<Label>("Header");
        //m_ItemDetailBody = itemDetails.Q<Label>("Body");
        //m_ItemDetailPrice = itemDetails.Q<Label>("SellPrice");
        //await UniTask.WaitForEndOfFrame();

        //ConfigureInventoryTelegraph();
        ConfigureMouseTracker();

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        ConfigureSlotDimensions();
        m_IsInventoryReady = true;
        
        // Ensure the inventory starts closed
        deactivateGUI();
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
        mouseTracker.BringToFront();
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

        UIController.Instance.InitialSetupMortarAndPestle();
        DisableInventoryView();// Disable the inventory once it's loaded
    }



    // =================================
    // ========= Safe Teardown =========
    // =================================




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
    }

    // ========================================
    // ========= Inventory Management =========
    // ========================================

    public void EnableInventoryView()
    {
        UIController.Instance.ActivatePlayerInventory();
    }

    public void DisableInventoryView()
    {
        UIController.Instance.DeactivatePlayerInventory();
    }

    // Loads adds an item to the left panel, outside of the inventory grid
    public void AddLooseItem(StoredItem item)
    {
        ItemVisual looseItemVisual = new ItemVisual(item);

        // Hide it until we can place it
        looseItemVisual.style.visibility = Visibility.Hidden;

        AddItemToInventoryGrid(looseItemVisual);
        ConfigureInventoryItem(item, looseItemVisual);
        looseItems.Add(item);

        PlaceAndUnhideAfterFrame(looseItemVisual);
    }

    private async void PlaceAndUnhideAfterFrame(VisualElement visual)
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        SetItemPosition(visual, GetLooseCenterRelativeToGrid() - (visual.layout.size / 2f));
        visual.style.visibility = Visibility.Visible;
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

    public void MoveItemToSubscreen(StoredItem item)
    {
        IGUIInventorySubscreen currentSubscreen = UIController.Instance.GetActiveSubscreen();
        
        if (currentSubscreen == null)
        {
            MakeItemLoose(item);
        } else
        { 
            Action<ItemDefinition> returnAction = (ItemDefinition item) => PlayerInventory.Instance.OpenInventoryWithItem(item);

            if (currentSubscreen.MoveItemFromInventoryToSubscreen(item.Details, returnAction))
            {
                RemoveItem(item);
            } else
            {
                MakeItemLoose(item);
            }
        }
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

    private Vector2 GetLooseCenterRelativeToGrid()
    {
        return m_LooseCenter.layout.center - m_InventoryGrid.worldBound.position;
    }

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

    // =============================================
    // ========= IGUIScreen Implementation =========
    // =============================================

    public bool isGUIActive()
    {
        return isActive;
    }

    public void activateGUI()
    {
        if (Instance != null)
        {
            m_Root.style.display = DisplayStyle.Flex;
            isActive = true;
        }
    }

    public void deactivateGUI()
    {
        if (Instance != null)
        {
            m_Root.style.display = DisplayStyle.None;
            isActive = false;
            ClearLooseItems();
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

    // Experimental

    public void AddUIDocumentToLoose(VisualElement root)
    {
        //Debug.Log("In AddUIDoc");
        //Debug.Log(root == null);
        root.AddToClassList("other-ui-doc");
        //Debug.Log(m_Loose == null);
        m_Loose.Add(root);
        root.visible = true;
    }

    public Vector2 GetLooseWorldBoundPosition()
    {
        return m_Loose.worldBound.position + m_Container.worldBound.position + m_WholeScreen.worldBound.position;
    }
}

