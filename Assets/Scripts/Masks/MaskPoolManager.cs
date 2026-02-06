using System.Collections.Generic;
using UnityEngine;


//Sistema de pooling para máscaras. Maneja spawn, respawn y reciclaje.
public class MaskPoolManager : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private GameObject greenMaskPrefab;
    [SerializeField] private GameObject redMaskPrefab;
    [SerializeField] private GameObject blueMaskPrefab;
    [SerializeField] private int masksPerType = 1; //Cuántas máscaras de cada tipo en el mundo

    [Header("Spawn Configuration")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minDistanceFromPlayer = 10f;
    [SerializeField] private Transform playerTransform;

    //Pool de máscaras organizadas por tipo
    private Dictionary<MaskType, Queue<GameObject>> inactiveMasks = new Dictionary<MaskType, Queue<GameObject>>();
    private Dictionary<MaskType, HashSet<GameObject>> activeMasks = new Dictionary<MaskType, HashSet<GameObject>>();

    //Trackear spawn points ocupados
    private HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>();

    void Awake()
    {
        InitializePool();
    }

    void Start()
    {
        //Spawnear máscaras iniciales
        SpawnInitialMasks();
    }


    //Crea el pool de máscaras (sin instanciarlas en el mundo todavía)
    void InitializePool()
    {
        //Inicializar diccionarios
        foreach (MaskType type in System.Enum.GetValues(typeof(MaskType)))
        {
            inactiveMasks[type] = new Queue<GameObject>();
            activeMasks[type] = new HashSet<GameObject>();
        }

        //Crear máscaras en el pool (desactivadas)
        CreateMasksInPool(MaskType.GreenMask, greenMaskPrefab, masksPerType);
        CreateMasksInPool(MaskType.RedMask, redMaskPrefab, masksPerType);
        CreateMasksInPool(MaskType.BlueMask, blueMaskPrefab, masksPerType);
    }

  
    //Crea máscaras del tipo especificado y las agrega al pool inactivo
    void CreateMasksInPool(MaskType type, GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogError($"[Pool] Prefab para {type} no asignado!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject mask = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            mask.name = $"{type}_Mask_{i}";
            mask.SetActive(false);
            inactiveMasks[type].Enqueue(mask);
        }
    }


    //Spawnea las máscaras iniciales en posiciones random
    void SpawnInitialMasks()
    {
        foreach (MaskType type in System.Enum.GetValues(typeof(MaskType)))
        {
            int count = inactiveMasks[type].Count;
            for (int i = 0; i < count; i++)
            {
                SpawnMask(type);
            }
        }
    }

    //Spawnea una máscara del tipo especificado en una posición random
    public void SpawnMask(MaskType type)
    {
        if (inactiveMasks[type].Count == 0)
        {
            Debug.LogWarning($"[Pool] No hay máscaras {type} disponibles en el pool!");
            return;
        }

        GameObject mask = inactiveMasks[type].Dequeue();

        //Obtener spawn point libre
        Transform spawnPoint = GetFreeSpawnPoint();
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : GetRandomSpawnPosition();

        mask.transform.position = spawnPos;
        mask.transform.rotation = Quaternion.identity;
        mask.SetActive(true);

        activeMasks[type].Add(mask);

        //Marcar spawn point como ocupado si encontramos uno
        if (spawnPoint != null)
        {
            occupiedSpawnPoints.Add(spawnPoint);

            // Guardar referencia del spawn point en el mask para liberarlo después
            MaskSpawnTracker tracker = mask.GetComponent<MaskSpawnTracker>();
            if (tracker == null)
                tracker = mask.AddComponent<MaskSpawnTracker>();
            tracker.occupiedSpawnPoint = spawnPoint;
        }
    }


    //Recicla una máscara (la desactiva y la devuelve al pool)
    public void RecycleMask(GameObject mask)
    {
        Mask maskComponent = mask.GetComponent<Mask>();
        if (maskComponent == null)
        {
            return;
        }

        MaskType type = maskComponent.GetMaskType();

        //Liberar spawn point si estaba ocupado
        MaskSpawnTracker tracker = mask.GetComponent<MaskSpawnTracker>();
        if (tracker != null && tracker.occupiedSpawnPoint != null)
        {
            occupiedSpawnPoints.Remove(tracker.occupiedSpawnPoint);
            tracker.occupiedSpawnPoint = null;
        }

        //Remover de activas
        activeMasks[type].Remove(mask);

        //Desactivar
        mask.SetActive(false);

        //Devolver al pool
        inactiveMasks[type].Enqueue(mask);
    }


    //Encuentra un spawn point que no esté ocupado
    Transform GetFreeSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        int maxAttempts = 10;

        // Optimizamos la distancia al cuadrado 
        float minDistanceSqr = minDistanceFromPlayer * minDistanceFromPlayer;

        for (int i = 0; i < maxAttempts; i++)
        {
            //Elegir un punto al azar directamente del array original
            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];

            //¿Está ocupado?
            if (occupiedSpawnPoints.Contains(candidate))
                continue; // Está ocupado, probar otro

            //¿Está lejos del jugador?
            if (playerTransform != null)
            {
                //Usamos sqrMagnitude que es mucho más rápido que Distance
                float sqrDist = (candidate.position - playerTransform.position).sqrMagnitude;
                if (sqrDist < minDistanceSqr)
                    continue; //Está muy cerca, probar otro
            }

            //Encontramos uno válido
            return candidate;
        }

       
        Debug.LogWarning("[Pool] Fast path falló, usando fallback lento.");
        return GetFreeSpawnPointSlow();
    }

    //Este es un método para usarlo solo en emergencias
    Transform GetFreeSpawnPointSlow()
    {
        List<Transform> freeSpawnPoints = new List<Transform>();

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (!occupiedSpawnPoints.Contains(spawnPoint))
                freeSpawnPoints.Add(spawnPoint);
        }

        if (freeSpawnPoints.Count == 0) return null;

        // Lógica original de filtrado por distancia...
        if (playerTransform != null)
        {
            List<Transform> farEnoughPoints = new List<Transform>();
            foreach (Transform point in freeSpawnPoints)
            {
                if (Vector3.Distance(point.position, playerTransform.position) >= minDistanceFromPlayer)
                    farEnoughPoints.Add(point);
            }
            if (farEnoughPoints.Count > 0)
                return farEnoughPoints[Random.Range(0, farEnoughPoints.Count)];
        }

        return freeSpawnPoints[Random.Range(0, freeSpawnPoints.Count)];
    }


    //Encuentra una posición válida para spawn (lejos del jugador)

    Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Pool] No hay spawn points configurados, usando posición default");
            return new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        }

        // Intentar encontrar un spawn point válido
        int attempts = 0;
        int maxAttempts = 20;

        while (attempts < maxAttempts)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (playerTransform == null)
                return spawnPoint.position;

            float distance = Vector3.Distance(spawnPoint.position, playerTransform.position);

            if (distance >= minDistanceFromPlayer)
                return spawnPoint.position;

            attempts++;
        }

        // Fallback: usar el más lejano
        return GetFarthestSpawnPoint();
    }


    //Encuentra el spawn point más lejano del jugador
    Vector3 GetFarthestSpawnPoint()
    {
        if (spawnPoints.Length == 0)
            return Vector3.zero;

        Transform farthest = spawnPoints[0];
        float maxDistance = 0f;

        if (playerTransform != null)
        {
            foreach (Transform point in spawnPoints)
            {
                float distance = Vector3.Distance(point.position, playerTransform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthest = point;
                }
            }
        }
        else
        {
            farthest = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        return farthest.position;
    }


    
}
