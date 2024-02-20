using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleStateChanges : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Toggle, bool> OnSelected = delegate(Toggle tgl, bool b)
	{
	};

	private void Awake()
	{
		this._tgl = base.GetComponent<Toggle>();
	}

	[ExecuteInEditMode]
	public void OnHighlight()
	{
		if (this.Disabled)
		{
			return;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.HighlightSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.HighlightTextColor;
			if (this._toggleLabelFontHighlight != null)
			{
				this.ToggleLabel.font = this._toggleLabelFontHighlight;
			}
		}
		if (this.ToggleLabel2 != null)
		{
			this.ToggleLabel2.color = this.HighlightTextColor;
		}
	}

	[ExecuteInEditMode]
	public void OnNormal()
	{
		if (this.Disabled)
		{
			return;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.NormalSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.NormalTextColor;
			if (this._toggleLabelFontHighlight != null)
			{
				this.ToggleLabel.font = this._toggleLabelFontNormal;
			}
		}
		if (this.ToggleLabel2 != null)
		{
			this.ToggleLabel2.color = this.NormalTextColor;
		}
		if (this.ToggleSprite2 != null)
		{
			this.ToggleSprite2.color = this.NormalSprite2Color;
		}
	}

	[ExecuteInEditMode]
	public void OnPressed()
	{
		if (this.Disabled)
		{
			return;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.PressSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.PressTextColor;
			if (this._toggleLabelFontHighlight != null)
			{
				this.ToggleLabel.font = this._toggleLabelFontNormal;
			}
		}
		if (this.ToggleLabel2 != null)
		{
			this.ToggleLabel2.color = this.PressTextColor;
		}
	}

	[ExecuteInEditMode]
	public void IsActive(bool state)
	{
		if (this.Disabled)
		{
			return;
		}
		if (state)
		{
			if (this.ToggleSprite != null)
			{
				this.ToggleSprite.color = this.ActiveSpriteColor;
			}
			if (this.ToggleLabel != null)
			{
				this.ToggleLabel.color = this.ActiveTextColor;
				if (this._toggleLabelFontHighlight != null)
				{
					this.ToggleLabel.font = this._toggleLabelFontHighlight;
				}
			}
			if (this.ToggleLabel2 != null)
			{
				this.ToggleLabel2.color = this.ActiveTextColor;
			}
			if (this.ToggleSprite2 != null)
			{
				this.ToggleSprite2.color = this.ActiveSprite2Color;
			}
		}
		else
		{
			this.OnNormal();
		}
	}

	[ExecuteInEditMode]
	private void Start()
	{
		if (this.Disabled)
		{
			this.Disable();
		}
		else
		{
			this.Enable();
		}
		this.IsActive(this._tgl.isOn);
		this._tgl.onValueChanged.AddListener(new UnityAction<bool>(this.OnChangeAction<bool>));
		if (this.ToggleLabel != null)
		{
			Button component = this.ToggleLabel.gameObject.GetComponent<Button>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	[ExecuteInEditMode]
	private void OnChangeAction<T0>(T0 arg0)
	{
		if (arg0 is bool)
		{
			this.IsActive((arg0 as bool?).Value);
		}
	}

	[ExecuteInEditMode]
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!this._tgl.isOn)
		{
			this.OnHighlight();
		}
	}

	[ExecuteInEditMode]
	public void OnPointerExit(PointerEventData eventData)
	{
		if (this._tgl != null && !this._tgl.isOn)
		{
			this.OnNormal();
		}
	}

	[ExecuteInEditMode]
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!this._tgl.isOn)
		{
			this.OnPressed();
		}
	}

	[ExecuteInEditMode]
	public void Disable()
	{
		if (this.changeInteractible)
		{
			this._tgl.interactable = false;
		}
		this.Disabled = true;
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.DisableSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.DisableTextColor;
			if (this._toggleLabelFontHighlight != null)
			{
				this.ToggleLabel.font = this._toggleLabelFontNormal;
			}
		}
		if (this.ToggleLabel2 != null)
		{
			this.ToggleLabel2.color = this.DisableTextColor;
		}
	}

	[ExecuteInEditMode]
	public void Enable()
	{
		if (this.changeInteractible)
		{
			this._tgl.interactable = true;
		}
		this.Disabled = false;
		this.OnNormal();
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this._isAutoOn)
			{
				this._tgl.isOn = true;
			}
			else
			{
				this.OnHighlight();
			}
			this.OnSelected(this._tgl, true);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this._isAutoOff)
			{
				this._tgl.isOn = false;
			}
			this.IsActive(this._tgl.isOn);
			this.OnSelected(this._tgl, false);
		}
	}

	[SerializeField]
	private Font _toggleLabelFontNormal;

	[SerializeField]
	private Font _toggleLabelFontHighlight;

	[SerializeField]
	private bool _isAutoOn;

	[SerializeField]
	private bool _isAutoOff;

	public Text ToggleLabel;

	public Text ToggleLabel2;

	public Image ToggleSprite;

	public Image ToggleSprite2;

	public Color HighlightSpriteColor;

	public Color HighlightTextColor;

	public Color ActiveSpriteColor;

	public Color ActiveTextColor;

	public Color NormalSpriteColor;

	public Color NormalTextColor;

	public Color PressSpriteColor;

	public Color PressTextColor;

	public Color NormalSprite2Color;

	public Color ActiveSprite2Color;

	public Color DisableSpriteColor;

	public Color DisableTextColor;

	public bool Disabled;

	[SerializeField]
	private bool changeInteractible = true;

	private Toggle _tgl;
}
