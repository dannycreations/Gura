using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundRandomizer : MonoBehaviour
{
	public bool Enabled
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
		private set
		{
			base.gameObject.SetActive(value);
			if (!value)
			{
				this._timeToTheNextSound = -1f;
			}
		}
	}

	public void SetActive(bool flag)
	{
		if (!this._isInited)
		{
			this.Awake();
		}
		if (this._isLoopGroup)
		{
			base.gameObject.SetActive(flag);
			return;
		}
		if (this._sources.Count == 0)
		{
			LogHelper.Error("No one clip in group [{0}] found", new object[] { base.name });
			return;
		}
		this._isDisabling = !flag;
		if (flag)
		{
			this.Enabled = true;
			this._clipIndex = -1;
			this.PrepareTimer(this._minInitialDelay, this._maxInitialDelay);
		}
		else if ((int)this._clipIndex != -1)
		{
			this._disablingClipVolume.value = 1f;
			this._disablingClipVolume.changeSpeedDown = 1f / this._disableSoundDuration;
		}
		else
		{
			this.Enabled = false;
		}
	}

	public void Mute(bool flag)
	{
		base.gameObject.SetActive(!flag);
	}

	private void Awake()
	{
		this.Enabled = false;
		if (this._isLoopGroup)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				AudioSource component = child.GetComponent<AudioSource>();
				if (component != null)
				{
					component.loop = true;
					this._loopSources.Add(new SoundRandomizer.LoopSource
					{
						InitialVolume = component.volume,
						Source = component
					});
					component.volume *= GlobalConsts.BgVolume;
				}
			}
			return;
		}
		if (this._audioSourcePrefab == null)
		{
			LogHelper.Error("You need to setup AudioSourcePrefab for group [{0}] to activate", new object[] { base.name });
			return;
		}
		for (int j = 0; j < this._clips.Length; j++)
		{
			AudioSource audioSource = Object.Instantiate<AudioSource>(this._audioSourcePrefab, base.transform);
			audioSource.name = this._clips[j].name;
			audioSource.transform.localPosition = Vector3.zero;
			AudioSource component2 = audioSource.GetComponent<AudioSource>();
			component2.playOnAwake = false;
			component2.loop = false;
			component2.mute = false;
			component2.gameObject.SetActive(false);
			component2.clip = this._clips[j];
			this._sources.Add(component2);
		}
		this._isInited = true;
	}

	private void Update()
	{
		if (this._isLoopGroup)
		{
			return;
		}
		if (this._isDisabling)
		{
			this._disablingClipVolume.update(Time.deltaTime);
			if (!this._sources[(int)this._clipIndex].isPlaying || Mathf.Approximately(this._disablingClipVolume.value, 0f))
			{
				this._sources[(int)this._clipIndex].gameObject.SetActive(false);
				this.Enabled = false;
			}
			else
			{
				this._sources[(int)this._clipIndex].volume = this._curVolume * this._disablingClipVolume.value;
			}
		}
		else
		{
			if ((int)this._clipIndex != -1 && !this._sources[(int)this._clipIndex].isPlaying)
			{
				this._sources[(int)this._clipIndex].gameObject.SetActive(false);
				this._clipIndex = -1;
			}
			this._timeToTheNextSound -= Time.deltaTime;
			if (this._timeToTheNextSound < 0f)
			{
				this._clipIndex = (sbyte)Random.Range(0, this._sources.Count);
				AudioSource audioSource = this._sources[(int)this._clipIndex];
				float length = audioSource.clip.length;
				this._curVolume = Random.Range(this._minVolume, this._maxVolume);
				audioSource.volume = this._curVolume * GlobalConsts.BgVolume;
				audioSource.gameObject.SetActive(true);
				audioSource.Play();
				this.PrepareTimer(length + this._minDelay, length + this._maxDelay);
			}
		}
	}

	private void PrepareTimer(float minDelay, float maxDelay)
	{
		this._timeToTheNextSound = Random.Range(minDelay, maxDelay);
	}

	public void UpdateVolume()
	{
		if (this._isLoopGroup)
		{
			for (int i = 0; i < this._loopSources.Count; i++)
			{
				this._loopSources[i].Source.volume = this._loopSources[i].InitialVolume * GlobalConsts.BgVolume;
			}
		}
		else if ((int)this._clipIndex != -1)
		{
			this._sources[(int)this._clipIndex].volume = this._curVolume * GlobalConsts.BgVolume;
		}
	}

	private List<SoundRandomizer.LoopSource> _loopSources = new List<SoundRandomizer.LoopSource>();

	[SerializeField]
	private bool _isLoopGroup;

	[SerializeField]
	private float _minInitialDelay = 10f;

	[SerializeField]
	private float _maxInitialDelay = 20f;

	[SerializeField]
	private float _minDelay = 5f;

	[SerializeField]
	private float _maxDelay = 30f;

	[SerializeField]
	private float _minVolume = 0.5f;

	[SerializeField]
	private float _maxVolume = 1f;

	[SerializeField]
	private float _disableSoundDuration = 2f;

	[SerializeField]
	private AudioSource _audioSourcePrefab;

	[SerializeField]
	private AudioClip[] _clips;

	[Tooltip("This field is for debug purposes only. It show how many time is left to the next sound playing")]
	[SerializeField]
	private float _timeToTheNextSound = -1f;

	private List<AudioSource> _sources = new List<AudioSource>();

	private sbyte _clipIndex = -1;

	private ValueChanger _disablingClipVolume = new ValueChanger(0f, 0f, 1f, new float?(1f));

	private bool _isDisabling;

	private float _curVolume;

	private bool _isMuted;

	private bool _isInited;

	private class LoopSource
	{
		public AudioSource Source { get; set; }

		public float InitialVolume { get; set; }
	}
}
