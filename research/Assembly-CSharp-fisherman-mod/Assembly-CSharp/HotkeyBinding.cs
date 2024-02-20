using System;
using InControl;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class HotkeyBinding
{
	public int Id
	{
		get
		{
			if (this._id < 0)
			{
				this._id = Guid.NewGuid().GetHashCode();
			}
			return this._id;
		}
	}

	public bool Check()
	{
		if (this.isClickedAction)
		{
			return InputManager.ActiveDevice.GetControl(this.Hotkey).WasClicked || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey).WasClicked || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey2).WasClicked || Input.GetKeyDown(this.KeyboardAltKey) || ((InputManager.ActiveDevice.GetControl(this.Hotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey2).WasRepeated || Input.GetKey(this.KeyboardAltKey)) && this.repeatable);
		}
		if (this.repeatable)
		{
			return InputManager.ActiveDevice.GetControl(this.Hotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey).WasRepeated || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey2).WasRepeated || Input.GetKey(this.KeyboardAltKey);
		}
		if (this.isLongPressed)
		{
			return InputManager.ActiveDevice.GetControl(this.Hotkey).WasLongPressed || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey).WasLongPressed || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey2).WasLongPressed || Input.GetKeyDown(this.KeyboardAltKey);
		}
		return InputManager.ActiveDevice.GetControl(this.Hotkey).WasPressed || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey).WasPressed || InputManager.ActiveDevice.GetControl(this.AlternativeHotkey2).WasPressed || Input.GetKeyDown(this.KeyboardAltKey);
	}

	[SerializeField]
	public Selectable Selectable;

	[SerializeField]
	public InputControlType Hotkey;

	[SerializeField]
	public InputControlType AlternativeHotkey;

	[SerializeField]
	public InputControlType AlternativeHotkey2;

	[SerializeField]
	public KeyCode KeyboardAltKey;

	[SerializeField]
	public string LocalizationKey;

	[SerializeField]
	public bool disableSelectable = true;

	[SerializeField]
	public bool repeatable;

	[SerializeField]
	public bool appendToText;

	[SerializeField]
	public bool appendSpacesAfterKey = true;

	[SerializeField]
	public bool isClickedAction;

	[SerializeField]
	public bool isLongPressed;

	[SerializeField]
	public bool IsOpposite;

	[SerializeField]
	public bool IsIncreasing;

	[SerializeField]
	public HotkeyIcons.OppositeIconTypes OppositeIcon;

	private int _id = -1;
}
