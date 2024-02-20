using System;
using System.Linq;
using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(FlowmapGenerator))]
[AddComponentMenu("Flowmaps/Heightmap/Render From Scene")]
public class FlowRenderHeightmap : FlowHeightmap
{
	public static bool Supported
	{
		get
		{
			return SystemInfo.supportsRenderTextures;
		}
	}

	public static string UnsupportedReason
	{
		get
		{
			string text = string.Empty;
			if (!SystemInfo.supportsRenderTextures)
			{
				text = "System doesn't support RenderTextures.";
			}
			return text;
		}
	}

	public override Texture HeightmapTexture
	{
		get
		{
			return this.heightmap;
		}
		set
		{
			Debug.LogWarning("Can't set HeightmapTexture.");
		}
	}

	public override Texture PreviewHeightmapTexture
	{
		get
		{
			return this.HeightmapTexture;
		}
	}

	private Shader ClippedHeightShader
	{
		get
		{
			return Shader.Find("Hidden/DepthToHeightClipped");
		}
	}

	private Shader HeightShader
	{
		get
		{
			return Shader.Find("Hidden/DepthToHeight");
		}
	}

	private Shader WaterMapShader
	{
		get
		{
			return Shader.Find("Hidden/WaterMap");
		}
	}

	private Material CompareMaterial
	{
		get
		{
			if (!this.compareMaterial)
			{
				this.compareMaterial = new Material(Shader.Find("Hidden/DepthCompare"));
				this.compareMaterial.hideFlags = 61;
			}
			return this.compareMaterial;
		}
	}

	private Material ResizeMaterial
	{
		get
		{
			if (!this.resizeMaterial)
			{
				this.resizeMaterial = new Material(Shader.Find("Hidden/RenderHeightmapResize"));
				this.resizeMaterial.hideFlags = 61;
			}
			return this.resizeMaterial;
		}
	}

	public void UpdateHeightmap()
	{
		if (this.heightmap == null || this.heightmap.width != this.resolutionX || this.heightmap.height != this.resolutionY || (this.fluidDepth == FluidDepth.WaterMap && this.heightmap.format != null) || (this.fluidDepth != FluidDepth.WaterMap && this.heightmap.format == null))
		{
			Object.Destroy(this.heightmap);
			RenderTextureDescriptor renderTextureDescriptor = default(RenderTextureDescriptor);
			this.heightmap = ((this.fluidDepth != FluidDepth.WaterMap) ? new RenderTexture(this.resolutionX, this.resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, 1) : new RenderTexture(this.resolutionX, this.resolutionY, 0, 0, 1));
			this.heightmap.hideFlags = 61;
		}
		if (this.renderingCamera == null)
		{
			this.renderingCamera = Object.FindObjectsOfType<Camera>().FirstOrDefault((Camera x) => x.gameObject.name.Equals("Render Heightmap"));
		}
		if (this.renderingCamera == null)
		{
			this.renderingCamera = new GameObject("Render Heightmap", new Type[] { typeof(Camera) }).GetComponent<Camera>();
			this.renderingCamera.gameObject.hideFlags = 4;
		}
		if (this.renderingCameraTransform == null)
		{
			this.renderingCameraTransform = this.renderingCamera.transform;
		}
		this.renderingCamera.enabled = false;
		this.renderingCamera.renderingPath = 1;
		this.renderingCamera.clearFlags = 2;
		this.renderingCamera.backgroundColor = Color.black;
		this.renderingCamera.orthographic = true;
		this.renderingCamera.useOcclusionCulling = false;
		this.renderingCamera.allowHDR = false;
		this.renderingCamera.allowMSAA = false;
		if (this.fluidDepth == FluidDepth.WaterMap)
		{
			this.renderingCamera.depthTextureMode = 0;
		}
		IsobarRenderPostProcess component = this.renderingCamera.GetComponent<IsobarRenderPostProcess>();
		if (this.fluidDepth == FluidDepth.WaterMap && component == null)
		{
			this.renderingCamera.gameObject.AddComponent<IsobarRenderPostProcess>();
		}
		if (this.fluidDepth != FluidDepth.WaterMap && component != null)
		{
			Object.Destroy(component);
		}
		this.renderingCamera.cullingMask = this.cullingMask;
		this.renderingCamera.orthographicSize = Mathf.Max(base.Generator.Dimensions.x, base.Generator.Dimensions.y) * 0.5f;
		this.renderingCameraTransform.position = base.Generator.transform.position + Vector3.up * this.heightMax;
		this.renderingCameraTransform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
		this.renderingCamera.useOcclusionCulling = false;
		FluidDepth fluidDepth = this.fluidDepth;
		if (fluidDepth != FluidDepth.DeepWater)
		{
			if (fluidDepth != FluidDepth.Surface)
			{
				if (fluidDepth == FluidDepth.WaterMap)
				{
					float lowestBottomY = SurfaceSettings.LowestBottomY;
					Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - lowestBottomY + 1f);
					Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
					Shader.SetGlobalFloat("_DepthStep", this.depthStep);
					Shader.SetGlobalFloat("_Contrast", this.contrast);
					Shader.SetGlobalFloat("_Brightness", this.brightness);
					Shader.SetGlobalVector("_Rect", this.visibleRect);
					TextureWrapMode wrapMode = this.heightmap.wrapMode;
					FilterMode filterMode = this.heightmap.filterMode;
					int anisoLevel = this.heightmap.anisoLevel;
					this.heightmap.wrapMode = 1;
					this.heightmap.filterMode = 2;
					this.heightmap.anisoLevel = 4;
					this.renderingCameraTransform.position = base.Generator.transform.position - new Vector3(0f, 0.001f, 0f);
					this.renderingCamera.nearClipPlane = 0f;
					this.renderingCamera.farClipPlane = Mathf.Abs(lowestBottomY);
					this.renderingCamera.targetTexture = this.heightmap;
					float shadowDistance = QualitySettings.shadowDistance;
					QualitySettings.shadowDistance = 0f;
					this.renderingCamera.RenderWithShader(this.WaterMapShader, "RenderType");
					QualitySettings.shadowDistance = shadowDistance;
					this.heightmap.wrapMode = wrapMode;
					this.heightmap.filterMode = filterMode;
					this.heightmap.anisoLevel = anisoLevel;
				}
			}
			else
			{
				Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - this.heightMin);
				Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y + this.heightMax);
				this.renderingCamera.nearClipPlane = 0.001f;
				this.renderingCamera.farClipPlane = this.heightMin + this.heightMax;
				this.renderingCamera.targetTexture = this.heightmap;
				this.renderingCamera.RenderWithShader(this.HeightShader, "RenderType");
			}
		}
		else
		{
			RenderTexture temporary = RenderTexture.GetTemporary(this.resolutionX, this.resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, 2);
			RenderTexture temporary2 = RenderTexture.GetTemporary(this.resolutionX, this.resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, 2);
			RenderTexture temporary3 = RenderTexture.GetTemporary(this.resolutionX, this.resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, 2);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - this.heightMin);
			this.renderingCamera.targetTexture = temporary;
			this.renderingCamera.nearClipPlane = 0.01f;
			this.renderingCamera.farClipPlane = 100f;
			this.renderingCamera.RenderWithShader(this.ClippedHeightShader, "RenderType");
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - this.heightMin);
			this.renderingCamera.nearClipPlane = this.heightMax;
			this.renderingCamera.farClipPlane = this.heightMin + this.heightMax;
			this.renderingCamera.targetTexture = temporary2;
			this.renderingCamera.RenderWithShader(this.HeightShader, "RenderType");
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - this.heightMin);
			this.renderingCamera.nearClipPlane = 0.01f;
			this.renderingCamera.farClipPlane = this.heightMin + this.heightMax;
			this.renderingCamera.targetTexture = temporary3;
			this.renderingCamera.RenderWithShader(this.HeightShader, "RenderType");
			this.CompareMaterial.SetTexture("_OverhangMaskTex", temporary);
			this.CompareMaterial.SetTexture("_HeightBelowSurfaceTex", temporary2);
			this.CompareMaterial.SetTexture("_HeightIntersectingTex", temporary3);
			Graphics.Blit(null, this.heightmap, this.CompareMaterial);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
		}
		if (base.Generator.Dimensions.x != base.Generator.Dimensions.y)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(this.resolutionX, this.resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, 1);
			this.ResizeMaterial.SetTexture("_Heightmap", this.heightmap);
			if (base.Generator.Dimensions.y > base.Generator.Dimensions.x)
			{
				this.ResizeMaterial.SetVector("_AspectRatio", new Vector4(base.Generator.Dimensions.x / base.Generator.Dimensions.y, 1f, 0f, 0f));
			}
			else
			{
				this.ResizeMaterial.SetVector("_AspectRatio", new Vector4(1f, 1f / (base.Generator.Dimensions.x / base.Generator.Dimensions.y), 0f, 0f));
			}
			Graphics.Blit(null, temporary4, this.ResizeMaterial, 0);
			Graphics.Blit(temporary4, this.heightmap);
			RenderTexture.ReleaseTemporary(temporary4);
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * (this.heightMax - this.heightMin) / 2f, new Vector3(base.Generator.Dimensions.x, this.heightMax + this.heightMin, base.Generator.Dimensions.y));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public int resolutionX = 256;

	public int resolutionY = 256;

	public FluidDepth fluidDepth;

	public float heightMax = 1f;

	public float heightMin = 1f;

	public LayerMask cullingMask = 1;

	public bool dynamicUpdating;

	public float depthStep = 0.5f;

	public float contrast = 1f;

	public float brightness;

	public Vector4 visibleRect;

	private Camera renderingCamera;

	private Transform renderingCameraTransform;

	private RenderTexture heightmap;

	private Material compareMaterial;

	private Material resizeMaterial;
}
