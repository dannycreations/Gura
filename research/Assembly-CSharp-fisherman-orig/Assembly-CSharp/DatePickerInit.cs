using System;
using I2.Loc;
using Kender.uGUI;
using UnityEngine;

public class DatePickerInit : MonoBehaviour
{
	public DateTime SelectedDate
	{
		get
		{
			return new DateTime((int)this._currentYear, (int)this._currentMonth, (int)this._currentDay);
		}
		set
		{
			this._setDate = value;
			this._currentYear = (ushort)value.Year;
			this._currentMonth = (ushort)value.Month;
			this._currentDay = (ushort)value.Day;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
			this.YearComboBox.SelectItem(value.Year - 1900);
			this.MonthComboBox.SelectItem(value.Month - 1);
			this.DaysComboBox.SelectItem(value.Day - 1);
		}
	}

	private void Start()
	{
		this.DaysComboBox.ClearItems();
		this.MonthComboBox.ClearItems();
		this.YearComboBox.ClearItems();
		ComboBoxItem[] array = new ComboBoxItem[DateTime.Now.Year + 1 - 1900];
		for (int i = 1900; i < DateTime.Now.Year + 1; i++)
		{
			array[i - 1900] = new ComboBoxItem(i.ToString());
			int i1 = i;
			ComboBoxItem comboBoxItem = array[i - 1900];
			comboBoxItem.OnSelect = (Action)Delegate.Combine(comboBoxItem.OnSelect, new Action(delegate
			{
				this._currentYear = (ushort)i1;
			}));
		}
		this.YearComboBox.ItemsToDisplay = 8;
		this.YearComboBox.AddItems(array);
		ComboBoxItem[] array2 = new ComboBoxItem[12];
		array2[0] = new ComboBoxItem(ScriptLocalization.Get("January"));
		ComboBoxItem comboBoxItem2 = array2[0];
		comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
		{
			this._currentMonth = 1;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[1] = new ComboBoxItem(ScriptLocalization.Get("February"));
		ComboBoxItem comboBoxItem3 = array2[1];
		comboBoxItem3.OnSelect = (Action)Delegate.Combine(comboBoxItem3.OnSelect, new Action(delegate
		{
			this._currentMonth = 2;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[2] = new ComboBoxItem(ScriptLocalization.Get("March"));
		ComboBoxItem comboBoxItem4 = array2[2];
		comboBoxItem4.OnSelect = (Action)Delegate.Combine(comboBoxItem4.OnSelect, new Action(delegate
		{
			this._currentMonth = 3;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[3] = new ComboBoxItem(ScriptLocalization.Get("April"));
		ComboBoxItem comboBoxItem5 = array2[3];
		comboBoxItem5.OnSelect = (Action)Delegate.Combine(comboBoxItem5.OnSelect, new Action(delegate
		{
			this._currentMonth = 4;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[4] = new ComboBoxItem(ScriptLocalization.Get("May"));
		ComboBoxItem comboBoxItem6 = array2[4];
		comboBoxItem6.OnSelect = (Action)Delegate.Combine(comboBoxItem6.OnSelect, new Action(delegate
		{
			this._currentMonth = 5;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[5] = new ComboBoxItem(ScriptLocalization.Get("June"));
		ComboBoxItem comboBoxItem7 = array2[5];
		comboBoxItem7.OnSelect = (Action)Delegate.Combine(comboBoxItem7.OnSelect, new Action(delegate
		{
			this._currentMonth = 6;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[6] = new ComboBoxItem(ScriptLocalization.Get("July"));
		ComboBoxItem comboBoxItem8 = array2[6];
		comboBoxItem8.OnSelect = (Action)Delegate.Combine(comboBoxItem8.OnSelect, new Action(delegate
		{
			this._currentMonth = 7;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[7] = new ComboBoxItem(ScriptLocalization.Get("August"));
		ComboBoxItem comboBoxItem9 = array2[7];
		comboBoxItem9.OnSelect = (Action)Delegate.Combine(comboBoxItem9.OnSelect, new Action(delegate
		{
			this._currentMonth = 8;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[8] = new ComboBoxItem(ScriptLocalization.Get("September"));
		ComboBoxItem comboBoxItem10 = array2[8];
		comboBoxItem10.OnSelect = (Action)Delegate.Combine(comboBoxItem10.OnSelect, new Action(delegate
		{
			this._currentMonth = 9;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[9] = new ComboBoxItem(ScriptLocalization.Get("October"));
		ComboBoxItem comboBoxItem11 = array2[9];
		comboBoxItem11.OnSelect = (Action)Delegate.Combine(comboBoxItem11.OnSelect, new Action(delegate
		{
			this._currentMonth = 10;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[10] = new ComboBoxItem(ScriptLocalization.Get("November"));
		ComboBoxItem comboBoxItem12 = array2[10];
		comboBoxItem12.OnSelect = (Action)Delegate.Combine(comboBoxItem12.OnSelect, new Action(delegate
		{
			this._currentMonth = 11;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		array2[11] = new ComboBoxItem(ScriptLocalization.Get("December"));
		ComboBoxItem comboBoxItem13 = array2[11];
		comboBoxItem13.OnSelect = (Action)Delegate.Combine(comboBoxItem13.OnSelect, new Action(delegate
		{
			this._currentMonth = 12;
			this.InitDays(DateTime.DaysInMonth((int)this._currentYear, (int)this._currentMonth));
		}));
		this.MonthComboBox.ItemsToDisplay = 8;
		this.MonthComboBox.AddItems(array2);
		this.InitDays(31);
		this.SelectedDate = this._setDate;
	}

	private void InitDays(int count)
	{
		try
		{
			ushort currentDay = this._currentDay;
			if (this.DaysComboBox.Items != null && this.DaysComboBox.Items.Length > 0)
			{
				this.DaysComboBox.ClearItems();
			}
			ComboBoxItem[] array = new ComboBoxItem[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new ComboBoxItem((i + 1).ToString());
				int i1 = i;
				ComboBoxItem comboBoxItem = array[i];
				comboBoxItem.OnSelect = (Action)Delegate.Combine(comboBoxItem.OnSelect, new Action(delegate
				{
					this._currentDay = (ushort)(i1 + 1);
				}));
			}
			this.DaysComboBox.ItemsToDisplay = 8;
			this.DaysComboBox.AddItems(array);
			this.DaysComboBox.SelectItem((int)(currentDay - 1));
		}
		catch (NullReferenceException)
		{
		}
	}

	public ComboBox DaysComboBox;

	public ComboBox MonthComboBox;

	public ComboBox YearComboBox;

	private ushort _currentMonth;

	private ushort _currentYear;

	private ushort _currentDay;

	private DateTime _setDate = DateTime.Now;
}
