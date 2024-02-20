using System;
using TMPro;
using UnityEngine;

public class HintBobberIndicator : HintColorBase
{
	protected override void Init()
	{
		base.Init();
		base.CloneFontMaterial(this._text);
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		if (this.shouldShow && !isDestroy)
		{
			this._text.fontSharedMaterial.EnableKeyword(ShaderUtilities.Keyword_Glow);
		}
		else
		{
			this._text.fontSharedMaterial.DisableKeyword(ShaderUtilities.Keyword_Glow);
		}
		this._text.UpdateMeshPadding();
	}

	public override void SetObserver(ManagedHint observer, int id)
	{
		base.SetObserver(observer, id);
		if (observer != null && observer.Message != null && observer.Message.ElementId == "HUDBobberIndicatorTimer" && observer.Message.Value > 0)
		{
			TextMeshProUGUI text = this._text;
			text.text += string.Format("{0}s", observer.Message.Value);
		}
	}

	[SerializeField]
	private TextMeshProUGUI _text;
}
