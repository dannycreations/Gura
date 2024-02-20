using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderToggle : Slider, IPointerClickHandler, IBeginDragHandler, ISubmitHandler, IEventSystemHandler
{
	public bool isOn
	{
		get
		{
			return this._boolValue;
		}
		set
		{
			this.SetValue(value);
		}
	}

	private void Highlight()
	{
		if (base.interactable)
		{
			this._highlighted = true;
			this.ProcessColors(this.value);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.Dehighlight();
	}

	private void Dehighlight()
	{
		this._highlighted = false;
		this.ProcessColors(this.value);
	}

	protected override void Awake()
	{
		base.Awake();
		this.lastValidValue = this.value;
		base.onValueChanged.AddListener(new UnityAction<float>(this.OnValueChanged));
		this._playButtonEffect = base.GetComponent<PlayButtonEffect>();
	}

	protected override void OnDestroy()
	{
		base.onValueChanged.RemoveAllListeners();
		base.OnDestroy();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		this.Highlight();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		this.Dehighlight();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!this._dragging)
		{
			this.SetValue(!this._boolValue);
		}
		this._dragging = false;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		this._firstPress = false;
		if ((!eventData.eligibleForClick && eventData.dragging) || this._dragging)
		{
			this.SetValue(this.value >= 0.5f);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		this._firstPress = true;
		this._dragging = false;
		base.OnPointerDown(eventData);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		this.Highlight();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		this.Dehighlight();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		this._firstPress = false;
		this._dragging = true;
	}

	public override void OnDrag(PointerEventData eventData)
	{
		this._firstPress = false;
		this._dragging = true;
		base.OnDrag(eventData);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (!this._dragging)
		{
			this.SetValue(!this._boolValue);
		}
		this._dragging = false;
	}

	protected void OnValueChanged(float currValue)
	{
		if (!DOTween.IsTweening(this, false) && !this._dragging && this._firstPress && Mathf.Abs(currValue - this.lastValidValue) / Time.deltaTime > 7.3333335f)
		{
			this.value = this.lastValidValue;
			return;
		}
		this.lastValidValue = currValue;
		this.ProcessColors(currValue);
	}

	private void ProcessColors(float currValue)
	{
		Color color = ((!this._highlighted) ? this.HandleOffColor : this.HandleOffHighlightedColor);
		Color color2 = ((!this._highlighted) ? this.HandleOnColor : this.HandleOnHighlightedColor);
		Color color3 = ((!this._highlighted) ? this.BgOffColor : this.BgOffHighlightedColor);
		Color color4 = ((!this._highlighted) ? this.BgOnColor : this.BgOnHighlightedColor);
		this.Handle.color = Color.Lerp(color, color2, currValue);
		this.Background.color = Color.Lerp(color3, color4, currValue);
	}

	public void SetValue(bool booLValue)
	{
		ShortcutExtensions.DOKill(this, false);
		this._boolValue = booLValue;
		float num = ((!booLValue) ? 0f : 1f);
		float num2 = Mathf.Abs(num - this.value) / 6.6666665f;
		if (base.gameObject.activeInHierarchy)
		{
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOValue(this, num, num2, false), 6);
			if (this._playButtonEffect != null)
			{
				this._playButtonEffect.OnSubmit();
			}
		}
		else
		{
			this.value = num;
			this.ProcessColors(this.value);
		}
	}

	public Graphic Handle;

	public Graphic Background;

	public Color HandleOnColor;

	public Color HandleOnHighlightedColor;

	public Color HandleOffColor;

	public Color HandleOffHighlightedColor;

	public Color BgOnColor;

	public Color BgOnHighlightedColor;

	public Color BgOffColor;

	public Color BgOffHighlightedColor;

	private PlayButtonEffect _playButtonEffect;

	private bool _highlighted;

	private bool _boolValue;

	private const float FinishMaxTime = 0.15f;

	private const float FinishSpeed = 6.6666665f;

	private bool _firstPress;

	private bool _dragging;

	private float lastValidValue;
}
