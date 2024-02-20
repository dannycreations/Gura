using System;
using UnityEngine;

namespace Phy
{
	public class HeightFieldChunk : ICollider
	{
		public HeightFieldChunk(int size, Vector3 position, float horScale, float vertScale)
		{
			this.heightData = new float[size, size];
			this.position = position;
			this.scale = new Vector3(horScale, vertScale, horScale);
			float num = this.heightData[0, 0];
			float num2 = this.heightData[0, 0];
			for (int i = 0; i < this.heightData.GetLength(0); i++)
			{
				for (int j = 0; j < this.heightData.GetLength(1); j++)
				{
					if (this.heightData[i, j] > num2)
					{
						num2 = this.heightData[i, j];
					}
					if (this.heightData[i, j] < num)
					{
						num = this.heightData[i, j];
					}
				}
			}
			this.dimensions = new Vector3(horScale * (float)(this.heightData.GetLength(0) - 1), vertScale * num2, horScale * (float)(this.heightData.GetLength(1) - 1));
			this.hmIndexX = -1;
			this.hmIndexZ = -1;
		}

		public HeightFieldChunk(MeshCollider sourceCollider, float resolution)
		{
			this.position = sourceCollider.bounds.center - sourceCollider.bounds.extents - Vector3.up * 10f;
			this.SourceInstanceId = sourceCollider.gameObject.GetInstanceID();
			this.dimensions = sourceCollider.bounds.size + Vector3.up * 10f;
			int num = Mathf.Max(2, Mathf.CeilToInt(this.dimensions.x * resolution + 1f));
			int num2 = Mathf.Max(2, Mathf.CeilToInt(this.dimensions.z * resolution + 1f));
			this.heightData = new float[num, num2];
			this.scale = new Vector3(this.dimensions.x / (float)(num - 1), 1f, this.dimensions.z / (float)(num2 - 1));
			Ray ray = default(Ray);
			RaycastHit raycastHit = default(RaycastHit);
			Vector3 vector;
			vector..ctor(this.position.x, sourceCollider.bounds.center.y + sourceCollider.bounds.extents.y, this.position.z);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					ray.origin = vector + new Vector3((float)i * this.scale.x, 1f, (float)j * this.scale.z);
					ray.direction = Vector3.down;
					if (sourceCollider.Raycast(ray, ref raycastHit, sourceCollider.bounds.size.y + 1f))
					{
						this.heightData[i, j] = raycastHit.point.y - this.position.y + 0.02f;
					}
					else
					{
						this.heightData[i, j] = this.position.y;
					}
				}
			}
		}

		public HeightFieldChunk(HeightFieldChunk source)
		{
			this.heightData = new float[source.DataSizeX, source.DataSizeZ];
			this.position = source.position;
			this.scale = source.scale;
			this.dimensions = source.dimensions;
			this.hmIndexX = source.hmIndexX;
			this.hmIndexZ = source.hmIndexZ;
			this.Sync(source);
		}

		public int DataSizeX
		{
			get
			{
				return this.heightData.GetLength(0);
			}
		}

		public int DataSizeZ
		{
			get
			{
				return this.heightData.GetLength(1);
			}
		}

		public int SourceInstanceId { get; private set; }

		public Vector3 position { get; private set; }

		public int hmIndexX { get; private set; }

		public int hmIndexZ { get; private set; }

		public void MarkDirty()
		{
			this.isDirty = true;
		}

		public void Move(Vector3 newPosition, float scale, int hmIndexX, int hmIndexZ)
		{
			this.position = newPosition;
			this.scale.x = scale;
			this.scale.z = scale;
			this.hmIndexX = hmIndexX;
			this.hmIndexZ = hmIndexZ;
			this.MarkDirty();
		}

		public void Sync(ICollider source)
		{
			HeightFieldChunk heightFieldChunk = source as HeightFieldChunk;
			if (heightFieldChunk != null && heightFieldChunk.isDirty)
			{
				this.position = heightFieldChunk.position;
				this.scale = heightFieldChunk.scale;
				this.hmIndexX = heightFieldChunk.hmIndexX;
				this.hmIndexZ = heightFieldChunk.hmIndexZ;
				int dataSizeX = this.DataSizeX;
				int dataSizeZ = this.DataSizeZ;
				float num = heightFieldChunk.heightData[0, 0];
				float num2 = heightFieldChunk.heightData[0, 0];
				for (int i = 0; i < dataSizeX; i++)
				{
					for (int j = 0; j < dataSizeZ; j++)
					{
						this.heightData[i, j] = heightFieldChunk.heightData[i, j];
						if (this.heightData[i, j] > num2)
						{
							num2 = this.heightData[i, j];
						}
						if (this.heightData[i, j] < num)
						{
							num = this.heightData[i, j];
						}
					}
				}
				this.dimensions = new Vector3(this.scale.x * (float)(dataSizeX - 1), this.scale.y * num2, this.scale.x * (float)(dataSizeZ - 1));
				heightFieldChunk.isDirty = false;
			}
		}

		public Vector3 GetHeightAtPoint(Vector3 point, out Vector3 normal)
		{
			Vector3 vector = point - this.position;
			float num = vector.x / this.scale.x;
			float num2 = vector.z / this.scale.z;
			int num3 = (int)num;
			int num4 = (int)num2;
			float num5 = num - (float)num3;
			float num6 = num2 - (float)num4;
			float num7 = (float)num3 * this.scale.x;
			float num8 = (float)num4 * this.scale.z;
			float num9 = (float)(num3 + 1) * this.scale.x;
			float num10 = (float)(num4 + 1) * this.scale.z;
			if (num3 + 1 >= this.DataSizeX || num4 + 1 >= this.DataSizeZ || num3 < 0 || num4 < 0)
			{
				num3 = Mathf.Clamp(num3, 0, this.DataSizeX - 2);
				num4 = Mathf.Clamp(num4, 0, this.DataSizeZ - 2);
			}
			Vector3 vector2;
			vector2..ctor(num7, this.heightData[num3, num4] * this.scale.y, num8);
			Vector3 vector3;
			vector3..ctor(num9, this.heightData[num3 + 1, num4 + 1] * this.scale.y, num10);
			normal = Vector3.zero;
			if (num5 < num6)
			{
				Vector3 vector4;
				vector4..ctor(num7, this.heightData[num3, num4 + 1] * this.scale.y, num10);
				normal = Vector3.up;
				return new Vector3(point.x, this.position.y + vector2.y + (vector3.y - vector4.y) * num5 + (vector4.y - vector2.y) * num6, point.z);
			}
			Vector3 vector5;
			vector5..ctor(num9, this.heightData[num3 + 1, num4] * this.scale.y, num8);
			normal = Vector3.up;
			return new Vector3(point.x, this.position.y + vector2.y + (vector5.y - vector2.y) * num5 + (vector3.y - vector5.y) * num6, point.z);
		}

		public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = point - this.position;
			if (vector.x < 0f || vector.x >= this.dimensions.x || vector.z < 0f || vector.z >= this.dimensions.z || vector.y > this.dimensions.y)
			{
				normal = Vector3.zero;
				return Vector3.zero;
			}
			float num = vector.x / this.scale.x;
			float num2 = vector.z / this.scale.z;
			int num3 = (int)num;
			int num4 = (int)num2;
			float num5 = num - (float)num3;
			float num6 = num2 - (float)num4;
			float num7 = (float)num3 * this.scale.x;
			float num8 = (float)num4 * this.scale.z;
			float num9 = (float)(num3 + 1) * this.scale.x;
			float num10 = (float)(num4 + 1) * this.scale.z;
			if (num3 + 1 >= this.DataSizeX || num4 + 1 >= this.DataSizeZ || num3 < 0 || num4 < 0)
			{
				num3 = Mathf.Clamp(num3, 0, this.DataSizeX - 2);
				num4 = Mathf.Clamp(num4, 0, this.DataSizeZ - 2);
			}
			Vector3 vector2;
			vector2..ctor(num7, this.heightData[num3, num4] * this.scale.y, num8);
			Vector3 vector3;
			vector3..ctor(num9, this.heightData[num3 + 1, num4 + 1] * this.scale.y, num10);
			normal = Vector3.zero;
			if (num5 < num6)
			{
				Vector3 vector4;
				vector4..ctor(num7, this.heightData[num3, num4 + 1] * this.scale.y, num10);
				normal = Vector3.up;
				float num11 = vector2.y + (vector3.y - vector4.y) * num5 + (vector4.y - vector2.y) * num6;
				float num12 = num11 - vector.y;
				if (num12 > 0f)
				{
					return num12 * normal;
				}
				return Vector3.zero;
			}
			else
			{
				Vector3 vector5;
				vector5..ctor(num9, this.heightData[num3 + 1, num4] * this.scale.y, num8);
				normal = Vector3.up;
				float num13 = vector2.y + (vector5.y - vector2.y) * num5 + (vector3.y - vector5.y) * num6;
				float num14 = num13 - vector.y;
				if (num14 > 0f)
				{
					return num14 * normal;
				}
				return Vector3.zero;
			}
		}

		public void DebugDraw(Color c)
		{
			for (int i = 0; i < this.DataSizeX - 1; i++)
			{
				for (int j = 0; j < this.DataSizeZ - 1; j++)
				{
					float num = (float)i * this.scale.x + this.position.x;
					float num2 = (float)j * this.scale.z + this.position.z;
					float num3 = (float)(i + 1) * this.scale.x + this.position.x;
					float num4 = (float)(j + 1) * this.scale.z + this.position.z;
					Vector3 vector;
					vector..ctor(num, this.position.y + this.heightData[i, j] * this.scale.y, num2);
					Vector3 vector2;
					vector2..ctor(num3, this.position.y + this.heightData[i + 1, j + 1] * this.scale.y, num4);
					Vector3 vector3;
					vector3..ctor(num, this.position.y + this.heightData[i, j + 1] * this.scale.y, num4);
					Vector3 vector4;
					vector4..ctor(num3, this.position.y + this.heightData[i + 1, j] * this.scale.y, num2);
					Debug.DrawLine(vector, vector3, Color.green);
					Debug.DrawLine(vector3, vector2, Color.green);
					Debug.DrawLine(vector2, vector4, Color.green);
					Debug.DrawLine(vector4, vector, Color.green);
				}
			}
		}

		public float[,] heightData;

		private Vector3 scale;

		private Vector3 dimensions;

		private HeightFieldChunk rightNeighbour;

		private HeightFieldChunk leftNeighbour;

		private HeightFieldChunk forwardNeighbour;

		private HeightFieldChunk backwardNeighbour;

		private bool isDirty;

		private const float meshColliderHeightFieldLowOffset = 10f;

		public const float MaxRasterizableMeshColliderSize = 31f;
	}
}
