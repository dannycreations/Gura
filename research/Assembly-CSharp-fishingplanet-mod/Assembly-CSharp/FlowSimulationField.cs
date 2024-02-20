using System;
using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
public class FlowSimulationField : MonoBehaviour
{
	public virtual FieldPass Pass
	{
		get
		{
			return FieldPass.Force;
		}
	}

	protected virtual Shader RenderShader
	{
		get
		{
			return null;
		}
	}

	public Material FalloffMaterial
	{
		get
		{
			if (!this.falloffMaterial)
			{
				this.falloffMaterial = new Material(this.RenderShader);
				this.falloffMaterial.hideFlags = 61;
				this.falloffMaterial.name = "FlowFieldFalloff";
			}
			if (this.falloffMaterial.shader != this.RenderShader)
			{
				this.falloffMaterial.shader = this.RenderShader;
			}
			return this.falloffMaterial;
		}
	}

	public GpuRenderPlane RenderPlane
	{
		get
		{
			return this.renderPlane;
		}
	}

	protected void CreateMesh()
	{
		if (this.renderPlane && this.renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.renderPlane.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.renderPlane.gameObject);
			}
		}
		if (this == null)
		{
			return;
		}
		if (this.renderPlane == null)
		{
			this.renderPlane = new GameObject(base.name + " render plane")
			{
				hideFlags = 1,
				layer = FlowmapGenerator.GpuRenderLayer
			}.AddComponent<GpuRenderPlane>();
			this.renderPlane.field = this;
		}
		MeshFilter meshFilter = this.renderPlane.GetComponent<MeshFilter>();
		if (!meshFilter)
		{
			meshFilter = this.renderPlane.gameObject.AddComponent<MeshFilter>();
		}
		meshFilter.sharedMesh = Primitives.PlaneMesh;
		MeshRenderer meshRenderer = this.renderPlane.GetComponent<MeshRenderer>();
		if (!meshRenderer)
		{
			meshRenderer = this.renderPlane.gameObject.AddComponent<MeshRenderer>();
		}
		meshRenderer.material = this.FalloffMaterial;
		meshRenderer.enabled = false;
	}

	private void Awake()
	{
		this.Init();
	}

	protected virtual void Update()
	{
		if (!this.initialized)
		{
			this.Init();
		}
		if (Application.isPlaying)
		{
			this.UpdateRenderPlane();
		}
	}

	public void DisableRenderPlane()
	{
		if (this.renderPlane)
		{
			this.renderPlane.GetComponent<Renderer>().enabled = false;
		}
	}

	public void DrawFalloffTextureEnabled(bool state)
	{
		this.wantsToDrawPreviewTexture = state;
	}

	public virtual void UpdateRenderPlane()
	{
		if (this.renderPlane == null || this.renderPlane.field != this)
		{
			this.CreateMesh();
		}
		this.renderPlane.transform.position = base.transform.position;
		this.renderPlane.transform.localScale = base.transform.lossyScale;
		this.renderPlane.transform.rotation = base.transform.rotation;
		this.FalloffMaterial.SetTexture("_MainTex", this.falloffTexture);
		this.FalloffMaterial.SetFloat("_Strength", this.strength);
		this.renderPlane.GetComponent<Renderer>().enabled = FlowSimulationField.DrawFalloffTextures && (this.wantsToDrawPreviewTexture || FlowSimulationField.DrawFalloffUnselected) && base.enabled;
	}

	public virtual void Init()
	{
		if (this.initialized)
		{
			return;
		}
		this.cachedTransform = base.transform;
		this.CreateMesh();
		this.renderPlane.GetComponent<Renderer>().enabled = this.wantsToDrawPreviewTexture;
		this.cachedTransform = base.transform;
		this.cachedPosition = this.cachedTransform.position;
		this.cachedRotation = this.cachedTransform.rotation;
		this.cachedScale = this.cachedTransform.lossyScale;
		this.hasFalloffTexture = this.falloffTexture != null;
		if (this.falloffTexture)
		{
			this.falloffTextureDimensions = new Vector2((float)this.falloffTexture.width, (float)this.falloffTexture.height);
			this.falloffTexturePixels = this.falloffTexture.GetPixels();
		}
		else
		{
			this.falloffTextureDimensions = Vector2.zero;
		}
		this.initialized = true;
	}

	public virtual void TickStart()
	{
		if (!base.enabled)
		{
			return;
		}
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.GPU)
		{
			if (simulationPath == SimulationPath.CPU)
			{
				this.cachedTransform = base.transform;
				this.cachedPosition = this.cachedTransform.position;
				this.cachedRotation = this.cachedTransform.rotation;
				this.cachedScale = this.cachedTransform.lossyScale;
				this.hasFalloffTexture = this.falloffTexture != null;
				if (this.falloffTexture)
				{
					this.falloffTextureDimensions = new Vector2((float)this.falloffTexture.width, (float)this.falloffTexture.height);
					this.falloffTexturePixels = this.falloffTexture.GetPixels();
				}
				else
				{
					this.falloffTextureDimensions = Vector2.zero;
				}
			}
		}
		else
		{
			this.UpdateRenderPlane();
			this.FalloffMaterial.SetFloat("_Renderable", 1f);
			this.renderPlane.GetComponent<Renderer>().enabled = true;
		}
	}

	public virtual void TickEnd()
	{
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath == SimulationPath.GPU)
		{
			this.UpdateRenderPlane();
			this.FalloffMaterial.SetFloat("_Renderable", 0f);
		}
	}

	public Vector2 GetUvScale(FlowmapGenerator generator)
	{
		return new Vector2(this.cachedScale.x / generator.Dimensions.x, this.cachedScale.z / generator.Dimensions.y);
	}

	public Vector2 GetUvTransform(FlowmapGenerator generator)
	{
		return new Vector2((generator.Position.x - this.cachedPosition.x) / generator.Dimensions.x, (generator.Position.z - this.cachedPosition.z) / generator.Dimensions.y);
	}

	public float GetUvRotation(FlowmapGenerator generator)
	{
		return this.cachedRotation.eulerAngles.y * 0.017453292f;
	}

	public float GetStrengthCpu(FlowmapGenerator generator, Vector2 uv)
	{
		Vector2 vector = this.TransformSampleUv(generator, uv, false);
		float num = this.strength;
		if (vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
		{
			num = 0f;
		}
		if (FlowmapGenerator.ThreadCount > 1)
		{
			return num * ((!this.hasFalloffTexture) ? 1f : TextureUtilities.SampleColorBilinear(this.falloffTexturePixels, (int)this.falloffTextureDimensions.x, (int)this.falloffTextureDimensions.y, vector.x, vector.y).r);
		}
		return num * ((!this.hasFalloffTexture) ? 1f : this.falloffTexture.GetPixelBilinear(vector.x, vector.y).r);
	}

	protected Vector2 TransformSampleUv(FlowmapGenerator generator, Vector2 uv, bool invertY)
	{
		Vector2 vector = uv;
		vector..ctor(vector.x + this.GetUvTransform(generator).x, vector.y + this.GetUvTransform(generator).y);
		vector -= Vector2.one * 0.5f;
		vector..ctor(vector.x * Mathf.Cos(this.GetUvRotation(generator)) - vector.y * Mathf.Sin(this.GetUvRotation(generator)), vector.x * Mathf.Sin(this.GetUvRotation(generator)) + vector.y * Mathf.Cos(this.GetUvRotation(generator)));
		vector..ctor(vector.x / this.GetUvScale(generator).x * (float)((!invertY) ? 1 : (-1)), vector.y / this.GetUvScale(generator).y * (float)((!invertY) ? 1 : (-1)));
		vector += Vector2.one * 0.5f;
		return vector;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Vector3 vector = this.cachedTransform.position + this.cachedTransform.right * (-this.cachedTransform.lossyScale.x / 2f) + this.cachedTransform.forward * (-this.cachedTransform.lossyScale.z / 2f);
		Vector3 vector2 = this.cachedTransform.position + this.cachedTransform.right * (this.cachedTransform.lossyScale.x / 2f) + this.cachedTransform.forward * (-this.cachedTransform.lossyScale.z / 2f);
		Vector3 vector3 = this.cachedTransform.position + this.cachedTransform.right * (-this.cachedTransform.lossyScale.x / 2f) + this.cachedTransform.forward * (this.cachedTransform.lossyScale.z / 2f);
		Vector3 vector4 = this.cachedTransform.position + this.cachedTransform.right * (this.cachedTransform.lossyScale.x / 2f) + this.cachedTransform.forward * (this.cachedTransform.lossyScale.z / 2f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector, vector3);
		Gizmos.DrawLine(vector2, vector4);
		this.wantsToDrawPreviewTexture = true;
		this.UpdateRenderPlane();
	}

	protected virtual void OnDrawGizmos()
	{
		this.wantsToDrawPreviewTexture = false;
		this.UpdateRenderPlane();
	}

	private void OnDisable()
	{
		this.wantsToDrawPreviewTexture = false;
		if (this.renderPlane)
		{
			this.renderPlane.GetComponent<Renderer>().enabled = FlowSimulationField.DrawFalloffTextures && this.wantsToDrawPreviewTexture;
		}
	}

	private void OnDestroy()
	{
		this.Cleaup();
	}

	protected virtual void Cleaup()
	{
		if (this.renderPlane && this.renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.renderPlane.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.renderPlane.gameObject);
			}
		}
		if (this.falloffMaterial)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.falloffMaterial);
			}
			else
			{
				Object.DestroyImmediate(this.falloffMaterial);
			}
		}
	}

	public static bool DrawFalloffTextures = true;

	public static bool DrawFalloffUnselected;

	public float strength = 1f;

	public Texture2D falloffTexture;

	protected Transform cachedTransform;

	protected Vector3 cachedPosition;

	protected Quaternion cachedRotation;

	protected Vector3 cachedScale;

	protected Vector2 falloffTextureDimensions;

	protected Color[] falloffTexturePixels;

	private bool initialized;

	protected bool wantsToDrawPreviewTexture;

	protected bool hasFalloffTexture;

	private Material falloffMaterial;

	[SerializeField]
	[HideInInspector]
	protected GpuRenderPlane renderPlane;
}
