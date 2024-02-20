using System;
using UnityEngine;

namespace SelectedEffect
{
	[RequireComponent(typeof(Renderer))]
	public class Outline : MonoBehaviour
	{
		public void Initialize()
		{
			this.m_Mgr = Object.FindObjectOfType<OutlineMgr>();
			this.m_Rd = base.GetComponent<Renderer>();
			this.m_BackupMats = this.m_Rd.materials;
			this.m_SdrOriginal = Shader.Find("Standard");
			this.m_SdrOutlineOnly = Shader.Find("Selected Effect --- Outline/Normal Expansion/Outline Only");
			this.m_SdrOutlineDiffuse = Shader.Find("Selected Effect --- Outline/Normal Expansion/Diffuse");
		}

		public void UpdateSelfParameters()
		{
			if (this.m_TriggerMethod == Outline.ETriggerMethod.MouseRightPress)
			{
				bool flag = this.m_IsMouseOn && Input.GetMouseButton(1);
				if (flag)
				{
					this.OutlineEnable();
				}
				else
				{
					this.OutlineDisable();
				}
			}
			else if (this.m_TriggerMethod == Outline.ETriggerMethod.MouseLeftPress)
			{
				bool flag2 = this.m_IsMouseOn && Input.GetMouseButton(0);
				if (flag2)
				{
					this.OutlineEnable();
				}
				else
				{
					this.OutlineDisable();
				}
			}
			if (this.m_OverlayFlash)
			{
				float num = Mathf.Sin(Time.time * this.m_OverlayFlashSpeed) * 0.5f + 0.5f;
				this.m_Overlay = num * 0.6f;
			}
			Material[] materials = this.m_Rd.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].SetFloat("_OutlineWidth", this.m_OutlineWidth);
				materials[i].SetColor("_OutlineColor", this.m_OutlineColor);
				materials[i].SetFloat("_OutlineFactor", this.m_OutlineFactor);
				materials[i].SetColor("_OverlayColor", this.m_OverlayColor);
				materials[i].SetTexture("_MainTex", this.m_BackupMats[i].GetTexture("_MainTex"));
				materials[i].SetTextureOffset("_MainTex", this.m_BackupMats[i].GetTextureOffset("_MainTex"));
				materials[i].SetTextureScale("_MainTex", this.m_BackupMats[i].GetTextureScale("_MainTex"));
				materials[i].SetFloat("_OutlineWriteZ", (!this.m_WriteZ) ? 0f : 1f);
				materials[i].SetFloat("_OutlineBasedVertexColorR", (!this.m_BasedOnVertexColorR) ? 1f : 0f);
				materials[i].SetFloat("_Overlay", this.m_Overlay);
			}
		}

		private void OutlineEnable()
		{
			if (this.m_Mgr.m_Tech == OutlineMgr.ETech.NormalExpansion)
			{
				Material[] materials = this.m_Rd.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					if (this.m_OutlineOnly)
					{
						materials[i].shader = this.m_SdrOutlineOnly;
					}
					else
					{
						materials[i].shader = this.m_SdrOutlineDiffuse;
					}
				}
			}
			else if (this.m_Mgr.m_Tech == OutlineMgr.ETech.PostProcess)
			{
				base.gameObject.layer = LayerMask.NameToLayer(this.m_Mgr.m_Layer);
			}
		}

		private void OutlineDisable()
		{
			if (this.m_Mgr.m_Tech == OutlineMgr.ETech.NormalExpansion)
			{
				Material[] materials = this.m_Rd.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i].shader = this.m_SdrOriginal;
				}
			}
			else if (this.m_Mgr.m_Tech == OutlineMgr.ETech.PostProcess)
			{
				base.gameObject.layer = LayerMask.NameToLayer("Default");
			}
		}

		private void OnMouseEnter()
		{
			this.m_IsMouseOn = true;
			if (this.m_TriggerMethod == Outline.ETriggerMethod.MouseMove)
			{
				this.OutlineEnable();
			}
		}

		private void OnMouseExit()
		{
			this.m_IsMouseOn = false;
			if (!this.m_Persistent)
			{
				this.OutlineDisable();
			}
		}

		public void DisableFx()
		{
			this.OutlineDisable();
			base.gameObject.layer = LayerMask.NameToLayer("Default");
		}

		[Header("Trigger Method")]
		public Outline.ETriggerMethod m_TriggerMethod;

		public bool m_Persistent;

		[Header("Normal Expansion Parameters")]
		public Color m_OutlineColor = Color.green;

		[Range(0.01f, 0.2f)]
		public float m_OutlineWidth = 0.02f;

		[Range(0f, 1f)]
		public float m_OutlineFactor = 1f;

		public bool m_WriteZ;

		public bool m_BasedOnVertexColorR;

		public bool m_OutlineOnly;

		[Header("Normal Expansion Flash")]
		public Color m_OverlayColor = Color.red;

		[Range(0f, 0.6f)]
		public float m_Overlay;

		public bool m_OverlayFlash;

		[Range(1f, 6f)]
		public float m_OverlayFlashSpeed = 3f;

		[Header("Internal")]
		public Material[] m_BackupMats;

		public Renderer m_Rd;

		public Shader m_SdrOutlineOnly;

		public Shader m_SdrOutlineDiffuse;

		public Shader m_SdrOriginal;

		public OutlineMgr m_Mgr;

		private bool m_IsMouseOn;

		public enum ETriggerMethod
		{
			MouseMove,
			MouseRightPress,
			MouseLeftPress
		}
	}
}
