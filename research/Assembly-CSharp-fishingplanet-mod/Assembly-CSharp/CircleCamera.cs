using System;
using UnityEngine;

public class CircleCamera : RoutingBase
{
	public override Vector3 Position
	{
		get
		{
			return base.transform.TransformPoint(this._a * Mathf.Cos(this._curAngle), 0f, this._b * Mathf.Sin(this._curAngle));
		}
	}

	public override Quaternion Rotation
	{
		get
		{
			return Quaternion.LookRotation(base.transform.position + new Vector3(0f, this._dh, 0f) - this.Position);
		}
	}

	public bool IsActive
	{
		get
		{
			return this._isActive;
		}
	}

	private float Dt
	{
		get
		{
			return Time.deltaTime;
		}
	}

	public override void Play()
	{
		this._curAngle = this._alpha0 * 0.017453292f;
		this.Continue();
	}

	public void Continue()
	{
		this._isActive = true;
	}

	public override void Stop()
	{
		this._isActive = false;
	}

	public void SetCurrentAngleAsStart()
	{
		this._alpha0 = this._curAngle * 57.29578f;
	}

	private void Update()
	{
		if (this._isActive)
		{
			this._curAngle += this.Dt * this._w * 0.017453292f;
			if (Mathf.Abs(this._curAngle) > 6.2831855f)
			{
				this._curAngle -= (float)(((this._w <= 0f) ? (-1) : 1) * 2) * 3.1415927f;
			}
		}
	}

	[Tooltip("Ellipse radius 1")]
	[SerializeField]
	private float _a;

	[Tooltip("Ellipse radius 2")]
	[SerializeField]
	private float _b;

	[Tooltip("Set look at point higher (>0) or lower(<0) from current transform height")]
	[SerializeField]
	private float _dh = -0.5f;

	[Tooltip("Initial position")]
	[SerializeField]
	private float _alpha0;

	[Tooltip("Radial speed")]
	[SerializeField]
	private float _w;

	private float _curAngle;

	private bool _isActive;

	private double _lastUpdateTime;
}
