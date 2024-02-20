using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextColorPulsation : ColorPulsation
{
	protected override void Start()
	{
		this._graphicToPulse = base.GetComponent<Text>();
		if (this._graphicToPulse == null)
		{
			this._graphicToPulse = base.GetComponent<TextMeshProUGUI>();
		}
		if (this._graphicToPulse != null)
		{
			this.StartColor = this._graphicToPulse.color;
		}
	}

	protected override void SetColor(Color newColor)
	{
		if (this._graphicToPulse != null)
		{
			this._graphicToPulse.color = newColor;
		}
	}

	private Graphic _graphicToPulse;
}
