using UnityEngine;

public class EarthquakeObject : MonoBehaviour
{
    public enum LockedAxis
    {
        X,
        Y,
        Z
    }

    [Header("Shake Settings")]
    [Tooltip("Strength of the shake force applied to the object.")]
    public float positionMultiplier = 0.2f;

    [Tooltip("Strength of the rotation force (torque).")]
    public float rotationMultiplier = 0.5f;

    [Tooltip("Which axis should NOT have shake force applied? (Usually Y for gravity).")]
    public LockedAxis lockedAxis = LockedAxis.Y;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isShaking = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        EarthquakeSimulator.OnShakeUpdate += HandleShake;
    }

    private void OnDisable()
    {
        EarthquakeSimulator.OnShakeUpdate -= HandleShake;
    }

    private void HandleShake(float intensity)
    {
        if (intensity <= 0.001f)
        {
            if (isShaking)
            {
                // Only reset non-physics objects. Physics objects stay where they fell.
                if (rb == null)
                {
                    transform.localPosition = initialPosition;
                    transform.localRotation = initialRotation;
                }
                isShaking = false;
            }
            return;
        }

        isShaking = true;

        float shakeForce = intensity * positionMultiplier;
        float rotForce = intensity * rotationMultiplier;

        // Generate random noise for the two free axes
        float noiseA = Random.Range(-1f, 1f) * shakeForce;
        float noiseB = Random.Range(-1f, 1f) * shakeForce;

        if (rb != null)
        {
            // Apply forces to Rigidbody (respects collisions and gravity)
            Vector3 force = Vector3.zero;

            switch (lockedAxis)
            {
                case LockedAxis.X:
                    force = new Vector3(0, noiseA, noiseB);
                    break;
                case LockedAxis.Y:
                    force = new Vector3(noiseA, 0, noiseB);
                    break;
                case LockedAxis.Z:
                    force = new Vector3(noiseA, noiseB, 0);
                    break;
            }

            // Using VelocityChange for instant, snappy movement that ignores mass
            rb.AddForce(force, ForceMode.VelocityChange);

            Vector3 torque = Random.insideUnitSphere * rotForce;
            rb.AddTorque(torque, ForceMode.VelocityChange);
        }
        else
        {
            // Fallback for non-physics objects (still uses teleportation)
            Vector3 shakeOffset = Vector3.zero;

            switch (lockedAxis)
            {
                case LockedAxis.X:
                    shakeOffset = new Vector3(0, noiseA, noiseB);
                    break;
                case LockedAxis.Y:
                    shakeOffset = new Vector3(noiseA, 0, noiseB);
                    break;
                case LockedAxis.Z:
                    shakeOffset = new Vector3(noiseA, noiseB, 0);
                    break;
            }

            transform.localPosition = initialPosition + shakeOffset;

            float rX = Random.Range(-1f, 1f) * rotForce;
            float rY = Random.Range(-1f, 1f) * rotForce;
            float rZ = Random.Range(-1f, 1f) * rotForce;

            transform.localRotation = initialRotation * Quaternion.Euler(rX, rY, rZ);
        }
    }
}