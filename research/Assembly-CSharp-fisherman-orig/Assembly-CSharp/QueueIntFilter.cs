using System;

public class QueueIntFilter : QueueFilter<int>
{
	public QueueIntFilter(int maxSize)
		: base(maxSize, (int v1, int v2) => v1 + v2, (int v1, int v2) => v1 - v2, (int v1, int v2) => v1 / v2)
	{
	}
}
