using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;
using UnityEngine;

namespace BiteEditor.ObjectModel
{
	[Serializable]
	public class HeightMap : ICloneable
	{
		public HeightMap(Vector2f leftTop, Vector2f size)
		{
			this._leftTop = leftTop;
			this._size = size;
		}

		public HeightMap()
		{
		}

		[JsonIgnore]
		public Vector2f LeftTop
		{
			get
			{
				return this._leftTop;
			}
		}

		[JsonIgnore]
		public Vector2f CellSize
		{
			get
			{
				return new Vector2f(this._size.x / (float)this.Width, this._size.y / (float)this.Height);
			}
		}

		public object Clone()
		{
			return (HeightMap)base.MemberwiseClone();
		}

		public void FinishInitialization(string pondName)
		{
		}

		public int Height
		{
			get
			{
				return this.Texture.height;
			}
		}

		public int Width
		{
			get
			{
				return this.Texture.width;
			}
		}

		private float GetHeight(int y, int x)
		{
			y = Math.Min(y, this.Height - 1);
			x = Math.Min(x, this.Width - 1);
			y = this.Texture.height - y - 1;
			Color pixel = this.Texture.GetPixel(x, y);
			return Converter.BytesToFloat(new byte[]
			{
				(byte)(pixel.r * 255f),
				(byte)(pixel.g * 255f),
				(byte)(pixel.b * 255f),
				(byte)(pixel.a * 255f)
			});
		}

		public Depth GetWaterDepth(Vector3f pos)
		{
			if (float.IsNaN(pos.x))
			{
				return Depth.Invalid;
			}
			float num = -pos.y;
			Vector2f vector2f = new Vector2f(pos.x, pos.z) - this._leftTop;
			if (vector2f.x < 0f || vector2f.x >= this._size.x || vector2f.y > 0f || -vector2f.y >= this._size.y || this.Height == 0 || this.Width == 0)
			{
				return Depth.Invalid;
			}
			float num2 = -this.GetHeight((int)(-vector2f.y / this._size.y * (float)this.Height), (int)(vector2f.x / this._size.x * (float)this.Width));
			float topDepth = HeightMap.GetTopDepth(num2);
			float bottomDepth = HeightMap.GetBottomDepth(num2);
			if (num <= topDepth)
			{
				return Depth.Top;
			}
			if (num2 - num <= bottomDepth)
			{
				return Depth.Bottom;
			}
			return Depth.Middle;
		}

		public float GetBottomHeight(Vector3f pos)
		{
			Vector2f vector2f = new Vector2f(pos.x, pos.z) - this._leftTop;
			if (float.IsNaN(pos.x) || vector2f.x < 0f || vector2f.x >= this._size.x || vector2f.y > 0f || -vector2f.y >= this._size.y || this.Height == 0 || this.Width == 0)
			{
				return 0f;
			}
			return this.GetHeight((int)(-vector2f.y / this._size.y * (float)this.Height), (int)(vector2f.x / this._size.x * (float)this.Width));
		}

		public float GetBottomHeight0(Vector3f pos)
		{
			Vector2f vector2f = new Vector2f(pos.x, pos.z) - this._leftTop;
			int height = this.Height;
			int width = this.Width;
			if (vector2f.x < 0f || vector2f.x >= this._size.x || vector2f.y > 0f || -vector2f.y >= this._size.y || width == 0 || height == 0)
			{
				return 0f;
			}
			int num = (int)(-vector2f.y / this._size.y * (float)height);
			int num2 = (int)(vector2f.x / this._size.x * (float)width);
			float height2 = this.GetHeight(num, num2);
			if (height2 >= 0f)
			{
				for (int i = 0; i < HeightMap._dirs.Length; i++)
				{
					int num3 = num2 + (int)HeightMap._dirs[i].x;
					int num4 = num + (int)HeightMap._dirs[i].y;
					if (num3 >= 0 && num3 < width && num4 >= 0 && num4 < height)
					{
						float height3 = this.GetHeight(num4, num3);
						if (height3 < 0f)
						{
							return height2;
						}
					}
				}
			}
			return height2;
		}

		private static int FindEdgeSide(Vector2i cell, HeightMap.IsMarkedCellDelegate isMarkedCellFunc)
		{
			for (int i = 0; i < HeightMap._edgeShortDirs.Length; i++)
			{
				Vector2i vector2i = cell + HeightMap._edgeShortDirs[i];
				if (!isMarkedCellFunc(vector2i.y, vector2i.x))
				{
					return i;
				}
			}
			return -1;
		}

		private static int FindNextEdgeSide(Vector2i cell, int edgeFromIndex, HeightMap.IsMarkedCellDelegate isMarkedCellFunc)
		{
			for (int i = edgeFromIndex + 1; i < HeightMap._edgeShortDirs.Length; i++)
			{
				Vector2i vector2i = cell + HeightMap._edgeShortDirs[i];
				if (isMarkedCellFunc(vector2i.y, vector2i.x))
				{
					return i - 1;
				}
			}
			for (int j = 0; j < edgeFromIndex; j++)
			{
				Vector2i vector2i2 = cell + HeightMap._edgeShortDirs[j];
				if (isMarkedCellFunc(vector2i2.y, vector2i2.x))
				{
					return (j != 0) ? (j - 1) : (HeightMap._edgeShortDirs.Length - 1);
				}
			}
			return -1;
		}

		public static List<List<HeightMap.Edge>> BuildEdgeContour(HeightMap.IsMarkedCellDelegate isMarkedCellFunc, short maxY, short maxX)
		{
			List<List<HeightMap.Edge>> list = new List<List<HeightMap.Edge>>();
			HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
			for (short num = 0; num < maxY; num += 1)
			{
				for (short num2 = 0; num2 < maxX; num2 += 1)
				{
					Vector2i vector2i = new Vector2i(num2, num);
					if (isMarkedCellFunc(num, num2) && !hashSet.Contains(vector2i))
					{
						int num3 = HeightMap.FindEdgeSide(vector2i, isMarkedCellFunc);
						if (num3 >= 0)
						{
							int num4 = HeightMap.FindNextEdgeSide(vector2i, num3, isMarkedCellFunc);
							if (num4 >= 0)
							{
								List<HeightMap.Edge> list2 = new List<HeightMap.Edge>();
								HeightMap.Edge edge = new HeightMap.Edge(vector2i, num3, HeightMap.FindNextEdgeSide(vector2i, num3, isMarkedCellFunc));
								list2.Add(edge);
								do
								{
									int num5;
									if (edge.LastSideIndex == 0)
									{
										vector2i = edge.Cell + new Vector2i(1, 0);
										Vector2i vector2i2 = vector2i + new Vector2i(0, -1);
										if (!isMarkedCellFunc(vector2i2.y, vector2i2.x))
										{
											num5 = 0;
										}
										else
										{
											vector2i = vector2i2;
											num5 = 3;
										}
									}
									else if (edge.LastSideIndex == 1)
									{
										vector2i = edge.Cell + new Vector2i(0, 1);
										Vector2i vector2i3 = vector2i + new Vector2i(1, 0);
										if (!isMarkedCellFunc(vector2i3.y, vector2i3.x))
										{
											num5 = 1;
										}
										else
										{
											vector2i = vector2i3;
											num5 = 0;
										}
									}
									else if (edge.LastSideIndex == 2)
									{
										vector2i = edge.Cell + new Vector2i(-1, 0);
										Vector2i vector2i4 = vector2i + new Vector2i(0, 1);
										if (!isMarkedCellFunc(vector2i4.y, vector2i4.x))
										{
											num5 = 2;
										}
										else
										{
											vector2i = vector2i4;
											num5 = 1;
										}
									}
									else
									{
										vector2i = edge.Cell + new Vector2i(0, -1);
										Vector2i vector2i5 = vector2i + new Vector2i(-1, 0);
										if (!isMarkedCellFunc(vector2i5.y, vector2i5.x))
										{
											num5 = 3;
										}
										else
										{
											vector2i = vector2i5;
											num5 = 2;
										}
									}
									int num6 = HeightMap.FindNextEdgeSide(vector2i, num5, isMarkedCellFunc);
									edge = new HeightMap.Edge(vector2i, num5, num6);
									list2.Add(edge);
									hashSet.Add(vector2i);
								}
								while (!(edge.Cell == list2[0].Cell) || edge.LastSideIndex < list2[0].Sides[0]);
								list2.RemoveAt(0);
								list.Add(list2);
							}
						}
					}
				}
			}
			return list;
		}

		public static void TestEdgeBuilding()
		{
			byte[,] data = new byte[,]
			{
				{ 1, 1, 0, 0, 0, 0, 0 },
				{ 0, 1, 0, 0, 0, 0, 0 },
				{ 0, 1, 1, 0, 0, 0, 0 },
				{ 0, 0, 0, 0, 0, 0, 0 },
				{ 0, 0, 0, 0, 0, 0, 0 }
			};
			short maxX = (short)data.GetLength(1);
			short maxY = (short)data.GetLength(0);
			HeightMap.BuildEdgeContour((short y, short x) => y >= 0 && x >= 0 && y < maxY && x < maxX && data[(int)y, (int)x] > 0, maxY, maxX);
		}

		public List<List<Vector3>> BuildWaterEdge(float waterHeight = 0f)
		{
			List<List<HeightMap.Edge>> list = HeightMap.BuildEdgeContour((short y, short x) => y >= 0 && x >= 0 && (int)y < this.Texture.height && (int)x < this.Texture.width && this.GetHeight((int)y, (int)x) < waterHeight, (short)this.Texture.height, (short)this.Texture.width);
			List<List<Vector3>> list2 = new List<List<Vector3>>();
			for (int i = 0; i < list.Count; i++)
			{
				List<HeightMap.Edge> list3 = list[i];
				List<Vector3> list4 = new List<Vector3>();
				list2.Add(list4);
				for (int j = 0; j < list3.Count; j++)
				{
					HeightMap.Edge edge = list3[j];
					float height = this.GetHeight((int)edge.Cell.y, (int)edge.Cell.x);
					Vector3 vector = this.MatrixToVector3((int)edge.Cell.y, (int)edge.Cell.x, height);
					for (int k = 0; k < edge.Sides.Count; k++)
					{
						Vector2i vector2i = edge.Cell + HeightMap._edgeShortDirs[edge.Sides[k]];
						int num = Mathf.Max(Mathf.Min((int)vector2i.x, this.Texture.width - 1), 0);
						int num2 = Mathf.Max(Mathf.Min((int)vector2i.y, this.Texture.height - 1), 0);
						vector2i = new Vector2i((short)num, (short)num2);
						if (vector2i == edge.Cell)
						{
							list4.Add(new Vector3(vector.x, waterHeight, vector.z));
						}
						else
						{
							float height2 = this.GetHeight(num2, num);
							Vector3 vector2 = this.MatrixToVector3(num2, num, height2);
							float num3 = 1f - (height2 - waterHeight) / (height2 - height);
							Vector3 vector3 = vector - vector2;
							vector3 = 1f / vector3.magnitude * vector3;
							list4.Add(vector2 + num3 * vector3);
						}
					}
				}
			}
			return list2;
		}

		private Vector3 MatrixToVector3(int y, int x, float height)
		{
			float num = (float)(-(float)y) * this._size.y / (float)this.Height;
			float num2 = (float)x * this._size.x / (float)this.Width;
			return new Vector3(num2 + this._leftTop.x, height, num + this._leftTop.y);
		}

		public Vector2i? Vector3fToMatrix(Vector3f pos)
		{
			Vector2f vector2f = new Vector2f(pos.x, pos.z) - this._leftTop;
			if (vector2f.x < 0f || vector2f.x >= this._size.x || vector2f.y > 0f || -vector2f.y >= this._size.y)
			{
				return null;
			}
			return new Vector2i?(new Vector2i((short)(vector2f.x * (float)this.Width / this._size.x), (short)(-vector2f.y * (float)this.Height / this._size.y)));
		}

		public static float GetTopDepth(float depth)
		{
			if (depth >= 9f)
			{
				return 2f;
			}
			if (depth > 5f)
			{
				float num = (depth - 5f) / 4f;
				return 1f * (1f - num) + 2f * num;
			}
			if (depth > 1f)
			{
				float num2 = (depth - 1f) / 4f;
				return 0.25f * (1f - num2) + 1f * num2;
			}
			float num3 = depth / 1f;
			return 0.05f * (1f - num3) + 0.25f * num3;
		}

		public static float GetBottomDepth(float depth)
		{
			if (depth > 10f)
			{
				return 1.5f;
			}
			if (depth > 2.5f)
			{
				float num = (depth - 2.5f) / 7.5f;
				return 0.5f * (1f - num) + 1.5f * num;
			}
			return depth / 2.5f * 0.5f;
		}

		[JsonIgnore]
		public const float TopHighDepth = 9f;

		[JsonIgnore]
		public const float TopMiddleDepth = 5f;

		[JsonIgnore]
		public const float TopLowDepth = 1f;

		[JsonIgnore]
		public const float TopHighDy = 2f;

		[JsonIgnore]
		public const float TopMiddleDy = 1f;

		[JsonIgnore]
		public const float TopLowDy = 0.25f;

		[JsonIgnore]
		public const float TopLowestDy = 0.05f;

		[JsonIgnore]
		public const float BottomMiddleDepth = 2.5f;

		[JsonIgnore]
		public const float BottomMiddleDy = 0.5f;

		[JsonIgnore]
		public const float BottomHighDepth = 10f;

		[JsonIgnore]
		public const float BottomHighDy = 1.5f;

		[SerializeField]
		[JsonIgnore]
		public Texture2D Texture;

		[JsonIgnore]
		private float[,] _data;

		[SerializeField]
		[JsonProperty]
		private Vector2f _leftTop;

		[SerializeField]
		[JsonProperty]
		private Vector2f _size;

		private static Vector2i[] _dirs = new Vector2i[]
		{
			new Vector2i(-1, 0),
			new Vector2i(1, 0),
			new Vector2i(0, -1),
			new Vector2i(0, 1)
		};

		private static Vector2i[] _edgeShortDirs = new Vector2i[]
		{
			new Vector2i(0, -1),
			new Vector2i(1, 0),
			new Vector2i(0, 1),
			new Vector2i(-1, 0)
		};

		private static Vector2i[] _edgeFullDirs = new Vector2i[]
		{
			new Vector2i(-1, -1),
			new Vector2i(-1, 0),
			new Vector2i(-1, 1),
			new Vector2i(0, 1),
			new Vector2i(1, 1),
			new Vector2i(1, 0),
			new Vector2i(1, -1),
			new Vector2i(0, -1)
		};

		public delegate bool IsMarkedCellDelegate(short y, short x);

		public enum Side
		{
			Top,
			Right,
			Down,
			Left
		}

		public struct Edge
		{
			public Edge(Vector2i cell, int fromSideIndex, int toSideIndex)
			{
				this = default(HeightMap.Edge);
				this.Cell = cell;
				this.Sides = new List<int>();
				int num = ((fromSideIndex > toSideIndex) ? 3 : toSideIndex);
				for (int i = fromSideIndex; i <= num; i++)
				{
					this.Sides.Add(i);
				}
				if (fromSideIndex > toSideIndex)
				{
					for (int j = 0; j <= toSideIndex; j++)
					{
						this.Sides.Add(j);
					}
				}
			}

			public int LastSideIndex
			{
				get
				{
					return this.Sides[this.Sides.Count - 1];
				}
			}

			public override string ToString()
			{
				return string.Format("cell = {0}, from side: {1}, to side: {2}", this.Cell, this.Sides[0], this.LastSideIndex);
			}

			public readonly Vector2i Cell;

			public readonly List<int> Sides;
		}
	}
}
