using System;

public class TrueTracker
{
	public TrueTracker(float timeout)
	{
		this.timeout = timeout;
	}

	public bool UpdateAndGet(bool value, float dt)
	{
		if (this.priorDt == dt)
		{
			return this.priorValue;
		}
		this.priorDt = dt;
		if (value && !this.wasTrue)
		{
			this.wasTrue = true;
			this.isTrueTime = 0f;
		}
		this.isTrueTime += dt;
		if (!value && this.wasTrue)
		{
			this.wasTrue = false;
			this.isTrueTime = 0f;
		}
		if (value && this.isTrueTime <= this.timeout)
		{
			this.priorValue = false;
			return false;
		}
		this.priorValue = value;
		return value;
	}

	private readonly float timeout;

	private bool wasTrue;

	private float isTrueTime;

	private float priorDt;

	private bool priorValue;
}
