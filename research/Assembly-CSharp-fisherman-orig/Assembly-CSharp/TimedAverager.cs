using System;

public class TimedAverager
{
	public TimedAverager(float timeDelta, int maxValues = 100)
	{
		this.timeDelta = timeDelta;
		this.MaxValues = maxValues;
		this.values = new float[this.MaxValues];
		this.timeStamps = new float[this.MaxValues];
	}

	public void Clear()
	{
		this.startIndex = 0;
		this.length = 0;
	}

	public float UpdateAndGet(float currentValue, float dt)
	{
		float num;
		try
		{
			if (dt > this.timeDelta)
			{
				this.Clear();
				num = currentValue;
			}
			else
			{
				this.Enqueue(currentValue, dt);
				this.Shrink();
				num = this.Average();
			}
		}
		catch (InvalidOperationException)
		{
			num = 0f;
		}
		return num;
	}

	private void Enqueue(float currentValue, float dt)
	{
		this.timeStampsTranslate(dt);
		if (this.length < this.MaxValues)
		{
			int num = this.startIndex + this.length;
			if (num > this.MaxValues - 1)
			{
				num -= this.MaxValues;
			}
			this.values[num] = currentValue;
			this.timeStamps[num] = dt;
			this.length++;
		}
		else
		{
			int num2 = this.startIndex + 1;
			if (num2 > this.MaxValues - 1)
			{
				num2 = 0;
			}
			this.values[this.startIndex] = currentValue;
			this.timeStamps[this.startIndex] = dt;
			this.startIndex = num2;
		}
	}

	private void Shrink()
	{
		if (this.length <= 1)
		{
			return;
		}
		int shrinkCount = this.getShrinkCount();
		this.startIndex += shrinkCount;
		if (this.startIndex > this.MaxValues - 1)
		{
			this.startIndex -= this.MaxValues;
		}
		if (shrinkCount < this.length)
		{
			this.length -= shrinkCount;
		}
		else
		{
			this.Clear();
		}
	}

	private float Average()
	{
		if (this.length == 0)
		{
			return 0f;
		}
		int i = 0;
		float num = 0f;
		int num2 = this.startIndex;
		while (i < this.length)
		{
			if (num2 > this.values.Length - 1)
			{
				num2 = 0;
			}
			num += this.values[num2];
			i++;
			num2++;
		}
		return num / (float)i;
	}

	private void timeStampsTranslate(float dt)
	{
		if (this.length == 0)
		{
			return;
		}
		int i = 0;
		int num = this.startIndex;
		while (i < this.length)
		{
			if (num > this.timeStamps.Length - 1)
			{
				num = 0;
			}
			this.timeStamps[num] += dt;
			i++;
			num++;
		}
	}

	private void TimeModifier(Func<float, float> func)
	{
		if (this.length == 0)
		{
			return;
		}
		int i = 0;
		int num = this.startIndex;
		while (i < this.length)
		{
			if (num > this.timeStamps.Length - 1)
			{
				num = 0;
			}
			this.timeStamps[num] = func(this.timeStamps[num]);
			i++;
			num++;
		}
	}

	private void TimeVisitor(Action<float> action)
	{
		if (this.length == 0)
		{
			return;
		}
		int i = 0;
		int num = this.startIndex;
		while (i < this.length)
		{
			if (num > this.timeStamps.Length - 1)
			{
				num = 0;
			}
			action(this.timeStamps[num]);
			i++;
			num++;
		}
	}

	private int getShrinkCount()
	{
		int num = 0;
		if (this.length == 0)
		{
			return num;
		}
		int i = 0;
		int num2 = this.startIndex;
		while (i < this.length)
		{
			if (num2 > this.timeStamps.Length - 1)
			{
				num2 = 0;
			}
			if (this.timeStamps[num2] > this.timeDelta)
			{
				num++;
			}
			i++;
			num2++;
		}
		return num;
	}

	private int MaxValues;

	private float[] values;

	private float[] timeStamps;

	private readonly float timeDelta;

	private int startIndex;

	private int length;
}
