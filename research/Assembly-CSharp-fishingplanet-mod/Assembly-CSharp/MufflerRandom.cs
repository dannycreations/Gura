using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MufflerRandom : MonoBehaviour
{
	private void Start()
	{
		this.res = base.gameObject.transform.parent.GetComponent<RealisticEngineSound>();
		if (this.audioMixer != null)
		{
			this._audioMixer = this.audioMixer;
		}
		else if (this.res.audioMixer != null)
		{
			this._audioMixer = this.res.audioMixer;
			this.audioMixer = this._audioMixer;
		}
		this.playTime_ = this.playTime;
		this.UpdateWaitTime();
	}

	private void Update()
	{
		this.clipsValue = this.res.engineCurrentRPM / this.res.maxRPMLimit;
		if (this.res.isCameraNear)
		{
			if (this.res.gasPedalPressing)
			{
				this.oneShotController = 1;
			}
			else if (this.oneShotController == 1)
			{
				if (this.mufflerOffVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.09f)
				{
					this.oneShotController = 2;
				}
				else
				{
					this.oneShotController = 0;
				}
			}
			if (this.mufflerOffVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.09f)
			{
				if (this.oneShotController == 2)
				{
					if (this.offLoop == null)
					{
						this.CreateOff();
					}
					else
					{
						this.offLoop.pitch = this.res.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						this.offLoop.volume = this.mufflerOffVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
					}
				}
			}
			else if (this.offLoop != null)
			{
				Object.Destroy(this.offLoop);
			}
			if (this.mufflerOnVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.09f)
			{
				if (this.oneShotController == 1)
				{
					if (this.onLoop == null)
					{
						this.CreateOn();
					}
					else
					{
						this.onLoop.pitch = this.res.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						this.onLoop.volume = this.mufflerOnVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
					}
				}
			}
			else if (this.onLoop != null)
			{
				Object.Destroy(this.onLoop);
			}
		}
		if (this.playTime_ != this.playTime)
		{
			this.UpdateWaitTime();
		}
	}

	private void OnEnable()
	{
		this.Start();
	}

	private void OnDisable()
	{
		if (this.onLoop != null)
		{
			Object.Destroy(this.onLoop);
		}
		if (this.offLoop != null)
		{
			Object.Destroy(this.offLoop);
		}
	}

	private void CreateOff()
	{
		if (this.offClip != null)
		{
			this.offLoop = base.gameObject.AddComponent<AudioSource>();
			this.offLoop.spatialBlend = this.res.spatialBlend;
			this.offLoop.rolloffMode = this.res.audioRolloffMode;
			this.offLoop.dopplerLevel = this.res.dopplerLevel;
			this.offLoop.minDistance = this.res.minDistance;
			this.offLoop.maxDistance = this.res.maxDistance;
			this.offLoop.volume = this.mufflerOffVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.offLoop.pitch = this.res.medPitchCurve.Evaluate(this.clipsValue) * 2f;
			this.offLoop.clip = this.offClip[Random.Range(0, this.offClip.Length)];
			this.offLoop.loop = true;
			if (this._audioMixer != null)
			{
				this.offLoop.outputAudioMixerGroup = this._audioMixer;
			}
			this.offLoop.Play();
			base.StartCoroutine(this.Wait2());
		}
	}

	private void CreateOn()
	{
		if (this.onClip != null)
		{
			this.onLoop = base.gameObject.AddComponent<AudioSource>();
			this.onLoop.spatialBlend = this.res.spatialBlend;
			this.onLoop.rolloffMode = this.res.audioRolloffMode;
			this.onLoop.dopplerLevel = this.res.dopplerLevel;
			this.onLoop.minDistance = this.res.minDistance;
			this.onLoop.maxDistance = this.res.maxDistance;
			this.onLoop.volume = this.mufflerOnVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.onLoop.pitch = this.res.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.onLoop.clip = this.onClip[Random.Range(0, this.onClip.Length)];
			this.onLoop.loop = true;
			if (this._audioMixer != null)
			{
				this.onLoop.outputAudioMixerGroup = this._audioMixer;
			}
			this.onLoop.Play();
			base.StartCoroutine(this.Wait1());
		}
	}

	private void UpdateWaitTime()
	{
		this._playtime = new WaitForSeconds(this.playTime);
		this.playTime_ = this.playTime;
	}

	private IEnumerator Wait1()
	{
		yield return this._playtime;
		this.oneShotController = 0;
		Object.Destroy(this.onLoop);
		yield break;
	}

	private IEnumerator Wait2()
	{
		yield return this._playtime;
		this.oneShotController = 0;
		Object.Destroy(this.offLoop);
		yield break;
	}

	private RealisticEngineSound res;

	[Range(0.1f, 1f)]
	public float masterVolume = 1f;

	public AudioMixerGroup audioMixer;

	private AudioMixerGroup _audioMixer;

	[Range(0.5f, 2f)]
	public float pitchMultiplier = 1f;

	[Range(0.5f, 4f)]
	public float playTime = 2f;

	private float playTime_;

	public AudioClip[] offClip;

	public AudioClip[] onClip;

	private AudioSource offLoop;

	private AudioSource onLoop;

	public AnimationCurve mufflerOffVolCurve;

	public AnimationCurve mufflerOnVolCurve;

	private float clipsValue;

	private int oneShotController;

	private WaitForSeconds _playtime;
}
