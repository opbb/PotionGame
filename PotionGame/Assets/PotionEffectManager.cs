using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionEffectManager : MonoBehaviour
{
    PlayerController controller;
    float timer = 0f;
    float originalValue = 0f;
    string effectName = "";
    float effectDuration = 0f;
    bool effectActive = false;

    [HideInInspector] public static PotionEffectManager Instance;
    public GameObject potionEffectCanvas;
    Text potionEffectText;

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

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();

        if (potionEffectCanvas == null)
        {
            potionEffectCanvas = GameObject.Find("UI").transform.Find("PotionEffectCanvas").gameObject;
        }

        potionEffectText = potionEffectCanvas.transform.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (effectActive && !PlayerController.isUIActive)
        {
            timer += Time.deltaTime;

            potionEffectText.text = effectName.ToUpperInvariant() + " active for " + (effectDuration - timer).ToString("F2") + "s";

            if (timer >= effectDuration)
            {
                var field = controller.GetType().GetField(effectName);
                field.SetValue(controller, originalValue);
                effectActive = false;
                timer = 0f;

                potionEffectText.text = "";
                potionEffectCanvas.SetActive(false);
            }
        }
    }

    public void ApplyEffect(PotionEffect effect)
    {
        var field = controller.GetType().GetField(effect.controllerVariable);
        originalValue = (float)field.GetValue(controller);
        field.SetValue(controller, originalValue + effect.value);
        effectActive = true;
        effectDuration = effect.duration;
        effectName = effect.controllerVariable;

        potionEffectCanvas.SetActive(true);
    }

    public string GetCurrentEffect()
    {
        if (effectActive)
        {
            return effectName;
        } else
        {
            return "";
        }
    }
}
