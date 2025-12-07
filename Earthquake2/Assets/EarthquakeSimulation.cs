using System; // Added for Actions
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class EarthquakeSimulator : MonoBehaviour
{
    // --- NEW EVENT SYSTEM ---
    public static event Action<float> OnShakeUpdate;
    // ------------------------

    public enum QuakeIntensity
    {
        Low,
        Medium,
        High
    }

    [Header("1. SHAKE SETTINGS")]
    [Tooltip("Total time the shaking will last (seconds).")]
    public float duration = 60f; // Updated to 1 minute as requested
    
    [Tooltip("Select the intensity level for the next earthquake.")]
    public QuakeIntensity intensityLevel = QuakeIntensity.Medium;

    [Tooltip("Base translational intensity of the camera shake. This is multiplied by the Intensity Level.")]
    public float baseShakeIntensity = 0.05f;

    [Header("2. REQUIRED REFERENCES")]
    [Tooltip("Drag the XR Origin (VR) GameObject here.")]
    public Transform xrOrigin;
    
    [Tooltip("Drag the Left Controller (child of Camera Offset) here.")]
    public Transform leftController;
    
    [Tooltip("Drag the Right Controller (child of Camera Offset) here.")]
    public Transform rightController;
    
    [Tooltip("Drag your Rumble Audio Source here. MUST be set to NOT 'Play On Awake'.")]
    public AudioSource rumbleAudio;

    [Header("4. ADVANCED AUDIO LAYERS")]
    [Tooltip("Looping sound for structural stress (creaking, groaning).")]
    public AudioSource stressLoopSource;
    
    [Tooltip("Looping sound for environmental ambience (alarms, dogs, etc).")]
    public AudioSource ambienceSource;

    [Tooltip("Source for playing one-shot sounds (debris, impacts).")]
    public AudioSource oneShotSource;

    [Tooltip("Random sounds of things falling or breaking.")]
    public List<AudioClip> debrisSounds;

    [Tooltip("Random sharp cracks or loud stress sounds.")]
    public List<AudioClip> stressOneShots;

    [Header("3. COMFORT VIGNETTE (OPTIONAL)")]
    [Tooltip("Drag the TunnelingVignetteController from the Main Camera here.")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController vignetteController; 
    
    [Tooltip("Desired vignette size during the quake (0.0 = full black; 1.0 = none).")]
    [Range(0f, 1f)]
    public float vignetteSizeDuringQuake = 0.3f; 

    public void StartEarthquake()
    {
        StopAllCoroutines(); 
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0.0f;
        
        // Determine Multiplier based on Intensity Level
        float intensityMultiplier = 1.0f;
        switch (intensityLevel)
        {
            case QuakeIntensity.Low: intensityMultiplier = 0.5f; break;
            case QuakeIntensity.Medium: intensityMultiplier = 1.0f; break;
            case QuakeIntensity.High: intensityMultiplier = 2.5f; break;
        }

        float targetShakeIntensity = baseShakeIntensity * intensityMultiplier;

        // --- Setup Audio ---
        if (rumbleAudio != null) { rumbleAudio.volume = 0f; rumbleAudio.Play(); }
        if (stressLoopSource != null) { stressLoopSource.volume = 0f; stressLoopSource.Play(); }
        if (ambienceSource != null) { ambienceSource.volume = 0f; ambienceSource.Play(); }
        
        if (vignetteController != null)
        {
            vignetteController.enabled = true;
            vignetteController.defaultParameters.apertureSize = vignetteSizeDuringQuake; 
        }

        // --- THE MAIN SHAKE LOOP ---
        while (elapsed < duration)
        {
            // Calculate how strong the shake is right now (fades out near end)
            float normalizedTime = elapsed / duration;
            // Simple curve: ramp up quickly, hold, then fade out
            float fadeOutPoint = 0.8f;
            float envelope = 1.0f;
            if (normalizedTime > fadeOutPoint)
            {
                envelope = 1.0f - ((normalizedTime - fadeOutPoint) / (1.0f - fadeOutPoint));
            }
            
            float currentStrength = targetShakeIntensity * envelope;
            float currentAudioVol = envelope; // Audio volume follows the envelope

            // Update Audio Volumes
            if (rumbleAudio != null) rumbleAudio.volume = currentAudioVol;
            if (stressLoopSource != null) stressLoopSource.volume = currentAudioVol * 0.8f; // Slightly quieter
            if (ambienceSource != null) ambienceSource.volume = currentAudioVol * 0.6f;

            // Random One-Shot Sounds
            // Chance to play sound increases with intensity
            if (oneShotSource != null && currentStrength > 0.01f)
            {
                // Try to play debris sound
                if (debrisSounds != null && debrisSounds.Count > 0)
                {
                    // Higher intensity = higher chance. 
                    // e.g. 1% chance per frame at low, 5% at high. 
                    if (UnityEngine.Random.value < (0.02f * intensityMultiplier)) 
                    {
                        PlayRandomClip(debrisSounds, 0.5f + (0.5f * intensityMultiplier));
                    }
                }

                // Try to play stress crack
                if (stressOneShots != null && stressOneShots.Count > 0)
                {
                    if (UnityEngine.Random.value < (0.01f * intensityMultiplier))
                    {
                        PlayRandomClip(stressOneShots, 0.7f + (0.3f * intensityMultiplier));
                    }
                }
            }

            // 1. BROADCAST TO GROUND OBJECTS & LIGHTS
            // 1. BROADCAST TO GROUND OBJECTS & LIGHTS
            // Increased to 4x (was 10x originally, then 1x) to get movement without explosion.
            OnShakeUpdate?.Invoke(currentStrength * 10f); 

            // 2. VISUAL SHAKE (VR RIG)
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * currentStrength;
            
            xrOrigin.localPosition += randomOffset;
            
            Vector3 controllerShake = randomOffset * 0.75f;
            leftController.localPosition += controllerShake;
            rightController.localPosition += controllerShake;

            // 3. HAPTICS
            TriggerHaptics(currentStrength);

            yield return null;

            // --- Reset Phase ---
            xrOrigin.localPosition -= randomOffset;
            leftController.localPosition -= controllerShake;
            rightController.localPosition -= controllerShake;

            elapsed += Time.deltaTime;
        }

        // --- CLEANUP ---
        
        OnShakeUpdate?.Invoke(0f); 

        if (vignetteController != null) 
        {
            StartCoroutine(FadeVignetteOut(1.0f));
        }

        // Fade out all audio
        StartCoroutine(FadeAudioOut(2.0f));
    }

    IEnumerator FadeVignetteOut(float fadeDuration)
    {
        float startTime = Time.time;
        float startSize = vignetteController.defaultParameters.apertureSize; 
        float endSize = 1.0f; 

        while (Time.time < startTime + fadeDuration)
        {
            float t = (Time.time - startTime) / fadeDuration;
            vignetteController.defaultParameters.apertureSize = Mathf.Lerp(startSize, endSize, t);
            yield return null;
        }
        vignetteController.defaultParameters.apertureSize = 1.0f;
        vignetteController.enabled = false;
    }

    IEnumerator FadeAudioOut(float fadeTime)
    {
        float startRumble = rumbleAudio != null ? rumbleAudio.volume : 0f;
        float startStress = stressLoopSource != null ? stressLoopSource.volume : 0f;
        float startAmbience = ambienceSource != null ? ambienceSource.volume : 0f;
        
        float currentFadeTime = 0f;

        while (currentFadeTime < fadeTime)
        {
            float t = currentFadeTime / fadeTime;
            if (rumbleAudio != null) rumbleAudio.volume = Mathf.Lerp(startRumble, 0f, t);
            if (stressLoopSource != null) stressLoopSource.volume = Mathf.Lerp(startStress, 0f, t);
            if (ambienceSource != null) ambienceSource.volume = Mathf.Lerp(startAmbience, 0f, t);
            
            currentFadeTime += Time.deltaTime;
            yield return null;
        }
        
        if (rumbleAudio != null) rumbleAudio.Stop();
        if (stressLoopSource != null) stressLoopSource.Stop();
        if (ambienceSource != null) ambienceSource.Stop();
    }

    void PlayRandomClip(List<AudioClip> clips, float volumeScale)
    {
        if (clips.Count == 0) return;
        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Count)];
        oneShotSource.PlayOneShot(clip, volumeScale);
    }

    void TriggerHaptics(float strength)
    {
        strength = Mathf.Clamp01(strength);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHand.isValid) leftHand.SendHapticImpulse(0, strength, 0.1f);
        
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.isValid) rightHand.SendHapticImpulse(0, strength, 0.1f);
    }
}