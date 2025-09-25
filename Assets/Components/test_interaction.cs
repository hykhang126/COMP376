using UnityEngine;

public class test_interaction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InteractableComponent ic = GetComponent<InteractableComponent>();
        ic.interactionTriggered.AddListener(OnInteractionTriggered);
        ic.interactionEntered.AddListener(OnInteractionEntered);
        ic.interactionExited.AddListener(OnInteractionExited);
    }

    void OnInteractionEntered()
    {
        Debug.Log("Interaction Entered");
    }

    void OnInteractionExited()
    {
        Debug.Log("Interaction Exited");
    }
    void OnInteractionTriggered()
    {
        Debug.Log("Interaction Triggered!");
        
    }

}
