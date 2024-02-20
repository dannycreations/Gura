using System;
using UnityEngine;

public class TackleThrowData
{
	public float AccuracyValue
	{
		get
		{
			return (float)((double)(StaticUserData.RodInHand.Rod.Action / 40f) + 0.025) * this.CastLength * this.AccuracyRatio;
		}
	}

	public Vector3 Direction;

	public float CastLength;

	public float AccuracyRatio;

	public bool IsThrowing;

	public bool IsOvercasting;

	public float Windage;

	public float ThrowForce;

	public Vector3? Target;

	public float RodLength;

	public float MaxCastLength;
}
