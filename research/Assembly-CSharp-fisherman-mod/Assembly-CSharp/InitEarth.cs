using System;
using UnityEngine;

public class InitEarth : MonoBehaviour
{
	private void Start()
	{
		this.m_sun = GameObject.FindGameObjectWithTag("Sun");
		float x = base.transform.localScale.x;
		this.m_innerRadius = x;
		this.m_outerRadius = 1.025f * x;
		this.InitMaterial(this.m_groundMaterial);
		this.InitMaterial(this.m_skyMaterial);
	}

	private void Update()
	{
		this.InitMaterial(this.m_groundMaterial);
		this.InitMaterial(this.m_skyMaterial);
	}

	private void InitMaterial(Material mat)
	{
		Vector3 vector;
		vector..ctor(1f / Mathf.Pow(this.m_atmoColor.x, 4f), 1f / Mathf.Pow(this.m_atmoColor.y, 4f), 1f / Mathf.Pow(this.m_atmoColor.z, 4f));
		float num = 1f / (this.m_outerRadius - this.m_innerRadius);
		mat.SetVector("v3LightPos", this.m_sun.transform.forward * -1f);
		mat.SetVector("v3InvWavelength", vector);
		mat.SetFloat("fOuterRadius", this.m_outerRadius);
		mat.SetFloat("fOuterRadius2", this.m_outerRadius * this.m_outerRadius);
		mat.SetFloat("fInnerRadius", this.m_innerRadius);
		mat.SetFloat("fInnerRadius2", this.m_innerRadius * this.m_innerRadius);
		mat.SetFloat("fKrESun", this.m_kr * this.m_ESun);
		mat.SetFloat("fKmESun", this.m_km * this.m_ESun);
		mat.SetFloat("fKr4PI", this.m_kr * 4f * 3.1415927f);
		mat.SetFloat("fKm4PI", this.m_km * 4f * 3.1415927f);
		mat.SetFloat("fScale", num);
		mat.SetFloat("fScaleDepth", this.m_scaleDepth);
		mat.SetFloat("fScaleOverScaleDepth", num / this.m_scaleDepth);
		mat.SetFloat("fHdrExposure", this.m_hdrExposure);
		mat.SetFloat("g", this.m_g);
		mat.SetFloat("g2", this.m_g * this.m_g);
		mat.SetVector("v3LightPos", this.m_sun.transform.forward * -1f);
		mat.SetVector("v3Translate", base.transform.localPosition);
	}

	public GameObject m_sun;

	public Material m_groundMaterial;

	public Material m_skyMaterial;

	public float m_hdrExposure = 0.6f;

	public Vector3 m_atmoColor = new Vector3(0.8f, 0.7f, 0.45f);

	public float m_ESun = 20f;

	public float m_kr = 0.0025f;

	public float m_km = 0.001f;

	public float m_g = -0.99f;

	private const float m_outerScaleFactor = 1.025f;

	private float m_innerRadius;

	private float m_outerRadius;

	private float m_scaleDepth = 0.25f;
}
