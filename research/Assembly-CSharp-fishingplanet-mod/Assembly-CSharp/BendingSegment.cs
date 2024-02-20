using System;
using UnityEngine;

[Serializable]
public class BendingSegment
{
	public Transform firstTransform;

	public Transform lastTransform;

	public float curveTest = 3f;

	public float action = 0.3f;

	public float progressive = 1f;

	public float rodLength;

	public float weight = 0.1f;

	public float quiverLength;

	public float quiverMass;

	public float quiverTest = 1f;

	public float bellDistance;

	internal float AngleH;

	internal float AngleV;

	internal int ChainLength;

	internal Transform[] Nodes;

	internal float[] CumulativeLengths;
}
