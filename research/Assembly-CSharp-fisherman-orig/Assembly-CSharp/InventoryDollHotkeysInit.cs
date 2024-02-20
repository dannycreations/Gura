using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDollHotkeysInit : MonoBehaviour
{
	private void Awake()
	{
		this.infoButtonsHotkeys[0].enabled = false;
	}

	private void OnEnable()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this.infoButtons[0].enabled)
			{
				this.infoButtonsHotkeys[0].enabled = true;
			}
			else
			{
				this.infoButtonsHotkeys[1].enabled = true;
			}
			for (int i = 0; i < this.rodToggles.Length; i++)
			{
				if (this.rodToggles[i].isOn)
				{
					this.rodHotkeys[i].enabled = true;
					return;
				}
			}
		}
	}

	private void OnDisable()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			for (int i = 0; i < this.rodToggles.Length; i++)
			{
				if (this.rodToggles[i].isOn)
				{
					this.rodHotkeys[i].enabled = false;
					break;
				}
			}
			this.infoButtonsHotkeys[0].enabled = false;
			this.infoButtonsHotkeys[1].enabled = false;
		}
	}

	[SerializeField]
	private Toggle[] rodToggles = new Toggle[0];

	[SerializeField]
	private HotkeyPressRedirect[] rodHotkeys = new HotkeyPressRedirect[0];

	[SerializeField]
	private HotkeyPressRedirect[] infoButtonsHotkeys = new HotkeyPressRedirect[0];

	[SerializeField]
	private Button[] infoButtons = new Button[0];
}
