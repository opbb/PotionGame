using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGUIScreen
{
    // Returns true if this GUI is currently active
    public bool isGUIActive();

    // Activates this GUI. Should be safe to call while the GUI is already active.
    public void activateGUI();

    // Deactivates this GUI. Should be safe to call while the GUI is already deactivated.
    public void deactivateGUI();
}
