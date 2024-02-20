using System;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class FlowMap : ICloneable
	{
		public FlowMap(float maxSpeed, Vector3f scale)
		{
			this._maxSpeed = maxSpeed;
			this._scale = scale;
		}

		public FlowMap()
		{
		}

		public object Clone()
		{
			return (FlowMap)base.MemberwiseClone();
		}

		public void FinishInitialization(string pondName)
		{
		}

		public static float GetHeightModifier(float deep)
		{
			if (deep < 0f)
			{
				return 0f;
			}
			if (deep < 1f)
			{
				return 1f;
			}
			if (deep < 10f)
			{
				float num = (deep - 1f) / 9f;
				return 1f - num + num * 0.33f;
			}
			if (deep < 70f)
			{
				float num2 = (deep - 10f) / 60f;
				return (1f - num2) * 0.33f;
			}
			return 0f;
		}

		public Vector3f GetFlowVector(Vector3f pos)
		{
			int num = (int)((pos.x * this._scale.z + this._scale.x) * (float)this._size.x);
			int num2 = (int)((pos.z * this._scale.z + this._scale.y) * (float)this._size.y);
			ushort num3 = this._data[num2, num];
			float num4 = (float)(num3 >> 8) / 255f;
			float num5 = (float)(num3 & 255) / 255f;
			float num6 = this._maxSpeed * FlowMap.GetHeightModifier(-pos.y);
			return new Vector3f((1f - num4 * 2f) * num6, 0f, (1f - num5 * 2f) * num6);
		}

		[JsonIgnore]
		private ushort[,] _data;

		[JsonIgnore]
		private Vector2i _size;

		[JsonProperty]
		private float _maxSpeed;

		[JsonProperty]
		private Vector3f _scale;

		private const float MAX_SPEED_DEEP = 1f;

		private const float THIRD_SPEED_DEEP = 10f;

		private const float ZERO_SPEED_DEEP = 70f;
	}
}
