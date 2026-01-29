using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ConfigurableJoint mainJoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private Vector2 moveInput;
    private bool isJumpPressed;
    private bool isGrounded;

    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float moveForce = 30f;
    [SerializeField] private float jumpMultiplier = 20f;
    [SerializeField] private float rotationSpeed = 10f;

    private RaycastHit[] raycastHits = new RaycastHit[10];
    private SyncPhysicsObject[] syncPhysicsObjects;

    RagdollController controller;

    void Awake()
    {
        controller = GetComponent<RagdollController>();
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    void Start()
    {
        startSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    void Update()
    {
        HandleInput();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGroundStatus();
        ApplyGravity();
        HandleMovement();
        HandleJump();
        SyncPhysicsWithAnimation();
    }

    
    //Captura los inputs del jugador (movimiento, salto, ataques)
    private void HandleInput()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        if (!isGrounded && Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Air Spin");
        if (Input.GetKeyDown(KeyCode.Space))
            isJumpPressed = true;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            animator.SetTrigger("LeftPunch");
        if (Input.GetKeyDown(KeyCode.Mouse1))
            animator.SetTrigger("RightPunch");
        
    }

    
    //Actualiza los par�metros del animador
    private void UpdateAnimations()
    {
        animator.SetBool("Jumping", !isGrounded);
        animator.SetBool("Walking", moveInput.sqrMagnitude > 0.01f);
        animator.SetBool("Grabing", Input.GetKey(KeyCode.E));
    }

    
    //Verifica si el jugador est� tocando el suelo usando un SphereCast
    private void CheckGroundStatus()
    {
        isGrounded = false;

        int hits = Physics.SphereCastNonAlloc(rb.position, 0.1f, Vector3.down, raycastHits, 0.5f);

        for (int i = 0; i < hits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }
    }


    //Aplica gravedad adicional cuando el jugador est� en el aire
    private void ApplyGravity()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * 10f);
    }


    //Maneja el movimiento del jugador relativo a la c�mara
    private void HandleMovement()
    {
        Vector3 movementDirection = GetCameraRelativeMovement();
        float inputMagnitude = movementDirection.magnitude;

        if (inputMagnitude > 0.01f)
        {
            movementDirection.Normalize();
            RotatePlayerTowardsMovement(movementDirection);
            ApplyMovementForce(movementDirection, inputMagnitude);
        }
    }


    //Calcula la direcci�n del movimiento relativa a la orientaci�n de la c�mara
    private Vector3 GetCameraRelativeMovement()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        //Proyectar en el plano horizontal
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        return camForward * moveInput.y + camRight * moveInput.x;
    }

    
    //Rota suavemente al jugador hacia la direcci�n del movimiento
    private void RotatePlayerTowardsMovement(Vector3 direction)
    {
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, angleY, 0f);

        //Invertir la rotaci�n porque ConfigurableJoint trabaja en espacio local invertido
        mainJoint.targetRotation = Quaternion.Slerp(
            mainJoint.targetRotation,
            Quaternion.Inverse(targetRotation),
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    //Aplica fuerza de movimiento si no se ha alcanzado la velocidad m�xima
    private void ApplyMovementForce(Vector3 direction, float inputMagnitude)
    {
        float currentVelocity = Vector3.Dot(direction, rb.velocity);

        if (currentVelocity < maxSpeed)
        {
            rb.AddForce(direction * inputMagnitude * moveForce, ForceMode.Force);
        }
    }


    //Maneja el salto del jugador
    private void HandleJump()
    {
        if (isGrounded && isJumpPressed)
        {
            rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Impulse);
            isJumpPressed = false;
        }
    }

    void MakeRagdoll()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
        
        for (int i=0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].MakeRagdoll();
        }
        isActiveRagdoll=false;
    }

    void MakeActiveRagdoll()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;

        for (int i=0; i < syncPhysicsObjects.Length; i++)
            syncPhysicsObjects[i].MakeActiveRagdoll();

        isActiveRagdoll=true;
    }

    public void OnPlayerBodyPartHit()
    {
        MakeRagdoll();
    }

    //Sincroniza los objetos de f�sica con la animaci�n
    private void SyncPhysicsWithAnimation()
    {
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}