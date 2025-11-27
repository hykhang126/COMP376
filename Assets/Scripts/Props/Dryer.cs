using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

public class Dryer :  Interactable
{
    private AudioSource dryerAudio;

    // bool shirtInDryer = false;

    // bool pantsInDryer = false;

    public static UnityEvent onClothesInDryer = new UnityEvent();

    [SerializeField] private AudioClip dryerWarningSound;

    [SerializeField] private AudioClip dryerLoadingSound;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        dryerAudio = GetComponent<AudioSource>();
    }

    void OnClothesInDryer()
    {
        dryerAudio.Play();
    }

    void OnClothesNotInDryer()
    {
        dryerAudio.PlayOneShot(dryerWarningSound);
    }

    public override void Interact()
    {
        
        /*if(Inventory.InstanceReference.items.Count != 0 && !shirtInDryer && Inventory.InstanceReference.items[Inventory.InstanceReference.GetCurrentItemIndex()].Name == "Shirt")
        {
            shirtInDryer = true;
            Inventory.InstanceReference.RemoveItem();
            CheckLaundryStart();
        }
        else if(Inventory.InstanceReference.items.Count != 0 &&!pantsInDryer && Inventory.InstanceReference.items[Inventory.InstanceReference.GetCurrentItemIndex()].Name == "Pants")
        {
            pantsInDryer = true;
            Inventory.InstanceReference.RemoveItem();
            CheckLaundryStart();
        }*/
        
        dryerAudio.PlayOneShot(dryerWarningSound);
        
        
    }

    /*private void CheckLaundryStart()
    {
        if(shirtInDryer && pantsInDryer)
        {
            onClothesInDryer.Invoke();
            dryerAudio.Play();
        }
        else
        {
            dryerAudio.PlayOneShot(dryerLoadingSound);
        }
    }*/
}
