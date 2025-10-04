using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{

  [SerializeField] GameObject FlashlightLight;

  private bool IsActivated = false;
  private bool HasBatteryLeft = true;

  [SerializeField] private const float MAX_BATTERY = 20.0f;
  private float TimeLeft = MAX_BATTERY;
  private const float FIRST_WARNING_TIME = 10.0f;
  private const float LAST_WARNING_TIME = 5.0f;
  private const float EMPTY_BATTERY_TIME = 0.0f;

  private bool FirstWarningFlag = false;
  private bool LastWarningFlag = false;

  public UnityEvent OnFirstWarning;
  public UnityEvent OnLastWarning;
  // This is the battery insert event that is checked by the battery indicator light
  public UnityEvent OnBatteryInsert;
  public UnityEvent OnBatteryEmpty;


  // Replace below with whatever indicates that the player has inserted battery
  // public TargetScript targetScript;




  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    FlashlightLight.gameObject.SetActive(false);

    // Replace below with whatever indicates that the player has inserted battery
    // targetScript.OnBatteryInventorySelect += HandleBatteryInventorySelect;


  }

  // Update is called once per frame
  void FixedUpdate()
  {


    if (HasBatteryLeft)
    {
      if (IsActivated)
      {

        TimeLeft -= Time.deltaTime;

        if (!FirstWarningFlag && TimeLeft < FIRST_WARNING_TIME && TimeLeft >= LAST_WARNING_TIME)
        {
          FirstWarningFlag = true;
          OnFirstWarning.Invoke(); // for the blinking light
          Debug.Log("Flashlight first warning");
        }
        else if (!LastWarningFlag && TimeLeft <= LAST_WARNING_TIME && TimeLeft > EMPTY_BATTERY_TIME)
        {
          LastWarningFlag = true;
          OnLastWarning.Invoke(); // for the blinking light
          Debug.Log("Flashlight last warning");
        }
        else if (TimeLeft <= EMPTY_BATTERY_TIME)
        {
          TimeLeft = EMPTY_BATTERY_TIME;
          IsActivated = false;
          HasBatteryLeft = false;

          FlashlightLight.gameObject.SetActive(false);
          OnBatteryEmpty.Invoke(); // for the blinking light
        }

      }

    }
    else return;

    HandlePLayerInput();

  }

  private void HandlePLayerInput()
  {

    if (Input.GetMouseButtonDown(0))
    {
      if (IsActivated)
      {

        IsActivated = false;
        FlashlightLight.gameObject.SetActive(false);
      }
      else
      {
        IsActivated = true;
        FlashlightLight.gameObject.SetActive(true);
      }
    }
  }

  private void HandleBatteryInventorySelect()
  {
    HasBatteryLeft = true;
    FirstWarningFlag = false;
    LastWarningFlag = false;
    TimeLeft = MAX_BATTERY;
    OnBatteryInsert.Invoke(); // for the blinking light

  }

}
