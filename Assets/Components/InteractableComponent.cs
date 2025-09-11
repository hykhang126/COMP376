using System;
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

    [SerializeField] private Collider _collider;
    [SerializeField] public float cooldown = 1.0f;
    [SerializeField] public bool isOneShot = false;

    private bool isCoolingDown = false;

    void Start()
    {
        interactionTriggered.AddListener(OnInteractionTriggered);
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
}
