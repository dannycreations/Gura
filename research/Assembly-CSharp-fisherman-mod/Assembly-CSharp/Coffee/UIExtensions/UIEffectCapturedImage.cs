using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	public class UIEffectCapturedImage : RawImage
	{
		public float toneLevel
		{
			get
			{
				return this.m_ToneLevel;
			}
			set
			{
				this.m_ToneLevel = Mathf.Clamp(value, 0f, 1f);
			}
		}

		public float blur
		{
			get
			{
				return this.m_Blur;
			}
			set
			{
				this.m_Blur = Mathf.Clamp(value, 0f, 4f);
			}
		}

		public UIEffect.ToneMode toneMode
		{
			get
			{
				return this.m_ToneMode;
			}
			set
			{
				this.m_ToneMode = value;
			}
		}

		public UIEffect.ColorMode colorMode
		{
			get
			{
				return this.m_ColorMode;
			}
			set
			{
				this.m_ColorMode = value;
			}
		}

		public UIEffect.BlurMode blurMode
		{
			get
			{
				return this.m_BlurMode;
			}
			set
			{
				this.m_BlurMode = value;
			}
		}

		public Color effectColor
		{
			get
			{
				return this.m_EffectColor;
			}
			set
			{
				this.m_EffectColor = value;
			}
		}

		public virtual Material effectMaterial
		{
			get
			{
				return this.m_EffectMaterial;
			}
		}

		public UIEffectCapturedImage.DesamplingRate desamplingRate
		{
			get
			{
				return this.m_DesamplingRate;
			}
			set
			{
				this.m_DesamplingRate = value;
			}
		}

		public UIEffectCapturedImage.DesamplingRate reductionRate
		{
			get
			{
				return this.m_ReductionRate;
			}
			set
			{
				this.m_ReductionRate = value;
			}
		}

		public FilterMode filterMode
		{
			get
			{
				return this.m_FilterMode;
			}
			set
			{
				this.m_FilterMode = value;
			}
		}

		public RenderTexture capturedTexture
		{
			get
			{
				return this._rt;
			}
		}

		public int iterations
		{
			get
			{
				return this.m_Iterations;
			}
			set
			{
				this.m_Iterations = value;
			}
		}

		public bool keepCanvasSize
		{
			get
			{
				return this.m_KeepCanvasSize;
			}
			set
			{
				this.m_KeepCanvasSize = value;
			}
		}

		protected override void OnDestroy()
		{
			this._Release(true);
			base.OnDestroy();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (base.texture == null || this.effectColor.a < 0.003921569f || base.canvasRenderer.GetAlpha() < 0.003921569f)
			{
				vh.Clear();
			}
			else
			{
				base.OnPopulateMesh(vh);
			}
		}

		public void GetDesamplingSize(UIEffectCapturedImage.DesamplingRate rate, out int w, out int h)
		{
			Camera camera = base.canvas.worldCamera ?? Camera.main;
			h = camera.pixelHeight;
			w = camera.pixelWidth;
			if (rate != UIEffectCapturedImage.DesamplingRate.None)
			{
				h = Mathf.ClosestPowerOfTwo(h / (int)rate);
				w = Mathf.ClosestPowerOfTwo(w / (int)rate);
			}
		}

		public void Capture()
		{
			this._camera = base.canvas.worldCamera ?? Camera.main;
			if (UIEffectCapturedImage.s_CopyId == 0)
			{
				UIEffectCapturedImage.s_CopyId = Shader.PropertyToID("_UIEffectCapturedImage_ScreenCopyId");
				UIEffectCapturedImage.s_EffectId1 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId1");
				UIEffectCapturedImage.s_EffectId2 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId2");
			}
			int num;
			int num2;
			this.GetDesamplingSize(this.m_DesamplingRate, out num, out num2);
			if (this._rt && (this._rt.width != num || this._rt.height != num2))
			{
				this._rtToRelease = this._rt;
				this._rt = null;
			}
			if (this._rt == null)
			{
				this._rt = new RenderTexture(num, num2, 0, 0, 0);
				this._rt.filterMode = this.m_FilterMode;
				this._rt.useMipMap = false;
				this._rt.wrapMode = 1;
				this._rt.hideFlags = 61;
			}
			if (this._buffer == null)
			{
				RenderTargetIdentifier renderTargetIdentifier;
				renderTargetIdentifier..ctor(this._rt);
				Material effectMaterial = this.effectMaterial;
				this._buffer = new CommandBuffer();
				CommandBuffer buffer = this._buffer;
				string text = ((!effectMaterial) ? "noeffect" : effectMaterial.name);
				this._rt.name = text;
				buffer.name = text;
				this._buffer.GetTemporaryRT(UIEffectCapturedImage.s_CopyId, -1, -1, 0, this.m_FilterMode);
				this._buffer.Blit(1, UIEffectCapturedImage.s_CopyId);
				this._buffer.SetGlobalVector("_EffectFactor", new Vector4(this.toneLevel, 0f, this.blur, 1f));
				this._buffer.SetGlobalVector("_ColorFactor", new Vector4(this.effectColor.r, this.effectColor.g, this.effectColor.b, this.effectColor.a));
				if (!effectMaterial)
				{
					this._buffer.Blit(UIEffectCapturedImage.s_CopyId, renderTargetIdentifier);
					this._buffer.ReleaseTemporaryRT(UIEffectCapturedImage.s_CopyId);
				}
				else
				{
					this.GetDesamplingSize(this.m_ReductionRate, out num, out num2);
					this._buffer.GetTemporaryRT(UIEffectCapturedImage.s_EffectId1, num, num2, 0, this.m_FilterMode);
					this._buffer.Blit(UIEffectCapturedImage.s_CopyId, UIEffectCapturedImage.s_EffectId1, effectMaterial);
					this._buffer.ReleaseTemporaryRT(UIEffectCapturedImage.s_CopyId);
					if (1 < this.m_Iterations)
					{
						this._buffer.SetGlobalVector("_EffectFactor", new Vector4(this.toneLevel, 0f, this.blur, 0f));
						this._buffer.GetTemporaryRT(UIEffectCapturedImage.s_EffectId2, num, num2, 0, this.m_FilterMode);
						for (int i = 1; i < this.m_Iterations; i++)
						{
							this._buffer.Blit((i % 2 != 0) ? UIEffectCapturedImage.s_EffectId1 : UIEffectCapturedImage.s_EffectId2, (i % 2 != 0) ? UIEffectCapturedImage.s_EffectId2 : UIEffectCapturedImage.s_EffectId1, effectMaterial);
						}
					}
					this._buffer.Blit((this.m_Iterations % 2 != 0) ? UIEffectCapturedImage.s_EffectId1 : UIEffectCapturedImage.s_EffectId2, renderTargetIdentifier);
					this._buffer.ReleaseTemporaryRT(UIEffectCapturedImage.s_EffectId1);
					if (1 < this.m_Iterations)
					{
						this._buffer.ReleaseTemporaryRT(UIEffectCapturedImage.s_EffectId2);
					}
				}
			}
			this._camera.AddCommandBuffer(20, this._buffer);
			Canvas rootCanvas = base.canvas.rootCanvas;
			CanvasScaler component = rootCanvas.GetComponent<CanvasScaler>();
			component.StartCoroutine(this._CoUpdateTextureOnNextFrame());
			if (this.m_KeepCanvasSize)
			{
				Vector2 size = (rootCanvas.transform as RectTransform).rect.size;
				base.rectTransform.SetSizeWithCurrentAnchors(0, size.x);
				base.rectTransform.SetSizeWithCurrentAnchors(1, size.y);
			}
		}

		public void Release()
		{
			this._Release(true);
		}

		private void _Release(bool releaseRT)
		{
			if (releaseRT)
			{
				base.texture = null;
				if (this._rt != null)
				{
					this._rt.Release();
					this._rt = null;
				}
			}
			if (this._buffer != null)
			{
				if (this._camera != null)
				{
					this._camera.RemoveCommandBuffer(20, this._buffer);
				}
				this._buffer.Release();
				this._buffer = null;
			}
			if (this._rtToRelease)
			{
				this._rtToRelease.Release();
				this._rtToRelease = null;
			}
		}

		private IEnumerator _CoUpdateTextureOnNextFrame()
		{
			yield return new WaitForEndOfFrame();
			this._Release(false);
			base.texture = this._rt;
			yield break;
		}

		public const string shaderName = "UI/Hidden/UI-EffectCapture";

		[SerializeField]
		[Range(0f, 1f)]
		private float m_ToneLevel = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Blur;

		[SerializeField]
		private UIEffect.ToneMode m_ToneMode;

		[SerializeField]
		private UIEffect.ColorMode m_ColorMode;

		[SerializeField]
		private UIEffect.BlurMode m_BlurMode;

		[SerializeField]
		private Color m_EffectColor = Color.white;

		[SerializeField]
		private UIEffectCapturedImage.DesamplingRate m_DesamplingRate;

		[SerializeField]
		private UIEffectCapturedImage.DesamplingRate m_ReductionRate;

		[SerializeField]
		private FilterMode m_FilterMode = 1;

		[SerializeField]
		private Material m_EffectMaterial;

		[SerializeField]
		[Range(1f, 8f)]
		private int m_Iterations = 1;

		[SerializeField]
		private bool m_KeepCanvasSize = true;

		private const CameraEvent kCameraEvent = 20;

		private Camera _camera;

		private RenderTexture _rt;

		private RenderTexture _rtToRelease;

		private CommandBuffer _buffer;

		private static int s_CopyId;

		private static int s_EffectId1;

		private static int s_EffectId2;

		public enum DesamplingRate
		{
			None,
			x1,
			x2,
			x4 = 4,
			x8 = 8
		}
	}
}
