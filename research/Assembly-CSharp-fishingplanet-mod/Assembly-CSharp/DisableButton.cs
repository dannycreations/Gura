using System;
using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{
	private void Update()
	{
		if (this._isEnabled != this.ButtonControl.enabled)
		{
			this._isEnabled = this.ButtonControl.enabled;
			if (this.ButtonControl.enabled)
			{
				this.TextControl.color = this.TextNormalColor;
				base.GetComponent<Image>().color = this.ButtonNormalColor;
			}
			else
			{
				this.TextControl.color = this.TextColor;
				base.GetComponent<Image>().color = this.ButtonColor;
			}
		}
	}

	public Color ButtonColor;

	public Color ButtonNormalColor;

	public Color TextColor;

	public Color TextNormalColor;

	public Text TextControl;

	public Button ButtonControl;

	private bool _isEnabled = true;
}
