using UnityEngine;
using System;

public static class CameraShakeEvents
{
    
    public static event Action OnDashHit;

    public static void TriggerDashHitShake()
    {
        OnDashHit?.Invoke();
    }
}