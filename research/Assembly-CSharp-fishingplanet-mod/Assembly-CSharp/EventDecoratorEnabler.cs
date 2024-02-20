using System;
using UnityEngine;

public class EventDecoratorEnabler : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < this.DecorationObjects.Length; i++)
		{
			if (this.DecorationObjects[i] == null)
			{
				break;
			}
			if (EventsController.CurrentEvent != null && EventsController.CurrentEvent.EventId == this.EventId)
			{
				this.DecorationObjects[i].SetActive(true);
			}
			else
			{
				this.DecorationObjects[i].SetActive(false);
			}
		}
		for (int j = 0; j < this.DecorationAudioSources.Length; j++)
		{
			if (this.DecorationAudioSources[j] == null)
			{
				break;
			}
			if (EventsController.CurrentEvent != null && EventsController.CurrentEvent.EventId == this.EventId)
			{
				this.DecorationAudioSources[j].enabled = true;
			}
			else
			{
				this.DecorationAudioSources[j].enabled = false;
			}
		}
		for (int k = 0; k < this.DisableObjects.Length; k++)
		{
			if (this.DisableObjects[k] == null)
			{
				break;
			}
			if (EventsController.CurrentEvent != null && EventsController.CurrentEvent.EventId == this.EventId)
			{
				this.DisableObjects[k].SetActive(false);
			}
			else
			{
				this.DisableObjects[k].SetActive(true);
			}
		}
	}

	public int EventId;

	public GameObject[] DecorationObjects;

	public AudioSource[] DecorationAudioSources;

	public GameObject[] DisableObjects;
}
