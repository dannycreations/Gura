using System;
using UnityEngine;

[ExecuteInEditMode]
public class LakeWaterMesh : MonoBehaviour
{
	private void Start()
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		int num = this.gridKnotsX * this.gridKnotsZ;
		Vector3[] array = new Vector3[num];
		float num2 = this.gridSizeZ / (float)(this.gridKnotsZ - 3);
		float num3 = this.gridSizeX / (float)(this.gridKnotsX - 3);
		for (int i = 0; i < this.gridKnotsZ; i++)
		{
			for (int j = 0; j < this.gridKnotsX; j++)
			{
				array[i * this.gridKnotsX + j].x = (float)(j - 1) * num3;
				array[i * this.gridKnotsX + j].y = 0f;
				array[i * this.gridKnotsX + j].z = (float)(i - 1) * num2;
			}
		}
		for (int k = 0; k < this.gridKnotsZ; k++)
		{
			array[k * this.gridKnotsX].y = -this.skirtDepth;
			Vector3[] array2 = array;
			int num4 = k * this.gridKnotsX;
			array2[num4].x = array2[num4].x + num3;
			array[k * this.gridKnotsX + this.gridKnotsX - 1].y = -this.skirtDepth;
			Vector3[] array3 = array;
			int num5 = k * this.gridKnotsX + this.gridKnotsX - 1;
			array3[num5].x = array3[num5].x - num3;
		}
		for (int l = 0; l < this.gridKnotsX; l++)
		{
			array[l].y = -this.skirtDepth;
			Vector3[] array4 = array;
			int num6 = l;
			array4[num6].z = array4[num6].z + num2;
			array[(this.gridKnotsZ - 1) * this.gridKnotsX + l].y = -this.skirtDepth;
			Vector3[] array5 = array;
			int num7 = (this.gridKnotsZ - 1) * this.gridKnotsX + l;
			array5[num7].z = array5[num7].z - num2;
		}
		mesh.vertices = array;
		int num8 = ((this.gridKnotsX - 1) * this.gridKnotsZ + (this.gridKnotsZ - 1)) * 6 + 6;
		int[] array6 = new int[num8];
		for (int m = 0; m < this.gridKnotsZ - 1; m++)
		{
			for (int n = 0; n < this.gridKnotsX - 1; n++)
			{
				array6[(m * (this.gridKnotsX - 1) + n) * 6] = n + m * this.gridKnotsX;
				array6[(m * (this.gridKnotsX - 1) + n) * 6 + 1] = n + (m + 1) * this.gridKnotsX;
				array6[(m * (this.gridKnotsX - 1) + n) * 6 + 2] = n + 1 + m * this.gridKnotsX;
				array6[(m * (this.gridKnotsX - 1) + n) * 6 + 3] = n + 1 + m * this.gridKnotsX;
				array6[(m * (this.gridKnotsX - 1) + n) * 6 + 4] = n + (m + 1) * this.gridKnotsX;
				array6[(m * (this.gridKnotsX - 1) + n) * 6 + 5] = n + 1 + (m + 1) * this.gridKnotsX;
			}
		}
		mesh.triangles = array6;
		mesh.RecalculateNormals();
		MeshFilter meshFilter = (MeshFilter)base.gameObject.GetComponent(typeof(MeshFilter));
		meshFilter.mesh = mesh;
	}

	public float gridSizeX = 10f;

	public float gridSizeZ = 10f;

	public int gridKnotsX = 10;

	public int gridKnotsZ = 10;

	public float skirtDepth = 0.05f;
}
