using System;
using UnityEngine;
using UnityEngine.Audio;

public class ShiftingSound_mobile : MonoBehaviour
{
	private void Start()
	{
		this.res = base.gameObject.transform.parent.GetComponent<RealisticEngineSound_mobile>();
		if (this.audioMixer != null)
		{
			this._audioMixer = this.audioMixer;
		}
		else if (this.res.audioMixer != null)
		{
			this._audioMixer = this.res.audioMixer;
			this.audioMixer = this._audioMixer;
		}
	}

	private void Update()
	{
		if (this.res.isCameraNear)
		{
			if (this.res.isShifting)
			{
				if (this.playOnce == 0)
				{
					this.CreateShiftSound();
					this.playOnce = 1;
				}
			}
			else
			{
				this.playOnce = 0;
				if (this.shiftingSound != null && !this.shiftingSound.isPlaying)
				{
					Object.Destroy(this.shiftingSound);
				}
			}
		}
		else
		{
			this.playOnce = 0;
			if (this.shiftingSound != null && !this.shiftingSound.isPlaying)
			{
				Object.Destroy(this.shiftingSound);
			}
		}
	}

	private void OnEnable()
	{
		this.Start();
	}

	private void OnDisable()
	{
		if (this.shiftingSound != null)
		{
			Object.Destroy(this.shiftingSound);
		}
	}

	private void CreateShiftSound()
	{
		this.shiftingSound = base.gameObject.AddComponent<AudioSource>();
		this.shiftingSound.rolloffMode = this.res.audioRolloffMode;
		this.shiftingSound.minDistance = this.res.minDistance;
		this.shiftingSound.maxDistance = this.res.maxDistance;
		this.shiftingSound.spatialBlend = this.res.spatialBlend;
		this.shiftingSound.dopplerLevel = this.res.dopplerLevel;
		this.shiftingSound.volume = this.masterVolume;
		if (this._audioMixer != null)
		{
			this.shiftingSound.outputAudioMixerGroup = this._audioMixer;
		}
		this.shiftingSound.pitch = Random.Range(0.8f, 1.2f);
		this.shiftingSound.loop = false;
		this.shiftingSound.clip = this.shiftingSoundClip;
		this.shiftingSound.Play();
	}

	private RealisticEngineSound_mobile res;

	[Range(0.1f, 1f)]
	public float masterVolume = 1f;

	public AudioMixerGroup audioMixer;

	private AudioMixerGroup _audioMixer;

	public AudioClip shiftingSoundClip;

	private AudioSource shiftingSound;

	private int playOnce;
}
