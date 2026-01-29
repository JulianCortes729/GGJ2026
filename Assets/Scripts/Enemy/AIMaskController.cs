using UnityEngine;

//Controlador de máscaras para enemigos IA
//Similar a PlayerMaskController pero sin inputs manuales
public class AIMaskController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform maskHolder;
    [SerializeField] private MaskPoolManager poolManager;

    [Header("Visual")]
    [SerializeField] private float equipScale = 0.3f;
    [SerializeField] private float pickupRange = 2f;

    private GameObject currentMaskObject;
    private Mask currentMask;
    private Vector3 originalMaskScale;

    void Update()
    {
        if (currentMask != null)
        {
            currentMask.Tick(Time.deltaTime);
        }
    }

    public bool TryPickupMask(GameObject maskObject)
    {
        if (maskObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, maskObject.transform.position);
        if (distance > pickupRange)
            return false;

        if (currentMaskObject == maskObject)
            return false;

        PickupMask(maskObject);
        return true;
    }

    public bool TryUseAbility()
    {
        if (currentMask != null && currentMask.CanUse())
        {
            currentMask.TryUse(gameObject);
            return true;
        }
        return false;
    }

    private void PickupMask(GameObject maskObject)
    {
        if (currentMaskObject != null)
        {
            DropCurrentMask();
        }

        currentMaskObject = maskObject;
        currentMask = maskObject.GetComponent<Mask>();

        if (currentMask == null)
            return;

        originalMaskScale = maskObject.transform.localScale;

        maskObject.transform.SetParent(maskHolder);
        maskObject.transform.localPosition = Vector3.zero;
        maskObject.transform.localRotation = Quaternion.identity;
        maskObject.transform.localScale = Vector3.one * equipScale;

        Collider col = maskObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    private void DropCurrentMask()
    {
        if (currentMaskObject == null)
            return;

        currentMaskObject.transform.SetParent(null);

        Collider col = currentMaskObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        currentMaskObject.transform.localScale = originalMaskScale;

        poolManager.RecycleMask(currentMaskObject);
        poolManager.SpawnMask(currentMask.GetMaskType());

        currentMaskObject = null;
        currentMask = null;
    }

    public void ForceDropMask()
    {
        DropCurrentMask();
    }

    public Mask GetCurrentMask() => currentMask;
    public bool HasMask() => currentMask != null;
    public MaskType GetCurrentMaskType() => currentMask != null ? currentMask.GetMaskType() : MaskType.None;
    public GameObject GetCurrentMaskObject() => currentMaskObject;
    public float GetPickupRange() => pickupRange;
}