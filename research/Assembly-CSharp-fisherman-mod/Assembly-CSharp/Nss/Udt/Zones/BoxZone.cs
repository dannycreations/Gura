using System;
using UnityEngine;

namespace Nss.Udt.Zones
{
	[RequireComponent(typeof(BoxCollider))]
	public class BoxZone : Zone
	{
		public Vector3 Size
		{
			get
			{
				return base.GetComponent<BoxCollider>().size;
			}
		}

		public Vector3[] Vertices
		{
			get
			{
				Vector3 vector;
				vector..ctor(base.transform.position.x - this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y - this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z - this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector2;
				vector2..ctor(base.transform.position.x - this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y + this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z - this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector3;
				vector3..ctor(base.transform.position.x + this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y + this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z - this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector4;
				vector4..ctor(base.transform.position.x + this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y - this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z - this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector5;
				vector5..ctor(base.transform.position.x - this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y - this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z + this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector6;
				vector6..ctor(base.transform.position.x - this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y + this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z + this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector7;
				vector7..ctor(base.transform.position.x + this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y + this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z + this.Size.z / 2f * base.transform.localScale.z);
				Vector3 vector8;
				vector8..ctor(base.transform.position.x + this.Size.x / 2f * base.transform.localScale.x, base.transform.position.y - this.Size.y / 2f * base.transform.localScale.y, base.transform.position.z + this.Size.z / 2f * base.transform.localScale.z);
				vector = BoxZone.RotateAroundPoint(vector, base.transform.position, base.transform.rotation);
				vector2 = BoxZone.RotateAroundPoint(vector2, base.transform.position, base.transform.rotation);
				vector3 = BoxZone.RotateAroundPoint(vector3, base.transform.position, base.transform.rotation);
				vector4 = BoxZone.RotateAroundPoint(vector4, base.transform.position, base.transform.rotation);
				vector5 = BoxZone.RotateAroundPoint(vector5, base.transform.position, base.transform.rotation);
				vector6 = BoxZone.RotateAroundPoint(vector6, base.transform.position, base.transform.rotation);
				vector7 = BoxZone.RotateAroundPoint(vector7, base.transform.position, base.transform.rotation);
				vector8 = BoxZone.RotateAroundPoint(vector8, base.transform.position, base.transform.rotation);
				return new Vector3[] { vector, vector2, vector3, vector4, vector5, vector6, vector7, vector8 };
			}
		}

		private void OnDrawGizmos()
		{
			this.DrawMeshNow();
		}

		private void DrawMeshNow()
		{
			Mesh mesh = new Mesh
			{
				hideFlags = 61
			};
			Material material = new Material(Shader.Find("Custom/TranspUnlit"))
			{
				hideFlags = 61
			};
			material.color = this.color;
			mesh.vertices = this.Vertices;
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
			for (int j = 0; j < material.passCount; j++)
			{
				if (material.SetPass(j))
				{
					Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
				}
			}
			Object.DestroyImmediate(material);
			Object.DestroyImmediate(mesh);
		}

		private static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion rotation)
		{
			return rotation * (point - pivot) + pivot;
		}
	}
}
