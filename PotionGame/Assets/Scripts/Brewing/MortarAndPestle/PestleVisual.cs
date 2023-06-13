using UnityEngine;
using UnityEngine.UIElements;
using System;
public class PestleVisual : VisualElement
{
    public PestleVisual()
    {
        style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
        style.width = new StyleLength(new Length(100f, LengthUnit.Percent));

        RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
    }

    ~PestleVisual()
    {
        UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
    }

    private void OnMouseDownEvent(MouseDownEvent mouseEvent)
    {
        parent.transform.position = mouseEvent.localMousePosition;
    }

}
