using System;
using UnityEngine;

public class ValueGainTrigger
{
	public ValueGainTrigger(float minV, float maxV, float m_mass = 15f)
	{
		this._m_mass = m_mass;
		this.minValue = minV;
		this.maxValue = maxV;
		this.currentValue = minV;
	}

	public float Mass
	{
		get
		{
			return this._m_mass;
		}
		set
		{
			this._m_mass = value;
		}
	}

	public float Stiffness
	{
		get
		{
			return this._m_stiffness;
		}
		set
		{
			this._m_stiffness = value;
		}
	}

	public float Damping
	{
		get
		{
			return this._m_damping;
		}
		set
		{
			this._m_damping = value;
		}
	}

	public float Speed
	{
		get
		{
			return this._m_speed;
		}
		set
		{
			this._m_speed = value;
		}
	}

	public void update(float dt)
	{
		float num = dt;
		if (num > 0.05f)
		{
			num = 0.05f;
		}
		float num2 = ((!this.isTriggered) ? this.minValue : this.maxValue);
		if (this.currentValue > this.maxValue)
		{
			this.currentValue = this.maxValue;
			this.lastSpeed = 0f;
		}
		if (this.currentValue < this.minValue)
		{
			this.currentValue = this.minValue;
			this.lastSpeed = 0f;
		}
		float num3 = num2 - this.currentValue;
		float num4 = 0f;
		if (this.isTriggered)
		{
			num4 = this.dumpingForce.magnitude;
		}
		if (num4 > 100f)
		{
			num4 = 100f;
		}
		float num5 = this._m_damping * this.lastSpeed;
		float num6 = this._m_stiffness * Mathf.Sqrt(this._m_speed) * num3;
		if (this.isBack && !this.isTriggered)
		{
			num5 *= 1.6f;
			num6 *= 0.8f;
		}
		float num7 = num6 - num5 - num4 * 5f;
		float num8 = num7 / this._m_mass;
		this.lastSpeed += num8 * num * this._m_speed;
		this.currentValue += this.lastSpeed * num;
	}

	public void setTriggered(bool v)
	{
		this.isTriggered = v;
	}

	public float getValue()
	{
		return this.currentValue;
	}

	public bool isActive()
	{
		return this.isTriggered;
	}

	public float ValueRatio
	{
		get
		{
			return this.currentValue / this.maxValue;
		}
	}

	private float minValue;

	private float maxValue;

	private float currentValue;

	public bool isTriggered;

	public bool isBack;

	public bool slowBack;

	public Vector3 dumpingForce;

	private float lastSpeed;

	private float _m_mass = 15f;

	private float _m_stiffness = 1500f;

	private float _m_damping = 150f;

	private float _m_speed = 1f;
}
