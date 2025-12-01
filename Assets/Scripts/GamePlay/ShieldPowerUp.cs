using UnityEngine;
using System.Collections;

public class ShieldPowerUp : BasePowerUp
{
    [Header("Shield Config")]
    [SerializeField] private float shieldDuration = 3f;
    [SerializeField] private Color shieldColor = Color.blue;

    protected override void ApplyEffect(PlayerLifeManager player)
    {
        base.ApplyEffect(player);

        Debug.Log($"=== ESCUDO ACTIVADO para {player.GetPlayerName()} ===");
        StartCoroutine(ApplyShieldEffect(player));
    }

    private IEnumerator ApplyShieldEffect(PlayerLifeManager player)
    {
        // Activar escudo en el MovementController
        MovementController movement = player.GetComponent<MovementController>();
        if (movement != null)
        {
            movement.ActivateShield(shieldDuration);
        }

        // Crear objeto escudo visual
        GameObject shieldObject = CreateShieldVisual(player.transform);

        Debug.Log($"Escudo activo. Jugador INMUNE por {shieldDuration} segundos");

        // Esperar duración del escudo
        yield return new WaitForSeconds(shieldDuration);

        // Remover escudo visual
        if (shieldObject != null)
        {
            Destroy(shieldObject);
        }

        Debug.Log($"Escudo terminado para {player.GetPlayerName()}");
    }

    private GameObject CreateShieldVisual(Transform playerTransform)
    {
        GameObject shield = new GameObject("PlayerShield");
        shield.transform.position = playerTransform.position;
        shield.transform.SetParent(playerTransform);

        SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();
        sr.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.5f);
        sr.sortingOrder = 5; // Sobre el jugador

        // Usar el mismo sprite del jugador (o uno circular)
        SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSprite != null && playerSprite.sprite != null)
        {
            sr.sprite = playerSprite.sprite;
        }

        // Escalar un poco más grande
        shield.transform.localScale = Vector3.one * 1.2f;

        // Añadir CircleCollider2D para detectar colisiones
        CircleCollider2D shieldCollider = shield.AddComponent<CircleCollider2D>();
        shieldCollider.isTrigger = true;
        shieldCollider.radius = 0.6f;

        // Añadir script para bloquear empujones
        ShieldCollisionHandler handler = shield.AddComponent<ShieldCollisionHandler>();
        handler.Initialize(playerTransform.GetComponent<PlayerLifeManager>());

        return shield;
    }
}

// Script que maneja las colisiones del escudo
public class ShieldCollisionHandler : MonoBehaviour
{
    private PlayerLifeManager protectedPlayer;

    public void Initialize(PlayerLifeManager player)
    {
        protectedPlayer = player;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (protectedPlayer == null) return;

        // Si otro jugador intenta dashear
        if (other.CompareTag("Player") && other.gameObject != protectedPlayer.gameObject)
        {
            MovementController otherMovement = other.GetComponent<MovementController>();
            if (otherMovement != null && otherMovement.IsDashing)
            {
                Debug.Log($"ESCUDO BLOQUEÓ dash de {other.gameObject.name}");

                // Crear efecto de bloqueo
                CreateBlockEffect(transform.position);

                // El jugador con escudo NO es empujado
                return;
            }
        }

        // Si es un shockwave
        if (other.name.Contains("Shockwave"))
        {
            Debug.Log("ESCUDO BLOQUEÓ shockwave");
            Destroy(other.gameObject); // Destruir el shockwave
        }
    }

    private void CreateBlockEffect(Vector3 position)
    {
        GameObject effect = new GameObject("ShieldBlockEffect");
        effect.transform.position = position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.color = Color.blue;
        sr.sortingOrder = 10;

        Destroy(effect, 0.3f);
    }
}