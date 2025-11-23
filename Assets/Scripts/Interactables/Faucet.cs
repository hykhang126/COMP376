using UnityEngine;

public class Faucet : Interactable
{
    public GameObject waterStream;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        if (waterStream != null)
        {
            bool isActive = waterStream.activeSelf;
            waterStream.SetActive(!isActive);
        }
    }
}
