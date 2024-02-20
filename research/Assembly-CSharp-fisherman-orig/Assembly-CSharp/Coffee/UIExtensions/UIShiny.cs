using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class UIShiny : BaseMeshEffect
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
				value = Mathf.Clamp(value, 0.01f, 1f);
				if (!Mathf.Approximately(this.m_Softness, value))
				{
					this.m_Softness = value;
					this._SetDirty();
				}
			}
		}

		[Obsolete("Use brightness instead (UnityUpgradable) -> brightness")]
		public float alpha
		{
			get
			{
				return this.m_Brightness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Brightness, value))
				{
					this.m_Brightness = value;
					this._SetDirty();
				}
			}
		}

		public float brightness
		{
			get
			{
				return this.m_Brightness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Brightness, value))
				{
					this.m_Brightness = value;
					this._SetDirty();
				}
			}
		}

		public float highlight
		{
			get
			{
				return this.m_Highlight;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(this.m_Highlight, value))
				{
					this.m_Highlight = value;
					this._SetDirty();
				}
			}
		}

		public float rotation
		{
			get
			{
				return this.m_Rotation;
			}
			set
			{
				if (!Mathf.Approximately(this.m_Rotation, value))
				{
					this.m_Rotation = value;
					this._SetDirty();
				}
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

		public bool loop
		{
			get
			{
				return this.m_Loop;
			}
			set
			{
				this.m_Loop = value;
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

		public float loopDelay
		{
			get
			{
				return this.m_LoopDelay;
			}
			set
			{
				this.m_LoopDelay = Mathf.Max(value, 0f);
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
			float num = this.rotation * 0.017453292f;
			Vector2 normalized;
			normalized..ctor(Mathf.Cos(num), Mathf.Sin(num));
			normalized.x *= rect.height / rect.width;
			normalized = normalized.normalized;
			UIVertex uivertex = default(UIVertex);
			Matrix2x3 matrix2x = new Matrix2x3(rect, normalized.x, normalized.y);
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref uivertex, i);
				uivertex.uv1 = new Vector2(Packer.ToFloat(Mathf.Clamp01((matrix2x * uivertex.position).y), this.softness, this.width, this.brightness), Packer.ToFloat(this.location, this.highlight));
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
				this.m_Play = this.m_Loop;
				this._time = ((!this.m_Loop) ? 0f : (-this.m_LoopDelay));
			}
		}

		private void _SetDirty()
		{
			if (this.graphic)
			{
				this.graphic.SetVerticesDirty();
			}
		}

		public const string shaderName = "UI/Hidden/UI-Effect-Shiny";

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Location;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Width = 0.25f;

		[SerializeField]
		[Range(-180f, 180f)]
		private float m_Rotation;

		[SerializeField]
		[Range(0.01f, 1f)]
		private float m_Softness = 1f;

		[FormerlySerializedAs("m_Alpha")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Brightness = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Highlight = 1f;

		[SerializeField]
		private Material m_EffectMaterial;

		[Space]
		[SerializeField]
		private bool m_Play;

		[SerializeField]
		private bool m_Loop;

		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_Duration = 1f;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_LoopDelay = 1f;

		[SerializeField]
		private AnimatorUpdateMode m_UpdateMode;

		private float _time;
	}
}
