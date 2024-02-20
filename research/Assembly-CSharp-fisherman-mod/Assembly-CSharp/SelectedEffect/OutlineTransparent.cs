using System;
using UnityEngine;

namespace SelectedEffect
{
	[RequireComponent(typeof(Renderer))]
	public class OutlineTransparent : MonoBehaviour
	{
		private void Start()
		{
			this.m_Rd = base.GetComponent<Renderer>();
		}

		private void Update()
		{
			Material[] materials = this.m_Rd.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].SetFloat("_Transparent", this.m_Transparent);
				materials[i].SetFloat("_OutlineWidth", this.m_OutlineWidth);
				materials[i].SetColor("_OutlineColor", this.m_OutlineColor);
				materials[i].SetFloat("_OutlineFactor", this.m_OutlineFactor);
				materials[i].SetFloat("_Overlay", this.m_Overlay);
				materials[i].SetColor("_OverlayColor", this.m_OverlayColor);
			}
		}

		[Header("Parameters")]
		[Range(0f, 1f)]
		public float m_Transparent = 0.5f;

		public Color m_OutlineColor = Color.green;

		[Range(0.01f, 0.1f)]
		public float m_OutlineWidth = 0.02f;

		[Range(0f, 1f)]
		public float m_OutlineFactor = 1f;

		[Range(0f, 0.6f)]
		public float m_Overlay;

		public Color m_OverlayColor = Color.red;

		[Header("Auto")]
		private Renderer m_Rd;
	}
}
