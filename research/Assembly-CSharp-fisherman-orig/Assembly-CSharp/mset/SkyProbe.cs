using System;
using UnityEngine;

namespace mset
{
	public class SkyProbe
	{
		public SkyProbe()
		{
			SkyProbe.buildRandomValueTable();
		}

		public static void buildRandomValueTable()
		{
			if (SkyProbe.randomValues == null)
			{
				float num = (float)SkyProbe.sampleCount;
				SkyProbe.randomValues = new Vector4[SkyProbe.sampleCount];
				float[] array = new float[SkyProbe.sampleCount];
				for (int i = 0; i < SkyProbe.sampleCount; i++)
				{
					SkyProbe.randomValues[i] = default(Vector4);
					array[i] = (SkyProbe.randomValues[i].x = (float)(i + 1) / num);
				}
				int num2 = SkyProbe.sampleCount;
				for (int j = 0; j < SkyProbe.sampleCount; j++)
				{
					int num3 = Random.Range(0, num2 - 1);
					float num4 = array[num3];
					array[num3] = array[--num2];
					SkyProbe.randomValues[j].y = num4;
					SkyProbe.randomValues[j].z = Mathf.Cos(6.2831855f * num4);
					SkyProbe.randomValues[j].w = Mathf.Sin(6.2831855f * num4);
				}
			}
		}

		public static void bindRandomValueTable(Material mat, string paramName, int inputFaceSize)
		{
			for (int i = 0; i < SkyProbe.sampleCount; i++)
			{
				mat.SetVector(paramName + i, SkyProbe.randomValues[i]);
			}
			float num = (float)(inputFaceSize * inputFaceSize) / (float)SkyProbe.sampleCount;
			num = 0.5f * Mathf.Log(num, 2f) + 0.5f;
			mat.SetFloat("_ImportantLog", num);
		}

		public static void buildRandomValueCode()
		{
		}

		public void blur(Cubemap targetCube, Texture sourceCube, bool dstRGBM, bool srcRGBM, bool linear)
		{
			if (sourceCube == null || targetCube == null)
			{
				return;
			}
			GameObject gameObject = new GameObject("_temp_probe");
			gameObject.hideFlags = 61;
			gameObject.SetActive(true);
			Camera camera = gameObject.AddComponent<Camera>();
			camera.renderingPath = this.renderPath;
			camera.useOcclusionCulling = false;
			Material material = new Material(Shader.Find("Hidden/Marmoset/RGBM Cube"));
			Matrix4x4 identity = Matrix4x4.identity;
			int num = this.maxExponent;
			bool flag = this.generateMipChain;
			this.maxExponent = 8 * num;
			this.generateMipChain = false;
			this.convolve_internal(targetCube, sourceCube, dstRGBM, srcRGBM, linear, camera, material, identity);
			this.convolve_internal(targetCube, targetCube, dstRGBM, dstRGBM, linear, camera, material, identity);
			this.maxExponent = num;
			this.generateMipChain = flag;
			SkyManager skyManager = SkyManager.Get();
			if (skyManager)
			{
				skyManager.GlobalSky = skyManager.GlobalSky;
			}
			Object.DestroyImmediate(material);
			Object.DestroyImmediate(gameObject);
		}

		public void convolve(Cubemap targetCube, Texture sourceCube, bool dstRGBM, bool srcRGBM, bool linear)
		{
			if (targetCube == null)
			{
				return;
			}
			GameObject gameObject = new GameObject("_temp_probe");
			gameObject.hideFlags = 61;
			gameObject.SetActive(true);
			Camera camera = gameObject.AddComponent<Camera>();
			camera.renderingPath = this.renderPath;
			camera.useOcclusionCulling = false;
			Material material = new Material(Shader.Find("Hidden/Marmoset/RGBM Cube"));
			Matrix4x4 identity = Matrix4x4.identity;
			this.copy_internal(targetCube, sourceCube, dstRGBM, srcRGBM, linear, camera, material, identity);
			int num = this.maxExponent;
			this.maxExponent = 2 * num;
			this.convolve_internal(targetCube, sourceCube, dstRGBM, srcRGBM, linear, camera, material, identity);
			this.maxExponent = 8 * num;
			this.convolve_internal(targetCube, targetCube, dstRGBM, dstRGBM, linear, camera, material, identity);
			this.maxExponent = num;
			SkyManager skyManager = SkyManager.Get();
			if (skyManager)
			{
				skyManager.GlobalSky = skyManager.GlobalSky;
			}
			Object.DestroyImmediate(material);
			Object.DestroyImmediate(gameObject);
		}

		public bool capture(Texture targetCube, Vector3 position, Quaternion rotation, bool HDR, bool linear, bool convolve)
		{
			if (targetCube == null)
			{
				return false;
			}
			bool flag = false;
			if (this.cubeRT == null)
			{
				flag = true;
				this.cubeRT = RenderTexture.GetTemporary(targetCube.width, targetCube.width, 24, 2, 1);
				this.cubeRT.isCubemap = true;
				this.cubeRT.useMipMap = true;
				this.cubeRT.autoGenerateMips = true;
				if (!this.cubeRT.IsCreated() && !this.cubeRT.Create())
				{
					this.cubeRT = RenderTexture.GetTemporary(targetCube.width, targetCube.width, 24, 7, 1);
					this.cubeRT.isCubemap = true;
					this.cubeRT.useMipMap = true;
					this.cubeRT.autoGenerateMips = true;
				}
			}
			if (!this.cubeRT.IsCreated() && !this.cubeRT.Create())
			{
				return false;
			}
			GameObject gameObject = new GameObject("_temp_probe");
			Camera camera = gameObject.AddComponent<Camera>();
			SkyManager skyManager = SkyManager.Get();
			if (skyManager && skyManager.ProbeCamera)
			{
				camera.CopyFrom(skyManager.ProbeCamera);
			}
			else if (Camera.main)
			{
				camera.CopyFrom(Camera.main);
			}
			camera.renderingPath = this.renderPath;
			camera.useOcclusionCulling = false;
			camera.allowHDR = true;
			gameObject.hideFlags = 61;
			gameObject.SetActive(true);
			gameObject.transform.position = position;
			Shader.SetGlobalVector("_UniformOcclusion", this.exposures);
			camera.RenderToCubemap(this.cubeRT);
			Shader.SetGlobalVector("_UniformOcclusion", Vector4.one);
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetTRS(position, rotation, Vector3.one);
			Material material = new Material(Shader.Find("Hidden/Marmoset/RGBM Cube"));
			bool flag2 = false;
			this.copy_internal(targetCube, this.cubeRT, HDR, flag2, linear, camera, material, identity);
			if (convolve)
			{
				this.convolve_internal(targetCube, this.cubeRT, HDR, false, linear, camera, material, identity);
			}
			if (skyManager)
			{
				skyManager.GlobalSky = skyManager.GlobalSky;
			}
			Object.DestroyImmediate(material);
			Object.DestroyImmediate(gameObject);
			if (flag)
			{
				RenderTexture.ReleaseTemporary(this.cubeRT);
			}
			return true;
		}

		private static void toggleKeywordPair(string on, string off, bool yes)
		{
			if (yes)
			{
				Shader.EnableKeyword(on);
				Shader.DisableKeyword(off);
			}
			else
			{
				Shader.EnableKeyword(off);
				Shader.DisableKeyword(on);
			}
		}

		private static void toggleKeywordPair(Material mat, string on, string off, bool yes)
		{
			if (yes)
			{
				mat.EnableKeyword(on);
				mat.DisableKeyword(off);
			}
			else
			{
				mat.EnableKeyword(off);
				mat.DisableKeyword(on);
			}
		}

		private void copy_internal(Texture dstCube, Texture srcCube, bool dstRGBM, bool srcRGBM, bool linear, Camera cam, Material skyMat, Matrix4x4 matrix)
		{
			bool allowHDR = cam.allowHDR;
			CameraClearFlags clearFlags = cam.clearFlags;
			int cullingMask = cam.cullingMask;
			cam.clearFlags = 1;
			cam.cullingMask = 0;
			cam.allowHDR = !dstRGBM;
			skyMat.name = "Internal HDR to RGBM Skybox";
			skyMat.shader = Shader.Find("Hidden/Marmoset/RGBM Cube");
			SkyProbe.toggleKeywordPair("MARMO_RGBM_INPUT_ON", "MARMO_RGBM_INPUT_OFF", srcRGBM);
			SkyProbe.toggleKeywordPair("MARMO_RGBM_OUTPUT_ON", "MARMO_RGBM_OUTPUT_OFF", dstRGBM);
			skyMat.SetMatrix("_SkyMatrix", matrix);
			skyMat.SetTexture("_CubeHDR", srcCube);
			Material skybox = RenderSettings.skybox;
			RenderSettings.skybox = skyMat;
			RenderTexture renderTexture = dstCube as RenderTexture;
			Cubemap cubemap = dstCube as Cubemap;
			if (renderTexture)
			{
				cam.RenderToCubemap(renderTexture);
			}
			else if (cubemap)
			{
				cam.RenderToCubemap(cubemap);
			}
			cam.allowHDR = allowHDR;
			cam.clearFlags = clearFlags;
			cam.cullingMask = cullingMask;
			RenderSettings.skybox = skybox;
		}

		private void convolve_internal(Texture dstTex, Texture srcCube, bool dstRGBM, bool srcRGBM, bool linear, Camera cam, Material skyMat, Matrix4x4 matrix)
		{
			bool allowHDR = cam.allowHDR;
			CameraClearFlags clearFlags = cam.clearFlags;
			int cullingMask = cam.cullingMask;
			cam.clearFlags = 1;
			cam.cullingMask = 0;
			cam.allowHDR = !dstRGBM;
			skyMat.name = "Internal Convolve Skybox";
			skyMat.shader = Shader.Find("Hidden/Marmoset/RGBM Convolve");
			SkyProbe.toggleKeywordPair("MARMO_RGBM_INPUT_ON", "MARMO_RGBM_INPUT_OFF", srcRGBM);
			SkyProbe.toggleKeywordPair("MARMO_RGBM_OUTPUT_ON", "MARMO_RGBM_OUTPUT_OFF", dstRGBM);
			skyMat.SetMatrix("_SkyMatrix", matrix);
			skyMat.SetTexture("_CubeHDR", srcCube);
			SkyProbe.bindRandomValueTable(skyMat, "_PhongRands", srcCube.width);
			Material skybox = RenderSettings.skybox;
			RenderSettings.skybox = skyMat;
			Cubemap cubemap = dstTex as Cubemap;
			RenderTexture renderTexture = dstTex as RenderTexture;
			if (cubemap)
			{
				if (this.generateMipChain)
				{
					int num = QPow.Log2i(cubemap.width) - 1;
					for (int i = ((!this.highestMipIsMirror) ? 0 : 1); i < num; i++)
					{
						int num2 = 1 << num - i;
						float num3 = (float)QPow.clampedDownShift(this.maxExponent, (!this.highestMipIsMirror) ? i : (i - 1), 1);
						skyMat.SetFloat("_SpecularExp", num3);
						skyMat.SetFloat("_SpecularScale", this.convolutionScale);
						Cubemap cubemap2 = new Cubemap(num2, cubemap.format, false);
						cam.RenderToCubemap(cubemap2);
						for (int j = 0; j < 6; j++)
						{
							CubemapFace cubemapFace = j;
							cubemap.SetPixels(cubemap2.GetPixels(cubemapFace), cubemapFace, i);
						}
						Object.DestroyImmediate(cubemap2);
					}
					cubemap.Apply(false);
				}
				else
				{
					skyMat.SetFloat("_SpecularExp", (float)this.maxExponent);
					skyMat.SetFloat("_SpecularScale", this.convolutionScale);
					cam.RenderToCubemap(cubemap);
				}
			}
			else if (renderTexture)
			{
				skyMat.SetFloat("_SpecularExp", (float)this.maxExponent);
				skyMat.SetFloat("_SpecularScale", this.convolutionScale);
				cam.RenderToCubemap(renderTexture);
			}
			cam.clearFlags = clearFlags;
			cam.cullingMask = cullingMask;
			cam.allowHDR = allowHDR;
			RenderSettings.skybox = skybox;
		}

		public RenderTexture cubeRT;

		public int maxExponent = 512;

		public Vector4 exposures = Vector4.one;

		public bool generateMipChain = true;

		public bool highestMipIsMirror = true;

		public float convolutionScale = 1f;

		public RenderingPath renderPath = 1;

		private static int sampleCount = 128;

		private static Vector4[] randomValues;
	}
}
