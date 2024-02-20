using System;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
	public sealed class FogComponent : PostProcessingComponentCommandBuffer<FogModel>
	{
		public override bool active
		{
			get
			{
				return base.model.enabled && this.context.isGBufferAvailable && RenderSettings.fog && !this.context.interrupted;
			}
		}

		public override string GetName()
		{
			return "Fog";
		}

		public override DepthTextureMode GetCameraFlags()
		{
			return 1;
		}

		public override CameraEvent GetCameraEvent()
		{
			return 13;
		}

		public override void PopulateCommandBuffer(CommandBuffer cb)
		{
			FogModel.Settings settings = base.model.settings;
			Material material = this.context.materialFactory.Get("Hidden/Post FX/Fog");
			material.shaderKeywords = null;
			Color color = ((!GraphicsUtils.isLinearColorSpace) ? RenderSettings.fogColor : RenderSettings.fogColor.linear);
			material.SetColor(FogComponent.Uniforms._FogColor, color);
			material.SetFloat(FogComponent.Uniforms._Density, RenderSettings.fogDensity);
			material.SetFloat(FogComponent.Uniforms._Start, RenderSettings.fogStartDistance);
			material.SetFloat(FogComponent.Uniforms._End, RenderSettings.fogEndDistance);
			FogMode fogMode = RenderSettings.fogMode;
			if (fogMode != 1)
			{
				if (fogMode != 2)
				{
					if (fogMode == 3)
					{
						material.EnableKeyword("FOG_EXP2");
					}
				}
				else
				{
					material.EnableKeyword("FOG_EXP");
				}
			}
			else
			{
				material.EnableKeyword("FOG_LINEAR");
			}
			RenderTextureFormat renderTextureFormat = ((!this.context.isHdr) ? 7 : 9);
			cb.GetTemporaryRT(FogComponent.Uniforms._TempRT, this.context.width, this.context.height, 24, 1, renderTextureFormat);
			cb.Blit(2, FogComponent.Uniforms._TempRT);
			cb.Blit(FogComponent.Uniforms._TempRT, 2, material, (!settings.excludeSkybox) ? 0 : 1);
			cb.ReleaseTemporaryRT(FogComponent.Uniforms._TempRT);
		}

		private const string k_ShaderString = "Hidden/Post FX/Fog";

		private static class Uniforms
		{
			internal static readonly int _FogColor = Shader.PropertyToID("_FogColor");

			internal static readonly int _Density = Shader.PropertyToID("_Density");

			internal static readonly int _Start = Shader.PropertyToID("_Start");

			internal static readonly int _End = Shader.PropertyToID("_End");

			internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
		}
	}
}
