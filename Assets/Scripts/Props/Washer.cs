using UnityEngine;
using UnityEngine.Events;

public class Washer : MonoBehaviour
{
    private AudioSource washerAudio;

    [SerializeField] private AudioClip washerWarningSound;

    bool isClothesInWasher = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TaskManager.onClothesInWasher.AddListener(OnClothesInWasher);

        TaskManager.onClothesNotInWasher.AddListener(OnClothesNotInWasher);

        washerAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClothesInWasher()
    {
        if(!isClothesInWasher)
        {
            isClothesInWasher = true;
            washerAudio.Play();
        }   
    }

    void OnClothesNotInWasher()
    {
        if(!isClothesInWasher)
        {
            isClothesInWasher = false;
            washerAudio.PlayOneShot(washerWarningSound);
        }
    }
}
