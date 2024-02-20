using System;
using UnityEngine;

namespace Cayman
{
	public class CaymanJumping : CaymanActivity
	{
		public float InitialY
		{
			get
			{
				return this._initialY;
			}
		}

		private void Awake()
		{
			this._animator = base.GetComponent<Animation>();
			AnimationClip animationClip = (((double)Random.Range(0f, 1f) >= 0.5) ? this._rightAnimation : this._leftAnimation);
			this._animator.AddClip(animationClip, animationClip.name);
			this._animator.Play(animationClip.name);
			this._animation = this._animator[animationClip.name];
			this._timers.AddTimer(CaymanJumping.Actions.IN_WATER, this._outWaterTime, delegate
			{
				FishBoxActivity.PlayWaterContactEffect(new Vector3(this._waterDisturber.position.x, 0f, this._waterDisturber.position.z), this._outWaterSplashPrefabName, this._outWaterSplashSize, this._outWaterSound);
			}, false);
			this._timers.AddTimer(CaymanJumping.Actions.OUT_WATER, this._inWaterTime, delegate
			{
				FishBoxActivity.PlayWaterContactEffect(new Vector3(this._waterDisturber.position.x, 0f, this._waterDisturber.position.z), this._inWaterSplashPrefabName, this._inWaterSplashSize, this._inWaterSound);
			}, false);
			this._timers.AddTimer(CaymanJumping.Actions.GAG_SOUND, this._gagTime, delegate
			{
				if (!string.IsNullOrEmpty(this._gagSound))
				{
					RandomSounds.PlaySoundAtPoint(this._gagSound, base.transform.position, this._gagSoundVolume * SettingsManager.EnvironmentForcedVolume, false);
				}
			}, false);
		}

		public void Init(Vector3 fromPos, float yaw)
		{
			base.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);
			base.transform.position = new Vector3(fromPos.x, this._initialY, fromPos.z);
		}

		private void Update()
		{
			this._timers.Update(Time.deltaTime);
			if (this._animation.time > this._animation.length)
			{
				Object.Destroy(base.gameObject);
			}
		}

		[SerializeField]
		private float _initialY = -1f;

		[SerializeField]
		private float _outWaterTime = 0.5f;

		[SerializeField]
		private string _outWaterSplashPrefabName = "2D/Splashes/pSplash_universal";

		[SerializeField]
		private string _outWaterSound = "Sounds/Actions/Lure/Lure_Out";

		[SerializeField]
		private float _outWaterSplashSize = 1f;

		[SerializeField]
		private float _inWaterTime = 1f;

		[SerializeField]
		private string _inWaterSplashPrefabName = "2D/Splashes/pSplash_universal";

		[SerializeField]
		private string _inWaterSound = "Sounds/Actions/Lure/Lure_Splash_Big";

		[SerializeField]
		private float _inWaterSplashSize = 2f;

		[SerializeField]
		private float _gagTime = 0.75f;

		[SerializeField]
		private string _gagSound = string.Empty;

		[SerializeField]
		private float _gagSoundVolume = 0.5f;

		[SerializeField]
		private Transform _waterDisturber;

		[SerializeField]
		private AnimationClip _leftAnimation;

		[SerializeField]
		private AnimationClip _rightAnimation;

		private TimerCore<CaymanJumping.Actions> _timers = new TimerCore<CaymanJumping.Actions>();

		private Animation _animator;

		private AnimationState _animation;

		private enum Actions
		{
			OUT_WATER,
			IN_WATER,
			GAG_SOUND
		}
	}
}
