using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float dashHitShakeIntensity = 0.5f;
    [SerializeField] private float dashHitShakeDuration = 0.3f;

    private Camera mainCamera;
    private Vector3 originalCameraPos;
    private bool isShaking = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.position;
        }
    }

    private void OnEnable()
    {
        
        CameraShakeEvents.OnDashHit += HandleDashHitShake;
    }

    private void OnDisable()
    {
        
        CameraShakeEvents.OnDashHit -= HandleDashHitShake;
    }

    private void HandleDashHitShake()
    {
        if (!isShaking && mainCamera != null)
        {
            StartCoroutine(ShakeCameraCoroutine());
        }
    }

    private IEnumerator ShakeCameraCoroutine()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < dashHitShakeDuration)
        {
            float currentIntensity = Mathf.Lerp(dashHitShakeIntensity, 0f, elapsed / dashHitShakeDuration);
            float x = Random.Range(-1f, 1f) * currentIntensity;
            float y = Random.Range(-1f, 1f) * currentIntensity;

            mainCamera.transform.position = originalCameraPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCameraPos;
        isShaking = false;
    }
}