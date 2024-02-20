using System;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Heightmap/Texture")]
public class FlowTextureHeightmap : FlowHeightmap
{
	public override Texture HeightmapTexture
	{
		get
		{
			return this.heightmap;
		}
		set
		{
			this.heightmap = value as Texture2D;
		}
	}

	public override Texture PreviewHeightmapTexture
	{
		get
		{
			if (this.isRaw)
			{
				if (this.rawPreview == null)
				{
					this.GenerateRawPreview();
				}
				return this.rawPreview;
			}
			return this.HeightmapTexture;
		}
	}

	public void GenerateRawPreview()
	{
		if (this.rawPreview)
		{
			Object.DestroyImmediate(this.rawPreview);
		}
		if (this.heightmap)
		{
			this.rawPreview = TextureUtilities.GetRawPreviewTexture(this.heightmap);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.rawPreview)
		{
			Object.DestroyImmediate(this.rawPreview);
		}
	}

	[SerializeField]
	private Texture2D heightmap;

	public bool isRaw;

	private Texture2D rawPreview;
}
