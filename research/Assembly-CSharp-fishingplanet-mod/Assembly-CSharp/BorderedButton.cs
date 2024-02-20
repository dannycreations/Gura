using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BorderedButton : Button
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> PointerEnter = delegate(bool b)
	{
	};

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (this._border != null && base.IsHighlighted(eventData) && this.IsInteractable())
		{
			this._border.color = this._borderVisibleColor;
		}
		if (this._border2 != null && base.IsHighlighted(eventData) && this.IsInteractable())
		{
			this._border2.color = this._border2VisibleColor;
		}
		this.PointerEnter(true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (this._border != null && !base.IsHighlighted(eventData))
		{
			this._border.color = this._borderInvisibleColor;
		}
		if (this._border2 != null && !base.IsHighlighted(eventData))
		{
			this._border2.color = this._border2InvisibleColor;
		}
		this.OnDeselect(new PointerEventData(EventSystem.current));
		this.PointerEnter(false);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (this._border != null && !base.IsHighlighted(eventData))
		{
			this._border.color = this._borderInvisibleColor;
		}
		if (this._border2 != null && !base.IsHighlighted(eventData))
		{
			this._border2.color = this._border2InvisibleColor;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (this._border != null && base.IsHighlighted(eventData) && this.IsInteractable())
		{
			this._border.color = this._borderVisibleColor;
		}
		if (this._border2 != null && base.IsHighlighted(eventData) && this.IsInteractable())
		{
			this._border2.color = this._border2VisibleColor;
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (this._border != null && !base.IsHighlighted(eventData))
		{
			this._border.color = this._borderInvisibleColor;
		}
		if (this._border2 != null && !base.IsHighlighted(eventData))
		{
			this._border2.color = this._border2InvisibleColor;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (this._border != null && base.IsHighlighted(eventData))
		{
			this._border.color = this._borderVisibleColor;
		}
		if (this._border2 != null && base.IsHighlighted(eventData))
		{
			this._border2.color = this._border2VisibleColor;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this._border != null)
		{
			if (base.currentSelectionState == 1)
			{
				this._border.color = this._borderVisibleColor;
			}
			else
			{
				this._border.color = this._borderInvisibleColor;
			}
		}
		if (this._border2 != null)
		{
			if (base.currentSelectionState == 1)
			{
				this._border2.color = this._border2VisibleColor;
			}
			else
			{
				this._border2.color = this._border2InvisibleColor;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	protected void Update()
	{
		if (this._text != null)
		{
			this._text.color = ((!base.interactable) ? this._textDisabledColor : this._textNormalColor);
		}
		if (this._icon != null)
		{
			this._icon.color = ((!base.interactable) ? this._textDisabledColor : this._textNormalColor);
		}
	}

	public void Highlight()
	{
		if (base.currentSelectionState == 1)
		{
			return;
		}
		this.DoStateTransition(1, true);
		if (this._border != null)
		{
			this._border.color = this._borderVisibleColor;
		}
		if (this._border2 != null)
		{
			this._border2.color = this._border2VisibleColor;
		}
	}

	public void Deselect()
	{
		if (base.currentSelectionState == 1)
		{
			return;
		}
		this.DoStateTransition(0, true);
		if (this._border != null)
		{
			this._border.color = this._borderInvisibleColor;
		}
		if (this._border2 != null)
		{
			this._border2.color = this._border2InvisibleColor;
		}
	}

	public RectTransform GetBorderTransform()
	{
		if (this._border)
		{
			return this._border.transform as RectTransform;
		}
		if (this._border2)
		{
			return this._border2.transform as RectTransform;
		}
		return base.transform as RectTransform;
	}

	[SerializeField]
	private Text _text;

	[SerializeField]
	private Text _icon;

	[SerializeField]
	private Color _textNormalColor;

	[SerializeField]
	private Color _textDisabledColor;

	[SerializeField]
	private Graphic _border;

	[SerializeField]
	private Graphic _border2;

	[SerializeField]
	private Color _borderVisibleColor = Color.white;

	[SerializeField]
	private Color _borderInvisibleColor = Color.white;

	[SerializeField]
	private Color _border2VisibleColor = Color.white;

	[SerializeField]
	private Color _border2InvisibleColor = Color.white;
}
