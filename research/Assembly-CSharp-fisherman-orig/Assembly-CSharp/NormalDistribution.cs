using System;

public class NormalDistribution
{
	public static float GetNormMarsaglia(Random rnd, float sigma = 0.2f, float mu = 0f)
	{
		double num;
		double num3;
		do
		{
			num = rnd.NextDouble() * 2.0 - 1.0;
			double num2 = rnd.NextDouble() * 2.0 - 1.0;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1.0 || num3 == 0.0);
		num3 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
		return (float)((double)mu + num * num3 * (double)sigma);
	}

	public static float GetMarsaglia(Random rnd, float sigma = 0.2f, float mu = 0f, float max = 1f)
	{
		float normMarsaglia;
		do
		{
			normMarsaglia = NormalDistribution.GetNormMarsaglia(rnd, sigma, mu);
		}
		while (Math.Abs(normMarsaglia) > max);
		return normMarsaglia;
	}

	public static float GetAbsMarsaglia(Random rnd, float sigma = 0.2f, float mu = 0f, float max = 1f)
	{
		return Math.Abs(NormalDistribution.GetMarsaglia(rnd, sigma, mu, max));
	}
}
