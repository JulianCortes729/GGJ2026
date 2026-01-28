using UnityEngine;


//Controla el sistema de máscaras del jugador: recoger, equipar, usar habilidades
public class PlayerMaskController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform maskHolder; //Posición en la cabeza donde se equipa la máscara
    [SerializeField] private MaskPoolManager poolManager;

    [Header("Input")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private KeyCode useAbilityKey = KeyCode.C;

    [Header("Visual")]
    [SerializeField] private float equipScale = 0.3f; //Escala de la máscara cuando está equipada

    // Estado actual
    private GameObject currentMaskObject;
    private Mask currentMask;
    private Vector3 originalMaskScale; //Guardar escala original
    private GameObject nearbyMask;

    void Update()
    {
        //Actualizar cooldown de máscara equipada
        if (currentMask != null)
        {
            currentMask.Tick(Time.deltaTime);
        }

        //Input para usar habilidad
        if (Input.GetKeyDown(useAbilityKey) && currentMask != null)
        {
            currentMask.TryUse(gameObject);
        }

        //Input para recoger/cambiar máscara
        if (Input.GetKeyDown(pickupKey) && nearbyMask != null && nearbyMask != currentMaskObject)
        {
            PickupMask(nearbyMask);
        }
    }


    //Recoge una máscara nueva (y suelta la actual si existe)
    void PickupMask(GameObject maskObject)
    {
        //Si ya tengo una máscara, la devuelvo al pool y respawneo
        if (currentMaskObject != null)
        {
            DropCurrentMask();
        }

        //Equipar nueva máscara
        currentMaskObject = maskObject;
        currentMask = maskObject.GetComponent<Mask>();

        if (currentMask == null)
        {
            Debug.LogError("[Player] GameObject no tiene component Mask!");
            return;
        }

        //Guardar escala original ANTES de modificarla
        originalMaskScale = maskObject.transform.localScale;

        //Mover máscara al holder (cabeza)
        maskObject.transform.SetParent(maskHolder);
        maskObject.transform.localPosition = Vector3.zero;
        maskObject.transform.localRotation = Quaternion.identity;
        maskObject.transform.localScale = Vector3.one * equipScale;

        //Desactivar collider para que no interfiera
        Collider col = maskObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        //Limpiar referencia para evitar recoger inmediatamente otra
        nearbyMask = null;

        Debug.Log($"[Player] Equipada máscara: {currentMask.GetMaskType()}");
    }


    //Suelta la máscara actual y la respawnea en el mundo
    void DropCurrentMask()
    {
        if (currentMaskObject == null) return;

        Debug.Log($"[Player] Soltando máscara: {currentMask.GetMaskType()}");

        //Desparentear PRIMERO antes de reciclar
        currentMaskObject.transform.SetParent(null);

        //Reactivar collider
        Collider col = currentMaskObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        //Restaurar escala ORIGINAL (la que tenía cuando la recogimos)
        currentMaskObject.transform.localScale = originalMaskScale;

        //Reciclar (desactiva y devuelve al pool)
        poolManager.RecycleMask(currentMaskObject);

        //Respawnear en nueva posición
        poolManager.SpawnMask(currentMask.GetMaskType());

        //Limpiar referencias
        currentMaskObject = null;
        currentMask = null;
    }

    //Detecta cuando el jugador está cerca de una máscara
    void OnTriggerEnter(Collider other)
    {
        Mask mask = other.GetComponent<Mask>();
        if (mask != null)
        {
            nearbyMask = other.gameObject;
            Debug.Log($"[Player] Máscara cerca: {mask.GetMaskType()}");
        }
    }


    //Detecta cuando el jugador se aleja de una máscara
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyMask)
        {
            nearbyMask = null;
            Debug.Log("[Player] Máscara fuera de rango");
        }
    }



    public Mask GetCurrentMask() => currentMask;
    public bool HasMask() => currentMask != null;
    public MaskType? GetCurrentMaskType() => currentMask?.GetMaskType();
}