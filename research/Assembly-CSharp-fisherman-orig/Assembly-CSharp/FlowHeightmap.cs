using System;
using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(FlowmapGenerator))]
public class FlowHeightmap : MonoBehaviour
{
	public virtual Texture HeightmapTexture { get; set; }

	public virtual Texture PreviewHeightmapTexture { get; set; }

	protected FlowmapGenerator Generator
	{
		get
		{
			if (!this.generator)
			{
				this.generator = base.GetComponent<FlowmapGenerator>();
			}
			return this.generator;
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		this.DisplayPreviewHeightmap(true);
		this.UpdatePreviewHeightmap();
	}

	public void DisplayPreviewHeightmap(bool state)
	{
		this.wantsToDrawHeightmap = state;
		this.UpdatePreviewHeightmap();
	}

	public void UpdatePreviewHeightmap()
	{
		if (this.previewGameObject == null || this.previewMaterial == null)
		{
			this.Cleanup();
			this.previewGameObject = new GameObject("Preview Heightmap");
			this.previewGameObject.hideFlags = 11;
			MeshFilter meshFilter = this.previewGameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = Primitives.PlaneMesh;
			MeshRenderer meshRenderer = this.previewGameObject.AddComponent<MeshRenderer>();
			this.previewMaterial = new Material(Shader.Find("Hidden/HeightmapFieldPreview"));
			this.previewMaterial.hideFlags = 61;
			meshRenderer.sharedMaterial = this.previewMaterial;
		}
		if (this.previewHeightmap && this.wantsToDrawHeightmap)
		{
			this.previewMaterial.SetTexture("_MainTex", this.PreviewHeightmapTexture);
			this.previewMaterial.SetFloat("_Strength", 1f);
			this.previewGameObject.GetComponent<Renderer>().enabled = true;
			this.previewGameObject.transform.position = base.transform.position;
			this.previewGameObject.transform.localScale = new Vector3(this.Generator.Dimensions.x, 1f, this.Generator.Dimensions.y);
		}
		else
		{
			this.previewGameObject.GetComponent<Renderer>().enabled = false;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		this.DisplayPreviewHeightmap(false);
		this.UpdatePreviewHeightmap();
	}

	protected virtual void OnDestroy()
	{
		this.Cleanup();
	}

	private void Cleanup()
	{
		if (this.previewGameObject)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.previewGameObject);
			}
			else
			{
				Object.DestroyImmediate(this.previewGameObject);
			}
		}
		if (this.previewMaterial)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.previewMaterial);
			}
			else
			{
				Object.DestroyImmediate(this.previewMaterial);
			}
		}
	}

	public bool previewHeightmap;

	public bool drawPreviewPlane;

	private bool wantsToDrawHeightmap;

	[SerializeField]
	private GameObject previewGameObject;

	[SerializeField]
	private Material previewMaterial;

	private FlowmapGenerator generator;
}
