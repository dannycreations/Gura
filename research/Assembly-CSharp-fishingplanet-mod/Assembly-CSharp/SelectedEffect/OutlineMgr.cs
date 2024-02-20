using System;
using UnityEngine;

namespace SelectedEffect
{
	public class OutlineMgr : MonoBehaviour
	{
		private void Start()
		{
			this.m_Outlines = Object.FindObjectsOfType<Outline>();
			for (int i = 0; i < this.m_Outlines.Length; i++)
			{
				this.m_Outlines[i].Initialize();
			}
			this.m_OutlineFilter = Object.FindObjectOfType<OutlineFilter>();
			this.OnTechChange();
		}

		private void Update()
		{
			if (this.m_PrevTech != this.m_Tech)
			{
				this.OnTechChange();
			}
			for (int i = 0; i < this.m_Outlines.Length; i++)
			{
				this.m_Outlines[i].UpdateSelfParameters();
			}
		}

		private void OnTechChange()
		{
			this.m_PrevTech = this.m_Tech;
			for (int i = 0; i < this.m_Outlines.Length; i++)
			{
				this.m_Outlines[i].DisableFx();
			}
			if (this.m_Tech == OutlineMgr.ETech.PostProcess)
			{
				this.m_OutlineFilter.enabled = true;
			}
			else
			{
				this.m_OutlineFilter.enabled = false;
			}
		}

		public OutlineMgr.ETech m_Tech;

		private OutlineMgr.ETech m_PrevTech;

		private Outline[] m_Outlines;

		private OutlineFilter m_OutlineFilter;

		public string m_Layer = "Water";

		public enum ETech
		{
			NormalExpansion,
			PostProcess
		}
	}
}
