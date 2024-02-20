using System;
using UnityEngine;

public class TerrainTest : MonoBehaviour
{
	private void Start()
	{
		if (this.leftBottom == null)
		{
			this.leftBottom = new GameObject("TerrainTest_leftBottom")
			{
				transform = 
				{
					position = new Vector3(-100f, 0f, -100f)
				}
			}.transform;
		}
		if (this.rightTop == null)
		{
			this.rightTop = new GameObject("TerrainTest_rightTop")
			{
				transform = 
				{
					position = new Vector3(100f, 0f, 100f)
				}
			}.transform;
		}
		this.terrain = base.GetComponent<Terrain>();
		this.map = new Vector3[this.resolution, this.resolution];
	}

	private Vector3 TestPoint(Vector3 origin)
	{
		float num = this.terrain.terrainData.size.x / (float)this.terrain.terrainData.heightmapWidth;
		float num2 = this.terrain.terrainData.size.z / (float)this.terrain.terrainData.heightmapHeight;
		Vector3 vector = origin - this.terrain.transform.position;
		float num3 = vector.x / num;
		float num4 = vector.z / num2;
		int num5 = (int)num3;
		int num6 = (int)num4;
		float num7 = num3 - (float)num5;
		float num8 = num4 - (float)num6;
		float height = this.terrain.terrainData.GetHeight(num5, num6);
		float height2 = this.terrain.terrainData.GetHeight(num5, num6 + 1);
		float height3 = this.terrain.terrainData.GetHeight(num5 + 1, num6 + 1);
		float height4 = this.terrain.terrainData.GetHeight(num5 + 1, num6);
		return new Vector3(origin.x, MathHelper.InterpolateTriangular(height, height4, height3, height2, num7, num8), origin.z);
	}

	private void Update()
	{
		if (this.leftBottom != null && this.rightTop != null)
		{
			if (this.map.GetLength(0) != this.resolution || this.map.GetLength(1) != this.resolution)
			{
				this.map = new Vector3[this.resolution, this.resolution];
			}
			for (int i = 0; i < this.resolution; i++)
			{
				for (int j = 0; j < this.resolution; j++)
				{
					Vector3 vector;
					vector..ctor(Mathf.Lerp(this.leftBottom.position.x, this.rightTop.position.x, (float)i / (float)this.resolution), 0f, Mathf.Lerp(this.leftBottom.position.z, this.rightTop.position.z, (float)j / (float)this.resolution));
					this.map[i, j] = this.TestPoint(vector);
				}
			}
			for (int k = 1; k < this.resolution; k++)
			{
				for (int l = 1; l < this.resolution; l++)
				{
					Debug.DrawLine(this.map[k, l], this.map[k - 1, l], Color.green);
					Debug.DrawLine(this.map[k - 1, l], this.map[k - 1, l - 1], Color.green);
					Debug.DrawLine(this.map[k - 1, l - 1], this.map[k, l - 1], Color.green);
					Debug.DrawLine(this.map[k, l - 1], this.map[k, l], Color.green);
				}
			}
		}
	}

	public Transform leftBottom;

	public Transform rightTop;

	public int resolution;

	private Terrain terrain;

	private Vector3[,] map;
}
