using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace cakeslice
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class OutlineEffect : MonoBehaviour
	{
		private OutlineEffect()
		{
		}

		public static OutlineEffect Instance
		{
			get
			{
				if (object.Equals(OutlineEffect.m_instance, null))
				{
					return OutlineEffect.m_instance = Object.FindObjectOfType(typeof(OutlineEffect)) as OutlineEffect;
				}
				return OutlineEffect.m_instance;
			}
		}

		private Material GetMaterialFromID(int ID)
		{
			if (ID == 0)
			{
				return this.outline1Material;
			}
			if (ID == 1)
			{
				return this.outline2Material;
			}
			return this.outline3Material;
		}

		private Material CreateMaterial(Color emissionColor)
		{
			Material material = new Material(this.outlineBufferShader);
			material.SetColor("_Color", emissionColor);
			material.SetInt("_SrcBlend", 5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			return material;
		}

		private void Awake()
		{
			OutlineEffect.m_instance = this;
		}

		private void Start()
		{
			this.CreateMaterialsIfNeeded();
			this.UpdateMaterialsPublicProperties();
			if (this.sourceCamera == null)
			{
				this.sourceCamera = base.GetComponent<Camera>();
			}
			if (this.outlineCamera == null)
			{
				this.outlineCamera = new GameObject("Outline Camera")
				{
					transform = 
					{
						parent = this.sourceCamera.transform
					}
				}.AddComponent<Camera>();
				this.outlineCamera.enabled = false;
			}
			this.renderTexture = new RenderTexture(this.sourceCamera.pixelWidth, this.sourceCamera.pixelHeight, 16, 7);
			this.extraRenderTexture = new RenderTexture(this.sourceCamera.pixelWidth, this.sourceCamera.pixelHeight, 16, 7);
			this.UpdateOutlineCameraFromSource();
			this.commandBuffer = new CommandBuffer();
			this.outlineCamera.AddCommandBuffer(18, this.commandBuffer);
		}

		public void OnPreRender()
		{
			if (this.commandBuffer == null)
			{
				return;
			}
			this.CreateMaterialsIfNeeded();
			if (this.renderTexture.width != this.sourceCamera.pixelWidth || this.renderTexture.height != this.sourceCamera.pixelHeight)
			{
				this.renderTexture = new RenderTexture(this.sourceCamera.pixelWidth, this.sourceCamera.pixelHeight, 16, 7);
				this.extraRenderTexture = new RenderTexture(this.sourceCamera.pixelWidth, this.sourceCamera.pixelHeight, 16, 7);
				this.outlineCamera.targetTexture = this.renderTexture;
			}
			this.UpdateMaterialsPublicProperties();
			this.UpdateOutlineCameraFromSource();
			this.outlineCamera.targetTexture = this.renderTexture;
			this.commandBuffer.SetRenderTarget(this.renderTexture);
			this.commandBuffer.Clear();
			for (int i = 0; i < this._outlines.Count; i++)
			{
				Outline outline = this._outlines[i];
				LayerMask layerMask = this.sourceCamera.cullingMask;
				if (outline != null && layerMask == (layerMask | 1))
				{
					for (int j = 0; j < outline.Renderer.sharedMaterials.Length; j++)
					{
						Material material = null;
						if (outline.Renderer.sharedMaterials[j].mainTexture != null)
						{
							for (int k = 0; k < this.materialBuffer.Count; k++)
							{
								Material material2 = this.materialBuffer[k];
								if (material2.mainTexture == outline.Renderer.sharedMaterials[j].mainTexture && material2.color == this.GetMaterialFromID(outline.ColorIndex).color)
								{
									material = material2;
								}
							}
							if (material == null)
							{
								material = new Material(this.GetMaterialFromID(outline.ColorIndex));
								material.mainTexture = outline.Renderer.sharedMaterials[j].mainTexture;
								this.materialBuffer.Add(material);
							}
						}
						else
						{
							material = this.GetMaterialFromID(outline.ColorIndex);
						}
						if (this.backfaceCulling)
						{
							material.SetInt("_Culling", 2);
						}
						else
						{
							material.SetInt("_Culling", 0);
						}
						this.commandBuffer.DrawRenderer(outline.Renderer, material, 0, 0);
						if (outline.MF != null)
						{
							for (int l = 1; l < outline.MF.sharedMesh.subMeshCount; l++)
							{
								this.commandBuffer.DrawRenderer(outline.Renderer, material, l, 0);
							}
						}
						if (outline.SMR != null)
						{
							for (int m = 1; m < outline.SMR.sharedMesh.subMeshCount; m++)
							{
								this.commandBuffer.DrawRenderer(outline.Renderer, material, m, 0);
							}
						}
					}
				}
			}
			this.outlineCamera.Render();
		}

		private void OnDestroy()
		{
			if (this.renderTexture != null)
			{
				this.renderTexture.Release();
			}
			if (this.extraRenderTexture != null)
			{
				this.extraRenderTexture.Release();
			}
			this.DestroyMaterials();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			this.outlineShaderMaterial.SetTexture("_OutlineSource", this.renderTexture);
			if (this.addLinesBetweenColors)
			{
				Graphics.Blit(source, this.extraRenderTexture, this.outlineShaderMaterial, 0);
				this.outlineShaderMaterial.SetTexture("_OutlineSource", this.extraRenderTexture);
			}
			Graphics.Blit(source, destination, this.outlineShaderMaterial, 1);
		}

		private void CreateMaterialsIfNeeded()
		{
			if (this.outlineShader == null)
			{
				this.outlineShader = Resources.Load<Shader>("OutlineShader");
			}
			if (this.outlineBufferShader == null)
			{
				this.outlineBufferShader = Resources.Load<Shader>("OutlineBufferShader");
			}
			if (this.outlineShaderMaterial == null)
			{
				this.outlineShaderMaterial = new Material(this.outlineShader);
				this.outlineShaderMaterial.hideFlags = 61;
				this.UpdateMaterialsPublicProperties();
			}
			if (this.outlineEraseMaterial == null)
			{
				this.outlineEraseMaterial = this.CreateMaterial(new Color(0f, 0f, 0f, 0f));
			}
			if (this.outline1Material == null)
			{
				this.outline1Material = this.CreateMaterial(new Color(1f, 0f, 0f, 0f));
			}
			if (this.outline2Material == null)
			{
				this.outline2Material = this.CreateMaterial(new Color(0f, 1f, 0f, 0f));
			}
			if (this.outline3Material == null)
			{
				this.outline3Material = this.CreateMaterial(new Color(0f, 0f, 1f, 0f));
			}
		}

		private void DestroyMaterials()
		{
			for (int i = 0; i < this.materialBuffer.Count; i++)
			{
				Object.DestroyImmediate(this.materialBuffer[i]);
			}
			this.materialBuffer.Clear();
			Object.DestroyImmediate(this.outlineShaderMaterial);
			Object.DestroyImmediate(this.outlineEraseMaterial);
			Object.DestroyImmediate(this.outline1Material);
			Object.DestroyImmediate(this.outline2Material);
			Object.DestroyImmediate(this.outline3Material);
			this.outlineShader = null;
			this.outlineBufferShader = null;
			this.outlineShaderMaterial = null;
			this.outlineEraseMaterial = null;
			this.outline1Material = null;
			this.outline2Material = null;
			this.outline3Material = null;
		}

		public void UpdateMaterialsPublicProperties()
		{
			if (this.outlineShaderMaterial)
			{
				float num = 1f;
				if (this.scaleWithScreenSize)
				{
					num = (float)Screen.height / 360f;
				}
				if (this.scaleWithScreenSize && num < 1f)
				{
					this.outlineShaderMaterial.SetFloat("_LineThicknessX", 0.001f * (1f / (float)Screen.width) * 1000f);
					this.outlineShaderMaterial.SetFloat("_LineThicknessY", 0.001f * (1f / (float)Screen.height) * 1000f);
				}
				else
				{
					this.outlineShaderMaterial.SetFloat("_LineThicknessX", num * (this.lineThickness / 1000f) * (1f / (float)Screen.width) * 1000f);
					this.outlineShaderMaterial.SetFloat("_LineThicknessY", num * (this.lineThickness / 1000f) * (1f / (float)Screen.height) * 1000f);
				}
				this.outlineShaderMaterial.SetFloat("_LineIntensity", this.lineIntensity);
				this.outlineShaderMaterial.SetFloat("_FillAmount", this.fillAmount);
				this.outlineShaderMaterial.SetColor("_LineColor1", this.lineColor0 * this.lineColor0);
				this.outlineShaderMaterial.SetColor("_LineColor2", this.lineColor1 * this.lineColor1);
				this.outlineShaderMaterial.SetColor("_LineColor3", this.lineColor2 * this.lineColor2);
				if (this.flipY)
				{
					this.outlineShaderMaterial.SetInt("_FlipY", 1);
				}
				else
				{
					this.outlineShaderMaterial.SetInt("_FlipY", 0);
				}
				if (!this.additiveRendering)
				{
					this.outlineShaderMaterial.SetInt("_Dark", 1);
				}
				else
				{
					this.outlineShaderMaterial.SetInt("_Dark", 0);
				}
				if (this.cornerOutlines)
				{
					this.outlineShaderMaterial.SetInt("_CornerOutlines", 1);
				}
				else
				{
					this.outlineShaderMaterial.SetInt("_CornerOutlines", 0);
				}
				Shader.SetGlobalFloat("_OutlineAlphaCutoff", this.alphaCutoff);
			}
		}

		private void UpdateOutlineCameraFromSource()
		{
			this.outlineCamera.CopyFrom(this.sourceCamera);
			this.outlineCamera.renderingPath = 1;
			this.outlineCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			this.outlineCamera.clearFlags = 2;
			this.outlineCamera.rect = new Rect(0f, 0f, 1f, 1f);
			this.outlineCamera.cullingMask = 0;
			this.outlineCamera.targetTexture = this.renderTexture;
			this.outlineCamera.enabled = false;
			this.outlineCamera.allowHDR = false;
		}

		public void AddOutline(Outline outline)
		{
			if (!this._outlines.Contains(outline))
			{
				this._outlines.Add(outline);
			}
		}

		public void RemoveOutline(Outline outline)
		{
			this._outlines.Remove(outline);
		}

		private static OutlineEffect m_instance;

		private readonly List<Outline> _outlines = new List<Outline>();

		[Range(1f, 6f)]
		public float lineThickness = 1.25f;

		[Range(0f, 10f)]
		public float lineIntensity = 0.5f;

		[Range(0f, 1f)]
		public float fillAmount = 0.2f;

		public Color lineColor0 = Color.red;

		public Color lineColor1 = Color.green;

		public Color lineColor2 = Color.blue;

		public bool additiveRendering;

		public bool backfaceCulling = true;

		[Header("These settings can affect performance!")]
		public bool cornerOutlines;

		public bool addLinesBetweenColors;

		[Header("Advanced settings")]
		public bool scaleWithScreenSize = true;

		[Range(0.1f, 0.9f)]
		public float alphaCutoff = 0.5f;

		public bool flipY;

		public Camera sourceCamera;

		[HideInInspector]
		public Camera outlineCamera;

		private Material outline1Material;

		private Material outline2Material;

		private Material outline3Material;

		private Material outlineEraseMaterial;

		private Shader outlineShader;

		private Shader outlineBufferShader;

		[HideInInspector]
		public Material outlineShaderMaterial;

		[HideInInspector]
		public RenderTexture renderTexture;

		[HideInInspector]
		public RenderTexture extraRenderTexture;

		private CommandBuffer commandBuffer;

		private List<Material> materialBuffer = new List<Material>();
	}
}
