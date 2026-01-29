using UnityEngine;


//Controla el sistema de m�scaras del jugador: recoger, equipar, usar habilidades
public class PlayerMaskController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform maskHolder; //Posici�n en la cabeza donde se equipa la m�scara
    [SerializeField] private MaskPoolManager poolManager;

    [Header("Input")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private KeyCode useAbilityKey = KeyCode.C;

    [Header("Visual")]
    [SerializeField] private float equipScale = 1.1f; //Escala de la m�scara cuando est� equipada

    // Estado actual
    private GameObject currentMaskObject;
    private Mask currentMask;
    private Vector3 originalMaskScale; //Guardar escala original
    private GameObject nearbyMask;

    void Update()
    {
        //Actualizar cooldown de m�scara equipada
        if (currentMask != null)
        {
            currentMask.Tick(Time.deltaTime);
        }

        //Input para usar habilidad
        if (Input.GetKeyDown(useAbilityKey) && currentMask != null)
        {
            currentMask.TryUse(gameObject);
        }

        //Input para recoger/cambiar m�scara
        if (Input.GetKeyDown(pickupKey) && nearbyMask != null && nearbyMask != currentMaskObject)
        {
            PickupMask(nearbyMask);
        }
    }


    //Recoge una m�scara nueva (y suelta la actual si existe)
    void PickupMask(GameObject maskObject)
    {
        //Si ya tengo una m�scara, la devuelvo al pool y respawneo
        if (currentMaskObject != null)
        {
            DropCurrentMask();
        }

        //Equipar nueva m�scara
        currentMaskObject = maskObject;
        currentMask = maskObject.GetComponent<Mask>();

        if (currentMask == null)
        {
            Debug.LogError("[Player] GameObject no tiene component Mask!");
            return;
        }

        //Guardar escala original ANTES de modificarla
        originalMaskScale = maskObject.transform.localScale;

        //Mover m�scara al holder (cabeza)
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

        Debug.Log($"[Player] Equipada m�scara: {currentMask.GetMaskType()}");
    }


    //Suelta la m�scara actual y la respawnea en el mundo
    void DropCurrentMask()
    {
        if (currentMaskObject == null) return;

        Debug.Log($"[Player] Soltando m�scara: {currentMask.GetMaskType()}");

        //Desparentear PRIMERO antes de reciclar
        currentMaskObject.transform.SetParent(null);

        //Reactivar collider
        Collider col = currentMaskObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        //Restaurar escala ORIGINAL (la que ten�a cuando la recogimos)
        currentMaskObject.transform.localScale = originalMaskScale;

        //Reciclar (desactiva y devuelve al pool)
        poolManager.RecycleMask(currentMaskObject);

        //Respawnear en nueva posici�n
        poolManager.SpawnMask(currentMask.GetMaskType());

        //Limpiar referencias
        currentMaskObject = null;
        currentMask = null;
    }

    //Detecta cuando el jugador est� cerca de una m�scara
    void OnTriggerEnter(Collider other)
    {
        Mask mask = other.GetComponent<Mask>();
        if (mask != null)
        {
            nearbyMask = other.gameObject;
            Debug.Log($"[Player] M�scara cerca: {mask.GetMaskType()}");
        }
    }


    //Detecta cuando el jugador se aleja de una m�scara
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyMask)
        {
            nearbyMask = null;
            Debug.Log("[Player] M�scara fuera de rango");
        }
    }



    public Mask GetCurrentMask() => currentMask;
    public bool HasMask() => currentMask != null;
    public MaskType? GetCurrentMaskType() => currentMask?.GetMaskType();
}