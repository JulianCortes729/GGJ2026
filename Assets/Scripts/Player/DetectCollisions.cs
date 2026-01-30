using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class DetectCollisions : MonoBehaviour
{

    RagdollPlayerMovement ragdollPlayer;
    Rigidbody rb;
    ContactPoint[] contactPoints= new ContactPoint[5];

    // Start is called before the first frame update
    void Awake()
    {
        ragdollPlayer = GetComponentInParent<RagdollPlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ragdollPlayer.IsActiveRAgdoll || !collision.transform.CompareTag("CauseDamage") || CollisionIsPlayer(collision))
        {
            return;
        }

        int numberOfContacts = collision.GetContacts(contactPoints);

        for (int i=0; i<numberOfContacts; i++)
        {
            ContactPoint contactPoint= contactPoints[i];
            Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;

            if (contactImpulse.magnitude < 15 || !ragdollPlayer.IsActiveRAgdoll)
                continue;
            
            Debug.Log("contactImpulse is: "+ contactImpulse.magnitude);

            ragdollPlayer.OnBodyPartHit();
            if (!ragdollPlayer.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.25f;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 25);
                
                Debug.Log("Small Hit, force aplied: "+forceDirection);
    
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
            else if(ragdollPlayer.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse +(Vector3.up * 8f)) *2;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 100f);
                
                Debug.Log("Big Hit, force aplied: "+forceDirection);
    
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
            
            
        }
    }

    private bool CollisionIsPlayer(Collision collision)
    {
        return collision.collider.transform.root == ragdollPlayer.transform;
    }

}
