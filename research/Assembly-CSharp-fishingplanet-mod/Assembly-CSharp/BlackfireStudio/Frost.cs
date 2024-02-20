using System;
using UnityEngine;

namespace BlackfireStudio
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Blackfire Studio/Frost")]
	public class Frost : MonoBehaviour
	{
		protected Material material
		{
			get
			{
				if (this.frostMaterial == null)
				{
					this.frostMaterial = new Material(this.shader);
					this.frostMaterial.hideFlags = 61;
				}
				return this.frostMaterial;
			}
		}

		private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
		{
			if (this.shader != null)
			{
				this.material.SetColor("_Color", this.color);
				this.material.SetFloat("_Transparency", this.transparency);
				this.material.SetFloat("_Refraction", this.refraction);
				this.material.SetFloat("_Coverage", this.coverage);
				this.material.SetFloat("_Smooth", this.smooth);
				if (this.diffuseTex != null)
				{
					this.material.SetTexture("_DiffuseTex", this.diffuseTex);
				}
				else
				{
					this.material.SetTexture("_DiffuseTex", null);
				}
				if (this.bumpTex != null)
				{
					this.material.SetTexture("_BumpTex", this.bumpTex);
				}
				else
				{
					this.material.SetTexture("_BumpTex", null);
				}
				if (this.coverageTex != null)
				{
					this.material.SetTexture("_CoverageTex", this.coverageTex);
				}
				else
				{
					this.material.SetTexture("_CoverageTex", null);
				}
				Graphics.Blit(sourceTexture, destTexture, this.material);
			}
			else
			{
				Graphics.Blit(sourceTexture, destTexture);
			}
		}

		public Shader shader;

		public Color color;

		public Texture2D diffuseTex;

		public Texture2D bumpTex;

		public Texture2D coverageTex;

		public float transparency;

		public float refraction;

		public float coverage;

		public float smooth;

		private Material frostMaterial;
	}
}
