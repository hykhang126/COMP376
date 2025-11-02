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
    if (stateMachine != null)
      stateMachine.InvokeStateEvent("toGreenLight");

    if (BatteryIndicatorLight != null)
      BlinkingLight = BatteryIndicatorLight.GetComponent<Light>();

    if (BlinkingLight == null)
      Debug.LogWarning("[BatteryIndicator] No Light component found on BatteryIndicatorLight.", this);
  }

  public void GreenLight()
  {
    if (BlinkingLight == null) return;
    BlinkingLight.color = Color.green;
    BlinkingLight.enabled = true;
  }

  public void YellowLight()
  {
    if (BlinkingLight == null) return;
    BlinkingLight.color = Color.yellow;
    BlinkingLight.enabled = true;
  }

  public void RedLight()
  {
    if (BlinkingLight == null) return;
    BlinkingLight.color = Color.red;
    BlinkingLight.enabled = true;
  }

  public void StartYellowBlink()
  {
    if (BlinkingLight == null) return;
    if (blinkCoroutine == null)
      blinkCoroutine = StartCoroutine(Blink());
  }

  public void StopYellowBlink()
  {
    if (blinkCoroutine != null)
    {
        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
    }
    if (BlinkingLight != null)
      BlinkingLight.enabled = true;
  }

  private IEnumerator Blink()
  {
    if (BlinkingLight == null) yield break;

    while (true)
    {
        BlinkingLight.enabled = !BlinkingLight.enabled;
        yield return new WaitForSeconds(blinkInterval);
    }
  }

  public void HandleFirstWarning()
  {
    stateMachine?.InvokeStateEvent("toYellowLight");
    Debug.Log("Blinking light received FIRST warning");
  }

  public void HandleLastWarning()
  {
    stateMachine?.InvokeStateEvent("toBlinkingYellowLight");
    Debug.Log("Blinking light received LAST warning");
  }

  public void HandleBatteryInsert()
  {
    stateMachine?.InvokeStateEvent("toGreenLight");
    Debug.Log("Blinking light received BATTERY INSERT");
  }

  public void HandleEmptyBattery()
  {
    stateMachine?.InvokeStateEvent("toRedLight");
    Debug.Log("Blinking light received EMPTY BATTERY");
  }
}
