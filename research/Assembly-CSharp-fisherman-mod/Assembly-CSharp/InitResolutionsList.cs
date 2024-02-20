using System;
using UnityEngine;
using UnityEngine.UI;

public class InitResolutionsList : MonoBehaviour
{
	private void Update()
	{
		if (!this._setListResolution)
		{
			Resolution[] resolutions = Screen.resolutions;
			int targetFrameRate = Application.targetFrameRate;
			foreach (Resolution resolution in resolutions)
			{
				GameObject gameObject = GUITools.AddChild(this.currentPanel, this.togglePrefab);
				gameObject.GetComponent<Toggle>().group = this.currentPanel.GetComponent<ToggleGroup>();
				gameObject.GetComponent<SetResolutionToggle>().resolution = resolution;
				gameObject.GetComponent<Toggle>().isOn = false;
				if (SettingsManager.CurrentResolution.width == resolution.width && SettingsManager.CurrentResolution.height == resolution.height && SettingsManager.CurrentResolution.refreshRate == resolution.refreshRate)
				{
					Toggle component = gameObject.GetComponent<Toggle>();
					component.isOn = true;
				}
			}
			this._setListResolution = true;
		}
	}

	public GameObject togglePrefab;

	public GameObject currentPanel;

	private bool _setListResolution;
}
