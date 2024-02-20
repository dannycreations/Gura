using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ObjectModel;
using UnityEngine;

namespace BiteEditor.ObjectModel
{
	[Serializable]
	public class SplatMap : ICloneable
	{
		public SplatMap(Vector2f leftTop, Vector2f size, byte layersCount)
		{
			this._leftTop = leftTop;
			this._size = size;
			this._layers = new SplatMap.LayerName[(int)layersCount];
		}

		public SplatMap()
		{
		}

		public object Clone()
		{
			SplatMap splatMap = (SplatMap)base.MemberwiseClone();
			if (this._layers != null)
			{
				splatMap._layers = this._layers.ToArray<SplatMap.LayerName>();
			}
			return splatMap;
		}

		public void SetLayer(byte layerIndex, SplatMap.LayerName layerName)
		{
			this._layers[(int)layerIndex] = layerName;
		}

		public void FinishInitialization(string pondName)
		{
		}

		private int Height
		{
			get
			{
				return this.Texture.height;
			}
		}

		private int Width
		{
			get
			{
				return this.Texture.width;
			}
		}

		public SplatMap.Layer GetLayer(Vector3f pos)
		{
			if (float.IsNaN(pos.x))
			{
				return SplatMap._layersSettings[SplatMap.LayerName.None];
			}
			SplatMap.LayerName layerName = SplatMap.LayerName.None;
			Vector2f vector2f = new Vector2f(pos.x, pos.z) - this._leftTop;
			int height = this.Height;
			int width = this.Width;
			if (vector2f.x < 0f || vector2f.x >= this._size.x || vector2f.y > 0f || -vector2f.y >= this._size.y || height == 0 || width == 0)
			{
				return SplatMap._layersSettings[layerName];
			}
			int num = Math.Min((int)(-vector2f.y / this._size.y * (float)height), height - 1);
			int num2 = Math.Min((int)(vector2f.x / this._size.x * (float)width), width - 1);
			byte b = (byte)(this.Texture.GetPixel(num2, num).b * (float)(this._layers.Length - 1) + 0.5f);
			if ((int)b < this._layers.Length)
			{
				layerName = this._layers[(int)b];
			}
			else
			{
				LogHelper.Error("There is no Layer({0}) in splat map", new object[] { b });
			}
			return SplatMap._layersSettings[layerName];
		}

		private static Dictionary<SplatMap.LayerName, SplatMap.Layer> _layersSettings = new Dictionary<SplatMap.LayerName, SplatMap.Layer>
		{
			{
				SplatMap.LayerName.None,
				new SplatMap.Layer(SplatMap.LayerName.None, 0.3f, 0.25f, 0f)
			},
			{
				SplatMap.LayerName.Sand,
				new SplatMap.Layer(SplatMap.LayerName.Sand, 0.4f, 0.35f, 0.1f)
			},
			{
				SplatMap.LayerName.Silt,
				new SplatMap.Layer(SplatMap.LayerName.Silt, 0.15f, 0.1f, 0.2f)
			},
			{
				SplatMap.LayerName.Gravel,
				new SplatMap.Layer(SplatMap.LayerName.Gravel, 0.6f, 0.5f, 0.4f)
			},
			{
				SplatMap.LayerName.Stone,
				new SplatMap.Layer(SplatMap.LayerName.Stone, 0.6f, 0.5f, 0.6f)
			},
			{
				SplatMap.LayerName.Grass,
				new SplatMap.Layer(SplatMap.LayerName.Grass, 0.3f, 0.25f, 0.8f)
			},
			{
				SplatMap.LayerName.Shell,
				new SplatMap.Layer(SplatMap.LayerName.Shell, 0.4f, 0.35f, 1f)
			}
		};

		[SerializeField]
		[JsonIgnore]
		public Texture2D Texture;

		[JsonIgnore]
		private byte[,] _data;

		[SerializeField]
		[JsonProperty]
		private SplatMap.LayerName[] _layers;

		[SerializeField]
		[JsonProperty]
		private Vector2f _leftTop;

		[SerializeField]
		[JsonProperty]
		private Vector2f _size;

		public enum LayerName
		{
			None = -1,
			Sand,
			Silt,
			Gravel,
			Stone,
			Grass,
			Shell
		}

		public struct Layer
		{
			public Layer(SplatMap.LayerName name, float staticFriction, float slidingFriction, float wearK)
			{
				this.Name = name;
				this.StaticFriction = staticFriction;
				this.SlidingFriction = slidingFriction;
				this.WearK = wearK;
			}

			public readonly SplatMap.LayerName Name;

			public readonly float StaticFriction;

			public readonly float SlidingFriction;

			public readonly float WearK;
		}
	}
}
