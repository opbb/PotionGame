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

    public readonly string dragButton = "Fire1";

    // A static variable allowing any script to access the inventory UI.
    [HideInInspector] public static PlayerInventoryView Instance;

    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;
    private VisualElement m_WholeScreen;
    private MouseTracker mouseTracker;
    //private static Label m_ItemDetailHeader;
    //private static Label m_ItemDetailBody;
    //private static Label m_ItemDetailPrice;
    private bool m_IsInventoryReady;
    public static Dimensions SlotDimension { get; private set; }

    private VisualElement m_Telegraph;

    private bool isDragging = false;
    private StoredItem draggedItem = null;

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

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        Configure();
        LoadInventory();
    }

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

    // Methods below copied from tutorial, I will finish them later
    // TODO: Implement inventory
    private async void Configure()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        m_WholeScreen = m_Root.Q<VisualElement>("WholeScreen");
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

    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    public async void LoadInventory()
    {
        await UniTask.WaitUntil(() => m_IsInventoryReady);
        List<StoredItem> invList = PlayerInventory.Instance.GetItemsInInventory();
        Debug.Log("Items in inventory:");
        foreach (StoredItem loadedItem in invList)
        {
            Debug.Log(loadedItem);
            Debug.Log(loadedItem.Details.CommonName);
            ItemVisual inventoryItemVisual = new ItemVisual(loadedItem);

            AddItemToInventoryGrid(inventoryItemVisual);
            SetItemPosition(inventoryItemVisual, new Vector2(SlotDimension.Width * loadedItem.position.x,
                    SlotDimension.Height * loadedItem.position.y));
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

    private void ConfigureMouseTracker()
    {
        mouseTracker = new MouseTracker();
        mouseTracker.AddToClassList("mouse-tracker");
        mouseTracker.BringToFront();
        mouseTracker.pickingMode = PickingMode.Ignore;
        m_WholeScreen.Add(mouseTracker);

        Debug.Log("Mouse Tracker: " + mouseTracker);
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

