using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MortarAndPestleTest : MonoBehaviour
{

    VisualElement root;
    VisualElement pestle;
    VisualElement mortarBackground;
    VisualElement workSpace;
    VisualElement wholeScreen;

    [SerializeField] private float leftBoundSlope;
    // Bounds in percentage
    [SerializeField] private float topBound;
    [SerializeField] private float bottomBound;
    [SerializeField] private float bottomLeftX;


    private MouseTracker mouseTracker;
    private bool isHoldingPestle;

    // The pivot values are hardcoded because they're set in the inspector and can't be accessed at runtime for some ducking reason (which took hours of testing to figure out, truck me)
    [SerializeField] private float pivotXPercentage = .5f;
    [SerializeField] private float pivotYPercentage = .08f;

    // Bounds information
    float bottomLeftXPixels;
    float bottomRightXPixels;
    float topBoundPixels;
    float bottomBoundPixels;


    // Start is called before the first frame update
    void Start()
    {
        root = GetComponentInChildren<UIDocument>().rootVisualElement;
        pestle = root.Q<VisualElement>("Pestle");
        PestleMouseCapture mouseCapture = new PestleMouseCapture(this);
        pestle.Add(mouseCapture);
        mortarBackground = root.Q<VisualElement>("MortarBackground");
        workSpace = root.Q<VisualElement>("WorkZone");
        wholeScreen = root.Q<VisualElement>("WholeScreen");
        isHoldingPestle = false;
        UpdateBounds();
        ConfigureMouseTracker();
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
            } else
            {
                mouseTracker.pickingMode = PickingMode.Ignore;
                isHoldingPestle = false;
            }
            
        }
    }

    public void OnClick()
    {
        if (!isHoldingPestle) {
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
        Vector2 pivotLocationInPestle = GetPivotLocationInPestle();
        float topBoundPixelsAdj = topBoundPixels - pivotLocationInPestle.y;
        float bottomBoundPixelsAdj = bottomBoundPixels - pivotLocationInPestle.y;
        float bottomLeftXPixelsAdj = bottomLeftXPixels - pivotLocationInPestle.x;
        float bottomRightXPixelsAdj = bottomRightXPixels - pivotLocationInPestle.x;
        float clampedX;
        float clampedY;

        //Debug.Log("Pos is (" + unclampedPos.x + "," + unclampedPos.y + ")");

        // Clamp Y
        // Note that a lower Y means it is higher up on the screen, as counting starts from the top left corner.
        if(unclampedPos.y < topBoundPixelsAdj)
        {
            clampedY = topBoundPixelsAdj;
        } else if (unclampedPos.y > bottomBoundPixelsAdj)
        {
            clampedY = bottomBoundPixelsAdj;
        } else
        {
            clampedY = unclampedPos.y;
        }

        // Clamp X based on Y.
        // This is just using point slope form to define a sloped line for the X bound.
        // Uses the bottom left point of the bounding box for x_1 and y_1.
        float leftXLimit = ((clampedY - bottomBoundPixelsAdj) / leftBoundSlope) + bottomLeftXPixelsAdj;
        float rightXLimit = ((clampedY - bottomBoundPixelsAdj) / -leftBoundSlope) + bottomRightXPixelsAdj;

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

        return new Vector2(clampedX,clampedY);
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
        Debug.Log(pestle.transform.rotation.eulerAngles);
        float centerX = (bottomLeftXPixels + bottomRightXPixels) / 2f;
        float centerY = bottomBoundPixels;

        Vector2 centerPos = new Vector2(centerX, centerY);
        Vector2 pestlePos = pestle.worldBound.position;

        Vector2 pestleToCenter = pestlePos - centerPos;

        Debug.Log("Angle: " + Vector2.SignedAngle(pestleToCenter, Vector2.up));
        
        Quaternion rotationToCenter = Quaternion.Euler(0f,0f,Vector2.SignedAngle(pestleToCenter, Vector2.up));

        pestle.transform.rotation = rotationToCenter;
    }

    private Vector2 MouseSpaceToMortarSpace(Vector2 mousePosition)
    {
        // Makes mouse position relative to the origin of mortarBackground
        return mousePosition - workSpace.worldBound.position - mortarBackground.worldBound.position;
    }

    private Vector2 GetPestlePivotInMouseSpace()
    {
        Vector2 pivotLocationInPestle = GetPivotLocationInPestle();
        return workSpace.worldBound.position + mortarBackground.worldBound.position + pestle.worldBound.position + pivotLocationInPestle;
    }

    private void MovePestleToPos(Vector2 targetPos)
    {
        Vector2 clampedPos = ClampPestlePositionToBounds(targetPos);
        pestle.style.left = clampedPos.x;
        pestle.style.top = clampedPos.y;
    }

    private void ConfigureMouseTracker()
    {
        mouseTracker = new MouseTracker();
        mouseTracker.BringToFront();
        wholeScreen.Add(mouseTracker);
    }

    private Vector2 GetPivotLocationInPestle()
    {
        return new Vector2(pestle.worldBound.width * pivotXPercentage, pestle.worldBound.height * pivotYPercentage);
    }
}
