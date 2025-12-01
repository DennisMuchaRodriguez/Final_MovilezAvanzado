using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool instantDeath = true; // Nueva opción

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            var player = other.GetComponent<PlayerLifeManager>();
            if (player != null)
            {
                if (instantDeath)
                {
                    // Muerte instantánea
                    player.InstantDeath();
                }
                else
                {
                    // Método normal (con delay de respawn)
                    player.HandleFall();
                }
            }
            else
            {
                // Buscar en padres
                player = other.GetComponentInParent<PlayerLifeManager>();
                if (player != null)
                {
                    if (instantDeath)
                        player.InstantDeath();
                    else
                        player.HandleFall();
                }
            }
        }
    }
}