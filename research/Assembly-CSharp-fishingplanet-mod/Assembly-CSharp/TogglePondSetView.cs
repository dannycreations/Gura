using System;
using UnityEngine;
using UnityEngine.UI;

public class TogglePondSetView : MonoBehaviour
{
	private void Start()
	{
		this.ChangeActive();
	}

	public void ChangeActive()
	{
		bool isOn = base.GetComponent<Toggle>().isOn;
		if (this.ActiveView != null)
		{
			this.ActiveView.SetActive(isOn);
		}
	}

	public GameObject ActiveView;
}
