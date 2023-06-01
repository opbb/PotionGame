using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public GameObject player;
    public Transform portal;

    PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        controller = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 portalToPlayer = player.transform.position - portal.position;
            float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

            float rotationDiff = -Quaternion.Angle(transform.rotation, portal.rotation);
            rotationDiff += 180;
            player.transform.Rotate(Vector3.up, rotationDiff);

            controller.TeleportPlayer(portal.position + new Vector3(-2.0f, -0.5f, 0));
        }
    }
}
