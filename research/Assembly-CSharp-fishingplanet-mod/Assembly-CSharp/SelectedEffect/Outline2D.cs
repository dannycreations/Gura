using System;
using UnityEngine;

namespace SelectedEffect
{
	public class Outline2D : MonoBehaviour
	{
		private void UpdateOutlineShader()
		{
			if (this.m_Method != this.m_MethodPrev)
			{
				if (this.m_Method == Outline2D.EMethod.Image)
				{
					this.m_MatOutline = new Material(this.m_SdrImageOutline);
				}
				if (this.m_Method == Outline2D.EMethod.Sdf)
				{
					this.m_MatOutline = new Material(this.m_SdrSdfOutline);
				}
				this.m_MethodPrev = this.m_Method;
			}
		}

		private void Awake()
		{
			this.m_SprRdr = base.GetComponent<SpriteRenderer>();
			this.m_SdrImageOutline = Shader.Find("Selected Effect --- Outline/Sprite");
			this.m_SdrSdfOutline = Shader.Find("Selected Effect --- Outline/Sprite Sdf");
			this.m_MatOrig = this.m_SprRdr.material;
			if (this.m_Method == Outline2D.EMethod.Image)
			{
				this.m_MatOutline = new Material(this.m_SdrImageOutline);
			}
			if (this.m_Method == Outline2D.EMethod.Sdf)
			{
				this.m_MatOutline = new Material(this.m_SdrSdfOutline);
			}
			this.m_MethodPrev = this.m_Method;
		}

		private void OnDestroy()
		{
			if (this.m_MatOutline)
			{
				Object.DestroyImmediate(this.m_MatOutline);
				this.m_MatOutline = null;
			}
		}

		private void Update()
		{
			this.UpdateOutlineShader();
			this.m_SprRdr.material.SetColor("_OutlineColor", this.m_OutlineColor);
			if (this.m_Method == Outline2D.EMethod.Image)
			{
				this.m_SprRdr.material.SetFloat("_OutlineThickness", this.m_ImageOutline.Thickness);
				if (this.m_ImageOutline.OutlineOnly)
				{
					this.m_SprRdr.material.EnableKeyword("OUTLINE_ONLY");
				}
				else
				{
					this.m_SprRdr.material.DisableKeyword("OUTLINE_ONLY");
				}
				if (this.m_ImageOutline.Dash)
				{
					this.m_SprRdr.material.EnableKeyword("OUTLINE_DASH");
				}
				else
				{
					this.m_SprRdr.material.DisableKeyword("OUTLINE_DASH");
				}
			}
			else if (this.m_Method == Outline2D.EMethod.Sdf)
			{
				this.m_SprRdr.material.SetTexture("_SDFTex", this.m_SdfOutline.Sdf);
				this.m_SprRdr.material.SetFloat("_OutlineThickness", this.m_SdfOutline.Thickness);
				this.m_SprRdr.material.SetFloat("_OutlineOffset", this.m_SdfOutline.Offset);
				this.m_SprRdr.material.SetFloat("_OutlineEdgeSmoothness", this.m_SdfOutline.EdgeSmoothness);
				this.m_SprRdr.material.SetFloat("_OutlineSoftness", this.m_SdfOutline.Softness);
			}
		}

		private void OnMouseOver()
		{
			this.m_SprRdr.material = this.m_MatOutline;
		}

		private void OnMouseExit()
		{
			if (!this.m_Persistent)
			{
				this.m_SprRdr.material = this.m_MatOrig;
			}
		}

		public Outline2D.EMethod m_Method;

		private Outline2D.EMethod m_MethodPrev;

		public Color m_OutlineColor = Color.white;

		public bool m_Persistent = true;

		public Outline2D.ImageOutlineParameters m_ImageOutline;

		public Outline2D.SdfOutlineParameters m_SdfOutline;

		private Shader m_SdrImageOutline;

		private Shader m_SdrSdfOutline;

		private SpriteRenderer m_SprRdr;

		private Material m_MatOutline;

		private Material m_MatOrig;

		public enum EMethod
		{
			Image,
			Sdf
		}

		[Serializable]
		public struct ImageOutlineParameters
		{
			[Range(1f, 3f)]
			public float Thickness;

			public bool OutlineOnly;

			public bool Dash;
		}

		[Serializable]
		public struct SdfOutlineParameters
		{
			public Texture Sdf;

			public float Thickness;

			public float Softness;

			public float Offset;

			public float EdgeSmoothness;
		}
	}
}
