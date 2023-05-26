using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBehavior : MonoBehaviour
{

    [Header("Gamefeel Options")]
    [SerializeField] private float pickUpRange;
    [SerializeField] private float pickUpWidth;

    [Header("Performance Options")]
    [SerializeField] private float checkInterval;

    [Header("Unity Options")]
    [SerializeField] private string pickUpButton;
    [SerializeField] private LayerMask pickupLayers;

    private float lastCheckTime;
    private GameObject lastOutlined;


    // Start is called before the first frame update
    void Start()
    {
        lastCheckTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        /* 
         * Checks what's in front of the player if:
         *  - The player is trying to pick something up
         *  - There is currently something highlighted (so that we can instantly un-highlight once the item isn't in range)
         *  - It has been _checkInterval_ since our last check
         */
        
        if (Input.GetButton(pickUpButton) || lastOutlined != null || Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            RaycastHit hit;

            // Cast a sphere in front of the camera to check for anything in the pickup layer. We use a sphere instead of a ray to reduce the need for accuracy.
            if (Physics.SphereCast(transform.position, pickUpWidth, transform.forward, out hit, pickUpRange, pickupLayers, QueryTriggerInteraction.Collide))
            {
                // There is a pickup in front of us
                GameObject pickup = hit.collider.gameObject;

                if (Input.GetButtonDown(pickUpButton))
                {
                    PickUp(pickup);
                }
                else
                {
                    HighlightPickup(pickup);
                }
            } else
            {
                // There is no pickup in front of us
                NullSafeClearOldOutline();
            }
        }
    }

    private void PickUp(GameObject pickup)
    {
        PlayerInventory.Instance.StoreInInv(pickup.name);
        PlayerInventory.Instance.ShowInv();

        Destroy(pickup); // Destroy the pickup
    }

    // Highlights the given pickup and unhighlights the old one.
    private void HighlightPickup(GameObject pickup)
    {
        // Only execute code if there is a new object to highlight
        if (!ReferenceEquals(pickup, lastOutlined))
        {
            NullSafeClearOldOutline();

            Outline pickupOutline;
            if (pickup.TryGetComponent<Outline>(out pickupOutline))
            {
                // If the new object has an outline, enable it and store
                pickupOutline.enabled = true;
                lastOutlined = pickup;
            } else
            {
                // If the new object doesn't have an outline, report it and clear outline
                Debug.Log("The pickup that is being hovered over should have an Outline component.\nCheck to make sure that the children of the PickupParent arent set to the Pickup layer.");
                lastOutlined = null;
            }
        }
    }

    private void NullSafeClearOldOutline()
    {
        // Disable old outline
        if (lastOutlined != null)
        {
            lastOutlined.GetComponent<Outline>().enabled = false;
            lastOutlined = null;
        }
    }
}
