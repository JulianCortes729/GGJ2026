using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    SeekingMask,
    ApproachingMask,
    FollowingPlayer,
    Fleeing,
    WaitingForMask
}

//Controlador principal de IA para enemigos ragdoll
public class AIEnemyController : RagdollController
{
    [Header("AI References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private AIMaskController maskController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMaskController playerMaskController;
    [SerializeField] private Animator animator;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float approachDistance = 3f;
    [SerializeField] private float maskCheckInterval = 0.5f;
    [SerializeField] private float aggressiveness = 0.6f; // 0-1

    [Header("Mask Seeking")]
    [SerializeField] private float maskSearchRadius = 30f;
    [SerializeField] private LayerMask maskLayer;

    private AIState currentState = AIState.Idle;
    private GameObject targetMaskObject;
    private float lastMaskCheckTime;
    private float lastMaskSearchTime;
    private float maskSearchInterval = 1f;

    [SerializeField] private float atackCooldown = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    private float lastAtackTime=0f;
    private List<string> armsAnimations= new List<string>{"LeftPunch","RightPunch"};

    private void Awake()
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (maskController == null)
            maskController = GetComponent<AIMaskController>();
    }

    private void Start()
    {
        if (navAgent != null)
        {
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        ChangeState(AIState.Idle);
    }

    private void Update()
    {
        UpdateIntent();
    }

    public override void UpdateIntent()
    {
        UpdateAIBehavior();
        UpdateMovementVector();
    }

    private void UpdateAIBehavior()
    {
        if (Time.time - lastMaskCheckTime > maskCheckInterval)
        {
            lastMaskCheckTime = Time.time;
            EvaluateSituation();
        }

        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.SeekingMask:
                HandleSeekingMaskState();
                break;
            case AIState.ApproachingMask:
                HandleApproachingMaskState();
                break;
            case AIState.FollowingPlayer:
                HandleFollowingPlayerState();
                break;
            case AIState.Fleeing:
                HandleFleeingState();
                break;
            case AIState.WaitingForMask:
                HandleWaitingForMaskState();
                break;
        }
    }

    private void EvaluateSituation()
    {
        if (playerTransform == null || playerMaskController == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer < detectionRange;

        //Si no tiene m�scara, buscar una
        if (!maskController.HasMask())
        {
            if (currentState != AIState.SeekingMask && currentState != AIState.ApproachingMask)
            {
                ChangeState(AIState.SeekingMask);
            }
            return;
        }

        //Si el jugador est� en rango y tiene m�scara
        if (playerInRange)
        {
            MaskType myMask = maskController.GetCurrentMaskType();
            MaskType playerMask = playerMaskController.GetCurrentMaskType().GetValueOrDefault();

            bool hasAdvantage = MaskAdvantageSystem.HasAdvantage(myMask, playerMask);
            bool hasDisadvantage = MaskAdvantageSystem.HasDisadvantage(myMask, playerMask);

            if (hasDisadvantage)
            {
                if (currentState != AIState.Fleeing)
                {
                    ChangeState(AIState.Fleeing);
                }
                return;
            }

            if (hasAdvantage || Random.value < aggressiveness)
            {
                if (currentState != AIState.FollowingPlayer)
                {
                    ChangeState(AIState.FollowingPlayer);
                }
            }
        }
    }

    private void HandleIdleState()
    {
        if (!maskController.HasMask())
        {
            ChangeState(AIState.SeekingMask);
        }
    }

    private void HandleSeekingMaskState()
    {
        if (Time.time - lastMaskSearchTime > maskSearchInterval)
        {
            lastMaskSearchTime = Time.time;

            GameObject bestMask = FindBestAvailableMask();

            if (bestMask != null)
            {
                targetMaskObject = bestMask;
                ChangeState(AIState.ApproachingMask);
            }
            else
            {
                ChangeState(AIState.WaitingForMask);
            }
        }
    }

    private void HandleApproachingMaskState()
    {
        if (targetMaskObject == null)
        {
            ChangeState(AIState.SeekingMask);
            return;
        }

        //Actualizar constantemente el destino
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(targetMaskObject.transform.position);
        }

        float distanceToMask = Vector3.Distance(transform.position, targetMaskObject.transform.position);

        if (distanceToMask <= maskController.GetPickupRange())
        {
            bool success = maskController.TryPickupMask(targetMaskObject);

            if (success)
            {
                targetMaskObject = null;
                ChangeState(AIState.Idle);
            }
            else
            {
                targetMaskObject = null;
                ChangeState(AIState.SeekingMask);
            }
        }
    }

    private void HandleFollowingPlayerState()
    {
        if (playerTransform == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        navAgent.SetDestination(playerTransform.position);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= approachDistance)
        {
            maskController.TryUseAbility();
            if (Time.time-atackCooldown> lastAtackTime)
            {
                animator.SetTrigger(armsAnimations[Random.Range(0,2)]);
                lastAtackTime=Time.time;
            }
        }
    }

    private void HandleFleeingState()
    {
        if (playerTransform == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > fleeDistance)
        {
            ChangeState(AIState.SeekingMask);
            return;
        }

        Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    private void HandleWaitingForMaskState()
    {
        if (Time.time - lastMaskSearchTime > maskSearchInterval * 2f)
        {
            ChangeState(AIState.SeekingMask);
        }
    }

    private GameObject FindBestAvailableMask()
    {
        Collider[] maskColliders = Physics.OverlapSphere(transform.position, maskSearchRadius, maskLayer);

        if (maskColliders.Length == 0)
            return null;

        GameObject bestMask = null;
        float closestDistance = Mathf.Infinity;
        MaskType targetMaskType = MaskType.None;

        if (playerMaskController != null && playerMaskController.HasMask())
        {
            MaskType playerMask = playerMaskController.GetCurrentMaskType().GetValueOrDefault();
            targetMaskType = MaskAdvantageSystem.GetCounterMask(playerMask);
        }

        foreach (Collider col in maskColliders)
        {
            Mask mask = col.GetComponent<Mask>();
            if (mask == null)
                continue;

            GameObject maskObj = col.gameObject;

            if (maskObj == maskController.GetCurrentMaskObject())
                continue;

            // Verificar si est� equipada
            if (maskObj.transform.parent != null)
            {
                bool isInPool = maskObj.transform.parent.GetComponent<MaskPoolManager>() != null;

                if (!isInPool)
                    continue;
            }

            float distance = Vector3.Distance(transform.position, maskObj.transform.position);

            if (targetMaskType != MaskType.None && mask.GetMaskType() == targetMaskType)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestMask = maskObj;
                }
            }
            else if (bestMask == null && distance < closestDistance)
            {
                closestDistance = distance;
                bestMask = maskObj;
            }
        }

        return bestMask;
    }

    private void UpdateMovementVector()
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
        {
            movementVector = Vector2.zero;
            return;
        }

        if (navAgent.hasPath && navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            Vector3 worldVelocity = navAgent.desiredVelocity;
            Vector3 worldDirection = worldVelocity.normalized;

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            float forwardAmount = Vector3.Dot(worldDirection, forward);
            float rightAmount = Vector3.Dot(worldDirection, right);

            movementVector = new Vector2(rightAmount, forwardAmount);

            if (movementVector.sqrMagnitude > 1f)
            {
                movementVector.Normalize();
            }
        }
        else
        {
            movementVector = Vector2.zero;
        }
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        OnExitState(currentState);
        currentState = newState;
        OnEnterState(newState);
    }

    private void OnEnterState(AIState state)
    {
        switch (state)
        {
            case AIState.Fleeing:
            case AIState.FollowingPlayer:
            case AIState.ApproachingMask:
                if (navAgent != null)
                    navAgent.isStopped = false;
                break;

            case AIState.Idle:
            case AIState.WaitingForMask:
                if (navAgent != null)
                    navAgent.isStopped = true;
                break;
        }
    }

    private void OnExitState(AIState state)
    {
        //Cleanup si es necesario
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, approachDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maskSearchRadius);

        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        if (targetMaskObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetMaskObject.transform.position);
            Gizmos.DrawWireSphere(targetMaskObject.transform.position, 0.5f);
        }
    }
}