using System;
using UnityEngine;

[ExecuteInEditMode]
public class IsobarRenderPostProcess : MonoBehaviour
{
	private Shader IsobarLinesShader
	{
		get
		{
			return Shader.Find("Hidden/IsobarLines");
		}
	}

	private Material IsobarLinesMaterial
	{
		get
		{
			if (!this.isobarLinesMaterial)
			{
				this.isobarLinesMaterial = new Material(this.IsobarLinesShader);
				this.isobarLinesMaterial.hideFlags = 61;
			}
			return this.isobarLinesMaterial;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, this.IsobarLinesMaterial, 0);
	}

	private Material isobarLinesMaterial;
}
