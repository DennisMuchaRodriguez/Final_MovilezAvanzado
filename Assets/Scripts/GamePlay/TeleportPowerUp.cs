using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportPowerUp : BasePowerUp
{
    [Header("Teleport Config")]
    [SerializeField] private float teleportRange = 8f;
    [SerializeField] private GameObject teleportEffect;
    [SerializeField] private LayerMask playerLayer = 1 << 0; // Default layer

    protected override void ApplyEffect(PlayerLifeManager player)
    {
        base.ApplyEffect(player);

        Debug.Log($"=== TELEPORT ACTIVADO por {player.GetPlayerName()} ===");

        // Buscar TODOS los jugadores en la escena
        PlayerLifeManager[] allPlayers = GameObject.FindObjectsByType<PlayerLifeManager>(FindObjectsSortMode.None);
        Debug.Log($"Total jugadores encontrados: {allPlayers.Length}");

        // Crear lista de jugadores válidos (excluyendo al que recogió el power-up)
        List<PlayerLifeManager> validPlayers = new List<PlayerLifeManager>();

        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer == null) continue;
            if (otherPlayer.gameObject == player.gameObject) continue;
            if (!otherPlayer.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(player.transform.position, otherPlayer.transform.position);
            Debug.Log($"- {otherPlayer.GetPlayerName()} a distancia: {distance:F1} (rango: {teleportRange})");

            if (distance <= teleportRange)
            {
                validPlayers.Add(otherPlayer);
            }
        }

        Debug.Log($"Jugadores válidos para teleport: {validPlayers.Count}");

        if (validPlayers.Count > 0)
        {
            // Seleccionar el jugador más cercano
            PlayerLifeManager targetPlayer = validPlayers[0];
            float closestDistance = Vector2.Distance(player.transform.position, targetPlayer.transform.position);

            foreach (var p in validPlayers)
            {
                float d = Vector2.Distance(player.transform.position, p.transform.position);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    targetPlayer = p;
                }
            }

            Debug.Log($"OBJETIVO SELECCIONADO: {targetPlayer.GetPlayerName()} ({closestDistance:F1} unidades)");

            // Ejecutar teleport
            StartCoroutine(ExecuteTeleport(player, targetPlayer));
        }
        else
        {
            Debug.LogWarning("No hay jugadores en rango para teleport");
        }
    }

    private IEnumerator ExecuteTeleport(PlayerLifeManager player1, PlayerLifeManager player2)
    {
        Debug.Log($"INICIANDO TELEPORT: {player1.GetPlayerName()} <-> {player2.GetPlayerName()}");

        // Guardar posiciones originales
        Vector3 pos1 = player1.transform.position;
        Vector3 pos2 = player2.transform.position;

        Debug.Log($"Posición {player1.GetPlayerName()}: {pos1}");
        Debug.Log($"Posición {player2.GetPlayerName()}: {pos2}");

        // Efecto visual ANTES
        CreateTeleportEffect(pos1, Color.magenta);
        CreateTeleportEffect(pos2, Color.cyan);

        yield return new WaitForSeconds(0.15f);

        // INTERCAMBIAR POSICIONES
        player1.transform.position = pos2;
        player2.transform.position = pos1;

        // Forzar actualización de física
        Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();

        if (rb1 != null) rb1.linearVelocity = Vector2.zero;
        if (rb2 != null) rb2.linearVelocity = Vector2.zero;

        Debug.Log($"TELEPORT COMPLETADO:");
        Debug.Log($"{player1.GetPlayerName()} ahora en: {player1.transform.position}");
        Debug.Log($"{player2.GetPlayerName()} ahora en: {player2.transform.position}");

        // Efecto visual DESPUÉS
        CreateTeleportEffect(player1.transform.position, Color.magenta);
        CreateTeleportEffect(player2.transform.position, Color.cyan);
    }

    private void CreateTeleportEffect(Vector3 position, Color color)
    {
        GameObject effect = new GameObject("TeleportEffect");
        effect.transform.position = position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingOrder = 100;

        // Crear un círculo simple
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        circle.transform.SetParent(effect.transform);
        circle.transform.localPosition = Vector3.zero;
        circle.transform.localScale = Vector3.one * 0.5f;

        Destroy(effect, 0.5f);
    }
}