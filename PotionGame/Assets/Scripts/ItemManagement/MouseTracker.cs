using UnityEngine;
using UnityEngine.UIElements;

// A visual element meant to encompass the whole screen so that we always know where the mouse is in the UI coordinate system
// I could not find a better way to get the mouse's position in UI coordinates for a UI Document. If a translation between
// screen space and UI space exists, I couldn't find it.
public class MouseTracker : VisualElement
{
    public Vector2 mousePosition {get; set; }
    public MouseTracker()
    {
        //this.style.width = parent.style.width;
        //this.style.height = parent.style.height;
        //this.style.position = parent.style.position;
        RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
        Debug.Log("MouseTracker Created");
    }

    ~MouseTracker()
    {
        UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
    }

    private void OnMouseMoveEvent(MouseMoveEvent mouseEvent)
    {
        mousePosition = mouseEvent.mousePosition;
    }
}
