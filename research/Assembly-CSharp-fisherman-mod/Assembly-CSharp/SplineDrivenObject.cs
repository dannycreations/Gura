using System;
using SWS;
using UnityEngine;

public class SplineDrivenObject : MobileObject
{
	public override float MaxSpeed
	{
		get
		{
			return (this._pathIndex >= 0) ? this._pathes[this._pathIndex].Speed : 0f;
		}
	}

	public override void Freeze()
	{
		this._isFrozen = true;
		this._isAcceleration = false;
		this._changeSpeedFrom = Time.time;
	}

	public override void UnFreeze()
	{
		if (this._isFrozen)
		{
			this._isAcceleration = true;
			this._changeSpeedFrom = Time.time;
			this._isFrozen = false;
			this._engine.Resume();
		}
	}

	private void Awake()
	{
		this._engine = base.GetComponent<ISplineMover>();
	}

	private void LateUpdate()
	{
		if (this._isHorizontalMovementOnly)
		{
			base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
		}
	}

	private void Update()
	{
		if (base.LeadingObj != null && !this._isFrozen)
		{
			float magnitude = (base.LeadingObj.transform.position - base.transform.position).magnitude;
			if (this._isAcceleration)
			{
				if (magnitude < base.DistToLeaderToStop && this._changeSpeedFrom < 0f)
				{
					this._isAcceleration = false;
					this._changeSpeedFrom = Time.time;
				}
			}
			else if (magnitude > base.DistToLeaderToStart && this._changeSpeedFrom < 0f)
			{
				this._engine.Resume();
				this._isAcceleration = true;
				this._changeSpeedFrom = Time.time;
			}
		}
		if (this._changeSpeedFrom > 0f)
		{
			float num = Time.time - this._changeSpeedFrom;
			if (this._isAcceleration)
			{
				if (num > this._accelerationTime)
				{
					num = this._accelerationTime;
					this._changeSpeedFrom = -1f;
				}
				this._engine.ChangeSpeed(Mathf.SmoothStep(0f, this.MaxSpeed, num / this._accelerationTime));
			}
			else
			{
				if (num > this._slowDownTime)
				{
					num = this._slowDownTime;
					this._changeSpeedFrom = -1f;
					this._engine.Pause(0f);
				}
				this._engine.ChangeSpeed(Mathf.SmoothStep(this.MaxSpeed, 0f, num / this._slowDownTime));
			}
		}
		if (this._light != null)
		{
			this._time += Time.deltaTime;
			if (this._time > this._updateDatNightDelaySeconds)
			{
				this.CheckLight();
			}
		}
	}

	public override void Launch(float prc)
	{
		this.CheckLight();
		base.gameObject.SetActive(true);
		this._isAcceleration = true;
		this._pathIndex = Random.Range(0, this._pathes.Length);
		SplineDrivenObject.Path path = this._pathes[this._pathIndex];
		this._engine.SetPath(path.Spline, prc, path.Speed);
		this._engine.EEndOfSpline += this.OnEndOfSpline;
		base.Group = path.Spline.name;
	}

	private void OnEndOfSpline()
	{
		this._pathIndex = -1;
		this.EDisabled(this);
		base.gameObject.SetActive(false);
	}

	private void CheckLight()
	{
		this._time = 0f;
		bool flag = false;
		if (TimeAndWeatherManager.CurrentTime != null)
		{
			DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
			dateTime = dateTime.Add(TimeAndWeatherManager.CurrentTime.Value);
			flag = dateTime.Hour >= 21 || dateTime.Hour <= 4;
		}
		if (this._isNight != flag)
		{
			this._isNight = flag;
			if (this._light != null)
			{
				this._light.SetActive(this._isNight);
			}
		}
	}

	[SerializeField]
	private SplineDrivenObject.Path[] _pathes;

	[SerializeField]
	private float _accelerationTime = 2f;

	[SerializeField]
	private float _slowDownTime = 1f;

	[SerializeField]
	private GameObject _light;

	[SerializeField]
	private float _updateDatNightDelaySeconds = 15f;

	[SerializeField]
	private bool _isHorizontalMovementOnly = true;

	private bool _isNight;

	private float _time;

	private ISplineMover _engine;

	private int _pathIndex = -1;

	private bool _isFrozen;

	private float _changeSpeedFrom = -1f;

	private bool _isAcceleration;

	[Serializable]
	public class Path
	{
		public PathManager Spline;

		public float Speed;
	}
}
