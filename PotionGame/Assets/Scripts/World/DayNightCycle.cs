using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DayNightCycle : MonoBehaviour
{
    public float speed = 0.1f;
    public float time = 0;
    public int day = 1;
    public float angleOffset = 0;

    public List<Material> skyboxes;
    public float skyboxLerpTime = 2f;
    Material prev, curr;
    bool isLerping = false;
    float lerpTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        prev = skyboxes.Find(x => x.name == $"Night{(day-1 > 0 ? day-1 : 4)}");
        curr = skyboxes.Find(x => x.name == $"Morning{day}");

        RenderSettings.skybox = curr;
    }

    public Light lightSource;
    public float dayIntensity = 1;
    public float nightIntensity = 0;
    public float intensity;

    public Color dayColor;
    public Color nightColor;
    public Color lightColor;

    private void Update()
    {
        time += speed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(time + angleOffset, 0, 0);

        if (time >= 360)
        {
            time = 0;
            day++;
            if (day > 4)
            {
                day = 1;
            }
        }

        LoadSkybox();
    }

    void LoadSkybox() {
        // morning, day, sunset, night skyboxes for 4 days
        // Morning{day}, Day{day}, Sunset{day}, Night{day}
        // lord forgive me for this code

        if (!isLerping) {
            if ((time >= 0 && time < 90) && prev.name != $"Night{(day-1 > 0 ? day-1 : 4)}") {
                prev = skyboxes.Find(x => x.name == $"Night{(day-1 > 0 ? day-1 : 4)}");
                curr = skyboxes.Find(x => x.name == $"Morning{day}");
                isLerping = true;
            } 
            else if (time >= 90 && time < 180 && prev.name != $"Morning{day}") {
                prev = skyboxes.Find(x => x.name == $"Morning{day}");
                curr = skyboxes.Find(x => x.name == $"Day{day}");
                isLerping = true;
            } 
            else if (time >= 180 && time < 270 && prev.name != $"Day{day}") {
                prev = skyboxes.Find(x => x.name == $"Day{day}");
                curr = skyboxes.Find(x => x.name == $"Sunset{day}");
                isLerping = true;
            } 
            else if (time >= 270 && time < 360 && prev.name != $"Sunset{day}") {
                prev = skyboxes.Find(x => x.name == $"Sunset{day}");
                curr = skyboxes.Find(x => x.name == $"Night{day}");
                isLerping = true;
            }
        }


        if (isLerping) {
            float inc = Time.deltaTime * skyboxLerpTime;
            lerpTime += inc;
            RenderSettings.skybox.Lerp(prev, curr, inc);
            if (lerpTime >= 4f) {
                RenderSettings.skybox = curr;
                isLerping = false;
                lerpTime = 0;
            }
        }

        DynamicGI.UpdateEnvironment();
    }

    // private void Update()
    // {
    //     Vector3 lightAngle = transform.localEulerAngles;

    //     time += speed * Time.deltaTime;
    //     lightSource.transform.rotation = Quaternion.Euler(time, 0, 0);

    //     intensity = lightSource.intensity;

    //     if (lightAngle.x >= 0 && lightAngle.x < 90)
    //     {
    //         intensity = Mathf.Lerp(nightIntensity, dayIntensity, 100 * Time.deltaTime);
    //     }
    //     else if (lightAngle.x >= 90 && lightAngle.x < 120)
    //     {
    //         intensity = dayIntensity;
    //     }
    //     else if (lightAngle.x >= 120 && lightAngle.x < 180)
    //     {
    //         intensity = Mathf.Lerp(dayIntensity, nightIntensity, 100 * Time.deltaTime);
    //     }
    //     else if (lightAngle.x >= 180 && lightAngle.x < 0)
    //     {
    //         intensity = nightIntensity;
    //     }

    //     lightColor = lightSource.color;

    //     if (lightAngle.x >= 0 && lightAngle.x < 90)
    //     {
    //         lightColor = Color.Lerp(nightColor, dayColor, speed * Time.deltaTime);
    //     }
    //     else if (lightAngle.x >= 90 && lightAngle.x < 120)
    //     {
    //         lightColor = dayColor;
    //     }
    //     else if (lightAngle.x >= 120 && lightAngle.x < 180)
    //     {
    //         lightColor = Color.Lerp(dayColor, nightColor, speed * Time.deltaTime);
    //     }
    //     else if (lightAngle.x >= 180 && lightAngle.x < 0)
    //     {
    //         lightColor = nightColor;
    //     }
    // }
}
