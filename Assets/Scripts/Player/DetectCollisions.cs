using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisions : MonoBehaviour
{

    RagdollPlayerMovement ragdollPlayer;
    Rigidbody rb;
    ContactPoint[] contactPoints= new ContactPoint[5];

    // Start is called before the first frame update
    void Awake()
    {
        ragdollPlayer = GetComponent<RagdollPlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ragdollPlayer.IsActiveRAgdoll || !collision.transform.CompareTag("CauseDamage") || CollisionIsPlayer(collision))
            return;
        
        int numberOfContacts = collision.GetContacts(contactPoints);

        for (int i=0; i<numberOfContacts; i++)
        {
            ContactPoint contactPoint= contactPoints[i];
            Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;

            if (contactImpulse.magnitude < 15)
                continue;
            
            
        }
    }

    private bool CollisionIsPlayer(Collision collision)
    {
        return collision.collider.transform.root == ragdollPlayer;
    }

}
