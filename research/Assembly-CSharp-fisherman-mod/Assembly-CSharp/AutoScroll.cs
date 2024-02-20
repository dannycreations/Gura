using System;
using frame8.Logic.Misc.Visual.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
	private void Start()
	{
		this._scrollRect = base.GetComponent<ScrollRect>();
	}

	private void LateUpdate()
	{
		if (this._horizontalScrollBar == null && this._verticalScrollBar == null)
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject == null || InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad)
		{
			return;
		}
		if (this._verticalScrollBar != null && this.size != this._verticalScrollBar.size && this.updateNumber == 0)
		{
			this.updateNumber = 2;
		}
		if (this.currentSelectable == null || this.currentSelectable.gameObject != EventSystem.current.currentSelectedGameObject)
		{
			if (this._viewportRect == null || this._viewportRect.rect.width == 0f || this._viewportRect.rect.height == 0f)
			{
				this.CalculateContentRect();
				this.ScrollRectProxy = base.GetComponentInParent<IScrollRectProxy>();
			}
			if (this.updateNumber != 0)
			{
				this.updateNumber--;
				if (this._scrollRect != null)
				{
					this._scrollRect.Rebuild(2);
				}
				if (this._verticalScrollBar != null)
				{
					this.size = this._verticalScrollBar.size;
				}
			}
			if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
			{
				return;
			}
			this.currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			if (this.currentSelectable == null)
			{
				return;
			}
			AutoScroll componentInParent = this.currentSelectable.GetComponentInParent<AutoScroll>();
			if (!this.currentSelectable.IsInteractable() || componentInParent == null || !object.ReferenceEquals(this, componentInParent))
			{
				return;
			}
			Vector3[] array = new Vector3[4];
			RectTransform rectTransform = this.currentSelectable.transform.GetComponent<RectTransform>();
			if (this.useParentHeight)
			{
				rectTransform = this.currentSelectable.transform.parent.GetComponent<RectTransform>();
			}
			rectTransform.GetWorldCorners(array);
			this.UpdateVerticalScroll(array[0], array[1]);
			this.UpdateHorizontalScroll(array[0], array[2]);
		}
	}

	private void CalculateContentRect()
	{
		if (this._viewportRect == null)
		{
			this._viewportRect = base.GetComponent<RectTransform>();
		}
		if (this._verticalScrollBar != null)
		{
			this.multiplier = ((this._verticalScrollBar.direction != 3) ? (-1) : 1);
		}
	}

	private void UpdateVerticalScroll(Vector3 yMin, Vector3 yMax)
	{
		if (this._viewportRect.rect.height == 0f)
		{
			return;
		}
		if (this._verticalScrollBar != null)
		{
			float num = ((this.ScrollRectProxy != null) ? this.ScrollRectProxy.GetContentSize() : (this._viewportRect.rect.height / this._verticalScrollBar.size * (1f - this._verticalScrollBar.size)));
			float y = this._viewportRect.InverseTransformPoint(yMax).y;
			float y2 = this._viewportRect.InverseTransformPoint(yMin).y;
			if (y > this._viewportRect.rect.yMax)
			{
				float num2 = Mathf.Abs(y - this._viewportRect.rect.yMax) + 15f;
				num2 /= ((this.ScrollRectProxy == null) ? num : (num - this._viewportRect.rect.height));
				if (this.ScrollRectProxy != null)
				{
					float num3 = this._verticalScrollBar.value - (float)this.multiplier * num2;
					this.ScrollRectProxy.SetNormalizedPosition(num3);
				}
				else
				{
					this._verticalScrollBar.value -= (float)this.multiplier * num2;
				}
			}
			else if (y2 < this._viewportRect.rect.yMin)
			{
				float num4 = Mathf.Abs(this._viewportRect.rect.yMin - y2) + 15f;
				num4 /= ((this.ScrollRectProxy == null) ? num : (num - this._viewportRect.rect.height));
				if (this.ScrollRectProxy != null)
				{
					float num5 = this._verticalScrollBar.value + (float)this.multiplier * num4;
					this.ScrollRectProxy.SetNormalizedPosition(num5);
				}
				else
				{
					this._verticalScrollBar.value += (float)this.multiplier * num4;
				}
			}
		}
	}

	private void UpdateHorizontalScroll(Vector3 xMin, Vector3 xMax)
	{
		if (this._horizontalScrollBar != null)
		{
			float num = this._viewportRect.rect.width / this._horizontalScrollBar.size * (1f - this._horizontalScrollBar.size);
			float x = this._viewportRect.InverseTransformPoint(xMax).x;
			float x2 = this._viewportRect.InverseTransformPoint(xMin).x;
			if (x > this._viewportRect.rect.xMax)
			{
				float num2 = x - this._viewportRect.rect.xMax - 15f;
				num2 /= num;
				this._horizontalScrollBar.value += num2;
			}
			else if (x2 < this._viewportRect.rect.xMin)
			{
				float num3 = this._viewportRect.rect.xMin - x2 + 15f;
				num3 /= num;
				this._horizontalScrollBar.value -= num3;
			}
		}
	}

	private RectTransform _viewportRect;

	[SerializeField]
	public Scrollbar _verticalScrollBar;

	[SerializeField]
	public Scrollbar _horizontalScrollBar;

	[SerializeField]
	public bool useParentHeight = true;

	[SerializeField]
	public IScrollRectProxy ScrollRectProxy;

	private int multiplier = 1;

	private Selectable currentSelectable;

	private int updateNumber = 2;

	private float size;

	private const float additionalPixelScroll = 15f;

	private ScrollRect _scrollRect;
}
