using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class Herbarium : MonoBehaviour, IGUIScreen
{

    bool isActive;

    [HideInInspector] public static Herbarium Instance;
    [SerializeField] private List<ItemDefinition> itemDefinitions;

    [SerializeField] private Text rightPageName;
    [SerializeField] private Text rightPageDescription;
    [SerializeField] private Image rightPageImage;
    [SerializeField] private Text leftPageName;
    [SerializeField] private Text leftPageDescription;
    [SerializeField] private Image leftPageImage;

    [HideInInspector] public static int currentPageIndex = 0;

    // This awake method enforces the singleton design pattern.
    // i.e. there can only ever be one Herbarium
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

        if (rightPageImage == null || rightPageDescription == null || rightPageName == null 
            || leftPageDescription == null || leftPageImage == null || leftPageName == null)
        {
            throw new InvalidOperationException("The Herbarium cant find all of the book elements, " +
              "they are probably not attached to the same GameObject, which they should be.");
        }

        if (itemDefinitions.Count == 0)
        {
            throw new InvalidOperationException("The Herbarium has no data to display. The Scriptable objects " +
                "are probably not attached in the inspector");
        }
    }

    void Update()
    {

    }

    // IGUIScreen implementation
    public bool isGUIActive()
    {
        return isActive;
    }

    public void activateGUI()
    {
        if (Instance != null)
        {
            isActive = true;
            gameObject.SetActive(true);

            setBothPages();
        }
    }

    public void deactivateGUI()
    {
        if (Instance != null)
        {
            isActive = false;
            gameObject.SetActive(false);
        }
           
    }

    private void setBothPages()
    {
        setLeftPage();
        setRightPage(); 
    }

    private void setRightPage()
    {
        if (currentPageIndex + 1 >= itemDefinitions.Count)
        {
            rightPageName.text = "";
            rightPageDescription.text = "";
            rightPageImage.sprite = null;
            rightPageImage.enabled = false;
        } else
        {
            rightPageName.text = itemDefinitions[currentPageIndex + 1].CommonName;
            rightPageDescription.text = itemDefinitions[currentPageIndex + 1].Description;
            rightPageImage.sprite = itemDefinitions[currentPageIndex + 1].Icon;
            rightPageImage.enabled = true;
        }
    }

    private void setLeftPage()
    {
        if (currentPageIndex >= itemDefinitions.Count)
        {
            leftPageName.text = "";
            leftPageDescription.text = "";
            leftPageImage.sprite = null;
            leftPageImage.enabled = false;
        }
        {
            leftPageName.text = itemDefinitions[currentPageIndex].CommonName;
            leftPageDescription.text = itemDefinitions[currentPageIndex].Description;
            leftPageImage.sprite = itemDefinitions[currentPageIndex].Icon;
        }
       
    }

    public void UpdatePageIndex(int amount)
    {
        // Increase page counter, make sure we have more pages to right
        if ((currentPageIndex < itemDefinitions.Count - 2) && (amount > 0))
        {
            currentPageIndex += amount;
            setBothPages();
        }
        else if ((currentPageIndex >= 2) && (amount < 0))
        {
            currentPageIndex += amount;
            setBothPages();
        }
    }
}
