using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ShiftingSoundRandom : MonoBehaviour
{
	private void Start()
	{
		this.res = base.gameObject.transform.parent.GetComponent<RealisticEngineSound>();
		this._playtime = new WaitForSeconds(0.05f);
		if (this.audioMixer != null)
		{
			this._audioMixer = this.audioMixer;
		}
		if (this.audioMixer == null && this.res.audioMixer != null)
		{
			this._audioMixer = this.res.audioMixer;
			this.audioMixer = this._audioMixer;
		}
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
			}
		}
		else if (this.shiftingSound != null)
		{
			Object.Destroy(this.shiftingSound);
		}
	}

	private void OnDisable()
	{
		if (this.shiftingSound != null)
		{
			Object.Destroy(this.shiftingSound);
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.WaitForStart());
	}

	private IEnumerator WaitForStart()
	{
		yield return this._playtime;
		if (this.shiftingSound == null)
		{
			this.Start();
		}
		yield break;
	}

	private void CreateShiftSound()
	{
		if (this.shiftingSound != null)
		{
			this.shiftingSound.clip = this.shiftingSoundClips[Random.Range(0, this.shiftingSoundClips.Length)];
			this.shiftingSound.pitch = Random.Range(0.8f, 1.2f);
			this.shiftingSound.loop = false;
			this.shiftingSound.Play();
		}
		else
		{
			base.StartCoroutine(this.WaitForStart());
		}
	}

	private RealisticEngineSound res;

	[Range(0.1f, 1f)]
	public float masterVolume = 1f;

	public AudioMixerGroup audioMixer;

	private AudioMixerGroup _audioMixer;

	public AudioClip[] shiftingSoundClips;

	private AudioSource shiftingSound;

	private int playOnce;

	private WaitForSeconds _playtime;
}
