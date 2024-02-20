using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UICustomEffect : UIEffectBase
	{
		public Vector4 customFactor1
		{
			get
			{
				return this.m_CustomFactor1;
			}
			set
			{
				this.m_CustomFactor1 = value;
				base.SetDirty();
			}
		}

		public Vector4 customFactor2
		{
			get
			{
				return this.m_CustomFactor2;
			}
			set
			{
				this.m_CustomFactor2 = value;
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
			vector..ctor(Packer.ToFloat(this.m_CustomFactor1), Packer.ToFloat(this.m_CustomFactor2));
			for (int i = 0; i < UIEffectBase.tempVerts.Count; i++)
			{
				UIVertex uivertex = UIEffectBase.tempVerts[i];
				uivertex.uv1 = vector;
				UIEffectBase.tempVerts[i] = uivertex;
			}
			vh.Clear();
			vh.AddUIVertexTriangleStream(UIEffectBase.tempVerts);
			UIEffectBase.tempVerts.Clear();
		}

		[SerializeField]
		private Vector4 m_CustomFactor1 = default(Vector4);

		[SerializeField]
		private Vector4 m_CustomFactor2 = default(Vector4);
	}
}
