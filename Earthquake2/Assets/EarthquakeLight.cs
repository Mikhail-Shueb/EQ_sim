using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class EarthquakeLight : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("The minimum intensity of the earthquake required to start flickering this light.")]
    public float flickerThreshold = 0.1f;

    [Tooltip("How frequently the light toggles on/off.")]
    public float flickerSpeed = 0.05f;

    private Light myLight;
    private float originalIntensity;
    private bool isQuaking = false;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        myLight = GetComponent<Light>();
        originalIntensity = myLight.intensity;
    }

    private void OnEnable()
    {
        EarthquakeSimulator.OnShakeUpdate += HandleShake;
    }

    private void OnDisable()
    {
        EarthquakeSimulator.OnShakeUpdate -= HandleShake;
    }

    private void HandleShake(float quakeIntensity)
    {
        // Check if the earthquake is strong enough to affect lights
        if (quakeIntensity > flickerThreshold)
        {
            if (!isQuaking)
            {
                isQuaking = true;
                flickerCoroutine = StartCoroutine(FlickerRoutine());
            }
        }
        else
        {
            // Earthquake ended or is too weak
            if (isQuaking)
            {
                isQuaking = false;
                if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
                
                // Restore light to normal
                myLight.enabled = true;
                myLight.intensity = originalIntensity;
            }
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (isQuaking)
        {
            // Randomly decide to be ON or OFF
            // 70% chance to be ON, 30% chance to flicker OFF
            bool lightState = Random.value > 0.3f;
            
            myLight.enabled = lightState;

            // Occasionally dim the light instead of turning it off completely
            if (lightState)
            {
                myLight.intensity = originalIntensity * Random.Range(0.5f, 1.2f);
            }

            // Wait a tiny random amount of time before next state change
            yield return new WaitForSeconds(Random.Range(0.02f, flickerSpeed * 5));
        }
    }
}