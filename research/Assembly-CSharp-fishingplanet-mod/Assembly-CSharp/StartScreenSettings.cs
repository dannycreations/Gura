using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEngine;

public class StartScreenSettings : MonoBehaviour
{
	private void OnEnable()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.WindowOpen, SettingsManager.InterfaceVolume);
		this.InitResolution();
		this.InitViseoSettings();
		if (this._allowedResolutions.Count > 0)
		{
			this.ResolutionPanel.Index = this._allowedResolutions.IndexOf(SettingsManager.CurrentResolution);
		}
		base.StartCoroutine(this.SelectBtnOk());
	}

	private IEnumerator SelectBtnOk()
	{
		yield return new WaitForEndOfFrame();
		BorderedButton btnOk = base.GetComponentsInChildren<BorderedButton>().FirstOrDefault((BorderedButton p) => p.name == "btnOk");
		if (btnOk != null)
		{
			UINavigation.SetSelectedGameObject(btnOk.gameObject);
			base.GetComponent<UINavigation>().SetConcreteActiveForce(btnOk.gameObject);
		}
		yield break;
	}

	private void InitResolution()
	{
		Resolution[] resolutions = Screen.resolutions;
		int num = 0;
		int num2 = 0;
		List<string> list = new List<string>();
		this._allowedResolutions.Clear();
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width >= 1280 && resolutions[i].height >= 720)
			{
				string text = string.Format("{0}x{1} {2}Hz", resolutions[i].width, resolutions[i].height, resolutions[i].refreshRate);
				list.Add(text);
				this._allowedResolutions.Add(resolutions[i]);
				if (SettingsManager.CurrentResolution.width == resolutions[i].width && SettingsManager.CurrentResolution.height == resolutions[i].height && SettingsManager.CurrentResolution.refreshRate == resolutions[i].refreshRate)
				{
					num = num2;
				}
				num2++;
			}
		}
		if (list.Count > 0)
		{
			this.ResolutionPanel.Items = list;
			this.ResolutionPanel.Index = num;
		}
	}

	private void InitViseoSettings()
	{
		this._allowedRenderQualities.Clear();
		bool flag = SettingsManager.IsLowLevelSystem(SystemInfo.graphicsDeviceName);
		foreach (KeyValuePair<RenderQualities, string> keyValuePair in SaveSettingsOnStart.RenderQualitiesLocalizationIds)
		{
			if (!flag || keyValuePair.Key < RenderQualities.Good)
			{
				this.QualityPanel.Items.Add(ScriptLocalization.Get(keyValuePair.Value));
				this._allowedRenderQualities.Add(keyValuePair.Key);
			}
		}
		int num = this._allowedRenderQualities.IndexOf(SettingsManager.RenderQuality);
		this.QualityPanel.Index = ((num < 0) ? 0 : num);
	}

	public void Close()
	{
		SettingsManager.RenderQuality = this._allowedRenderQualities[this.QualityPanel.Index];
		if (!Application.isEditor && this._allowedResolutions != null && this._allowedResolutions.Count > 0)
		{
			SettingsManager.CurrentResolution = this._allowedResolutions[this.ResolutionPanel.Index];
		}
		SceneController.CallAction(ScenesList.Registration, SceneStatuses.StartSettingChanged, this, null);
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.WindowClose, SettingsManager.InterfaceVolume);
	}

	public VerticalPickList ResolutionPanel;

	public VerticalPickList QualityPanel;

	private List<Resolution> _allowedResolutions = new List<Resolution>();

	private List<RenderQualities> _allowedRenderQualities = new List<RenderQualities>();
}
