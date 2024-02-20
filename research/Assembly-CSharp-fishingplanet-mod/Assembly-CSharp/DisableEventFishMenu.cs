using System;
using UnityEngine;

public class DisableEventFishMenu : MonoBehaviour
{
	private void Start()
	{
		if (EventsController.CurrentEvent == null)
		{
			base.gameObject.SetActive(false);
		}
	}
}
