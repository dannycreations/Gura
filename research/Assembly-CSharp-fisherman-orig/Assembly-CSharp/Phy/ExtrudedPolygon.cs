using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public class ExtrudedPolygon : ICollider
	{
		public ExtrudedPolygon(MeshCollider sourceCollider)
		{
			this.SourceInstanceId = sourceCollider.gameObject.GetInstanceID();
			this.vertices = new List<Vector2>();
			this.triangles = new List<int>();
			Vector3[] array = sourceCollider.sharedMesh.vertices;
			Vector3[] normals = sourceCollider.sharedMesh.normals;
			int[] array2 = sourceCollider.sharedMesh.triangles;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Vector3 vector = array[0];
			Vector3 vector2 = array[0];
			for (int i = 0; i < array2.Length; i += 3)
			{
				int num = array2[i];
				int num2 = array2[i + 1];
				int num3 = array2[i + 2];
				if (array[num].y > vector.y)
				{
					vector = array[num];
				}
				if (array[num].y < vector2.y)
				{
					vector2 = array[num];
				}
				if (normals[num].y > 0.9f && normals[num2].y > 0.9f && normals[num3].y > 0.9f)
				{
					if (dictionary.ContainsKey(num))
					{
						this.triangles.Add(dictionary[num]);
					}
					else
					{
						this.vertices.Add(new Vector2(array[num].x, array[num].z));
						dictionary[num] = this.vertices.Count - 1;
						this.triangles.Add(this.vertices.Count - 1);
					}
					if (dictionary.ContainsKey(num2))
					{
						this.triangles.Add(dictionary[num2]);
					}
					else
					{
						this.vertices.Add(new Vector2(array[num2].x, array[num2].z));
						dictionary[num2] = this.vertices.Count - 1;
						this.triangles.Add(this.vertices.Count - 1);
					}
					if (dictionary.ContainsKey(num3))
					{
						this.triangles.Add(dictionary[num3]);
					}
					else
					{
						this.vertices.Add(new Vector2(array[num3].x, array[num3].z));
						dictionary[num3] = this.vertices.Count - 1;
						this.triangles.Add(this.vertices.Count - 1);
					}
				}
			}
			this.top = sourceCollider.transform.position.y + (sourceCollider.transform.rotation * vector).y + 0.02f;
			this.bottom = sourceCollider.transform.position.y + (sourceCollider.transform.rotation * vector2).y;
			if (Mathf.Approximately(this.bottom, this.top))
			{
				this.bottom = -100f;
			}
		}

		public int SourceInstanceId { get; private set; }

		public Quaternion rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				this._rotation = value;
				this._invRotation = Quaternion.Inverse(value);
			}
		}

		public static bool CheckSourceCollider(MeshCollider sourceCollider)
		{
			Vector3[] array = sourceCollider.sharedMesh.vertices;
			float num = array[0].y;
			float num2 = num;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].y > num2)
				{
					num2 = array[i].y;
				}
				if (array[i].y < num)
				{
					num = array[i].y;
				}
			}
			return num2 - num < 0.01f;
		}

		private static bool pointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			float num = 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
			float num2 = ((num >= 0f) ? 1f : (-1f));
			float num3 = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) * num2;
			float num4 = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) * num2;
			return num3 > 0f && num4 > 0f && num3 + num4 < 2f * num * num2;
		}

		public void Sync(ICollider source)
		{
			ExtrudedPolygon extrudedPolygon = source as ExtrudedPolygon;
			this.SourceInstanceId = source.SourceInstanceId;
			this.position = extrudedPolygon.position;
			this.rotation = extrudedPolygon.rotation;
			this.top = extrudedPolygon.top;
			this.bottom = extrudedPolygon.bottom;
		}

		public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = this._invRotation * (point - this.position);
			if (vector.y > this.bottom && vector.y < this.top)
			{
				Vector2 vector2;
				vector2..ctor(vector.x, vector.z);
				for (int i = 0; i < this.triangles.Count; i += 3)
				{
					if (ExtrudedPolygon.pointInTriangle(vector2, this.vertices[this.triangles[i]], this.vertices[this.triangles[i + 1]], this.vertices[this.triangles[i + 2]]))
					{
						Vector3 vector3 = this._invRotation * (prevPoint - this.position);
						if (vector3.y >= this.top && vector.y < this.top)
						{
							normal = Vector3.up;
							Vector3 vector4;
							vector4..ctor(vector.x, this.top, vector.z);
							vector4 = this.rotation * vector4 + this.position;
							return vector4 - point;
						}
						if (vector3.y <= this.bottom && vector.y > this.bottom)
						{
							normal = Vector3.down;
							Vector3 vector5;
							vector5..ctor(vector.x, this.bottom, vector.z);
							vector5 = this.rotation * vector5 + this.position;
							return vector5 - point;
						}
						Vector3 vector6 = vector - vector3;
						bool flag = false;
						Vector2 vector7 = this.vertices[this.triangles[i]];
						Vector2 vector8 = this.vertices[this.triangles[i + 1]];
						Vector2 vector9 = this.vertices[this.triangles[i + 2]];
						Vector3 vector10 = Vector3.zero;
						Vector3 vector11 = Vector3.zero;
						float num;
						float num2;
						if (Math3d.SegmentSegmentIntersectionParams2D(vector, vector3, vector7, vector8, out num, out num2) && num > 0f && num2 >= 0f && num2 <= 1f)
						{
							vector10 = vector7;
							vector11 = vector8;
							flag = true;
						}
						if (!flag && Math3d.SegmentSegmentIntersectionParams2D(vector, vector3, vector8, vector9, out num, out num2) && num > 0f && num2 >= 0f && num2 <= 1f)
						{
							vector10 = vector8;
							vector11 = vector9;
							flag = true;
						}
						if (!flag && Math3d.SegmentSegmentIntersectionParams2D(vector, vector3, vector9, vector7, out num, out num2) && num > 0f && num2 >= 0f && num2 <= 1f)
						{
							vector10 = vector9;
							vector11 = vector7;
							flag = true;
						}
						if (flag)
						{
							Vector2 vector12 = Vector2.Lerp(vector10, vector11, num2);
							Vector3 vector13;
							vector13..ctor(vector12.x, vector.y, vector12.y);
							vector13 = this.rotation * vector13 + this.position;
							Vector3 vector14 = vector11 - vector10;
							normal = Vector3.Cross(vector6, vector14).normalized;
							if (Vector3.Dot(normal, vector6) > 0f)
							{
								normal = -normal;
							}
						}
					}
				}
			}
			normal = Vector3.zero;
			return Vector3.zero;
		}

		public void DebugDraw(Color c)
		{
		}

		public Vector3 position;

		public float top;

		public float bottom;

		protected Quaternion _rotation;

		protected Quaternion _invRotation;

		protected List<Vector2> vertices;

		protected List<int> triangles;

		public const int MaxTriangles = 128;
	}
}
