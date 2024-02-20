using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleColorTransitionChanges : ActivityStateControlled, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ToggleColorTransitionChanges.SelectionState> OnStateChanged = delegate(ToggleColorTransitionChanges.SelectionState s)
	{
	};

	[ExecuteInEditMode]
	public void OnHighlight()
	{
		this._selectionState = ToggleColorTransitionChanges.SelectionState.Highlighted;
		this.OnStateChanged(this._selectionState);
		if (this.ToggleHighlightedSprite != null)
		{
			this.ToggleHighlightedSprite.color = this.HighlightSpriteColor;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.HighlightSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.HighlightTextColor;
		}
		if (this.TMPToggleLabel != null)
		{
			this.TMPToggleLabel.color = this.HighlightTextColor;
		}
		if (this.ToggleIcon != null)
		{
			this.ToggleIcon.color = this.HighlightTextColor;
		}
		if (this._toggleSprite2 != null)
		{
			this._toggleSprite2.color = this._highlightSpriteColor2;
		}
		this.UpdateToggleGraphics();
	}

	[ExecuteInEditMode]
	public void OnNormal()
	{
		this._selectionState = ToggleColorTransitionChanges.SelectionState.Normal;
		this.OnStateChanged(this._selectionState);
		if (this.ToggleHighlightedSprite != null)
		{
			this.ToggleHighlightedSprite.color = this.NormalSpriteColor;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.NormalSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.NormalTextColor;
		}
		if (this.TMPToggleLabel != null)
		{
			this.TMPToggleLabel.color = this.NormalTextColor;
		}
		if (this.ToggleIcon != null)
		{
			this.ToggleIcon.color = this.NormalTextColor;
		}
		if (this._toggleSprite2 != null)
		{
			this._toggleSprite2.color = this._normalSpriteColor2;
		}
		this.UpdateToggleGraphics();
	}

	[ExecuteInEditMode]
	public void OnPressed()
	{
		this._selectionState = ToggleColorTransitionChanges.SelectionState.Pressed;
		this.OnStateChanged(this._selectionState);
		if (this.ToggleHighlightedSprite != null)
		{
			this.ToggleHighlightedSprite.color = this.PressSpriteColor;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.PressSpriteColor;
		}
		if (this._toggleSprite2 != null)
		{
			this._toggleSprite2.color = this._pressSpriteColor2;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.PressTextColor;
		}
		if (this.TMPToggleLabel != null)
		{
			this.TMPToggleLabel.color = this.PressTextColor;
		}
		if (this.ToggleIcon != null)
		{
			this.ToggleIcon.color = this.PressTextColor;
		}
		this.UpdateToggleGraphics();
	}

	[ExecuteInEditMode]
	public void IsActive(bool state)
	{
		if (state)
		{
			this._selectionState = ToggleColorTransitionChanges.SelectionState.Active;
			this.OnStateChanged(this._selectionState);
			if (this.ToggleHighlightedSprite != null)
			{
				this.ToggleHighlightedSprite.color = this.ActiveSpriteColor;
			}
			if (this.ToggleSprite != null)
			{
				this.ToggleSprite.color = this.ActiveSpriteColor;
			}
			if (this.ToggleLabel != null)
			{
				this.ToggleLabel.color = this.ActiveTextColor;
			}
			if (this.TMPToggleLabel != null)
			{
				this.TMPToggleLabel.color = this.ActiveTextColor;
			}
			if (this.ToggleIcon != null)
			{
				this.ToggleIcon.color = this.ActiveTextColor;
			}
			if (this._toggleSprite2 != null)
			{
				this._toggleSprite2.color = this._activeSpriteColor2;
			}
			this.UpdateToggleGraphics();
		}
		else
		{
			this.OnNormal();
		}
	}

	public void Subscribe()
	{
		if (!this.subscribed)
		{
			base.GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(this.OnChangeAction<bool>));
		}
		this.subscribed = true;
	}

	[ExecuteInEditMode]
	protected override void Start()
	{
		base.Start();
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.Enable();
			if (!this.isSelected)
			{
				this.IsActive(base.GetComponent<Toggle>().isOn);
			}
			this.Subscribe();
		}
		else
		{
			this.Disable();
		}
	}

	[ExecuteInEditMode]
	private void OnChangeAction<T0>(T0 arg0)
	{
		if (arg0 is bool && this._selectionState != ToggleColorTransitionChanges.SelectionState.Highlighted)
		{
			this.IsActive((arg0 as bool?).Value);
		}
	}

	[ExecuteInEditMode]
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.OnHighlight();
		}
	}

	[ExecuteInEditMode]
	public void OnPointerExit(PointerEventData eventData)
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this._selectionState != ToggleColorTransitionChanges.SelectionState.Disabled)
		{
			this.IsActive(base.GetComponent<Toggle>().isOn);
		}
	}

	[ExecuteInEditMode]
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (base.GetComponent<Toggle>().IsInteractable() && !base.GetComponent<Toggle>().isOn)
		{
			this.OnPressed();
		}
	}

	[ExecuteInEditMode]
	public void Disable()
	{
		this._selectionState = ToggleColorTransitionChanges.SelectionState.Disabled;
		this.OnStateChanged(this._selectionState);
		if (this.ToggleHighlightedSprite != null)
		{
			this.ToggleHighlightedSprite.color = this.DisableSpriteColor;
		}
		if (this.ToggleSprite != null)
		{
			this.ToggleSprite.color = this.DisableSpriteColor;
		}
		if (this.ToggleLabel != null)
		{
			this.ToggleLabel.color = this.DisableTextColor;
		}
		if (this.TMPToggleLabel != null)
		{
			this.TMPToggleLabel.color = this.DisableTextColor;
		}
		if (this.ToggleIcon != null)
		{
			this.ToggleIcon.color = this.DisableTextColor;
		}
		if (this._toggleSprite2 != null)
		{
			this._toggleSprite2.color = this._disableSpriteColor2;
		}
		this.UpdateToggleGraphics();
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

	protected override void HideHelp()
	{
		this.isSelected = false;
		this.OnNormal();
	}

	protected override void SetHelp()
	{
		if (base.GetComponent<Toggle>().IsInteractable())
		{
			this.Enable();
			if (!this.isSelected)
			{
				this.IsActive(base.GetComponent<Toggle>().isOn);
			}
			this.Subscribe();
		}
		else
		{
			this.Disable();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		this.isSelected = true;
		this.OnHighlight();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.OnDeselect();
	}

	public void OnDeselect()
	{
		this.isSelected = false;
		this.IsActive(base.GetComponent<Toggle>().isOn);
	}

	private void UpdateToggleGraphics()
	{
		if (this._toggleGraphics != null)
		{
			for (int i = 0; i < this._toggleGraphics.Length; i++)
			{
				ToggleColorTransitionChanges.TransitionColors transitionColors = this._toggleGraphics[i];
				for (int j = 0; j < transitionColors.ColorStates.Length; j++)
				{
					if (transitionColors.ColorStates[j].State == this._selectionState)
					{
						transitionColors.Graphic.color = transitionColors.ColorStates[j].ColorState;
						break;
					}
				}
			}
		}
	}

	[SerializeField]
	private ToggleColorTransitionChanges.TransitionColors[] _toggleGraphics;

	[SerializeField]
	private Image _toggleSprite2;

	[SerializeField]
	private Color _highlightSpriteColor2;

	[SerializeField]
	private Color _activeSpriteColor2;

	[SerializeField]
	private Color _normalSpriteColor2;

	[SerializeField]
	private Color _pressSpriteColor2;

	[SerializeField]
	private Color _disableSpriteColor2;

	[Space(5f)]
	public Text ToggleLabel;

	public TextMeshProUGUI TMPToggleLabel;

	public Text ToggleIcon;

	public Image ToggleSprite;

	public Image ToggleHighlightedSprite;

	public Color HighlightSpriteColor;

	public Color HighlightTextColor;

	public Color ActiveSpriteColor;

	public Color ActiveTextColor;

	public Color NormalSpriteColor;

	public Color NormalTextColor;

	public Color PressSpriteColor;

	public Color PressTextColor;

	public Color DisableSpriteColor;

	public Color DisableTextColor;

	private bool isSelected;

	private ToggleColorTransitionChanges.SelectionState _selectionState;

	private bool subscribed;

	public enum SelectionState
	{
		Normal,
		Highlighted,
		Pressed,
		Disabled,
		Active
	}

	[Serializable]
	public class TransitionColors
	{
		[SerializeField]
		public Graphic Graphic;

		[SerializeField]
		public ToggleColorTransitionChanges.TransitionColorStates[] ColorStates;
	}

	[Serializable]
	public class TransitionColorStates
	{
		[SerializeField]
		public ToggleColorTransitionChanges.SelectionState State;

		[SerializeField]
		public Color ColorState;
	}
}
