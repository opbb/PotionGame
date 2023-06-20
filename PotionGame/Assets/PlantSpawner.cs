using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    public GameObject[] plants;

    public int maxPlantCount = 10;
    int activePlantCount = 0;

    public float spawnRadius = 10f;

    public float respawnTime = 5f;
    float respawnTimer = 0f;

    public Terrain terrain;
    public Color gizmoColor;

    // Start is called before the first frame update
    void Start()
    {
        if (terrain == null)
        {
            terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        }

        LoadPlants();
    }

    // Update is called once per frame
    void Update()
    {
        if (activePlantCount != gameObject.transform.childCount || activePlantCount < maxPlantCount)
        {
            RespawnPlant();
        }
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
        Vector3 randomPosition = new Vector3(Random.Range(-spawnRadius + transform.position.x + terrain.transform.position.x, spawnRadius + transform.position.x + terrain.transform.position.x), 0, Random.Range(-spawnRadius + transform.position.z + terrain.transform.position.z, spawnRadius + transform.position.z + terrain.transform.position.z));
        randomPosition.y = terrain.SampleHeight(randomPosition) + terrain.transform.position.y;
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
}
