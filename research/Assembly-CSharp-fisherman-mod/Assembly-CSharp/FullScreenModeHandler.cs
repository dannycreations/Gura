using System;
using Kender.uGUI;
using UnityEngine;

public class FullScreenModeHandler : MonoBehaviour
{
	public WinFullScreenModeValue WinFullScreenMode
	{
		get
		{
			return this._winFullscreenMode;
		}
		set
		{
			if (value != WinFullScreenModeValue.FullscreenWindow)
			{
				if (value == WinFullScreenModeValue.EsclusiveMode)
				{
					this.comboBox.SelectedIndex = 1;
				}
			}
			else
			{
				this.comboBox.SelectedIndex = 0;
			}
			this._winFullscreenMode = value;
		}
	}

	public OSXFullScreenModeValue OsXnFullScreenMode
	{
		get
		{
			return this._osxFullscreenMode;
		}
		set
		{
			if (value != OSXFullScreenModeValue.FullscreenWindowWithMenuBar)
			{
				if (value != OSXFullScreenModeValue.FullscreenWindow)
				{
					if (value == OSXFullScreenModeValue.CaptureDisplay)
					{
						this.comboBox.SelectedIndex = 2;
					}
				}
				else
				{
					this.comboBox.SelectedIndex = 1;
				}
			}
			else
			{
				this.comboBox.SelectedIndex = 0;
			}
			this._osxFullscreenMode = value;
		}
	}

	public bool _isInited;

	public ComboBox comboBox;

	private WinFullScreenModeValue _winFullscreenMode;

	private OSXFullScreenModeValue _osxFullscreenMode;
}
