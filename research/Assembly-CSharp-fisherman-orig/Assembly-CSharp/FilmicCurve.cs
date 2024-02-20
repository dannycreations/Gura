using System;
using UnityEngine;

[Serializable]
public class FilmicCurve
{
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

	public void ComputeShaderCoefficientsLegacy(float t, float c, float b, float s, float w, float k)
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

	private float ToRange(float x)
	{
		return (x + 1f) * 0.5f;
	}

	public float GraphHD(float _lOll01ll00)
	{
		float num = Mathf.Log10(_lOll01ll00);
		float x = this.m_ToeCoef.x;
		float w = this.m_ToeCoef.w;
		float x2 = this.m_ShoulderCoef.x;
		float num2 = this.m_ToeCoef.y / (1f + Mathf.Exp(this.m_ToeCoef.z * (num - x)));
		float num3 = 1f - this.m_ShoulderCoef.y / (1f + Mathf.Exp(this.m_ShoulderCoef.z * (num - x2)));
		float num4 = this.m_ShoulderCoef.w * (num + w);
		float num5 = Mathf.Clamp01((num - x) / (x2 - x));
		float num6 = ((num >= x) ? num4 : num2);
		float num7 = ((num <= x2) ? num4 : num3);
		float num8;
		if (x > x2)
		{
			num8 = Mathf.Lerp(num7, num6, num5);
		}
		else
		{
			num8 = Mathf.Lerp(num6, num7, num5);
		}
		return num8;
	}

	public void ComputeShaderCoefficientsHD(float _001l0l01Ol, float _ll00Ol1O0O, float _ll00l0O1O1, float _OlOO100ll0, float _OOlO1l0ll0, float _OOO1O00O0l)
	{
		float num = (1f - this.ToRange(_001l0l01Ol)) / 1.25f;
		float num2 = this.ToRange(_OlOO100ll0);
		float num3 = Mathf.Clamp01(_ll00Ol1O0O * 0.5f + 0.5f);
		float num4 = 1f - num;
		float num5 = 1f - num2;
		float num6 = 0.18f;
		float num7 = -0.5f * Mathf.Log((1f + (num6 / num4 - 1f)) / (1f - (num6 / num4 - 1f))) * (num4 / num3) + Mathf.Log10(num6);
		float num8 = num4 / num3 - num7;
		float num9 = num2 / num3 - num8;
		float num10 = 2f * num4;
		float num11 = -2f * num3 / num4;
		float num12 = 2f * num5;
		float num13 = 2f * num3 / num5;
		this.m_ToeCoef = new Vector4(num7, num10, num11, num8);
		this.m_ShoulderCoef = new Vector4(num9, num12, num13, num3);
	}

	public void UpdateCoefficients()
	{
		if (this.m_UseLegacyCurve)
		{
			this.StoreK();
			this.ComputeShaderCoefficientsLegacy(this.m_ToeStrength, this.m_CrossOverPoint, this.m_BlackPoint, this.m_ShoulderStrength, this.m_WhitePoint, this.m_k);
			return;
		}
		this.ComputeShaderCoefficientsHD(this.m_ToeStrength, this.m_CrossOverPoint, this.m_BlackPoint, this.m_ShoulderStrength, this.m_WhitePoint, this.m_k);
	}

	[SerializeField]
	public float m_BlackPoint;

	[SerializeField]
	public float m_WhitePoint = 1f;

	[SerializeField]
	public float m_CrossOverPoint = 0.5f;

	[SerializeField]
	public float m_ToeStrength;

	[SerializeField]
	public float m_ShoulderStrength;

	[SerializeField]
	public float m_LuminositySaturationPoint = 0.95f;

	public bool m_UseLegacyCurve;

	public Vector4 m_ToeCoef;

	public Vector4 m_ShoulderCoef;

	public float m_k;
}
