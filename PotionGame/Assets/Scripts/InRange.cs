using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Detects if the player is in range of this obeject
public class InRange : MonoBehaviour
{
    public static bool isInRange;

    void Start()
    {
        isInRange = false;
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }
}
