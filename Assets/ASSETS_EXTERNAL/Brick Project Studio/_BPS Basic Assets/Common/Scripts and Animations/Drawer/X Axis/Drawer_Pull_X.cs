using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles
{
	public class Drawer_Pull_X : MonoBehaviour
	{
		public Animator pull_01;
		public bool open;
		public Transform Player;

		[Header("SFX (simple)")]
		public AudioClip openSFX;
		public AudioClip closeSFX;
		[Range(0f, 1f)]
		public float sfxVolume = 1f;

		private AudioSource audioSource;

		void Start()
		{
			open = false;

			// ensure there's an AudioSource to play SFX
			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
			}
		}

		public void OpenorClose()
		{
			if (!open)
			{
				StartCoroutine(opening());
			}
			else
			{
				StartCoroutine(closing());
			}
		}

		IEnumerator opening()
		{
			print("you are opening the door");
			pull_01.Play("openpull_01");

			// play open SFX
			if (openSFX != null && audioSource != null)
				audioSource.PlayOneShot(openSFX, sfxVolume);

			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			pull_01.Play("closepush_01");

			// play close SFX
			if (closeSFX != null && audioSource != null)
				audioSource.PlayOneShot(closeSFX, sfxVolume);

			open = false;
			yield return new WaitForSeconds(.5f);
		}
	}
}
