using System.Collections;
using UnityEngine;

public class BatteryIndicator : MonoBehaviour
{

  [SerializeField] GameObject BatteryIndicatorLight;

  private Light BlinkingLight;

  public StateMachine stateMachine { get; private set; }

  public float blinkInterval = 0.5f;

  private Coroutine blinkCoroutine;

  public void Awake()
  {

    stateMachine = GetComponent<StateMachine>();
    stateMachine.InvokeStateEvent("toGreenLight");
    BlinkingLight = BatteryIndicatorLight.gameObject.GetComponent<Light>();
  }
  public void GreenLight()
  {
    BlinkingLight.color = Color.green;
  }

  public void YellowLight()
  {
    BlinkingLight.color = Color.yellow;
  }

  public void RedLight()
  {
    BlinkingLight.color = Color.red;
  }

  public void StartYellowBlink()
  {
    if (blinkCoroutine == null)
    {
      blinkCoroutine = StartCoroutine(Blink());
    }
  }

  public void StopYellowBlink()
  {
    if (blinkCoroutine != null)
    {
      StopCoroutine(blinkCoroutine);
      blinkCoroutine = null;
    }
  }

  private IEnumerator Blink()
  {
    while (true)
    {
      BlinkingLight.enabled = !BlinkingLight.enabled;
      yield return new WaitForSeconds(blinkInterval);
    }
  }

  public void HandleFirstWarning()
  {
    stateMachine.InvokeStateEvent("toYellowLight");
    Debug.Log("Blinking light received FIRST warning");
  }

  public void HandleLastWarning()
  {
    stateMachine.InvokeStateEvent("toBlinkingYellowLight");
    Debug.Log("Blinking light received LAST warning");
  }
  public void HandleBatteryInsert()
  {
    stateMachine.InvokeStateEvent("toGreenLight");
    Debug.Log("Blinking light received BATTERY INSERT");
  }
  public void HandleEmptyBattery()
  {
    stateMachine.InvokeStateEvent("toRedLight");
    Debug.Log("Blinking light received EMPTY BATTERY");
  }
}
