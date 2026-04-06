using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generador procedural de segmentos de pista con object pooling.
/// Genera obstáculos, coleccionables y elementos especiales según probabilidades configurables.
/// Requiere: GameManager, PlayerController, prefabs de obstáculos y nodos
/// </summary>
public class TrackGenerator : MonoBehaviour
{
    [System.Serializable]
    public class LevelConfig
    {
        public Material trackMaterial;
        public Color primaryColor;
        public Color accentColor;
        public float baseSpeed;
    }

    [Header("Segmentos")]
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float despawnDistance = 30f;

    [Header("Probabilidades de Generación")]
    [SerializeField] private float obstacleChance = 0.4f; // 40%
    [SerializeField] private float collectibleChance = 0.3f; // 30%
    [SerializeField] private float specialChance = 0.05f; // 5%

    [Header("Prefabs")]
    [SerializeField] private GameObject obstacleStaticPrefab;
    [SerializeField] private GameObject obstacleMovingPrefab;
    [SerializeField] private GameObject collectibleStandardPrefab;
    [SerializeField] private GameObject collectibleEmpathyPrefab;
    [SerializeField] private GameObject collectibleSpeedPrefab;
    [SerializeField] private GameObject collectibleTraumaPrefab;

    [Header("Configuración por Nivel")]
    [SerializeField] private LevelConfig[] levelConfigs = new LevelConfig[5];

    // Object pooling
    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
    private float nextSegmentZ = 0f;
    private PlayerController playerController;
    private int currentLevel = 1;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        currentLevel = GameManager.Instance?.GetCurrentLevel() ?? 1;

        // Inicializar pools
        InitializePools();

        // Generar primeros segmentos
        for (int i = 0; i < 5; i++)
        {
            GenerateSegment();
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        float playerZ = playerController.transform.position.z;

        // Generar nuevos segmentos
        while (nextSegmentZ < playerZ + 50f)
        {
            GenerateSegment();
        }

        // Desactivar segmentos lejanos
        DespawnFarSegments(playerZ);
    }

    private void InitializePools()
    {
        // Crear pools para cada tipo de objeto
        objectPools["ObstacleStatic"] = new Queue<GameObject>();
        objectPools["ObstacleMoving"] = new Queue<GameObject>();
        objectPools["CollectibleStandard"] = new Queue<GameObject>();
        objectPools["CollectibleEmpathy"] = new Queue<GameObject>();
        objectPools["CollectibleSpeed"] = new Queue<GameObject>();
        objectPools["CollectibleTrauma"] = new Queue<GameObject>();

        // Pre-instanciar algunos objetos
        PrePoolObjects("ObstacleStatic", obstacleStaticPrefab, 10);
        PrePoolObjects("ObstacleMoving", obstacleMovingPrefab, 10);
        PrePoolObjects("CollectibleStandard", collectibleStandardPrefab, 20);
        PrePoolObjects("CollectibleEmpathy", collectibleEmpathyPrefab, 10);
    }

    private void PrePoolObjects(string poolKey, GameObject prefab, int count)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPools[poolKey].Enqueue(obj);
        }
    }

    private void GenerateSegment()
    {
        float segmentZ = nextSegmentZ;

        // Decisión de qué generar
        float randomValue = Random.value;

        if (randomValue < obstacleChance)
        {
            GenerateObstacle(segmentZ);
        }
        else if (randomValue < obstacleChance + collectibleChance)
        {
            GenerateCollectible(segmentZ);
        }
        else if (randomValue < obstacleChance + collectibleChance + specialChance)
        {
            GenerateSpecialElement(segmentZ);
        }

        nextSegmentZ += segmentLength;
    }

    private void GenerateObstacle(float segmentZ)
    {
        int randomLane = Random.Range(0, 3);
        float laneX = (randomLane - 1) * laneWidth;

        if (Random.value > 0.5f)
        {
            // Obstáculo estático
            SpawnObject("ObstacleStatic", laneX, 0.5f, segmentZ);
        }
        else
        {
            // Obstáculo móvil
            SpawnObject("ObstacleMoving", laneX, 0.5f, segmentZ);
        }
    }

    private void GenerateCollectible(float segmentZ)
    {
        int randomLane = Random.Range(0, 3);
        float laneX = (randomLane - 1) * laneWidth;

        float collectibleRoll = Random.value;

        if (collectibleRoll < 0.5f)
        {
            SpawnObject("CollectibleStandard", laneX, 0.3f, segmentZ);
        }
        else if (collectibleRoll < 0.75f)
        {
            SpawnObject("CollectibleEmpathy", laneX, 0.3f, segmentZ);
        }
        else if (collectibleRoll < 0.9f)
        {
            SpawnObject("CollectibleSpeed", laneX, 0.3f, segmentZ);
        }
        else
        {
            SpawnObject("CollectibleTrauma", laneX, 0.3f, segmentZ);
        }
    }

    private void GenerateSpecialElement(float segmentZ)
    {
        // Elementos especiales según el nivel
        // Por ahora solo generamos coleccionables especiales
        GenerateCollectible(segmentZ);
    }

    private void SpawnObject(string poolKey, float x, float y, float z)
    {
        if (!objectPools.ContainsKey(poolKey))
            return;

        GameObject obj = null;

        if (objectPools[poolKey].Count > 0)
        {
            obj = objectPools[poolKey].Dequeue();
        }
        else
        {
            // Crear nuevo si el pool está vacío
            Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>
            {
                { "ObstacleStatic", obstacleStaticPrefab },
                { "ObstacleMoving", obstacleMovingPrefab },
                { "CollectibleStandard", collectibleStandardPrefab },
                { "CollectibleEmpathy", collectibleEmpathyPrefab },
                { "CollectibleSpeed", collectibleSpeedPrefab },
                { "CollectibleTrauma", collectibleTraumaPrefab }
            };

            if (prefabMap.ContainsKey(poolKey) && prefabMap[poolKey] != null)
            {
                obj = Instantiate(prefabMap[poolKey], transform);
            }
        }

        if (obj != null)
        {
            obj.transform.position = new Vector3(x, y, z);
            obj.SetActive(true);
        }
    }

    private void DespawnFarSegments(float playerZ)
    {
        // Desactivar objetos que están muy atrás
        foreach (Transform child in transform)
        {
            if (child.position.z < playerZ - despawnDistance)
            {
                string poolKey = GetPoolKeyFromObject(child.gameObject);
                if (poolKey != null && objectPools.ContainsKey(poolKey))
                {
                    child.gameObject.SetActive(false);
                    objectPools[poolKey].Enqueue(child.gameObject);
                }
            }
        }
    }

    private string GetPoolKeyFromObject(GameObject obj)
    {
        // Identificar el tipo de objeto por tag o nombre
        if (obj.CompareTag("MemoryObstacle"))
        {
            if (obj.name.Contains("Moving"))
                return "ObstacleMoving";
            return "ObstacleStatic";
        }

        if (obj.CompareTag("IdentityNode"))
        {
            if (obj.name.Contains("Empathy"))
                return "CollectibleEmpathy";
            if (obj.name.Contains("Speed"))
                return "CollectibleSpeed";
            if (obj.name.Contains("Trauma"))
                return "CollectibleTrauma";
            return "CollectibleStandard";
        }

        return null;
    }

    /// <summary>
    /// Cambia la configuración visual del nivel actual.
    /// </summary>
    public void SetLevelTheme(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 5) return;

        currentLevel = levelNumber;
        LevelConfig config = levelConfigs[levelNumber - 1];

        // Aplicar material y colores a segmentos activos
        foreach (Transform child in transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null && config.trackMaterial != null)
            {
                renderer.material = config.trackMaterial;
                renderer.material.color = config.primaryColor;
            }
        }
    }

    public int GetCurrentLevel() => currentLevel;
}
