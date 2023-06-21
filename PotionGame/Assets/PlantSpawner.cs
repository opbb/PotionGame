using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    public GameObject[] plants;

    // The distance away from the player at which this item will deactivate all its children.
    private static float loadDistance = 150f;
    private static Transform playerTransform;
    private static float loadedBehindCheckInterval = .05f;
    private static float loadedCloseCheckInterval = .1f;
    private static float loadedFarCheckInterval = .1f;
    private static float unloadAngle = 0f; // Given from 1f (0 degrees) to -1f (360 degrees)
    private bool isLoaded = true;

    public int maxPlantCount = 10;
    int activePlantCount = 0;

    public float spawnRadius = 10f;

    public float respawnTime = 5f;
    private float respawnTimer = 0f;
    public bool spawnAtSealevel;
    private float sealevel = -.25f;

    private string instanceID;

    private Terrain terrain;
    public Color gizmoColor;

    // Start is called before the first frame update
    void Start()
    {
        if (terrain == null)
        {
            terrain = GameObject.Find("MainTerrain").GetComponent<Terrain>();
        }

        if(playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // This will generate an id used for saving state that should be unique between instances but persistent between game session.
        // However it will break if a spawner is at the exact same position as another spawner, and spawns the exact same plants.
        // This shouldn't happen, but it could it someone intentionally copy + pasted a spawner then didn't edit it at all.
        instanceID = "spawner at (" + transform.position.x + "," + transform.position.y + "," + transform.position.z + ") with ";
        foreach(GameObject plant in plants)
        {
            instanceID += plant.name + ",";
        }

        InitializeState();

        LoadPlants();

        Invoke("SetLoadedBasedOnPlayerDistance", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (activePlantCount != gameObject.transform.childCount || activePlantCount < maxPlantCount)
        {
            RespawnPlant();
        }
    }

    // Save state when the scene is exited
    private void OnDestroy()
    {
        SaveState();

    }

    void LoadPlants()
    {
        while(gameObject.transform.childCount < maxPlantCount)
        {
            LoadPlant();
        }

        activePlantCount = gameObject.transform.childCount;
    }

    void LoadPlant() {
        float angleFromForward = Random.Range(0f, 2f * Mathf.PI);
        float distanceFromSpawner = Random.Range(0f, spawnRadius);

        Vector3 randomPosition = GetVectorFromAngleAndMagnitude(angleFromForward, distanceFromSpawner) + transform.position;

        if (spawnAtSealevel)
        {
            randomPosition.y = sealevel;
        } else
        {
            randomPosition.y = terrain.SampleHeight(randomPosition) + terrain.transform.position.y;
        }
        GameObject plant = Instantiate(plants[Random.Range(0, plants.Length)], randomPosition, Quaternion.identity);
        plant.transform.parent = transform;
    }

    void RespawnPlant()
    {
        respawnTimer += Time.deltaTime;
        if (respawnTimer >= respawnTime)
        {
            respawnTimer = 0f;
            LoadPlant();

            activePlantCount = gameObject.transform.childCount;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawIcon(transform.position, "mushroom_test_sprite", true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }


    // For saving info on quit, and loading it on enter
    private void InitializeState()
    {
        activePlantCount = PlayerPrefs.GetInt(instanceID + "activePlantCount", 0);
        respawnTimer = PlayerPrefs.GetFloat(instanceID + "respawnTimer", 0f);
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(instanceID + "activePlantCount", activePlantCount);
        PlayerPrefs.SetFloat(instanceID + "respawnTimer", respawnTimer);
    }

    // So that the spawn area is circular
    // Input angle in radians
    private Vector3 GetVectorFromAngleAndMagnitude(float angle, float magnitude)
    {
        float x = magnitude * Mathf.Cos(angle);
        float z = magnitude * Mathf.Sin(angle);
        return new Vector3(x, 0f, z);
    } 



    // Unloads spawned plants when the player is far, and loads them when the player is close. For optimization.
    // This method will invoke itself every call, so it will repeat forever, however the time between calls depends on how far away the player is.
    private void SetLoadedBasedOnPlayerDistance()
    {
        Vector3 playerToSpawner = transform.position - playerTransform.position;
        float distToPlayer = playerToSpawner.magnitude - spawnRadius;

        if (distToPlayer > loadDistance)
        {
            UnloadChildren();
            if(distToPlayer > loadDistance + 25f)
            {
                // If we wont have to load in soon, chill
                Invoke("SetLoadedBasedOnPlayerDistance", loadedFarCheckInterval);
            } else
            {
                // IF the player is close to loading us in, check frequently
                Invoke("SetLoadedBasedOnPlayerDistance", loadedCloseCheckInterval);
            }
        } else if (distToPlayer > 1f && Vector3.Dot(playerToSpawner, playerTransform.forward) < unloadAngle) // IF the player isnt within the spawn radius and we're behind them
        {
            UnloadChildren();
            Invoke("SetLoadedBasedOnPlayerDistance", loadedBehindCheckInterval);
        } else 
        {
            LoadChildren();
            Invoke("SetLoadedBasedOnPlayerDistance", loadedCloseCheckInterval);
        }
    }

    private void UnloadChildren()
    {
        if (isLoaded)
        {
            isLoaded = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    private void LoadChildren()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
