using UnityEngine;

public class Dryer : MonoBehaviour
{
    private AudioSource dryerAudio;

    [SerializeField] private AudioClip dryerWarningSound;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TaskManager.onClothesInDryer.AddListener(OnClothesInDryer);

        TaskManager.onClothesNotInDryer.AddListener(OnClothesNotInDryer);

        dryerAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClothesInDryer()
    {
        dryerAudio.Play();
    }

    void OnClothesNotInDryer()
    {
        dryerAudio.PlayOneShot(dryerWarningSound);
    }
}
