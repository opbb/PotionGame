using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DayNightCycle : MonoBehaviour
{
    public GameObject lightSource;
    public float timeSpeed = 0.1f;
    public float time;

    private void Update()
    {
        time += timeSpeed * Time.deltaTime;
        lightSource.transform.rotation = Quaternion.Euler(time, 0, 0);
    }
}
