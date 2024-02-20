using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIShadow : Shadow
	{
		public Graphic graphic
		{
			get
			{
				return base.graphic;
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
				this.m_Blur = Mathf.Clamp(value, 0f, 2f);
				this._SetDirty();
			}
		}

		public UIShadow.ShadowStyle style
		{
			get
			{
				return this.m_Style;
			}
			set
			{
				this.m_Style = value;
				this._SetDirty();
			}
		}

		public List<UIShadow.AdditionalShadow> additionalShadows
		{
			get
			{
				return this.m_AdditionalShadows;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!base.isActiveAndEnabled || vh.currentVertCount <= 0)
			{
				return;
			}
			vh.GetUIVertexStream(UIShadow.s_Verts);
			this._uiEffect = base.GetComponent<UIEffect>();
			int count = UIShadow.s_Verts.Count;
			int num = 0;
			int num2 = count;
			float num3 = ((!this._uiEffect || !this._uiEffect.isActiveAndEnabled) ? 0f : this._uiEffect.toneLevel);
			int num4 = this.additionalShadows.Count - 1;
			while (0 <= num4)
			{
				UIShadow.AdditionalShadow additionalShadow = this.additionalShadows[num4];
				this.UpdateFactor(num3, additionalShadow.blur, additionalShadow.effectColor);
				this._ApplyShadow(UIShadow.s_Verts, additionalShadow.effectColor, ref num, ref num2, additionalShadow.effectDistance, additionalShadow.style, additionalShadow.useGraphicAlpha);
				num4--;
			}
			this.UpdateFactor(num3, this.blur, base.effectColor);
			this._ApplyShadow(UIShadow.s_Verts, base.effectColor, ref num, ref num2, base.effectDistance, this.style, base.useGraphicAlpha);
			vh.Clear();
			vh.AddUIVertexTriangleStream(UIShadow.s_Verts);
			UIShadow.s_Verts.Clear();
		}

		private void UpdateFactor(float tone, float blur, Color color)
		{
			if (this._uiEffect && this._uiEffect.isActiveAndEnabled)
			{
				this._factor = new Vector2(Packer.ToFloat(tone, 0f, blur, 0f), Packer.ToFloat(color.r, color.g, color.b, 1f));
			}
		}

		private void _ApplyShadow(List<UIVertex> verts, Color color, ref int start, ref int end, Vector2 effectDistance, UIShadow.ShadowStyle style, bool useGraphicAlpha)
		{
			if (style == UIShadow.ShadowStyle.None || color.a <= 0f)
			{
				return;
			}
			this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, effectDistance.x, effectDistance.y, useGraphicAlpha);
			if (style == UIShadow.ShadowStyle.Shadow3)
			{
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, effectDistance.x, 0f, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, 0f, effectDistance.y, useGraphicAlpha);
			}
			else if (style == UIShadow.ShadowStyle.Outline)
			{
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, effectDistance.x, -effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, -effectDistance.x, effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, -effectDistance.x, -effectDistance.y, useGraphicAlpha);
			}
			else if (style == UIShadow.ShadowStyle.Outline8)
			{
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, effectDistance.x, -effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, -effectDistance.x, effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, -effectDistance.x, -effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, -effectDistance.x, 0f, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, 0f, -effectDistance.y, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, effectDistance.x, 0f, useGraphicAlpha);
				this._ApplyShadowZeroAlloc(UIShadow.s_Verts, color, ref start, ref end, 0f, effectDistance.y, useGraphicAlpha);
			}
		}

		private void _ApplyShadowZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x, float y, bool useGraphicAlpha)
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
				color2.a = ((!useGraphicAlpha) ? color.a : (color.a * (float)uivertex.color.a / 255f));
				uivertex.color = color2;
				if (this._uiEffect)
				{
					uivertex.uv1 = this._factor;
				}
				verts[i] = uivertex;
			}
			start = end;
			end = verts.Count;
		}

		private void _SetDirty()
		{
			if (this.graphic)
			{
				this.graphic.SetVerticesDirty();
			}
		}

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Blur = 0.25f;

		[SerializeField]
		private UIShadow.ShadowStyle m_Style = UIShadow.ShadowStyle.Shadow;

		[SerializeField]
		private List<UIShadow.AdditionalShadow> m_AdditionalShadows = new List<UIShadow.AdditionalShadow>();

		private UIEffect _uiEffect;

		private Vector2 _factor;

		private static readonly List<UIVertex> s_Verts = new List<UIVertex>();

		[Serializable]
		public class AdditionalShadow
		{
			[Range(0f, 1f)]
			public float blur = 0.25f;

			public UIShadow.ShadowStyle style = UIShadow.ShadowStyle.Shadow;

			public Color effectColor = Color.black;

			public Vector2 effectDistance = new Vector2(1f, -1f);

			public bool useGraphicAlpha = true;
		}

		public enum ShadowStyle
		{
			None,
			Shadow,
			Outline,
			Outline8,
			Shadow3
		}
	}
}
