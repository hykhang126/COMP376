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
            lightDetector.OnScaredByLight.AddListener(OnScaredByLight);

        
        if (enemyConfig != null)
        {
            moveSpeed = enemyConfig.moveSpeed;
            stoppingDistance = enemyConfig.stoppingDistance;
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = true;
            agent.updateUpAxis = true;
            // other agent tuning: agent.acceleration = enemyConfig.acceleration; etc
            if (enemyConfig != null) agent.acceleration = enemyConfig.acceleration;
        }

        // TEST: force chase at start for testing scenes (remove in production)
        stateMachine?.InvokeStateEvent("PlayerDetected");
    }

    void OnDestroy()
    {
        if (lightDetector != null)
            lightDetector.OnScaredByLight.RemoveListener(OnScaredByLight);
    }

    private void OnScaredByLight()
    {
        // stop agent and transition state
        if (agent != null) agent.isStopped = true;
        stateMachine?.InvokeStateEvent("ScaredByLight");

        if (SpawnerManager.Instance != null && enemyConfig != null)
        {
            SpawnerManager.Instance.NotifyDespawned(this, enemyConfig.respawnDelay);
        }
    }

    // Called by state machine's stateUpdate
    public void MoveTowardPlayer()
    {
        if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;
        if (player == null || agent == null) return;

        // If we are already too close, stop
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
        }

        stateMachine?.InvokeStateEvent("Respawned");
    }
}
