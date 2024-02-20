using System;
using UnityEngine;

public class DirectionChanger
{
	public DirectionChanger(Vector3 v, Vector3 t, float s)
	{
		this.value = v;
		this.target = t;
		this.changeSpeed_ = s;
	}

	public float changeSpeed
	{
		get
		{
			return this.changeSpeed_;
		}
		set
		{
			this.changeSpeed_ = value;
		}
	}

	public Vector3 value
	{
		get
		{
			return this.composeDirection(this.phi_, this.theta_);
		}
		set
		{
			this.decomposeDirection(value, out this.phi_, out this.theta_);
		}
	}

	public Vector3 target
	{
		get
		{
			return this.composeDirection(this.phiTarget_, this.thetaTarget_);
		}
		set
		{
			this.decomposeDirection(value, out this.phiTarget_, out this.thetaTarget_);
		}
	}

	public void update(float dt)
	{
		if (this.phi_ == this.phiTarget_ && this.theta_ == this.thetaTarget_)
		{
			return;
		}
		float num = this.phiTarget_ - this.phi_;
		if (Mathf.Abs(num) > 3.1415927f)
		{
			num -= Mathf.Sign(num) * 3.1415927f * 2f;
		}
		float num2 = this.thetaTarget_ - this.theta_;
		float num3 = this.changeSpeed_;
		float num4 = this.changeSpeed_;
		if (Mathf.Abs(num) > Mathf.Abs(num2))
		{
			num4 *= Mathf.Abs(num2) / Mathf.Abs(num);
		}
		if (Mathf.Abs(num) < Mathf.Abs(num2))
		{
			num3 *= Mathf.Abs(num) / Mathf.Abs(num2);
		}
		float num5 = num4 * dt * Mathf.Sign(num2);
		if ((num5 > 0f && num5 > num2) || (num5 < 0f && num5 < num2))
		{
			num5 = num2;
		}
		this.theta_ += num5;
		float num6 = num3 * dt * Mathf.Sign(num);
		if ((num6 > 0f && num6 > num) || (num6 < 0f && num6 < num))
		{
			num6 = num;
		}
		this.phi_ += num6;
	}

	private void decomposeDirection(Vector3 d, out float p, out float t)
	{
		d.Normalize();
		t = Mathf.Asin(d.y);
		p = Mathf.Atan2(d.z, d.x);
	}

	private Vector3 composeDirection(float p, float t)
	{
		return new Vector3(Mathf.Cos(t) * Mathf.Cos(p), Mathf.Sin(t), Mathf.Cos(t) * Mathf.Sin(p));
	}

	private float phi_;

	private float theta_;

	private float phiTarget_;

	private float thetaTarget_;

	private float changeSpeed_;
}
