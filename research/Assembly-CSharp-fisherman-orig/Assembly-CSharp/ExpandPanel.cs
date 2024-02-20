using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExpandPanel : MonoBehaviour
{
	private void Start()
	{
		this.Refresh();
	}

	public void Refresh()
	{
		if (this.IsExpanded)
		{
			this.Caption.text = this.ExpandedText;
		}
		else
		{
			this.Caption.text = this.TurnedText;
		}
	}

	public void Expand()
	{
		this.IsExpanded = !this.IsExpanded;
		this.Refresh();
		if (this.OnExpanded != null)
		{
			this.OnExpanded.Invoke(this.IsExpanded);
		}
	}

	public Text Caption;

	public string TurnedText;

	public string ExpandedText;

	public bool IsExpanded = true;

	public UnityEvent<bool> OnExpanded = new ResizeablePanel.UnityEventBool();
}
