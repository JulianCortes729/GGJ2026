using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    private float lastAtackTime = 0f;
    private int leftPunchHash;
    private int rightPunchHash;

    private Collider[] results = new Collider[15];
    private Transform poolTransform;

    private float pathUpdateInterval = 0.2f; //Actualizar camino 5 veces por segundo
    private float lastPathUpdateTime;

    //Inicializa referencias y cachea los Hashes de animación para evitar basura
    private void Awake()
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (maskController == null)
            maskController = GetComponent<AIMaskController>();

        poolTransform = FindObjectOfType<MaskPoolManager>()?.transform;

        // Convertimos los strings a números UNA SOLA VEZ
        leftPunchHash = Animator.StringToHash("LeftPunch");
        rightPunchHash = Animator.StringToHash("RightPunch");
    }

    //Configura NavAgent y desincroniza la IA
    private void Start()
    {
        if (navAgent != null)
        {
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        //Desincronizar: Cada enemigo empieza con un offset aleatorio su 'reloj' mental.
        lastMaskCheckTime = Time.time + Random.Range(0f, maskCheckInterval);
        ChangeState(AIState.Idle);
    }

    //Loop principal de Unity
    private void Update()
    {
        UpdateIntent();
    }

    //Conecta la lógica de IA con el movimiento físico del Ragdoll
    public override void UpdateIntent()
    {
        UpdateAIBehavior();
        UpdateMovementVector();
    }

    //Máquina de estados: evalúa la situación y ejecuta el comportamiento actual
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

    //Decide qué hacer (atacar, buscar máscara o huir) analizando el entorno
    private void EvaluateSituation()
    {
        if (playerTransform == null || playerMaskController == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer < detectionRange;

        //Si no tiene máscara, buscar una
        if (!maskController.HasMask())
        {
            if (currentState != AIState.SeekingMask && currentState != AIState.ApproachingMask)
            {
                ChangeState(AIState.SeekingMask);
            }
            return;
        }

        //Si el jugador está en rango y tiene máscara
        if (playerInRange)
        {
            MaskType myMask = maskController.GetCurrentMaskType();
            MaskType playerMask = playerMaskController.GetCurrentMaskType().GetValueOrDefault();
            MaskType neededCounter = MaskAdvantageSystem.GetCounterMask(playerMask);

            bool hasAdvantage = MaskAdvantageSystem.HasAdvantage(myMask, playerMask);
            bool hasDisadvantage = MaskAdvantageSystem.HasDisadvantage(myMask, playerMask);

            if (hasDisadvantage)
            {
                //Solo huir si hay una máscara disponible que nos dé ventaja
                GameObject betterMask = FindNearestMask(neededCounter);

                if (betterMask != null)
                {
                    //Hay una máscara mejor disponible, ir a buscarla
                    targetMaskObject = betterMask;
                    if (currentState != AIState.ApproachingMask)
                    {
                        ChangeState(AIState.ApproachingMask);
                    }
                }
                else
                {
                    //No hay mejor máscara disponible, seguir peleando
                    if (currentState != AIState.FollowingPlayer)
                    {
                        ChangeState(AIState.FollowingPlayer);
                    }
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

    //Comportamiento en reposo: si no tiene máscara, empieza a buscar
    private void HandleIdleState()
    {
        if (!maskController.HasMask())
        {
            ChangeState(AIState.SeekingMask);
        }
    }

    //Busca activamente máscaras cercanas periódicamente
    private void HandleSeekingMaskState()
    {
        if (Time.time - lastMaskSearchTime > maskSearchInterval)
        {
            lastMaskSearchTime = Time.time;

            GameObject bestMask = FindNearestMask(null);

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

    //Se mueve hacia la máscara objetivo y la recoge si está en rango (Pathfinding optimizado)
    private void HandleApproachingMaskState()
    {
        if (targetMaskObject == null || !targetMaskObject.activeInHierarchy)
        {
            ChangeState(AIState.SeekingMask);
            return;
        }

        //Verificar que la máscara objetivo sigue disponible (no fue recogida por otro)
        if (targetMaskObject.transform.parent != null)
        {
            bool isInPool = targetMaskObject.transform.parent.GetComponent<MaskPoolManager>() != null;
            if (!isInPool)
            {
                //La máscara fue recogida por otro, buscar una nueva
                Debug.Log($"[AI {gameObject.name}] Máscara objetivo fue recogida por otro, buscando nueva...");
                targetMaskObject = null;
                ChangeState(AIState.SeekingMask);
                return;
            }
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

    //Persigue al jugador limitando las llamadas al NavMesh y ataca con Hash IDs
    private void HandleFollowingPlayerState()
    {
        if (playerTransform == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        if (Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            lastPathUpdateTime = Time.time;

            if (navAgent != null && navAgent.isOnNavMesh && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(playerTransform.position);
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= approachDistance)
        {
            maskController.TryUseAbility();
            if (Time.time - atackCooldown > lastAtackTime)
            {
                //Usamos los números en lugar de strings. Mucho más rápido.
                int triggerToUse = (Random.value > 0.5f) ? leftPunchHash : rightPunchHash;
                animator.SetTrigger(triggerToUse);

                lastAtackTime = Time.time;
            }
        }
    }

    //Calcula una posición segura lejos del jugador y huye
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

    //Espera un tiempo antes de volver a intentar buscar máscaras
    private void HandleWaitingForMaskState()
    {
        if (Time.time - lastMaskSearchTime > maskSearchInterval * 2f)
        {
            ChangeState(AIState.SeekingMask);
        }
    }

    //Búsqueda optimizada (Zero Garbage) de la mejor máscara o counter
    private GameObject FindNearestMask(MaskType? specificType = null)
    {
        //Usamos el array pre-asignado
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, maskSearchRadius, results, maskLayer);

        if (numFound == 0) return null;

        GameObject bestMask = null;
        float closestDistSqr = Mathf.Infinity;

        // Variable clave para la lógica de prioridad
        bool bestIsPriority = false;

        MaskType counterType = MaskType.None;

        //Solo calculamos el counter si NO estamos buscando un tipo específico
        //y el jugador tiene máscara.
        if (!specificType.HasValue && playerMaskController != null && playerMaskController.HasMask())
        {
            MaskType playerMask = playerMaskController.GetCurrentMaskType().GetValueOrDefault();
            counterType = MaskAdvantageSystem.GetCounterMask(playerMask);
        }

        for (int i = 0; i < numFound; i++)
        {
            //Obtener componente de forma segura
            if (!results[i].TryGetComponent(out Mask mask))
                continue;

            GameObject maskObj = mask.gameObject; //Cacheamos el GameObject

            if (maskObj.transform.parent != poolTransform)
                continue;

            //No recoger nuestra propia máscara
            if (maskObj == maskController.GetCurrentMaskObject())
                continue;

            //No recoger una máscara del mismo tipo si ya la tengo
            if (maskController.HasMask() && mask.GetMaskType() == maskController.GetCurrentMaskType())
                continue;


            //LÓGICA DE SELECCIÓN
            float currentDistSqr = (results[i].transform.position - transform.position).sqrMagnitude;
            MaskType currentMaskType = mask.GetMaskType();


            //Buscamos un tipo específico
            if (specificType.HasValue)
            {
                if (currentMaskType == specificType.Value)
                {
                    if (currentDistSqr < closestDistSqr)
                    {
                        closestDistSqr = currentDistSqr;
                        bestMask = maskObj;
                    }
                }
            }

            //Buscamos la mejor disponible
            else
            {

                bool isPriority = (counterType != MaskType.None && currentMaskType == counterType);

                // ¿Cuándo cambiamos nuestra 'bestMask' por esta nueva 'candidate'?
                if (isPriority && !bestIsPriority)
                {
                    //Encontré oro y antes tenía cobre.
                    closestDistSqr = currentDistSqr;
                    bestMask = maskObj;
                    bestIsPriority = true;
                }
                else if (isPriority == bestIsPriority)
                {
                    //Ambas son del mismo "rango"
                    //Gana la que esté más cerca.
                    if (currentDistSqr < closestDistSqr)
                    {
                        closestDistSqr = currentDistSqr;
                        bestMask = maskObj;
                        //bestIsPriority se mantiene igual
                    }
                }
            }
        }

        return bestMask;
    }

    //Convierte la ruta del NavMesh en inputs de movimiento para el Ragdoll
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

    //Gestiona la transición limpia entre estados
    private void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        OnExitState(currentState);
        currentState = newState;
        OnEnterState(newState);
    }

    //Configuración inicial al entrar a un estado (SetDestination único)
    private void OnEnterState(AIState state)
    {
        switch (state)
        {
            case AIState.ApproachingMask:
                if (navAgent != null && navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = false;

                    //Asignamos el destino UNA VEZ al entrar al estado.
                    if (targetMaskObject != null)
                    {
                        navAgent.SetDestination(targetMaskObject.transform.position);
                    }
                }
                break;

            case AIState.Fleeing:
            case AIState.FollowingPlayer:
                if (navAgent != null) navAgent.isStopped = false;
                break;

            case AIState.Idle:
            case AIState.WaitingForMask:
                if (navAgent != null)
                    navAgent.isStopped = true;
                break;
        }
    }

    //Limpieza al salir de un estado
    private void OnExitState(AIState state)
    {
        //Cleanup si es necesario
    }

    //Dibuja ayudas visuales en el editor para depuración
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