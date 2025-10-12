using UnityEngine;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(LightDetector))]
public class LightStalkerController : MonoBehaviour
{
    private StateMachine stateMachine;
    private LightDetector lightDetector;
    private Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Enemy Config")]
    public LightStalkerConfig enemyConfig; //Assign the ScriptableObject in Inspector

    [Header("Respawn/Spawners")]
    public Transform[] spawners;

    void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        lightDetector = GetComponent<LightDetector>();
    }

    void Start()
    {
        player = Camera.main?.transform;

        if (lightDetector != null)
            lightDetector.OnScaredByLight.AddListener(OnScaredByLight);
    }

    void OnDestroy()
    {
        if (lightDetector != null)
            lightDetector.OnScaredByLight.RemoveListener(OnScaredByLight);
    }

    private void OnScaredByLight()
    {
        //Tell the state machine to transition
        stateMachine.InvokeStateEvent("ScaredByLight");

        //Notify SpawnerManager to handle respawn (SpawnerManager should accept LightStalker)
        if (SpawnerManager.Instance != null && enemyConfig != null)
        {
            SpawnerManager.Instance.NotifyDespawned(this, enemyConfig.respawnDelay);
        }
    }

    //Called from a ChasingPlayer state in the state machine
    public void MoveTowardPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
    }

    //Called by SpawnerManager when respawning the enemy
    public void RespawnAt(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        gameObject.SetActive(true);
        stateMachine.InvokeStateEvent("Respawned");
    }
}
