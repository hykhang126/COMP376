using UnityEngine;

public class key : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GetComponent<InteractableComponent>().interactionEntered.AddListener(OnInteractEntered);
        //GetComponent<InteractableComponent>().interactionTriggered.AddListener(OnInteractTriggered);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnInteractEntered()
    {
        Debug.Log("Key Interact Enter");
        //Hover code here
    }

    public void OnInteractTriggered()
    {
        Debug.Log("Key Interact Triggered");
        //Interaction code here
    }
}
