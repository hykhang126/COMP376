using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LightDetector : MonoBehaviour
{
    [Header("References")]
    public Flashlight playerFlashlight;
    public LightStalkerConfig enemyConfig;

    // fired after the object has been lit for flashlightStunSeconds
    public UnityEvent OnScaredByLight;

    // per-enemy immediate enter/exit (no collider param)
    public UnityEvent OnLightEnter;
    public UnityEvent OnLightExit;

    // configured hold time (fallback 3s)
    private float lightHoldTime => enemyConfig != null ? enemyConfig.flashlightStunSeconds : 3f;

    // count of overlapping child colliders currently hit by beam
    private int beamHitCount = 0;

    // accumulating timer & small state to avoid double triggering
    private float lightTimer = 0f;
    private bool hasTriggeredThisExposure = false;
    private bool isUnderBeam => beamHitCount > 0;

    [Header("Tuning (for forgiving occlusions)")]
    [Tooltip("How fast accumulated light time decays per second when not under beam. 0 = no decay, higher = faster loss.")]
    public float lightDecayPerSecond = 0.5f;

    void Start()
    {
        if (playerFlashlight == null)
        {
            playerFlashlight = Object.FindFirstObjectByType<Flashlight>();
            if (playerFlashlight == null)
                Debug.LogWarning($"[LightDetector] No Flashlight found in scene for {name}. Assign playerFlashlight in inspector.");
        }

        if (playerFlashlight != null)
        {
            playerFlashlight.OnBeamEnter.AddListener(HandleBeamEnter);
            playerFlashlight.OnBeamExit.AddListener(HandleBeamExit);
        }
    }

    private void Update()
    {
        if (isUnderBeam)
        {
            // accumulate while in beam (only if we haven't already triggered for this continuous exposure)
            if (!hasTriggeredThisExposure)
            {
                lightTimer += Time.deltaTime;
                if (lightTimer >= lightHoldTime)
                {
                    TriggerScared();
                }
            }
        }
        else
        {
            // slowly decay accumulated time when briefly out of beam
            if (lightTimer > 0f)
            {
                lightTimer = Mathf.Max(0f, lightTimer - lightDecayPerSecond * Time.deltaTime);
                
            }
            // Note: do NOT reset hasTriggeredThisExposure here; reset when beam fully exits in HandleBeamExit
        }
    }

    private void HandleBeamEnter(Collider col)
    {
        if (!IsColliderForThisObject(col)) return;
        if (!gameObject.activeInHierarchy) return;

        // Increase counter (multiple child colliders -> multiple enter calls)
        beamHitCount = Mathf.Max(0, beamHitCount) + 1;
        if (beamHitCount == 1)
        {
            // first time entering the beam for this enemy
            OnLightEnter?.Invoke();
        }

        // nothing else to do here — accumulation happens in Update()
    }

    private void HandleBeamExit(Collider col)
    {
        if (!IsColliderForThisObject(col)) return;

        beamHitCount = Mathf.Max(0, beamHitCount - 1);
        if (beamHitCount == 0)
        {
            // fully exited beam
            OnLightExit?.Invoke();

            // Allow future exposures to trigger again
            hasTriggeredThisExposure = false;

            // Optionally keep accumulated lightTimer (it will decay slowly).
            // If you prefer instant reset on full exit, uncomment next line:
            // lightTimer = 0f;
        }

        // no Invoke/CancelInvoke anymore
    }

    private bool IsColliderForThisObject(Collider col)
    {
        if (col == null) return false;

        if (col.transform == this.transform) return true;
        //if (col.transform.IsChildOf(this.transform)) return true;
        //if (this.transform.IsChildOf(col.transform)) return true;

    return false;
    }

    private void TriggerScared()
    {
        if (!gameObject.activeInHierarchy) return;
        if (hasTriggeredThisExposure) return; // guard

        OnScaredByLight?.Invoke();

        // Mark that we've already triggered for this continuous exposure so we don't spam the event
        hasTriggeredThisExposure = true;

        // reset accumulated time so we don't immediately re-trigger (optional)
        lightTimer = 0f;
    }

    void OnDisable()
    {
        // Reset counter and fire exit if needed
        if (beamHitCount > 0)
        {
            beamHitCount = 0;
            OnLightExit?.Invoke();
        }

        // Reset state
        lightTimer = 0f;
        hasTriggeredThisExposure = false;
    }

    void OnDestroy()
    {
        if (playerFlashlight != null)
        {
            playerFlashlight.OnBeamEnter.RemoveListener(HandleBeamEnter);
            playerFlashlight.OnBeamExit.RemoveListener(HandleBeamExit);
        }
    }
}
