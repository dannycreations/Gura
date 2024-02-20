using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[DisallowMultipleComponent]
	public class UIGradient : BaseMeshEffect
	{
		public Graphic graphic
		{
			get
			{
				return base.graphic;
			}
		}

		public UIGradient.Direction direction
		{
			get
			{
				return this.m_Direction;
			}
			set
			{
				if (this.m_Direction != value)
				{
					this.m_Direction = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public Color color1
		{
			get
			{
				return this.m_Color1;
			}
			set
			{
				if (this.m_Color1 != value)
				{
					this.m_Color1 = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public Color color2
		{
			get
			{
				return this.m_Color2;
			}
			set
			{
				if (this.m_Color2 != value)
				{
					this.m_Color2 = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public Color color3
		{
			get
			{
				return this.m_Color3;
			}
			set
			{
				if (this.m_Color3 != value)
				{
					this.m_Color3 = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public Color color4
		{
			get
			{
				return this.m_Color4;
			}
			set
			{
				if (this.m_Color4 != value)
				{
					this.m_Color4 = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public float rotation
		{
			get
			{
				return (this.m_Direction != UIGradient.Direction.Horizontal) ? ((this.m_Direction != UIGradient.Direction.Vertical) ? this.m_Rotation : 0f) : (-90f);
			}
			set
			{
				if (!Mathf.Approximately(this.m_Rotation, value))
				{
					this.m_Rotation = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public float offset
		{
			get
			{
				return this.m_Offset1;
			}
			set
			{
				if (this.m_Offset1 != value)
				{
					this.m_Offset1 = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public Vector2 offset2
		{
			get
			{
				return new Vector2(this.m_Offset2, this.m_Offset1);
			}
			set
			{
				if (this.m_Offset1 != value.y || this.m_Offset2 != value.x)
				{
					this.m_Offset1 = value.y;
					this.m_Offset2 = value.x;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public UIGradient.GradientStyle gradientStyle
		{
			get
			{
				return this.m_GradientStyle;
			}
			set
			{
				if (this.m_GradientStyle != value)
				{
					this.m_GradientStyle = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public ColorSpace colorSpace
		{
			get
			{
				return this.m_ColorSpace;
			}
			set
			{
				if (this.m_ColorSpace != value)
				{
					this.m_ColorSpace = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public bool ignoreAspectRatio
		{
			get
			{
				return this.m_IgnoreAspectRatio;
			}
			set
			{
				if (this.m_IgnoreAspectRatio != value)
				{
					this.m_IgnoreAspectRatio = value;
					this.graphic.SetVerticesDirty();
				}
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!this.IsActive())
			{
				return;
			}
			Rect rect = default(Rect);
			UIVertex uivertex = default(UIVertex);
			if (!(this.graphic is Text) || this.m_GradientStyle == UIGradient.GradientStyle.Rect)
			{
				rect = this.graphic.rectTransform.rect;
			}
			else if (this.m_GradientStyle == UIGradient.GradientStyle.Split)
			{
				rect.Set(0f, 0f, 1f, 1f);
			}
			else if (this.m_GradientStyle == UIGradient.GradientStyle.Fit)
			{
				float num = float.MaxValue;
				rect.yMin = num;
				rect.xMin = num;
				num = float.MinValue;
				rect.yMax = num;
				rect.xMax = num;
				for (int i = 0; i < vh.currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref uivertex, i);
					rect.xMin = Mathf.Min(rect.xMin, uivertex.position.x);
					rect.yMin = Mathf.Min(rect.yMin, uivertex.position.y);
					rect.xMax = Mathf.Max(rect.xMax, uivertex.position.x);
					rect.yMax = Mathf.Max(rect.yMax, uivertex.position.y);
				}
			}
			float num2 = this.rotation * 0.017453292f;
			Vector2 normalized;
			normalized..ctor(Mathf.Cos(num2), Mathf.Sin(num2));
			if (!this.m_IgnoreAspectRatio && UIGradient.Direction.Angle <= this.m_Direction)
			{
				normalized.x *= rect.height / rect.width;
				normalized = normalized.normalized;
			}
			UIGradient.Matrix2x3 matrix2x = new UIGradient.Matrix2x3(rect, normalized.x, normalized.y);
			for (int j = 0; j < vh.currentVertCount; j++)
			{
				vh.PopulateUIVertex(ref uivertex, j);
				Vector2 vector;
				if (this.m_GradientStyle == UIGradient.GradientStyle.Split)
				{
					vector = matrix2x * UIGradient.s_SplitedCharacterPosition[j % 4] + this.offset2;
				}
				else
				{
					vector = matrix2x * uivertex.position + this.offset2;
				}
				Color color;
				if (this.direction == UIGradient.Direction.Diagonal)
				{
					color = Color.LerpUnclamped(Color.LerpUnclamped(this.m_Color1, this.m_Color2, vector.x), Color.LerpUnclamped(this.m_Color3, this.m_Color4, vector.x), vector.y);
				}
				else
				{
					color = Color.LerpUnclamped(this.m_Color2, this.m_Color1, vector.y);
				}
				uivertex.color *= ((this.m_ColorSpace != null) ? ((this.m_ColorSpace != 1) ? color : color.linear) : color.gamma);
				vh.SetUIVertex(uivertex, j);
			}
		}

		[SerializeField]
		private UIGradient.Direction m_Direction;

		[SerializeField]
		private Color m_Color1 = Color.white;

		[SerializeField]
		private Color m_Color2 = Color.white;

		[SerializeField]
		private Color m_Color3 = Color.white;

		[SerializeField]
		private Color m_Color4 = Color.white;

		[SerializeField]
		[Range(-180f, 180f)]
		private float m_Rotation;

		[SerializeField]
		[Range(-1f, 1f)]
		private float m_Offset1;

		[SerializeField]
		[Range(-1f, 1f)]
		private float m_Offset2;

		[SerializeField]
		private UIGradient.GradientStyle m_GradientStyle;

		[SerializeField]
		private ColorSpace m_ColorSpace = -1;

		[SerializeField]
		private bool m_IgnoreAspectRatio = true;

		private static readonly Vector2[] s_SplitedCharacterPosition = new Vector2[]
		{
			Vector2.up,
			Vector2.one,
			Vector2.right,
			Vector2.zero
		};

		public enum Direction
		{
			Horizontal,
			Vertical,
			Angle,
			Diagonal
		}

		public enum GradientStyle
		{
			Rect,
			Fit,
			Split
		}

		private struct Matrix2x3
		{
			public Matrix2x3(Rect rect, float cos, float sin)
			{
				float num = -rect.xMin / rect.width - 0.5f;
				float num2 = -rect.yMin / rect.height - 0.5f;
				this.m00 = cos / rect.width;
				this.m01 = -sin / rect.height;
				this.m02 = num * cos - num2 * sin + 0.5f;
				this.m10 = sin / rect.width;
				this.m11 = cos / rect.height;
				this.m12 = num * sin + num2 * cos + 0.5f;
			}

			public static Vector2 operator *(UIGradient.Matrix2x3 m, Vector2 v)
			{
				return new Vector2(m.m00 * v.x + m.m01 * v.y + m.m02, m.m10 * v.x + m.m11 * v.y + m.m12);
			}

			public float m00;

			public float m01;

			public float m02;

			public float m10;

			public float m11;

			public float m12;
		}
	}
}
