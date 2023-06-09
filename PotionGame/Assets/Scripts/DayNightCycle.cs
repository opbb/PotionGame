using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DayNightCycle : MonoBehaviour
{
    public Light lightSource;
    public float speed = 0.1f;
    public float time;

    public float dayIntensity = 1;
    public float nightIntensity = 0;
    public float intensity;

    public Color dayColor;
    public Color nightColor;
    public Color lightColor;

    private void Update()
    {
        Vector3 lightAngle = transform.localEulerAngles;

        time += speed * Time.deltaTime;
        lightSource.transform.rotation = Quaternion.Euler(time, 0, 0);

        intensity = lightSource.intensity;

        if (lightAngle.x >= 0 && lightAngle.x < 90)
        {
            intensity = Mathf.Lerp(nightIntensity, dayIntensity, 100 * Time.deltaTime);
        }
        else if (lightAngle.x >= 90 && lightAngle.x < 120)
        {
            intensity = dayIntensity;
        }
        else if (lightAngle.x >= 120 && lightAngle.x < 180)
        {
            intensity = Mathf.Lerp(dayIntensity, nightIntensity, 100 * Time.deltaTime);
        }
        else if (lightAngle.x >= 180 && lightAngle.x < 0)
        {
            intensity = nightIntensity;
        }

        lightColor = lightSource.color;

        if (lightAngle.x >= 0 && lightAngle.x < 90)
        {
            lightColor = Color.Lerp(nightColor, dayColor, speed * Time.deltaTime);
        }
        else if (lightAngle.x >= 90 && lightAngle.x < 120)
        {
            lightColor = dayColor;
        }
        else if (lightAngle.x >= 120 && lightAngle.x < 180)
        {
            lightColor = Color.Lerp(dayColor, nightColor, speed * Time.deltaTime);
        }
        else if (lightAngle.x >= 180 && lightAngle.x < 0)
        {
            lightColor = nightColor;
        }
    }
}
