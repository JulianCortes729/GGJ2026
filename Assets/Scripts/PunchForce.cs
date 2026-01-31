using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchForce : MonoBehaviour
{
    [SerializeField] Rigidbody leftArmRB;
    [SerializeField] Rigidbody rightArmRB;
    [SerializeField] float punchingForce=5;

    void AddForceToLeftPunch()
    {
        leftArmRB.AddForce(leftArmRB.transform.up*punchingForce,ForceMode.Impulse);
    }
    
    void AddForceToRightPunch()
    {
        rightArmRB.AddForce(rightArmRB.transform.up*punchingForce,ForceMode.Impulse);
    }
}
