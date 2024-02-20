using System;
using UnityEngine;

public class ValueChanger
{
	public ValueChanger(float v, float targetV, float changeSpeedUp, float? changeSpeedDown = null)
	{
		this.value_ = v;
		this.targetValue_ = targetV;
		this.changeSpeedUp_ = changeSpeedUp;
		if (changeSpeedDown == null)
		{
			this.changeSpeedDown_ = changeSpeedUp;
		}
		else
		{
			this.changeSpeedDown_ = changeSpeedDown.Value;
		}
	}

	public float changeSpeedUp
	{
		get
		{
			return this.changeSpeedUp_;
		}
		set
		{
			this.changeSpeedUp_ = value;
		}
	}

	public float changeSpeedDown
	{
		get
		{
			return this.changeSpeedDown_;
		}
		set
		{
			this.changeSpeedDown_ = value;
		}
	}

	public float changeSign
	{
		get
		{
			return Mathf.Sign(this.targetValue_ - this.value_);
		}
	}

	public float value
	{
		get
		{
			return this.value_;
		}
		set
		{
			this.value_ = value;
		}
	}

	public float target
	{
		get
		{
			return this.targetValue_;
		}
		set
		{
			this.targetValue_ = value;
		}
	}

	public void setTriggered(bool v)
	{
	}

	public void update(float dt)
	{
		if (this.targetValue_ != this.value_)
		{
			float num = this.targetValue_ - this.value_;
			float num2 = Mathf.Sign(num);
			float num3 = ((num2 <= 0f) ? this.changeSpeedDown_ : this.changeSpeedUp_) * dt * num2;
			this.value_ += num3;
			if ((num2 > 0f && this.value_ > this.targetValue_) || (num2 < 0f && this.value_ < this.targetValue_))
			{
				this.value_ = this.targetValue_;
			}
		}
	}

	private float value_;

	private float changeSpeedUp_;

	private float changeSpeedDown_;

	private float targetValue_;
}
