using System;

public class RodSegmentConfig
{
	public RodSegmentConfig(SegmentConfig[] config, float curveTest)
	{
		this.Config = config;
		this.CurveTest = curveTest;
	}

	public float GetNormalizeLengthFactor()
	{
		float num = 0f;
		for (int i = 0; i < this.Config.Length; i++)
		{
			num += this.Config[i].SegmentLength;
		}
		if (num > 0f)
		{
			return 1f / num;
		}
		return 1f;
	}

	public float GetNormalizeMassFactor()
	{
		float num = 0f;
		for (int i = 0; i < this.Config.Length; i++)
		{
			num += this.Config[i].RelativeMass;
		}
		if (num > 0f)
		{
			return 1f / num;
		}
		return 1f;
	}

	public readonly SegmentConfig[] Config;

	public readonly float CurveTest;

	public static RodSegmentConfig Fast = new RodSegmentConfig(new SegmentConfig[]
	{
		new SegmentConfig(0, 0f, 10f, 0f, 5f),
		new SegmentConfig(1, 0.35f, 8f, 200f, 5f),
		new SegmentConfig(2, 0.35f, 5f, 100f, 5f),
		new SegmentConfig(3, 0.1f, 2.5f, 80f, 5f),
		new SegmentConfig(4, 0.1f, 1.5f, 40f, 5f),
		new SegmentConfig(5, 0.1f, 0.5f, 15f, 5f)
	}, 0.4f);

	public static RodSegmentConfig Moderate = new RodSegmentConfig(new SegmentConfig[]
	{
		new SegmentConfig(0, 0f, 10f, 0f, 5f),
		new SegmentConfig(1, 0.25f, 8f, 200f, 5f),
		new SegmentConfig(2, 0.25f, 5f, 90f, 5f),
		new SegmentConfig(3, 0.18f, 2.5f, 70f, 5f),
		new SegmentConfig(4, 0.15f, 1.5f, 50f, 5f),
		new SegmentConfig(5, 0.15f, 0.5f, 10f, 5f)
	}, 0.9f);

	public static RodSegmentConfig Slow = new RodSegmentConfig(new SegmentConfig[]
	{
		new SegmentConfig(0, 0f, 10f, 0f, 3f),
		new SegmentConfig(1, 0.2f, 9f, 500f, 3f),
		new SegmentConfig(2, 0.2f, 7f, 300f, 3f),
		new SegmentConfig(3, 0.2f, 5f, 250f, 3f),
		new SegmentConfig(4, 0.2f, 4f, 200f, 3f),
		new SegmentConfig(5, 0.2f, 2f, 150f, 3f)
	}, 4.4f);

	public static RodSegmentConfig Quiver = new RodSegmentConfig(new SegmentConfig[]
	{
		new SegmentConfig(6, 1f, 0.7f, 8f, 2f),
		new SegmentConfig(7, 1f, 0.6f, 6f, 1f),
		new SegmentConfig(8, 1f, 0.5f, 3f, 0.1f),
		new SegmentConfig(9, 1f, 0.4f, 2f, 0.05f),
		new SegmentConfig(10, 1f, 0.1f, 1f, 0.02f)
	}, 0.005f);
}
