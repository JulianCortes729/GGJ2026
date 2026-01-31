using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class DetectCollisions : MonoBehaviour
{

    RagdollPlayerMovement ragdollPlayer;
    Rigidbody rb;
    ContactPoint[] contactPoints= new ContactPoint[5];
    PlayerMaskController maskController;

    // Start is called before the first frame update
    void Awake()
    {
        ragdollPlayer = GetComponentInParent<RagdollPlayerMovement>();
        rb = GetComponent<Rigidbody>();
        maskController = GetComponentInParent<PlayerMaskController>();
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

            if (AudioManager.Instance != null)
                AudioManager.PlaySFX(AudioManager.Instance.ragdollImpactClip, 1f, Random.Range(0.8f, 1.2f));

            MaskType myMask = maskController != null && maskController.GetCurrentMaskType().HasValue ? maskController.GetCurrentMaskType().Value : MaskType.None;
            
            PlayerMaskController enemyController = collision.collider.GetComponentInParent<PlayerMaskController>();
            MaskType enemyMask = enemyController != null && enemyController.GetCurrentMaskType().HasValue ? enemyController.GetCurrentMaskType().Value : MaskType.None;

            if (MaskAdvantageSystem.HasAdvantage(enemyMask, myMask))
            {
                ragdollPlayer.OnAdvantageBodyPartHit();
            }
            else if (MaskAdvantageSystem.HasDisadvantage(enemyMask, myMask))
            {
                ragdollPlayer.OnDisvantageBodyPartHit();
            }
            else
            {
                ragdollPlayer.OnNeutralBodyPartHit();
            }
            
            if (!ragdollPlayer.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.25f;

                forceDirection = Vector3.ClampMagnitude(forceDirection, 25);
                
                Debug.Log("Small Hit, force aplied: "+forceDirection);
    
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }
            else if(ragdollPlayer.CanBeLaunched)
            {
                Vector3 forceDirection = (contactImpulse +(Vector3.up * 15f)) *2;

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
