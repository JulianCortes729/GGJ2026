using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPlayerController : RagdollController
{

    // Update is called once per frame
    void Update()
    {
        this.movementVector = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        this.wantsToJump = Input.GetKeyDown(KeyCode.Space);
    }
}
