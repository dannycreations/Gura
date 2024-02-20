using System;
using UnityEngine;

namespace BiteEditor
{
	public class BiteMapPatch : MonoBehaviour
	{
		public Terrain Terrain
		{
			get
			{
				return this._terrain;
			}
		}

		public short WidthResolution
		{
			get
			{
				return (short)this.Texture.width;
			}
		}

		public short HeightResolution
		{
			get
			{
				return (short)this.Texture.height;
			}
		}

		public bool IsMain
		{
			get
			{
				return this._isMain;
			}
		}

		public Material ProjectorMaterial
		{
			get
			{
				return this._projectorMaterial;
			}
		}

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public float Width
		{
			get
			{
				return base.transform.localScale.x;
			}
		}

		public float Height
		{
			get
			{
				return base.transform.localScale.y;
			}
		}

		public float WidthK
		{
			get
			{
				return this.Width / (float)this.WidthResolution;
			}
		}

		public float HeightK
		{
			get
			{
				return this.Height / (float)this.HeightResolution;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return base.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
			}
		}

		public float[,] MapToCpuMatrix(bool isInversed, float rangeMin, float rangeMax)
		{
			Texture2D texture2D = base.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
			Color[] pixels = texture2D.GetPixels();
			float[,] array = new float[texture2D.height, texture2D.width];
			if (isInversed)
			{
				for (int i = 0; i < texture2D.height; i++)
				{
					for (int j = 0; j < texture2D.width; j++)
					{
						Color color = pixels[i * texture2D.width + j];
						float num = Mathf.Lerp(rangeMin, rangeMax, 1f - color.r);
						array[texture2D.height - i - 1, j] = ((color.a >= 1f) ? num : (-1f));
					}
				}
			}
			else
			{
				for (int k = 0; k < texture2D.height; k++)
				{
					for (int l = 0; l < texture2D.width; l++)
					{
						Color color2 = pixels[k * texture2D.width + l];
						float num2 = Mathf.Lerp(rangeMin, rangeMax, color2.r);
						array[texture2D.height - k - 1, l] = ((color2.a >= 1f) ? num2 : (-1f));
					}
				}
			}
			return array;
		}

		private const float MAIN_Y = 0.01f;

		private const float PATCH_Y = 0.011f;

		[SerializeField]
		private int _id;

		[SerializeField]
		private Terrain _terrain;

		[SerializeField]
		private Material _baseForDrawMaterial;

		[SerializeField]
		private MapResolution _widthResolution = MapResolution._1024;

		[SerializeField]
		private MapResolution _heightResolution = MapResolution._1024;

		[SerializeField]
		private bool _isMain;

		[SerializeField]
		private Material _projectorMaterial;

		[HideInInspector]
		[SerializeField]
		private Material _copyShader;

		[HideInInspector]
		[SerializeField]
		private Renderer _renderer;
	}
}
