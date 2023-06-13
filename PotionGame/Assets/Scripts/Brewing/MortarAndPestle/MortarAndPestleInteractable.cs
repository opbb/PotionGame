using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarAndPestleInteractable : MonoBehaviour, IBrewingInteractable
{
    public void OpenBrewingGUI()
    {
        UIController.Instance.ActivateMortarAndPestle();
    }

    public void PrintTestMessage()
    {
        Debug.Log("Mortar and Pestle interactable " + name + " has been pinged.");
    }
}
