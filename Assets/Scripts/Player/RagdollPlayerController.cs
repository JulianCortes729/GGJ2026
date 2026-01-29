using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPlayerController : RagdollController
{

    void Update()
    {
        UpdateIntent();
    }

    public override void UpdateIntent()
    {
        movementVector = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space)) wantsToJump = true;
    }

    public void ConsumeJump()
    {
        wantsToJump = false;
    }


}
