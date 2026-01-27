using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPlayerMovement : MonoBehaviour
{

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    ConfigurableJoint mainJoint;

    Vector2 moveInput = Vector2.zero;
    bool isJumpPressed = false;
    float maxSpeed = 5;
    float jumpMultiplier = 20;
    bool isGrounded = false;
    RaycastHit[] raycastHits = new RaycastHit[10];

    SyncPhysicsObject[] syncPhysicsObjects;

    [SerializeField]
    Animator animator;

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = false;

        int numberOfHits = Physics.SphereCastNonAlloc(rb.position,0.1f,transform.up * -1, raycastHits, 0.5f);

        for (int i=0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;
            
            isGrounded = true;
            break;
        }

        if (!isGrounded)
            rb.AddForce(Vector3.down * 10);
        
        float inputMagnitude = moveInput.magnitude;
        
        animator.SetBool("Walking", inputMagnitude!=0);

        if (inputMagnitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInput.x * -1 ,0,moveInput.y),transform.up);
            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation,desiredDirection, Time.fixedDeltaTime * 300);

            Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rb.velocity);

            float localForwardVelocity = localVelocityVsForward.magnitude;

            if(localForwardVelocity < maxSpeed)
            {
                rb.AddForce(transform.forward * inputMagnitude * 30);
            }
        }

        if(isGrounded && isJumpPressed)
        {
            rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Impulse);
            isJumpPressed = false;
        }

        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}
