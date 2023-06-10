using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbarium : MonoBehaviour, IGUIScreen
{

    bool isActive;

    [HideInInspector] public static Herbarium Instance;

    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one PlayerInventoryView
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    // IGUIScreen implementation
    public bool isGUIActive()
    {
        return isActive;
    }

    public void activateGUI()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void deactivateGUI()
    {
        isActive = false;
        gameObject.SetActive(false);
        //brewFailedText.gameObject.SetActive(false);
    }
}
