using System;
using UnityEngine;

public class HistoryTransform
{
	public HistoryTransform(Transform transform, bool isRoot = false)
	{
		this.Transform = transform;
		this.IsRoot = isRoot;
	}

	public bool IsEntering { get; private set; }

	public bool HasCollidedWithWater
	{
		get
		{
			bool flag = this.Transform.position.y < 0f;
			bool flag2 = this.wasUnderWater != flag;
			if (flag2)
			{
				this.IsEntering = flag;
			}
			this.wasUnderWater = flag;
			return flag2;
		}
	}

	public readonly Transform Transform;

	private bool wasUnderWater = true;

	public bool IsRoot;
}
