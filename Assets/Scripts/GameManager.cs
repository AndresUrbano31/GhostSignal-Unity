using UnityEngine;
using System.Collections;

/// <summary>
/// Gestor central del juego. Singleton con DontDestroyOnLoad.
/// Gestiona estados de juego, resonancia emocional, progresión de niveles.
/// Requiere: Singleton pattern, se adjunta a GameObject persistente
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Prologue,
        Level1_Amber,
        Level2_Broken,
        Level3_Oblivion,
        Level4_Forest,
        Level5_Core,
        GuardianScene,
        Ending_Good,
        Ending_Neutral,
        Ending_Poor,
        GameOver
    }

    [Header("Estado del Juego")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    [Header("Resonancia Emocional")]
    [SerializeField] private float emotionalResonance = 50f;
    [SerializeField] private float maxResonance = 100f;
    [SerializeField] private float minResonance = 0f;

    [Header("Niveles")]
    [SerializeField] private int currentLevel = 1;

    [Header("Configuración")]
    [SerializeField] private float level1Distance = 500f;
    [SerializeField] private float level2Distance = 800f;
    [SerializeField] private float level3Distance = 1000f;
    [SerializeField] private float level4Distance = 1200f;
    [SerializeField] private float level5Distance = 1500f;

    private PlayerController playerController;
    private TrackGenerator trackGenerator;
    private bool guardianEncounterActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        emotionalResonance = 50f;
        ChangeState(GameState.Prologue);
    }

    private void Update()
    {
        if (currentState >= GameState.Level1_Amber && currentState <= GameState.Level5_Core)
        {
            CheckLevelCompletion();
        }
    }

    /// <summary>
    /// Cambia el estado del juego y carga la escena correspondiente o executa lógica de estado.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Debug.Log($"[GameManager] Estado cambiado a: {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                UIManager.Instance?.ShowMainMenu();
                break;

            case GameState.Prologue:
                // Mostrar prólogo (cinemática con Dra. Chen)
                UIManager.Instance?.ShowPrologue();
                break;

            case GameState.Level1_Amber:
                LoadLevel(1, "Las Calles de Ámbar");
                break;

            case GameState.Level2_Broken:
                LoadLevel(2, "Galería de los Recuerdos Rotos");
                break;

            case GameState.Level3_Oblivion:
                LoadLevel(3, "Arquitectura del Olvido");
                break;

            case GameState.Level4_Forest:
                LoadLevel(4, "El Bosque de los Bifurcados");
                break;

            case GameState.Level5_Core:
                LoadLevel(5, "El Corazón de ARIA");
                break;

            case GameState.GuardianScene:
                // Pausar runner, mostrar diálogo de guardián
                Time.timeScale = 0f;
                break;

            case GameState.Ending_Good:
            case GameState.Ending_Neutral:
            case GameState.Ending_Poor:
                ShowEnding(newState);
                break;

            case GameState.GameOver:
                UIManager.Instance?.ShowGameOver();
                Time.timeScale = 0f;
                break;
        }
    }

    private void LoadLevel(int levelNumber, string levelName)
    {
        currentLevel = levelNumber;
        Time.timeScale = 1f;

        // Encontrar o crear generador de pistas
        trackGenerator = FindObjectOfType<TrackGenerator>();
        if (trackGenerator == null)
        {
            GameObject trackGenObject = new GameObject("TrackGenerator");
            trackGenerator = trackGenObject.AddComponent<TrackGenerator>();
        }

        // Encontrar controller del jugador
        playerController = FindObjectOfType<PlayerController>();

        UIManager.Instance?.ShowHUD(levelNumber);
        AudioManager.Instance?.PlayLevelMusic(levelNumber);

        Debug.Log($"[GameManager] Nivel cargado: {levelName}");
    }

    private void CheckLevelCompletion()
    {
        if (playerController == null) return;

        float targetDistance = GetTargetDistanceForLevel(currentLevel);

        if (playerController.GetDistanceTraveled() >= targetDistance)
        {
            CompleteLevel();
        }
    }

    private float GetTargetDistanceForLevel(int level)
    {
        return level switch
        {
            1 => level1Distance,
            2 => level2Distance,
            3 => level3Distance,
            4 => level4Distance,
            5 => level5Distance,
            _ => level1Distance
        };
    }

    private void CompleteLevel()
    {
        if (currentLevel < 5)
        {
            // Mostrar diálogo de guardián
            TriggerGuardianEncounter(currentLevel);
        }
        else
        {
            // Nivel 5 - último nivel, resolver final
            ResolveFinal();
        }
    }

    /// <summary>
    /// Activa el encuentro con el guardián del nivel actual.
    /// </summary>
    public void TriggerGuardianEncounter(int levelNumber)
    {
        if (guardianEncounterActive) return;

        guardianEncounterActive = true;
        Time.timeScale = 0f;

        GuardianBase guardian = FindObjectOfType<GuardianBase>();
        if (guardian != null)
        {
            guardian.Activate();
        }

        ChangeState(GameState.GuardianScene);
    }

    /// <summary>
    /// Resuelve el encuentro con el guardián y continúa al siguiente nivel.
    /// </summary>
    public void ResolveGuardianEncounter(int nextLevel)
    {
        guardianEncounterActive = false;
        Time.timeScale = 1f;

        if (nextLevel <= 5)
        {
            ChangeState((GameState)(GameState.Level1_Amber + nextLevel - 1));
        }
        else
        {
            ResolveFinal();
        }
    }

    /// <summary>
    /// Determina el final según la resonancia emocional.
    /// </summary>
    private void ResolveFinal()
    {
        Time.timeScale = 0f;

        if (emotionalResonance >= 70f)
        {
            ChangeState(GameState.Ending_Good);
        }
        else if (emotionalResonance >= 35f)
        {
            ChangeState(GameState.Ending_Neutral);
        }
        else
        {
            ChangeState(GameState.Ending_Poor);
        }
    }

    private void ShowEnding(GameState endingState)
    {
        UIManager.Instance?.ShowEndScreen(endingState);
        AudioManager.Instance?.PlayEndingMusic(endingState == GameState.Ending_Good);
    }

    /// <summary>
    /// Modifica la resonancia emocional por un delta.
    /// </summary>
    public void ModifyResonance(float delta)
    {
        emotionalResonance = Mathf.Clamp(emotionalResonance + delta, minResonance, maxResonance);
        UIManager.Instance?.UpdateResonanceMeter(emotionalResonance / maxResonance);

        Debug.Log($"[GameManager] Resonancia: {emotionalResonance}");
    }

    /// <summary>
    /// Dispara Game Over cuando el jugador falla en el Quantum Fall.
    /// </summary>
    public void TriggerGameOver()
    {
        ChangeState(GameState.GameOver);
    }

    /// <summary>
    /// Muestra el prompt del Quantum Fall durante 3 segundos.
    /// </summary>
    public void ShowQuantumFallPrompt(bool show)
    {
        UIManager.Instance?.ShowQuantumFallPrompt(show);
    }

    // Getters
    public float GetEmotionalResonance() => emotionalResonance;
    public GameState GetCurrentState() => currentState;
    public int GetCurrentLevel() => currentLevel;
}
