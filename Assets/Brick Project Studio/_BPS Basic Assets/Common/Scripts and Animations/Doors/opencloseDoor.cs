using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles

{
	public class opencloseDoor : MonoBehaviour
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

		// Make a function to open the door in the inspector
		[NaughtyAttributes.Button("Open Door")]
		public void OpenDoorInspector()
		{
			OpenDoor();
		}

		// Make a function to close the door in the inspector
		[NaughtyAttributes.Button("Close Door")]
		public void CloseDoorInspector()
		{
			CloseDoor();
		}

	}
}