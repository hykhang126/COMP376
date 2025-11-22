using UnityEngine;
using UnityEngine.Events;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
public class InteractableComponent : MonoBehaviour
{
    public UnityEvent interactionTriggered = new UnityEvent();
    public UnityEvent interactionEntered = new UnityEvent();
    public UnityEvent interactionExited = new UnityEvent();

    private Collider _collider;
    [SerializeField] public float cooldown = 1.0f;
    [SerializeField] public bool isOneShot = false;

    public bool isCoolingDown = false;

    void Start()
    {
        interactionTriggered.AddListener(OnInteractionTriggered);

        _collider = GetComponent<Collider>();
        if(_collider == null)
        {
            Debug.LogError("Cannot Find Collider Component");
        }
    }

    public void AttempyTriggerInteraction()
    {
        if (isCoolingDown)
        {
            return;
        }
        else
        {
            interactionTriggered.Invoke();
            if (this.gameObject.activeSelf)
              StartCoroutine(CooldownCoroutine());
        }

    }
    void OnInteractionTriggered()
    {
        //If interaction is one-shot, detach all listeners 
        if (isOneShot)
        {
            interactionTriggered.RemoveAllListeners();
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCoolingDown = true;
        float timer = cooldown;
        while (timer > 0.0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        isCoolingDown = false;

    }

    void OnValidate()
    {
        // If no collider is set, grab the first available 
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
    }
}
