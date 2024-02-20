using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
	private void Start()
	{
		AudioListener.volume = SettingsManager.SoundVolume;
		this._src = base.GetComponent<AudioSource>();
		this._audioListener = base.GetComponent<AudioListener>();
		this._src.loop = false;
		this._src.Stop();
		this._src.clip = null;
	}

	private void StartRandomClip()
	{
		int num;
		do
		{
			num = Random.Range(0, this._clips.Length);
		}
		while (this._src.clip == this._clips[num]);
		this._src.Stop();
		this._src.clip = this._clips[num];
		this._src.Play();
	}

	public void SetActive(bool flag)
	{
		if (flag == this._isActive)
		{
			return;
		}
		this._isActive = flag;
		this._isChanging = true;
		this._audioListener.enabled = flag;
		if (flag)
		{
			if (this._src.clip == null)
			{
				this.StartRandomClip();
			}
			this._src.UnPause();
		}
	}

	private void Update()
	{
		if (this._isChanging)
		{
			if (this._isActive)
			{
				this._src.volume += this._fadeSpeed * Time.deltaTime;
				if (this._src.volume > SettingsManager.ClampedMusicVolume)
				{
					this._isChanging = false;
					this._src.volume = SettingsManager.ClampedMusicVolume;
				}
			}
			else
			{
				this._src.volume -= this._fadeSpeed * Time.deltaTime;
				if (this._src.volume <= 0f)
				{
					this._isChanging = false;
					this._src.volume = 0f;
					this._src.Pause();
				}
			}
		}
		else if (this._isActive)
		{
			this._src.volume = SettingsManager.ClampedMusicVolume;
			if (this._src.time >= this._src.clip.length)
			{
				this.StartRandomClip();
			}
			else if (!this._src.isPlaying)
			{
				this.StartRandomClip();
			}
		}
	}

	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private float _fadeSpeed = 0.3f;

	private bool _isActive;

	private bool _isChanging;

	private AudioSource _src;

	private AudioListener _audioListener;
}
