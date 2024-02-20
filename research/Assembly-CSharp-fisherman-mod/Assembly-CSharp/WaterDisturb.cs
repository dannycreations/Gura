using System;
using UnityEngine;

public class WaterDisturb : QueuePool<WaterDisturb>.IItem
{
	public void Clone(WaterDisturb obj)
	{
		this.GlobalPosition = obj.GlobalPosition;
		this.Radius = obj.Radius;
		this.Force = obj.Force;
	}

	public Vector3 GlobalPosition;

	public float Radius;

	public float Force;
}
