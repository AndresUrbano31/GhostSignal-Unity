using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controla el movimiento de KAI en el juego: cambio de carriles, salto, deslizamiento y recolección de nodos.
/// Requiere: CharacterController component, Collider (trigger), AudioSource
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Carriles")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float laneSwitchSpeed = 10f;
    private int currentLane = 1; // 0: izquierda, 1: centro, 2: derecha
    private float targetLanePosition = 0f;

    [Header("Movimiento")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float maxSpeed = 28f;
    [SerializeField] private float accelerationRate = 0.05f;
    private float currentSpeed;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravity = -25f;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;

    [Header("Deslizamiento")]
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float slideColliderHeight = 0.5f;
    private bool isSliding = false;
    private CharacterController characterController;
    private float normalColliderHeight;

    [Header("Quantum Fall")]
    [SerializeField] private float quantumFallTimeScale = 0.3f;
    [SerializeField] private float quantumFallDuration = 3f;
    private bool isInQuantumFall = false;
    private float quantumFallTimer = 0f;

    [Header("Coleccionables")]
    public System.Action<int> OnNodeCollected; // eventista: delta de resonancia

    [Header("Distancia")]
    private float distanceTraveled = 0f;

    [Header("Circuitos")]
    [SerializeField] private Material circuitMaterial;
    [SerializeField] private Color circuitActiveColor = Color.cyan;
    private SkinnedMeshRenderer meshRenderer;
    private int nodesCollectedThisRun = 0;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        if (characterController != null)
        {
            normalColliderHeight = characterController.height;
        }

        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        // Entrada del jugador
        HandleLaneInput();
        HandleJumpInput();
        HandleSlideInput();

        // Movimiento
        MovePlayer();

        // Distancia
        distanceTraveled += currentSpeed * Time.deltaTime;

        // Quantum Fall
        if (isInQuantumFall)
        {
            UpdateQuantumFall();
        }

        // Aumentar velocidad gradualmente
        if (!isInQuantumFall && currentSpeed < maxSpeed)
        {
            currentSpeed += accelerationRate * Time.deltaTime;
        }
    }

    private void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
            {
                currentLane--;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < 2)
            {
                currentLane++;
            }
        }

        // Calcular posición de carril suavemente
        float desiredX = (currentLane - 1) * laneWidth;
        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, desiredX, laneSwitchSpeed * Time.deltaTime);
        
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;
    }

    private void HandleJumpInput()
    {
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void HandleSlideInput()
    {
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && isGrounded && !isSliding)
        {
            StartCoroutine(PerformSlide());
        }
    }

    private void MovePlayer()
    {
        Vector3 movement = Vector3.zero;

        // Movimiento hacia adelante
        movement.z = currentSpeed * Time.deltaTime;

        // Movimiento vertical (gravedad)
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -1f; // pequeña fuerza para mantener en el suelo
        }

        movement.y = verticalVelocity * Time.deltaTime;

        if (characterController != null)
        {
            characterController.Move(movement);
        }
        else
        {
            transform.position += movement;
        }

        // Verificar si está en el suelo
        if (characterController != null && characterController.isGrounded)
        {
            isGrounded = true;
            verticalVelocity = 0f;
        }
    }

    private IEnumerator PerformSlide()
    {
        isSliding = true;

        if (characterController != null)
        {
            characterController.height = slideColliderHeight;
        }

        yield return new WaitForSeconds(slideDuration);

        if (characterController != null)
        {
            characterController.height = normalColliderHeight;
        }

        isSliding = false;
    }

    /// <summary>
    /// Activa la caída cuántica cuando el jugador golpea un obstáculo.
    /// El jugador debe presionar cualquier tecla para recuperarse.
    /// </summary>
    public void TriggerQuantumFall()
    {
        if (isInQuantumFall) return;

        isInQuantumFall = true;
        quantumFallTimer = quantumFallDuration;
        Time.timeScale = quantumFallTimeScale;

        GameManager.Instance.ShowQuantumFallPrompt(true);
    }

    private void UpdateQuantumFall()
    {
        if (Input.anyKeyDown)
        {
            RecoverFromQuantumFall();
            return;
        }

        quantumFallTimer -= Time.deltaTime;

        if (quantumFallTimer <= 0)
        {
            // Fallo - muerte
            Time.timeScale = 1f;
            isInQuantumFall = false;
            GameManager.Instance.ShowQuantumFallPrompt(false);
            GameManager.Instance.TriggerGameOver();
        }
    }

    private void RecoverFromQuantumFall()
    {
        Time.timeScale = 1f;
        isInQuantumFall = false;
        GameManager.Instance.ShowQuantumFallPrompt(false);
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Nodo de identidad
        if (collision.CompareTag("IdentityNode"))
        {
            IdentityNode node = collision.GetComponent<IdentityNode>();
            if (node != null)
            {
                CollectNode(node);
            }
        }

        // Obstáculo de memoria
        if (collision.CompareTag("MemoryObstacle"))
        {
            TriggerQuantumFall();
        }
    }

    private void CollectNode(IdentityNode node)
    {
        nodesCollectedThisRun++;

        // Modificar brillo de circuitos
        if (meshRenderer != null && circuitMaterial != null)
        {
            float intensity = 0.5f + (nodesCollectedThisRun * 0.05f);
            meshRenderer.material.SetColor("_EmissionColor", circuitActiveColor * intensity);
        }

        // Disparar evento con delta de resonancia
        OnNodeCollected?.Invoke(node.GetResonanceDelta());

        // Bonus cada 10 nodos
        if (nodesCollectedThisRun % 10 == 0)
        {
            OnNodeCollected?.Invoke(2);
        }

        node.Collect();
    }

    public float GetDistanceTraveled() => distanceTraveled;
    public int GetNodesCollected() => nodesCollectedThisRun;
    public void ResetNodes() => nodesCollectedThisRun = 0;
    public int GetCurrentLane() => currentLane;
    public bool IsSliding() => isSliding;
    public bool IsInQuantumFall() => isInQuantumFall;
}
