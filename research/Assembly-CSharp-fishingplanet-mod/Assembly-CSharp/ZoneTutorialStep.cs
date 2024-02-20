using System;
using UnityEngine;

public class ZoneTutorialStep : MonoBehaviour
{
	internal void OnTriggerEnter(Collider other)
	{
		if (this.EventCollider == other)
		{
			this.InZone = true;
		}
	}

	internal void OnTriggerExit(Collider other)
	{
		if (this.EventCollider == other)
		{
			this.InZone = false;
		}
	}

	public bool InZone;

	public Collider EventCollider;

	private bool _isExit;
}
