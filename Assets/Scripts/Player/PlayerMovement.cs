using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    private float movementH;
    private float movementV;
    [SerializeField] Transform cameraTransform;
    private float rotationSpeed = 10f;
   

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        movementH = Input.GetAxis("Horizontal");
        movementV = Input.GetAxis("Vertical");

    }

    private void FixedUpdate()
    {

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDirection = (camRight * movementH + camForward * movementV).normalized;

        //rotacion del player
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed));
        }

        rb.MovePosition(rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime);
    }

 

}
