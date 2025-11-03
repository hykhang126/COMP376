using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles

{
	public class OvenFlip: MonoBehaviour
	{

		public Animator openandcloseoven;
		public bool open;
		public Transform Player;

		void Start()
		{
			open = false;
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
			print("you are opening the Window");
			openandcloseoven.Play("OpenOven");
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the Window");
			openandcloseoven.Play("ClosingOven");
			open = false;
			yield return new WaitForSeconds(.5f);
		}


	}
}