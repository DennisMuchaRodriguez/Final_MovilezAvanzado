using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLifeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private int targetPlayerIndex = 0;

    [Header("GameState Reference")]
    [SerializeField] private GameStateManager gameStateManager; // Referencia opcional

    private PlayerLifeManager targetPlayer;

    private void OnEnable()
    {
        // Suscribirse a eventos estáticos
        PlayerLifeManager.OnPlayerSpawned += HandlePlayerSpawned;
        PlayerLifeManager.OnPlayerLifeChanged += HandlePlayerLifeChanged;

        // Buscar jugador existente
        FindExistingPlayer();

        // Buscar GameStateManager si no está asignado
        if (gameStateManager == null)
        {
            gameStateManager = FindFirstObjectByType<GameStateManager>();
        }
    }

    private void OnDisable()
    {
        // Desuscribirse
        PlayerLifeManager.OnPlayerSpawned -= HandlePlayerSpawned;
        PlayerLifeManager.OnPlayerLifeChanged -= HandlePlayerLifeChanged;
    }

    private void FindExistingPlayer()
    {
        // Buscar jugador que ya existe
        var players = FindObjectsByType<PlayerLifeManager>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetPlayerIndex() == targetPlayerIndex)
            {
                SetTargetPlayer(player);
                break;
            }
        }
    }

    private void HandlePlayerSpawned(PlayerLifeManager player)
    {
        if (player.GetPlayerIndex() == targetPlayerIndex)
        {
            SetTargetPlayer(player);
        }
    }

    private void HandlePlayerLifeChanged(int playerIndex, int newLives)
    {
        if (playerIndex == targetPlayerIndex)
        {
            UpdateLivesDisplay(newLives);
        }
    }

    private void SetTargetPlayer(PlayerLifeManager player)
    {
        targetPlayer = player;

        if (playerNameText != null)
        {
            playerNameText.text = player.GetPlayerName();
        }

        UpdateLivesDisplay(player.GetCurrentLives());

        Debug.Log($"UI conectada a: {player.GetPlayerName()}");
    }

    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
}