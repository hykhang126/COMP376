using System.Collections;
using UnityEngine;

public class DoorAction : MonoBehaviour
{

	public Animator openandclose;
	public bool open;
	public Transform Player;

	void Start()
	{
		open = false;
	}

	void OnMouseOver()
	{
		{
			if (Player)
			{
				float dist = Vector3.Distance(Player.position, transform.position);
				if (dist < 15)
				{
					if (open == false)
					{
						if (Input.GetMouseButtonDown(0))
						{
							StartCoroutine(opening());
						}
					}
					else
					{
						if (open == true)
						{
							if (Input.GetMouseButtonDown(0))
							{
								StartCoroutine(closing());
							}
						}

					}

				}
			}

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

	IEnumerator opening()
	{
		print("you are opening the door");
		openandclose.Play("Opening 1");
		open = true;
		yield return new WaitForSeconds(.5f);
	}

	IEnumerator closing()
	{
		print("you are closing the door");
		openandclose.Play("Closing 1");
		open = false;
		yield return new WaitForSeconds(.5f);
	}
}