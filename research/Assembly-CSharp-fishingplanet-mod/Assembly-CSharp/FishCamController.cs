using System;
using UnityEngine;

public class FishCamController : MonoBehaviour
{
	public void Update()
	{
		if (this.fish == null)
		{
			return;
		}
		base.transform.position = this.fish.position;
	}

	public Transform fish;
}
