using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnvironmentSound : MonoBehaviour
{
	private void Awake()
	{
		this._audioSources = base.GetComponents<AudioSource>();
		this._defaultVolumes = new float[this._audioSources.Length];
		for (int i = 0; i < this._audioSources.Length; i++)
		{
			this._defaultVolumes[i] = this._audioSources[i].volume;
		}
		WeatherController.RegisterSound(this);
	}

	public void OnEnvironmentVolumeChanged(float windMultiplier)
	{
		for (int i = 0; i < this._audioSources.Length; i++)
		{
			this._audioSources[i].volume = this._defaultVolumes[i] * GlobalConsts.BgVolume;
			if (this._isVolumeControlledByWind)
			{
				this._audioSources[i].volume *= windMultiplier;
			}
		}
	}

	public void OnMute(bool flag)
	{
		for (int i = 0; i < this._audioSources.Length; i++)
		{
			this._audioSources[i].mute = flag;
		}
	}

	private void OnDestroy()
	{
		WeatherController.UnregisterSound(this);
	}

	[SerializeField]
	private bool _isVolumeControlledByWind = true;

	private AudioSource[] _audioSources;

	private float[] _defaultVolumes;
}
