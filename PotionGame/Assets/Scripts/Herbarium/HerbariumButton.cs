using UnityEngine;
using UnityEngine.UI;

public class HerbariumButton : MonoBehaviour
{
    // Amount to increment/decrement the current page index
    [SerializeField] private int pageIndexOffset = 2; 

    private Button button;
    [SerializeField] private Herbarium herbarium;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);

        if (herbarium == null)
        {
            Debug.LogError("Herbarium instance not found. Make sure it is attached in the inspector");
        }
    }

    private void OnButtonClick()
    {
        if (herbarium != null)
        {
            herbarium.UpdatePageIndex(pageIndexOffset);
        }
    }
}