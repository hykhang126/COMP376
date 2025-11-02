using System.Collections;
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
  [Tooltip("Multiply the computed flee distance (1 = unchanged, 4 = four times as far)")]
  public float fleeDistanceMultiplier = 4f;
  [Header("Light effect")]
    [Tooltip("Multiplier applied to agent.speed while the enemy is under the flashlight beam")]
    public float inBeamSpeedMultiplier = 0.25f;
    public float originalAgentSpeed = 3f;
    private bool isInBeam = false;

    // internal state
    private bool isFleeing = false;

    [Header("Proximity Audio (optional)")]
    [Tooltip("Optional ProximityTrigger child (if null it will search in children)")]
    public ProximityTrigger proximityTriggerOverride;

    [Tooltip("Hush (outer) audio clip")]
    public AudioClip hushClip;
    [Tooltip("Whisper (inner) audio clip")]
    public AudioClip whisperClip;

    [Range(0f,1f)] public float hushVolume = 0.25f;
    [Range(0f,1f)] public float whisperVolume = 0.85f;
    [Tooltip("Seconds to crossfade between volumes")]
    public float crossfadeTime = 0.25f;
    [Tooltip("Seconds between proximity checks while player is inside trigger")]
    public float proximityCheckInterval = 0.18f;
    [Tooltip("If true, raycast checks will reduce volume when occluded")]
    public bool useOcclusionRaycast = true;
    [Tooltip("Layers considered occluding")]
    public LayerMask occlusionMask = ~0;

    [Header("Flee SFX")]
    [Tooltip("Screech sound played once the enemy begins fleeing")]
    public AudioClip fleeScreechClip;
    [Range(0f,1f)]
    public float fleeScreechVolume = 1f;

    // runtime
    private AudioSource hushSource;
    private AudioSource whisperSource;
    private Coroutine proximityCoroutine;
    private bool playerInsideProximity = false;
    private ProximityTrigger proxTrigger;
    
    // colliders considered "body" / capable of directly touching the player
    private Collider[] bodyColliders;



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

        // Proximity trigger (optional)
        proxTrigger = proximityTriggerOverride ?? GetComponentInChildren<ProximityTrigger>();
        if (proxTrigger != null)
        {
            proxTrigger.OnPlayerEnter.AddListener(OnPlayerEnteredProximity);
            proxTrigger.OnPlayerExit.AddListener(OnPlayerExitedProximity);
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

        CreateAudioSources();

        // populate bodyColliders: all non-trigger colliders under this enemy, excluding the proximity trigger collider.
        Collider proxCol = (proxTrigger != null) ? proxTrigger.GetComponent<Collider>() : null;
        var all = GetComponentsInChildren<Collider>(true);
        var list = new System.Collections.Generic.List<Collider>(all.Length);
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c == proxCol) continue;          // exclude proximity trigger collider
            if (c.isTrigger) continue;           // exclude any trigger colliders (we only want physical body colliders)
            list.Add(c);
        }
        bodyColliders = list.ToArray();
        // END body collider initialization
    }

    void OnDestroy()
    {
        if (lightDetector != null)
        {
            lightDetector.OnScaredByLight.RemoveListener(OnScaredByLight);
            lightDetector.OnLightEnter.RemoveListener(HandleLightEnter);
            lightDetector.OnLightExit.RemoveListener(HandleLightExit);
        }

        if (proxTrigger != null)
        {
            proxTrigger.OnPlayerEnter.RemoveListener(OnPlayerEnteredProximity);
            proxTrigger.OnPlayerExit.RemoveListener(OnPlayerExitedProximity);
        }

        CancelInvoke(nameof(CompleteFleeAndDespawn));
        StopProximityCoroutine();
    }

    private void OnScaredByLight()
    {
        // Transition into Scared state (inspector should wire Scared state's stateEnter to StartFlee)
        stateMachine?.InvokeStateEvent("ScaredByLight");
    }

    //Slows enemy when in light.
    private void HandleLightEnter()
    {
        if (isFleeing) return;
        isInBeam = true;
        Debug.Log("LightStalker: In Beam: " + isInBeam);
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

    //-----OLD FLEE METHOD (Radii Calculations). ISSUES IN BASELEVELSCENE, WORKS IN LARGE OPEN-----
    // Called by the StateMachine's 'Scared' state's stateEnter event
    // public void StartFlee()
    // {
    //     if (isFleeing) return;
    //     isFleeing = true;

    //     // Play flee screech
    //     if (fleeScreechClip != null)
    //     {
    //         AudioSource.PlayClipAtPoint(fleeScreechClip, transform.position, Mathf.Clamp01(fleeScreechVolume));
    //     }

    //     // stop proximity audio while fleeing
    //     playerInsideProximity = false;
    //     StopProximityCoroutine();

    //     // ensure agent exists and override speed for fleeing
    //     if (agent != null)
    //     {
    //         agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed) * fleeSpeedMultiplier;
    //         agent.isStopped = false;
    //     }

    //     if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;
    //     if (agent == null)
    //     {
    //         transform.rotation = Quaternion.LookRotation((transform.position - (player != null ? player.position : transform.position + transform.forward)).normalized);
    //         Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
    //         return;
    //     }

    //     Vector3 awayDir;
    //     if (player != null)
    //     {
    //         awayDir = (transform.position - player.position);
    //         awayDir.y = 0f;
    //         if (awayDir.sqrMagnitude < 0.1f) awayDir = -transform.forward;
    //     }
    //     else awayDir = transform.forward;

    //     // NOTE: use the configured moveSpeed (not agent.speed which may have been modified)
    //     float movementSpeed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed);

    //     // choose the greater of configured flee distance, or the distance agent can run for fleeDuration
    //     float baseFleeDistance = Mathf.Max(
    //         (enemyConfig != null) ? enemyConfig.fleeDistanceWhenIlluminated : 6f,
    //         movementSpeed * fleeSpeedMultiplier * fleeDuration);

    //     // apply multiplier (allows easy tuning from inspector)
    //     float fleeDistance = Mathf.Max(0.01f, baseFleeDistance * fleeDistanceMultiplier);

    //     // Search candidates around a circle
    //     NavMeshHit navHit;
    //     NavMeshPath bestPath = null;
    //     Vector3 bestPos = transform.position;
    //     float bestPathLength = -1f;

    //     // increase sampleCount slightly so we have more candidate directions in tight spaces
    //     int sampleCount = 50; // increased from 16
    //     float angleStep = 360f / sampleCount;

    //     // allow larger sample radius for bigger flee distances — don't clamp to tiny 5m ceiling
    //     float sampleRadius = Mathf.Clamp(fleeDistance * 0.5f, 1f, Mathf.Max(5f, fleeDistance * 0.5f));

    //     for (int i = 0; i < sampleCount; i++)
    //     {
    //         float angle = i * angleStep;
    //         Quaternion rot = Quaternion.Euler(0f, angle, 0f);
    //         Vector3 candDir = rot * awayDir.normalized;
    //         if (candDir.sqrMagnitude < 0.001f) candDir = Quaternion.Euler(0, angle, 0) * transform.forward;

    //         Vector3 candWorld = transform.position + candDir * fleeDistance;

    //         // Snap to navmesh near candidate using a capped sample radius (safer on small maps)
    //         if (NavMesh.SamplePosition(candWorld, out navHit, sampleRadius, NavMesh.AllAreas))
    //         {
    //             NavMeshPath path = new NavMeshPath();
    //             bool calc = NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, path);
    //             // Accept either complete paths OR partial paths that actually contain movement (corners > 1)
    //             if (calc && (path.status == NavMeshPathStatus.PathComplete || path.corners.Length > 1))
    //             {
    //                 // compute path length
    //                 float len = 0f;
    //                 for (int p = 1; p < path.corners.Length; p++) len += Vector3.Distance(path.corners[p - 1], path.corners[p]);

    //                 // prefer longest path (furthest reachable)
    //                 if (len > bestPathLength)
    //                 {
    //                     bestPathLength = len;
    //                     bestPath = path;
    //                     bestPos = navHit.position;
    //                 }
    //             }
    //         }
    //     }

    //     if (bestPath != null && bestPathLength > 0f)
    //     {
    //         agent.speed = movementSpeed * fleeSpeedMultiplier; // use movementSpeed for consistency
    //         agent.isStopped = false;
    //         agent.SetPath(bestPath);

    //         // schedule despawn based on travel time so the enemy disappears shortly after reaching the flee spot.
    //         // This prevents the player following it all the way and seeing the despawn.
    //         float travelTime = bestPathLength / Mathf.Max(0.001f, agent.speed);
    //         float despawnAfter = Mathf.Min(fleeDuration, travelTime + 0.35f); // 0.35s slack so it finishes moving
    //         CancelInvoke(nameof(CompleteFleeAndDespawn));
    //         Invoke(nameof(CompleteFleeAndDespawn), despawnAfter);
    //         return; // done
    //     }
    //     else
    //     {
    //         // Fallback 1: try a closer one-step sample in the away direction (helps tiny rooms)
    //         Vector3 fallbackCandidate = transform.position + awayDir.normalized * Mathf.Min(fleeDistance * 0.5f, 3f);
    //         if (NavMesh.SamplePosition(fallbackCandidate, out navHit, 2f, NavMesh.AllAreas))
    //         {
    //             NavMeshPath fallbackPath = new NavMeshPath();
    //             bool calc = NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, fallbackPath);
    //             if (calc && (fallbackPath.status == NavMeshPathStatus.PathComplete || fallbackPath.corners.Length > 1))
    //             {
    //                 // compute path length
    //                 float len = 0f;
    //                 for (int p = 1; p < fallbackPath.corners.Length; p++) len += Vector3.Distance(fallbackPath.corners[p - 1], fallbackPath.corners[p]);

    //                 agent.speed = movementSpeed * fleeSpeedMultiplier;
    //                 agent.isStopped = false;
    //                 agent.SetPath(fallbackPath);

    //                 float travelTime = len / Mathf.Max(0.001f, agent.speed);
    //                 float despawnAfter = Mathf.Min(fleeDuration, travelTime + 0.35f);
    //                 CancelInvoke(nameof(CompleteFleeAndDespawn));
    //                 Invoke(nameof(CompleteFleeAndDespawn), despawnAfter);
    //                 return;
    //             }
    //         }

    //         // Original fallback: try closest navmesh edge path (kept, but with smaller sample radius)
    //         if (NavMesh.FindClosestEdge(transform.position, out navHit, NavMesh.AllAreas))
    //         {
    //             Vector3 fallbackTarget = transform.position + navHit.normal * (fleeDistance * 0.5f);
    //             if (NavMesh.SamplePosition(fallbackTarget, out navHit, 2f, NavMesh.AllAreas))
    //             {
    //                 NavMeshPath fallbackPath = new NavMeshPath();
    //                 if (NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, fallbackPath)
    //                     && (fallbackPath.status == NavMeshPathStatus.PathComplete || fallbackPath.corners.Length > 1))
    //                 {
    //                     // compute path length
    //                     float len = 0f;
    //                     for (int p = 1; p < fallbackPath.corners.Length; p++) len += Vector3.Distance(fallbackPath.corners[p - 1], fallbackPath.corners[p]);

    //                     agent.speed = movementSpeed * fleeSpeedMultiplier;
    //                     agent.isStopped = false;
    //                     agent.SetPath(fallbackPath);

    //                     float travelTime = len / Mathf.Max(0.001f, agent.speed);
    //                     float despawnAfter = Mathf.Min(fleeDuration, travelTime + 0.35f);
    //                     CancelInvoke(nameof(CompleteFleeAndDespawn));
    //                     Invoke(nameof(CompleteFleeAndDespawn), despawnAfter);
    //                     return;
    //                 }
    //             }
    //         }

    //         // Last resort: cannot find any reachable flee spot. stop and schedule despawn.
    //         agent.isStopped = true;
    //     }

    //     // If we fall through here (no path found), keep the original behavior to despawn after fleeDuration
    //     CancelInvoke(nameof(CompleteFleeAndDespawn));
    //     Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
    // }

    //--NEW FLEE METHOD. WORKS BUT HAS SOME WEIRD EDGE CASE BEHAVIOUR, MOST LIKELY NAVMESH ISSUE--
    // Called by the StateMachine's 'Scared' state's stateEnter event
    public void StartFlee()
    {
        if (isFleeing) return;
        isFleeing = true;

        // Play flee screech
        if (fleeScreechClip != null)
        {
            AudioSource.PlayClipAtPoint(fleeScreechClip, transform.position, Mathf.Clamp01(fleeScreechVolume));
        }

        // stop proximity audio while fleeing
        playerInsideProximity = false;
        StopProximityCoroutine();

        // If we don't have an agent, just rotate away and schedule a normal despawn.
        if (agent == null)
        {
            if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;
            transform.rotation = Quaternion.LookRotation((transform.position - (player != null ? player.position : transform.position + transform.forward)).normalized);
            CancelInvoke(nameof(CompleteFleeAndDespawn));
            Invoke(nameof(CompleteFleeAndDespawn), fleeDuration);
            return;
        }

        // Ensure we have player reference
        if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;

        // compute consistent movement speed (use config if present)
        float movementSpeed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed);
        agent.speed = movementSpeed * fleeSpeedMultiplier;
        agent.isStopped = false;

        // --- Try to pick the furthest reachable spawner (furthest from the player) ---
        Transform bestSpawner = null;
        NavMeshPath bestSpawnerPath = null;
        float bestPlayerDist = -1f;      // maximize distance from player
        float bestSpawnerPathLen = -1f;  // store path length for scheduling

        if (spawners != null && spawners.Length > 0 && player != null)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                var s = spawners[i];
                if (s == null) continue;

                // distance from player to this spawner (we prefer larger)
                float playerDist = Vector3.Distance(player.position, s.position);

                // find a navmesh point near the spawn (small radius)
                NavMeshHit sampleHit;
                if (!NavMesh.SamplePosition(s.position, out sampleHit, 2f, NavMesh.AllAreas))
                {
                    // skip spawners that aren't on/near the navmesh
                    continue;
                }

                // calculate path from current position to sampleHit.position
                NavMeshPath path = new NavMeshPath();
                bool calc = NavMesh.CalculatePath(transform.position, sampleHit.position, NavMesh.AllAreas, path);
                if (!calc) continue;
                if (path.status != NavMeshPathStatus.PathComplete && path.corners.Length <= 1) continue;

                // compute path length
                float len = 0f;
                for (int p = 1; p < path.corners.Length; p++) len += Vector3.Distance(path.corners[p - 1], path.corners[p]);

                // choose the spawner farthest from player (tie-breaker: longer path)
                bool prefer = false;
                if (playerDist > bestPlayerDist + 0.01f) prefer = true;
                else if (Mathf.Approximately(playerDist, bestPlayerDist) && len > bestSpawnerPathLen + 0.01f) prefer = true;

                if (prefer)
                {
                    bestPlayerDist = playerDist;
                    bestSpawner = s;
                    bestSpawnerPath = path;
                    bestSpawnerPathLen = len;
                }
            }
        }

        // scheduling safety and arrival parameters
        float slackAfterTravel = 0.35f;
        float minDespawnTime = 0.9f;
        float arrivalThreshold = 0.6f; // how close to dest we require before despawning (tweak)

        if (bestSpawner != null && bestSpawnerPath != null && bestSpawnerPathLen > 0.01f)
        {
            // Found a reachable spawner. Flee to it.
            agent.isStopped = false;
            agent.SetPath(bestSpawnerPath);

            // compute travelTime and cap into reasonable bounds for max wait
            float travelTime = bestSpawnerPathLen / Mathf.Max(0.001f, agent.speed);
            float maxWait = Mathf.Clamp(travelTime + slackAfterTravel, minDespawnTime, Mathf.Max(fleeDuration, travelTime + slackAfterTravel));

            // Destination is the last corner of the path
            Vector3 destination = bestSpawnerPath.corners[bestSpawnerPath.corners.Length - 1];

            // Start coroutine that waits for arrival or timeout, then despawns.
            StartCoroutine(WaitForArrivalAndDespawn(maxWait, destination, arrivalThreshold));
            return;
        }

        // --- NO reachable spawner found: instant despawn (no circular sampling fallback) ---
        Debug.Log($"[StartFlee] No reachable spawner found for {name} — despawning immediately.");
        CompleteFleeAndDespawn();
    }
    private IEnumerator WaitForArrivalAndDespawn(float maxWait, Vector3 destination, float arrivalThreshold)
    {
        float t = 0f;
        // small safety: ensure we give navmesh path time to be set
        yield return null;

        while (t < maxWait)
        {
            t += Time.deltaTime;

            // If agent has no path anymore, break (it might have been stopped)
            if (agent == null) break;

            // If path is pending, skip checking this frame
            if (agent.pathPending)
            {
                yield return null;
                continue;
            }

            // If agent has a valid remaining distance, check it.
            // Use either remainingDistance (if available) or direct distance to destination as fallback.
            float remaining = (agent.hasPath) ? agent.remainingDistance : Vector3.Distance(agent.transform.position, destination);

            // also check direct distance to destination in case remainingDistance is unreliable
            float directDist = Vector3.Distance(agent.transform.position, destination);
            float checkDist = Mathf.Min(remaining, directDist);

            if (checkDist <= arrivalThreshold)
            {
                // Close enough to dest — despawn now (or optionally delay a tiny bit)
                CompleteFleeAndDespawn();
                yield break;
            }

            yield return null;
        }

        // Timeout reached — despawn anyway to avoid stuck agents
        CompleteFleeAndDespawn();
    }


    //--FALLBACK FLEE METHOD (Instant despawn). USE IF OTHER 2 DONT WORK---
    // private void StartFlee()
    // {
    //     Debug.Log($"[StartFlee] No reachable spawner found for {name} — despawning immediately.");
    //     // Play flee screech
    //     if (fleeScreechClip != null)
    //     {
    //        AudioSource.PlayClipAtPoint(fleeScreechClip, transform.position, Mathf.Clamp01(fleeScreechVolume));
    //     }
    //     CompleteFleeAndDespawn();
    // }

  // Called when fleeing time completes
  private void CompleteFleeAndDespawn()
    {
        isFleeing = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = (enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed);
        }

        if (SpawnerManager.Instance != null && enemyConfig != null)
        {
            SpawnerManager.Instance.NotifyDespawned(this, enemyConfig.respawnDelay);
        }
        else
        {
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
        transform.position = spawnPosition;
        gameObject.SetActive(true);

        if (agent != null)
        {
            agent.Warp(spawnPosition);
            agent.isStopped = false;
            agent.speed = moveSpeed;
        }

        // Re-setup proximity/audio/body colliders after respawn
        ReinitializeProximityAndBodies();

        stateMachine?.InvokeStateEvent("Respawned");
    }

    void OnDisable()
    {
        // Ensure scheduled calls are canceled if the object is disabled
        CancelInvoke(nameof(CompleteFleeAndDespawn));
        isFleeing = false;
        StopProximityCoroutine();
    }

    // -------------------------
    // Proximity audio functions
    // -------------------------
    void CreateAudioSources()
    {
        if (hushClip != null && hushSource == null)
        {
            hushSource = gameObject.AddComponent<AudioSource>();
            hushSource.clip = hushClip;
            hushSource.loop = true;
            hushSource.playOnAwake = false;
            hushSource.spatialBlend = 1f;
            hushSource.rolloffMode = AudioRolloffMode.Logarithmic;
            hushSource.minDistance = 1f;
            hushSource.maxDistance = (enemyConfig != null) ? enemyConfig.terrorSoundStartDistance : 20f;
            hushSource.volume = 0f;
            hushSource.Play();
        }

        if (whisperClip != null && whisperSource == null)
        {
            whisperSource = gameObject.AddComponent<AudioSource>();
            whisperSource.clip = whisperClip;
            whisperSource.loop = true;
            whisperSource.playOnAwake = false;
            whisperSource.spatialBlend = 1f;
            whisperSource.rolloffMode = AudioRolloffMode.Logarithmic;
            whisperSource.minDistance = 0.5f;
            whisperSource.maxDistance = (enemyConfig != null) ? enemyConfig.terrorRadius : 12f;
            whisperSource.volume = 0f;
            whisperSource.Play();
        }
    }

    private void OnPlayerEnteredProximity()
    {
        playerInsideProximity = true;
        StartProximityCoroutine();
    }

    private void OnPlayerExitedProximity()
    {
        playerInsideProximity = false;
        StopProximityCoroutine();
    }

    private void StartProximityCoroutine()
    {
        if (proximityCoroutine == null)
            proximityCoroutine = StartCoroutine(ProximityAudioRoutine());
    }

    private void StopProximityCoroutine()
    {
        // Stop the background proximity loop
        if (proximityCoroutine != null)
        {
            StopCoroutine(proximityCoroutine);
            proximityCoroutine = null;
        }

        // If this object is active, start a fade-out coroutine so audio fades out smoothly.
        // If it's already inactive (or being deactivated), avoid StartCoroutine and immediately stop audio.
        if (this.isActiveAndEnabled)
        {
            StartCoroutine(FadeOutAndStopBoth(0.2f));
        }
        else
        {
            if (hushSource != null)
            {
                hushSource.volume = 0f;
                hushSource.Stop();
            }
            if (whisperSource != null)
            {
                whisperSource.volume = 0f;
                whisperSource.Stop();
            }
        }
    }

    private IEnumerator ProximityAudioRoutine()
    {
        if (player == null) player = Camera.main?.transform ?? Object.FindFirstObjectByType<Player>()?.transform;

        while (playerInsideProximity)
        {
            if (player == null) yield return new WaitForSeconds(proximityCheckInterval);
            else
            {
                float sqr = (player.position - transform.position).sqrMagnitude;
                float outerR = (enemyConfig != null) ? enemyConfig.terrorSoundStartDistance : 20f;
                float innerR = (enemyConfig != null) ? enemyConfig.terrorRadius : 12f;
                float outer_sqr = outerR * outerR;
                float inner_sqr = innerR * innerR;

                bool inOuter = sqr <= outer_sqr;
                bool inInner = sqr <= inner_sqr;

                float occlusionFactor = 1f;
                if (useOcclusionRaycast && player != null)
                {
                    Vector3 src = transform.position + Vector3.up * 0.5f;
                    Vector3 dir = (player.position - src);
                    float dist = dir.magnitude;
                    dir /= Mathf.Max(dist, 0.0001f);
                    if (Physics.Raycast(src, dir, out RaycastHit hit, dist, occlusionMask))
                    {
                        occlusionFactor = 0.35f;
                    }
                }

                float targetHush = (inOuter && !inInner) ? hushVolume * occlusionFactor : 0f;
                float targetWhisper = inInner ? whisperVolume * occlusionFactor : 0f;

                if (hushSource != null)
                    hushSource.volume = Mathf.MoveTowards(hushSource.volume, targetHush, Time.deltaTime / Mathf.Max(0.0001f, crossfadeTime));
                if (whisperSource != null)
                    whisperSource.volume = Mathf.MoveTowards(whisperSource.volume, targetWhisper, Time.deltaTime / Mathf.Max(0.0001f, crossfadeTime));

                yield return new WaitForSeconds(proximityCheckInterval);
            }
        }
    }

    private IEnumerator FadeOutAndStopBoth(float fadeTime)
    {
        float t = 0f;
        float startH = (hushSource != null) ? hushSource.volume : 0f;
        float startW = (whisperSource != null) ? whisperSource.volume : 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeTime);
            if (hushSource != null) hushSource.volume = Mathf.Lerp(startH, 0f, a);
            if (whisperSource != null) whisperSource.volume = Mathf.Lerp(startW, 0f, a);
            yield return null;
        }

        if (hushSource != null)
        {
            hushSource.volume = 0f;
            hushSource.Stop();
        }
        if (whisperSource != null)
        {
            whisperSource.volume = 0f;
            whisperSource.Stop();
        }
    }

    // -------------------------
    // Player contact / jumpscare placeholder
    // -------------------------
    // Fires when the enemy physically touches the player (Collision)
    void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.collider == null) return;
        if (!collision.collider.CompareTag("Player")) return;

        // Only treat as a "touch" if the player's collider is actually overlapping/touching a body collider (not the proximity trigger)
        if (IsPlayerTouchingBody(collision.collider))
            HandlePlayerTouch(collision.collider);
    }

    // Fires when a trigger touches the player (Trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        // OnTrigger events may be fired from the proximity trigger. Ensure the player is actually touching a body collider.
        if (IsPlayerTouchingBody(other))
            HandlePlayerTouch(other);
    }

    // Centralized touch handling — placeholder: log a jumpscare event.
    // Does nothing if the stalker is currently fleeing.
    private void HandlePlayerTouch(Collider playerCollider)
    {
        if (isFleeing) return; // fleeing enemies cannot kill/damage

        // Placeholder: substitute actual damage/jumpscare logic here.
        Debug.Log($"{name}: Player touched! (jumpscare placeholder)");
    }

    // Determine whether the provided player collider is actually touching one of the non-trigger body colliders of this enemy.
    // Uses ComputePenetration first (accurate overlap check), and a ClosestPoint distance fallback for near-contact.
    private bool IsPlayerTouchingBody(Collider playerCollider)
    {
        if (playerCollider == null || bodyColliders == null || bodyColliders.Length == 0) return false;

        // First try ComputePenetration for each body collider
        foreach (var bc in bodyColliders)
        {
            if (bc == null) continue;

            // If colliders overlap (ComputePenetration returns true), treat as touching
            if (Physics.ComputePenetration(
                bc, bc.transform.position, bc.transform.rotation,
                playerCollider, playerCollider.transform.position, playerCollider.transform.rotation,
                out Vector3 outDir, out float outDistance))
            {
                return true;
            }

            // fallback: if closest point on body collider to player's collider bounds is essentially at/inside player's collider position -> touching
            Vector3 playerClosest = playerCollider.ClosestPoint(bc.transform.position);
            Vector3 bodyClosest = bc.ClosestPoint(playerCollider.transform.position);
            float dist = Vector3.Distance(playerClosest, bodyClosest);
            // small threshold (tweak if needed). This detects near-contact even without penetration.
            if (dist < 0.1f) return true;
        }

        return false;
    }

    // -------------------------
    // Helpers for respawn re-initialization
    // -------------------------
    private void ReinitializeProximityAndBodies()
    {
        // Re-find/attach prox trigger references & listeners
        // Remove listeners first to avoid duplicates
        ProximityTrigger newProx = proximityTriggerOverride ?? GetComponentInChildren<ProximityTrigger>();
        if (proxTrigger != null && proxTrigger != newProx)
        {
            // remove old listeners if proxTrigger changed
            proxTrigger.OnPlayerEnter.RemoveListener(OnPlayerEnteredProximity);
            proxTrigger.OnPlayerExit.RemoveListener(OnPlayerExitedProximity);
        }

        proxTrigger = newProx;
        if (proxTrigger != null)
        {
            // ensure we don't double-subscribe
            proxTrigger.OnPlayerEnter.RemoveListener(OnPlayerEnteredProximity);
            proxTrigger.OnPlayerExit.RemoveListener(OnPlayerExitedProximity);

            proxTrigger.OnPlayerEnter.AddListener(OnPlayerEnteredProximity);
            proxTrigger.OnPlayerExit.AddListener(OnPlayerExitedProximity);
        }

        // Ensure audio sources exist (CreateAudioSources will only create if null)
        CreateAudioSources();

        // If audio sources exist but were stopped, restart them so fades work
        if (hushSource != null && !hushSource.isPlaying)
            hushSource.Play();
        if (whisperSource != null && !whisperSource.isPlaying)
            whisperSource.Play();

        // Rebuild bodyColliders array (exclude proximity trigger collider)
        Collider proxCol = (proxTrigger != null) ? proxTrigger.GetComponent<Collider>() : null;
        var all = GetComponentsInChildren<Collider>(true);
        var list = new System.Collections.Generic.List<Collider>(all.Length);
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c == proxCol) continue;
            if (c.isTrigger) continue;
            list.Add(c);
        }
        bodyColliders = list.ToArray();

        // If player is currently inside the proximity collider at the moment of respawn,
        // Unity won't fire OnTriggerEnter, so manually start proximity audio now.
        if (proxTrigger != null && player != null)
        {
            var proxColComp = proxTrigger.GetComponent<Collider>();
            if (proxColComp != null)
            {
                // ClosestPoint returns the same point as player.position if inside the trigger
                Vector3 closest = proxColComp.ClosestPoint(player.position);
                float dist = Vector3.Distance(closest, player.position);
                if (dist < 0.01f)
                {
                    // player is inside the respawned proximity trigger -> start proximity behavior
                    OnPlayerEnteredProximity();
                }
            }
        }
    }
}
