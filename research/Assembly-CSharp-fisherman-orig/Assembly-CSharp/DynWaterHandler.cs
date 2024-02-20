using System;
using I2.Loc;
using Kender.uGUI;
using UnityEngine;

public class DynWaterHandler : MonoBehaviour
{
	public DynWaterValue DynamicWaterValue
	{
		get
		{
			return this._dynWaterValue;
		}
		set
		{
			if (value != DynWaterValue.Off)
			{
				if (value != DynWaterValue.Low)
				{
					if (value == DynWaterValue.High)
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
			this._dynWaterValue = value;
		}
	}

	public void Start()
	{
		ComboBoxItem comboBoxItem = new ComboBoxItem(ScriptLocalization.Get("OffDynWaterCaption"));
		ComboBoxItem comboBoxItem2 = comboBoxItem;
		comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
		{
			this._dynWaterValue = DynWaterValue.Off;
		}));
		ComboBoxItem comboBoxItem3 = new ComboBoxItem(ScriptLocalization.Get("LowDynWaterCaption"));
		ComboBoxItem comboBoxItem4 = comboBoxItem3;
		comboBoxItem4.OnSelect = (Action)Delegate.Combine(comboBoxItem4.OnSelect, new Action(delegate
		{
			this._dynWaterValue = DynWaterValue.Low;
		}));
		ComboBoxItem comboBoxItem5 = new ComboBoxItem(ScriptLocalization.Get("HighDynWaterCaption"));
		ComboBoxItem comboBoxItem6 = comboBoxItem5;
		comboBoxItem6.OnSelect = (Action)Delegate.Combine(comboBoxItem6.OnSelect, new Action(delegate
		{
			this._dynWaterValue = DynWaterValue.High;
		}));
		this.comboBox.AddItems(new object[] { comboBoxItem, comboBoxItem3, comboBoxItem5 });
		this.comboBox.SelectedIndex = 0;
		this._isInited = true;
	}

	public ComboBox comboBox;

	private DynWaterValue _dynWaterValue;

	public bool _isInited;
}
