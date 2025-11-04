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

	public AudioSource doorAudioSource { get; private set; }

  public NavMeshObstacle navObstacle;

  void Start()
	{
    // auto-find NavMeshObstacle in children if not explicitly assigned
    if (navObstacle == null)
    {
      navObstacle = GetComponentInChildren<NavMeshObstacle>();
    }
    open = false;

		doorAudioSource = gameObject.GetComponent<AudioSource>();
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
    // If a NavMeshObstacle is present, disable carving so agents can pass once the door opens
    if (navObstacle != null)
    {
      navObstacle.carving = false;
    }
    print("you are opening the door");
		openandclose.Play(openAnimation);
		/*doorAudioSource.clip = openSound;
		doorAudioSource.pitch = Random.Range(0.9f, 1.1f);
		doorAudioSource.Play();*/
		open = true;
		yield return new WaitForSeconds(waitTime);
	}

	public IEnumerator closing(float waitTime = 0.5f)
	{
    // If a NavMeshObstacle is present, enable carving so agents treat the doorway as blocked
    if (navObstacle != null)
    {
      navObstacle.carving = true;
    }
    print("you are closing the door");
		openandclose.Play(closeAnimation);
		/*doorAudioSource.clip = closeSound;
		doorAudioSource.Play();*/
		open = false;
		yield return new WaitForSeconds(waitTime);
	}
}