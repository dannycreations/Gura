using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ComboBoxHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ChangedValue;

	public string TextCaption { get; set; }

	public int CurrentValue { get; set; }

	public void ChangeValue(GameObject selectedItem)
	{
		this.CurrentValue = selectedItem.GetComponent<ComboBoxContent>().Index;
		this.TextCaption = selectedItem.transform.Find("Text").GetComponent<Text>().text;
		base.transform.Find("Text").GetComponent<Text>().text = this.TextCaption;
		base.transform.Find("Text").GetComponent<Text>().fontStyle = 1;
		if (this.ChangedValue != null)
		{
			this.ChangedValue(this, new EventArgs());
		}
	}

	private void Start()
	{
	}
}
