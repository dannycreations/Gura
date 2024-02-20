using System;
using InControl;

public class CustomPlayerAction : PlayerAction
{
	public CustomPlayerAction(string name, PlayerActionSet owner, string localizationKey = "", ControlsActionsCategories category = ControlsActionsCategories.Misc)
		: base(name, owner)
	{
		this.CAInstance = (ControlsActions)owner;
		this.LocKey = localizationKey;
		this.Category = category;
	}

	public string LocalizationKey
	{
		get
		{
			return string.IsNullOrEmpty(this.LocKey) ? base.Name : this.LocKey;
		}
	}

	public ControlsActionsCategories Category { get; protected set; }

	protected string LocKey { get; set; }

	public override bool IsPressed
	{
		get
		{
			return !this.IsIgnored && !this._isBlocked && (!this.CAInstance.IsBlockedKeyboardInput || this.CAInstance.ExcludeInputList.Contains(base.Name)) && base.IsPressed;
		}
	}

	public override bool WasPressed
	{
		get
		{
			return !this.IsIgnored && !this._isBlocked && (!this.CAInstance.IsBlockedKeyboardInput || this.CAInstance.ExcludeInputList.Contains(base.Name)) && base.WasPressed;
		}
	}

	public override bool WasReleased
	{
		get
		{
			return !this.IsIgnored && !this._isBlocked && (!this.CAInstance.IsBlockedKeyboardInput || this.CAInstance.ExcludeInputList.Contains(base.Name)) && base.WasReleased;
		}
	}

	public bool IsPressedSimultaneous(CustomPlayerAction action)
	{
		return !this.IsIgnored && !action.IsIgnored && !this._isBlocked && ((this.WasPressed && action.IsPressed) || (action.WasPressed && this.IsPressed));
	}

	public override bool WasClicked
	{
		get
		{
			return !this.IsIgnored && !this._isBlocked && (!this.CAInstance.IsBlockedKeyboardInput || this.CAInstance.ExcludeInputList.Contains(base.Name)) && base.WasClicked;
		}
	}

	public bool IsIgnored
	{
		get
		{
			if (this.CAInstance.IgnoreControlTypes.Count > 0)
			{
				for (int i = 0; i < base.Bindings.Count; i++)
				{
					DeviceBindingSource deviceBindingSource = base.Bindings[i] as DeviceBindingSource;
					if (deviceBindingSource != null && this.CAInstance.ContainsControlInIgnores((int)deviceBindingSource.Control))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool WasClickedMandatory
	{
		get
		{
			return base.WasClicked;
		}
	}

	public bool IsPressedMandatory
	{
		get
		{
			return base.IsPressed;
		}
	}

	public bool WasPressedMandatory
	{
		get
		{
			return base.WasPressed;
		}
	}

	public bool WasReleasedMandatory
	{
		get
		{
			return base.WasReleased;
		}
	}

	private ControlsActions CAInstance;
}
