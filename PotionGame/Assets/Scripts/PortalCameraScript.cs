using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraScript : MonoBehaviour
{
    public Transform playerCamera;
    public Transform portal;
    public Vector3 offset = new Vector3(0, 36.18f, 0);

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float angularDifferenceBetweenPortalRotations = Quaternion.Angle(portal.rotation, transform.rotation);

        Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
        Vector3 newCameraDirection = (portalRotationalDifference * playerCamera.forward);
        Quaternion lookDirection = Quaternion.LookRotation(newCameraDirection, Vector3.up) * Quaternion.Euler(offset);
        Quaternion clampedLookDirection = Quaternion.Euler(lookDirection.eulerAngles.x, lookDirection.eulerAngles.y, 0);
        transform.rotation = clampedLookDirection;
    }


}
