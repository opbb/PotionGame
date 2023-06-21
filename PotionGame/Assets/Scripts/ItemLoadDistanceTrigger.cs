using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadDistanceTrigger : MonoBehaviour
{
    [SerializeField] private float insideLoadDistance;
    [SerializeField] private float outsideLoadDistance;

    private Terrain mainTerrain;

    private void Start()
    {
        mainTerrain = GameObject.Find("MainTerrain").GetComponent<Terrain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlantSpawner.loadDistance = insideLoadDistance;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlantSpawner.loadDistance = outsideLoadDistance;
        }
    }
}
