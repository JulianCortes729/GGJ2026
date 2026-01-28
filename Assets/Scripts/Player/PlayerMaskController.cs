using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskController : MonoBehaviour
{
    [SerializeField] private Transform maskHolder; //donde se pone la máscara
    private Mask currentMask;
    private MaskPickup nearbyPickup; //pickup que tengo cerca

    void Start()
    {

    }

    void Update()
    {
        if (currentMask != null)
        {
            currentMask.Tick(Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.C)) currentMask.TryUse(gameObject);
        }

        
        if (Input.GetKeyDown(KeyCode.E) && nearbyPickup != null) //Cambio voluntario, solo si hay una máscara cerca
        {
            ChangeMask(nearbyPickup);
        }

    }

    void ChangeMask(MaskPickup pickup)
    {
        if (currentMask != null)
        {
            currentMask.transform.parent = null;
            currentMask.transform.position = transform.position + Vector3.right;
            currentMask.gameObject.SetActive(true);
        }

        currentMask = pickup.GetMaskInstance(); // se instancia
        currentMask.transform.SetParent(maskHolder); // se equipa
        currentMask.transform.localPosition = Vector3.zero; // se resetea la posición y rotación
        currentMask.transform.localRotation = Quaternion.identity;


        Destroy(pickup.gameObject); // se destruya la mascara que estaba en el suelo
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MaskPickup pickup))
            nearbyPickup = pickup;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out MaskPickup pickup))
            if (pickup == nearbyPickup)
                nearbyPickup = null;
    }
}