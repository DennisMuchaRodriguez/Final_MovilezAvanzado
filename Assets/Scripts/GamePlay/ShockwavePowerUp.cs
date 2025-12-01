using UnityEngine;
using System.Collections;

public class ShockwavePowerUp : BasePowerUp
{
    [Header("Shockwave Config")]
    [SerializeField] private float shockwaveForce = 20f;
    [SerializeField] private float shockwaveRadius = 6f;
    [SerializeField] private float pushDuration = 0.3f;

    protected override void ApplyEffect(PlayerLifeManager player)
    {
        base.ApplyEffect(player);

        Debug.Log($"=== SHOCKWAVE ACTIVADO por {player.GetPlayerName()} ===");
        StartCoroutine(ExecuteShockwave(player));
    }

    private IEnumerator ExecuteShockwave(PlayerLifeManager player)
    {
        Vector2 center = player.transform.position;
        Debug.Log($"Centro del shockwave: {center}");

        // Crear efecto visual
        CreateShockwaveEffect(center);

        yield return new WaitForSeconds(0.1f);

        // Buscar TODOS los jugadores manualmente (más confiable)
        PlayerLifeManager[] allPlayers = GameObject.FindObjectsByType<PlayerLifeManager>(FindObjectsSortMode.None);
        int playersPushed = 0;

        Debug.Log($"Buscando jugadores en radio {shockwaveRadius}...");

        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer == null) continue;
            if (otherPlayer.gameObject == player.gameObject) continue;
            if (!otherPlayer.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(center, otherPlayer.transform.position);
            Debug.Log($"- {otherPlayer.GetPlayerName()} a distancia: {distance:F1}");

            if (distance <= shockwaveRadius)
            {
                // Calcular dirección de empuje (alejándose del centro)
                Vector2 direction = (otherPlayer.transform.position - player.transform.position).normalized;

                // Aplicar empujón DIRECTO al MovementController
                MovementController movement = otherPlayer.GetComponent<MovementController>();
                if (movement != null)
                {
                    movement.GetPushed(direction, shockwaveForce, pushDuration);
                    playersPushed++;
                    Debug.Log($"  ¡EMPUJADO! {otherPlayer.GetPlayerName()} con fuerza {shockwaveForce}");
                }
                else
                {
                    Debug.LogWarning($"No se encontró MovementController en {otherPlayer.GetPlayerName()}");
                }
            }
        }

        Debug.Log($"Shockwave empujó a {playersPushed} jugadores");
    }

    private void CreateShockwaveEffect(Vector2 center)
    {
        // Crear múltiples círculos concéntricos
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(CreateExpandingCircle(center, i * 0.1f));
        }
    }

    private IEnumerator CreateExpandingCircle(Vector2 center, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject circle = new GameObject("ShockwaveCircle");
        circle.transform.position = center;

        SpriteRenderer sr = circle.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 0f, 0.7f); // Amarillo brillante
        sr.sortingOrder = 50;

        // Crear un círculo usando un sprite o GameObject primitivo
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitive.transform.SetParent(circle.transform);
        primitive.transform.localPosition = Vector3.zero;
        primitive.transform.localScale = Vector3.one * 0.1f;

        // Eliminar collider del primitivo
        Collider col = primitive.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Animación de expansión
        float duration = 0.5f;
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 endScale = Vector3.one * shockwaveRadius * 2f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            circle.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Desvanecer
            Color c = sr.color;
            c.a = Mathf.Lerp(0.7f, 0f, t);
            sr.color = c;

            yield return null;
        }

        Destroy(circle);
    }
}