using System;
using DG.Tweening;
using ObjectModel;
using TMPro;
using UnityEngine;

public class ScreenMarker : CompassMarkerBase
{
	public ScreenMarker.DirectionTypes DirectionType
	{
		get
		{
			return this._type;
		}
	}

	public void Init(ScreenMarker.DirectionTypes type, HintArrowType3D hintType)
	{
		this._type = type;
		this._icoContent.transform.Rotate(0f, (float)((this._type != ScreenMarker.DirectionTypes.Left) ? 0 : 180), 0f);
		this._icoText.text = ((!HintSettings.IcoDictionary.ContainsKey(hintType)) ? string.Empty : HintSettings.IcoDictionary[hintType]);
	}

	public void UpdateText(string text)
	{
		this._textDist.text = text;
	}

	public override void SetActive(bool flag)
	{
		base.SetActive(flag);
		if (flag)
		{
			this.ResetView();
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOScale(this._icoContent, 1.3f, 1f), 4, 1);
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOFade(this._shine, 1f, 1f), 4, 1);
		}
	}

	protected override void _alphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this.ResetView();
		base._alphaFade_HideFinished(sender, e);
	}

	private void ResetView()
	{
		ShortcutExtensions.DOKill(this._icoContent, false);
		this._icoContent.localScale = Vector3.one;
		ShortcutExtensions.DOKill(this._shine, false);
		this._shine.alpha = 0f;
	}

	[SerializeField]
	private CanvasGroup _shine;

	[SerializeField]
	private TextMeshProUGUI _icoText;

	[SerializeField]
	private TextMeshProUGUI _textDist;

	[SerializeField]
	private RectTransform _icoContent;

	private ScreenMarker.DirectionTypes _type = ScreenMarker.DirectionTypes.Right;

	private const float AnimTime = 1f;

	public enum DirectionTypes : byte
	{
		Left,
		Right
	}
}
