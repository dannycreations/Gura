using System;
using System.Collections;
using DG.Tweening;
using ObjectModel;
using Phy.Verlet;
using UnityEngine;

public class RodStandSignalizer : MonoBehaviour
{
	public Vector3 ClipPosition
	{
		get
		{
			return this.ClipTransform.position;
		}
	}

	public Vector3 NestPosition
	{
		get
		{
			return base.transform.position + Vector3.up * this.signalizerNestUpOffset;
		}
	}

	private void Start()
	{
		this.startRotation = this.SignalizerTrnsform.localRotation;
		this.LightSource.enabled = false;
		this.RedLightSource.enabled = false;
		this.desiredRotation = this.startRotation;
	}

	public void SetState(RodStandSignalizer.State state)
	{
		if (this.currntState == RodStandSignalizer.State.Empty && state != this.currntState)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.Setup());
		}
		this.currntState = state;
		switch (this.currntState)
		{
		case RodStandSignalizer.State.Empty:
			this.desiredRotation = this.startRotation;
			this.RedLightSource.intensity = 0f;
			this.LightSource.intensity = 0f;
			this.LightSource.enabled = false;
			this.RedLightSource.enabled = false;
			break;
		case RodStandSignalizer.State.Idle:
			this.desiredRotation = this.startRotation * Quaternion.Euler(Vector3.right * this.loadedIdleAngle);
			break;
		case RodStandSignalizer.State.Signalling:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private IEnumerator Setup()
	{
		this.Beep(this.minVolumeScale);
		this.Blink(0.7f, false);
		yield return new WaitForSeconds(0.3f);
		this.Beep(this.minVolumeScale);
		this.Blink(0.7f, false);
		yield break;
	}

	public void SetParams(Bell parameters)
	{
		this.beepStep = this.baseBeepStepDistance - this.beepStepSensitivityRange * Mathf.Min(1f, Mathf.Max(0f, parameters.Sensitivity));
		this.AudioSource.volume = 1f * parameters.Loudness;
	}

	public void Blink(float time = 0.25f, bool red = false)
	{
		Light currLightSource = ((!red) ? this.LightSource : this.RedLightSource);
		double num = ((!red) ? this.lastBlink : this.lastredBlink);
		double num2 = (double)Time.time;
		if (num2 - num < 0.25)
		{
			return;
		}
		if (red)
		{
			this.lastredBlink = num2;
		}
		else
		{
			this.lastBlink = num2;
		}
		ShortcutExtensions.DOKill(currLightSource, false);
		currLightSource.color = ((!red) ? this.SignalizerColor : Color.red);
		currLightSource.intensity = ((!red) ? 0f : this.lightMaxIntensity);
		currLightSource.range = this.lightRange;
		currLightSource.enabled = true;
		TweenSettingsExtensions.OnStepComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOIntensity(currLightSource, (!red) ? this.lightMaxIntensity : 0f, time * 0.2f), 32), delegate
		{
			TweenSettingsExtensions.SetDelay<Tweener>(ShortcutExtensions.DOIntensity(currLightSource, (!red) ? 0f : this.lightMaxIntensity, time * 0.4f), time * 0.4f);
		});
	}

	public void Beep(float volumeScale)
	{
		volumeScale *= SettingsManager.EnvironmentForcedVolume;
		this.AudioSource.Stop();
		this.AudioSource.pitch = this.MyPitch;
		this.AudioSource.PlayOneShot(this.SignalizerAudioClip, volumeScale);
	}

	private void Update()
	{
		bool flag = this.Controller != null && this.Controller.IsSlotOccupied(this.slotId);
		Rod1stBehaviour rod1stBehaviour = ((!flag) ? null : (this.Controller.GetOccupiedSlotData(this.slotId).Rod.Behaviour as Rod1stBehaviour));
		if (flag != this.lastOccupied)
		{
			this.timeThreshold = Time.time + this.waitTime;
		}
		this.lastOccupied = flag;
		RodOnPodBehaviour.PodFishState podFishState = RodOnPodBehaviour.PodFishState.None;
		if (rod1stBehaviour != null)
		{
			if (rod1stBehaviour.Tackle != null && rod1stBehaviour.Tackle.Fish != null)
			{
				Type state = rod1stBehaviour.Tackle.Fish.State;
				if (state == typeof(FishBite) || state == typeof(FishAttack) || state == typeof(FishPredatorAttack))
				{
					podFishState = RodOnPodBehaviour.PodFishState.Attacking;
				}
				else if (rod1stBehaviour.Tackle.IsFishHooked || state == typeof(FishSwimAway))
				{
					podFishState = RodOnPodBehaviour.PodFishState.Hooked;
				}
			}
			if ((podFishState == RodOnPodBehaviour.PodFishState.Attacking || podFishState == RodOnPodBehaviour.PodFishState.Hooked) && rod1stBehaviour.Tackle.Fish != null)
			{
				AbstractFishBody abstractFishBody = rod1stBehaviour.Tackle.Fish.FishObject as AbstractFishBody;
				Vector3 velocity = abstractFishBody.Root.Velocity;
				Vector3 normalized = (abstractFishBody.Root.Position - rod1stBehaviour.CurrentTipPosition).normalized;
				float num = Vector3.Dot(normalized, velocity);
				Vector3 position = rod1stBehaviour.TackleTipMass.Position;
				Vector3 position2 = abstractFishBody.Mouth.Position;
				Vector3 position3 = abstractFishBody.Root.Position;
				float magnitude = (position2 - position).magnitude;
				float magnitude2 = (position2 - position3).magnitude;
				float magnitude3 = (position - position3).magnitude;
				if (magnitude3 <= magnitude2 || magnitude <= magnitude2 || podFishState == RodOnPodBehaviour.PodFishState.Hooked)
				{
					this.currentLineTension = Mathf.Max(0f, Mathf.Min(velocity.magnitude / this.velocityValueNormalizer + this.directionFactor * num / velocity.magnitude, 1f));
					this.fishTravelDistance += Mathf.Abs(velocity.magnitude * Time.fixedDeltaTime);
					float num2 = (this.currentLineTension - this.lineSensitivityThreshold) / (1f - this.lineSensitivityThreshold);
					float num3 = this.minVolumeScale + (this.maxVolumeScale - this.minVolumeScale) * num2;
					if (this.fishTravelDistance >= this.beepStep)
					{
						this.fishTravelDistance -= this.beepStep;
						if (Time.time - this.lastBeepTime > this.minBeepPeriod)
						{
							this.lastBeepTime = Time.time;
							this.Beep(num3);
							this.Blink(0.25f, false);
						}
					}
				}
				else
				{
					this.fishTravelDistance = 0f;
				}
			}
			if (podFishState == RodOnPodBehaviour.PodFishState.Attacking)
			{
				this.currentLineTension = Mathf.Max(this.lineSensitivityThreshold, this.currentLineTension);
			}
			else if (podFishState == RodOnPodBehaviour.PodFishState.Hooked)
			{
				this.currentLineTension = 1f;
			}
			else if (podFishState == RodOnPodBehaviour.PodFishState.None)
			{
				this.currentLineTension = 0f;
			}
		}
		else
		{
			this.currentLineTension = 0f;
		}
		if (rod1stBehaviour == null && this.currntState != RodStandSignalizer.State.Empty && Time.time > this.timeThreshold)
		{
			this.SetState(RodStandSignalizer.State.Empty);
			return;
		}
		if (rod1stBehaviour != null && this.currntState == RodStandSignalizer.State.Empty)
		{
			this.SetState(RodStandSignalizer.State.Idle);
			return;
		}
		if (this.currentLineTension >= this.lineSensitivityThreshold && this.currntState != RodStandSignalizer.State.Signalling)
		{
			this.SetState(RodStandSignalizer.State.Signalling);
			return;
		}
		if (this.currntState == RodStandSignalizer.State.Signalling && (!flag || Time.time > this.timeThreshold))
		{
			if (this.currentLineTension < this.lineSensitivityThreshold)
			{
				this.SetState(RodStandSignalizer.State.Idle);
				return;
			}
			float num2 = (this.currentLineTension - this.lineSensitivityThreshold) / (1f - this.lineSensitivityThreshold);
			this.desiredRotation = this.startRotation * Quaternion.Euler(Vector3.right * (this.loadedIdleAngle + (this.maxTriggeredAngle - this.loadedIdleAngle) * num2));
		}
		this.SignalizerTrnsform.localRotation = Quaternion.Lerp(this.SignalizerTrnsform.localRotation, this.desiredRotation, Time.deltaTime * 30f);
	}

	public Transform SignalizerTrnsform;

	public Transform ClipTransform;

	public AudioClip SignalizerAudioClip;

	public AudioSource AudioSource;

	public RodPodController Controller;

	public Color SignalizerColor;

	public Light LightSource;

	public Light RedLightSource;

	private float lightMaxIntensity = 15f;

	private float lightRange = 0.25f;

	public float signalizerNestUpOffset = 0.05f;

	private float maxTriggeredAngle = 87f;

	private float loadedIdleAngle = 45f;

	public static float maxPitch = 1.4f;

	public static float basePitch = 1f;

	private float minVolumeScale = 0.2f;

	private float maxVolumeScale = 0.7f;

	private float minBeepPeriod = 0.1f;

	private float lastBeepTime;

	public float lineSensitivityThreshold = 0.04f;

	public float currentLineTension;

	[Space(5f)]
	public int slotId;

	public bool test;

	private Quaternion startRotation;

	private RodStandSignalizer.State currntState;

	private Quaternion desiredRotation;

	public float velocityValueNormalizer = 0.5f;

	public float directionFactor = 0.2f;

	private float baseBeepStepDistance = 0.15f;

	private float beepStepSensitivityRange = 0.12f;

	private float beepStep = 0.1f;

	private float fishTravelDistance;

	public float MyPitch = 1f;

	private double lastBlink;

	private double lastredBlink;

	public float minSensitivity = 0.01f;

	public float maxSensitivity = 0.25f;

	private bool lastOccupied;

	private float waitTime = 2f;

	private float timeThreshold;

	public enum State
	{
		Empty,
		Idle,
		Signalling
	}
}
