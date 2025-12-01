using UnityEngine;
using System.Collections;

public class MegaDashPowerUp : BasePowerUp
{
    [Header("Mega Dash Config")]
    [SerializeField] private float dashMultiplier = 2f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private Color dashEffectColor = new Color(1f, 0.5f, 0f); // Naranja

    private float originalDashForce;
    private float originalDashSpeed;
    private Color originalTrailColor;
    private MovementController playerMovement;

    protected override void ApplyEffect(PlayerLifeManager player)
    {
        base.ApplyEffect(player);

        // Buscar el MovementController del jugador
        playerMovement = player.GetComponent<MovementController>();
        if (playerMovement != null)
        {
            StartCoroutine(ApplyMegaDashEffect(playerMovement, player));
        }
        else
        {
            Debug.LogError($"MovementController no encontrado en {player.GetPlayerName()}");
        }
    }

    private IEnumerator ApplyMegaDashEffect(MovementController movement, PlayerLifeManager player)
    {
        Debug.Log($"Aplicando Mega Dash a {player.GetPlayerName()}");

        // Guardar configuración original del DashController
        DashController dashController = movement.GetComponent<DashController>();
        if (dashController != null)
        {
            // Obtener dash force del dashController (si existe) o del movement
            originalDashForce = dashController.dashPushForce; // Necesitamos acceso a dashPushForce
        }

        // Guardar configuración original del MovementController
        originalDashSpeed = movement.dashSpeed;
        originalTrailColor = movement.trailColor;

        // Aplicar mega dash - Aumentar velocidad del dash
        movement.dashSpeed *= dashMultiplier;

        // Aura visual
        GameObject aura = CreateAuraEffect(player.transform);

        Debug.Log($"Mega Dash activado para {player.GetPlayerName()}. Dash speed: {movement.dashSpeed}");

        // Esperar duración del power-up
        yield return new WaitForSeconds(duration);

        // Restaurar configuración original
        movement.dashSpeed = originalDashSpeed;

        // Remover aura
        if (aura != null)
            Destroy(aura);

        Debug.Log($"Mega Dash terminado para {player.GetPlayerName()}. Dash speed restaurado: {movement.dashSpeed}");
    }

    private GameObject CreateAuraEffect(Transform playerTransform)
    {
        GameObject aura = new GameObject("MegaDashAura");
        aura.transform.SetParent(playerTransform);
        aura.transform.localPosition = Vector3.zero;

        SpriteRenderer auraRenderer = aura.AddComponent<SpriteRenderer>();

        // Usar sprite del jugador o crear un círculo
        SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            auraRenderer.sprite = playerSprite.sprite;
        }

        auraRenderer.color = new Color(dashEffectColor.r, dashEffectColor.g, dashEffectColor.b, 0.3f);
        auraRenderer.sortingOrder = -1;

        // Escalar un poco más grande que el jugador
        aura.transform.localScale = Vector3.one * 1.2f;

        // Animación de pulso
        StartCoroutine(PulseAura(auraRenderer));

        return aura;
    }

    private IEnumerator PulseAura(SpriteRenderer auraRenderer)
    {
        float pulseSpeed = 2f;
        float timer = 0f;

        while (auraRenderer != null)
        {
            timer += Time.deltaTime * pulseSpeed;
            float alpha = 0.3f + Mathf.Sin(timer) * 0.2f;
            auraRenderer.color = new Color(dashEffectColor.r, dashEffectColor.g, dashEffectColor.b, alpha);

            // Escalar ligeramente
            float scale = 1.2f + Mathf.Sin(timer) * 0.1f;
            auraRenderer.transform.localScale = Vector3.one * scale;

            yield return null;
        }
    }
}