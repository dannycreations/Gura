using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownList : ActivityStateControlled
{
	public int value
	{
		get
		{
			return this._index;
		}
		set
		{
			this._index = value;
			this.RefreshShownValue();
		}
	}

	public List<Dropdown.OptionData> options
	{
		get
		{
			return this._options;
		}
		private set
		{
			this.ClearOptions();
			foreach (Dropdown.OptionData optionData in value)
			{
				this.AddOption(optionData);
			}
		}
	}

	public void ClearOptions()
	{
		if (this._options == null)
		{
			this._options = new List<Dropdown.OptionData>();
		}
		else
		{
			this._options.Clear();
		}
		this._index = 0;
		for (int i = 0; i < this._relatedObject.transform.childCount; i++)
		{
			Object.Destroy(this._relatedObject.transform.GetChild(i).gameObject);
		}
	}

	public void AddOption(Dropdown.OptionData option)
	{
		if (this.template == null)
		{
			return;
		}
		this._options.Add(option);
		GameObject gameObject = GUITools.AddChild(this._relatedObject, this.template);
		gameObject.GetComponentInChildren<Text>().text = option.text;
		gameObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			this.value = this._options.IndexOf(option);
		});
		gameObject.SetActive(true);
	}

	public void RefreshShownValue()
	{
		this.label.text = this._options[this.value].text;
		this.Hide();
	}

	public void Show()
	{
		this.OpenRegion();
	}

	public void Hide()
	{
		this.CloseRegion();
	}

	private void Awake()
	{
		this.closeBinding = new HotkeyBinding();
		this.closeBinding.Hotkey = InputControlType.Action2;
		this.closeBinding.KeyboardAltKey = 27;
		this.closeBinding.LocalizationKey = "CloseButton";
		if (this._dropDownOpenButton != null)
		{
			this._dropDownOpenButton.onClick.AddListener(new UnityAction(this.OpenRegion));
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.CloseRegion();
	}

	private void Destroy()
	{
		this.CloseRegion();
		if (this._dropDownOpenButton != null)
		{
			this._dropDownOpenButton.onClick.RemoveListener(new UnityAction(this.OpenRegion));
		}
	}

	protected override void SetHelp()
	{
		if (this.openned && InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad && this.closeBinding != null)
		{
			HelpLinePanel.SetActionHelp(this.closeBinding);
		}
	}

	protected override void HideHelp()
	{
		if (this.openned && this.closeBinding != null)
		{
			HelpLinePanel.HideActionHelp(this.closeBinding);
		}
	}

	private void OpenRegion()
	{
		if (this.openned)
		{
			return;
		}
		this.openned = true;
		UINavigation.SetSelectedGameObject(this._dropDownOpenButton.gameObject);
		base.InitHelp(true);
		this.insideButtons = this._transitionRegionRoot.GetComponentsInChildren<Button>(true);
		for (int i = 0; i < this.insideButtons.Length; i++)
		{
			this.insideButtons[i].onClick.AddListener(new UnityAction(this.CloseRegion));
		}
		if (this._relatedObject != null)
		{
			this._relatedObject.SetActive(true);
		}
	}

	private void CloseRegion()
	{
		if (!this.openned)
		{
			return;
		}
		base.InitHelp(false);
		this.openned = false;
		for (int i = 0; i < this.insideButtons.Length; i++)
		{
			this.insideButtons[i].onClick.RemoveListener(new UnityAction(this.CloseRegion));
		}
		if (this._relatedObject != null)
		{
			this._relatedObject.SetActive(false);
		}
		if (this._destroyOnClose)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			if (this.openned)
			{
				this.CloseRegion();
			}
			return;
		}
		if (this.openned)
		{
			if (!InputManager.ActiveDevice.GetControl(this.closeBinding.Hotkey).WasClicked && !Input.GetKeyDown(this.closeBinding.KeyboardAltKey))
			{
				if (!(EventSystem.current.currentSelectedGameObject != null))
				{
					return;
				}
				if (Array.FindIndex<Button>(this.insideButtons, (Button button) => object.ReferenceEquals(EventSystem.current.currentSelectedGameObject, button.gameObject)) != -1 || object.ReferenceEquals(EventSystem.current.currentSelectedGameObject, this._dropDownOpenButton.gameObject))
				{
					return;
				}
			}
			if (this.OnClick != null)
			{
				this.OnClick();
			}
			this.CloseRegion();
			return;
		}
	}

	[SerializeField]
	public Transform _transitionRegionRoot;

	[SerializeField]
	private Button _dropDownOpenButton;

	private Button[] insideButtons;

	[SerializeField]
	private GameObject _relatedObject;

	[SerializeField]
	private bool _destroyOnClose;

	private bool openned;

	private HotkeyBinding closeBinding;

	public Action OnClick;

	[SerializeField]
	private GameObject template;

	[SerializeField]
	private Text label;

	private int _index;

	private List<Dropdown.OptionData> _options;
}
