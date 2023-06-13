using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeArrow : MonoBehaviour
{
    // Amount to increment/decrement the current page index
    [SerializeField] private int pageIndexOffset = 1;

    private Button button;
    [SerializeField] private RecipeManager recipeManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);

        if (recipeManager == null)
        {
            Debug.LogError("Recipe Manager not found, make sure it is attached in the inspector!");
        }
    }

    private void OnButtonClick()
    {
        if (recipeManager != null)
        {
            recipeManager.UpdatePageIndex(pageIndexOffset);
        }
    }
}
