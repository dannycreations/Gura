using System;
using I2.Loc;
using Kender.uGUI;
using UnityEngine;

public class MouseWheelHandler : MonoBehaviour
{
	public MouseWheelValue MouseWheelValue
	{
		get
		{
			return this._mouseWheelValue;
		}
		set
		{
			if (value != MouseWheelValue.Reel)
			{
				if (value == MouseWheelValue.Drag)
				{
					this.comboBox.SelectedIndex = 1;
				}
			}
			else
			{
				this.comboBox.SelectedIndex = 0;
			}
			this._mouseWheelValue = value;
		}
	}

	public void Start()
	{
		ComboBoxItem comboBoxItem = new ComboBoxItem(ScriptLocalization.Get("ReelSpeedCaption"));
		ComboBoxItem comboBoxItem2 = comboBoxItem;
		comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
		{
			this._mouseWheelValue = MouseWheelValue.Reel;
		}));
		ComboBoxItem comboBoxItem3 = new ComboBoxItem(ScriptLocalization.Get("DragSpeedCaption"));
		ComboBoxItem comboBoxItem4 = comboBoxItem3;
		comboBoxItem4.OnSelect = (Action)Delegate.Combine(comboBoxItem4.OnSelect, new Action(delegate
		{
			this._mouseWheelValue = MouseWheelValue.Drag;
		}));
		this.comboBox.AddItems(new object[] { comboBoxItem, comboBoxItem3 });
		this.comboBox.SelectedIndex = 0;
		this.MouseWheelValue = SettingsManager.MouseWheel;
	}

	private void OnEnable()
	{
		this.MouseWheelValue = SettingsManager.MouseWheel;
	}

	public ComboBox comboBox;

	private MouseWheelValue _mouseWheelValue;
}
