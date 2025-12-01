using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Image winnerColorDisplay;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button teamSelectButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Names")]
    [SerializeField] private string teamSelectionScene = "TeamSelectionScene";
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Colors")]
    [SerializeField]
    private Color[] playerColors = {
        Color.red,    // P1
        Color.blue,   // P2
        Color.green,  // P3
        Color.yellow  // P4
    };

    private void Start()
    {
        // Ocultar panel al inicio
        victoryPanel.SetActive(false);

        // Configurar botones
        restartButton.onClick.AddListener(RestartGame);
        teamSelectButton.onClick.AddListener(GoToTeamSelection);
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        // Buscar GameStateManager en la escena y suscribirse
        GameStateManager gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (gameStateManager != null)
        {
            gameStateManager.OnGameWon.AddListener(ShowVictoryScreen);
            gameStateManager.OnGameDraw.AddListener(ShowDrawScreen);
            Debug.Log("VictoryUI suscrito a GameStateManager");
        }
        else
        {
            Debug.LogWarning("No se encontró GameStateManager en la escena");
        }
    }

    // Estos métodos ahora son públicos para que GameStateManager pueda llamarlos
    public void ShowVictoryScreen(PlayerLifeManager winner)
    {
        victoryPanel.SetActive(true);

        // Mostrar información del ganador
        string winnerName = winner.GetPlayerName();
        int winnerIndex = winner.GetPlayerIndex();

        winnerText.text = $"¡{winnerName} WINS!";
        winnerText.color = GetPlayerColor(winnerIndex);

        // Mostrar color del ganador
        if (winnerColorDisplay != null)
        {
            winnerColorDisplay.color = GetPlayerColor(winnerIndex);
        }

        // Pausar el juego (opcional)
        Time.timeScale = 0f;

        Debug.Log($"Mostrando pantalla de victoria para {winnerName}");
    }

    public void ShowDrawScreen()
    {
        victoryPanel.SetActive(true);
        winnerText.text = "¡DRAW GAME!";
        winnerText.color = Color.white;

        if (winnerColorDisplay != null)
        {
            winnerColorDisplay.color = Color.gray;
        }

        Time.timeScale = 0f;
    }

    private Color GetPlayerColor(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerColors.Length)
        {
            return playerColors[playerIndex];
        }
        return Color.white;
    }

    private void RestartGame()
    {
        // Reanudar tiempo
        Time.timeScale = 1f;

        // Recargar escena actual
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void GoToTeamSelection()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(teamSelectionScene);
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    // Para debug
    [ContextMenu("Test Victory P1")]
    public void TestVictoryP1()
    {
        var testPlayer = FindFirstObjectByType<PlayerLifeManager>();
        if (testPlayer != null)
        {
            ShowVictoryScreen(testPlayer);
        }
    }

    [ContextMenu("Test Draw")]
    public void TestDraw()
    {
        ShowDrawScreen();
    }

    private void OnDestroy()
    {
        // Asegurarse de reanudar el tiempo
        Time.timeScale = 1f;
    }
}