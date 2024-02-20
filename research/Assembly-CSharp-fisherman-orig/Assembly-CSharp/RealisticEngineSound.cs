using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class RealisticEngineSound : MonoBehaviour
{
	private float MasterVolume
	{
		get
		{
			return this.masterVolume * 0.6f * GlobalConsts.BgVolume;
		}
	}

	private float EffectsVolume
	{
		get
		{
			return 0.6f * GlobalConsts.BgVolume;
		}
	}

	public void PlayGearShift()
	{
		if (this.gearShiftClip != null)
		{
			this.miscSounds.PlayOneShot(this.gearShiftClip, this.EffectsVolume);
		}
	}

	public void PlayShutdown()
	{
		if (this.shutdownClip != null)
		{
			this.miscSounds.PlayOneShot(this.shutdownClip, this.EffectsVolume);
		}
	}

	public void PlayStartSuccess()
	{
		if (this.startSuccessClip != null)
		{
			this.miscSounds.PlayOneShot(this.startSuccessClip, this.EffectsVolume);
		}
	}

	public void PlayStartFail()
	{
		if (this.startFailClips.Length > 0)
		{
			this.miscSounds.PlayOneShot(this.startFailClips[Random.Range(0, this.startFailClips.Length)], this.EffectsVolume);
		}
	}

	private void Start()
	{
		this._wait = new WaitForSeconds(0.15f);
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
		this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
		this.miscSounds = base.gameObject.AddComponent<AudioSource>();
		this.miscSounds.priority = this._soundPriority;
		this.miscSounds.volume = this.miscSoundsVolume;
		if (this.mainCamera != null)
		{
			if (Vector3.Distance(this.mainCamera.transform.position, base.gameObject.transform.position) <= this.maxDistance)
			{
				this.isCameraNear = true;
				if (this.idleVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel && this.engineIdle == null)
				{
					this.CreateIdle();
				}
				if (this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
				{
					if (this.gasPedalPressing)
					{
						if (this.lowOn == null)
						{
							this.CreateLowOn();
						}
					}
					else if (this.lowOff == null)
					{
						this.CreateLowOff();
					}
				}
				if (this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
				{
					if (this.gasPedalPressing)
					{
						if (this.medOn == null)
						{
							this.CreateMedOn();
						}
					}
					else if (this.medOff == null)
					{
						this.CreateMedOff();
					}
				}
				if (this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
				{
					if (this.gasPedalPressing)
					{
						if (this.highOn == null)
						{
							this.CreateHighOn();
						}
					}
					else if (this.highOff == null)
					{
						this.CreateHighOff();
					}
				}
				if (this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel && this.useRPMLimit && this.maxRPM == null)
				{
					this.CreateRPMLimit();
				}
				if (this.enableReverseGear)
				{
					if (this.isReversing)
					{
						if (this.reversingVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel && this.reversing == null)
						{
							this.CreateReverse();
						}
					}
					else if (this.reversing != null)
					{
						Object.Destroy(this.reversing);
					}
				}
				this.reverbZoneControll = this.reverbZoneSetting;
				this.SetReverbZone();
			}
			else
			{
				this.isCameraNear = false;
			}
		}
		this.UpdateStartRange();
	}

	private void Update()
	{
		if (this.mainCamera == null && Time.time > this.mainCameraGetAttemptTimestamp + 1f)
		{
			this.mainCamera = Camera.main;
			this.mainCameraGetAttemptTimestamp = Time.time;
		}
		if (this.isCameraNear)
		{
			if (this.shakeVolumeChangeDetect != this.shakeVolumeChange)
			{
				this.UpdateStartRange();
			}
			if (this.engineShakeSetting == RealisticEngineSound.EngineShake.Off)
			{
				this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
				if (this.gasPedalPressing)
				{
					this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 1f, Time.deltaTime * this.gasPedalSimSpeed);
				}
				else
				{
					this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 0f, Time.deltaTime * this.gasPedalSimSpeed);
				}
			}
			if (this.engineShakeSetting == RealisticEngineSound.EngineShake.AllwaysOn)
			{
				if (this.gasPedalPressing)
				{
					if (this.lenght < 1f)
					{
						if (this.shakeLenghtSetting == RealisticEngineSound.ShakeLenghtType.Fix)
						{
							this.gasPedalValue = this._oscillateOffset + Mathf.Sin(Time.time * (this.shakeLength * this.clipsValue)) * this._oscillateRange;
							this.clipsValue2 = this.engineCurrentRPM / this.maxRPMLimit + Mathf.Sin(Time.time * this.shakeLength) * (this._oscillateRange / 10f);
						}
						if (this.shakeLenghtSetting == RealisticEngineSound.ShakeLenghtType.Random)
						{
							this.gasPedalValue = this._oscillateOffset + Mathf.Sin(Time.time * ((float)Random.Range(10, 100) * this.clipsValue)) * this._oscillateRange;
							this.clipsValue2 = this.engineCurrentRPM / this.maxRPMLimit + Mathf.Sin(Time.time * (float)Random.Range(10, 100)) * (this._oscillateRange / 10f);
						}
						this.lenght += Random.Range(0.01f, 0.12f);
						this.clipsValue = this.clipsValue2;
					}
					else
					{
						this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 1f, Time.deltaTime * this.gasPedalSimSpeed);
						this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
					}
				}
				else
				{
					this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 0f, Time.deltaTime * this.gasPedalSimSpeed);
					this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
					this.lenght = 0f;
				}
			}
			if (this.engineShakeSetting == RealisticEngineSound.EngineShake.Random)
			{
				if (this.gasPedalPressing)
				{
					this.randomShakingValue2 = 0f;
					if (this.randomShakingValue == 0f)
					{
						this.randomShakingValue = Random.Range(0.1f, 1f);
					}
					if (this.randomShakingValue < this.randomChance)
					{
						if (this.lenght < 1f)
						{
							if (this.shakeLenghtSetting == RealisticEngineSound.ShakeLenghtType.Fix)
							{
								this.gasPedalValue = this._oscillateOffset + Mathf.Sin(Time.time * (this.shakeLength * this.clipsValue)) * this._oscillateRange;
								this.clipsValue2 = this.engineCurrentRPM / this.maxRPMLimit + Mathf.Sin(Time.time * this.shakeLength) * (this._oscillateRange / 10f);
							}
							if (this.shakeLenghtSetting == RealisticEngineSound.ShakeLenghtType.Random)
							{
								this.gasPedalValue = this._oscillateOffset + Mathf.Sin(Time.time * ((float)Random.Range(10, 100) * this.clipsValue)) * this._oscillateRange;
								this.clipsValue2 = this.engineCurrentRPM / this.maxRPMLimit + Mathf.Sin(Time.time * (float)Random.Range(10, 100)) * (this._oscillateRange / 10f);
							}
							this.lenght += Random.Range(0.01f, 0.12f);
							this.clipsValue = this.clipsValue2;
						}
						else
						{
							this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 1f, Time.deltaTime * this.gasPedalSimSpeed);
							this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
						}
					}
					else
					{
						this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 1f, Time.deltaTime * this.gasPedalSimSpeed);
						this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
					}
				}
				else
				{
					this.clipsValue = this.engineCurrentRPM / this.maxRPMLimit;
					this.randomShakingValue = 0f;
					if (this.randomShakingValue2 == 0f)
					{
						this.randomShakingValue2 = Random.Range(0.1f, 1f);
					}
					this.lenght = 0f;
					this.gasPedalValue = Mathf.Lerp(this.gasPedalValue, 0f, Time.deltaTime * this.gasPedalSimSpeed);
				}
			}
			if (this.idleClip != null)
			{
				if (this.idleVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
				{
					if (this.engineIdle == null)
					{
						this.CreateIdle();
					}
					else
					{
						this.engineIdle.volume = this.idleVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
						this.engineIdle.pitch = this.idlePitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
				}
				else
				{
					Object.Destroy(this.engineIdle);
				}
			}
			if (this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
			{
				if (this.gasPedalPressing)
				{
					if (this.lowOnClip != null)
					{
						if (this.lowOn == null)
						{
							this.CreateLowOn();
						}
						else
						{
							this.lowOn.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
							this.lowOn.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
							if (this.lowOff != null)
							{
								this.lowOff.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
								this.lowOff.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
								if (this.lowOff.volume < 0.1f)
								{
									Object.Destroy(this.lowOff);
								}
							}
						}
					}
				}
				else if (this.lowOffClip != null)
				{
					if (this.lowOff == null)
					{
						this.CreateLowOff();
					}
					else if (!this.isReversing)
					{
						this.lowOff.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
						this.lowOff.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
					if (this.lowOn != null)
					{
						this.lowOn.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
						this.lowOn.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						if (this.lowOn.volume < 0.1f)
						{
							Object.Destroy(this.lowOn);
						}
					}
				}
			}
			else
			{
				if (this.lowOn != null)
				{
					Object.Destroy(this.lowOn);
				}
				if (this.lowOff != null)
				{
					Object.Destroy(this.lowOff);
				}
			}
			if (this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
			{
				if (this.gasPedalPressing)
				{
					if (this.medOnClip != null)
					{
						if (this.medOn == null)
						{
							this.CreateMedOn();
						}
						else
						{
							this.medOn.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
							this.medOn.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						}
						if (this.medOff != null)
						{
							this.medOff.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
							this.medOff.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
							if (this.medOff.volume < 0.1f)
							{
								Object.Destroy(this.medOff);
							}
						}
					}
				}
				else if (this.medOffClip != null)
				{
					if (this.medOff == null)
					{
						this.CreateMedOff();
					}
					else if (!this.isReversing)
					{
						this.medOff.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
						this.medOff.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
					if (this.medOn != null)
					{
						this.medOn.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
						this.medOn.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						if (this.medOn.volume < 0.1f)
						{
							Object.Destroy(this.medOn);
						}
					}
				}
			}
			else
			{
				if (this.medOn != null)
				{
					Object.Destroy(this.medOn);
				}
				if (this.medOff != null)
				{
					Object.Destroy(this.medOff);
				}
			}
			if (this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
			{
				if (this.gasPedalPressing)
				{
					if (this.highOnClip != null)
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
									this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
								}
								else
								{
									this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue / 3.3f;
								}
							}
							else
							{
								this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
							}
							this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						}
						if (!this.isReversing && this.highOff != null)
						{
							this.highOff.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
							this.highOff.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
							if (this.highOff.volume < 0.1f)
							{
								Object.Destroy(this.highOff);
							}
						}
					}
				}
				else if (this.highOffClip != null)
				{
					if (this.highOff == null)
					{
						this.CreateHighOff();
					}
					else if (!this.isReversing)
					{
						this.highOff.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
						this.highOff.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
					}
					if (!this.isReversing && this.highOn != null)
					{
						if (this.maxRPM != null)
						{
							if (this.maxRPM.volume < 0.95f)
							{
								this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
							}
							else
							{
								this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue / 3.3f;
							}
						}
						else
						{
							this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
						}
						this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						if (this.highOn.volume < 0.1f)
						{
							Object.Destroy(this.highOn);
						}
					}
				}
			}
			else
			{
				if (this.highOn != null)
				{
					Object.Destroy(this.highOn);
				}
				if (this.highOff != null)
				{
					Object.Destroy(this.highOff);
				}
			}
			if (this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
			{
				if (this.maxRPMClip != null)
				{
					if (this.useRPMLimit)
					{
						if (this.maxRPM == null)
						{
							this.CreateRPMLimit();
						}
						else
						{
							this.maxRPM.volume = this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
							this.maxRPM.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
						}
					}
				}
				else
				{
					this.useRPMLimit = false;
				}
			}
			else
			{
				Object.Destroy(this.maxRPM);
			}
			if (this.enableReverseGear)
			{
				if (this.reversingClip != null)
				{
					if (this.isReversing)
					{
						if (this.reversingVolCurve.Evaluate(this.clipsValue) * this.MasterVolume > this.optimisationLevel)
						{
							if (this.reversing == null)
							{
								this.CreateReverse();
							}
							else
							{
								if (this.gasPedalPressing)
								{
									if (this.highOn == null)
									{
										this.CreateHighOn();
									}
									else
									{
										if (this.maxRPM != null)
										{
											if (this.maxRPM.volume < 0.95f)
											{
												this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
											}
											else
											{
												this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue / 3.3f;
											}
										}
										else
										{
											this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
										}
										this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
									}
									if (this.highOff != null)
									{
										this.highOff.volume = this.highVolCurve.Evaluate(this.clipsValue) * (1f - this.gasPedalValue);
										this.highOff.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
										if (this.highOff.volume < 0.1f)
										{
											Object.Destroy(this.highOff);
										}
									}
								}
								else
								{
									if (this.highOff == null)
									{
										this.CreateHighOff();
									}
									else
									{
										this.highOff.volume = this.highVolCurve.Evaluate(this.clipsValue) * (1f - this.gasPedalValue);
										this.highOff.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
									}
									if (this.highOn != null)
									{
										if (this.maxRPM != null)
										{
											if (this.maxRPM.volume < 0.95f)
											{
												this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
											}
											else
											{
												this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue / 3.3f;
											}
										}
										else
										{
											this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
										}
										this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
										if (this.highOn.volume < 0.1f)
										{
											Object.Destroy(this.highOn);
										}
									}
								}
								this.reversing.volume = this.reversingVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
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
			if (this.reverbZoneControll != this.reverbZoneSetting)
			{
				this.SetReverbZone();
			}
		}
		else
		{
			this.isCameraNear = false;
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.WaitForStart());
		this.SetReverbZone();
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
		if (this.lowOff != null)
		{
			Object.Destroy(this.lowOff);
		}
		if (this.medOn != null)
		{
			Object.Destroy(this.medOn);
		}
		if (this.medOff != null)
		{
			Object.Destroy(this.medOff);
		}
		if (this.highOn != null)
		{
			Object.Destroy(this.highOn);
		}
		if (this.highOff != null)
		{
			Object.Destroy(this.highOff);
		}
		if (this.useRPMLimit && this.maxRPM != null)
		{
			Object.Destroy(this.maxRPM);
		}
		if (this.enableReverseGear && this.reversing != null)
		{
			Object.Destroy(this.reversing);
		}
		if (base.gameObject.GetComponent<AudioReverbZone>() != null)
		{
			Object.Destroy(base.gameObject.GetComponent<AudioReverbZone>());
		}
	}

	private void UpdateStartRange()
	{
		this._oscillateRange = (this._endRange - (1f - this.shakeVolumeChange)) / 2f;
		this._oscillateOffset = this._oscillateRange + (1f - this.shakeVolumeChange);
		this.shakeVolumeChangeDetect = this.shakeVolumeChange;
	}

	private void SetReverbZone()
	{
		if (this.reverbZoneSetting == null)
		{
			if (base.gameObject.GetComponent<AudioReverbZone>() != null)
			{
				Object.Destroy(base.gameObject.GetComponent<AudioReverbZone>());
			}
		}
		else if (base.gameObject.GetComponent<AudioReverbZone>() == null)
		{
			base.gameObject.AddComponent<AudioReverbZone>();
			base.gameObject.GetComponent<AudioReverbZone>().reverbPreset = this.reverbZoneSetting;
		}
		else
		{
			base.gameObject.GetComponent<AudioReverbZone>().reverbPreset = this.reverbZoneSetting;
		}
		this.reverbZoneControll = this.reverbZoneSetting;
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
			this.engineIdle.priority = this._soundPriority;
			this.engineIdle.spatialBlend = this.spatialBlend;
			this.engineIdle.rolloffMode = this.audioRolloffMode;
			this.engineIdle.dopplerLevel = this.dopplerLevel;
			this.engineIdle.volume = this.idleVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
			this.engineIdle.pitch = this.idlePitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.engineIdle.minDistance = this.minDistance;
			this.engineIdle.maxDistance = this.maxDistance;
			this.engineIdle.clip = this.idleClip;
			this.engineIdle.loop = true;
			this.engineIdle.Play();
			if (this.audioMixer != null)
			{
				this.engineIdle.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateLowOff()
	{
		if (this.lowOffClip != null)
		{
			this.lowOff = base.gameObject.AddComponent<AudioSource>();
			this.lowOff.priority = this._soundPriority;
			this.lowOff.spatialBlend = this.spatialBlend;
			this.lowOff.rolloffMode = this.audioRolloffMode;
			this.lowOff.dopplerLevel = this.dopplerLevel;
			this.lowOff.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
			this.lowOff.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.lowOff.minDistance = this.minDistance;
			this.lowOff.maxDistance = this.maxDistance;
			this.lowOff.clip = this.lowOffClip;
			this.lowOff.loop = true;
			this.lowOff.Play();
			if (this.audioMixer != null)
			{
				this.lowOff.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateLowOn()
	{
		if (this.lowOnClip != null)
		{
			this.lowOn = base.gameObject.AddComponent<AudioSource>();
			this.lowOn.priority = this._soundPriority;
			this.lowOn.spatialBlend = this.spatialBlend;
			this.lowOn.rolloffMode = this.audioRolloffMode;
			this.lowOn.dopplerLevel = this.dopplerLevel;
			this.lowOn.volume = this.lowVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
			this.lowOn.pitch = this.lowPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.lowOn.minDistance = this.minDistance;
			this.lowOn.maxDistance = this.maxDistance;
			this.lowOn.clip = this.lowOnClip;
			this.lowOn.loop = true;
			this.lowOn.Play();
			if (this.audioMixer != null)
			{
				this.lowOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateMedOff()
	{
		if (this.medOffClip != null)
		{
			this.medOff = base.gameObject.AddComponent<AudioSource>();
			this.medOff.priority = this._soundPriority;
			this.medOff.spatialBlend = this.spatialBlend;
			this.medOff.rolloffMode = this.audioRolloffMode;
			this.medOff.dopplerLevel = this.dopplerLevel;
			this.medOff.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
			this.medOff.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.medOff.minDistance = this.minDistance;
			this.medOff.maxDistance = this.maxDistance;
			this.medOff.clip = this.medOffClip;
			this.medOff.loop = true;
			this.medOff.Play();
			if (this.audioMixer != null)
			{
				this.medOff.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateMedOn()
	{
		if (this.medOnClip != null)
		{
			this.medOn = base.gameObject.AddComponent<AudioSource>();
			this.medOn.priority = this._soundPriority;
			this.medOn.spatialBlend = this.spatialBlend;
			this.medOn.rolloffMode = this.audioRolloffMode;
			this.medOn.dopplerLevel = this.dopplerLevel;
			this.medOn.volume = this.medVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
			this.medOn.pitch = this.medPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.medOn.minDistance = this.minDistance;
			this.medOn.maxDistance = this.maxDistance;
			this.medOn.clip = this.medOnClip;
			this.medOn.loop = true;
			this.medOn.Play();
			if (this.audioMixer != null)
			{
				this.medOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateHighOff()
	{
		if (this.highOffClip != null)
		{
			this.highOff = base.gameObject.AddComponent<AudioSource>();
			this.highOff.priority = this._soundPriority;
			this.highOff.spatialBlend = this.spatialBlend;
			this.highOff.rolloffMode = this.audioRolloffMode;
			this.highOff.dopplerLevel = this.dopplerLevel;
			this.highOff.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * (1f - this.gasPedalValue);
			this.highOff.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.highOff.minDistance = this.minDistance;
			this.highOff.maxDistance = this.maxDistance;
			this.highOff.clip = this.highOffClip;
			this.highOff.loop = true;
			this.highOff.Play();
			if (this.audioMixer != null)
			{
				this.highOff.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateHighOn()
	{
		if (this.highOnClip != null)
		{
			this.highOn = base.gameObject.AddComponent<AudioSource>();
			this.highOn.priority = this._soundPriority;
			this.highOn.spatialBlend = this.spatialBlend;
			this.highOn.rolloffMode = this.audioRolloffMode;
			this.highOn.dopplerLevel = this.dopplerLevel;
			this.highOn.volume = this.highVolCurve.Evaluate(this.clipsValue) * this.MasterVolume * this.gasPedalValue;
			this.highOn.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.highOn.minDistance = this.minDistance;
			this.highOn.maxDistance = this.maxDistance;
			this.highOn.clip = this.highOnClip;
			this.highOn.loop = true;
			this.highOn.Play();
			if (this.audioMixer != null)
			{
				this.highOn.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	private void CreateRPMLimit()
	{
		if (this.maxRPMClip != null)
		{
			this.maxRPM = base.gameObject.AddComponent<AudioSource>();
			this.maxRPM.priority = this._soundPriority;
			this.maxRPM.spatialBlend = this.spatialBlend;
			this.maxRPM.rolloffMode = this.audioRolloffMode;
			this.maxRPM.dopplerLevel = this.dopplerLevel;
			this.maxRPM.volume = this.maxRPMVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
			this.maxRPM.pitch = this.highPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.maxRPM.minDistance = this.minDistance;
			this.maxRPM.maxDistance = this.maxDistance;
			this.maxRPM.clip = this.maxRPMClip;
			this.maxRPM.loop = true;
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
			this.reversing.priority = this._soundPriority;
			this.reversing.spatialBlend = this.spatialBlend;
			this.reversing.rolloffMode = this.audioRolloffMode;
			this.reversing.dopplerLevel = this.dopplerLevel;
			this.reversing.volume = this.reversingVolCurve.Evaluate(this.clipsValue) * this.MasterVolume;
			this.reversing.pitch = this.reversingPitchCurve.Evaluate(this.clipsValue) * this.pitchMultiplier;
			this.reversing.minDistance = this.minDistance;
			this.reversing.maxDistance = this.maxDistance;
			this.reversing.clip = this.reversingClip;
			this.reversing.loop = true;
			this.reversing.Play();
			if (this.audioMixer != null)
			{
				this.reversing.outputAudioMixerGroup = this.audioMixer;
			}
		}
	}

	public const float EngineVolumeScale = 0.6f;

	[Range(0.1f, 1f)]
	public float masterVolume = 1f;

	public AudioMixerGroup audioMixer;

	public float engineCurrentRPM;

	public bool gasPedalPressing;

	[Range(0f, 1f)]
	public float gasPedalValue = 1f;

	public RealisticEngineSound.GasPedalValue gasPedalValueSetting;

	[Range(1f, 15f)]
	public float gasPedalSimSpeed = 5.5f;

	public float maxRPMLimit = 7000f;

	[Range(0f, 5f)]
	public float dopplerLevel = 1f;

	[Range(0f, 1f)]
	public float spatialBlend;

	[Range(0.1f, 2f)]
	public float pitchMultiplier = 1f;

	[Range(0f, 1f)]
	public float miscSoundsVolume = 0.5f;

	public AudioReverbPreset reverbZoneSetting;

	private AudioReverbPreset reverbZoneControll;

	[Range(0f, 0.25f)]
	public float optimisationLevel = 0.01f;

	public AudioRolloffMode audioRolloffMode = 2;

	public float minDistance = 1f;

	public float maxDistance = 50f;

	public bool isReversing;

	public bool useRPMLimit = true;

	public bool enableReverseGear = true;

	[HideInInspector]
	public float carCurrentSpeed;

	[HideInInspector]
	public float carMaxSpeed;

	[HideInInspector]
	public bool isShifting;

	public AudioClip gearShiftClip;

	public AudioClip shutdownClip;

	public AudioClip[] startFailClips;

	public AudioClip startSuccessClip;

	public AudioClip idleClip;

	public AnimationCurve idleVolCurve;

	public AnimationCurve idlePitchCurve;

	public AudioClip lowOffClip;

	public AudioClip lowOnClip;

	public AnimationCurve lowVolCurve;

	public AnimationCurve lowPitchCurve;

	public AudioClip medOffClip;

	public AudioClip medOnClip;

	public AnimationCurve medVolCurve;

	public AnimationCurve medPitchCurve;

	public AudioClip highOffClip;

	public AudioClip highOnClip;

	public AnimationCurve highVolCurve;

	public AnimationCurve highPitchCurve;

	public AudioClip maxRPMClip;

	public AnimationCurve maxRPMVolCurve;

	public AudioClip reversingClip;

	public AnimationCurve reversingVolCurve;

	public AnimationCurve reversingPitchCurve;

	[SerializeField]
	[Range(0f, 255f)]
	private int _soundPriority = 20;

	private AudioSource miscSounds;

	private AudioSource engineIdle;

	private AudioSource lowOff;

	private AudioSource lowOn;

	private AudioSource medOff;

	private AudioSource medOn;

	private AudioSource highOff;

	private AudioSource highOn;

	private AudioSource maxRPM;

	private AudioSource reversing;

	private float clipsValue;

	private float clipsValue2;

	public Camera mainCamera;

	[HideInInspector]
	public bool isCameraNear;

	public RealisticEngineSound.EngineShake engineShakeSetting;

	[HideInInspector]
	public RealisticEngineSound.ShakeLenghtType shakeLenghtSetting;

	[HideInInspector]
	public float shakeLength = 50f;

	[HideInInspector]
	public float shakeVolumeChange = 0.35f;

	[HideInInspector]
	public float randomChance = 0.5f;

	private float _endRange = 1f;

	private float shakeVolumeChangeDetect;

	private float _oscillateRange;

	private float _oscillateOffset;

	private float lenght;

	private float randomShakingValue;

	private float randomShakingValue2;

	private WaitForSeconds _wait;

	private bool alreadyDestroyed;

	private float mainCameraGetAttemptTimestamp;

	public enum GasPedalValue
	{
		Simulated,
		NotSimulated
	}

	public enum EngineShake
	{
		Off,
		Random,
		AllwaysOn
	}

	public enum ShakeLenghtType
	{
		Fix,
		Random
	}
}
