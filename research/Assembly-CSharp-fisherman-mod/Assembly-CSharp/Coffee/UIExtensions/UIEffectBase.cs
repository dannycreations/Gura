using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public abstract class UIEffectBase : BaseMeshEffect, ISerializationCallbackReceiver
	{
		public Graphic targetGraphic
		{
			get
			{
				return base.graphic;
			}
		}

		public Material effectMaterial
		{
			get
			{
				return this.m_EffectMaterial;
			}
		}

		public virtual void OnBeforeSerialize()
		{
		}

		public virtual void OnAfterDeserialize()
		{
		}

		protected override void OnEnable()
		{
			this.targetGraphic.material = this.m_EffectMaterial;
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			this.targetGraphic.material = null;
			base.OnDisable();
		}

		protected void SetDirty()
		{
			if (this.targetGraphic)
			{
				this.targetGraphic.SetVerticesDirty();
			}
		}

		protected static readonly List<UIVertex> tempVerts = new List<UIVertex>();

		[SerializeField]
		protected Material m_EffectMaterial;
	}
}
