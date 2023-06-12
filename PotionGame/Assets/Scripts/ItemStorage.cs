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

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        ToggleUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) < interactRange)
        {
            Interact();
        }
        else
        {
            ToggleUI(false);
        }
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactRange))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                ToggleUI(true);
                quantityText.text = amount.ToString();
                itemImage.sprite = item.Icon;

                MouseInput();
            } 
            else
            {
                ToggleUI(false);
            }
        } 
        else
        {
            ToggleUI(false);
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
