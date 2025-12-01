using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int playersRequiredToWin = 1;

    [Header("Events")]
    public UnityEvent<PlayerLifeManager> OnGameWon;
    public UnityEvent OnGameDraw;

    [Header("References")]
    [SerializeField] private VictoryUI victoryUI;

    private List<PlayerLifeManager> players = new List<PlayerLifeManager>();
    private bool gameEnded = false;

    private void Start()
    {
        Debug.Log("=== GAME STATE MANAGER INICIADO ===");

        // Buscar jugadores iniciales
        FindAllPlayers();

        // Suscribirse a eventos estáticos para nuevos jugadores
        PlayerLifeManager.OnPlayerSpawned += OnPlayerSpawned;

        // Suscribir eventos a la UI
        SetupVictoryUI();

        Debug.Log($"Total de jugadores iniciales: {players.Count}");
    }

    private void OnPlayerSpawned(PlayerLifeManager newPlayer)
    {
        Debug.Log($"Nuevo jugador apareció: {newPlayer.GetPlayerName()}");

        if (!players.Contains(newPlayer))
        {
            players.Add(newPlayer);

            // Suscribirse al evento de eliminación del nuevo jugador
            newPlayer.OnPlayerEliminated.AddListener(CheckGameState);

            Debug.Log($"Jugador {newPlayer.GetPlayerName()} añadido. Total: {players.Count}");
        }
    }

    private void FindAllPlayers()
    {
        var foundPlayers = FindObjectsByType<PlayerLifeManager>(FindObjectsSortMode.None);
        players.Clear();

        foreach (var player in foundPlayers)
        {
            if (player != null && !players.Contains(player))
            {
                players.Add(player);

                // Suscribirse a eventos de muerte de jugadores
                player.OnPlayerEliminated.AddListener(CheckGameState);

                Debug.Log($"Jugador encontrado: {player.GetPlayerName()} (Índice: {player.GetPlayerIndex()})");
            }
        }

        Debug.Log($"Total jugadores encontrados: {players.Count}");
    }

    private void SetupVictoryUI()
    {
        if (victoryUI != null)
        {
            OnGameWon.AddListener(victoryUI.ShowVictoryScreen);
            OnGameDraw.AddListener(victoryUI.ShowDrawScreen);
            Debug.Log("UI asignada manualmente");
        }
        else
        {
            Debug.Log("Buscando VictoryUI en escena...");
            victoryUI = FindFirstObjectByType<VictoryUI>();
            if (victoryUI != null)
            {
                OnGameWon.AddListener(victoryUI.ShowVictoryScreen);
                OnGameDraw.AddListener(victoryUI.ShowDrawScreen);
                Debug.Log("VictoryUI encontrada y suscrita");
            }
            else
            {
                Debug.LogError("¡NO SE ENCONTRÓ VICTORY UI EN LA ESCENA!");
            }
        }
    }

    private void CheckGameState()
    {
        Debug.Log("=== CHECKGAMESTATE LLAMADO ===");
        Debug.Log($"Game ended? {gameEnded}");

        if (gameEnded)
        {
            Debug.Log("El juego ya terminó, ignorando...");
            return;
        }

        // Verificar que tenemos jugadores
        if (players.Count == 0)
        {
            Debug.LogWarning("No hay jugadores registrados en el GameStateManager");
            return;
        }

        int playersAlive = 0;
        PlayerLifeManager lastPlayerAlive = null;

        Debug.Log($"Analizando {players.Count} jugadores...");

        // Filtrar jugadores nulos o destruidos
        players.RemoveAll(p => p == null);

        foreach (var player in players)
        {
            if (player != null)
            {
                int lives = player.GetCurrentLives();
                bool isActive = player.gameObject.activeInHierarchy;
                Debug.Log($"{player.GetPlayerName()}: Vidas={lives}, Activo={isActive}, GameObject activo={player.gameObject.activeSelf}");

                if (lives > 0)
                {
                    playersAlive++;
                    lastPlayerAlive = player;
                    Debug.Log($"  -> Está vivo! (Total vivos ahora: {playersAlive})");
                }
            }
        }

        Debug.Log($"RESULTADO: {playersAlive} jugadores vivos de {players.Count}");
        Debug.Log($"Último jugador vivo: {(lastPlayerAlive != null ? lastPlayerAlive.GetPlayerName() : "NINGUNO")}");
        Debug.Log($"Condición para ganar: playersAlive ({playersAlive}) <= playersRequiredToWin ({playersRequiredToWin})");

        // Verificar condición de victoria
        if (playersAlive <= playersRequiredToWin)
        {
            gameEnded = true;
            Debug.Log("¡CONDICIÓN DE VICTORIA CUMPLIDA!");
            Debug.Log($"Game ended establecido a: {gameEnded}");

            if (lastPlayerAlive != null)
            {
                // Hay un ganador
                Debug.Log($"¡GANADOR DETECTADO: {lastPlayerAlive.GetPlayerName()}!");
                Debug.Log($"Invocando OnGameWon para {lastPlayerAlive.GetPlayerName()}");
                OnGameWon?.Invoke(lastPlayerAlive);
            }
            else
            {
                // Empate
                Debug.Log("¡EMPATE DETECTADO!");
                Debug.Log("Invocando OnGameDraw");
                OnGameDraw?.Invoke();
            }
        }
        else
        {
            Debug.Log("Condición de victoria NO cumplida, juego continúa...");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento estático
        PlayerLifeManager.OnPlayerSpawned -= OnPlayerSpawned;
    }

    [ContextMenu("Force Check Game State")]
    public void ForceCheckGameState()
    {
        Debug.Log("=== FORCE CHECK GAME STATE ===");
        CheckGameState();
    }

    [ContextMenu("Test Win Condition")]
    public void TestWinCondition()
    {
        Debug.Log("=== TEST WIN CONDITION ===");
        if (players.Count > 0 && players[0] != null)
        {
            Debug.Log($"Simulando victoria para {players[0].GetPlayerName()}");
            OnGameWon?.Invoke(players[0]);
        }
    }

    [ContextMenu("Debug All Players")]
    public void DebugAllPlayers()
    {
        FindAllPlayers();
        Debug.Log($"=== DEBUG PLAYERS ({players.Count}) ===");
        foreach (var player in players)
        {
            if (player != null)
            {
                Debug.Log($"- {player.GetPlayerName()}: Vidas={player.GetCurrentLives()}, Activo={player.gameObject.activeInHierarchy}");
            }
        }
    }
}