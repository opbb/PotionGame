using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemStorage : MonoBehaviour
{
    public ItemDefinition item;
    public int amount = 0;
    public float interactRange = 2f;

    public Text quantityText;
    public Image itemImage;

    Transform playerTransform;
    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (quantityText == null || itemImage == null)
        {
            GameObject ui = GameObject.Find("UI").transform.Find("HutStorageUI").gameObject;
            if (ui != null)
            {
                quantityText = ui.transform.Find("QuantityText").GetComponent<Text>();
                itemImage = ui.transform.Find("SpriteImage").GetComponent<Image>();
            }
        }

        ToggleUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.GetChild(0).transform.position, playerTransform.position) < interactRange)
        {
            Interact();
        }
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactRange))
        {
            if (hit.transform == transform || hit.transform.parent == transform)
            {
                ToggleUI(true);
                quantityText.text = amount.ToString();
                itemImage.sprite = item.Icon;

                isActive = true;

                MouseInput();
            } 
            else if (isActive)
            {
                ToggleUI(false);
                isActive = false;
            }
        } 
        else if (isActive)
        {
            ToggleUI(false);
            isActive = false;
        }
    }

    void MouseInput()
    {
        // left click to take out
        if (Input.GetMouseButtonDown(0) && amount > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                List<ItemDefinition> items = new List<ItemDefinition>();
                for (int i = 0; i < amount; i++)
                {
                    items.Add(item);
                }
                PlayerInventory.Instance.OpenInventoryWithItems(items);
                amount = 0;
            }
            else {
                PlayerInventory.Instance.OpenInventoryWithItem(item);
                amount--;
            }
        }
        // right click to put in
        else if (Input.GetMouseButtonDown(1))
        {
            bool success = PlayerInventory.Instance.TryTakeOutItem(item);
            if (success)
            {
                amount++;
            }
        }
    }

    void ToggleUI(bool toggle)
    {
        quantityText.transform.parent.gameObject.SetActive(toggle);
    }
}
