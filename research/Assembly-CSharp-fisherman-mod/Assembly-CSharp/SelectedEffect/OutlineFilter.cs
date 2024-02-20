using System;
using UnityEngine;

namespace SelectedEffect
{
	[RequireComponent(typeof(Camera))]
	public class OutlineFilter : MonoBehaviour
	{
		private void Start()
		{
			this.m_Mgr = Object.FindObjectOfType<OutlineMgr>();
			this.m_Camera = base.GetComponent<Camera>();
			this.m_RTCam = new GameObject().AddComponent<Camera>();
			this.m_RTCam.name = "RTCam";
			this.m_RTCam.transform.parent = this.m_Camera.gameObject.transform;
			this.m_RTCam.enabled = false;
		}

		private void OnEnable()
		{
			this.m_SdrGlowFlatColor = Shader.Find("Selected Effect --- Outline/Post Process/Flat Color");
			this.m_SdrDepthOnly = Shader.Find("Selected Effect --- Outline/Post Process/Depth Only");
			Shader shader = Shader.Find("Selected Effect --- Outline/Post Process/Halo");
			this.m_MatGlowHalo = new Material(shader);
			shader = Shader.Find("Selected Effect --- Outline/Post Process/Blur");
			this.m_MatGlowBlur = new Material(shader);
		}

		private void OnDisable()
		{
			if (this.m_MatGlowHalo)
			{
				Object.DestroyImmediate(this.m_MatGlowHalo);
				this.m_MatGlowHalo = null;
			}
			if (this.m_MatGlowBlur)
			{
				Object.DestroyImmediate(this.m_MatGlowBlur);
				this.m_MatGlowBlur = null;
			}
		}

		private void DoBlurPass(RenderTexture input, RenderTexture output, bool vertical)
		{
			if (vertical)
			{
				this.m_MatGlowBlur.SetVector("_Offsets", new Vector4(0f, this.m_BlurPixelOffset, 0f, 0f));
				Graphics.Blit(input, output, this.m_MatGlowBlur);
			}
			else
			{
				this.m_MatGlowBlur.SetVector("_Offsets", new Vector4(this.m_BlurPixelOffset, 0f, 0f, 0f));
				Graphics.Blit(input, output, this.m_MatGlowBlur);
			}
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			Graphics.Blit(src, dst);
			this.m_RTCam.CopyFrom(this.m_Camera);
			this.m_RTCam.clearFlags = 2;
			this.m_RTCam.backgroundColor = Color.black;
			RenderTexture temporary = RenderTexture.GetTemporary(src.width, src.height, 16, 16);
			this.m_RTCam.targetTexture = temporary;
			if (this.m_Obstacle)
			{
				this.m_RTCam.cullingMask = ~(1 << LayerMask.NameToLayer(this.m_Mgr.m_Layer));
				this.m_RTCam.RenderWithShader(this.m_SdrDepthOnly, string.Empty);
				this.m_RTCam.clearFlags = 4;
			}
			this.m_RTCam.cullingMask = 1 << LayerMask.NameToLayer(this.m_Mgr.m_Layer);
			this.m_RTCam.RenderWithShader(this.m_SdrGlowFlatColor, string.Empty);
			int num = 1;
			RenderTexture temporary2 = RenderTexture.GetTemporary(Screen.width / num, Screen.height / num, 0);
			RenderTexture temporary3 = RenderTexture.GetTemporary(Screen.width / num, Screen.height / num, 0);
			this.DoBlurPass(temporary, temporary2, true);
			this.DoBlurPass(temporary2, temporary3, false);
			this.m_MatGlowHalo.SetTexture("_GlowObjectTex", temporary);
			this.m_MatGlowHalo.SetColor("_GlowColor", this.m_Color);
			this.m_MatGlowHalo.SetFloat("_GlowIntensity", this.m_GlowIntensity);
			Graphics.Blit(temporary3, dst, this.m_MatGlowHalo);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
		}

		[Header("Post Process")]
		public Color m_Color = Color.green;

		public bool m_Obstacle;

		[Range(0.2f, 2.2f)]
		public float m_BlurPixelOffset = 1.2f;

		[Range(1f, 6f)]
		public float m_GlowIntensity = 3f;

		[Header("Internal")]
		public Shader m_SdrGlowFlatColor;

		public Shader m_SdrDepthOnly;

		public Material m_MatGlowHalo;

		public Material m_MatGlowBlur;

		private Camera m_Camera;

		private Camera m_RTCam;

		private OutlineMgr m_Mgr;
	}
}
