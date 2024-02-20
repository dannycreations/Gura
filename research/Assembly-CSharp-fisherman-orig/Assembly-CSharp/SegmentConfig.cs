using System;

public class SegmentConfig
{
	public SegmentConfig(int id, float segmentLength, float relativeMass, float bendConstant, float frictionConstant)
	{
		this.Id = id;
		this.SegmentLength = segmentLength;
		this.RelativeMass = relativeMass;
		this.BendConstant = bendConstant;
		this.FrictionConstant = frictionConstant;
	}

	public readonly int Id;

	public readonly float SegmentLength;

	public readonly float RelativeMass;

	public readonly float BendConstant;

	public readonly float FrictionConstant;
}
