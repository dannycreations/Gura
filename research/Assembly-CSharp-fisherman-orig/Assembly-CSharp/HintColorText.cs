using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HintColorText : HintColorBase
{
	protected override void Init()
	{
		base.FillData<TextMeshProUGUI>(this.Data, new Action<TextMeshProUGUI>(base.CloneFontMaterial), true, null);
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		bool flag = this.shouldShow && !isDestroy;
		if (this.IsFishKeepnet && ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.AdditionalInfo.IsHintFishKeepnetActive = flag;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		RectTransform rectTransform;
		foreach (KeyValuePair<TextMeshProUGUI, Color> keyValuePair in this.Data)
		{
			if (flag)
			{
				keyValuePair.Key.fontSharedMaterial.EnableKeyword(ShaderUtilities.Keyword_Glow);
			}
			else
			{
				keyValuePair.Key.fontSharedMaterial.DisableKeyword(ShaderUtilities.Keyword_Glow);
			}
			keyValuePair.Key.UpdateMeshPadding();
			keyValuePair.Key.color = ((!flag) ? keyValuePair.Value : this._highlightColor);
			if (flag && this.IsFishKeepnet)
			{
				rectTransform = keyValuePair.Key.GetComponent<RectTransform>();
				if (rectTransform.anchoredPosition.x < num3)
				{
					num3 = rectTransform.anchoredPosition.x;
					num4 = rectTransform.rect.width;
				}
				num += rectTransform.rect.width;
				num2 = Mathf.Max(num2, rectTransform.rect.height);
			}
		}
		if (!flag && this.IsFishKeepnet)
		{
			this.ClearShines();
			return;
		}
		if (this._shines.Count > 0 || !this.IsFishKeepnet)
		{
			return;
		}
		float num5 = num3 - num4 / 2f;
		float num6 = -num2 / 2f;
		rectTransform = Object.Instantiate<GameObject>(this._shinePrefab, base.gameObject.transform.parent).GetComponent<RectTransform>();
		rectTransform.anchoredPosition = new Vector2(num5, num6);
		rectTransform.gameObject.SetActive(true);
		Sequence sequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(num5 + num, num6), this.MoveTime, false));
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(num5 + num, num6 + num2), this.MoveTime, false));
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(num5, num6 + num2), this.MoveTime, false));
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(num5, num6), this.MoveTime, false));
		TweenSettingsExtensions.SetLoops<Sequence>(sequence, -1, 1);
		this._shines.Add(rectTransform);
	}

	private bool IsFishKeepnet
	{
		get
		{
			return this.observer != null && this.observer.Message != null && this.observer.Message.ElementId == "HUDFishKeepnetPanel";
		}
	}

	protected Dictionary<TextMeshProUGUI, Color> Data = new Dictionary<TextMeshProUGUI, Color>();
}
