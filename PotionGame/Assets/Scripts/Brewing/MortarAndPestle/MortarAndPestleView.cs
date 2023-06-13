using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class MortarAndPestleView : MonoBehaviour, IGUIInventorySubscreen
{

    private VisualElement root;
    private VisualElement pestlePosition;
    private VisualElement pestleVisual;
    private VisualElement mortarBackground;
    private VisualElement workSpace;
    private VisualElement wholeScreen;

    [Header("Pestle Bounding Box Settings")]
    [SerializeField] private float leftBoundSlope;
    // Bounds in percentage
    [SerializeField] private float topBound;
    [SerializeField] private float bottomBound;
    [SerializeField] private float bottomLeftX;


    private MouseTracker mouseTracker;
    private bool isHoldingPestle;

    //Circle counting vars
    private int clockwiseCounter = 0;
    private int counterClockwiseCounter = 0;
    private MortarQuadrant previousQuadrant = MortarQuadrant.TopLeft;
    private int circlesRemaining = -1;

    // The pivot values are hardcoded because they're set in the inspector and can't be accessed at runtime for some ducking reason (which took hours of testing to figure out, truck me)
    [Header("Pestle Pivot Values")]
    [Tooltip("These should match those in the UI Builder. Yes this is a bad way of doing this but it's a workaround.")]
    [SerializeField] private float pivotXPercentage = .5f;
    [Tooltip("These should match those in the UI Builder. Yes this is a bad way of doing this but it's a workaround.")]
    [SerializeField] private float pivotYPercentage = .08f;

    [Header("RotationSettings")]
    [SerializeField] private float rotationCenterYOffsetPercent = 3f;

    [Header("Pestle Quadrant Center Settings")]
    [SerializeField] private float quadrantCenterYOffsetPercent = -.45f;

    [Header("Input/Output Options")]
    [Tooltip("What will come out if you mash something that doesn't mash into anything. (Should probably be Dubious Mash)")]
    [SerializeField] private ItemDefinition defaultOutput;

    private ItemDefinition currentItem;
    private Action<ItemDefinition> returnAction;

    // Bounds information
    float bottomLeftXPixels;
    float bottomRightXPixels;
    float topBoundPixels;
    float bottomBoundPixels;


    // Start is called before the first frame update
    void Start()
    {
        root = GetComponentInChildren<UIDocument>().rootVisualElement;
        pestlePosition = root.Q<VisualElement>("Pestle");
        pestleVisual = root.Q<VisualElement>("PestleImage");
        PestleMouseCapture mouseCapture = new PestleMouseCapture(this);
        pestleVisual.Add(mouseCapture);
        mortarBackground = root.Q<VisualElement>("MortarBackground");
        workSpace = root.Q<VisualElement>("WorkZone");
        wholeScreen = root.Q<VisualElement>("WholeScreen");
        isHoldingPestle = false;

        //PlayerInventoryView.Instance.AddScreenToLoose(root);

        UpdateBounds();
        ConfigureMouseTracker();
        deactivateGUI();
    }

    // Update is called once per frame
    void Update()
    {

        if (isHoldingPestle)
        {
            if (Input.GetButton("Fire1"))
            {
                // The pestle is still being held
                UpdateBounds();
                MovePestleToMouse();
                RotatePestleToBottom();
                if (UpdateCirclesAndCheckForCompleted())
                {
                    ReturnMashFromMortar();
                }
            }
            else
            {
                mouseTracker.pickingMode = PickingMode.Ignore;
                isHoldingPestle = false;
            }

        }
    }

    public void OnClick()
    {
        if (!isHoldingPestle)
        {
            mouseTracker.pickingMode = PickingMode.Position;
            isHoldingPestle = true;
            mouseTracker.mousePosition = GetPestlePivotInMouseSpace();
        }
    }

    private void MovePestleToMouse()
    {
        Vector2 mortarSpaceMousePosition = MouseSpaceToMortarSpace(mouseTracker.mousePosition);
        MovePestleToPos(mortarSpaceMousePosition);
    }

    private Vector2 ClampPestlePositionToBounds(Vector2 unclampedPos)
    {
        float clampedX;
        float clampedY;

        // Clamp Y
        // Note that a lower Y means it is higher up on the screen, as counting starts from the top left corner.
        if (unclampedPos.y < topBoundPixels)
        {
            clampedY = topBoundPixels;
        }
        else if (unclampedPos.y > bottomBoundPixels)
        {
            clampedY = bottomBoundPixels;
        }
        else
        {
            clampedY = unclampedPos.y;
        }

        // Clamp X based on Y.
        // This is just using point slope form to define a sloped line for the X bound.
        // Uses the bottom left point of the bounding box for x_1 and y_1.
        float leftXLimit = ((clampedY - bottomBoundPixels) / leftBoundSlope) + bottomLeftXPixels;
        float rightXLimit = ((clampedY - bottomBoundPixels) / -leftBoundSlope) + bottomRightXPixels;

        if (unclampedPos.x < leftXLimit)
        {
            clampedX = leftXLimit;
        }
        else if (unclampedPos.x > rightXLimit)
        {
            clampedX = rightXLimit;
        }
        else
        {
            clampedX = unclampedPos.x;
        }

        return new Vector2(clampedX, clampedY);
    }

    private void UpdateBounds()
    {
        Vector2 pivotLocationInPestle = GetPivotLocationInPestle();

        float mortarBackgroundWidth = mortarBackground.worldBound.width;
        bottomLeftXPixels = mortarBackground.worldBound.width * bottomLeftX;
        bottomRightXPixels = mortarBackgroundWidth - mortarBackgroundWidth * bottomLeftX;
        topBoundPixels = mortarBackground.worldBound.height * topBound;
        bottomBoundPixels = mortarBackground.worldBound.height * bottomBound;
    }

    private void RotatePestleToBottom()
    {
        Quaternion rotationToCenter = Quaternion.Euler(0f, 0f, GetPestleAngleFromCenter());

        pestleVisual.transform.rotation = rotationToCenter;
    }

    private Vector2 MouseSpaceToMortarSpace(Vector2 mousePosition)
    {
        // Makes mouse position relative to the origin of mortarBackground
        return mousePosition - GetMortarSpaceOffset();
    }

    private Vector2 GetPestlePivotInMouseSpace()
    {
        return GetMortarSpaceOffset() + pestlePosition.worldBound.position + GetPivotLocationInPestle();
    }

    private void MovePestleToPos(Vector2 targetPos)
    {
        Vector2 clampedPos = ClampPestlePositionToBounds(targetPos);
        clampedPos -= GetPivotLocationInPestle();
        pestlePosition.style.left = clampedPos.x;
        pestlePosition.style.top = clampedPos.y;
    }

    private void ConfigureMouseTracker()
    {
        mouseTracker = new MouseTracker();
        mouseTracker.BringToFront();
        wholeScreen.Add(mouseTracker);
    }

    private float GetPestleAngleFromCenter()
    {
        float centerX = (bottomLeftXPixels + bottomRightXPixels) / 2f;
        float centerY = bottomBoundPixels + mortarBackground.worldBound.height * rotationCenterYOffsetPercent;

        Vector2 centerPos = new Vector2(centerX, centerY);
        Vector2 pestlePos = pestlePosition.worldBound.position;

        Vector2 pestleToCenter = centerPos - pestlePos;

        return -Vector2.SignedAngle(pestleToCenter, Vector2.up);
    }

    private Vector2 GetPivotLocationInPestle()
    {
        return new Vector2(pestlePosition.worldBound.width * pivotXPercentage, pestlePosition.worldBound.height * pivotYPercentage);
    }


    // Adds an item to the mortar if it is empty, returning false and not adding it if it is full
    public bool MoveItemFromInventoryToSubscreen(ItemDefinition item, Action<ItemDefinition> returnAction)
    {
        if (currentItem == null)
        {
            currentItem = item;
            this.returnAction = returnAction;
            circlesRemaining = 5; // Default value for testing
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns the mash of the current item using the action provided when the current item was added.
    private void ReturnMashFromMortar()
    {
        if (currentItem != null)
        {
            if (currentItem.afterMortarAndPestle != null)
            {
                returnAction.Invoke(currentItem.afterMortarAndPestle);
            }
            else
            {
                returnAction.Invoke(defaultOutput);
            }
            returnAction = null;
            currentItem = null;
            circlesRemaining = -1;
        }
        else
        {
            throw new InvalidOperationException("Mortar and Pestle is trying to return an item when it is empty.");
        }
    }

    private MortarQuadrant GetCurrentQuadrant()
    {
        bool isLeft = GetPestleAngleFromCenter() <= 0f;
        bool isTop = pestlePosition.worldBound.y <= (bottomBoundPixels - mortarBackground.worldBound.height * quadrantCenterYOffsetPercent);


        if (isLeft)
        {
            if (isTop)
            {
                return MortarQuadrant.TopLeft;
            }
            else
            {
                return MortarQuadrant.BottomLeft;
            }
        }
        else
        {
            if (isTop)
            {
                return MortarQuadrant.TopRight;
            }
            else
            {
                return MortarQuadrant.BottomRight;
            }
        }
    }

    private bool UpdateCirclesAndCheckForCompleted()
    {
        MortarQuadrant currentQuadrant = GetCurrentQuadrant();


        if (currentQuadrant == previousQuadrant)
        {
            // If nothing's changed, return.
            return false;
        }
        else if (((int)currentQuadrant - 1) == (int)previousQuadrant || (currentQuadrant == MortarQuadrant.TopLeft && previousQuadrant == MortarQuadrant.BottomLeft)) // The second condition here is because modulo doesn't work on negative numbers
        {
            // If we advanced clockwise, increment the clockwise counter and reset the counterclockwise counter.
            clockwiseCounter++;
            counterClockwiseCounter = 0;
        }
        else if (((int)currentQuadrant + 1) % 4 == (int)previousQuadrant)
        {
            // If we advanced counterclockwise, increment the counterclockwise counter and reset the clockwise counter.
            counterClockwiseCounter++;
            clockwiseCounter = 0;
        }
        else
        {
            // If we somehow moved diagonally, reset both counters
            clockwiseCounter = 0;
            counterClockwiseCounter = 0;
        }

        previousQuadrant = currentQuadrant;

        if (clockwiseCounter >= 4 || counterClockwiseCounter >= 4)
        {
            // If we've made a full rotation, increment the circle counter and reset the other counters.
            circlesRemaining--;
            clockwiseCounter = 0;
            counterClockwiseCounter = 0;

            return circlesRemaining == 0;
        }
        else
        {
            return false;
        }
    }

    public void NestInsideInventoryLoose()
    {
        PlayerInventoryView.Instance.AddUIDocumentToLoose(root);
        deactivateGUI();
    }

    // Gets the position of the mortar relative to the top left of the MortarAndPestleUIDoc
    private Vector2 GetMortarSpaceOffset()
    {
        return mortarBackground.worldBound.position;
    }

    // IGUIScreen implementation
    public bool isGUIActive()
    {
        return root.visible;
    }

    public void activateGUI()
    {
        root.visible = true;
    }

    public void deactivateGUI()
    {
        root.visible = false;
    }

    private enum MortarQuadrant
    {
        TopLeft = 0,
        TopRight = 1,
        BottomRight = 2,
        BottomLeft = 3
    }
}
