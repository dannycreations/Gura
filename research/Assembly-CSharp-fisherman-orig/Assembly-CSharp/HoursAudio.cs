using System;
using UnityEngine;

public class HoursAudio : HoursChild, IEnvironmentSound
{
	private void Awake()
	{
		this._source = base.GetComponent<AudioSource>();
		if (this._source != null)
		{
			this._volume = this._source.volume;
			this._source.playOnAwake = false;
			this._source.loop = false;
			this._source.Stop();
		}
	}

	private void Start()
	{
		if (GameFactory.AudioController != null)
		{
			GameFactory.AudioController.RegisterSound(this);
		}
		else
		{
			LogHelper.Error("{0} can't be register as environmnet sound", new object[0]);
		}
	}

	public override void StartEvent()
	{
		if (this._source != null)
		{
			this._source.volume = this._volume * GlobalConsts.BgVolume;
			this._source.Play();
		}
	}

	public void Mute(bool flag)
	{
		this._source.mute = flag;
	}

	public void SetVolume(float globalVolume)
	{
		this._source.volume = this._volume * globalVolume;
	}

	private AudioSource _source;

	private float _volume;
}
