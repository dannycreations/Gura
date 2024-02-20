using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HintColorImageChildren : HintColorBase
{
	protected override void Init()
	{
		base.FillData<Image>(this.Data, null, true, null);
		if (this._shinePrefab != null)
		{
			List<Image> list = this.Data.Keys.ToList<Image>();
			for (int i = 0; i < list.Count; i++)
			{
				RectTransform component = Object.Instantiate<GameObject>(this._shinePrefab, list[i].transform).GetComponent<RectTransform>();
				component.gameObject.SetActive(false);
				this._shines.Add(component);
			}
		}
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		foreach (KeyValuePair<Image, Color> keyValuePair in this.Data)
		{
			keyValuePair.Key.color = ((!this.shouldShow || isDestroy) ? keyValuePair.Value : this._highlightColor);
		}
		if (this.shouldShow && !isDestroy)
		{
			List<Image> list = this.Data.Keys.ToList<Image>();
			for (int i = 0; i < this._shines.Count; i++)
			{
				RectTransform component = list[i].GetComponent<RectTransform>();
				RectTransform rectTransform = this._shines[i];
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - component.rect.width / 2f, rectTransform.anchoredPosition.y);
				rectTransform.gameObject.SetActive(true);
				Sequence sequence = DOTween.Sequence();
				TweenSettingsExtensions.Insert(sequence, 0f, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(component.rect.width / 2f, rectTransform.anchoredPosition.y), this.MoveTime, false));
				TweenSettingsExtensions.SetLoops<Sequence>(sequence, -1, 1);
			}
		}
	}

	protected Dictionary<Image, Color> Data = new Dictionary<Image, Color>();
}
