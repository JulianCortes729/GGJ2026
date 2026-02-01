using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyRope : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Body") || collision.transform.CompareTag("Body"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            ContactPoint[] contactPoints= new ContactPoint[5];
            int numberOfContacts = collision.GetContacts(contactPoints);

            for (int i=0; i<numberOfContacts; i++)
            {
                ContactPoint contactPoint= contactPoints[i];
                Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;

                if (contactImpulse.magnitude < 10)
                continue;
            
                Debug.Log("contactImpulse is: "+ contactImpulse.magnitude);

                Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.25f;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 25);
                
                Debug.Log("Small Hit, force aplied: "+forceDirection);
    
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
        }
    }
}
