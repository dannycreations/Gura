using System;
using System.Collections.Generic;
using I2.Loc;
using Kender.uGUI;
using UnityEngine;
using UnityEngine.UI;

public class RenderQualityHandler : MonoBehaviour
{
	public RenderQualities RenderQuality
	{
		get
		{
			return this._renderQuality;
		}
		set
		{
			this.comboBox.SelectedIndex = (int)value;
			this._renderQuality = value;
		}
	}

	private void Start()
	{
		List<ComboBoxItem> list = new List<ComboBoxItem>();
		using (Dictionary<RenderQualities, string>.Enumerator enumerator = SaveSettingsOnStart.RenderQualitiesLocalizationIds.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<RenderQualities, string> rq = enumerator.Current;
				RenderQualityHandler $this = this;
				ComboBoxItem comboBoxItem = new ComboBoxItem(ScriptLocalization.Get(rq.Value));
				ComboBoxItem comboBoxItem2 = comboBoxItem;
				comboBoxItem2.OnSelect = (Action)Delegate.Combine(comboBoxItem2.OnSelect, new Action(delegate
				{
					$this._renderQuality = rq.Key;
					if ($this._renderQuality == RenderQualities.Fastest)
					{
						$this.SSAO.isOn = false;
						$this.DynWaterPanel.GetComponent<DynWaterHandler>().DynamicWaterValue = DynWaterValue.Off;
					}
				}));
				list.Add(comboBoxItem);
			}
		}
		this.comboBox.AddItems(list.ToArray());
		this.comboBox.SelectedIndex = 0;
		this.RenderQuality = SettingsManager.RenderQuality;
	}

	private void OnEnable()
	{
		this.RenderQuality = SettingsManager.RenderQuality;
	}

	public ComboBox comboBox;

	private RenderQualities _renderQuality;

	public Toggle SSAO;

	public ComboBox DynWaterPanel;
}
