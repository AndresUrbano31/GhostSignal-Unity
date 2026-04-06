using UnityEngine;
using System.Collections;

/// <summary>
/// Clase abstracta base para todos los guardianes.
/// Requiere: Animator, Collider (trigger), ParticleSystem (opcional)
/// </summary>
public abstract class GuardianBase : MonoBehaviour
{
    [Header("Base Guardian")]
    [SerializeField] protected float activationDistance = 5f;
    [SerializeField] protected float floatHeight = 0.5f;
    [SerializeField] protected float floatSpeed = 2f;
    protected float initialY;
    protected bool isActive = false;

    protected virtual void Start()
    {
        initialY = transform.position.y;
    }

    protected virtual void Update()
    {
        if (isActive)
        {
            // Efecto de flotación
            float floatY = initialY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            Vector3 pos = transform.position;
            pos.y = floatY;
            transform.position = pos;
        }
    }

    /// <summary>
    /// Activa al guardián (aparición y diálogo).
    /// </summary>
    public virtual void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Desactiva al guardián (disolución).
    /// </summary>
    public virtual void Deactivate()
    {
        isActive = false;
        StartCoroutine(DissolveOut());
    }

    /// <summary>
    /// Resuelve el encuentro con este guardián.
    /// Llamado por DialogueSystem después de que el jugador elige una opción.
    /// </summary>
    public abstract void ResolveEncounter(int optionIndex);

    protected IEnumerator DissolveOut()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            float elapsedTime = 0f;
            float dissolveDuration = 0.5f;
            Color originalColor = renderer.material.color;

            while (elapsedTime < dissolveDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(1f, 0f, elapsedTime / dissolveDuration);
                renderer.material.color = newColor;
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }
}

/// ==================== GUARDIÁN LUMEN (NIVEL 1) ====================

public class GuardianLUMEN : GuardianBase
{
    [Header("LUMEN Específico")]
    [SerializeField] private ParticleSystem fireflyParticles;
    [SerializeField] private int nodesTransferred = 20;

    private DialogueSystem dialogueSystem;

    protected override void Start()
    {
        base.Start();
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    public override void Activate()
    {
        base.Activate();

        // Mostrar diálogo
        if (dialogueSystem != null)
        {
            DialogueSystem.DialogueLine dialogue = DialogueSystem.GetLevel1Dialogue();
            dialogueSystem.ShowDialogue(dialogue, (optionIndex) => ResolveEncounter(optionIndex));
        }

        // Iniciar partículas de luciérnagas
        if (fireflyParticles != null)
        {
            fireflyParticles.Play();
        }
    }

    public override void ResolveEncounter(int optionIndex)
    {
        // LUMEN siempre transfiere nodos, solo cambia el tono del diálogo
        Debug.Log($"[LUMEN] Opción seleccionada: {optionIndex}. Transfiriendo {nodesTransferred} nodos.");

        // Darle al jugador los nodos de identidad
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.TryGetComponent(out IdentityNode identityNode))
        {
            for (int i = 0; i < nodesTransferred; i++)
            {
                // Los nodos ya fueron modificados por la resonancia del diálogo
                // Simplemente tranferimos el package
            }
        }

        // Disolverse y continuar al siguiente nivel
        Deactivate();
        Invoke(nameof(ContinueToNextLevel), 1f);
    }

    private void ContinueToNextLevel()
    {
        GameManager.Instance?.ResolveGuardianEncounter(2);
    }
}

/// ==================== GUARDIÁN VOID (NIVEL 2) ====================

public class GuardianVOID : GuardianBase
{
    [Header("VOID Específico")]
    [SerializeField] private float chaseSpeed = 8f;
    [SerializeField] private float neutralizationTime = 15f;
    private float neutralizationTimer = 0f;
    private bool isNeutralized = false;

    private PlayerController playerController;
    private DialogueSystem dialogueSystem;

    protected override void Start()
    {
        base.Start();
        playerController = FindObjectOfType<PlayerController>();
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isActive) return;

        // Perseguir al jugador
        if (playerController != null && !isNeutralized)
        {
            Vector3 directionToPlayer = (playerController.transform.position - transform.position).normalized;
            transform.position += directionToPlayer * chaseSpeed * Time.unscaledDeltaTime;

            // Contar tiempo para neutralización
            neutralizationTimer += Time.unscaledDeltaTime;

            if (neutralizationTimer >= neutralizationTime && !isNeutralized)
            {
                // No se neutralizó a tiempo
                playerController.TriggerQuantumFall();
                Deactivate();
            }
        }
    }

    public override void Activate()
    {
        base.Activate();

        neutralizationTimer = 0f;
        isNeutralized = false;

        // Mostrar diálogo
        if (dialogueSystem != null)
        {
            DialogueSystem.DialogueLine dialogue = DialogueSystem.GetLevel2Dialogue();
            dialogueSystem.ShowDialogue(dialogue, (optionIndex) => ResolveEncounter(optionIndex));
        }
    }

    public override void ResolveEncounter(int optionIndex)
    {
        // Opción 0 (Dejar emerger el recuerdo reprimido) neutraliza a VOID
        if (optionIndex == 0)
        {
            Neutralize();
        }
        else
        {
            // Opción 1 - Falla
            if (playerController != null)
            {
                playerController.TriggerQuantumFall();
            }
        }
    }

    /// <summary>
    /// Neutraliza a VOID cuando el jugador elige la opción correcta.
    /// </summary>
    public void Neutralize()
    {
        isNeutralized = true;
        Debug.Log("[VOID] Ha sido neutralizado.");

        // Efecto de desaparición
        Deactivate();
        Invoke(nameof(ContinueToNextLevel), 1f);
    }

    private void ContinueToNextLevel()
    {
        GameManager.Instance?.ResolveGuardianEncounter(3);
    }
}

/// ==================== GUARDIÁN ECHO (NIVEL 3) ====================

public class GuardianECHO : GuardianBase
{
    [Header("ECHO Específico")]
    [SerializeField] private float runDuration = 5f;
    [SerializeField] private int hologramCount = 3;
    [SerializeField] private float hologramTransparency = 0.6f;

    private DialogueSystem dialogueSystem;
    private PlayerController playerController;
    private float runTimer = 0f;
    private bool encounteredResolved = false;

    protected override void Start()
    {
        base.Start();
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        playerController = FindObjectOfType<PlayerController>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isActive || encounteredResolved) return;

        // Correr junto al jugador
        if (playerController != null)
        {
            Vector3 playerPos = playerController.transform.position;
            Vector3 echoPos = transform.position;

            // Mantener distancia lateral (carril adyacente)
            echoPos.x = playerPos.x + 3f;
            echoPos.z = playerPos.z;

            transform.position = echoPos;

            // Contar tiempo
            runTimer += Time.unscaledDeltaTime;

            if (runTimer >= runDuration)
            {
                ResolveEncounter(0); // Opción correcta: seguir corriendo
            }
        }
    }

    public override void Activate()
    {
        base.Activate();
        runTimer = 0f;
        encounteredResolved = false;

        // Mostrar diálogo
        if (dialogueSystem != null)
        {
            DialogueSystem.DialogueLine dialogue = DialogueSystem.GetLevel3Dialogue();
            dialogueSystem.ShowDialogue(dialogue, (optionIndex) => ResolveEncounter(optionIndex));
        }

        // Crear hologramas
        CreateHolograms();
    }

    private void CreateHolograms()
    {
        for (int i = 0; i < hologramCount; i++)
        {
            GameObject hologram = Instantiate(gameObject, transform.parent);
            Renderer renderer = hologram.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = hologramTransparency;
                renderer.material.color = color;
            }

            hologram.GetComponent<GuardianECHO>().enabled = false;
            hologram.transform.localPosition = new Vector3(i * 0.5f, 0, -i * 2f);
        }
    }

    public override void ResolveEncounter(int optionIndex)
    {
        encounteredResolved = true;

        if (optionIndex == 0) // Seguiría corriendo
        {
            Debug.Log("[ECHO] El jugador continuó corriendo correctamente.");
        }
        else
        {
            // Otras opciones no neutralizan bien a ECHO pero el encuentro termina
            Debug.Log("[ECHO] El jugador dudó pero ECHO desaparece.");
        }

        Deactivate();
        Invoke(nameof(ContinueToNextLevel), 1f);
    }

    private void ContinueToNextLevel()
    {
        GameManager.Instance?.ResolveGuardianEncounter(4);
    }
}

/// ==================== GUARDIÁN ARIA (NIVEL 5) ====================

public class GuardianARIA : GuardianBase
{
    [Header("ARIA Específico")]
    [SerializeField] private float countdownDuration = 40f;
    [SerializeField] private int ringCount = 3;
    [SerializeField] private float ringRotationSpeed = 30f;

    private DialogueSystem dialogueSystem;
    private float countdownTimer = 0f;
    private bool anchorAccepted = false;

    protected override void Start()
    {
        base.Start();
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isActive || anchorAccepted) return;

        // Contar hacia atrás
        countdownTimer += Time.unscaledDeltaTime;
        UIManager.Instance?.ShowARIACountdown(countdownDuration - countdownTimer);

        if (countdownTimer >= countdownDuration)
        {
            // Tiempo agotado - final pobre
            GameManager.Instance?.ModifyResonance(-20f);
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Poor);
        }

        // Rotar anillos
        RotateRings();
    }

    public override void Activate()
    {
        base.Activate();
        countdownTimer = 0f;
        anchorAccepted = false;

        // Mostrar diálogo
        if (dialogueSystem != null)
        {
            DialogueSystem.DialogueLine dialogue = DialogueSystem.GetLevel5Dialogue();
            dialogueSystem.ShowDialogue(dialogue, (optionIndex) => ResolveEncounter(optionIndex));
        }

        // Crear anillos holográficos
        CreateRings();
    }

    private void CreateRings()
    {
        for (int i = 0; i < ringCount; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Torus);
            ring.transform.SetParent(transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = Vector3.one * (1f + i * 0.5f);
            ring.name = $"Ring_{i}";

            Renderer renderer = ring.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.7f, 0f, 0.6f); // Ámbar semi-transparente
            }

            Collider collider = ring.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private void RotateRings()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Ring_"))
            {
                child.RotateAround(transform.position, Vector3.up, ringRotationSpeed * Time.unscaledDeltaTime);
            }
        }
    }

    public override void ResolveEncounter(int optionIndex)
    {
        // Opción 0: Acepto. Une condición — ellos mantienen su autonomía (+20 resonancia, final bueno)
        // Opción 1: Necesito más tiempo (0 resonancia, neutral probablemente)
        // Opción 2: No. Buscaré otra solución (-20 resonancia, final pobre)

        anchorAccepted = true;
        Time.timeScale = 1f;

        if (optionIndex == 0)
        {
            Debug.Log("[ARIA] El jugador aceptó ser el ancla con condiciones.");
            // El final se determina por resonancia general
            Invoke(nameof(StabilizeCore), 1f);
        }
        else if (optionIndex == 1)
        {
            Debug.Log("[ARIA] El jugador necesita más tiempo.");
            // El jugador falla por falta de tiempo
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Neutral);
        }
        else
        {
            Debug.Log("[ARIA] El jugador rechazó ser ancla.");
            // Final deficiente
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Poor);
        }
    }

    private void StabilizeCore()
    {
        Deactivate();

        // Determinar final basado en resonancia general
        float resonance = GameManager.Instance?.GetEmotionalResonance() ?? 50f;

        if (resonance >= 70f)
        {
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Good);
        }
        else if (resonance >= 35f)
        {
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Neutral);
        }
        else
        {
            GameManager.Instance?.ChangeState(GameManager.GameState.Ending_Poor);
        }
    }
}
