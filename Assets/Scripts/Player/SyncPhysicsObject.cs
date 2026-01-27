using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint joint;

    [SerializeField]
    Rigidbody animatedRigidbody;

    [SerializeField]
    bool syncAnimation = false;

    Quaternion startLocalRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        startLocalRotation = transform.localRotation;
    }

    // Start is called before the first frame update
    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation)
            return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidbody.transform.localRotation, startLocalRotation);
    }
}
