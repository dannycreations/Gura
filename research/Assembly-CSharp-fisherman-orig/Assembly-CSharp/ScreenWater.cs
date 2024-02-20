using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/ScreenWater")]
public class ScreenWater : MonoBehaviour
{
	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (this.shaderRGB == null)
		{
			Debug.Log("Sat shaders are not set up! Disabling saturation effect.");
			base.enabled = false;
		}
		else if (!this.shaderRGB.isSupported)
		{
			base.enabled = false;
		}
	}

	protected Material material
	{
		get
		{
			if (this.m_MaterialRGB == null)
			{
				this.m_MaterialRGB = new Material(this.shaderRGB);
				this.m_MaterialRGB.hideFlags = 61;
			}
			return this.m_MaterialRGB;
		}
	}

	protected void OnDisable()
	{
		if (this.m_MaterialRGB)
		{
			Object.DestroyImmediate(this.m_MaterialRGB);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = this.material;
		material.SetTexture("_Splat", this.WaterFlows);
		material.SetTexture("_Flow", this.WaterMask);
		material.SetTexture("_Water", this.WetScreen);
		material.SetFloat("_Speed", this.Speed);
		material.SetFloat("_Intens", this.Intens);
		Graphics.Blit(source, destination, material);
	}

	public Shader shaderRGB;

	private Material m_MaterialRGB;

	public Texture2D WaterFlows;

	public Texture2D WaterMask;

	public Texture2D WetScreen;

	public float Speed = 0.5f;

	public float Intens = 0.5f;
}
