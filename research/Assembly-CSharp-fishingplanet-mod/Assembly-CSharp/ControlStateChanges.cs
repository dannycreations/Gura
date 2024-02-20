using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlStateChanges : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IEventSystemHandler
{
	[ExecuteInEditMode]
	public void OnHighlight()
	{
		if (this.Disabled)
		{
			return;
		}
		this._state = ControlStateChanges.StateType.HightLighted;
		if (this.Sprite != null)
		{
			this.Sprite.color = this.HighlightSpriteColor;
		}
		if (this.Label != null)
		{
			this.Label.color = this.HighlightTextColor;
		}
		if (this.Label2 != null)
		{
			this.Label2.color = this.HighlightTextColor;
		}
	}

	[ExecuteInEditMode]
	public void OnNormal()
	{
		if (this.Disabled)
		{
			return;
		}
		this._state = ControlStateChanges.StateType.Normal;
		if (this.Sprite != null)
		{
			this.Sprite.color = this.NormalSpriteColor;
		}
		if (this.Label != null)
		{
			this.Label.color = this.NormalTextColor;
		}
		if (this.Label2 != null)
		{
			this.Label2.color = this.NormalTextColor;
		}
	}

	[ExecuteInEditMode]
	public void OnPressed()
	{
		if (this.Disabled)
		{
			return;
		}
		this._state = ControlStateChanges.StateType.Pressed;
		if (this.Sprite != null)
		{
			this.Sprite.color = this.PressSpriteColor;
		}
		if (this.Label != null)
		{
			this.Label.color = this.PressTextColor;
		}
		if (this.Label2 != null)
		{
			this.Label2.color = this.PressTextColor;
		}
	}

	public void OnSelected()
	{
		if (this.Sprite != null)
		{
			this.Sprite.color = this.HighlightSpriteColor;
		}
		if (this.Label != null)
		{
			this.Label.color = this.HighlightTextColor;
		}
		if (this.Label2 != null)
		{
			this.Label2.color = this.HighlightTextColor;
		}
	}

	[ExecuteInEditMode]
	private void Start()
	{
		if (this.Sprite != null)
		{
			this.ButtonComponent = this.Sprite.GetComponent<Button>();
		}
		if (this.Disabled)
		{
			this.Disable();
		}
		else
		{
			this.Enable();
		}
	}

	private void Update()
	{
		if (this.ButtonComponent != null && this.ButtonComponent.interactable && this.Disabled)
		{
			this.Enable();
		}
		if (this.ButtonComponent != null && !this.ButtonComponent.interactable && !this.Disabled)
		{
			this.Disable();
		}
		if (EventSystem.current.currentSelectedGameObject == base.gameObject && this._state == ControlStateChanges.StateType.Normal)
		{
			this.OnSelected();
		}
		else
		{
			ControlStateChanges.StateType state = this._state;
			if (state != ControlStateChanges.StateType.Normal)
			{
				if (state != ControlStateChanges.StateType.Pressed)
				{
					if (state == ControlStateChanges.StateType.HightLighted)
					{
						this.OnHighlight();
					}
				}
				else
				{
					this.OnPressed();
				}
			}
			else
			{
				this.OnNormal();
			}
		}
	}

	[ExecuteInEditMode]
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (this.Disabled)
		{
			return;
		}
		this.OnHighlight();
	}

	[ExecuteInEditMode]
	public void OnPointerExit(PointerEventData eventData)
	{
		if (this.Disabled)
		{
			return;
		}
		this.OnNormal();
	}

	[ExecuteInEditMode]
	public void OnPointerClick(PointerEventData eventData)
	{
		if (this.Disabled)
		{
			return;
		}
		this.OnPressed();
	}

	[ExecuteInEditMode]
	public void Disable()
	{
		if (!base.enabled)
		{
			return;
		}
		this._state = ControlStateChanges.StateType.Disabled;
		this.Disabled = true;
		if (this.Sprite != null)
		{
			if (this.Sprite.GetComponent<Button>() != null)
			{
				this.Sprite.GetComponent<Button>().interactable = false;
			}
			this.Sprite.color = this.DisableSpriteColor;
		}
		if (this.Label != null)
		{
			this.Label.color = this.DisableTextColor;
		}
		if (this.Label2 != null)
		{
			this.Label2.color = this.DisableTextColor;
		}
	}

	[ExecuteInEditMode]
	public void Enable()
	{
		if (!base.enabled)
		{
			return;
		}
		this.Disabled = false;
		if (this.Sprite != null && this.Sprite.GetComponent<Button>() != null)
		{
			this.Sprite.GetComponent<Button>().interactable = true;
		}
		this.OnNormal();
	}

	private void OnDisable()
	{
		if (!this.Disabled)
		{
			this.OnNormal();
		}
	}

	private void OnEnable()
	{
		if (!this.Disabled)
		{
			this.OnNormal();
		}
	}

	public Text Label;

	public Text Label2;

	public Image Sprite;

	public Color HighlightSpriteColor;

	public Color HighlightTextColor;

	public Color NormalSpriteColor;

	public Color NormalTextColor;

	public Color PressSpriteColor;

	public Color PressTextColor;

	public Color DisableSpriteColor;

	public Color DisableTextColor;

	public bool Disabled;

	private Button ButtonComponent;

	private ControlStateChanges.StateType _state;

	public enum StateType
	{
		Normal,
		HightLighted,
		Pressed,
		Disabled
	}
}
