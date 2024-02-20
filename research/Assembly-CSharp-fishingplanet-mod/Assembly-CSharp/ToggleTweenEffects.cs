using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleTweenEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	[ExecuteInEditMode]
	public void OnHighlight()
	{
		DOTween.Kill(this.ToggleTransform, false);
		this._selectionState = ToggleTweenEffects.SelectionState.Highlighted;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this.ToggleTransform, this.initialScale * this.ScaleValue, this.InTime), this.InEasing);
	}

	[ExecuteInEditMode]
	public void OnNormal()
	{
		this._selectionState = ToggleTweenEffects.SelectionState.Normal;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this.ToggleTransform, this.initialScale, this.OutTime), this.OutEasing);
	}

	[ExecuteInEditMode]
	public void OnPressed()
	{
		this._selectionState = ToggleTweenEffects.SelectionState.Pressed;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this.ToggleTransform, this.initialScale * this.ScaleValue, this.InTime), this.InEasing);
	}

	[ExecuteInEditMode]
	public void IsActive(bool state)
	{
		this.OnNormal();
	}

	private void Awake()
	{
		if (this.initialScale == Vector3.zero)
		{
			this.initialScale = this.ToggleTransform.localScale;
		}
	}

	[ExecuteInEditMode]
	private void Start()
	{
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.Enable();
			if (!this.isSelected)
			{
				this.IsActive(base.GetComponent<Toggle>().isOn);
			}
			base.GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(this.OnChangeAction<bool>));
		}
		else
		{
			this.Disable();
		}
	}

	[ExecuteInEditMode]
	private void OnChangeAction<T0>(T0 arg0)
	{
		if (arg0 is bool && this._selectionState != ToggleTweenEffects.SelectionState.Highlighted)
		{
			this.IsActive((arg0 as bool?).Value);
		}
	}

	[ExecuteInEditMode]
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.OnHighlight();
		}
	}

	[ExecuteInEditMode]
	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.IsActive(base.GetComponent<Toggle>().isOn);
		}
	}

	[ExecuteInEditMode]
	public void OnPointerClick(PointerEventData eventData)
	{
		if (base.GetComponent<Toggle>().IsInteractable() && !base.GetComponent<Toggle>().isOn)
		{
			this.OnPressed();
		}
	}

	[ExecuteInEditMode]
	public void Disable()
	{
		this._selectionState = ToggleTweenEffects.SelectionState.Disabled;
		this.OnNormal();
	}

	[ExecuteInEditMode]
	public void Enable()
	{
		if (!this.isSelected)
		{
			this.OnNormal();
		}
		else
		{
			this.OnHighlight();
		}
	}

	private void OnDisable()
	{
		this.isSelected = false;
		this.OnNormal();
	}

	private void OnEnable()
	{
		this.Enable();
		if (!this.isSelected)
		{
			this.IsActive(base.GetComponent<Toggle>().isOn);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		this.isSelected = true;
		this.OnHighlight();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.isSelected = false;
		this.IsActive(base.GetComponent<Toggle>().isOn);
	}

	public Transform ToggleTransform;

	public float ScaleValue;

	public float InTime;

	public float OutTime;

	public Ease InEasing;

	public Ease OutEasing;

	public Vector3 initialScale;

	private bool isSelected;

	private ToggleTweenEffects.SelectionState _selectionState;

	protected enum SelectionState
	{
		Normal,
		Highlighted,
		Pressed,
		Disabled,
		Active
	}
}
