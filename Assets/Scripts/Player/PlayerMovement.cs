using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

   
    private void FixedUpdate()
    {
        float movementH = Input.GetAxis("Horizontal");
        float movementV = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(movementH, 0.0f, movementV).normalized;
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

}
