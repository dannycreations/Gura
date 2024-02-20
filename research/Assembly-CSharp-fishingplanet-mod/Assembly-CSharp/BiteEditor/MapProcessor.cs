using System;
using Malee;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class MapProcessor : ProbabilityMap
	{
		public float RangeMin
		{
			get
			{
				return this._rangeMin;
			}
		}

		public float RangeMax
		{
			get
			{
				return this._rangeMax;
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

		public Renderer Renderer
		{
			get
			{
				return this._renderer;
			}
		}

		public void Reset()
		{
			this._renderer = base.GetComponent<Renderer>();
			if (this._actions.Count == 0)
			{
				this._actions = new MapProcessor.ActionList
				{
					new MapProcessor.Action()
				};
			}
		}

		public float[,] DebugCalcMatrixOnCpu()
		{
			return this.CalcMapsOnCpu(ProbabilityMap.GetBounds(base.transform.position, this.Width, this.Height), (int)this._widthResolution, (int)this._heightResolution, 0, WindDirection.None, false, 0f, 1f);
		}

		public override float[,] CalcMapsOnCpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			float[,] array = this._initialMap.CalcMapsOnCpu(worldBound, widthResolution, heightResolution, progress, wind, this._isInv, this._rangeMin, this._rangeMax);
			for (int i = 0; i < this._actions.Count; i++)
			{
				MapProcessor.Action action = this._actions[i];
				if (action != null && !(action.Operand == null))
				{
					float[,] array2 = action.Operand.CalcMapsOnCpu(worldBound, widthResolution, heightResolution, progress, wind, false, action.RangeMin, action.RangeMax);
					switch (action.Operation)
					{
					case Operation.Add:
						this.CpuAddMap(array, array2);
						break;
					case Operation.Sub:
						this.CpuSubMap(array, array2);
						break;
					case Operation.Mul:
						this.CpuMulMap(array, array2);
						break;
					case Operation.InvAdd:
						this.CpuAddInvMap(array, array2);
						break;
					case Operation.InvSub:
						this.CpuSubInvMap(array, array2);
						break;
					}
				}
			}
			if (isInversed)
			{
				for (int j = 0; j < heightResolution; j++)
				{
					for (int k = 0; k < widthResolution; k++)
					{
						array[j, k] = Mathf.Lerp(rangeMin, rangeMax, 1f - array[j, k]);
					}
				}
			}
			else
			{
				for (int l = 0; l < heightResolution; l++)
				{
					for (int m = 0; m < widthResolution; m++)
					{
						array[l, m] = Mathf.Lerp(rangeMin, rangeMax, array[l, m]);
					}
				}
			}
			return array;
		}

		public RenderTexture DebugCalcMatrixOnGpu()
		{
			return this.CalcMapsOnGpu(ProbabilityMap.GetBounds(base.transform.position, this.Width, this.Height), (int)this._widthResolution, (int)this._heightResolution, 0, WindDirection.None, false, 0f, 1f);
		}

		public override RenderTexture CalcMapsOnGpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			RenderTexture[] array = RTUtility.CreateTexturesBufer(widthResolution, heightResolution, 11, 1, 1);
			array[0] = this._initialMap.CalcMapsOnGpu(worldBound, widthResolution, heightResolution, progress, wind, this._isInv, this._rangeMin, this._rangeMax);
			for (int i = 0; i < this._actions.Count; i++)
			{
				MapProcessor.Action action = this._actions[i];
				if (action != null && !(action.Operand == null))
				{
					RenderTexture renderTexture = action.Operand.CalcMapsOnGpu(worldBound, widthResolution, heightResolution, progress, wind, false, action.RangeMin, action.RangeMax);
					Material material = this.FindShader(action.Operation);
					material.SetTexture("_OperationTex", renderTexture);
					RTUtility.BlitBuffered(array, material);
				}
			}
			return array[0];
		}

		private Material FindShader(Operation operation)
		{
			switch (operation)
			{
			case Operation.Add:
				return this._addShader;
			case Operation.Sub:
				return this._subShader;
			case Operation.Mul:
				return this._mulShader;
			case Operation.InvAdd:
				return this._mulShader;
			case Operation.InvSub:
				return this._mulShader;
			default:
				return null;
			}
		}

		private void CpuAddMap(float[,] dst, float[,] src)
		{
			int length = dst.GetLength(0);
			int length2 = dst.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					float num = Mathf.Max(dst[i, j], 0f);
					float num2 = Mathf.Max(src[i, j], 0f);
					dst[i, j] = Mathf.Min(num + num2, 1f);
				}
			}
		}

		private void CpuSubMap(float[,] dst, float[,] src)
		{
			int length = dst.GetLength(0);
			int length2 = dst.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					float num = Mathf.Max(dst[i, j], 0f);
					float num2 = Mathf.Max(src[i, j], 0f);
					dst[i, j] = Mathf.Max(num - num2, 0f);
				}
			}
		}

		private void CpuMulMap(float[,] dst, float[,] src)
		{
			int length = dst.GetLength(0);
			int length2 = dst.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					float num = Mathf.Max(dst[i, j], 0f);
					float num2 = Mathf.Max(src[i, j], 0f);
					dst[i, j] = num * num2;
				}
			}
		}

		private void CpuAddInvMap(float[,] dst, float[,] src)
		{
			int length = dst.GetLength(0);
			int length2 = dst.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					float num = Mathf.Max(dst[i, j], 0f);
					float num2 = Mathf.Max(src[i, j], 0f);
					dst[i, j] = Mathf.Min(num + 1f - num2, 1f);
				}
			}
		}

		private void CpuSubInvMap(float[,] dst, float[,] src)
		{
			int length = dst.GetLength(0);
			int length2 = dst.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					float num = Mathf.Max(dst[i, j], 0f);
					float num2 = Mathf.Max(src[i, j], 0f);
					dst[i, j] = Mathf.Max(num - (1f - num2), 0f);
				}
			}
		}

		[SerializeField]
		private MapResolution _widthResolution = MapResolution._16;

		[SerializeField]
		private MapResolution _heightResolution = MapResolution._16;

		[SerializeField]
		private ProbabilityMap _initialMap;

		[SerializeField]
		private bool _isInv;

		[Range(0f, 1f)]
		[SerializeField]
		private float _rangeMin;

		[Range(0f, 1f)]
		[SerializeField]
		private float _rangeMax = 1f;

		[SerializeField]
		private Material _displayMaterialPrefab;

		[SerializeField]
		private Material _addShader;

		[SerializeField]
		private Material _subShader;

		[SerializeField]
		private Material _mulShader;

		[SerializeField]
		private Material _invAddShader;

		[SerializeField]
		private Material _invSubShader;

		[Reorderable]
		[SerializeField]
		private MapProcessor.ActionList _actions;

		private Renderer _renderer;

		[Serializable]
		public class Action
		{
			public Operation Operation
			{
				get
				{
					return this._operation;
				}
			}

			public ProbabilityMap Operand
			{
				get
				{
					return this._operand;
				}
			}

			public float RangeMin
			{
				get
				{
					return this._rangeMin;
				}
			}

			public float RangeMax
			{
				get
				{
					return this._rangeMax;
				}
			}

			[SerializeField]
			private Operation _operation;

			[SerializeField]
			private ProbabilityMap _operand;

			[Range(0f, 1f)]
			[SerializeField]
			private float _rangeMin;

			[Range(0f, 1f)]
			[SerializeField]
			private float _rangeMax = 1f;
		}

		[Serializable]
		public class ActionList : ReorderableArray<MapProcessor.Action>
		{
		}
	}
}
