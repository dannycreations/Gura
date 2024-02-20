using System;
using UnityEngine;
using UnityEngine.UI;

public class SetResolutionToggle : MonoBehaviour
{
	private void Update()
	{
		string text = string.Format("{0}x{1} {2}Hz", this.resolution.width, this.resolution.height, this.resolution.refreshRate);
		if (this.LabelControl.text != text)
		{
			this.LabelControl.text = text;
		}
	}

	public Text LabelControl;

	[HideInInspector]
	public Resolution resolution;
}
