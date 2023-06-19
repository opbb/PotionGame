using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadUnderwaterCheck : MonoBehaviour
{
    [Tooltip("Unfortunately due to time contraints we were not able to make this value slowly rise for added realism.")]
    [SerializeField] private float sealevel;
    private static HeadUnderwaterCheck Instance;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public static bool isHeadUnderwater()
    {
        return Instance.transform.position.y < Instance.sealevel;
    }

}
