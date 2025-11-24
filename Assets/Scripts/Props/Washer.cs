using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Washer : Interactable
{
    private AudioSource washerAudio;

    [SerializeField] private AudioClip washerWarningSound;

    [SerializeField] private AudioClip washerLoadingSound;

    public static UnityEvent onClothesInWasher = new UnityEvent();

    bool shirtInWasher = false;

    bool pantsInWasher = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        washerAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        
        if(Inventory.InstanceReference.items.Count != 0 && !shirtInWasher && Inventory.InstanceReference.items[Inventory.InstanceReference.GetCurrentItemIndex()].Name == "Shirt")
        {
            shirtInWasher = true;
            Inventory.InstanceReference.RemoveItem();
            CheckLaundryStart();
        }
        else if(Inventory.InstanceReference.items.Count != 0 &&!pantsInWasher && Inventory.InstanceReference.items[Inventory.InstanceReference.GetCurrentItemIndex()].Name == "Pants")
        {
            pantsInWasher = true;
            Inventory.InstanceReference.RemoveItem();
            CheckLaundryStart();
        }
        else
        {
            washerAudio.PlayOneShot(washerWarningSound);
        }
        
    }

    private void CheckLaundryStart()
    {
        if(shirtInWasher && pantsInWasher)
        {
            onClothesInWasher.Invoke();
            washerAudio.Play();
        }
        else
        {
            washerAudio.PlayOneShot(washerLoadingSound);
        }
    }
}
