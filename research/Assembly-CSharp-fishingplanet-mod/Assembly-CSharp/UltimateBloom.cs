using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class UltimateBloom : MonoBehaviour
{
	private void DestroyMaterial(Material mat)
	{
		if (mat)
		{
			Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	private void LoadShader(ref Material material, ref Shader shader, string shaderPath)
	{
		if (shader != null)
		{
			return;
		}
		shader = Shader.Find(shaderPath);
		if (shader == null)
		{
			Debug.LogError("Shader not found: " + shaderPath);
			return;
		}
		if (!shader.isSupported)
		{
			Debug.LogError("Shader contains error: " + shaderPath + "\n Maybe include path? Try rebuilding the shader.");
			return;
		}
		material = this.CreateMaterial(shader);
	}

	public void CreateMaterials()
	{
		int num = 8;
		if (this.m_BloomIntensities == null || this.m_BloomIntensities.Length < num)
		{
			this.m_BloomIntensities = new float[num];
			for (int i = 0; i < 8; i++)
			{
				this.m_BloomIntensities[i] = 1f;
			}
		}
		if (this.m_BloomColors == null || this.m_BloomColors.Length < num)
		{
			this.m_BloomColors = new Color[num];
			for (int j = 0; j < 8; j++)
			{
				this.m_BloomColors[j] = Color.white;
			}
		}
		if (this.m_BloomUsages == null || this.m_BloomUsages.Length < num)
		{
			this.m_BloomUsages = new bool[num];
			for (int k = 0; k < 8; k++)
			{
				this.m_BloomUsages[k] = true;
			}
		}
		if (this.m_AnamorphicBloomIntensities == null || this.m_AnamorphicBloomIntensities.Length < num)
		{
			this.m_AnamorphicBloomIntensities = new float[num];
			for (int l = 0; l < 8; l++)
			{
				this.m_AnamorphicBloomIntensities[l] = 1f;
			}
		}
		if (this.m_AnamorphicBloomColors == null || this.m_AnamorphicBloomColors.Length < num)
		{
			this.m_AnamorphicBloomColors = new Color[num];
			for (int m = 0; m < 8; m++)
			{
				this.m_AnamorphicBloomColors[m] = Color.white;
			}
		}
		if (this.m_AnamorphicBloomUsages == null || this.m_AnamorphicBloomUsages.Length < num)
		{
			this.m_AnamorphicBloomUsages = new bool[num];
			for (int n = 0; n < 8; n++)
			{
				this.m_AnamorphicBloomUsages[n] = true;
			}
		}
		if (this.m_StarBloomIntensities == null || this.m_StarBloomIntensities.Length < num)
		{
			this.m_StarBloomIntensities = new float[num];
			for (int num2 = 0; num2 < 8; num2++)
			{
				this.m_StarBloomIntensities[num2] = 1f;
			}
		}
		if (this.m_StarBloomColors == null || this.m_StarBloomColors.Length < num)
		{
			this.m_StarBloomColors = new Color[num];
			for (int num3 = 0; num3 < 8; num3++)
			{
				this.m_StarBloomColors[num3] = Color.white;
			}
		}
		if (this.m_StarBloomUsages == null || this.m_StarBloomUsages.Length < num)
		{
			this.m_StarBloomUsages = new bool[num];
			for (int num4 = 0; num4 < 8; num4++)
			{
				this.m_StarBloomUsages[num4] = true;
			}
		}
		if (this.m_FlareSpriteRenderer == null && this.m_FlareShape != null && this.m_UseBokehFlare)
		{
			if (this.m_FlareSpriteRenderer != null)
			{
				this.m_FlareSpriteRenderer.Clear(ref this.m_BokehMeshes);
			}
			this.m_FlareSpriteRenderer = new BokehRenderer();
		}
		if (this.m_SamplingMaterial == null)
		{
			this.m_DownSamples = new RenderTexture[this.GetNeededDownsamples()];
			this.m_UpSamples = new RenderTexture[this.m_DownscaleCount];
			this.m_AnamorphicUpscales = new RenderTexture[this.m_AnamorphicDownscaleCount];
			this.m_StarUpscales = new RenderTexture[this.m_StarDownscaleCount];
		}
		string text = ((this.m_FlareType != UltimateBloom.FlareType.Single) ? "Hidden/Ultimate/FlareDouble" : "Hidden/Ultimate/FlareSingle");
		this.LoadShader(ref this.m_FlareMaterial, ref this.m_FlareShader, text);
		this.LoadShader(ref this.m_SamplingMaterial, ref this.m_SamplingShader, "Hidden/Ultimate/Sampling");
		this.LoadShader(ref this.m_BrightpassMaterial, ref this.m_BrightpassShader, "Hidden/Ultimate/BrightpassMask");
		this.LoadShader(ref this.m_FlareMaskMaterial, ref this.m_FlareMaskShader, "Hidden/Ultimate/FlareMask");
		this.LoadShader(ref this.m_MixerMaterial, ref this.m_MixerShader, "Hidden/Ultimate/BloomMixer");
		this.LoadShader(ref this.m_FlareBokehMaterial, ref this.m_FlareBokehShader, "Hidden/Ultimate/FlareMesh");
		bool flag = this.m_UseLensDust || this.m_UseLensFlare || this.m_UseAnamorphicFlare || this.m_UseStarFlare;
		string text2 = "Hidden/Ultimate/BloomCombine";
		if (flag)
		{
			text2 = "Hidden/Ultimate/BloomCombineFlareDirt";
		}
		this.LoadShader(ref this.m_CombineMaterial, ref this.m_CombineShader, text2);
	}

	private Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = 61
		};
	}

	private void OnDisable()
	{
		this.ForceShadersReload();
		if (this.m_FlareSpriteRenderer != null)
		{
			this.m_FlareSpriteRenderer.Clear(ref this.m_BokehMeshes);
			this.m_FlareSpriteRenderer = null;
		}
	}

	public void ForceShadersReload()
	{
		this.DestroyMaterial(this.m_FlareMaterial);
		this.m_FlareMaterial = null;
		this.m_FlareShader = null;
		this.DestroyMaterial(this.m_SamplingMaterial);
		this.m_SamplingMaterial = null;
		this.m_SamplingShader = null;
		this.DestroyMaterial(this.m_CombineMaterial);
		this.m_CombineMaterial = null;
		this.m_CombineShader = null;
		this.DestroyMaterial(this.m_BrightpassMaterial);
		this.m_BrightpassMaterial = null;
		this.m_BrightpassShader = null;
		this.DestroyMaterial(this.m_FlareBokehMaterial);
		this.m_FlareBokehMaterial = null;
		this.m_FlareBokehShader = null;
		this.DestroyMaterial(this.m_FlareMaskMaterial);
		this.m_FlareMaskMaterial = null;
		this.m_FlareMaskShader = null;
		this.DestroyMaterial(this.m_MixerMaterial);
		this.m_MixerMaterial = null;
		this.m_MixerShader = null;
	}

	private int GetNeededDownsamples()
	{
		int num = Mathf.Max(this.m_DownscaleCount, (!this.m_UseAnamorphicFlare) ? 0 : this.m_AnamorphicDownscaleCount);
		num = Mathf.Max(num, (!this.m_UseLensFlare) ? 0 : (this.GetGhostBokehLayer() + 1));
		return Mathf.Max(num, (!this.m_UseStarFlare) ? 0 : this.m_StarDownscaleCount);
	}

	private void ComputeBufferOptimization()
	{
		if (this.m_BufferUsage == null)
		{
			this.m_BufferUsage = new bool[this.m_DownSamples.Length];
		}
		if (this.m_BufferUsage.Length != this.m_DownSamples.Length)
		{
			this.m_BufferUsage = new bool[this.m_DownSamples.Length];
		}
		for (int i = 0; i < this.m_BufferUsage.Length; i++)
		{
			this.m_BufferUsage[i] = false;
		}
		for (int j = 0; j < this.m_BufferUsage.Length; j++)
		{
			this.m_BufferUsage[j] = this.m_BloomUsages[j] || this.m_BufferUsage[j];
		}
		if (this.m_UseAnamorphicFlare)
		{
			for (int k = 0; k < this.m_BufferUsage.Length; k++)
			{
				this.m_BufferUsage[k] = this.m_AnamorphicBloomUsages[k] || this.m_BufferUsage[k];
			}
		}
		if (this.m_UseStarFlare)
		{
			for (int l = 0; l < this.m_BufferUsage.Length; l++)
			{
				this.m_BufferUsage[l] = this.m_StarBloomUsages[l] || this.m_BufferUsage[l];
			}
		}
	}

	private int GetGhostBokehLayer()
	{
		if (this.m_UseBokehFlare && this.m_FlareShape != null)
		{
			if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.VeryHigh)
			{
				return 1;
			}
			if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.High)
			{
				return 2;
			}
			if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.Medium)
			{
				return 3;
			}
			if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.Low)
			{
				return 4;
			}
		}
		return 0;
	}

	private UltimateBloom.BlurSampleCount GetUpsamplingSize()
	{
		if (this.m_SamplingMode == UltimateBloom.SamplingMode.Fixed)
		{
			UltimateBloom.BlurSampleCount blurSampleCount = UltimateBloom.BlurSampleCount.ThrirtyOne;
			if (this.m_UpsamplingQuality == UltimateBloom.BloomSamplingQuality.VerySmallKernel)
			{
				blurSampleCount = UltimateBloom.BlurSampleCount.Nine;
			}
			else if (this.m_UpsamplingQuality == UltimateBloom.BloomSamplingQuality.SmallKernel)
			{
				blurSampleCount = UltimateBloom.BlurSampleCount.Thirteen;
			}
			else if (this.m_UpsamplingQuality == UltimateBloom.BloomSamplingQuality.MediumKernel)
			{
				blurSampleCount = UltimateBloom.BlurSampleCount.Seventeen;
			}
			else if (this.m_UpsamplingQuality == UltimateBloom.BloomSamplingQuality.LargeKernel)
			{
				blurSampleCount = UltimateBloom.BlurSampleCount.TwentyThree;
			}
			else if (this.m_UpsamplingQuality == UltimateBloom.BloomSamplingQuality.LargerKernel)
			{
				blurSampleCount = UltimateBloom.BlurSampleCount.TwentySeven;
			}
			return blurSampleCount;
		}
		float num = (float)Screen.height;
		int num2 = 0;
		float num3 = float.MaxValue;
		for (int i = 0; i < this.m_ResSamplingPixelCount.Length; i++)
		{
			float num4 = Math.Abs(num - this.m_ResSamplingPixelCount[i]);
			if (num4 < num3)
			{
				num3 = num4;
				num2 = i;
			}
		}
		if (num2 == 0)
		{
			return UltimateBloom.BlurSampleCount.Nine;
		}
		if (num2 == 1)
		{
			return UltimateBloom.BlurSampleCount.Thirteen;
		}
		if (num2 == 2)
		{
			return UltimateBloom.BlurSampleCount.Seventeen;
		}
		if (num2 == 3)
		{
			return UltimateBloom.BlurSampleCount.TwentyThree;
		}
		if (num2 == 4)
		{
			return UltimateBloom.BlurSampleCount.TwentySeven;
		}
		return UltimateBloom.BlurSampleCount.ThrirtyOne;
	}

	public void ComputeResolutionRelativeData()
	{
		float num = this.m_SamplingMinHeight;
		float num2 = 9f;
		for (int i = 0; i < this.m_ResSamplingPixelCount.Length; i++)
		{
			this.m_ResSamplingPixelCount[i] = num;
			float num3 = num2 + 4f;
			float num4 = num3 / num2;
			num *= num4;
			num2 = num3;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		bool flag;
		if (this.m_HDR == UltimateBloom.HDRBloomMode.Auto)
		{
			flag = source.format == 2 && base.GetComponent<Camera>().allowHDR;
		}
		else
		{
			flag = this.m_HDR == UltimateBloom.HDRBloomMode.On;
		}
		this.m_Format = ((!flag) ? 7 : 2);
		if (this.m_DownSamples != null && this.m_DownSamples.Length != this.GetNeededDownsamples())
		{
			this.OnDisable();
		}
		if (this.m_LastDownscaleCount != this.m_DownscaleCount || this.m_LastAnamorphicDownscaleCount != this.m_AnamorphicDownscaleCount || this.m_LastStarDownscaleCount != this.m_StarDownscaleCount)
		{
			this.OnDisable();
		}
		this.m_LastDownscaleCount = this.m_DownscaleCount;
		this.m_LastAnamorphicDownscaleCount = this.m_AnamorphicDownscaleCount;
		this.m_LastStarDownscaleCount = this.m_StarDownscaleCount;
		this.CreateMaterials();
		if (this.m_DirectDownSample || this.m_DirectUpsample)
		{
			this.ComputeBufferOptimization();
		}
		bool flag2 = false;
		if (this.m_SamplingMode == UltimateBloom.SamplingMode.HeightRelative)
		{
			this.ComputeResolutionRelativeData();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, this.m_Format);
		temporary.filterMode = 1;
		if (this.m_IntensityManagement == UltimateBloom.BloomIntensityManagement.Threshold)
		{
			this.BrightPass(source, temporary, this.m_BloomThreshhold * this.m_BloomThreshholdColor);
		}
		else
		{
			this.m_BloomCurve.UpdateCoefficients();
			Graphics.Blit(source, temporary);
		}
		if (this.m_IntensityManagement == UltimateBloom.BloomIntensityManagement.Threshold)
		{
			this.CachedDownsample(temporary, this.m_DownSamples, null, flag);
		}
		else
		{
			this.CachedDownsample(temporary, this.m_DownSamples, this.m_BloomCurve, flag);
		}
		UltimateBloom.BlurSampleCount upsamplingSize = this.GetUpsamplingSize();
		this.CachedUpsample(this.m_DownSamples, this.m_UpSamples, source.width, source.height, upsamplingSize);
		Texture texture = Texture2D.blackTexture;
		RenderTexture renderTexture = null;
		if (this.m_UseLensFlare)
		{
			int ghostBokehLayer = this.GetGhostBokehLayer();
			int num = source.width / (int)Mathf.Pow(2f, (float)ghostBokehLayer);
			int num2 = source.height / (int)Mathf.Pow(2f, (float)ghostBokehLayer);
			if (this.m_FlareShape != null && this.m_UseBokehFlare)
			{
				float num3 = 15f;
				if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.Medium)
				{
					num3 *= 2f;
				}
				if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.High)
				{
					num3 *= 4f;
				}
				if (this.m_BokehFlareQuality == UltimateBloom.BokehFlareQuality.VeryHigh)
				{
					num3 *= 8f;
				}
				num3 *= this.m_BokehScale;
				this.m_FlareSpriteRenderer.SetMaterial(this.m_FlareBokehMaterial);
				this.m_FlareSpriteRenderer.RebuildMeshIfNeeded(num, num2, 1f / (float)num * num3, 1f / (float)num2 * num3, ref this.m_BokehMeshes);
				this.m_FlareSpriteRenderer.SetTexture(this.m_FlareShape);
				renderTexture = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, this.m_Format);
				int num4 = ghostBokehLayer;
				RenderTexture temporary2 = RenderTexture.GetTemporary(source.width / (int)Mathf.Pow(2f, (float)(num4 + 1)), source.height / (int)Mathf.Pow(2f, (float)(num4 + 1)), 0, this.m_Format);
				this.BrightPass(this.m_DownSamples[ghostBokehLayer], temporary2, this.m_FlareTreshold * Vector4.one);
				this.m_FlareSpriteRenderer.RenderFlare(temporary2, renderTexture, (!this.m_UseBokehFlare) ? this.m_FlareIntensity : 1f, ref this.m_BokehMeshes);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture temporary3 = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, this.m_Format);
				this.m_FlareMaskMaterial.SetTexture("_MaskTex", this.m_FlareMask);
				Graphics.Blit(renderTexture, temporary3, this.m_FlareMaskMaterial, 0);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = null;
				this.RenderFlares(temporary3, source, ref texture);
				RenderTexture.ReleaseTemporary(temporary3);
			}
			else
			{
				int ghostBokehLayer2 = this.GetGhostBokehLayer();
				RenderTexture renderTexture2 = this.m_DownSamples[ghostBokehLayer2];
				RenderTexture temporary4 = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, this.m_Format);
				this.BrightPassWithMask(this.m_DownSamples[ghostBokehLayer2], temporary4, this.m_FlareTreshold * Vector4.one, this.m_FlareMask);
				this.RenderFlares(temporary4, source, ref texture);
				RenderTexture.ReleaseTemporary(temporary4);
			}
		}
		if (!this.m_UseLensFlare && this.m_FlareSpriteRenderer != null)
		{
			this.m_FlareSpriteRenderer.Clear(ref this.m_BokehMeshes);
		}
		if (this.m_UseAnamorphicFlare)
		{
			RenderTexture renderTexture3 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.Anamorphic);
			if (renderTexture3 != null)
			{
				if (this.m_UseLensFlare)
				{
					this.RenderTextureAdditive(renderTexture3, (RenderTexture)texture, 1f);
					RenderTexture.ReleaseTemporary(renderTexture3);
				}
				else
				{
					texture = renderTexture3;
				}
			}
		}
		if (this.m_UseStarFlare)
		{
			if (this.m_StarBlurPass == 1)
			{
				RenderTexture renderTexture4 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.Star);
				if (renderTexture4 != null)
				{
					if (this.m_UseLensFlare || this.m_UseAnamorphicFlare)
					{
						this.RenderTextureAdditive(renderTexture4, (RenderTexture)texture, this.m_StarFlareIntensity);
					}
					else
					{
						texture = RenderTexture.GetTemporary(source.width, source.height, 0, this.m_Format);
						this.BlitIntensity(renderTexture4, (RenderTexture)texture, this.m_StarFlareIntensity);
					}
					RenderTexture.ReleaseTemporary(renderTexture4);
				}
			}
			else if (this.m_UseLensFlare || this.m_UseAnamorphicFlare)
			{
				RenderTexture renderTexture4 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.DiagonalUpright);
				if (renderTexture4 != null)
				{
					this.RenderTextureAdditive(renderTexture4, (RenderTexture)texture, this.m_StarFlareIntensity);
					RenderTexture.ReleaseTemporary(renderTexture4);
					renderTexture4 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.DiagonalUpleft);
					this.RenderTextureAdditive(renderTexture4, (RenderTexture)texture, this.m_StarFlareIntensity);
					RenderTexture.ReleaseTemporary(renderTexture4);
				}
			}
			else
			{
				RenderTexture renderTexture4 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.DiagonalUpleft);
				if (renderTexture4 != null)
				{
					RenderTexture renderTexture5 = this.RenderStripe(this.m_DownSamples, upsamplingSize, source.width, source.height, UltimateBloom.FlareStripeType.DiagonalUpright);
					this.CombineAdditive(renderTexture5, renderTexture4, this.m_StarFlareIntensity, this.m_StarFlareIntensity);
					RenderTexture.ReleaseTemporary(renderTexture5);
					texture = renderTexture4;
				}
			}
		}
		if (this.m_DirectDownSample)
		{
			for (int i = 0; i < this.m_DownSamples.Length; i++)
			{
				if (this.m_BufferUsage[i])
				{
					RenderTexture.ReleaseTemporary(this.m_DownSamples[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < this.m_DownSamples.Length; j++)
			{
				RenderTexture.ReleaseTemporary(this.m_DownSamples[j]);
			}
		}
		this.m_CombineMaterial.SetFloat("_Intensity", this.m_BloomIntensity);
		this.m_CombineMaterial.SetFloat("_FlareIntensity", this.m_FlareIntensity);
		this.m_CombineMaterial.SetTexture("_ColorBuffer", source);
		this.m_CombineMaterial.SetTexture("_FlareTexture", texture);
		this.m_CombineMaterial.SetTexture("_AdditiveTexture", (!this.m_UseLensDust) ? Texture2D.whiteTexture : this.m_DustTexture);
		this.m_CombineMaterial.SetTexture("_brightTexture", temporary);
		if (this.m_UseLensDust)
		{
			this.m_CombineMaterial.SetFloat("_DirtIntensity", this.m_DustIntensity);
			this.m_CombineMaterial.SetFloat("_DirtLightIntensity", this.m_DirtLightIntensity);
		}
		else
		{
			this.m_CombineMaterial.SetFloat("_DirtIntensity", 1f);
			this.m_CombineMaterial.SetFloat("_DirtLightIntensity", 0f);
		}
		if (this.m_BlendMode == UltimateBloom.BlendMode.SCREEN)
		{
			this.m_CombineMaterial.SetFloat("_ScreenMaxIntensity", this.m_ScreenMaxIntensity);
		}
		if (this.m_InvertImage)
		{
			Graphics.Blit(this.m_LastBloomUpsample, destination, this.m_CombineMaterial, 1);
		}
		else
		{
			Graphics.Blit(this.m_LastBloomUpsample, destination, this.m_CombineMaterial, 0);
		}
		for (int k = 0; k < this.m_UpSamples.Length; k++)
		{
			if (this.m_UpSamples[k] != null)
			{
				RenderTexture.ReleaseTemporary(this.m_UpSamples[k]);
			}
		}
		if (flag2)
		{
			Graphics.Blit(renderTexture, destination);
		}
		if ((this.m_UseLensFlare || this.m_UseAnamorphicFlare || this.m_UseStarFlare) && texture != null && texture is RenderTexture)
		{
			RenderTexture.ReleaseTemporary((RenderTexture)texture);
		}
		RenderTexture.ReleaseTemporary(temporary);
		if (this.m_FlareShape != null && this.m_UseBokehFlare && renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	private RenderTexture RenderStar(RenderTexture[] sources, UltimateBloom.BlurSampleCount upsamplingCount, int sourceWidth, int sourceHeight)
	{
		for (int i = this.m_StarUpscales.Length - 1; i >= 0; i--)
		{
			this.m_StarUpscales[i] = RenderTexture.GetTemporary(sourceWidth / (int)Mathf.Pow(2f, (float)i), sourceHeight / (int)Mathf.Pow(2f, (float)i), 0, this.m_Format);
			this.m_StarUpscales[i].filterMode = 1;
			float num = 1f / (float)sources[i].width;
			float num2 = 1f / (float)sources[i].height;
			if (i < this.m_StarDownscaleCount - 1)
			{
				this.GaussianBlur2(sources[i], this.m_StarUpscales[i], num * this.m_StarScale, num2 * this.m_StarScale, this.m_StarUpscales[i + 1], upsamplingCount, Color.white, 1f);
			}
			else
			{
				this.GaussianBlur2(sources[i], this.m_StarUpscales[i], num * this.m_StarScale, num2 * this.m_StarScale, null, upsamplingCount, Color.white, 1f);
			}
		}
		for (int j = 1; j < this.m_StarUpscales.Length; j++)
		{
			if (this.m_StarUpscales[j] != null)
			{
				RenderTexture.ReleaseTemporary(this.m_StarUpscales[j]);
			}
		}
		return this.m_StarUpscales[0];
	}

	private RenderTexture RenderStripe(RenderTexture[] sources, UltimateBloom.BlurSampleCount upsamplingCount, int sourceWidth, int sourceHeight, UltimateBloom.FlareStripeType type)
	{
		RenderTexture[] array = this.m_AnamorphicUpscales;
		bool[] array2 = this.m_AnamorphicBloomUsages;
		float[] array3 = this.m_AnamorphicBloomIntensities;
		Color[] array4 = this.m_AnamorphicBloomColors;
		bool flag = this.m_AnamorphicSmallVerticalBlur;
		float num = (float)this.m_AnamorphicBlurPass;
		float num2 = this.m_AnamorphicScale;
		float num3 = this.m_AnamorphicFlareIntensity;
		float num4 = 1f;
		float num5 = 0f;
		if (this.m_AnamorphicDirection == UltimateBloom.AnamorphicDirection.Vertical)
		{
			num4 = 0f;
			num5 = 1f;
		}
		if (type != UltimateBloom.FlareStripeType.Anamorphic)
		{
			array = this.m_StarUpscales;
			array2 = this.m_StarBloomUsages;
			array3 = this.m_StarBloomIntensities;
			array4 = this.m_StarBloomColors;
			flag = false;
			num = (float)this.m_StarBlurPass;
			num2 = this.m_StarScale;
			num3 = this.m_StarFlareIntensity;
			if (type == UltimateBloom.FlareStripeType.DiagonalUpleft)
			{
				num5 = -1f;
			}
			else
			{
				num5 = 1f;
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = null;
		}
		RenderTexture renderTexture = null;
		for (int j = array.Length - 1; j >= 0; j--)
		{
			if (!(sources[j] == null) || !this.m_DirectUpsample)
			{
				if (array2[j] || !this.m_DirectUpsample)
				{
					array[j] = RenderTexture.GetTemporary(sourceWidth / (int)Mathf.Pow(2f, (float)j), sourceHeight / (int)Mathf.Pow(2f, (float)j), 0, this.m_Format);
					array[j].filterMode = 1;
					float num6 = 1f / (float)array[j].width;
					float num7 = 1f / (float)array[j].height;
					RenderTexture renderTexture2 = sources[j];
					RenderTexture renderTexture3 = array[j];
					if (!array2[j])
					{
						if (renderTexture != null)
						{
							if (flag)
							{
								this.GaussianBlur1(renderTexture, renderTexture3, (this.m_AnamorphicDirection != UltimateBloom.AnamorphicDirection.Vertical) ? 0f : num6, (this.m_AnamorphicDirection != UltimateBloom.AnamorphicDirection.Horizontal) ? 0f : num7, null, UltimateBloom.BlurSampleCount.FourSimple, Color.white, 1f);
							}
							else
							{
								Graphics.Blit(renderTexture, renderTexture3);
							}
						}
						else
						{
							Graphics.Blit(Texture2D.blackTexture, renderTexture3);
						}
						renderTexture = array[j];
					}
					else
					{
						RenderTexture renderTexture4 = null;
						if (flag && renderTexture != null)
						{
							renderTexture4 = RenderTexture.GetTemporary(renderTexture3.width, renderTexture3.height, 0, this.m_Format);
							this.GaussianBlur1(renderTexture, renderTexture4, (this.m_AnamorphicDirection != UltimateBloom.AnamorphicDirection.Vertical) ? 0f : num6, (this.m_AnamorphicDirection != UltimateBloom.AnamorphicDirection.Horizontal) ? 0f : num7, null, UltimateBloom.BlurSampleCount.FourSimple, Color.white, 1f);
							renderTexture = renderTexture4;
						}
						if (num == 1f)
						{
							if (type != UltimateBloom.FlareStripeType.Anamorphic)
							{
								this.GaussianBlur2(renderTexture2, renderTexture3, num6 * num2 * num4, num7 * num2 * num5, renderTexture, upsamplingCount, array4[j], array3[j] * num3);
							}
							else
							{
								this.GaussianBlur1(renderTexture2, renderTexture3, num6 * num2 * num4, num7 * num2 * num5, renderTexture, upsamplingCount, array4[j], array3[j] * num3);
							}
						}
						else
						{
							RenderTexture temporary = RenderTexture.GetTemporary(renderTexture3.width, renderTexture3.height, 0, this.m_Format);
							bool flag2 = false;
							int num8 = 0;
							while ((float)num8 < num)
							{
								RenderTexture renderTexture5 = (((float)num8 != num - 1f) ? null : renderTexture);
								if (num8 == 0)
								{
									if (type != UltimateBloom.FlareStripeType.Anamorphic)
									{
										this.GaussianBlur2(renderTexture2, temporary, num6 * num2 * num4, num7 * num2 * num5, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
									}
									else
									{
										this.GaussianBlur1(renderTexture2, temporary, num6 * num2 * num4, num7 * num2 * num5, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
									}
								}
								else
								{
									num6 = 1f / (float)renderTexture3.width;
									num7 = 1f / (float)renderTexture3.height;
									if (num8 % 2 == 1)
									{
										if (type != UltimateBloom.FlareStripeType.Anamorphic)
										{
											this.GaussianBlur2(temporary, renderTexture3, num6 * num2 * num4 * 1.5f, num7 * num2 * num5 * 1.5f, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
										}
										else
										{
											this.GaussianBlur1(temporary, renderTexture3, num6 * num2 * num4 * 1.5f, num7 * num2 * num5 * 1.5f, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
										}
										flag2 = false;
									}
									else
									{
										if (type != UltimateBloom.FlareStripeType.Anamorphic)
										{
											this.GaussianBlur2(renderTexture3, temporary, num6 * num2 * num4 * 1.5f, num7 * num2 * num5 * 1.5f, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
										}
										else
										{
											this.GaussianBlur1(renderTexture3, temporary, num6 * num2 * num4 * 1.5f, num7 * num2 * num5 * 1.5f, renderTexture5, upsamplingCount, array4[j], array3[j] * num3);
										}
										flag2 = true;
									}
								}
								num8++;
							}
							if (flag2)
							{
								Graphics.Blit(temporary, renderTexture3);
							}
							if (renderTexture4 != null)
							{
								RenderTexture.ReleaseTemporary(renderTexture4);
							}
							RenderTexture.ReleaseTemporary(temporary);
						}
						renderTexture = array[j];
					}
				}
			}
		}
		RenderTexture renderTexture6 = null;
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != null)
			{
				if (renderTexture6 == null)
				{
					renderTexture6 = array[k];
				}
				else
				{
					RenderTexture.ReleaseTemporary(array[k]);
				}
			}
		}
		return renderTexture6;
	}

	private void RenderFlares(RenderTexture brightTexture, RenderTexture source, ref Texture flareRT)
	{
		flareRT = RenderTexture.GetTemporary(source.width, source.height, 0, this.m_Format);
		flareRT.filterMode = 1;
		this.m_FlareMaterial.SetVector("_FlareScales", this.m_FlareScales * this.m_FlareGlobalScale);
		this.m_FlareMaterial.SetVector("_FlareScalesNear", this.m_FlareScalesNear * this.m_FlareGlobalScale);
		this.m_FlareMaterial.SetVector("_FlareTint0", this.m_FlareTint0);
		this.m_FlareMaterial.SetVector("_FlareTint1", this.m_FlareTint1);
		this.m_FlareMaterial.SetVector("_FlareTint2", this.m_FlareTint2);
		this.m_FlareMaterial.SetVector("_FlareTint3", this.m_FlareTint3);
		this.m_FlareMaterial.SetVector("_FlareTint4", this.m_FlareTint4);
		this.m_FlareMaterial.SetVector("_FlareTint5", this.m_FlareTint5);
		this.m_FlareMaterial.SetVector("_FlareTint6", this.m_FlareTint6);
		this.m_FlareMaterial.SetVector("_FlareTint7", this.m_FlareTint7);
		this.m_FlareMaterial.SetFloat("_Intensity", this.m_FlareIntensity);
		if (this.m_FlareRendering == UltimateBloom.FlareRendering.Sharp)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, this.m_Format);
			temporary.filterMode = 1;
			this.RenderSimple(brightTexture, temporary, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, UltimateBloom.SimpleSampleCount.Four);
			Graphics.Blit(temporary, (RenderTexture)flareRT, this.m_FlareMaterial, 0);
			RenderTexture.ReleaseTemporary(temporary);
			return;
		}
		if (this.m_FlareBlurQuality == UltimateBloom.FlareBlurQuality.Fast)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, this.m_Format);
			temporary2.filterMode = 1;
			RenderTexture temporary3 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, this.m_Format);
			temporary3.filterMode = 1;
			Graphics.Blit(brightTexture, temporary2, this.m_FlareMaterial, 0);
			if (this.m_FlareRendering == UltimateBloom.FlareRendering.Blurred)
			{
				this.GaussianBlurSeparate(temporary2, temporary3, 1f / (float)temporary2.width, 1f / (float)temporary2.height, null, UltimateBloom.BlurSampleCount.Thirteen, Color.white, 1f);
				this.RenderSimple(temporary3, (RenderTexture)flareRT, 1f / (float)temporary3.width, 1f / (float)temporary3.height, UltimateBloom.SimpleSampleCount.Four);
			}
			else if (this.m_FlareRendering == UltimateBloom.FlareRendering.MoreBlurred)
			{
				this.GaussianBlurSeparate(temporary2, temporary3, 1f / (float)temporary2.width, 1f / (float)temporary2.height, null, UltimateBloom.BlurSampleCount.ThrirtyOne, Color.white, 1f);
				this.RenderSimple(temporary3, (RenderTexture)flareRT, 1f / (float)temporary3.width, 1f / (float)temporary3.height, UltimateBloom.SimpleSampleCount.Four);
			}
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
			return;
		}
		if (this.m_FlareBlurQuality == UltimateBloom.FlareBlurQuality.Normal)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, this.m_Format);
			temporary4.filterMode = 1;
			RenderTexture temporary5 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, this.m_Format);
			temporary5.filterMode = 1;
			RenderTexture temporary6 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, this.m_Format);
			temporary6.filterMode = 1;
			this.RenderSimple(brightTexture, temporary4, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, UltimateBloom.SimpleSampleCount.Four);
			this.RenderSimple(temporary4, temporary5, 1f / (float)temporary4.width, 1f / (float)temporary4.height, UltimateBloom.SimpleSampleCount.Four);
			Graphics.Blit(temporary5, temporary6, this.m_FlareMaterial, 0);
			if (this.m_FlareRendering == UltimateBloom.FlareRendering.Blurred)
			{
				this.GaussianBlurSeparate(temporary6, temporary5, 1f / (float)temporary5.width, 1f / (float)temporary5.height, null, UltimateBloom.BlurSampleCount.Thirteen, Color.white, 1f);
				this.RenderSimple(temporary5, (RenderTexture)flareRT, 1f / (float)temporary5.width, 1f / (float)temporary5.height, UltimateBloom.SimpleSampleCount.Four);
			}
			else if (this.m_FlareRendering == UltimateBloom.FlareRendering.MoreBlurred)
			{
				this.GaussianBlurSeparate(temporary6, temporary5, 1f / (float)temporary5.width, 1f / (float)temporary5.height, null, UltimateBloom.BlurSampleCount.ThrirtyOne, Color.white, 1f);
				this.RenderSimple(temporary5, (RenderTexture)flareRT, 1f / (float)temporary5.width, 1f / (float)temporary5.height, UltimateBloom.SimpleSampleCount.Four);
			}
			RenderTexture.ReleaseTemporary(temporary4);
			RenderTexture.ReleaseTemporary(temporary5);
			RenderTexture.ReleaseTemporary(temporary6);
		}
		else if (this.m_FlareBlurQuality == UltimateBloom.FlareBlurQuality.High)
		{
			RenderTexture temporary7 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, this.m_Format);
			temporary7.filterMode = 1;
			RenderTexture temporary8 = RenderTexture.GetTemporary(temporary7.width / 2, temporary7.height / 2, 0, this.m_Format);
			temporary8.filterMode = 1;
			RenderTexture temporary9 = RenderTexture.GetTemporary(temporary8.width / 2, temporary8.height / 2, 0, this.m_Format);
			temporary9.filterMode = 1;
			RenderTexture temporary10 = RenderTexture.GetTemporary(temporary8.width / 2, temporary8.height / 2, 0, this.m_Format);
			temporary10.filterMode = 1;
			this.RenderSimple(brightTexture, temporary7, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, UltimateBloom.SimpleSampleCount.Four);
			this.RenderSimple(temporary7, temporary8, 1f / (float)temporary7.width, 1f / (float)temporary7.height, UltimateBloom.SimpleSampleCount.Four);
			this.RenderSimple(temporary8, temporary9, 1f / (float)temporary8.width, 1f / (float)temporary8.height, UltimateBloom.SimpleSampleCount.Four);
			Graphics.Blit(temporary9, temporary10, this.m_FlareMaterial, 0);
			if (this.m_FlareRendering == UltimateBloom.FlareRendering.Blurred)
			{
				this.GaussianBlurSeparate(temporary10, temporary9, 1f / (float)temporary9.width, 1f / (float)temporary9.height, null, UltimateBloom.BlurSampleCount.Thirteen, Color.white, 1f);
				this.RenderSimple(temporary9, (RenderTexture)flareRT, 1f / (float)temporary9.width, 1f / (float)temporary9.height, UltimateBloom.SimpleSampleCount.Four);
			}
			else if (this.m_FlareRendering == UltimateBloom.FlareRendering.MoreBlurred)
			{
				this.GaussianBlurSeparate(temporary10, temporary9, 1f / (float)temporary9.width, 1f / (float)temporary9.height, null, UltimateBloom.BlurSampleCount.ThrirtyOne, Color.white, 1f);
				this.RenderSimple(temporary9, (RenderTexture)flareRT, 1f / (float)temporary9.width, 1f / (float)temporary9.height, UltimateBloom.SimpleSampleCount.Four);
			}
			RenderTexture.ReleaseTemporary(temporary7);
			RenderTexture.ReleaseTemporary(temporary8);
			RenderTexture.ReleaseTemporary(temporary9);
			RenderTexture.ReleaseTemporary(temporary10);
		}
	}

	private void CachedUpsample(RenderTexture[] sources, RenderTexture[] destinations, int originalWidth, int originalHeight, UltimateBloom.BlurSampleCount upsamplingCount)
	{
		RenderTexture renderTexture = null;
		for (int i = 0; i < this.m_UpSamples.Length; i++)
		{
			this.m_UpSamples[i] = null;
		}
		for (int j = destinations.Length - 1; j >= 0; j--)
		{
			if (this.m_BloomUsages[j] || !this.m_DirectUpsample)
			{
				this.m_UpSamples[j] = RenderTexture.GetTemporary(originalWidth / (int)Mathf.Pow(2f, (float)j), originalHeight / (int)Mathf.Pow(2f, (float)j), 0, this.m_Format);
				this.m_UpSamples[j].filterMode = 1;
			}
			float num = 1f;
			if (this.m_BloomUsages[j])
			{
				float num2 = 1f / (float)sources[j].width;
				float num3 = 1f / (float)sources[j].height;
				this.GaussianBlurSeparate(this.m_DownSamples[j], this.m_UpSamples[j], num2 * num, num3, renderTexture, upsamplingCount, this.m_BloomColors[j], this.m_BloomIntensities[j]);
			}
			else if (j < this.m_DownscaleCount - 1)
			{
				if (!this.m_DirectUpsample)
				{
					this.RenderSimple(renderTexture, this.m_UpSamples[j], 1f / (float)this.m_UpSamples[j].width, 1f / (float)this.m_UpSamples[j].height, UltimateBloom.SimpleSampleCount.Four);
				}
			}
			else
			{
				Graphics.Blit(Texture2D.blackTexture, this.m_UpSamples[j]);
			}
			if (this.m_BloomUsages[j] || !this.m_DirectUpsample)
			{
				renderTexture = this.m_UpSamples[j];
			}
		}
		this.m_LastBloomUpsample = renderTexture;
	}

	private void CachedDownsample(RenderTexture source, RenderTexture[] destinations, DeluxeFilmicCurve intensityCurve, bool hdr)
	{
		int num = destinations.Length;
		RenderTexture renderTexture = source;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			if (!this.m_DirectDownSample || this.m_BufferUsage[i])
			{
				destinations[i] = RenderTexture.GetTemporary(source.width / (int)Mathf.Pow(2f, (float)(i + 1)), source.height / (int)Mathf.Pow(2f, (float)(i + 1)), 0, this.m_Format);
				destinations[i].filterMode = 1;
				RenderTexture renderTexture2 = destinations[i];
				float num2 = 1f;
				float num3 = 1f / (float)renderTexture.width;
				float num4 = 1f / (float)renderTexture.height;
				if (intensityCurve != null && !flag)
				{
					intensityCurve.StoreK();
					this.m_SamplingMaterial.SetFloat("_CurveExposure", intensityCurve.GetExposure());
					this.m_SamplingMaterial.SetFloat("_K", intensityCurve.m_k);
					this.m_SamplingMaterial.SetFloat("_Crossover", intensityCurve.m_CrossOverPoint);
					this.m_SamplingMaterial.SetVector("_Toe", intensityCurve.m_ToeCoef);
					this.m_SamplingMaterial.SetVector("_Shoulder", intensityCurve.m_ShoulderCoef);
					float num5 = ((!hdr) ? 1f : 2f);
					this.m_SamplingMaterial.SetFloat("_MaxValue", num5);
					num3 = 1f / (float)renderTexture.width;
					num4 = 1f / (float)renderTexture.height;
					if (this.m_TemporalStableDownsampling)
					{
						this.RenderSimple(renderTexture, renderTexture2, num3 * num2, num4 * num2, UltimateBloom.SimpleSampleCount.ThirteenTemporalCurve);
					}
					else
					{
						this.RenderSimple(renderTexture, renderTexture2, num3 * num2, num4 * num2, UltimateBloom.SimpleSampleCount.FourCurve);
					}
					flag = true;
				}
				else if (this.m_TemporalStableDownsampling)
				{
					this.RenderSimple(renderTexture, renderTexture2, num3 * num2, num4 * num2, UltimateBloom.SimpleSampleCount.ThirteenTemporal);
				}
				else
				{
					this.RenderSimple(renderTexture, renderTexture2, num3 * num2, num4 * num2, UltimateBloom.SimpleSampleCount.Four);
				}
				renderTexture = destinations[i];
			}
		}
	}

	private void BrightPass(RenderTexture source, RenderTexture destination, Vector4 treshold)
	{
		this.m_BrightpassMaterial.SetTexture("_MaskTex", Texture2D.whiteTexture);
		this.m_BrightpassMaterial.SetVector("_Threshhold", treshold);
		Graphics.Blit(source, destination, this.m_BrightpassMaterial, 0);
	}

	private void BrightPassWithMask(RenderTexture source, RenderTexture destination, Vector4 treshold, Texture mask)
	{
		this.m_BrightpassMaterial.SetTexture("_MaskTex", mask);
		this.m_BrightpassMaterial.SetVector("_Threshhold", treshold);
		Graphics.Blit(source, destination, this.m_BrightpassMaterial, 0);
	}

	private void RenderSimple(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, UltimateBloom.SimpleSampleCount sampleCount)
	{
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
		if (sampleCount == UltimateBloom.SimpleSampleCount.Four)
		{
			Graphics.Blit(source, destination, this.m_SamplingMaterial, 0);
		}
		else if (sampleCount == UltimateBloom.SimpleSampleCount.Nine)
		{
			Graphics.Blit(source, destination, this.m_SamplingMaterial, 1);
		}
		else if (sampleCount == UltimateBloom.SimpleSampleCount.FourCurve)
		{
			Graphics.Blit(source, destination, this.m_SamplingMaterial, 5);
		}
		else if (sampleCount == UltimateBloom.SimpleSampleCount.ThirteenTemporal)
		{
			Graphics.Blit(source, destination, this.m_SamplingMaterial, 11);
		}
		else if (sampleCount == UltimateBloom.SimpleSampleCount.ThirteenTemporalCurve)
		{
			Graphics.Blit(source, destination, this.m_SamplingMaterial, 12);
		}
	}

	private void GaussianBlur1(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, UltimateBloom.BlurSampleCount sampleCount, Color tint, float intensity)
	{
		int num = 2;
		if (sampleCount == UltimateBloom.BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		Texture texture;
		if (additiveTexture == null)
		{
			texture = Texture2D.blackTexture;
		}
		else
		{
			texture = additiveTexture;
		}
		this.m_SamplingMaterial.SetTexture("_AdditiveTexture", texture);
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
		this.m_SamplingMaterial.SetVector("_Tint", tint);
		this.m_SamplingMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, destination, this.m_SamplingMaterial, num);
	}

	private void GaussianBlur2(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, UltimateBloom.BlurSampleCount sampleCount, Color tint, float intensity)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(destination.width, destination.height, destination.depth, destination.format);
		temporary.filterMode = 1;
		int num = 2;
		if (sampleCount == UltimateBloom.BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		Texture texture;
		if (additiveTexture == null)
		{
			texture = Texture2D.blackTexture;
		}
		else
		{
			texture = additiveTexture;
		}
		this.m_SamplingMaterial.SetTexture("_AdditiveTexture", texture);
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
		this.m_SamplingMaterial.SetVector("_Tint", tint);
		this.m_SamplingMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, temporary, this.m_SamplingMaterial, num);
		texture = temporary;
		this.m_SamplingMaterial.SetTexture("_AdditiveTexture", texture);
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(-horizontalBlur, verticalBlur, 0f, 0f));
		this.m_SamplingMaterial.SetVector("_Tint", tint);
		this.m_SamplingMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, destination, this.m_SamplingMaterial, num);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void GaussianBlurSeparate(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, UltimateBloom.BlurSampleCount sampleCount, Color tint, float intensity)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(destination.width, destination.height, destination.depth, destination.format);
		temporary.filterMode = 1;
		int num = 2;
		if (sampleCount == UltimateBloom.BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == UltimateBloom.BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		this.m_SamplingMaterial.SetTexture("_AdditiveTexture", Texture2D.blackTexture);
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(0f, verticalBlur, 0f, 0f));
		this.m_SamplingMaterial.SetVector("_Tint", tint);
		this.m_SamplingMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, temporary, this.m_SamplingMaterial, num);
		Texture texture;
		if (additiveTexture == null)
		{
			texture = Texture2D.blackTexture;
		}
		else
		{
			texture = additiveTexture;
		}
		this.m_SamplingMaterial.SetTexture("_AdditiveTexture", texture);
		this.m_SamplingMaterial.SetVector("_OffsetInfos", new Vector4(horizontalBlur, 0f, 1f / (float)destination.width, 1f / (float)destination.height));
		this.m_SamplingMaterial.SetVector("_Tint", Color.white);
		this.m_SamplingMaterial.SetFloat("_Intensity", 1f);
		Graphics.Blit(temporary, destination, this.m_SamplingMaterial, num);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void RenderTextureAdditive(RenderTexture source, RenderTexture destination, float intensity)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
		Graphics.Blit(destination, temporary);
		this.m_MixerMaterial.SetTexture("_ColorBuffer", temporary);
		this.m_MixerMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, destination, this.m_MixerMaterial, 0);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void BlitIntensity(RenderTexture source, RenderTexture destination, float intensity)
	{
		this.m_MixerMaterial.SetFloat("_Intensity", intensity);
		Graphics.Blit(source, destination, this.m_MixerMaterial, 2);
	}

	private void CombineAdditive(RenderTexture source, RenderTexture destination, float intensitySource, float intensityDestination)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
		Graphics.Blit(destination, temporary);
		this.m_MixerMaterial.SetTexture("_ColorBuffer", temporary);
		this.m_MixerMaterial.SetFloat("_Intensity0", intensitySource);
		this.m_MixerMaterial.SetFloat("_Intensity1", intensityDestination);
		Graphics.Blit(source, destination, this.m_MixerMaterial, 1);
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void SetFilmicCurveParameters(float middle, float dark, float bright, float highlights)
	{
		this.m_BloomCurve.m_ToeStrength = -1f * dark;
		this.m_BloomCurve.m_ShoulderStrength = bright;
		this.m_BloomCurve.m_Highlights = highlights;
		this.m_BloomCurve.m_CrossOverPoint = middle;
		this.m_BloomCurve.UpdateCoefficients();
	}

	public float m_SamplingMinHeight = 400f;

	public float[] m_ResSamplingPixelCount = new float[6];

	public UltimateBloom.SamplingMode m_SamplingMode;

	public UltimateBloom.BlendMode m_BlendMode;

	public float m_ScreenMaxIntensity;

	public UltimateBloom.BloomQualityPreset m_QualityPreset;

	public UltimateBloom.HDRBloomMode m_HDR;

	public UltimateBloom.BloomScreenBlendMode m_ScreenBlendMode = UltimateBloom.BloomScreenBlendMode.Add;

	public float m_BloomIntensity = 1f;

	public float m_BloomThreshhold = 0.5f;

	public Color m_BloomThreshholdColor = Color.white;

	public int m_DownscaleCount = 5;

	public UltimateBloom.BloomIntensityManagement m_IntensityManagement;

	public float[] m_BloomIntensities;

	public Color[] m_BloomColors;

	public bool[] m_BloomUsages;

	[SerializeField]
	public DeluxeFilmicCurve m_BloomCurve = new DeluxeFilmicCurve();

	private int m_LastDownscaleCount = 5;

	public bool m_UseLensFlare;

	public float m_FlareTreshold = 0.8f;

	public float m_FlareIntensity = 0.25f;

	public Color m_FlareTint0 = new Color(0.5372549f, 0.32156864f, 0f);

	public Color m_FlareTint1 = new Color(0f, 0.24705882f, 0.49411765f);

	public Color m_FlareTint2 = new Color(0.28235295f, 0.5921569f, 0f);

	public Color m_FlareTint3 = new Color(0.44705883f, 0.13725491f, 0f);

	public Color m_FlareTint4 = new Color(0.47843137f, 0.34509805f, 0f);

	public Color m_FlareTint5 = new Color(0.5372549f, 0.2784314f, 0f);

	public Color m_FlareTint6 = new Color(0.38039216f, 0.54509807f, 0f);

	public Color m_FlareTint7 = new Color(0.15686275f, 0.5568628f, 0f);

	public float m_FlareGlobalScale = 1f;

	public Vector4 m_FlareScales = new Vector4(1f, 0.6f, 0.5f, 0.4f);

	public Vector4 m_FlareScalesNear = new Vector4(1f, 0.8f, 0.6f, 0.5f);

	public Texture2D m_FlareMask;

	public UltimateBloom.FlareRendering m_FlareRendering = UltimateBloom.FlareRendering.Blurred;

	public UltimateBloom.FlareType m_FlareType = UltimateBloom.FlareType.Double;

	public Texture2D m_FlareShape;

	public UltimateBloom.FlareBlurQuality m_FlareBlurQuality = UltimateBloom.FlareBlurQuality.High;

	private BokehRenderer m_FlareSpriteRenderer;

	private Mesh[] m_BokehMeshes;

	public bool m_UseBokehFlare;

	public float m_BokehScale = 0.4f;

	public UltimateBloom.BokehFlareQuality m_BokehFlareQuality = UltimateBloom.BokehFlareQuality.Medium;

	public bool m_UseAnamorphicFlare;

	public float m_AnamorphicFlareTreshold = 0.8f;

	public float m_AnamorphicFlareIntensity = 1f;

	public int m_AnamorphicDownscaleCount = 3;

	public int m_AnamorphicBlurPass = 2;

	private int m_LastAnamorphicDownscaleCount;

	private RenderTexture[] m_AnamorphicUpscales;

	public float[] m_AnamorphicBloomIntensities;

	public Color[] m_AnamorphicBloomColors;

	public bool[] m_AnamorphicBloomUsages;

	public bool m_AnamorphicSmallVerticalBlur = true;

	public UltimateBloom.AnamorphicDirection m_AnamorphicDirection;

	public float m_AnamorphicScale = 3f;

	public bool m_UseStarFlare;

	public float m_StarFlareTreshol = 0.8f;

	public float m_StarFlareIntensity = 1f;

	public float m_StarScale = 2f;

	public int m_StarDownscaleCount = 3;

	public int m_StarBlurPass = 2;

	private int m_LastStarDownscaleCount;

	private RenderTexture[] m_StarUpscales;

	public float[] m_StarBloomIntensities;

	public Color[] m_StarBloomColors;

	public bool[] m_StarBloomUsages;

	public bool m_UseLensDust;

	public float m_DustIntensity = 1f;

	public Texture2D m_DustTexture;

	public float m_DirtLightIntensity = 5f;

	public UltimateBloom.BloomSamplingQuality m_DownsamplingQuality;

	public UltimateBloom.BloomSamplingQuality m_UpsamplingQuality;

	public bool m_TemporalStableDownsampling = true;

	public bool m_InvertImage;

	private Material m_FlareMaterial;

	private Shader m_FlareShader;

	private Material m_SamplingMaterial;

	private Shader m_SamplingShader;

	private Material m_CombineMaterial;

	private Shader m_CombineShader;

	private Material m_BrightpassMaterial;

	private Shader m_BrightpassShader;

	private Material m_FlareMaskMaterial;

	private Shader m_FlareMaskShader;

	private Material m_MixerMaterial;

	private Shader m_MixerShader;

	private Material m_FlareBokehMaterial;

	private Shader m_FlareBokehShader;

	public bool m_DirectDownSample;

	public bool m_DirectUpsample;

	public bool m_UiShowBloomScales;

	public bool m_UiShowAnamorphicBloomScales;

	public bool m_UiShowStarBloomScales;

	public bool m_UiShowHeightSampling;

	public bool m_UiShowBloomSettings;

	public bool m_UiShowSampling;

	public bool m_UiShowIntensity;

	public bool m_UiShowOptimizations;

	public bool m_UiShowLensDirt;

	public bool m_UiShowLensFlare;

	public bool m_UiShowAnamorphic;

	public bool m_UiShowStar;

	private RenderTexture[] m_DownSamples;

	private RenderTexture[] m_UpSamples;

	private RenderTextureFormat m_Format;

	private bool[] m_BufferUsage;

	private RenderTexture m_LastBloomUpsample;

	public enum BloomQualityPreset
	{
		Optimized,
		Standard,
		HighVisuals,
		Custom
	}

	public enum BloomSamplingQuality
	{
		VerySmallKernel,
		SmallKernel,
		MediumKernel,
		LargeKernel,
		LargerKernel,
		VeryLargeKernel
	}

	public enum BloomScreenBlendMode
	{
		Screen,
		Add
	}

	public enum HDRBloomMode
	{
		Auto,
		On,
		Off
	}

	public enum BlurSampleCount
	{
		Nine,
		Seventeen,
		Thirteen,
		TwentyThree,
		TwentySeven,
		ThrirtyOne,
		NineCurve,
		FourSimple
	}

	public enum FlareRendering
	{
		Sharp,
		Blurred,
		MoreBlurred
	}

	public enum SimpleSampleCount
	{
		Four,
		Nine,
		FourCurve,
		ThirteenTemporal,
		ThirteenTemporalCurve
	}

	public enum FlareType
	{
		Single,
		Double
	}

	public enum BloomIntensityManagement
	{
		FilmicCurve,
		Threshold
	}

	private enum FlareStripeType
	{
		Anamorphic,
		Star,
		DiagonalUpright,
		DiagonalUpleft
	}

	public enum AnamorphicDirection
	{
		Horizontal,
		Vertical
	}

	public enum BokehFlareQuality
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	public enum BlendMode
	{
		ADD,
		SCREEN
	}

	public enum SamplingMode
	{
		Fixed,
		HeightRelative
	}

	public enum FlareBlurQuality
	{
		Fast,
		Normal,
		High
	}

	public enum FlarePresets
	{
		ChoosePreset,
		GhostFast,
		Ghost1,
		Ghost2,
		Ghost3,
		Bokeh1,
		Bokeh2,
		Bokeh3
	}

	private delegate void BlurFunction(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, UltimateBloom.BlurSampleCount sampleCount, Color tint, float intensity);
}
