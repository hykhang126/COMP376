using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

public class Flashlight : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private GameObject FlashlightLight;
  [SerializeField] public Light FlashlightSpotLight; //Assign if you have a Spot Light child

  [Header("Battery")]
  [SerializeField] private float maxBattery = 20.0f;
  private float TimeLeft;

  [Header("Warnings")]
  [SerializeField] private float FIRST_WARNING_TIME = 10.0f;
  [SerializeField] private float LAST_WARNING_TIME = 5.0f;

  private bool IsActivated = false;
  private bool HasBatteryLeft = true;

  private bool FirstWarningFlag = false;
  private bool LastWarningFlag = false;

  [Header("Events")]
  public UnityEvent OnFirstWarning;
  public UnityEvent OnLastWarning;
  public UnityEvent OnBatteryInsert;
  public UnityEvent OnBatteryEmpty;

  //Minimal beam events for detectors to subscribe
  public ColliderEvent OnBeamEnter = new ColliderEvent();
  public ColliderEvent OnBeamExit = new ColliderEvent();

  //Legacy single-enemy event you had (kept for compatibility if used elsewhere)
  public UnityEvent OnEnemyHit;

  // public read-only API
  public bool IsOn => IsActivated;

  //Raycast helpers
  [Header("Beam Detection")]
  [Tooltip("Raycast range used by CastRay (meters)")]
  public float beamRange = 10f;
  [Tooltip("LayerMask to include enemies (set in inspector)")]
  public LayerMask beamHitMask = ~0;

  // minimal tracking of last hit collider so we can produce Enter/Exit events
  private Collider lastHit = null;

  void Start()
  {
    TimeLeft = maxBattery;

    if (FlashlightLight != null)
      FlashlightLight.SetActive(false);

    if (FlashlightSpotLight != null)
      FlashlightSpotLight.enabled = false;
  }

  void Update()
  {
    HandlePlayerInput();

    if (!HasBatteryLeft) return;

    if (IsActivated)
    {
      // run raycast detection every frame while on
      CastRay();

      TimeLeft -= Time.deltaTime;
      TimeLeft = Mathf.Max(TimeLeft, 0f);

      if (!FirstWarningFlag && TimeLeft < FIRST_WARNING_TIME && TimeLeft >= LAST_WARNING_TIME)
      {
        FirstWarningFlag = true;
        OnFirstWarning?.Invoke();
        Debug.Log("Flashlight first warning");
      }
      else if (!LastWarningFlag && TimeLeft <= LAST_WARNING_TIME && TimeLeft > 0f)
      {
        LastWarningFlag = true;
        OnLastWarning?.Invoke();
        Debug.Log("Flashlight last warning");
      }
      else if (TimeLeft <= 0f)
      {
        TimeLeft = 0f;

        // ensure we send an Exit for any collider we were hitting so detectors don't remain "stuck"
        if (lastHit != null)
        {
          OnBeamExit?.Invoke(lastHit);
          lastHit = null;
        }

        IsActivated = false;
        HasBatteryLeft = false;

        if (FlashlightLight != null) FlashlightLight.SetActive(false);
        if (FlashlightSpotLight != null) FlashlightSpotLight.enabled = false;

        OnBatteryEmpty?.Invoke();
        Debug.Log("Flashlight battery empty");
      }
    }
    else
    {
        // if flashlight is off or battery dead, ensure we send exit for any prev hit
        if (lastHit != null)
        {
            OnBeamExit?.Invoke(lastHit);
            lastHit = null;
        }
    }
  }

  private void HandlePlayerInput()
  {
    // Using legacy Input.GetMouseButtonDown for simplicity — keep as before
    if (Input.GetMouseButtonDown(0))
    {
      if (!HasBatteryLeft) return;

      IsActivated = !IsActivated;

      if (FlashlightLight != null) FlashlightLight.SetActive(IsActivated);
      if (FlashlightSpotLight != null) FlashlightSpotLight.enabled = IsActivated;

      if (!IsActivated && lastHit != null)
      {
        // emit exit if turned off while hitting something
        OnBeamExit?.Invoke(lastHit);
        lastHit = null;
      }
    }
  }

  private void CastRay()
  {
    // prefer spot light transform if available, otherwise this transform
    Vector3 origin = (FlashlightSpotLight != null) ? FlashlightSpotLight.transform.position : transform.position;
    Vector3 dir = (FlashlightSpotLight != null) ? FlashlightSpotLight.transform.forward : transform.forward;

    Ray ray = new Ray(origin, dir);
    if (Physics.Raycast(ray, out RaycastHit hit, beamRange, beamHitMask))
    {
      Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 0.1f);

      // if we hit a collider and it's different from lastHit, fire enter/exit appropriately
      Collider hitCol = hit.collider;
      if (hitCol != lastHit)
      {
        // exit previous
        if (lastHit != null)
        {
          OnBeamExit?.Invoke(lastHit);
        }

        // enter new
        lastHit = hitCol;
        OnBeamEnter?.Invoke(lastHit);

        // legacy single-event
        if (hitCol.CompareTag("enemy"))
          OnEnemyHit?.Invoke();
      }
      else
      {
        // still hitting same collider — nothing to do
      }
    }
    else
    {
      Debug.DrawRay(ray.origin, ray.direction * beamRange, Color.red, 0.1f);

      // if previously hitting something, now exited
      if (lastHit != null)
      {
        OnBeamExit?.Invoke(lastHit);
        lastHit = null;
      }
    }
  }

  // Called when a battery is inserted by inventory system
  public void HandleBatteryInventorySelect()
  {
    HasBatteryLeft = true;
    FirstWarningFlag = false;
    LastWarningFlag = false;
    TimeLeft = maxBattery;
    OnBatteryInsert?.Invoke();
    Debug.Log("Battery inserted");
  }

  void OnDisable()
  {
    // ensure no dangling lastHit stays
    if (lastHit != null)
    {
      OnBeamExit?.Invoke(lastHit);
      lastHit = null;
    }
  }
}
