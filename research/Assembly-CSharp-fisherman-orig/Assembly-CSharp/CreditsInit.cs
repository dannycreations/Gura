using System;
using UnityEngine;

public class CreditsInit : MonoBehaviour
{
	private void Start()
	{
	}

	private void CreditsInit_OnStart(object sender, EventArgs e)
	{
	}

	internal void CreditsOn()
	{
		this.Credits.enabled = true;
	}

	public InfiniteVerticalScroll Credits;
}
