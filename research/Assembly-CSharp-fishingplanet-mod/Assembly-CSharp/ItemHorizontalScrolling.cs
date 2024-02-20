using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[ExecuteInEditMode]
public class ItemHorizontalScrolling : MonoBehaviour
{
	private void Awake()
	{
		this._scrollRect = base.GetComponent<ScrollRect>();
		this._rect = base.gameObject.GetComponent<RectTransform>();
		this.NextScrollButton.onClick.AddListener(new UnityAction(this.NextOnClick));
		this.PrevScrollButton.onClick.AddListener(new UnityAction(this.PrevOnClick));
	}

	private void Update()
	{
		if (this._scrollRect.content.rect.width <= this._rect.rect.width)
		{
			this.NextScrollButton.interactable = false;
			this.PrevScrollButton.interactable = false;
		}
		else if (this._scrollRect.horizontalNormalizedPosition <= 0.01f)
		{
			this.PrevScrollButton.interactable = false;
			this.NextScrollButton.interactable = true;
		}
		else if (this._scrollRect.horizontalNormalizedPosition >= 0.99f)
		{
			this.PrevScrollButton.interactable = true;
			this.NextScrollButton.interactable = false;
		}
		else
		{
			this.PrevScrollButton.interactable = true;
			this.NextScrollButton.interactable = true;
		}
	}

	private void NextOnClick()
	{
		float num = 1f / ((float)this._scrollRect.content.transform.childCount - (float)((int)this._rect.rect.width) / (this._scrollRect.content.rect.width / (float)this._scrollRect.content.transform.childCount));
		this._scrollRect.horizontalNormalizedPosition = this._scrollRect.horizontalNormalizedPosition + num;
	}

	private void PrevOnClick()
	{
		float num = 1f / ((float)this._scrollRect.content.transform.childCount - (float)((int)this._rect.rect.width) / (this._scrollRect.content.rect.width / (float)this._scrollRect.content.transform.childCount));
		this._scrollRect.horizontalNormalizedPosition = this._scrollRect.horizontalNormalizedPosition - num;
	}

	public Button NextScrollButton;

	public Button PrevScrollButton;

	private ScrollRect _scrollRect;

	private RectTransform _rect;
}
