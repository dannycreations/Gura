using System;
using UnityEngine;
using UnityEngine.UI;

public class SaveSettings : MonoBehaviour
{
	public virtual void Save()
	{
		if (!Application.isEditor)
		{
			SettingsManager.CurrentResolution = this.ResolutionPanel.GetComponent<ResolutionHandler>().CurrentResolution;
		}
		SettingsManager.RenderQuality = this.QualityPanel.GetComponent<RenderQualityHandler>().RenderQuality;
		SettingsManager.Antialiasing = this.AntialiasingPanel.GetComponent<AntialiasingHandler>().AntialiasingValue;
		SettingsManager.DynWater = this.DynWaterPanel.GetComponent<DynWaterHandler>().DynamicWaterValue;
		SettingsManager.SoundVolume = this.SoundVolumePanel.value;
		SettingsManager.MusicVolume = this.MusicVolumePanel.value;
		SettingsManager.EnvironmentVolume = this.EnvironmentVolumePanel.value;
		SettingsManager.MouseSensitivity = this.MouseSensitivitySlider.value;
		SettingsManager.SSAO = this.SSAO.isOn;
		SettingsManager.VSync = this.VSync.isOn;
		SettingsManager.InvertMouse = this.InvertMouse.isOn;
		SettingsManager.InvertMouse = this.InvertMouse.isOn;
		SettingsManager.HideWhatsNew = this.HideWhatsNew.isOn;
		SettingsManager.IsFullScreen = this.FullScreen.isOn;
		if (this.BobberScalePanel != null)
		{
			SettingsManager.BobberScale = this.BobberScalePanel.value;
		}
	}

	protected virtual void Update()
	{
		if (!this._isInited && this.AntialiasingPanel.GetComponent<AntialiasingHandler>()._isInited && this.DynWaterPanel.GetComponent<DynWaterHandler>()._isInited)
		{
			this._isInited = true;
			Debug.Log(SettingsManager.RenderQuality);
			this.QualityPanel.GetComponent<RenderQualityHandler>().RenderQuality = SettingsManager.RenderQuality;
			this.AntialiasingPanel.GetComponent<AntialiasingHandler>().AntialiasingValue = SettingsManager.Antialiasing;
			this.DynWaterPanel.GetComponent<DynWaterHandler>().DynamicWaterValue = SettingsManager.DynWater;
			this.SSAO.isOn = SettingsManager.SSAO;
			this.InvertMouse.isOn = SettingsManager.InvertMouse;
			this.HideWhatsNew.isOn = SettingsManager.HideWhatsNew;
			this.VSync.isOn = SettingsManager.VSync;
			this.FullScreen.isOn = SettingsManager.IsFullScreen;
			this.SoundVolumePanel.value = SettingsManager.SoundVolume;
			this.MusicVolumePanel.value = SettingsManager.MusicVolume;
			this.EnvironmentVolumePanel.value = SettingsManager.EnvironmentVolume;
			this.MouseSensitivitySlider.value = SettingsManager.MouseSensitivity;
			this.ResolutionPanel.GetComponent<ResolutionHandler>().CurrentResolution = SettingsManager.CurrentResolution;
			if (this.BobberScalePanel != null)
			{
				this.BobberScalePanel.value = SettingsManager.BobberScale;
			}
		}
	}

	private void OnDisable()
	{
		this._isInited = false;
	}

	public GameObject ResolutionPanel;

	public GameObject QualityPanel;

	public GameObject AntialiasingPanel;

	public GameObject DynWaterPanel;

	public Toggle SSAO;

	public Toggle VSync;

	public Toggle InvertMouse;

	public Toggle FullScreen;

	public Slider SoundVolumePanel;

	public Slider MusicVolumePanel;

	public Slider EnvironmentVolumePanel;

	public Slider BobberScalePanel;

	public Slider MouseSensitivitySlider;

	public Toggle HideWhatsNew;

	protected bool _isInited;
}
