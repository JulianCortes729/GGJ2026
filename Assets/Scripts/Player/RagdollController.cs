using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RagdollController : MonoBehaviour
{

    public Vector2 movementVector {get;protected set;} = Vector2.zero;
    public bool wantsToJump {get; protected set;} = false;

}
