using System;
using InControl;
using UnityEngine;

public class RemapControls : ActivityStateControlled
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.OnDeselect();
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetActionHelp(this.binding);
	}

	protected override void HideHelp()
	{
		HelpLinePanel.HideActionHelp(this.binding);
	}

	public void OnSelect()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			base.InitHelp(true);
			this.firstMapping.StartListenForHotkeys();
			base.enabled = true;
			this.firstActive = true;
		}
	}

	public void OnDeselect()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this.firstActive)
			{
				base.InitHelp(false);
				this.firstMapping.StopListenForHotKeys();
			}
			else
			{
				this.secondMapping.StopListenForHotKeys();
			}
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			if (base.enabled)
			{
				this.OnDeselect();
			}
			return;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (InputManager.ActiveDevice.GetControl(this.binding.Hotkey).Value > 0f && this.firstActive)
			{
				this.firstActive = false;
				this.firstMapping.StopListenForHotKeys();
				this.secondMapping.StartListenForHotkeys();
				base.InitHelp(false);
			}
			else if (InputManager.ActiveDevice.GetControl(this.binding.Hotkey).Value == 0f && !this.firstActive)
			{
				this.firstActive = true;
				this.firstMapping.StartListenForHotkeys();
				this.secondMapping.StopListenForHotKeys();
				base.InitHelp(true);
			}
		}
	}

	[SerializeField]
	private HotkeyPressRedirect firstMapping;

	[SerializeField]
	private HotkeyPressRedirect secondMapping;

	[SerializeField]
	private HotkeyBinding binding;

	private bool firstActive;
}
