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
            controller.TeleportPlayer(portal.position + new Vector3(-2.0f, -0.5f, 0));

            // rotate player to face portal.forward
            var lookPos = portal.forward;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            player.transform.rotation = rotation;
        }
    }
}
