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

    // Prevents scheduling the same “scare” timer more than once
    private bool isTimerScheduled = false;
    private float lightHoldTime => enemyConfig != null ? enemyConfig.flashlightStunSeconds : 3f;

    // count of overlapping child colliders currently hit by beam
    private int beamHitCount = 0;

    private float lightTimer = 0f;
    private bool isUnderBeam => beamHitCount > 0;

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
      lightTimer += Time.deltaTime;
      if (!isTimerScheduled && lightTimer >= lightHoldTime)
      {
        TriggerScared();
      }
    }
    else
    {
      // Optional: slowly decay timer if beam is lost (makes it forgiving)
      lightTimer = Mathf.Max(lightTimer - Time.deltaTime * 0.5f, 0f);
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

        if (!isTimerScheduled)
        {
            Invoke(nameof(TriggerScared), lightHoldTime);
            isTimerScheduled = true;
        }
    }

    private void HandleBeamExit(Collider col)
    {
        if (!IsColliderForThisObject(col)) return;

        beamHitCount = Mathf.Max(0, beamHitCount - 1);
        if (beamHitCount == 0)
        {
            // fully exited beam
            OnLightExit?.Invoke();
        }

        if (isTimerScheduled)
        {
            CancelInvoke(nameof(TriggerScared));
            isTimerScheduled = false;
        }
    }

    private bool IsColliderForThisObject(Collider col)
    {
        if (col == null) return false;

        if (col.transform == this.transform) return true;
        if (col.transform.IsChildOf(this.transform)) return true;
        if (this.transform.IsChildOf(col.transform)) return true;

        return false;
    }

  private void TriggerScared()
  {
    if (!gameObject.activeInHierarchy) return;

    OnScaredByLight?.Invoke();
    lightTimer = 0f;
    isTimerScheduled = true; // prevents retrigger until reset
  }

  void OnDisable()
    {
        if (isTimerScheduled)
        {
            CancelInvoke(nameof(TriggerScared));
            isTimerScheduled = false;
        }

        // Reset counter and fire exit if needed
        if (beamHitCount > 0)
        {
            beamHitCount = 0;
            OnLightExit?.Invoke();
        }
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
