using System;
using UnityEngine;

public class PlayLoggedClick : MonoBehaviour
{
	public void OnClick()
	{
		this.WriteSettingsToLogFile();
		SceneController.CallAction(ScenesList.Logged, SceneStatuses.ToGame, this, null);
	}

	private void WriteSettingsToLogFile()
	{
		Debug.Log(string.Format("SoundVolume: {0}", SettingsManager.SoundVolume));
		Debug.Log(string.Format("MusicVolume: {0}", SettingsManager.MusicVolume));
		Debug.Log(string.Format("EnvironmentVolume: {0}", SettingsManager.EnvironmentVolume));
		Debug.Log(string.Format("MouseSensitivity: {0}", SettingsManager.MouseSensitivity));
		Debug.Log(string.Format("Antialiasing: {0}", SettingsManager.Antialiasing));
		Debug.Log(string.Format("SSAO: {0}", SettingsManager.SSAO));
		Debug.Log(string.Format("VSync: {0}", SettingsManager.VSync));
		Debug.Log(string.Format("InvertMouse: {0}", SettingsManager.InvertMouse));
		Debug.Log(string.Format("CurrentResolution: {0}x{1}", SettingsManager.CurrentResolution.width, SettingsManager.CurrentResolution.height));
		Debug.Log(string.Format("IsFullScreen: {0}", SettingsManager.IsFullScreen));
		Debug.Log(string.Format("DynWater: {0}", SettingsManager.DynWater));
		Debug.Log(string.Format("RenderQuality: {0}", SettingsManager.RenderQuality));
		Debug.Log(string.Format("BobberScale: {0}", SettingsManager.BobberScale));
		Debug.Log(string.Format("FightIndicator: {0}", SettingsManager.FightIndicator));
	}
}
