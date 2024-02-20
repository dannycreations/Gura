using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using CodeStage.AdvancedFPSCounter;
using UnityEngine;

public class AudioController : MonoBehaviour, IAudioController
{
	private void Awake()
	{
		GameFactory.AudioController = this;
	}

	public void Start()
	{
		this._music = Object.FindObjectOfType<MusicController>();
		this.InGameOffVolume();
	}

	public void InGameOffVolume()
	{
		if (AFPSCounter.Instance != null)
		{
			AFPSCounter.Instance.fpsCounter.IsPaused = true;
		}
		GlobalConsts.InGameVolume = false;
		if (this._music != null)
		{
			this._music.SetActive(true);
		}
		if (WeatherController.Instance != null)
		{
			WeatherController.Instance.Mute(true);
		}
		GameObject gameObject = GameObject.Find("Rain audio");
		if (gameObject != null)
		{
			gameObject.GetComponent<AudioSource>().mute = true;
		}
		for (int i = 0; i < this._sounds.Count; i++)
		{
			this._sounds[i].Mute(true);
		}
	}

	public void InGameOnVolume()
	{
		AFPSCounter.Instance.fpsCounter.IsPaused = false;
		GlobalConsts.InGameVolume = true;
		this._music.SetActive(false);
		if (WeatherController.Instance != null)
		{
			WeatherController.Instance.Mute(false);
		}
		GameObject gameObject = GameObject.Find("Rain audio");
		if (gameObject != null)
		{
			gameObject.GetComponent<AudioSource>().mute = false;
		}
		for (int i = 0; i < this._sounds.Count; i++)
		{
			this._sounds[i].Mute(false);
		}
	}

	public void OnEnvironmentVolumeChanged(float volume)
	{
		for (int i = 0; i < this._sounds.Count; i++)
		{
			this._sounds[i].SetVolume(volume);
		}
	}

	public void RegisterSound(IEnvironmentSound sound)
	{
		this._sounds.Add(sound);
	}

	public void UnRegisterSound(IEnvironmentSound sound)
	{
		this._sounds.Remove(sound);
	}

	private void OnDisable()
	{
		GameFactory.AudioController = null;
	}

	private void OnDestroy()
	{
		this._sounds.Clear();
	}

	private MusicController _music;

	private float _currentMusicMultiplier = 1f;

	private static PondHelpers _pondHelpers = new PondHelpers();

	private List<IEnvironmentSound> _sounds = new List<IEnvironmentSound>();
}
