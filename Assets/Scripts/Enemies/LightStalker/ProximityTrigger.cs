using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ProximityTrigger : MonoBehaviour
{
    [Tooltip("Tag used to identify the player. Defaults to 'Player'.")]
    public string playerTag = "Player";

    public UnityEvent OnPlayerEnter = new UnityEvent();
    public UnityEvent OnPlayerExit = new UnityEvent();

    void Reset()
    {
        // attempt to set up collider defaults when component is added in inspector
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag(playerTag))
        {
            OnPlayerEnter?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag(playerTag))
        {
            OnPlayerExit?.Invoke();
        }
    }
}
