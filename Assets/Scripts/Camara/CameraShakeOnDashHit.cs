using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeOnDashHit : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.5f;

 
    private static CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }
        }
    }

    public static void Shake(float intensity = 0.5f)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(Vector3.one * intensity);
        }
    }


    public void DoShake()
    {
        impulseSource?.GenerateImpulse(Vector3.one * shakeIntensity);
    }
}