using UnityEngine;


// Componente helper que guarda referencia al spawn point ocupado por esta máscara
// Se agrega automáticamente en runtime por el MaskPoolManager
public class MaskSpawnTracker : MonoBehaviour
{
    [HideInInspector]
    public Transform occupiedSpawnPoint;
}