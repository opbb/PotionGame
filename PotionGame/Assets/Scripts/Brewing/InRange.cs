using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Detects if the player is in range of this obeject
public class InRange : MonoBehaviour
{

    // Static vars so all the InRanges can coordinate

    public static IBrewingInteractable nearestBrewingInteractableOrNull; // Is either the nearest brewing interactable or null if there is none

    // returns whether _nearestBrewingInteractableOrNull_ is not null, which is how we know whether anything is in range or not
    public static bool isInRange { get => nearestBrewingInteractableOrNull != null; }
    private static float minRange;
    private static Transform playerTransform;

    // Vars individual to each instance
    private bool isThisInRange;

    //This is a workaround to Unity not letting you set the slot type to an interface.
    [Header("Only fill one of the slots below.")]
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private MortarAndPestleInteractable mortarAndPestle;

    private IBrewingInteractable thisBrewingInteractable;


    void Start()
    {
        isThisInRange = false;
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }


        // The code below takes the IBrewingInteractable which was given in the inspector as its base class and stores it.
        // This is a workaround to the inspector not letting you drag and drop objects into fields which are of an Interface type.
        int interactableCount = 0;
        if (recipeManager != null)
        {
            thisBrewingInteractable = (IBrewingInteractable)recipeManager;
            interactableCount++;
        }
        if (mortarAndPestle != null)
        {
            thisBrewingInteractable = (IBrewingInteractable)mortarAndPestle;
            interactableCount++;
        }
        if (interactableCount < 1)
        {
            throw new ArgumentNullException("You must provide an IBrewingInteractable through the inspector.");
        }
        if (interactableCount > 1)
        {
            thisBrewingInteractable = null;
            throw new InvalidOperationException("Only fill one of the IBrewingInteractable slots.");
        }
    }

    private void Update()
    {
        float distToPlayer = Vector3.Distance(playerTransform.position, transform.position);

        if (isThisInRange)
        {
            if (isThisCurrentBrewingInteractable())
            {
                minRange = distToPlayer;
            }
            else if (distToPlayer < minRange || nearestBrewingInteractableOrNull == null)
            {
                // If this is the closest or only BrewingInteractable in range
                setThisAsCurrentBrewingInteractable(distToPlayer);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("In on trigger enter");
        if (other.CompareTag("Player"))
        {
            isThisInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isThisInRange = false;
            if (isThisCurrentBrewingInteractable())
            {
                // If this was the nearestBrewingInteractable and the player leaves range, reset nearest brewing interactable. 
                nearestBrewingInteractableOrNull = null;
            }
        }
    }

    private bool isThisCurrentBrewingInteractable()
    {
        return ReferenceEquals(nearestBrewingInteractableOrNull, thisBrewingInteractable);
    }

    private void setThisAsCurrentBrewingInteractable(float distToPlayer)
    {
        nearestBrewingInteractableOrNull = thisBrewingInteractable;
        minRange = distToPlayer;
    }
}
