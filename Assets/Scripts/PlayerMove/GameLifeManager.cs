using UnityEngine;
using System.Collections.Generic;

public class GameLifeManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;

    private List<PlayerLifeManager> players = new List<PlayerLifeManager>();

    private void Start()
    {
        FindAllPlayers();
        InitializePlayers();
    }

    private void FindAllPlayers()
    {
        players.Clear();
        players.AddRange(FindObjectsByType<PlayerLifeManager>(FindObjectsSortMode.None));

        // Ordenar por índice
        players.Sort((a, b) => a.GetPlayerIndex().CompareTo(b.GetPlayerIndex()));

        Debug.Log($"GameLifeManager: Encontrados {players.Count} jugadores");
    }

    private void InitializePlayers()
    {
        // Cada PlayerLifeManager ya se inicializa solo en su Awake
        // Este manager solo los registra para control global
        foreach (var player in players)
        {
            Debug.Log($"Jugador {player.GetPlayerName()} (Índice: {player.GetPlayerIndex()}) - Vidas: {player.GetCurrentLives()}");
        }
    }

    public bool IsGameOver()
    {
        int playersAlive = 0;
        foreach (var player in players)
        {
            if (player.GetCurrentLives() > 0)
            {
                playersAlive++;
            }
        }
        return playersAlive <= 1;
    }

    public PlayerLifeManager GetWinner()
    {
        foreach (var player in players)
        {
            if (player.GetCurrentLives() > 0)
            {
                return player;
            }
        }
        return null;
    }

    public PlayerLifeManager GetPlayerByIndex(int index)
    {
        foreach (var player in players)
        {
            if (player.GetPlayerIndex() == index)
            {
                return player;
            }
        }
        return null;
    }

    [ContextMenu("Reset All Players")]
    public void ResetAllPlayers()
    {
        foreach (var player in players)
        {
            player.ResetLives();
        }
        Debug.Log("Todos los jugadores reseteados");
    }

    [ContextMenu("Check Game State")]
    public void CheckGameState()
    {
        Debug.Log($"¿Juego terminado? {IsGameOver()}");
        if (IsGameOver())
        {
            var winner = GetWinner();
            if (winner != null)
            {
                Debug.Log($"¡Ganador: {winner.GetPlayerName()}!");
            }
            else
            {
                Debug.Log("Empate o sin ganador");
            }
        }
    }
}