using System;
using System.Collections.Generic;
using System.Linq;

public class Averager
{
	public Averager(int count)
	{
		this.floats = new Queue<float>(count);
		this.count = count;
	}

	public float Current
	{
		get
		{
			return this.priorAverage;
		}
	}

	public void Clear()
	{
		this.floats.Clear();
		this.priorValue = 0f;
		this.priorAverage = 0f;
	}

	public float UpdateAndGet(float currentValue)
	{
		float num;
		try
		{
			if (this.priorValue == currentValue)
			{
				num = this.floats.Average();
			}
			else
			{
				this.priorValue = currentValue;
				this.floats.Enqueue(currentValue);
				if (this.floats.Count > this.count)
				{
					this.floats.Dequeue();
				}
				this.priorAverage = this.floats.Average();
				num = this.priorAverage;
			}
		}
		catch (InvalidOperationException)
		{
			num = 0f;
		}
		return num;
	}

	public float GetMax()
	{
		float num;
		try
		{
			num = this.floats.Max();
		}
		catch (InvalidOperationException)
		{
			num = 0f;
		}
		return num;
	}

	public float GetMin()
	{
		float num;
		try
		{
			num = this.floats.Min();
		}
		catch (InvalidOperationException)
		{
			num = 0f;
		}
		return num;
	}

	private readonly Queue<float> floats;

	private readonly int count;

	private float priorValue;

	private float priorAverage;
}
