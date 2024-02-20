using System;
using UnityEngine;

namespace cakeslice
{
	public class Toggle : MonoBehaviour
	{
		private void Start()
		{
		}

		private void Update()
		{
			if (Input.GetKeyDown(107))
			{
				base.GetComponent<Outline>().enabled = !base.GetComponent<Outline>().enabled;
			}
		}
	}
}
