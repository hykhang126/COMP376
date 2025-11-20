using UnityEngine;

public class HallwayTrigger : MonoBehaviour
{
  public AudioSource audioSource;       // sound to play
  public GameObject eventObject;        // optional: something to enable
  public bool triggerOnce = true;

  private bool hasTriggered = false;

  private void OnTriggerEnter(Collider other)
  {
    if (hasTriggered && triggerOnce) return;

    if (other.CompareTag("Player"))
    {
      if (audioSource != null)
        audioSource.Play();

      if (eventObject != null)
        eventObject.SetActive(true);

      hasTriggered = true;
    }
  }
}
