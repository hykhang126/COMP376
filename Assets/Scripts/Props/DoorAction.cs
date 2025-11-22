using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DoorAction : MonoBehaviour
{
    public Animator openandclose;
    public bool open;
    public ItemContractSO key;

    [SerializeField] bool isLocked = true;

    [SerializeField] string openAnimation;
    [SerializeField] string closeAnimation;

    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;
    [SerializeField] AudioClip lockSound;

    public AudioSource doorAudioSource { get; private set; }

    public NavMeshObstacle navObstacle;

    void Start()
    {
        if (navObstacle == null)
            navObstacle = GetComponentInChildren<NavMeshObstacle>();

        open = false;

        // find AudioSource on this GameObject first, then in children
        doorAudioSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Example: activate a room when player enters
        if (other.CompareTag("enemy"))
        {
            Debug.Log("Triggered by: " + other.name);
            if (!open)
            {
                OpenDoor();
            }
        }
    }

    public void OpenorClose()
    {
        if (isLocked && key != null && Player.InstanceReference.inventory.items[Player.InstanceReference.inventory.GetCurrentItemIndex()].Id ==
            key.Id)
        {
            Debug.Log("Unlocked");
            isLocked = false;
        }
        else if (isLocked)
        {
            Debug.Log("Locked and you don't have the right key");

            // PLAY LOCKED SOUND
            if (doorAudioSource != null && lockSound != null)
            {
                doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
                doorAudioSource.PlayOneShot(lockSound);
            }

            return;
        }

        if (!open)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        StartCoroutine(opening());
    }

    public void CloseDoor()
    {
        StartCoroutine(closing());
    }

    public void TeleportPlayerTo(Transform player, Transform targetPosition)
    {
        if (player != null && targetPosition != null)
        {
            player.SetPositionAndRotation(targetPosition.position, targetPosition.rotation);
        }
        else
        {
            Debug.LogWarning("Player or target position is null.");
        }
    }

    public IEnumerator opening(float waitTime = 0.5f)
    {
        if (navObstacle != null) navObstacle.carving = false;

        openandclose.Play(openAnimation);

        if (doorAudioSource != null && openSound != null)
        {
            doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
            doorAudioSource.PlayOneShot(openSound);
        }

        open = true;
        yield return new WaitForSeconds(waitTime);
    }

    public IEnumerator closing(float waitTime = 0.5f)
    {
        if (navObstacle != null) navObstacle.carving = true;

        openandclose.Play(closeAnimation);

        if (doorAudioSource != null && closeSound != null)
        {
            doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
            doorAudioSource.PlayOneShot(closeSound);
        }

        open = false;
        yield return new WaitForSeconds(waitTime);
    }
}
