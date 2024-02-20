using System;
using UnityEngine;

[Serializable]
public class DeluxeFilmicCurve
{
	public float GetExposure()
	{
		float highlights = this.m_Highlights;
		float num = 2f + (1f - highlights) * 20f;
		return num * Mathf.Exp(-2f);
	}

	public float ComputeK(float t, float c, float b, float s, float w)
	{
		float num = (1f - t) * (c - b);
		float num2 = (1f - s) * (w - c) + (1f - t) * (c - b);
		return num / num2;
	}

	public float Toe(float x, float t, float c, float b, float s, float w, float k)
	{
		float num = this.m_ToeCoef.x * x;
		float num2 = this.m_ToeCoef.y * x;
		return (num + this.m_ToeCoef.z) / (num2 + this.m_ToeCoef.w);
	}

	public float Shoulder(float x, float t, float c, float b, float s, float w, float k)
	{
		float num = this.m_ShoulderCoef.x * x;
		float num2 = this.m_ShoulderCoef.y * x;
		return (num + this.m_ShoulderCoef.z) / (num2 + this.m_ShoulderCoef.w) + k;
	}

	public float Graph(float x, float t, float c, float b, float s, float w, float k)
	{
		if (x <= this.m_CrossOverPoint)
		{
			return this.Toe(x, t, c, b, s, w, k);
		}
		return this.Shoulder(x, t, c, b, s, w, k);
	}

	public void StoreK()
	{
		this.m_k = this.ComputeK(this.m_ToeStrength, this.m_CrossOverPoint, this.m_BlackPoint, this.m_ShoulderStrength, this.m_WhitePoint);
	}

	public void ComputeShaderCoefficients(float t, float c, float b, float s, float w, float k)
	{
		float num = k * (1f - t);
		float num2 = k * (1f - t) * -b;
		float num3 = -t;
		float num4 = c - (1f - t) * b;
		this.m_ToeCoef = new Vector4(num, num3, num2, num4);
		float num5 = 1f - k;
		float num6 = (1f - k) * -c;
		float num7 = (1f - s) * w - c;
		this.m_ShoulderCoef = new Vector4(num5, s, num6, num7);
	}

	public void UpdateCoefficients()
	{
		this.StoreK();
		this.ComputeShaderCoefficients(this.m_ToeStrength, this.m_CrossOverPoint, this.m_BlackPoint, this.m_ShoulderStrength, this.m_WhitePoint, this.m_k);
	}

	[SerializeField]
	public float m_BlackPoint;

	[SerializeField]
	public float m_WhitePoint = 1f;

	[SerializeField]
	public float m_CrossOverPoint = 0.3f;

	[SerializeField]
	public float m_ToeStrength = 0.98f;

	[SerializeField]
	public float m_ShoulderStrength;

	[SerializeField]
	public float m_Highlights = 0.5f;

	public float m_k;

	public Vector4 m_ToeCoef;

	public Vector4 m_ShoulderCoef;
}
