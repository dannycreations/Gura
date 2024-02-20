using System;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumber : MonoBehaviour
{
	public string Version
	{
		get
		{
			if (this.version == null)
			{
				this.version = LaunchInit.CurrentVersion;
			}
			return this.version;
		}
	}

	private void Start()
	{
		if (!CustomPlayerPrefs.HasKey("Version") || this.Version != CustomPlayerPrefs.GetString("Version"))
		{
			CustomPlayerPrefs.SetString("Version", this.Version);
			Caching.ClearCache();
		}
		Object.DontDestroyOnLoad(base.gameObject);
		Debug.Log(string.Format("Currently running version is {0}", this.Version));
		this.VersionText.text = string.Format("v{0}", this.Version);
	}

	private void HideText()
	{
		this.VersionText.text = string.Empty;
	}

	public Text VersionText;

	private string version;

	private Rect position = new Rect(0f, 0f, 150f, 20f);

	private Rect copyrightPosition = new Rect(0f, 0f, 380f, 30f);
}
