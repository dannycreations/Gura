using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class UIDissolve : BaseMeshEffect
	{
		public Graphic graphic
		{
			get
			{
				return base.graphic;
			}
		}

		public float location
		{
			get
			{
				return this.m_Location;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Location, value))
				{
					this.m_Location = value;
					this._SetDirty();
				}
			}
		}

		public float width
		{
			get
			{
				return this.m_Width;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Width, value))
				{
					this.m_Width = value;
					this._SetDirty();
				}
			}
		}

		public float softness
		{
			get
			{
				return this.m_Softness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Softness, value))
				{
					this.m_Softness = value;
					this._SetDirty();
				}
			}
		}

		public Color color
		{
			get
			{
				return this.m_Color;
			}
			set
			{
				if (this.m_Color != value)
				{
					this.m_Color = value;
					this._SetDirty();
				}
			}
		}

		public UIEffect.ColorMode colorMode
		{
			get
			{
				return this.m_ColorMode;
			}
		}

		public virtual Material effectMaterial
		{
			get
			{
				return this.m_EffectMaterial;
			}
		}

		public bool play
		{
			get
			{
				return this.m_Play;
			}
			set
			{
				this.m_Play = value;
			}
		}

		public float duration
		{
			get
			{
				return this.m_Duration;
			}
			set
			{
				this.m_Duration = Mathf.Max(value, 0.1f);
			}
		}

		public AnimatorUpdateMode updateMode
		{
			get
			{
				return this.m_UpdateMode;
			}
			set
			{
				this.m_UpdateMode = value;
			}
		}

		protected override void OnEnable()
		{
			this._time = 0f;
			this.graphic.material = this.effectMaterial;
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			this.graphic.material = null;
			base.OnDisable();
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			Rect rect = this.graphic.rectTransform.rect;
			UIVertex uivertex = default(UIVertex);
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref uivertex, i);
				float num = Mathf.Clamp01(uivertex.position.x / rect.width + 0.5f);
				float num2 = Mathf.Clamp01(uivertex.position.y / rect.height + 0.5f);
				uivertex.uv1 = new Vector2(Packer.ToFloat(num, num2, this.location, this.m_Width), Packer.ToFloat(this.m_Color.r, this.m_Color.g, this.m_Color.b, this.m_Softness));
				vh.SetUIVertex(uivertex, i);
			}
		}

		public void Play()
		{
			this._time = 0f;
			this.m_Play = true;
		}

		private void Update()
		{
			if (!this.m_Play || !Application.isPlaying)
			{
				return;
			}
			this._time += ((this.m_UpdateMode != 2) ? Time.deltaTime : Time.unscaledDeltaTime);
			this.location = this._time / this.m_Duration;
			if (this.m_Duration <= this._time)
			{
				this.m_Play = false;
				this._time = 0f;
			}
		}

		private void _SetDirty()
		{
			if (this.graphic)
			{
				this.graphic.SetVerticesDirty();
			}
		}

		public const string shaderName = "UI/Hidden/UI-Effect-Dissolve";

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Location = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Width = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Softness = 0.5f;

		[SerializeField]
		[ColorUsage(false)]
		private Color m_Color = new Color(0f, 0.25f, 1f);

		[SerializeField]
		private UIEffect.ColorMode m_ColorMode = UIEffect.ColorMode.Add;

		[SerializeField]
		private Material m_EffectMaterial;

		[Space]
		[SerializeField]
		private bool m_Play;

		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_Duration = 1f;

		[SerializeField]
		private AnimatorUpdateMode m_UpdateMode;

		private float _time;
	}
}
