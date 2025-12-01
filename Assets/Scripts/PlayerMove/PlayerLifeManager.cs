using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerLifeManager : MonoBehaviour
{
    public static event System.Action<PlayerLifeManager> OnPlayerSpawned;
    public static event System.Action<int, int> OnPlayerLifeChanged;

    [Header("Config")]
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private string playerName = "";
    [SerializeField] private int maxLives = 3;

    [Header("Spawn Settings")]
    [SerializeField] private bool useAssignedSpawn = true;
    private Vector2 assignedSpawnPosition;

    private int currentLives;
    private Rigidbody2D rb;
    private bool isInvincible = false;

    [Header("Events")]
    public UnityEvent<int> OnLifeLost;
    public UnityEvent OnRespawn;
    public UnityEvent OnPlayerEliminated;

    [Header("Player Colors")]
    [SerializeField] private Color playerColor = Color.white;
    [SerializeField] private bool autoAssignColor = true;
    private SpriteRenderer spriteRenderer;
    private bool isEliminated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentLives = maxLives;

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"P{playerIndex + 1}";
        }

        if (autoAssignColor)
        {
            AssignColorByIndex();
        }

        Debug.Log($"[Awake] Jugador {playerName} inicializado con {currentLives} vidas");
    }

    private void Start()
    {
        if (useAssignedSpawn && assignedSpawnPosition == Vector2.zero)
        {
            assignedSpawnPosition = transform.position;
        }

        Debug.Log($"[Start] Jugador {playerName} iniciado");

        // NOTIFICAR AL MUNDO QUE ESTE JUGADOR HA APARECIDO
        OnPlayerSpawned?.Invoke(this);
        OnPlayerLifeChanged?.Invoke(playerIndex, currentLives);

        OnLifeLost.AddListener((lives) => {
            Debug.Log($"[OnLifeLost] {playerName}, vidas restantes: {lives}");
            OnPlayerLifeChanged?.Invoke(playerIndex, lives);
        });
    }

    private void AssignColorByIndex()
    {
        if (spriteRenderer == null) return;

        switch (playerIndex)
        {
            case 0:
                playerColor = Color.red;
                break;
            case 1:
                playerColor = Color.blue;
                break;
            case 2:
                playerColor = Color.green;
                break;
            case 3:
                playerColor = Color.yellow;
                break;
            default:
                playerColor = Color.white;
                break;
        }

        spriteRenderer.color = playerColor;
    }
    public void SetColorByTeam(int teamId)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }

        switch (teamId)
        {
            case 1: // Team 1 - Rojo
                playerColor = Color.red;
                break;
            case 2: // Team 2 - Azul
                playerColor = Color.blue;
                break;
            default:
                playerColor = GetDefaultColor(playerIndex);
                break;
        }

        spriteRenderer.color = playerColor;
    }
    private Color GetDefaultColor(int index)
    {
        // Colores por defecto si no hay team
        Color[] defaultColors = {
        Color.red,     // P1
        Color.blue,    // P2
        Color.green,   // P3
        Color.yellow   // P4
    };

        return (index >= 0 && index < defaultColors.Length) ?
               defaultColors[index] : Color.white;
    }
    public void HandleFall()
    {
        Debug.Log($"=== HANDLEFALL LLAMADO PARA {playerName} ===");
        Debug.Log($"Es invencible? {isInvincible}");

        if (isInvincible)
        {
            Debug.Log($"{playerName} es invencible, ignorando caída");
            return;
        }

        Debug.Log($"{playerName} perdió una vida. Vidas antes: {currentLives}");
        currentLives--;
        Debug.Log($"Vidas después: {currentLives}");

        OnLifeLost?.Invoke(currentLives);

        if (currentLives > 0)
        {
            Debug.Log($"{playerName} todavía tiene vidas, reapareciendo...");
            Invoke(nameof(Respawn), 1f);
        }
        else
        {
            Debug.Log($"=== {playerName} ELIMINADO ===");
            Debug.Log("Invocando OnPlayerEliminated...");
            OnPlayerEliminated?.Invoke();
            Debug.Log("Desactivando objeto...");
            gameObject.SetActive(false);
            Debug.Log($"Estado después: Activo={gameObject.activeSelf}, Vidas={currentLives}");
        }
    }

    private void Respawn()
    {
        Debug.Log($"=== RESPAWN LLAMADO PARA {playerName} ===");
        Debug.Log($"Reapareciendo en: {assignedSpawnPosition}");

        transform.position = assignedSpawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        StartCoroutine(InvincibilityCoroutine());
        OnRespawn?.Invoke();
        Debug.Log($"{playerName} reaparecido exitosamente");
    }

    public void AssignSpawnPoint(Vector2 spawnPosition)
    {
        assignedSpawnPosition = spawnPosition;
        useAssignedSpawn = true;
        Debug.Log($"{playerName}: Spawn asignado en {spawnPosition}");
    }

    private IEnumerator InvincibilityCoroutine()
    {
        Debug.Log($"{playerName} ahora es invencible por 2 segundos");
        isInvincible = true;
        var sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            for (float t = 0; t < 2f; t += 0.1f)
            {
                sprite.enabled = !sprite.enabled;
                yield return new WaitForSeconds(0.1f);
            }
            sprite.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        isInvincible = false;
        Debug.Log($"{playerName} ya no es invencible");
    }

    // GETTERS
    public int GetCurrentLives()
    {
        Debug.Log($"[GetCurrentLives] {playerName}: {currentLives} vidas");
        return currentLives;
    }

    public int GetPlayerIndex() => playerIndex;
    public string GetPlayerName() => playerName;

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
        playerName = $"P{index + 1}";
    }

    public void ResetLives()
    {
        Debug.Log($"Reseteando vidas de {playerName} a {maxLives}");
        currentLives = maxLives;
        gameObject.SetActive(true);
        transform.position = assignedSpawnPosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        OnLifeLost?.Invoke(currentLives);
    }

    public void InstantDeath()
    {
        Debug.Log($"=== INSTANT DEATH LLAMADO PARA {playerName} ===");

        if (isInvincible || isEliminated)
        {
            Debug.Log($"{playerName} es invencible o ya eliminado, ignorando muerte instantánea");
            return;
        }

        currentLives--;
        Debug.Log($"{playerName} perdió una vida instantáneamente. Vidas restantes: {currentLives}");

        OnLifeLost?.Invoke(currentLives);

        if (currentLives > 0)
        {
            Debug.Log($"{playerName} todavía tiene vidas, reapareciendo inmediatamente");
            Respawn();
        }
        else
        {
            Debug.Log($"=== {playerName} ELIMINADO POR MUERTE INSTANTÁNEA ===");
            isEliminated = true;

          
            Debug.Log("Invocando OnPlayerEliminated...");
            OnPlayerEliminated?.Invoke();
            Debug.Log("OnPlayerEliminated invocado");


            StartCoroutine(DeactivateAfterDelay());
        }
    }
    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); 
        Debug.Log($"Desactivando GameObject de {playerName}");
        gameObject.SetActive(false);
        Debug.Log($"Estado final: Activo={gameObject.activeSelf}");
    }
  
    [ContextMenu("Test Eliminación")]
    public void TestElimination()
    {
        Debug.Log("=== TEST ELIMINACIÓN ===");
        InstantDeath();
    }

    [ContextMenu("Debug Estado")]
    public void DebugState()
    {
        Debug.Log($"=== DEBUG {playerName} ===");
        Debug.Log($"Índice: {playerIndex}");
        Debug.Log($"Vidas: {currentLives}");
        Debug.Log($"Invencible: {isInvincible}");
        Debug.Log($"Activo en jerarquía: {gameObject.activeInHierarchy}");
        Debug.Log($"Posición: {transform.position}");
        Debug.Log($"Spawn asignado: {assignedSpawnPosition}");
    }
}