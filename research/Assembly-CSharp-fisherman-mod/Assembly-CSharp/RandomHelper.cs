using System;
using UnityEngine;

public class RandomHelper
{
	public static float GetNormMarsaglia(float sigma = 0.2f, float mu = 0f)
	{
		float num;
		float num3;
		do
		{
			num = Random.Range(-1f, 1f);
			float num2 = Random.Range(-1f, 1f);
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1f || num3 == 0f);
		num3 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
		return mu + num * num3 * sigma;
	}

	public static float GetMarsaglia(float max = 1f, float sigma = 0.2f, float mu = 0f)
	{
		float normMarsaglia;
		do
		{
			normMarsaglia = RandomHelper.GetNormMarsaglia(sigma, mu);
		}
		while (Mathf.Abs(normMarsaglia) > max);
		return normMarsaglia;
	}
}
