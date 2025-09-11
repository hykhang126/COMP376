using UnityEngine;

public class test_interaction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InteractableComponent ic = GetComponent<InteractableComponent>();
        ic.interactionTriggered.AddListener(OnInteractionTriggered);
    }

    void OnInteractionTriggered()
    {
        Debug.Log("Interaction Triggered!");
        
    }

}
