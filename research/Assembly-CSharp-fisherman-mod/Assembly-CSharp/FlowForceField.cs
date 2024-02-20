using System;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Force")]
public class FlowForceField : FlowSimulationField
{
	public override FieldPass Pass
	{
		get
		{
			return FieldPass.Force;
		}
	}

	protected override Shader RenderShader
	{
		get
		{
			return Shader.Find("Hidden/ForceFieldPreview");
		}
	}

	public override void Init()
	{
		base.Init();
		this.UpdateVectorTexture();
	}

	protected override void Update()
	{
		base.Update();
	}

	public void UpdateVectorTexture()
	{
		int num = 64;
		int num2 = 64;
		this.vectorTexture = new Texture2D(num, num2, 5, false, true);
		this.vectorTexture.hideFlags = 61;
		this.vectorTexture.name = "VectorTexture";
		Color[] array = new Color[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector2 vector = Vector2.zero;
				float num3 = 1f - Mathf.Clamp01(vector.magnitude);
				Color white = Color.white;
				switch (this.force)
				{
				case FluidForce.Attract:
				case FluidForce.Repulse:
					vector..ctor(((float)j / (float)num - 0.5f) * 2f, ((float)i / (float)num2 - 0.5f) * 2f);
					vector = vector.normalized;
					vector..ctor(vector.x * 0.5f + 0.5f, vector.y * 0.5f + 0.5f);
					white..ctor(vector.x, vector.y, 0f, num3);
					break;
				case FluidForce.VortexCounterClockwise:
				case FluidForce.VortexClockwise:
				{
					vector..ctor(((float)j / (float)num - 0.5f) * 2f, ((float)i / (float)num2 - 0.5f) * 2f);
					vector = vector.normalized;
					Vector3 vector2 = Vector3.Cross(new Vector3(vector.x, 0f, vector.y), Vector3.down);
					vector..ctor(vector2.x * 0.5f + 0.5f, vector2.z * 0.5f + 0.5f);
					white..ctor(vector.x, vector.y, 0f, num3);
					break;
				}
				case FluidForce.Directional:
					vector = Vector2.one;
					white..ctor(vector.x, vector.y, 0f, num3);
					break;
				case FluidForce.Calm:
					white..ctor(0.5f, 0.5f, 1f, num3);
					break;
				}
				array[j + i * num] = white;
			}
		}
		this.vectorTexture.SetPixels(array);
		this.vectorTexture.Apply(false);
		this.vectorTexturePixels = this.vectorTexture.GetPixels();
		this.vectorTextureDimensions = new Vector2((float)this.vectorTexture.width, (float)this.vectorTexture.height);
	}

	public override void UpdateRenderPlane()
	{
		base.UpdateRenderPlane();
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath == SimulationPath.GPU)
		{
			base.FalloffMaterial.SetTexture("_VectorTex", this.vectorTexture);
			switch (this.force)
			{
			case FluidForce.Attract:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 1f);
				break;
			case FluidForce.Repulse:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			case FluidForce.VortexCounterClockwise:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 1f);
				break;
			case FluidForce.VortexClockwise:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			case FluidForce.Directional:
			{
				Vector2 vector;
				vector..ctor(base.transform.forward.x * 0.5f + 0.5f, base.transform.forward.z * 0.5f + 0.5f);
				base.FalloffMaterial.SetVector("_VectorScale", vector);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			}
			case FluidForce.Calm:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			}
		}
		if ((this.wantsToDrawPreviewTexture || FlowSimulationField.DrawFalloffUnselected) && base.enabled)
		{
			switch (this.force)
			{
			case FluidForce.Attract:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", this.attractVectorPreview);
				break;
			case FluidForce.Repulse:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", this.repulseVectorPreview);
				break;
			case FluidForce.VortexCounterClockwise:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", this.vortexCounterClockwiseVectorPreview);
				break;
			case FluidForce.VortexClockwise:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", this.vortexClockwiseVectorPreview);
				break;
			case FluidForce.Directional:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", this.directionalVectorPreview);
				break;
			}
		}
		else
		{
			base.FalloffMaterial.SetTexture("_VectorPreviewTex", null);
		}
	}

	public override void TickStart()
	{
		base.TickStart();
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath == SimulationPath.CPU)
		{
			if (this.vectorTexturePixels == null)
			{
				this.Init();
			}
			this.cachedForwardVector = base.transform.forward;
		}
	}

	public Vector3 GetForceCpu(FlowmapGenerator generator, Vector2 uv)
	{
		Vector2 vector = base.TransformSampleUv(generator, uv, this.force == FluidForce.Attract || this.force == FluidForce.VortexCounterClockwise);
		Color color = ((FlowmapGenerator.ThreadCount <= 1) ? this.vectorTexture.GetPixelBilinear(vector.x, vector.y) : TextureUtilities.SampleColorBilinear(this.vectorTexturePixels, (int)this.vectorTextureDimensions.x, (int)this.vectorTextureDimensions.y, vector.x, vector.y));
		if (this.force == FluidForce.Directional)
		{
			color..ctor(this.cachedForwardVector.x * 0.5f + 0.5f, this.cachedForwardVector.z * 0.5f + 0.5f, 0f, 0f);
		}
		Vector3 vector2;
		vector2..ctor(color.r * 2f - 1f, color.g * 2f - 1f, color.b);
		return this.strength * vector2 * (float)((vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f) ? 0 : 1) * ((!this.hasFalloffTexture) ? 1f : ((FlowmapGenerator.ThreadCount <= 1) ? this.falloffTexture.GetPixelBilinear(vector.x, vector.y).r : TextureUtilities.SampleColorBilinear(this.falloffTexturePixels, (int)this.falloffTextureDimensions.x, (int)this.falloffTextureDimensions.y, vector.x, vector.y).r));
	}

	protected override void Cleaup()
	{
		base.Cleaup();
		if (this.vectorTexture)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.vectorTexture);
			}
			else
			{
				Object.DestroyImmediate(this.vectorTexture);
			}
		}
	}

	public FluidForce force;

	[SerializeField]
	private Texture2D vectorTexture;

	private Vector2 vectorTextureDimensions;

	private Color[] vectorTexturePixels;

	private Vector3 cachedForwardVector;

	[HideInInspector]
	public Texture2D attractVectorPreview;

	[HideInInspector]
	public Texture2D repulseVectorPreview;

	[HideInInspector]
	public Texture2D vortexClockwiseVectorPreview;

	[HideInInspector]
	public Texture2D vortexCounterClockwiseVectorPreview;

	[HideInInspector]
	public Texture2D directionalVectorPreview;
}
