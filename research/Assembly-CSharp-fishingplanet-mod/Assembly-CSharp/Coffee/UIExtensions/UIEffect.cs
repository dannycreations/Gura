using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIEffect : UIEffectBase
	{
		[Obsolete("Use targetGraphic instead (UnityUpgradable) -> targetGraphic")]
		public Graphic graphic
		{
			get
			{
				return base.graphic;
			}
		}

		public float toneLevel
		{
			get
			{
				return this.m_ToneLevel;
			}
			set
			{
				this.m_ToneLevel = Mathf.Clamp(value, 0f, 1f);
				base.SetDirty();
			}
		}

		public float blur
		{
			get
			{
				return this.m_Blur;
			}
			set
			{
				this.m_Blur = Mathf.Clamp(value, 0f, 1f);
				base.SetDirty();
			}
		}

		public float shadowBlur
		{
			get
			{
				return this.m_ShadowBlur;
			}
			set
			{
				this.m_ShadowBlur = Mathf.Clamp(value, 0f, 1f);
				base.SetDirty();
			}
		}

		public UIEffect.ShadowStyle shadowStyle
		{
			get
			{
				return this.m_ShadowStyle;
			}
			set
			{
				this.m_ShadowStyle = value;
				base.SetDirty();
			}
		}

		public UIEffect.ToneMode toneMode
		{
			get
			{
				return this.m_ToneMode;
			}
		}

		public UIEffect.ColorMode colorMode
		{
			get
			{
				return this.m_ColorMode;
			}
		}

		public UIEffect.BlurMode blurMode
		{
			get
			{
				return this.m_BlurMode;
			}
		}

		public Color shadowColor
		{
			get
			{
				return this.m_ShadowColor;
			}
			set
			{
				this.m_ShadowColor = value;
				base.SetDirty();
			}
		}

		public Vector2 effectDistance
		{
			get
			{
				return this.m_EffectDistance;
			}
			set
			{
				this.m_EffectDistance = value;
				base.SetDirty();
			}
		}

		public bool useGraphicAlpha
		{
			get
			{
				return this.m_UseGraphicAlpha;
			}
			set
			{
				this.m_UseGraphicAlpha = value;
				base.SetDirty();
			}
		}

		public Color effectColor
		{
			get
			{
				return this.m_EffectColor;
			}
			set
			{
				this.m_EffectColor = value;
				base.SetDirty();
			}
		}

		public List<UIEffect.AdditionalShadow> additionalShadows
		{
			get
			{
				return this.m_AdditionalShadows;
			}
		}

		public Vector4 customFactor
		{
			get
			{
				return this.m_CustomFactor;
			}
			set
			{
				this.m_CustomFactor = value;
				base.SetDirty();
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			vh.GetUIVertexStream(UIEffectBase.tempVerts);
			Vector2 vector;
			vector..ctor((!this.m_CustomEffect) ? Packer.ToFloat(this.toneLevel, 0f, this.blur, 0f) : Packer.ToFloat(this.m_CustomFactor), Packer.ToFloat(this.effectColor.r, this.effectColor.g, this.effectColor.b, this.effectColor.a));
			for (int i = 0; i < UIEffectBase.tempVerts.Count; i++)
			{
				UIVertex uivertex = UIEffectBase.tempVerts[i];
				uivertex.uv1 = vector;
				UIEffectBase.tempVerts[i] = uivertex;
			}
			int count = UIEffectBase.tempVerts.Count;
			int num = 0;
			int num2 = count;
			int num3 = this.additionalShadows.Count - 1;
			while (0 <= num3)
			{
				UIEffect.AdditionalShadow additionalShadow = this.additionalShadows[num3];
				this._ApplyShadow(UIEffectBase.tempVerts, ref num, ref num2, additionalShadow.shadowMode, this.toneLevel, additionalShadow.shadowBlur, additionalShadow.effectDistance, additionalShadow.shadowColor, additionalShadow.useGraphicAlpha);
				num3--;
			}
			this._ApplyShadow(UIEffectBase.tempVerts, ref num, ref num2, this.shadowStyle, this.toneLevel, this.shadowBlur, this.effectDistance, this.shadowColor, this.useGraphicAlpha);
			vh.Clear();
			vh.AddUIVertexTriangleStream(UIEffectBase.tempVerts);
			UIEffectBase.tempVerts.Clear();
		}

		private void _ApplyShadow(List<UIVertex> verts, ref int start, ref int end, UIEffect.ShadowStyle mode, float toneLevel, float blur, Vector2 effectDistance, Color color, bool useGraphicAlpha)
		{
			if (mode == UIEffect.ShadowStyle.None)
			{
				return;
			}
			Vector2 vector;
			vector..ctor(Packer.ToFloat(toneLevel, 0f, blur, 0f), Packer.ToFloat(color.r, color.g, color.b, 1f));
			this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, effectDistance.x, effectDistance.y, vector, color, useGraphicAlpha);
			if (mode == UIEffect.ShadowStyle.Shadow3)
			{
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, effectDistance.x, 0f, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, 0f, effectDistance.y, vector, color, useGraphicAlpha);
			}
			else if (mode == UIEffect.ShadowStyle.Outline)
			{
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, effectDistance.x, -effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, -effectDistance.x, effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, -effectDistance.x, -effectDistance.y, vector, color, useGraphicAlpha);
			}
			else if (mode == UIEffect.ShadowStyle.Outline8)
			{
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, effectDistance.x, -effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, -effectDistance.x, effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, -effectDistance.x, -effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, -effectDistance.x, 0f, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, 0f, -effectDistance.y, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, effectDistance.x, 0f, vector, color, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIEffectBase.tempVerts, ref start, ref end, 0f, effectDistance.y, vector, color, useGraphicAlpha);
			}
		}

		private void _ApplyShadowZeroAlloc(List<UIVertex> verts, ref int start, ref int end, float x, float y, Vector2 factor, Color color, bool useGraphicAlpha)
		{
			int num = verts.Count + end - start;
			if (verts.Capacity < num)
			{
				verts.Capacity = num;
			}
			for (int i = start; i < end; i++)
			{
				UIVertex uivertex = verts[i];
				verts.Add(uivertex);
				Vector3 position = uivertex.position;
				uivertex.position.Set(position.x + x, position.y + y, position.z);
				Color color2 = color;
				if (this.colorMode != UIEffect.ColorMode.None)
				{
					color2.r = (color2.g = (color2.b = 1f));
				}
				color2.a = ((!useGraphicAlpha) ? color.a : (color.a * (float)uivertex.color.a / 255f));
				uivertex.color = color2;
				uivertex.uv1 = factor;
				verts[i] = uivertex;
			}
			start = end;
			end = verts.Count;
		}

		public const string shaderName = "UI/Hidden/UI-Effect";

		[SerializeField]
		[Range(0f, 1f)]
		private float m_ToneLevel = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Blur = 0.25f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_ShadowBlur = 0.25f;

		[SerializeField]
		private UIEffect.ShadowStyle m_ShadowStyle;

		[SerializeField]
		private UIEffect.ToneMode m_ToneMode;

		[SerializeField]
		private UIEffect.ColorMode m_ColorMode;

		[SerializeField]
		private UIEffect.BlurMode m_BlurMode;

		[SerializeField]
		private Color m_ShadowColor = Color.black;

		[SerializeField]
		private Vector2 m_EffectDistance = new Vector2(1f, -1f);

		[SerializeField]
		private bool m_UseGraphicAlpha = true;

		[SerializeField]
		private Color m_EffectColor = Color.white;

		[SerializeField]
		private List<UIEffect.AdditionalShadow> m_AdditionalShadows = new List<UIEffect.AdditionalShadow>();

		[SerializeField]
		private bool m_CustomEffect;

		[SerializeField]
		private Vector4 m_CustomFactor = default(Vector4);

		[Serializable]
		public class AdditionalShadow
		{
			[Range(0f, 1f)]
			public float shadowBlur = 0.25f;

			public UIEffect.ShadowStyle shadowMode = UIEffect.ShadowStyle.Shadow;

			public Color shadowColor = Color.black;

			public Vector2 effectDistance = new Vector2(1f, -1f);

			public bool useGraphicAlpha = true;
		}

		public enum ToneMode
		{
			None,
			Grayscale,
			Sepia,
			Nega,
			Pixel,
			Mono,
			Cutoff,
			Hue
		}

		public enum ColorMode
		{
			None,
			Set,
			Add,
			Sub
		}

		public enum ShadowStyle
		{
			None,
			Shadow,
			Outline,
			Outline8,
			Shadow3
		}

		public enum BlurMode
		{
			None,
			Fast,
			Medium,
			Detail
		}
	}
}
