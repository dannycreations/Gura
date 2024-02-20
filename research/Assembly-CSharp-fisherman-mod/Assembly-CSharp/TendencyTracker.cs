using System;
using System.Collections.Generic;
using System.Linq;

public class TendencyTracker
{
	public TendencyTracker(int count, float minAmplitude)
	{
		this.velocities = new Queue<float>(count);
		this.depths = new Queue<float>(count);
		this.count = count;
		this.minTendencyAmplitude = minAmplitude;
	}

	public bool UpdateAndGet(float currentValue, float currentDepth)
	{
		try
		{
			if (this.priorValue == currentValue)
			{
				return this.isCurrentlyUp;
			}
			this.priorValue = currentValue;
			this.velocities.Enqueue(currentValue);
			if (this.velocities.Count > this.count)
			{
				this.velocities.Dequeue();
			}
			this.depths.Enqueue(currentDepth);
			if (this.depths.Count > this.count)
			{
				this.depths.Dequeue();
			}
			if (!this.isCurrentlyUp && !this.isTendencyChangingUp)
			{
				if (this.velocities.All((float f) => f > 0f))
				{
					this.isTendencyChangingUp = true;
					this.minValue = this.depths.Min();
				}
			}
			if (!this.isCurrentlyUp && this.isTendencyChangingUp && currentDepth - this.minValue > this.minTendencyAmplitude)
			{
				this.isCurrentlyUp = true;
				this.isTendencyChangingUp = false;
				this.minValue = 0f;
			}
			if (!this.isCurrentlyUp && this.isTendencyChangingUp)
			{
				if (this.velocities.Any((float f) => f < 0f))
				{
					this.isTendencyChangingUp = false;
				}
			}
			if (this.isCurrentlyUp && !this.isTendencyChangingDown)
			{
				if (this.velocities.All((float f) => f < 0f))
				{
					this.isTendencyChangingDown = true;
					this.maxValue = this.depths.Max();
				}
			}
			if (this.isCurrentlyUp && this.isTendencyChangingDown && this.maxValue - currentDepth > this.minTendencyAmplitude)
			{
				this.isCurrentlyUp = false;
				this.isTendencyChangingDown = false;
				this.maxValue = 0f;
			}
			if (this.isCurrentlyUp && this.isTendencyChangingDown)
			{
				if (this.velocities.Any((float f) => f > 0f))
				{
					this.isTendencyChangingDown = false;
				}
			}
		}
		catch (InvalidOperationException)
		{
		}
		return this.isCurrentlyUp;
	}

	private readonly Queue<float> velocities;

	private readonly Queue<float> depths;

	private readonly int count;

	private float priorValue;

	private bool isCurrentlyUp;

	private bool isTendencyChangingUp;

	private bool isTendencyChangingDown;

	private readonly float minTendencyAmplitude;

	private float minValue;

	private float maxValue;
}
