using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotkeyPressRedirect : ActivityStateControlled
{
	private bool PausedFromScript { get; set; }

	public void SetPausedFromScript(bool flag)
	{
		this.PausedFromScript = flag;
		this.ClearIncreasing();
	}

	public static void PauseForLayersLess(int layer)
	{
		for (int i = 0; i < HotkeyPressRedirect.allHotkeys.Count; i++)
		{
			if (HotkeyPressRedirect.allHotkeys[i].visibleOnLayer < layer)
			{
				HotkeyPressRedirect.allHotkeys[i].paused = true;
			}
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		for (int i = 0; i < HotkeyPressRedirect.allHotkeys.Count; i++)
		{
			if (HotkeyPressRedirect.allHotkeys[i].visibleOnLayer >= layer)
			{
				HotkeyPressRedirect.allHotkeys[i].paused = false;
			}
		}
	}

	private void AppendHotkeyIcons(InputModuleManager.InputType inputType)
	{
		for (int i = 0; i < this._bindings.Length; i++)
		{
			if (this._bindings[i].Selectable != null && this._bindings[i].appendToText)
			{
				Text componentInChildren = this._bindings[i].Selectable.GetComponentInChildren<Text>();
				if (componentInChildren != null)
				{
					string text = ((this._bindings[i].AlternativeHotkey == InputControlType.None) ? string.Empty : (" " + HotkeyIcons.KeyMappings[this._bindings[i].Hotkey]));
					componentInChildren.text = componentInChildren.text.Replace(HotkeyIcons.KeyMappings[this._bindings[i].Hotkey] + text + ((!this._bindings[i].appendSpacesAfterKey) ? string.Empty : "   "), string.Empty);
					if (inputType == InputModuleManager.InputType.GamePad)
					{
						componentInChildren.text = HotkeyIcons.KeyMappings[this._bindings[i].Hotkey] + text + ((!this._bindings[i].appendSpacesAfterKey) ? string.Empty : "   ") + componentInChildren.text;
					}
				}
			}
		}
	}

	private void AddIgnoreComponent()
	{
		for (int i = 0; i < this._bindings.Length; i++)
		{
			if (this._bindings[i].Selectable != null && this._bindings[i].disableSelectable)
			{
				this._bindings[i].Selectable.gameObject.AddComponent<IgnoredSelectable>();
			}
		}
	}

	public void ForceAppendHotkeyIcons()
	{
		this.AppendHotkeyIcons(InputModuleManager.GameInputType);
	}

	protected override void Start()
	{
		base.Start();
		InputModuleManager.OnInputTypeChanged += this.AppendHotkeyIcons;
		this.AppendHotkeyIcons(InputModuleManager.GameInputType);
		this.AddIgnoreComponent();
	}

	private void Awake()
	{
		HotkeyPressRedirect.allHotkeys.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InputModuleManager.OnInputTypeChanged -= this.AppendHotkeyIcons;
		HotkeyPressRedirect.allHotkeys.Remove(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.resetActivityOnDisable)
		{
			base.enabled = false;
		}
		this.waiting = false;
	}

	public void StartListenForHotkeys()
	{
		if (this.waiting || !base.gameObject.activeSelf || !base.ShouldUpdate())
		{
			return;
		}
		this.RefreshVisibleLayer();
		this.waiting = true;
		base.InitHelp(true);
	}

	public void RefreshVisibleLayer()
	{
		if (this._activityState == null && this.activityStateChecked && base.transform.hasChanged)
		{
			base.CheckActivityState();
		}
		if (this._blockableRegion == null && this._blockableChecked && this._activityState == null && this.activityStateChecked && base.transform.hasChanged)
		{
			this._blockableChecked = false;
		}
		if (!this._blockableChecked)
		{
			this._blockableRegion = base.GetComponentInParent<BlockableRegion>();
			this._blockableChecked = true;
		}
		if (this._blockableRegion != null)
		{
			this.visibleOnLayer = this._blockableRegion.Layer;
		}
		this.paused = this.visibleOnLayer < BlockableRegion.CurrentLayer;
		base.transform.hasChanged = false;
	}

	protected override void SetHelp()
	{
		if (this.PausedFromScript)
		{
			return;
		}
		this.RefreshVisibleLayer();
		if (this.controlledByEnabling)
		{
			this.waiting = true;
		}
		if (!this.waiting)
		{
			return;
		}
		for (int i = 0; i < this._bindings.Length; i++)
		{
			if (this._bindings[i].Selectable != null && this._bindings[i].Selectable.gameObject.activeInHierarchy && this._bindings[i].Selectable.interactable)
			{
				HelpLinePanel.SetActionHelp(this._bindings[i]);
				if (ControlsController.ControlsActions != null && HotkeyPressRedirect._pondHelpers.PondControllerList != null)
				{
					ControlsController.ControlsActions.AddIgnoreControlType((int)this._bindings[i].Hotkey);
					ControlsController.ControlsActions.AddIgnoreControlType((int)this._bindings[i].AlternativeHotkey);
					ControlsController.ControlsActions.AddIgnoreControlType((int)this._bindings[i].AlternativeHotkey2);
				}
			}
		}
	}

	protected override void HideHelp()
	{
		if (!this.waiting)
		{
			return;
		}
		for (int i = 0; i < this._bindings.Length; i++)
		{
			HelpLinePanel.HideActionHelp(this._bindings[i]);
			if (ControlsController.ControlsActions != null)
			{
				ControlsController.ControlsActions.RemoveControlType((int)this._bindings[i].Hotkey);
				ControlsController.ControlsActions.RemoveControlType((int)this._bindings[i].AlternativeHotkey);
				ControlsController.ControlsActions.RemoveControlType((int)this._bindings[i].AlternativeHotkey2);
			}
		}
	}

	public void StopListenForHotKeys()
	{
		if (!this.waiting)
		{
			return;
		}
		base.InitHelp(false);
		this.waiting = false;
		this.ClearIncreasing();
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			if (this.waiting && this.helpInited)
			{
				base.InitHelp(false);
			}
			return;
		}
		if (this.waiting && !this.helpInited)
		{
			base.InitHelp(true);
		}
		if (!this.PausedFromScript && this.waiting && !this.paused && InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			for (int i = 0; i < this._bindings.Length; i++)
			{
				if (!(this._bindings[i].Selectable == null) && this._bindings[i].Selectable.gameObject.activeSelf)
				{
					if (this._bindings[i].isClickedAction || this._bindings[i].repeatable)
					{
						if (!this._bindings[i].isLongPressed)
						{
							if (InputManager.ActiveDevice.GetControl(this._bindings[i].Hotkey).WasPressed || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey).WasPressed || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey2).WasPressed || Input.GetKeyDown(this._bindings[i].KeyboardAltKey) || ((InputManager.ActiveDevice.GetControl(this._bindings[i].Hotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey2).WasRepeated || Input.GetKey(this._bindings[i].KeyboardAltKey)) && this._bindings[i].repeatable))
							{
								this.SubmitAction(i);
								this.CheckIncreasing(this._bindings[i], i);
							}
						}
						else if (InputManager.ActiveDevice.GetControl(this._bindings[i].Hotkey).WasLongPressed || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey).WasLongPressed || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey2).WasLongPressed)
						{
							this.SubmitAction(i);
						}
					}
					else if (InputManager.ActiveDevice.GetControl(this._bindings[i].Hotkey).WasClicked || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey).WasClicked || InputManager.ActiveDevice.GetControl(this._bindings[i].AlternativeHotkey2).WasClicked || Input.GetKeyUp(this._bindings[i].KeyboardAltKey))
					{
						this.SubmitAction(i);
					}
				}
			}
		}
	}

	private void CheckIncreasing(HotkeyBinding b, int i)
	{
		if (b.IsIncreasing)
		{
			int id = b.Id;
			if (!this._increasing.ContainsKey(id))
			{
				this._increasing[id] = new List<DateTime>();
				this._increasingCounter[id] = 0;
			}
			this._increasing[id].Add(DateTime.Now);
			int count = this._increasing[id].Count;
			DateTime dateTime = this._increasing[id][count - 1];
			double totalSeconds = (dateTime - this._increasing[id][0]).TotalSeconds;
			double num = 0.0;
			if (count > 1)
			{
				num = (dateTime - this._increasing[id][count - 2]).TotalSeconds;
			}
			if (num > 2.0)
			{
				this._increasingCounter[id] = 0;
				this._increasing[id].Clear();
				this._increasing[id].Add(DateTime.Now);
			}
			else if (totalSeconds >= 1.5)
			{
				Dictionary<int, int> increasingCounter;
				int num2;
				(increasingCounter = this._increasingCounter)[num2 = id] = increasingCounter[num2] + 1;
				int num3 = this._increasingCounter[id];
				for (int j = 0; j < num3; j++)
				{
					this.SubmitAction(i);
				}
			}
		}
	}

	private void ClearIncreasing()
	{
		this._increasing.Clear();
		this._increasingCounter.Clear();
	}

	private void SubmitAction(int i)
	{
		if (this._bindings[i].Selectable != null && this._bindings[i].Selectable.gameObject.activeSelf)
		{
			ISubmitHandler submitHandler = this._bindings[i].Selectable as ISubmitHandler;
			if (submitHandler != null)
			{
				submitHandler.OnSubmit(null);
			}
		}
	}

	[SerializeField]
	private HotkeyBinding[] _bindings = new HotkeyBinding[0];

	[SerializeField]
	private bool controlledByEnabling;

	[Obsolete("Never use!")]
	[SerializeField]
	public bool resetActivityOnDisable;

	private int visibleOnLayer;

	private bool waiting;

	private bool paused;

	private static List<HotkeyPressRedirect> allHotkeys = new List<HotkeyPressRedirect>();

	private bool[] _activity;

	private static PondHelpers _pondHelpers = new PondHelpers();

	private const int IncreasingSpeed = 1;

	private const float TimeForIncreasing = 1.5f;

	private const float TimeForStopIncreasing = 2f;

	private Dictionary<int, List<DateTime>> _increasing = new Dictionary<int, List<DateTime>>();

	private Dictionary<int, int> _increasingCounter = new Dictionary<int, int>();

	private BlockableRegion _blockableRegion;

	private bool _blockableChecked;
}
