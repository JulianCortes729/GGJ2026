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

    float startSlerpPositionSpring=0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        startLocalRotation = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    // Start is called before the first frame update
    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation)
            return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidbody.transform.localRotation, startLocalRotation);
    }

    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1;
        joint.slerpDrive= jointDrive;
    }

    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive= jointDrive;
    }
}
