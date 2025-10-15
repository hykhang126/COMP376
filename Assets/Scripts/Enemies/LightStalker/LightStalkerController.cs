using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(LightDetector))]
[RequireComponent(typeof(NavMeshAgent))]
public class LightStalkerController : MonoBehaviour
{
    private StateMachine stateMachine;
    private LightDetector lightDetector;
    private Transform player;
    private NavMeshAgent agent;

    [Header("Movement")]
    public float moveSpeed = 3f;
    [Tooltip("Stopping distance to player")]
    public float stoppingDistance = 1.2f;

    [Header("Enemy Config")]
    public LightStalkerConfig enemyConfig;

    [Header("Respawn/Spawners")]
    public Transform[] spawners;

    [Header("Flee behavior (on being lit)")]
    [Tooltip("How long to run away before despawning")]
    public float fleeDuration = 2.5f;
    [Tooltip("Multiplier applied to agent.speed while fleeing")]
    public float fleeSpeedMultiplier = 2.0f;

    [Header("Light effect")]
    [Tooltip("Multiplier applied to agent.speed while the enemy is under the flashlight beam")]
    public float inBeamSpeedMultiplier = 0.25f;
    private bool isInBeam = false;
    private float originalAgentSpeed = 3f;

    // internal state
    private bool isFleeing = false;

    void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        lightDetector = GetComponent<LightDetector>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = Camera.main?.transform;

        if (lightDetector != null)
        {
            lightDetector.OnScaredByLight.AddListener(OnScaredByLight);
            lightDetector.OnLightEnter.AddListener(HandleLightEnter);
            lightDetector.OnLightExit.AddListener(HandleLightExit);
        }

        if (enemyConfig != null)
        {
            moveSpeed = enemyConfig.moveSpeed;
            stoppingDistance = enemyConfig.stoppingDistance;
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            originalAgentSpeed = agent.speed;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = true;
            agent.updateUpAxis = true;
            if (enemyConfig != null) agent.acceleration = enemyConfig.acceleration;
        }

        // TEST: force chase at start for testing scenes (remove in production)
        stateMachine?.InvokeStateEvent("PlayerDetected");
    }

    void OnDestroy()
    {
        if (lightDetector != null)
        {
            lightDetector.OnScaredByLight.RemoveListener(OnScaredByLight);
            lightDetector.OnLightEnter.RemoveListener(HandleLightEnter);
            lightDetector.OnLightExit.RemoveListener(HandleLightExit);
        }

        CancelInvoke(nameof(CompleteFleeAndDespawn));
    }

    private void OnScaredByLight()
    {
        // Transition into Scared state (inspector should wire Scared state's stateEnter to StartFlee)
        stateMachine?.InvokeStateEvent("ScaredByLight");

        // NOTE: DO NOT call SpawnerManager.NotifyDespawned() here immediately.
        // We will call it after the flee duration completes inside CompleteFleeAndDespawn().
    }

    //Slowws enemy when in light.
    private void HandleLightEnter()
    {
        if (isFleeing) return;
        isInBeam = true;
        if (agent != null)
            agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed) * inBeamSpeedMultiplier;
    }

    //Return enemy to normal speed
    private void HandleLightExit()
    {
        if (isFleeing) return;
        isInBeam = false;
        if (agent != null)
            agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed);
    }

    // Called by the StateMachine's 'Scared' state's stateEnter event
    public void StartFlee()
    {
        if (isFleeing) return;
        isFleeing = true;

        // ensure agent exists and override speed for fleeing
        if (agent != null)
        {
            agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed) * fleeSpeedMultiplier;
            agent.isStopped = false;
        }

        if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;
        if (agent == null)
        {
            transform.rotation = Quaternion.LookRotation((transform.position - (player != null ? player.position : transform.position + transform.forward)).normalized);
            Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
            return;
        }

        Vector3 awayDir;
        if (player != null)
        {
            awayDir = (transform.position - player.position);
            awayDir.y = 0f;
            if (awayDir.sqrMagnitude < 0.1f) awayDir = -transform.forward;
        }
        else awayDir = transform.forward;

        // choose the greater of configured flee distance, or the distance agent can run for fleeDuration
        float fleeDistance = Mathf.Max((enemyConfig != null) ? enemyConfig.fleeDistanceWhenIlluminated : 6f,
                              (agent != null ? agent.speed : moveSpeed) * fleeSpeedMultiplier * fleeDuration);

        // Search candidates around a circle
        NavMeshHit navHit;
        NavMeshPath bestPath = null;
        Vector3 bestPos = transform.position;
        float bestPathLength = -1f;

        int sampleCount = 16; // tune: more samples => better chance to find escape
        float angleStep = 360f / sampleCount;

        for (int i = 0; i < sampleCount; i++)
        {
            float angle = i * angleStep;
            // Build candidate direction by rotating awayDir by angle
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            Vector3 candDir = rot * awayDir.normalized;
            if (candDir.sqrMagnitude < 0.001f) candDir = Quaternion.Euler(0, angle, 0) * transform.forward;

            Vector3 candWorld = transform.position + candDir * fleeDistance;

            // Snap to navmesh near candidate
            if (NavMesh.SamplePosition(candWorld, out navHit, Mathf.Max(1f, fleeDistance * 0.5f), NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    // compute path length
                    float len = 0f;
                    for (int p = 1; p < path.corners.Length; p++) len += Vector3.Distance(path.corners[p - 1], path.corners[p]);

                    // prefer longest path (furthest reachable)
                    if (len > bestPathLength)
                    {
                        bestPathLength = len;
                        bestPath = path;
                        bestPos = navHit.position;
                    }
                }
            }
        }

        // If we found a valid path, use it
        if (bestPath != null && bestPathLength > 0f)
        {
            agent.speed = moveSpeed * fleeSpeedMultiplier;
            agent.isStopped = false;
            agent.SetPath(bestPath);
        }
        else
        {
            // Try the easy fallback: sample the closest navmesh edge (if available)
            if (NavMesh.FindClosestEdge(transform.position, out navHit, NavMesh.AllAreas))
            {
                // attempt to run to a point slightly further along the edge normal
                Vector3 fallbackTarget = transform.position + navHit.normal * (fleeDistance * 0.5f);
                if (NavMesh.SamplePosition(fallbackTarget, out navHit, 5f, NavMesh.AllAreas))
                {
                    NavMeshPath fallbackPath = new NavMeshPath();
                    if (NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, fallbackPath) && fallbackPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.speed = moveSpeed * fleeSpeedMultiplier;
                        agent.isStopped = false;
                        agent.SetPath(fallbackPath);
                        // schedule despawn as usual
                        CancelInvoke(nameof(CompleteFleeAndDespawn));
                        Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
                        return;
                    }
                }
            }

            // Last resort: cannot find any reachable flee spot. stop and schedule despawn.
            agent.isStopped = true;
        }

        CancelInvoke(nameof(CompleteFleeAndDespawn));
        Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
    }

    // Called when fleeing time completes
    private void CompleteFleeAndDespawn()
    {
        isFleeing = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed);
        }

        // Use SpawnerManager to handle deactivation and respawn (same as before)
        if (SpawnerManager.Instance != null && enemyConfig != null)
        {
            SpawnerManager.Instance.NotifyDespawned(this, enemyConfig.respawnDelay);
        }
        else
        {
            // fallback behavior - just deactivate
            gameObject.SetActive(false);
        }
    }

    // Called by state machine's stateUpdate (ChasingPlayer)
    public void MoveTowardPlayer()
    {
        if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;
        if (player == null || agent == null) return;

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        if (sqrDist <= (stoppingDistance * stoppingDistance))
        {
            if (!agent.isStopped) agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // Optional: called by state enter when starting the chase
    public void OnEnterChase()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
    }

    // Optional: called by state exit when leaving chase
    public void OnExitChase()
    {
        if (agent != null)
            agent.isStopped = true;
    }

    // Called by SpawnerManager when respawning the enemy
    public void RespawnAt(Vector3 spawnPosition)
    {
        // Place at spawn location and enable agent
        transform.position = spawnPosition;
        gameObject.SetActive(true);

        if (agent != null)
        {
            agent.Warp(spawnPosition);
            agent.isStopped = false;
            agent.speed = moveSpeed;
        }

        stateMachine?.InvokeStateEvent("Respawned");
    }

    void OnDisable()
    {
        // Ensure scheduled calls are canceled if the object is disabled
        CancelInvoke(nameof(CompleteFleeAndDespawn));
        isFleeing = false;
    }
}
