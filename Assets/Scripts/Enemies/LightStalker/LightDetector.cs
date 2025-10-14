using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LightDetector : MonoBehaviour
{
    [Header("References")]
    public Flashlight playerFlashlight;
    public LightStalkerConfig enemyConfig;

    public UnityEvent OnScaredByLight;

    //Prevents scheduling the same “scare” timer more than once
    private bool isTimerScheduled = false;
    private float lightHoldTime => enemyConfig != null ? enemyConfig.flashlightStunSeconds : 3f;

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

    private void HandleBeamEnter(Collider col)
    {
        Debug.Log("Beam hit enemy");
        //Accept any collider that is part of this enemy (root or child)
        if (!IsColliderForThisObject(col)) return;

        //Only schedule if object is active and not already scheduled
        if (!gameObject.activeInHierarchy) return;

        if (!isTimerScheduled)
        {
            Invoke(nameof(TriggerScared), lightHoldTime);
            isTimerScheduled = true;
        }
    }

    private void HandleBeamExit(Collider col)
    {
        if (!IsColliderForThisObject(col)) return;

        if (isTimerScheduled)
        {
            CancelInvoke(nameof(TriggerScared));
            isTimerScheduled = false;
        }
    }

    private bool IsColliderForThisObject(Collider col)
    {
        if (col == null) return false;

        // True if collider is exactly this object, a child of this object,
        // or this object is child of the collider (covers collider-on-parent cases).
        if (col.transform == this.transform) return true;
        if (col.transform.IsChildOf(this.transform)) return true;
        if (this.transform.IsChildOf(col.transform)) return true;

        return false;
    }

    private void TriggerScared()
    {
        //Double-check we're still active before invoking
        if (!gameObject.activeInHierarchy) { isTimerScheduled = false; return; }

        OnScaredByLight?.Invoke();
        isTimerScheduled = false;
    }

    void OnDisable()
    {
        //If disabled while scheduled, cancel the pending invoke
        if (isTimerScheduled)
        {
            CancelInvoke(nameof(TriggerScared));
            isTimerScheduled = false;
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
