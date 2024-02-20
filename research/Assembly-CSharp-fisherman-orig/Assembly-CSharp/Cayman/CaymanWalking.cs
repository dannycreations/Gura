using System;
using UnityEngine;

namespace Cayman
{
	public class CaymanWalking : CaymanActivity
	{
		private void Awake()
		{
			this._animation = base.GetComponent<Animation>();
			this._sounds = base.GetComponents<AudioSource>();
			this._animation.AddClip(this._leftAnimation, this._leftAnimation.name);
			this._animation.AddClip(this._rightAnimation, this._rightAnimation.name);
		}

		public void Init(Transform pivot)
		{
			base.transform.rotation = pivot.rotation;
			base.transform.position = pivot.position;
			string text = string.Empty;
			if (GameFactory.Player != null)
			{
				Vector3 vector = Math3d.ProjectOXZ(GameFactory.Player.transform.position - base.transform.position);
				float num = Math3d.ClampAngleTo180(Vector3.SignedAngle(base.transform.forward, vector, Vector3.up));
				text = ((num <= 0f) ? this._leftAnimation.name : this._rightAnimation.name);
			}
			else
			{
				text = ((Random.Range(0f, 1f) >= 0.5f) ? this._rightAnimation.name : this._leftAnimation.name);
			}
			this._animation.Play(text);
			this._sounds[0].clip = this._movementSound;
			this._sounds[0].loop = true;
			if (this._movementSound != null)
			{
				this._sounds[0].Play();
			}
			if (this._waterContactSound != null)
			{
				this._sounds[1].clip = this._waterContactSound;
				this._sounds[1].PlayDelayed(this._startWaterSoundDelay);
			}
			this._timers.AddTimer(CaymanWalking.Actions.PAUSE_MOVEMENT, this._pauseMovementDelay, delegate
			{
				if (this._sounds[0].clip != null)
				{
					this._sounds[0].Stop();
				}
			}, false);
			this._timers.AddTimer(CaymanWalking.Actions.GROWL, this._growlDelay, delegate
			{
				this._sounds[0].clip = this._growlSound;
				if (this._sounds[0].clip != null)
				{
					this._sounds[0].loop = false;
					this._sounds[0].Play();
				}
			}, false);
			this._timers.AddTimer(CaymanWalking.Actions.RESUME_MOVEMENT, this._resumeMovementDelay, delegate
			{
				this._sounds[0].clip = this._movementSound;
				this._sounds[0].loop = true;
				if (this._sounds[0].clip != null)
				{
					this._sounds[0].Play();
				}
			}, false);
			this._timers.AddTimer(CaymanWalking.Actions.STOP_MOVEMENT_SOUND, this._stopMovementDelay, delegate
			{
				if (this._sounds[0].clip != null)
				{
					this._sounds[0].Stop();
				}
			}, false);
			this._timers.AddTimer(CaymanWalking.Actions.STOP_WATER_SOUND, this._stopWaterSoundDelay, delegate
			{
				if (this._sounds[1].clip != null)
				{
					this._sounds[1].Stop();
				}
			}, false);
			this._timers.AddTimer(CaymanWalking.Actions.DESTROY, this._lifeTime, delegate
			{
				Object.Destroy(base.gameObject);
			}, false);
		}

		private void Update()
		{
			this._timers.Update(Time.deltaTime);
			Vector3 position = this._rootBone.position;
			float num = this._groundCorrection * Mathf.Cos(Mathf.Abs(base.transform.eulerAngles.x) * 3.1415927f);
			Vector3? groundCollision = Math3d.GetGroundCollision(position);
			if (groundCollision != null)
			{
				float num2 = position.y - groundCollision.Value.y;
				base.transform.position -= new Vector3(0f, num2 - num, 0f);
			}
			if (GameFactory.Water != null)
			{
				if ((int)this._disturbsCounter == 0)
				{
					this._disturbsCounter = this._ticksAfterDisturbs;
					for (int i = 0; i < this._waterDisturbers.Length; i++)
					{
						Vector3 position2 = this._waterDisturbers[i].position;
						float num3 = ((position2.y <= 0f) ? Mathf.Clamp01(1f - position2.y / this._minDisturbsPowerAtHeight) : 0f);
						GameFactory.Water.AddWaterDisturb(position2, Mathf.Lerp(0f, this._disturbanceMaxRadius, num3), Mathf.Lerp(0f, (float)((byte)this._disturbanceMaxPower), num3));
					}
				}
				else
				{
					this._disturbsCounter = (sbyte)((int)this._disturbsCounter - 1);
				}
			}
		}

		private void OnDestroy()
		{
		}

		[SerializeField]
		private float _lifeTime = 20f;

		[SerializeField]
		private Transform _rootBone;

		[SerializeField]
		private float _groundCorrection;

		[SerializeField]
		private AnimationClip _leftAnimation;

		[SerializeField]
		private AnimationClip _rightAnimation;

		[SerializeField]
		private AudioClip _movementSound;

		[SerializeField]
		private float _pauseMovementDelay = 4f;

		[SerializeField]
		private float _resumeMovementDelay = 8.5f;

		[SerializeField]
		private float _stopMovementDelay = 11f;

		[SerializeField]
		private AudioClip _growlSound;

		[SerializeField]
		private float _growlDelay = 5.3f;

		[SerializeField]
		private AudioClip _waterContactSound;

		[SerializeField]
		private float _startWaterSoundDelay = 9f;

		[SerializeField]
		private float _stopWaterSoundDelay = 10f;

		[SerializeField]
		private Transform[] _waterDisturbers;

		[SerializeField]
		private float _minDisturbsPowerAtHeight = -0.3f;

		[SerializeField]
		private float _disturbanceMaxRadius = 0.3f;

		[SerializeField]
		private WaterDisturbForce _disturbanceMaxPower = WaterDisturbForce.Medium;

		[SerializeField]
		private sbyte _ticksAfterDisturbs = 5;

		private TimerCore<CaymanWalking.Actions> _timers = new TimerCore<CaymanWalking.Actions>();

		private Animation _animation;

		private AudioSource[] _sounds;

		private sbyte _disturbsCounter;

		private enum Actions
		{
			PAUSE_MOVEMENT,
			GROWL,
			RESUME_MOVEMENT,
			STOP_MOVEMENT_SOUND,
			STOP_WATER_SOUND,
			DESTROY
		}
	}
}
