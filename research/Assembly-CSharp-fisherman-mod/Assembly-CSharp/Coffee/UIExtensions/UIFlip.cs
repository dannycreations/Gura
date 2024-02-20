using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIFlip : BaseMeshEffect
	{
		public bool horizontal
		{
			get
			{
				return this.m_Horizontal;
			}
			set
			{
				this.m_Horizontal = value;
			}
		}

		public bool vertical
		{
			get
			{
				return this.m_Veritical;
			}
			set
			{
				this.m_Veritical = value;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			RectTransform rectTransform = base.graphic.rectTransform;
			UIVertex uivertex = default(UIVertex);
			Vector2 center = rectTransform.rect.center;
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref uivertex, i);
				Vector3 position = uivertex.position;
				uivertex.position = new Vector3((!this.m_Horizontal) ? position.x : (-position.x), (!this.m_Veritical) ? position.y : (-position.y));
				vh.SetUIVertex(uivertex, i);
			}
		}

		[SerializeField]
		private bool m_Horizontal;

		[SerializeField]
		private bool m_Veritical;
	}
}
