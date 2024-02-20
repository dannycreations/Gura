using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class RealisticEngineSound_mobile : MonoBehaviour
{
	private void Start()
	{
		this.spatialBlend = 1f;
		this._wait = new WaitForSeconds(0.15f);
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
		this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
		if (this.mainCamera != null && Vector3.Distance(this.mainCamera.transform.position, base.gameObject.transform.position) <= this.maxDistance)
		{
			this.isCameraNear = true;
			if (this.idleVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.01f && this.engineIdle == null)
			{
				this.CreateIdle();
			}
			if (this.lowVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.01f && this.lowOn == null)
			{
				this.CreateLowOn();
			}
			if (this.medVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.01f && this.medOn == null)
			{
				this.CreateMedOn();
			}
			if (this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume > 0.01f && this.highOn == null)
			{
				this.CreateHighOn();
			}
			if (this.useRPMLimit && this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel && this.maxRPM == null)
			{
				this.CreateMaxRPM();
			}
			if (this.enableReverseGear)
			{
				if (this.isReversing)
				{
					if (this.reversingVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel && this.reversing == null)
					{
						this.CreateReverse();
					}
				}
				else if (this.reversing != null)
				{
					Object.Destroy(this.reversing);
				}
			}
		}
	}

	private void Update()
	{
		if (this.isCameraNear)
		{
			this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
			if (this.idleClip != null)
			{
				if (this.idleVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
				{
					if (this.engineIdle == null)
					{
						this.CreateIdle();
					}
					else
					{
						this.engineIdle.volume = this.idleVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
						this.engineIdle.pitch = this.idlePitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
				}
				else if (this.engineIdle != null)
				{
					Object.Destroy(this.engineIdle);
				}
			}
			if (this.lowOnClip != null)
			{
				if (this.lowVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
				{
					if (this.lowOn == null)
					{
						this.CreateLowOn();
					}
					else
					{
						this.lowOn.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
						this.lowOn.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
				}
				else if (this.lowOn != null)
				{
					Object.Destroy(this.lowOn);
				}
			}
			if (this.medOnClip != null)
			{
				if (this.medVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
				{
					if (this.medOn == null)
					{
						this.CreateMedOn();
					}
					else
					{
						this.medOn.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
						this.medOn.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
				}
				else if (this.medOn != null)
				{
					Object.Destroy(this.medOn);
				}
			}
			if (this.highOnClip != null)
			{
				if (this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
				{
					if (this.highOn == null)
					{
						this.CreateHighOn();
					}
					else if (!this.isReversing)
					{
						if (this.maxRPM != null)
						{
							if (this.maxRPM.volume < 0.95f)
							{
								this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
							}
							else
							{
								this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume / 3.3f;
							}
						}
						else
						{
							this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
						}
						this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
				}
				else if (this.highOn != null)
				{
					Object.Destroy(this.highOn);
				}
			}
			if (this.maxRPMClip != null)
			{
				if (this.useRPMLimit)
				{
					if (this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
					{
						if (this.maxRPM == null)
						{
							this.CreateMaxRPM();
						}
						else
						{
							this.maxRPM.volume = this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
							this.maxRPM.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						}
					}
					else if (this.maxRPM != null)
					{
						Object.Destroy(this.maxRPM);
					}
				}
			}
			else
			{
				this.useRPMLimit = false;
			}
			if (this.enableReverseGear)
			{
				if (this.reversingClip != null)
				{
					if (this.isReversing)
					{
						if (this.reversingVolCurve.Evaluate(this.clipsValue) * this.masterVolume > this.optimisationLevel)
						{
							if (this.reversing == null)
							{
								this.CreateReverse();
							}
							else
							{
								if (this.highOn != null)
								{
									if (this.maxRPM != null)
									{
										if (this.maxRPM.volume < 0.95f)
										{
											this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
										}
										else
										{
											this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume / 3.3f;
										}
									}
									else
									{
										this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
									}
									this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
								}
								this.reversing.volume = this.reversingVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
								this.reversing.pitch = this.reversingPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
							}
						}
						else if (this.reversing != null)
						{
							Object.Destroy(this.reversing);
						}
					}
					else if (this.reversing != null)
					{
						Object.Destroy(this.reversing);
					}
				}
				else
				{
					this.isReversing = false;
					this.enableReverseGear = false;
				}
			}
			else if (this.isReversing)
			{
				this.isReversing = false;
			}
		}
		else if (!this.alreadyDestroyed)
		{
			this.DestroyAll();
			this.alreadyDestroyed = true;
		}
	}

	private void FixedUpdate()
	{
		if (this.mainCamera != null)
		{
			if (Vector3.Distance(this.mainCamera.transform.position, base.gameObject.transform.position) > this.maxDistance)
			{
				this.isCameraNear = false;
			}
			else
			{
				this.isCameraNear = true;
				if (this.alreadyDestroyed)
				{
					this.alreadyDestroyed = false;
				}
			}
			if (!this.enableReverseGear && this.reversing != null)
			{
				Object.Destroy(this.reversing);
			}
			if (!this.useRPMLimit && this.maxRPM != null)
			{
				Object.Destroy(this.maxRPM);
			}
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.WaitForStart());
	}

	private void OnDisable()
	{
		this.DestroyAll();
	}

	private void DestroyAll()
	{
		if (this.engineIdle != null)
		{
			Object.Destroy(this.engineIdle);
		}
		if (this.lowOn != null)
		{
			Object.Destroy(this.lowOn);
		}
		if (this.medOn != null)
		{
			Object.Destroy(this.medOn);
		}
		if (this.highOn != null)
		{
			Object.Destroy(this.highOn);
		}
		if (this.useRPMLimit && this.maxRPM != null)
		{
			Object.Destroy(this.maxRPM);
		}
		if (this.enableReverseGear && this.reversing != null)
		{
			Object.Destroy(this.reversing);
		}
	}

	private IEnumerator WaitForStart()
	{
		yield return this._wait;
		if (this.engineIdle == null)
		{
			this.Start();
		}
		yield break;
	}

	private void CreateIdle()
	{
		if (this.idleClip != null)
		{
			this.engineIdle = base.gameObject.AddComponent<AudioSource>();
			this.engineIdle.spatialBlend = this.spatialBlend;
			this.engineIdle.rolloffMode = this.audioRolloffMode;
			this.engineIdle.dopplerLevel = this.dopplerLevel;
			this.engineIdle.volume = this.idleVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.engineIdle.pitch = this.idlePitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.engineIdle.minDistance = this.minDistance;
			this.engineIdle.maxDistance = this.maxDistance;
			this.engineIdle.loop = true;
			this.engineIdle.clip = this.idleClip;
			this.engineIdle.Play();
			if (this.audioMixer != null)
			{
				this.engineIdle.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateLowOn()
	{
		if (this.lowOnClip != null)
		{
			this.lowOn = base.gameObject.AddComponent<AudioSource>();
			this.lowOn.spatialBlend = this.spatialBlend;
			this.lowOn.rolloffMode = this.audioRolloffMode;
			this.lowOn.dopplerLevel = this.dopplerLevel;
			this.lowOn.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.lowOn.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.lowOn.minDistance = this.minDistance;
			this.lowOn.maxDistance = this.maxDistance;
			this.lowOn.loop = true;
			this.lowOn.clip = this.lowOnClip;
			this.lowOn.Play();
			if (this.audioMixer != null)
			{
				this.lowOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateMedOn()
	{
		if (this.medOnClip != null)
		{
			this.medOn = base.gameObject.AddComponent<AudioSource>();
			this.medOn.spatialBlend = this.spatialBlend;
			this.medOn.rolloffMode = this.audioRolloffMode;
			this.medOn.dopplerLevel = this.dopplerLevel;
			this.medOn.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.medOn.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.medOn.minDistance = this.minDistance;
			this.medOn.maxDistance = this.maxDistance;
			this.medOn.loop = true;
			this.medOn.clip = this.medOnClip;
			this.medOn.Play();
			if (this.audioMixer != null)
			{
				this.medOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateHighOn()
	{
		if (this.highOnClip != null)
		{
			this.highOn = base.gameObject.AddComponent<AudioSource>();
			this.highOn.spatialBlend = this.spatialBlend;
			this.highOn.rolloffMode = this.audioRolloffMode;
			this.highOn.dopplerLevel = this.dopplerLevel;
			this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.highOn.minDistance = this.minDistance;
			this.highOn.maxDistance = this.maxDistance;
			this.highOn.loop = true;
			this.highOn.clip = this.highOnClip;
			this.highOn.Play();
			if (this.audioMixer != null)
			{
				this.highOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateMaxRPM()
	{
		if (this.maxRPMClip != null)
		{
			this.maxRPM = base.gameObject.AddComponent<AudioSource>();
			this.maxRPM.spatialBlend = this.spatialBlend;
			this.maxRPM.rolloffMode = this.audioRolloffMode;
			this.maxRPM.dopplerLevel = this.dopplerLevel;
			this.maxRPM.volume = this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.maxRPM.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.maxRPM.minDistance = this.minDistance;
			this.maxRPM.maxDistance = this.maxDistance;
			this.maxRPM.loop = true;
			this.maxRPM.clip = this.maxRPMClip;
			this.maxRPM.Play();
			if (this.audioMixer != null)
			{
				this.maxRPM.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateReverse()
	{
		if (this.reversingClip != null)
		{
			this.reversing = base.gameObject.AddComponent<AudioSource>();
			this.reversing.spatialBlend = this.spatialBlend;
			this.reversing.rolloffMode = this.audioRolloffMode;
			this.reversing.dopplerLevel = this.dopplerLevel;
			this.reversing.volume = this.reversingVolCurve.Evaluate(this.clipsValue) * this.masterVolume;
			this.reversing.pitch = this.reversingPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.reversing.minDistance = this.minDistance;
			this.reversing.maxDistance = this.maxDistance;
			this.reversing.loop = true;
			this.reversing.clip = this.reversingClip;
			this.reversing.Play();
			if (this.audioMixer != null)
			{
				this.reversing.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	[Range(0.1f, 1f)]
	public float masterVolume = 1f;

	public AudioMixerGroup audioMixer;

	public float engineCurrentRPM;

	public float maxRPMLimit = 7000f;

	[Range(0f, 5f)]
	public float dopplerLevel = 1f;

	[Range(0f, 1f)]
	[HideInInspector]
	public float spatialBlend = 1f;

	[Range(0.1f, 2f)]
	public float pitchMultiplier = 1f;

	[Range(0f, 0.25f)]
	public float optimisationLevel = 0.01f;

	public AudioRolloffMode audioRolloffMode = 2;

	public float minDistance = 1f;

	public float maxDistance = 50f;

	public bool isReversing;

	public bool useRPMLimit = true;

	public bool enableReverseGear;

	[HideInInspector]
	public float carCurrentSpeed;

	[HideInInspector]
	public float carMaxSpeed;

	[HideInInspector]
	public bool isShifting;

	public bool gasPedalPressing;

	public AudioClip idleClip;

	public AnimationCurve idleVolCurve;

	public AnimationCurve idlePitchCurve;

	public AudioClip lowOnClip;

	public AnimationCurve lowVolCurve;

	public AnimationCurve lowPitchCurve;

	public AudioClip medOnClip;

	public AnimationCurve medVolCurve;

	public AnimationCurve medPitchCurve;

	public AudioClip highOnClip;

	public AnimationCurve highVolCurve;

	public AnimationCurve highPitchCurve;

	public AudioClip maxRPMClip;

	public AnimationCurve maxRPMVolCurve;

	public AudioClip reversingClip;

	public AnimationCurve reversingVolCurve;

	public AnimationCurve reversingPitchCurve;

	private AudioSource engineIdle;

	private AudioSource lowOn;

	private AudioSource medOn;

	private AudioSource highOn;

	private AudioSource maxRPM;

	private AudioSource reversing;

	private float clipsValue;

	public Camera mainCamera;

	[HideInInspector]
	public bool isCameraNear;

	private WaitForSeconds _wait;

	private bool alreadyDestroyed;
}
