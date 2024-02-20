using System;
using I2.Loc;
using Kender.uGUI;
using UnityEngine;

public class AntialiasingHandler : MonoBehaviour
{
	public AntialiasingValue AntialiasingValue
	{
		get
		{
			return this._antialiasingValue;
		}
		set
		{
			if (value != AntialiasingValue.Off)
			{
				if (value != AntialiasingValue.Low)
				{
					if (value == AntialiasingValue.High)
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
			this._antialiasingValue = value;
		}
	}

	public void Start()
	{
		ComboBoxItem comboBoxItem = new ComboBoxItem(ScriptLocalization.Get("OffAntialiasingCaption"));
		ComboBoxItem comboBoxItem2 = comboBoxItem;
		comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
		{
			this._antialiasingValue = AntialiasingValue.Off;
		}));
		ComboBoxItem comboBoxItem3 = new ComboBoxItem(ScriptLocalization.Get("LowAntialiasingCaption"));
		ComboBoxItem comboBoxItem4 = comboBoxItem3;
		comboBoxItem4.OnSelect = (Action)Delegate.Combine(comboBoxItem4.OnSelect, new Action(delegate
		{
			this._antialiasingValue = AntialiasingValue.Low;
		}));
		ComboBoxItem comboBoxItem5 = new ComboBoxItem(ScriptLocalization.Get("HighAntialiasingCaption"));
		ComboBoxItem comboBoxItem6 = comboBoxItem5;
		comboBoxItem6.OnSelect = (Action)Delegate.Combine(comboBoxItem6.OnSelect, new Action(delegate
		{
			this._antialiasingValue = AntialiasingValue.High;
		}));
		this.comboBox.AddItems(new object[] { comboBoxItem, comboBoxItem3, comboBoxItem5 });
		this.comboBox.SelectedIndex = 0;
		this._isInited = true;
	}

	public ComboBox comboBox;

	private AntialiasingValue _antialiasingValue;

	public bool _isInited;
}
