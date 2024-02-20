using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class BiteMap : ProbabilityMap
	{
		public BiteMapPatch MainPatch
		{
			get
			{
				return this._main;
			}
		}

		private List<BiteMapPatch> Patches
		{
			get
			{
				List<BiteMapPatch> list = new List<BiteMapPatch>();
				for (int i = 0; i < base.transform.childCount; i++)
				{
					BiteMapPatch component = base.transform.GetChild(i).GetComponent<BiteMapPatch>();
					if (component != null && component != this._main)
					{
						list.Add(component);
					}
				}
				return list;
			}
		}

		public override RenderTexture CalcMapsOnGpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			RenderTexture[] array = RTUtility.CreateTexturesBufer(widthResolution, heightResolution, 11, 1, 1);
			Vector4 vector;
			vector..ctor(worldBound.width, worldBound.height);
			this._copyShader.SetVector("_targetSize", vector);
			this._copyShader.SetVector("_sourceSize", new Vector4(this._main.Width, this._main.Height));
			Rect rect = this.GetPatchBound(this._main);
			this._copyShader.SetVector("_translate", new Vector4(worldBound.x - rect.x, worldBound.y - rect.y));
			this._copyShader.SetInt("_isInverted", (!isInversed) ? 0 : 1);
			this._copyShader.SetFloat("_rangeMin", rangeMin);
			this._copyShader.SetFloat("_rangeMax", rangeMax);
			this._copyShader.SetFloat("_modifier", this._modifier);
			Graphics.Blit(this._main.Texture, array[0], this._copyShader);
			List<BiteMapPatch> patches = this.Patches;
			for (int i = 0; i < patches.Count; i++)
			{
				this._replaceShader.SetVector("_targetSize", vector);
				this._replaceShader.SetVector("_sourceSize", new Vector4(patches[i].Width, patches[i].Height));
				rect = this.GetPatchBound(patches[i]);
				this._replaceShader.SetVector("_translate", new Vector4(worldBound.x - rect.x, worldBound.y - rect.y));
				this._replaceShader.SetTexture("_OperationTex", patches[i].Texture);
				this._replaceShader.SetInt("_isInverted", (!isInversed) ? 0 : 1);
				this._replaceShader.SetFloat("_rangeMin", rangeMin);
				this._replaceShader.SetFloat("_rangeMax", rangeMax);
				RTUtility.BlitBuffered(array, this._replaceShader);
			}
			return array[0];
		}

		private Rect GetPatchBound(BiteMapPatch patch)
		{
			return ProbabilityMap.GetBounds(patch.Position, patch.Width, patch.Height);
		}

		public override float[,] CalcMapsOnCpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			rangeMin *= this._modifier;
			rangeMax *= this._modifier;
			float[,] array = new float[heightResolution, widthResolution];
			float num = worldBound.width / (float)widthResolution;
			float num2 = worldBound.height / (float)heightResolution;
			this.SamplePatchOnCpu(array, this._main, worldBound, num, num2, isInversed, rangeMin, rangeMax, false);
			List<BiteMapPatch> patches = this.Patches;
			for (int i = 0; i < patches.Count; i++)
			{
				this.SamplePatchOnCpu(array, patches[i], worldBound, num, num2, isInversed, rangeMin, rangeMax, true);
			}
			return array;
		}

		public void SamplePatchOnCpu(float[,] dst, BiteMapPatch patch, Rect worldBound, float widthScale, float heightScale, bool isInversed, float rangeMin, float rangeMax, bool ignoreAlpha)
		{
			Rect bounds = ProbabilityMap.GetBounds(patch.Position, patch.Width, patch.Height);
			if (worldBound.Overlaps(bounds))
			{
				float[,] array = patch.MapToCpuMatrix(isInversed, rangeMin, rangeMax);
				float num = patch.Width / (float)array.GetLength(1);
				float num2 = patch.Height / (float)array.GetLength(0);
				int num3 = Mathf.Max(0, (int)((bounds.x - worldBound.x) / widthScale));
				int num4 = Mathf.Max(0, (int)((worldBound.yMax - bounds.yMax) / heightScale));
				int num5 = Mathf.Min((int)((bounds.xMax - worldBound.x) / widthScale), dst.GetLength(1));
				int num6 = Mathf.Min((int)((worldBound.yMax - bounds.y) / heightScale), dst.GetLength(0));
				for (int i = num4; i < num6; i++)
				{
					float num7 = worldBound.yMax - (float)i * heightScale;
					int num8 = (int)((bounds.yMax - num7) / num2);
					for (int j = num3; j < num5; j++)
					{
						float num9 = worldBound.x + (float)j * widthScale;
						int num10 = (int)((num9 - bounds.x) / num);
						if (!ignoreAlpha || array[num8, num10] >= 0f)
						{
							dst[i, j] = array[num8, num10];
						}
					}
				}
			}
		}

		[SerializeField]
		private Terrain _terrain;

		[SerializeField]
		private BiteMapPatch _patchPrefab;

		[SerializeField]
		private BiteMapPatch _main;

		[SerializeField]
		private Material _copyShader;

		[SerializeField]
		private Material _replaceShader;

		[SerializeField]
		private float _modifier = 1f;
	}
}
