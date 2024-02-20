using System;
using UnityEngine;

public class FishBoxJumping : FishBoxActivity
{
	protected override void Awake()
	{
		base.Awake();
		this._fish.gameObject.SetActive(false);
		float num = Mathf.Min(this._minInitialSpeed, this._maxInitialSpeed) * Mathf.Sin(this._startPitch * 0.017453292f);
		this._actionDuration = 2f * num / this.G + this._actionAdditionalTime;
		this._fish.localScale = new Vector3(this._fishScale, this._fishScale, this._fishScale);
		this._curDirAngle = this.GetRandomYaw();
	}

	public override bool ManualUpdate()
	{
		if (base.ManualUpdate())
		{
			if (this._fish != null && this._actionStartedAt > 0f && this._actionStartedAt < Time.time)
			{
				this.UpdateFish();
			}
			return true;
		}
		return false;
	}

	protected override void StartAction()
	{
		base.StartAction();
		this._actionStartedAt = Time.time;
		if (!this._debugSameRotation)
		{
			this._curDirAngle = this.GetRandomYaw();
		}
		float num = Random.Range(this._minInitialSpeed, this._maxInitialSpeed);
		float num2 = this._startPitch * 0.017453292f;
		this._startVerticalSpeed = num * Mathf.Sin(num2);
		float num3 = num * Mathf.Cos(num2);
		this._startHorizontalSpeed = new Vector2(num3 * Mathf.Sin(this._curDirAngle), num3 * Mathf.Cos(this._curDirAngle));
		this._tUpMovement = this._startVerticalSpeed / this.G;
		this._vRotationSpeed = this._startPitch / this._tUpMovement;
		this._isOutWaterEffectPlayed = false;
		this._isInWaterEffectPlayed = false;
		this._fish.position = base.GetZoneRandomPos() + new Vector3(0f, this._fishStartDy, 0f);
		if ((GameFactory.Player.transform.position - this._fish.position).magnitude < this._distToPlayerToDisable)
		{
			this.StopAction();
			return;
		}
		int num4 = ((!this._isRotatedFish) ? (-1) : 1);
		this._curPitch = (float)num4 * this._startPitch;
		if (this._isFixedPitch)
		{
			this._curFixedPitch = (float)num4 * Random.Range(this._minFixedPitch, this._maxFixedPitch);
		}
		this._fish.rotation = Quaternion.Euler(this._curPitch, this._curDirAngle * 57.29578f + (float)((!this._isRotatedFish) ? 0 : 180), 0f);
		this._fish.gameObject.SetActive(true);
	}

	protected override float GetRandomYaw()
	{
		return (base.transform.eulerAngles.y + 90f) * 0.017453292f - base.GetRandomYaw();
	}

	protected override void StopAction()
	{
		base.StopAction();
		this._actionStartedAt = -1f;
		this._fish.gameObject.SetActive(false);
	}

	private void UpdateFish()
	{
		Vector2 vector = this._dt * this._startHorizontalSpeed;
		float num = Time.time - this._actionStartedAt;
		float num2 = this._fishStartDy + this._startVerticalSpeed * num - this.G * num * num / 2f;
		this._curPitch += (float)((!this._isRotatedFish) ? 1 : (-1)) * this._vRotationSpeed * this._dt;
		if (this._isFixedPitch)
		{
			if (this._isRotatedFish)
			{
				this._curPitch = Mathf.Max(this._curPitch, this._curFixedPitch);
			}
			else
			{
				this._curPitch = Mathf.Min(this._curPitch, this._curFixedPitch);
			}
		}
		this._fish.rotation = Quaternion.Euler(this._curPitch, this._curDirAngle * 57.29578f + (float)((!this._isRotatedFish) ? 0 : 180), 0f);
		this._fish.position = new Vector3(this._fish.position.x + vector.x, num2, this._fish.position.z + vector.y);
		if (!this._isOutWaterEffectPlayed)
		{
			if (this._fish.position.y >= 0f)
			{
				this._isOutWaterEffectPlayed = true;
				FishBoxActivity.PlayWaterContactEffect(new Vector3(this._fish.position.x, 0f, this._fish.position.z), this._outWaterSplashPrefabName, this._outWaterSplashSize, this._outWaterSound);
			}
		}
		else if (!this._isInWaterEffectPlayed)
		{
			if (this._fish.position.y <= 0f)
			{
				this._isInWaterEffectPlayed = true;
				FishBoxActivity.PlayWaterContactEffect(new Vector3(this._fish.position.x, 0f, this._fish.position.z), this._inWaterSplashPrefabName, this._inWaterSplashSize, this._inWaterSound);
			}
		}
		else if (Mathf.Abs(this._fish.position.y) > this._yToHideFish)
		{
			this.StopAction();
		}
	}

	[SerializeField]
	private Transform _fish;

	[SerializeField]
	private float _fishScale = 0.05f;

	[SerializeField]
	private float _minInitialSpeed = 1f;

	[SerializeField]
	private float _maxInitialSpeed = 2f;

	[SerializeField]
	private bool _isRotatedFish;

	[SerializeField]
	private float _startPitch = 45f;

	[SerializeField]
	private bool _isFixedPitch;

	[SerializeField]
	private float _minFixedPitch = 5f;

	[SerializeField]
	private float _maxFixedPitch = 15f;

	[SerializeField]
	private float _fishStartDy = -0.1f;

	[SerializeField]
	private float _actionAdditionalTime = 0.2f;

	[SerializeField]
	private float _yToHideFish = 0.3f;

	[SerializeField]
	private string _outWaterSplashPrefabName = "2D/Splashes/pSplash_universal";

	[SerializeField]
	private string _outWaterSound = "Sounds/Actions/Lure/Lure_Out";

	[SerializeField]
	private float _outWaterSplashSize = 0.5f;

	[SerializeField]
	private string _inWaterSplashPrefabName = "2D/Splashes/pSplash_universal";

	[SerializeField]
	private string _inWaterSound = "Sounds/Actions/Lure/Lure_Splash_Big";

	[SerializeField]
	private float _inWaterSplashSize = 1f;

	[SerializeField]
	private float G = 9.81f;

	[SerializeField]
	private bool _debugSameRotation;

	private float _actionStartedAt = -1f;

	private Vector2 _startHorizontalSpeed;

	private float _startVerticalSpeed;

	private bool _isOutWaterEffectPlayed;

	private bool _isInWaterEffectPlayed;

	private float _tUpMovement;

	private float _vRotationSpeed;

	private float _curDirAngle;

	private float _curPitch;

	private float _curFixedPitch;
}
