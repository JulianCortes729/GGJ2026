using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class AIDetectCollisions : MonoBehaviour
{

    AIRagdollMovement ragdollAI;
    Rigidbody rb;
    ContactPoint[] contactPoints= new ContactPoint[5];
    AIMaskController maskController;

    // Start is called before the first frame update
    void Awake()
    {
        ragdollAI = GetComponentInParent<AIRagdollMovement>();
        rb = GetComponent<Rigidbody>();
        maskController = GetComponentInParent<AIMaskController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ragdollAI.IsActiveRAgdoll || !collision.transform.CompareTag("CauseDamageToEnemy") || CollisionIsPlayer(collision))
        {
            return;
        }

        int numberOfContacts = collision.GetContacts(contactPoints);

        for (int i=0; i<numberOfContacts; i++)
        {
            ContactPoint contactPoint= contactPoints[i];
            Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;

            if (contactImpulse.magnitude < 15)
                continue;
            
            if (AudioManager.Instance != null)
                AudioManager.PlaySFX(AudioManager.Instance.ragdollImpactClip, 1f, Random.Range(0.8f, 1.2f));
                
            Debug.Log("contactImpulse is: "+ contactImpulse.magnitude);
        
            MaskType myMask = maskController != null ? maskController.GetCurrentMaskType() : MaskType.None;
            
            PlayerMaskController enemyController = collision.collider.GetComponentInParent<PlayerMaskController>();
            MaskType enemyMask = enemyController != null && enemyController.GetCurrentMaskType().HasValue ? enemyController.GetCurrentMaskType().Value : MaskType.None;

            if (MaskAdvantageSystem.HasAdvantage(enemyMask, myMask))
            {
                ragdollAI.OnAdvantageBodyPartHit();
            }
            else if (MaskAdvantageSystem.HasDisadvantage(enemyMask, myMask))
            {
                ragdollAI.OnDisvantageBodyPartHit();
            }
            else
            {
                ragdollAI.OnNeutralBodyPartHit();
            }

            if (!ragdollAI.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.25f;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 30);

                Debug.DrawRay(rb.position, forceDirection*40, Color.red);
            
                Debug.Log("force aplied: "+forceDirection);

                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
            else if(ragdollAI.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse + (Vector3.up * 15f)) * 2;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 100f);
                
                Debug.DrawRay(rb.position, forceDirection*40, Color.red);
                Debug.Log("Big Hit, force aplied: "+forceDirection);
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
        }
    }

    private bool CollisionIsPlayer(Collision collision)
    {
        return collision.collider.transform.root == ragdollAI.transform;
    }

}
