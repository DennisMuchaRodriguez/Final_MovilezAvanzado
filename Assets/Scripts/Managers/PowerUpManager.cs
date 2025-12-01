using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpConfig
    {
        public PowerUpType type;
        public GameObject prefab;
        public float spawnWeight = 1f; // Peso para spawn aleatorio
        public float minRespawnTime = 10f;
        public float maxRespawnTime = 20f;
    }

    public enum PowerUpType
    {
        MegaDash,
        Shield,
        Teleport,
        Shockwave
    }

    [Header("Configuración")]
    [SerializeField] private PowerUpConfig[] powerUpConfigs;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8, -4);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8, 4);
    [SerializeField] private LayerMask spawnCheckLayer;
    [SerializeField] private float spawnCheckRadius = 1f;

    [Header("Spawning")]
    [SerializeField] private int maxActivePowerUps = 3;
    [SerializeField] private float initialSpawnDelay = 5f;

    private List<GameObject> activePowerUps = new List<GameObject>();
    private Dictionary<PowerUpType, PowerUpConfig> configDictionary = new Dictionary<PowerUpType, PowerUpConfig>();

    private void Start()
    {
        // Inicializar diccionario
        foreach (var config in powerUpConfigs)
        {
            configDictionary[config.type] = config;
        }

        // Iniciar spawn de power-ups
        StartCoroutine(SpawnPowerUpsRoutine());
    }

    private IEnumerator SpawnPowerUpsRoutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            // Esperar si hay demasiados power-ups activos
            if (activePowerUps.Count >= maxActivePowerUps)
            {
                yield return new WaitForSeconds(5f);
                continue;
            }

            // Intentar spawnear un power-up
            if (TrySpawnPowerUp())
            {
                PowerUpType type = GetRandomPowerUpType();
                var config = configDictionary[type];

                // Esperar tiempo aleatorio antes del próximo spawn
                float waitTime = Random.Range(config.minRespawnTime, config.maxRespawnTime);
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                // Si no se pudo spawnear, esperar menos tiempo
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private bool TrySpawnPowerUp()
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();

        // Verificar si la posición es válida
        if (IsPositionValid(spawnPosition))
        {
            PowerUpType type = GetRandomPowerUpType();
            SpawnPowerUp(type, spawnPosition);
            return true;
        }

        return false;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector2(x, y);
    }

    private bool IsPositionValid(Vector2 position)
    {
        // Verificar que no haya colisiones
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, spawnCheckRadius, spawnCheckLayer);
        return colliders.Length == 0;
    }

    private PowerUpType GetRandomPowerUpType()
    {
        float totalWeight = 0f;
        foreach (var config in powerUpConfigs)
        {
            totalWeight += config.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var config in powerUpConfigs)
        {
            currentWeight += config.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return config.type;
            }
        }

        return powerUpConfigs[0].type; // Fallback
    }

    private void SpawnPowerUp(PowerUpType type, Vector2 position)
    {
        if (!configDictionary.ContainsKey(type))
        {
            Debug.LogError($"No hay configuración para power-up tipo: {type}");
            return;
        }

        var config = configDictionary[type];
        GameObject powerUp = Instantiate(config.prefab, position, Quaternion.identity);

        // Configurar el power-up
        BasePowerUp powerUpScript = powerUp.GetComponent<BasePowerUp>();
        if (powerUpScript != null)
        {
            powerUpScript.SetType(type);
            powerUpScript.OnCollected.AddListener(() => OnPowerUpCollected(powerUp));
        }

        activePowerUps.Add(powerUp);
        Debug.Log($"Power-up {type} spawnado en {position}");
    }

    private void OnPowerUpCollected(GameObject powerUp)
    {
        activePowerUps.Remove(powerUp);
        Destroy(powerUp);
    }

    [ContextMenu("Spawn Random PowerUp")]
    public void DebugSpawnRandomPowerUp()
    {
        Vector2 position = GetRandomSpawnPosition();
        PowerUpType type = GetRandomPowerUpType();
        SpawnPowerUp(type, position);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar área de spawn en el editor
        Gizmos.color = Color.green;
        Vector3 center = (spawnAreaMin + spawnAreaMax) / 2f;
        Vector3 size = spawnAreaMax - spawnAreaMin;
        Gizmos.DrawWireCube(center, size);
    }
}