using System;
using UnityEngine;
using UnityEngine.UI;

public class HintColorImage : HintColorBase
{
	protected override void Init()
	{
		this._rootImage = base.GetComponentInParent<Image>();
		this._normalColor = this._rootImage.color;
		this._colorPulsation.SetImage(this._rootImage);
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		if (isDestroy)
		{
			this._colorPulsation.SetImage(null);
		}
		this._rootImage.color = ((!this.shouldShow || isDestroy) ? this._normalColor : this._highlightColor);
	}

	protected Color _normalColor;

	protected Image _rootImage;
}
