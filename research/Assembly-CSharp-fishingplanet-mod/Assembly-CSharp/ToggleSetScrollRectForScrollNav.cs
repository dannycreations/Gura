using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleSetScrollRectForScrollNav : MonoBehaviour
{
	private void Awake()
	{
		this.toggle = base.GetComponent<Toggle>();
		if (this.toggle == null)
		{
			base.enabled = false;
			return;
		}
		this.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.Check));
	}

	private void Check(bool isOn)
	{
		if (isOn)
		{
			this.navigation.SetScrollRect(this.scrollRect);
		}
	}

	public ScrollbarNavigation navigation;

	public ScrollRect scrollRect;

	private Toggle toggle;
}
