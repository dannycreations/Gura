using System;
using Kender.uGUI;
using UnityEngine;

public class InputTypeHandler : MonoBehaviour
{
	public InputModuleManager.InputType InputTypeValue
	{
		get
		{
			return this._inputTypeValue;
		}
		set
		{
			if (value != InputModuleManager.InputType.Mouse)
			{
				if (value == InputModuleManager.InputType.GamePad)
				{
					this.comboBox.SelectedIndex = 1;
				}
			}
			else
			{
				this.comboBox.SelectedIndex = 0;
			}
			this._inputTypeValue = value;
		}
	}

	public void Start()
	{
		ComboBoxItem comboBoxItem = new ComboBoxItem("Mouse");
		ComboBoxItem comboBoxItem2 = comboBoxItem;
		comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
		{
			this._inputTypeValue = InputModuleManager.InputType.Mouse;
		}));
		ComboBoxItem comboBoxItem3 = new ComboBoxItem("GamePad");
		ComboBoxItem comboBoxItem4 = comboBoxItem3;
		comboBoxItem4.OnSelect = (Action)Delegate.Combine(comboBoxItem4.OnSelect, new Action(delegate
		{
			this._inputTypeValue = InputModuleManager.InputType.GamePad;
		}));
		this.comboBox.AddItems(new object[] { comboBoxItem, comboBoxItem3 });
		this.comboBox.SelectedIndex = 0;
		this.InputTypeValue = SettingsManager.InputType;
	}

	private void OnEnable()
	{
		this.InputTypeValue = SettingsManager.InputType;
	}

	public ComboBox comboBox;

	private InputModuleManager.InputType _inputTypeValue;
}
