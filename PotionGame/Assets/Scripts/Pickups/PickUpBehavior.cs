using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBehavior : MonoBehaviour
{
    [Header("Gamefeel Options")]
    [Tooltip("The maximum distance (in meters) from which items can be picked up.")]
    [SerializeField] private float pickUpRange;
    [Tooltip("How close to the center of the screen pickups need to be to be highlighted.\nRecommended: .25ish")]
    [SerializeField] private float pickUpWidth;

    [Header("Performance Options")]
    [Tooltip("The number of seconds between checks for items.\nRecommended: .1ish")]
    [SerializeField] private float checkInterval;

    [Header("Unity Options")]
    [Tooltip("The name of the button which is used to pick things up.\nRecommended: Fire1")]
    [SerializeField] private string pickUpButton;
    [Tooltip("The Layers which are able to be picked up. Set this to Pickup")]
    [SerializeField] private LayerMask pickupLayers;

    private float lastCheckTime;
    private Outline currentOutline;


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

        if (Input.GetButton(pickUpButton) || currentOutline != null || Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            RaycastHit hit;

            // Offset the origin back by the sphere radius so that you can pick up things directly in front of your face
            // (colliders that start within the sphere are not registered.)
            Vector3 castOrigin = transform.position + (transform.forward * pickUpWidth * -1f);

            // Cast a sphere in front of the camera to check for anything in the pickup layer. We use a sphere instead of a ray to reduce the need for accuracy.
            if (Physics.SphereCast(castOrigin, pickUpWidth, transform.forward, out hit, pickUpRange, pickupLayers, QueryTriggerInteraction.Collide))
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
            }
            else
            {
                // There is no pickup in front of us
                ClearCurrentOutline();
            }
        }
    }

    private void PickUp(GameObject pickup)
    {
        if (pickup.GetComponent<PickupableBehavior>().IsIndestructible())
        {
            pickup.GetComponent<PickupableBehavior>().OpenInventoryWith();
        }
        else
        {
            Destroy(pickup); // Destroy the pickup
        }
       
    }

    // Highlights the given pickup and unhighlights the old one.
    private void HighlightPickup(GameObject pickup)
    {
        // Only execute code if there is a new object to highlight
        if (currentOutline == null || !ReferenceEquals(pickup, currentOutline.gameObject))
        {
            ClearCurrentOutline();

            Outline pickupOutline;
            if (pickup.TryGetComponent<Outline>(out pickupOutline))
            {
                // If the new object has an outline, enable and store it
                pickupOutline.enabled = true;
                currentOutline = pickupOutline;
            }
            else
            {
                // If the new object doesn't have an outline, give it one
                currentOutline = pickup.AddComponent<Outline>();
            }
        }
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
