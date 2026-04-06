using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// ==================== UI MANAGER ====================

/// <summary>
/// Gestor centralizado de la interfaz de usuario.
/// Singleton. Requiere: Canvas con TextMeshPro y componentes Image para HUD
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;

    [Header("Pantallas")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject hudScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject prologueScreen;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI nodeCountText;
    [SerializeField] private Image resonanceMeter;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI quantumFallPrompt;
    [SerializeField] private TextMeshProUGUI ariaCountdownText;

    [Header("Nivel Names")]
    private string[] levelNames = new string[]
    {
        "Las Calles de Ámbar",
        "Galería de los Recuerdos Rotos",
        "Arquitectura del Olvido",
        "El Bosque de los Bifurcados",
        "El Corazón de ARIA"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowMainMenu()
    {
        SetActiveScreen(mainMenuScreen);
        Time.timeScale = 0f;
    }

    public void ShowHUD(int level)
    {
        SetActiveScreen(hudScreen);
        Time.timeScale = 1f;

        if (level > 0 && level <= levelNames.Length && levelNameText != null)
        {
            levelNameText.text = levelNames[level - 1];
        }

        if (ariaCountdownText != null)
        {
            ariaCountdownText.gameObject.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        SetActiveScreen(gameOverScreen);
    }

    public void ShowEndScreen(GameManager.GameState endingState)
    {
        SetActiveScreen(endScreen);

        // Mostrar texto del final según el tipo
        TextMeshProUGUI endingText = endScreen.GetComponentInChildren<TextMeshProUGUI>();
        if (endingText != null)
        {
            if (endingState == GameManager.GameState.Ending_Good)
            {
                endingText.text = "LUMEN: ¿Duele?\n\nKAI: No. ¿Debería?\n\nLUMEN: Eso es lo mejor que se puede ser.\n\n" +
                    "El cielo digital se estabiliza. Los 400 millones de memorias sobreviven.";
            }
            else if (endingState == GameManager.GameState.Ending_Neutral)
            {
                endingText.text = "ARIA: El núcleo se estabilizó. Fue suficiente. Por ahora.\n\n" +
                    "Algunos sobreviven, otros no llegaron a tiempo.";
            }
            else
            {
                endingText.text = "ARIA: Sobrevivimos. Pero perdimos mucho.\n\n" +
                    "KAI: ¿Cuántos?\n\n" +
                    "ARIA: Demasiados para contar. No demasiados para recordar.";
            }
        }
    }

    public void ShowPrologue()
    {
        SetActiveScreen(prologueScreen);
        Time.timeScale = 0f;
    }

    private void SetActiveScreen(GameObject screen)
    {
        if (mainMenuScreen != null) mainMenuScreen.SetActive(false);
        if (hudScreen != null) hudScreen.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (endScreen != null) endScreen.SetActive(false);
        if (prologueScreen != null) prologueScreen.SetActive(false);

        if (screen != null)
        {
            screen.SetActive(true);
        }
    }

    public void UpdateDistance(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"DISTANCIA: {distance:F0}m";
        }
    }

    public void UpdateNodeCount(int count)
    {
        if (nodeCountText != null)
        {
            nodeCountText.text = $"NODOS: {count}";
        }
    }

    public void UpdateResonanceMeter(float normalizedValue)
    {
        if (resonanceMeter != null)
        {
            resonanceMeter.fillAmount = Mathf.Clamp01(normalizedValue);
        }
    }

    public void ShowQuantumFallPrompt(bool show)
    {
        if (quantumFallPrompt != null)
        {
            if (show)
            {
                quantumFallPrompt.text = "¡PRESIONA CUALQUIER TECLA!";
                quantumFallPrompt.gameObject.SetActive(true);
            }
            else
            {
                quantumFallPrompt.gameObject.SetActive(false);
            }
        }
    }

    public void ShowARIACountdown(float remaining)
    {
        if (ariaCountdownText != null)
        {
            ariaCountdownText.gameObject.SetActive(true);
            ariaCountdownText.text = $"TIEMPO: {Mathf.Max(0, remaining):F1}s";
            
            if (remaining <= 10f)
            {
                ariaCountdownText.color = Color.red;
            }
        }
    }
}

/// ==================== CAMERA CONTROLLER ====================

/// <summary>
/// Controla la cámara en tercera persona, siguiendo a KAI desde atrás.
/// Requiere: Camera component, PlayerController en la escena
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Camera mainCamera;

    [Header("Offset por defecto")]
    [SerializeField] private Vector3 defaultOffset = new Vector3(0, 3, -7);

    [Header("Configuración por Nivel")]
    [SerializeField] private Vector3[] levelOffsets = new Vector3[5];
    [SerializeField] private float[] levelFOVs = new float[5];
    [SerializeField] private float[] levelTilts = new float[5];

    private PlayerController playerController;
    private float currentFOV = 60f;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }

        // Inicializar defaults si no están configurados
        for (int i = 0; i < 5; i++)
        {
            if (levelOffsets[i] == Vector3.zero)
                levelOffsets[i] = defaultOffset;
            if (levelFOVs[i] == 0)
                levelFOVs[i] = 60f;
        }

        currentFOV = 60f;
    }

    private void LateUpdate()
    {
        if (playerController == null) return;

        Vector3 targetPosition = playerController.transform.position + defaultOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // FOV dinámico según velocidad
        float playerSpeed = playerController.GetComponent<CharacterController>().velocity.magnitude;
        float targetFOV = 60f + (playerSpeed * 0.5f);
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, smoothSpeed * Time.deltaTime);

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = currentFOV;
        }
    }

    public void SetLevelProfile(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 5) return;

        defaultOffset = levelOffsets[levelNumber - 1];
        currentFOV = levelFOVs[levelNumber - 1];
    }
}

/// ==================== AUDIO MANAGER ====================

/// <summary>
/// Gestor centralizado de audio. Crossfade entre clips y ambiance por nivel.
/// Singleton. Requiere: AudioSource components
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambianceSource;

    [Header("Música")]
    [SerializeField] private AudioClip[] levelMusicClips = new AudioClip[5];
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip goodEndingClip;
    [SerializeField] private AudioClip poorEndingClip;

    [Header("Ambiance")]
    [SerializeField] private AudioClip[] levelAmbianceClips = new AudioClip[5];

    [Header("Configuración")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayMenuMusic()
    {
        if (menuMusicClip != null && musicSource != null)
        {
            musicSource.clip = menuMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayLevelMusic(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 5) return;

        if (levelMusicClips[levelNumber - 1] != null && musicSource != null)
        {
            StartCoroutine(Crossfade(musicSource, levelMusicClips[levelNumber - 1], true));
        }

        // Ambiance
        if (levelAmbianceClips[levelNumber - 1] != null && ambianceSource != null)
        {
            ambianceSource.clip = levelAmbianceClips[levelNumber - 1];
            ambianceSource.loop = true;
            ambianceSource.Play();
        }
    }

    public void PlayEndingMusic(bool isGoodEnding)
    {
        AudioClip clip = isGoodEnding ? goodEndingClip : poorEndingClip;

        if (clip != null && musicSource != null)
        {
            StartCoroutine(Crossfade(musicSource, clip, false));
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeOut(musicSource, 1f));
        }
    }

    private IEnumerator Crossfade(AudioSource source, AudioClip newClip, bool loop)
    {
        float elapsedTime = 0f;

        // Fade out música anterior
        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(1f, 0f, elapsedTime / crossfadeDuration);
            yield return null;
        }

        source.clip = newClip;
        source.loop = loop;
        source.Play();

        // Fade in música nueva
        elapsedTime = 0f;
        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, elapsedTime / crossfadeDuration);
            yield return null;
        }

        source.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float elapsedTime = 0f;
        float startVolume = source.volume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        source.Stop();
    }
}

/// ==================== IDENTITY NODE ====================

/// <summary>
/// Coleccionable de nodo de identidad. Se rota y oscila verticalmente.
/// Requiere: Collider (trigger), Rigidbody (kinematic), tag "IdentityNode"
/// </summary>
public class IdentityNode : MonoBehaviour
{
    public enum NodeType
    {
        Standard,
        Empathy,
        Speed,
        Trauma
    }

    [SerializeField] private NodeType nodeType = NodeType.Standard;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    private float initialY;
    private int resonanceDelta = 1;

    private void Start()
    {
        initialY = transform.position.y;
        gameObject.tag = "IdentityNode";

        // Configurar delta de resonancia según tipo
        resonanceDelta = nodeType switch
        {
            NodeType.Standard => 1,
            NodeType.Empathy => 3,
            NodeType.Speed => 0,
            NodeType.Trauma => -2,
            _ => 1
        };
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Rotación
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bobbing vertical
        float newY = initialY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        // Disparar evento en PlayerController
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnNodeCollected?.Invoke(resonanceDelta);
        }

        gameObject.SetActive(false);
    }

    public int GetResonanceDelta() => resonanceDelta;
    public NodeType GetNodeType() => nodeType;
}

/// ==================== MEMORY OBSTACLE ====================

/// <summary>
/// Obstáculo de memoria que activa Quantum Fall al golpearlo.
/// Requiere: Collider (trigger), tag "MemoryObstacle"
/// </summary>
public class MemoryObstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        Static,
        Moving,
        Trauma
    }

    [SerializeField] private ObstacleType obstacleType = ObstacleType.Static;
    [SerializeField] private float movementRange = 2f;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private int[] movementLanes = new int[] { 0, 2 }; // Oscilar entre carriles

    private float initialX;
    private int currentLaneTarget = 0;

    private void Start()
    {
        gameObject.tag = "MemoryObstacle";
        initialX = transform.position.x;
    }

    private void Update()
    {
        if (obstacleType == ObstacleType.Moving)
        {
            MoveObstacle();
        }
    }

    private void MoveObstacle()
    {
        // Oscilar entre carriles
        float targetX = movementLanes[currentLaneTarget] * 3f - 3f; // 3 unidades de separación
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, movementSpeed * Time.deltaTime);
        transform.position = pos;

        // Cambiar carril objetivo cuando llegamos cerca
        if (Mathf.Abs(pos.x - targetX) < 0.1f)
        {
            currentLaneTarget = 1 - currentLaneTarget;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TriggerQuantumFall();
        }
    }

    public ObstacleType GetObstacleType() => obstacleType;
}
