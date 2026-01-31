using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Movimiento ragdoll con NavMesh
public class AIRagdollMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ConfigurableJoint mainJoint;
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float moveForce = 30f;
    [SerializeField] private float jumpMultiplier = 20f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float velocityDamping = 0.95f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float groundCheckRadius = 0.1f;
    private RaycastHit[] raycastHits = new RaycastHit[10];
    private bool isGrounded;

    [Header("Stabilization")]
    [SerializeField] private float stabilizationForce = 5f;
    [SerializeField] private float maxTiltAngle = 45f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    [Header("Combat")]
    [SerializeField] private int maxHitsBeforeRagdoll = 5;
    private int currentHitsBeforeRagdoll;
    private bool canBeLaunched = false;
    public bool CanBeLaunched => canBeLaunched;

    private SyncPhysicsObject[] syncPhysicsObjects;
    private float startSlerpPositionSpring = 0f;
    bool isActiveRagdoll = true;
    public bool IsActiveRAgdoll => isActiveRagdoll;
    private AIEnemyController controller;
    private Vector2 moveInput;
    private float lastTimeRagdoll = 0;

    private void Awake()
    {
        controller = GetComponent<AIEnemyController>();
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
        currentHitsBeforeRagdoll = maxHitsBeforeRagdoll;

        if (navAgent != null)
        {
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }
    }

    private void Start()
    {
        if (rb != null && rb.drag < 0.5f)
        {
            rb.drag = 1f;
        }
        startSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    private void Update()
    {
        if (!isActiveRagdoll)
        {
            moveInput = Vector2.zero;
            if (Time.time - lastTimeRagdoll > 3)
            {
                MakeActiveRagdoll();
            }
        }
        else if (controller != null)
        {
            moveInput = controller.movementVector;
        }

        UpdateAnimations();
        SyncNavMeshAgent();
    }

    private void FixedUpdate()
    {
        CheckGroundStatus();
        ApplyGravity();
        ApplyStabilization();
        HandleMovement();
        SyncPhysicsWithAnimation();
    }

    private void SyncNavMeshAgent()
    {
        if (navAgent != null && rb != null && navAgent.isOnNavMesh)
        {
            Vector3 rbPosition = rb.position;

            if (NavMesh.SamplePosition(rbPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                navAgent.nextPosition = hit.position;
            }
            else
            {
                navAgent.nextPosition = rbPosition;
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            bool isWalking = moveInput.sqrMagnitude > 0.01f;
            animator.SetBool("Walking", isWalking);
        }
    }

    private void CheckGroundStatus()
    {
        isGrounded = false;

        if (rb == null)
            return;

        int hits = Physics.SphereCastNonAlloc(
            rb.position + Vector3.up * 0.1f,
            groundCheckRadius,
            Vector3.down,
            raycastHits,
            groundCheckDistance
        );

        for (int i = 0; i < hits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded && rb != null)
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    private void ApplyStabilization()
    {
        if (rb == null || mainJoint == null)
            return;

        Vector3 up = transform.up;
        float tiltAngle = Vector3.Angle(up, Vector3.up);

        if (tiltAngle > maxTiltAngle)
        {
            Vector3 correctionDirection = Vector3.Cross(up, Vector3.up);
            Vector3 torqueDirection = Vector3.Cross(correctionDirection, up);
            rb.AddTorque(torqueDirection * stabilizationForce, ForceMode.Acceleration);
        }

        if (rb.angularVelocity.sqrMagnitude > 25f)
        {
            rb.angularVelocity *= 0.9f;
        }
    }

    private void HandleMovement()
    {
        if (moveInput.sqrMagnitude < 0.01f)
            return;

        if (rb == null || mainJoint == null)
            return;

        Vector3 localMovementDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldMovementDirection = transform.TransformDirection(localMovementDirection);
        worldMovementDirection.y = 0;
        worldMovementDirection.Normalize();

        RotateTowardsMovement(worldMovementDirection);
        ApplyMovementForce(worldMovementDirection);
        LimitVelocity();
    }

    private void RotateTowardsMovement(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude < 0.01f || mainJoint == null)
            return;

        float angleY = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, angleY, 0f);

        mainJoint.targetRotation = Quaternion.Slerp(
            mainJoint.targetRotation,
            Quaternion.Inverse(targetRotation),
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    private void ApplyMovementForce(Vector3 worldDirection)
    {
        if (rb == null || !isGrounded)
            return;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (currentSpeed < maxSpeed)
        {
            Vector3 force = worldDirection * moveForce;
            rb.AddForce(force, ForceMode.Force);
        }
        else
        {
            rb.velocity = new Vector3(
                rb.velocity.x * velocityDamping,
                rb.velocity.y,
                rb.velocity.z * velocityDamping
            );
        }
    }

    private void LimitVelocity()
    {
        if (rb == null)
            return;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void SyncPhysicsWithAnimation()
    {
        if (syncPhysicsObjects != null)
        {
            for (int i = 0; i < syncPhysicsObjects.Length; i++)
            {
                syncPhysicsObjects[i].UpdateJointFromAnimation();
            }
        }
    }

    public void Jump()
    {
        if (isGrounded && rb != null)
        {
            rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || rb == null)
            return;

        // Mostrar direcci�n de movimiento
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 localDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 worldDirection = transform.TransformDirection(localDirection);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(rb.position, worldDirection.normalized * 2f);
        }

        // Mostrar velocidad actual
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rb.position, rb.velocity);

        // Mostrar detecci�n de suelo
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(rb.position, groundCheckRadius);
        Gizmos.DrawRay(rb.position + Vector3.up * 0.1f, Vector3.down * groundCheckDistance);
    }

    void MakeRagdoll()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;

        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].MakeRagdoll();
        }
        isActiveRagdoll = false;
        lastTimeRagdoll = Time.time;
        canBeLaunched = false;
    }

    void MakeActiveRagdoll()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;

        for (int i = 0; i < syncPhysicsObjects.Length; i++)
            syncPhysicsObjects[i].MakeActiveRagdoll();
        
        isActiveRagdoll = true;
        canBeLaunched = true;
    }

    public void OnBodyPartHit()
    {
        OnNeutralBodyPartHit();
    }

    public void OnAdvantageBodyPartHit()
    {
        ApplyHit(2);
    }

    public void OnDisvantageBodyPartHit()
    {
        ApplyHit(0);
    }

    public void OnNeutralBodyPartHit()
    {
        ApplyHit(1);
    }

    private void ApplyHit(int damage)
    {
        if (currentHitsBeforeRagdoll <= 0)
        {
            MakeRagdoll();
        }
        currentHitsBeforeRagdoll -= damage;
        Debug.Log("AI Current Hits before ragdoll: " + currentHitsBeforeRagdoll);
    }
}