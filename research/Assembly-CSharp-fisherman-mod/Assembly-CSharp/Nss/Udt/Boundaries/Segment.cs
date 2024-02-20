using System;
using UnityEngine;

namespace Nss.Udt.Boundaries
{
	[Serializable]
	public class Segment
	{
		public Vector3 Midpoint
		{
			get
			{
				return Vector3.Lerp(this.start, this.end, 0.5f);
			}
		}

		public Vector3 GetSize(float height, float depth)
		{
			return new Vector3(Vector3.Distance(this.start, this.end), height, depth);
		}

		public Vector3 GetCenter(float height, float depth, DepthAnchorTypes depthAnchor)
		{
			Vector3 vector = default(Vector3);
			if (depthAnchor != DepthAnchorTypes.Middle)
			{
				if (depthAnchor != DepthAnchorTypes.Left)
				{
					if (depthAnchor == DepthAnchorTypes.Right)
					{
						vector..ctor(0f, height / 2f, depth / 2f * -1f);
					}
				}
				else
				{
					vector..ctor(0f, height / 2f, depth / 2f);
				}
			}
			else
			{
				vector..ctor(0f, height / 2f, 0f);
			}
			return vector;
		}

		public Quaternion GetYAxisRotation()
		{
			Vector3 normalized = (this.start - this.end).normalized;
			return (!(normalized == Vector3.zero)) ? Quaternion.LookRotation(normalized) : Quaternion.identity;
		}

		public Mesh GetMesh(float height, float depth, DepthAnchorTypes depthAnchor)
		{
			Vector3 vector = default(Vector3);
			Vector3 vector2 = default(Vector3);
			Vector3 vector3 = default(Vector3);
			Vector3 vector4 = default(Vector3);
			Vector3 vector5 = default(Vector3);
			Vector3 vector6 = default(Vector3);
			Vector3 vector7 = default(Vector3);
			Vector3 vector8 = default(Vector3);
			Vector3 vector9 = default(Vector3);
			Vector3 vector10 = default(Vector3);
			Vector3 vector11 = default(Vector3);
			Vector3 vector12 = default(Vector3);
			if (depth == 0f)
			{
				vector5 = this.start;
				vector6 = this.start + Vector3.up * height;
				vector7 = this.end + Vector3.up * height;
				vector8 = this.end;
				vector9 = this.start;
				vector10 = this.start + Vector3.up * height;
				vector11 = this.end + Vector3.up * height;
				vector12 = this.end;
			}
			else
			{
				if (depthAnchor != DepthAnchorTypes.Middle)
				{
					if (depthAnchor != DepthAnchorTypes.Left)
					{
						if (depthAnchor == DepthAnchorTypes.Right)
						{
							vector = this.start;
							vector2 = this.start;
							vector.x -= depth;
							vector3 = this.end;
							vector4 = this.end;
							vector3.x -= depth;
						}
					}
					else
					{
						vector = this.start;
						vector2 = this.start;
						vector.x += depth;
						vector3 = this.end;
						vector4 = this.end;
						vector3.x += depth;
					}
				}
				else
				{
					vector = this.start;
					vector2 = this.start;
					vector.x -= depth / 2f;
					vector2.x += depth / 2f;
					vector3 = this.end;
					vector4 = this.end;
					vector3.x -= depth / 2f;
					vector4.x += depth / 2f;
				}
				vector = Segment.RotateAroundPoint(vector, this.start, this.GetYAxisRotation());
				vector2 = Segment.RotateAroundPoint(vector2, this.start, this.GetYAxisRotation());
				vector3 = Segment.RotateAroundPoint(vector3, this.end, this.GetYAxisRotation());
				vector4 = Segment.RotateAroundPoint(vector4, this.end, this.GetYAxisRotation());
				vector5 = vector;
				vector6 = vector + Vector3.up * height;
				vector7 = vector3 + Vector3.up * height;
				vector8 = vector3;
				vector9 = vector2;
				vector10 = vector2 + Vector3.up * height;
				vector11 = vector4 + Vector3.up * height;
				vector12 = vector4;
			}
			Mesh mesh = new Mesh
			{
				hideFlags = 61
			};
			mesh.vertices = new Vector3[] { vector5, vector6, vector7, vector8, vector9, vector10, vector11, vector12 };
			mesh.triangles = new int[]
			{
				0, 2, 1, 1, 2, 0, 0, 2, 3, 3,
				2, 0, 4, 6, 5, 5, 6, 4, 4, 6,
				7, 7, 6, 4, 0, 5, 1, 1, 5, 0,
				0, 5, 4, 4, 5, 0, 1, 6, 5, 5,
				6, 1, 1, 6, 2, 2, 6, 1, 0, 7,
				4, 4, 7, 0, 0, 7, 3, 3, 7, 0,
				2, 7, 6, 6, 7, 2, 2, 7, 3, 3,
				7, 2
			};
			Vector3[] vertices = mesh.vertices;
			Vector2[] array = new Vector2[vertices.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector2(vertices[i].x, vertices[i].z);
			}
			mesh.uv = array;
			mesh.RecalculateNormals();
			return mesh;
		}

		private static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion rotation)
		{
			return rotation * (point - pivot) + pivot;
		}

		public string name;

		public Vector3 start;

		public Vector3 end;
	}
}
