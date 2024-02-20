using System;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGeometry
{
	public class MutableMesh
	{
		public MutableMesh(int submeshesCount = 1)
		{
			this.vertices = new List<Vector3>();
			this.normals = new List<Vector3>();
			this.uv = new List<Vector2>();
			this.tangents = new List<Vector4>();
			this.submeshes = new List<List<int>>();
			for (int i = 0; i < submeshesCount; i++)
			{
				this.submeshes.Add(new List<int>());
			}
		}

		public MutableMesh(ProceduralGeometry.MutableMesh source)
		{
			this.vertices = new List<Vector3>(source.vertices);
			this.normals = new List<Vector3>(source.normals);
			this.uv = new List<Vector2>(source.uv);
			this.tangents = new List<Vector4>(source.tangents);
			this.submeshes = new List<List<int>>(source.submeshes);
		}

		public ProceduralGeometry.MutableMesh ExtractSubmesh(int index)
		{
			ProceduralGeometry.MutableMesh mutableMesh = new ProceduralGeometry.MutableMesh(1);
			if (index < this.submeshes.Count)
			{
				HashSet<int> hashSet = new HashSet<int>();
				foreach (int num in this.submeshes[index])
				{
					hashSet.Add(num);
				}
				List<int> list = new List<int>(hashSet);
				foreach (int num2 in list)
				{
					mutableMesh.vertices.Add(this.vertices[num2]);
					mutableMesh.normals.Add(this.normals[num2]);
					mutableMesh.uv.Add(this.uv[num2]);
					mutableMesh.tangents.Add(this.tangents[num2]);
				}
				foreach (int num3 in this.submeshes[index])
				{
					mutableMesh.triangles.Add(list.IndexOf(num3));
				}
			}
			return mutableMesh;
		}

		public List<int> triangles
		{
			get
			{
				return this.submeshes[0];
			}
			set
			{
				this.submeshes[0] = value;
			}
		}

		public List<int> allTriangles()
		{
			List<int> list = new List<int>();
			foreach (List<int> list2 in this.submeshes)
			{
				list.AddRange(list2);
			}
			return list;
		}

		public Mesh MakeMesh()
		{
			Mesh mesh = new Mesh();
			mesh.vertices = this.vertices.ToArray();
			mesh.subMeshCount = this.submeshes.Count;
			for (int i = 0; i < this.submeshes.Count; i++)
			{
				mesh.SetTriangles(this.submeshes[i].ToArray(), i);
			}
			mesh.RecalculateNormals();
			return mesh;
		}

		public void Merge(ProceduralGeometry.MutableMesh mesh, int submeshIndexOffset = 0)
		{
			int count = this.vertices.Count;
			this.vertices.AddRange(mesh.vertices);
			this.normals.AddRange(mesh.normals);
			this.uv.AddRange(mesh.uv);
			this.tangents.AddRange(mesh.tangents);
			for (int i = 0; i < mesh.submeshes.Count; i++)
			{
				if (i + submeshIndexOffset >= this.submeshes.Count)
				{
					break;
				}
				foreach (int num in mesh.submeshes[i])
				{
					this.submeshes[i + submeshIndexOffset].Add(num + count);
				}
			}
		}

		public void CalculateTangents()
		{
			List<int> list = this.allTriangles();
			int count = list.Count;
			int count2 = this.vertices.Count;
			Vector3[] array = new Vector3[count2];
			Vector3[] array2 = new Vector3[count2];
			this.tangents = new List<Vector4>();
			for (int i = 0; i < count; i += 3)
			{
				int num = list[i];
				int num2 = list[i + 1];
				int num3 = list[i + 2];
				Vector3 vector = this.vertices[num];
				Vector3 vector2 = this.vertices[num2];
				Vector3 vector3 = this.vertices[num3];
				Vector2 vector4 = this.uv[num];
				Vector2 vector5 = this.uv[num2];
				Vector2 vector6 = this.uv[num3];
				float num4 = vector2.x - vector.x;
				float num5 = vector3.x - vector.x;
				float num6 = vector2.y - vector.y;
				float num7 = vector3.y - vector.y;
				float num8 = vector2.z - vector.z;
				float num9 = vector3.z - vector.z;
				float num10 = vector5.x - vector4.x;
				float num11 = vector6.x - vector4.x;
				float num12 = vector5.y - vector4.y;
				float num13 = vector6.y - vector4.y;
				float num14 = 1f / (num10 * num13 - num11 * num12);
				Vector3 vector7;
				vector7..ctor((num13 * num4 - num12 * num5) * num14, (num13 * num6 - num12 * num7) * num14, (num13 * num8 - num12 * num9) * num14);
				Vector3 vector8;
				vector8..ctor((num10 * num5 - num11 * num4) * num14, (num10 * num7 - num11 * num6) * num14, (num10 * num9 - num11 * num8) * num14);
				array[num] += vector7;
				array[num2] += vector7;
				array[num3] += vector7;
				array2[num] += vector8;
				array2[num2] += vector8;
				array2[num3] += vector8;
			}
			for (int j = 0; j < count2; j++)
			{
				Vector3 vector9 = this.normals[j];
				Vector3 vector10 = array[j];
				Vector3.OrthoNormalize(ref vector9, ref vector10);
				this.tangents.Add(new Vector4(vector10.x, vector10.y, vector10.z, (Vector3.Dot(Vector3.Cross(vector9, vector10), array2[j]) >= 0f) ? 1f : (-1f)));
			}
		}

		public List<Vector3> vertices;

		public List<Vector3> normals;

		public List<Vector2> uv;

		public List<Vector4> tangents;

		public List<List<int>> submeshes;
	}
}
