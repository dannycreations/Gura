using System;
using UnityEngine;

public class ShowingFishingHUD : MonoBehaviour
{
	private void Start()
	{
		this.ContentPanel.FastHidePanel();
	}

	public void SetActive(bool flag)
	{
		if (flag)
		{
			this.ContentPanel.ShowPanel();
		}
		else
		{
			this.ContentPanel.HidePanel();
		}
	}

	public AlphaFade ContentPanel;
}
