using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashMask : Mask
{
    
    [SerializeField] private float dashForce = 10f;

    protected override void Use(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector3 dir = player.transform.forward;

        rb.AddForce(dir * dashForce, ForceMode.Impulse);
    }
    
}
