using System;

public class QueueFloatFilter : QueueFilter<float>
{
	public QueueFloatFilter(int maxSize)
		: base(maxSize, (float v1, float v2) => v1 + v2, (float v1, float v2) => v1 - v2, (float v1, int v2) => v1 / (float)v2)
	{
	}
}
