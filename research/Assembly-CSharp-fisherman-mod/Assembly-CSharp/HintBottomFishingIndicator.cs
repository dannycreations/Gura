using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HintBottomFishingIndicator : HintColorImageChildren
{
	protected override void Init()
	{
		for (int i = 0; i < base.transform.parent.childCount; i++)
		{
			Transform child = base.transform.parent.GetChild(i);
			if (!child.Equals(base.transform))
			{
				base.FillData<Image>(this.Data, null, false, child);
				if (child.name == "water")
				{
					RectTransform component = Object.Instantiate<GameObject>(this._shinePrefab, child).GetComponent<RectTransform>();
					component.gameObject.SetActive(false);
					this._shines.Add(component);
				}
			}
		}
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		bool flag = this.shouldShow && !isDestroy;
		foreach (KeyValuePair<Image, Color> keyValuePair in this.Data)
		{
			keyValuePair.Key.color = ((!flag) ? keyValuePair.Value : this._highlightColor);
		}
		if (flag)
		{
			for (int i = 0; i < this._shines.Count; i++)
			{
				RectTransform rectTransform = this._shines[i];
				RectTransform component = rectTransform.parent.GetComponent<RectTransform>();
				float y = rectTransform.anchoredPosition.y;
				rectTransform.anchoredPosition = new Vector2(-component.rect.width / 2f, y);
				rectTransform.gameObject.SetActive(true);
				Sequence sequence = DOTween.Sequence();
				TweenSettingsExtensions.Insert(sequence, 0f, ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(component.rect.width / 2f, y), this.MoveTime, false));
				TweenSettingsExtensions.SetLoops<Sequence>(sequence, -1, 1);
			}
		}
	}
}
