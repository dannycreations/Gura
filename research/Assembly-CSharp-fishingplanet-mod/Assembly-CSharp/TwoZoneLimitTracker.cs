using System;

public class TwoZoneLimitTracker
{
	public TwoZoneLimitTracker(float lowLimit, float highLimit, float timeout)
	{
		this.lowLimit = lowLimit;
		this.highLimit = highLimit;
		this.underTheLowLimit = this.lowLimit * 0.999f;
		this.underTheHighLimit = this.highLimit * 0.999f;
		this.timeout = timeout;
		this.currentZone = TwoZoneLimitTracker.LimitZone.Low;
	}

	public float UpdateAndGet(float value, float dt)
	{
		if (this.priorDt == dt)
		{
			return this.priorValue;
		}
		this.priorDt = dt;
		TwoZoneLimitTracker.LimitZone limitZone = this.HitZone(value);
		if (this.currentZone == TwoZoneLimitTracker.LimitZone.Low && limitZone == TwoZoneLimitTracker.LimitZone.High)
		{
			this.currentZone = TwoZoneLimitTracker.LimitZone.Mid;
		}
		this.isOverLimit = value > this.CurrentLimit;
		if (this.isOverLimit && !this.wasOverLimit)
		{
			this.wasOverLimit = true;
			this.isOverLimitTime = 0f;
		}
		this.isOverLimitTime += dt;
		if (!this.isOverLimit && this.wasOverLimit)
		{
			this.currentZone = limitZone;
			this.wasOverLimit = false;
			this.isOverLimitTime = 0f;
		}
		if (this.isOverLimit && this.isOverLimitTime <= this.timeout)
		{
			this.priorValue = this.CurrentUnderLimit;
			return this.priorValue;
		}
		this.priorValue = value;
		return value;
	}

	private float CurrentLimit
	{
		get
		{
			if (this.currentZone == TwoZoneLimitTracker.LimitZone.Low)
			{
				return this.lowLimit;
			}
			return this.highLimit;
		}
	}

	private float CurrentUnderLimit
	{
		get
		{
			if (this.currentZone == TwoZoneLimitTracker.LimitZone.Low)
			{
				return this.underTheLowLimit;
			}
			return this.underTheHighLimit;
		}
	}

	private TwoZoneLimitTracker.LimitZone HitZone(float value)
	{
		if (value <= this.lowLimit)
		{
			return TwoZoneLimitTracker.LimitZone.Low;
		}
		if (value <= this.highLimit)
		{
			return TwoZoneLimitTracker.LimitZone.Mid;
		}
		return TwoZoneLimitTracker.LimitZone.High;
	}

	private const float UnderTheLimit = 0.999f;

	private readonly float lowLimit;

	private readonly float highLimit;

	private readonly float underTheLowLimit;

	private readonly float underTheHighLimit;

	private readonly float timeout;

	private bool isOverLimit;

	private bool wasOverLimit;

	private float isOverLimitTime;

	private float priorDt;

	private float priorValue;

	private TwoZoneLimitTracker.LimitZone currentZone;

	private enum LimitZone
	{
		Low,
		Mid,
		High
	}
}
