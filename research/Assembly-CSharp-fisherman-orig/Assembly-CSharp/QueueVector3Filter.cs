using System;
using UnityEngine;

public class QueueVector3Filter : QueueFilter<Vector3>
{
	public QueueVector3Filter(int maxSize)
		: base(maxSize, (Vector3 v1, Vector3 v2) => v1 + v2, (Vector3 v1, Vector3 v2) => v1 - v2, (Vector3 v1, int v2) => v1 / (float)v2)
	{
	}
}
