using System;
using System.Collections.Generic;
using System.Linq;
using Kender.uGUI;
using UnityEngine;

public class ResolutionHandler : MonoBehaviour
{
	public Resolution CurrentResolution
	{
		get
		{
			return this._resolution;
		}
		set
		{
			this._resolution = value;
		}
	}

	private void Start()
	{
		Resolution[] resolutions = Screen.resolutions;
		int num = 0;
		int num2 = 0;
		IList<ComboBoxItem> list = new List<ComboBoxItem>();
		for (int i = 0; i < resolutions.Count<Resolution>(); i++)
		{
			if (resolutions[i].width >= 1280 && resolutions[i].height >= 720)
			{
				string text = string.Format("{0}x{1} {2}Hz", resolutions[i].width, resolutions[i].height, resolutions[i].refreshRate);
				ComboBoxItem comboBoxItem = new ComboBoxItem(text);
				list.Add(comboBoxItem);
				int i1 = i;
				ComboBoxItem comboBoxItem2 = comboBoxItem;
				comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
				{
					this._resolution = resolutions[i1];
				}));
				if (SettingsManager.CurrentResolution.width == resolutions[i].width && SettingsManager.CurrentResolution.height == resolutions[i].height && SettingsManager.CurrentResolution.refreshRate == resolutions[i].refreshRate)
				{
					num = num2;
				}
				num2++;
			}
		}
		this.comboBox.AddItems(list.ToArray<ComboBoxItem>());
		this.comboBox.SelectedIndex = num;
	}

	private void Update()
	{
	}

	public ComboBox comboBox;

	private Resolution _resolution;
}
