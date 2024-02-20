using System;
using UnityEngine;

[ExecuteInEditMode]
public class WaterRipples : MonoBehaviour
{
	private static float Clamp(float x)
	{
		return Mathf.Clamp(x, -1f, 1f);
	}

	private void Splash(Vector3 pos)
	{
		for (int i = -6; i <= 6; i++)
		{
			for (int j = -6; j <= 6; j++)
			{
				float num = Mathf.Sqrt((float)(i * i + j * j));
				if (num > 6f)
				{
					num = 0f;
				}
				else
				{
					num = (6f - num) / 6f;
				}
				int num2 = j + (int)pos.x + (i + (int)pos.z) * this.ripplesGridSize;
				if (num2 > 0 && num2 < this.ripplesGridSize * this.ripplesGridSize)
				{
					this.heightCur[num2] += num * num * 8f;
				}
			}
		}
	}

	private void Start()
	{
		this.ripplesHeightMapTex = new Texture2D(this.ripplesGridSize, this.ripplesGridSize, 4, false);
		this.ripplesHeightMap = new Color32[this.ripplesGridSize * this.ripplesGridSize];
		this.heightCur = new float[this.ripplesGridSize * this.ripplesGridSize];
		this.heightOld = new float[this.ripplesGridSize * this.ripplesGridSize];
		this.vertSpeed = new float[this.ripplesGridSize * this.ripplesGridSize];
	}

	private void Update()
	{
		for (int i = 0; i < this.ripplesGridSize * this.ripplesGridSize; i++)
		{
			this.heightOld[i] = this.heightCur[i];
		}
		if (Input.GetMouseButton(2))
		{
			for (int j = 0; j < this.ripplesGridSize * this.ripplesGridSize; j++)
			{
				this.heightOld[j] = 0f;
				this.heightCur[j] = 0f;
				this.vertSpeed[j] = 0f;
			}
		}
		if (Input.GetMouseButton(1))
		{
			float num = Input.GetAxis("Mouse X");
			float num2 = Input.GetAxis("Mouse Y");
			num = Input.mousePosition.x;
			num2 = Input.mousePosition.y;
			this.Splash(new Vector3(num, 0f, num2));
		}
		GameObject gameObject = GameObject.Find("TestRipple");
		if (gameObject && Mathf.Abs(gameObject.transform.position.y) < 0.5f)
		{
			this.Splash(gameObject.transform.position);
		}
		for (int k = 0; k < this.ripplesGridSize; k++)
		{
			this.heightCur[k] = 0f;
			this.vertSpeed[k] = 0f;
			this.heightCur[this.ripplesGridSize * (this.ripplesGridSize - 1) + k] = 0f;
			this.vertSpeed[this.ripplesGridSize * (this.ripplesGridSize - 1) + k] = 0f;
			this.heightCur[this.ripplesGridSize * k] = 0f;
			this.vertSpeed[this.ripplesGridSize * k] = 0f;
			this.heightCur[this.ripplesGridSize * k + this.ripplesGridSize - 1] = 0f;
			this.vertSpeed[this.ripplesGridSize * k + this.ripplesGridSize - 1] = 0f;
		}
		for (int l = 1; l < this.ripplesGridSize - 1; l++)
		{
			for (int m = 1; m < this.ripplesGridSize - 1; m++)
			{
				int num3 = m + l * this.ripplesGridSize;
				float num4 = (this.heightOld[num3 - 1] + this.heightOld[num3 + 1] + this.heightOld[num3 - this.ripplesGridSize] + this.heightOld[num3 + this.ripplesGridSize]) * 0.25f;
				this.vertSpeed[num3] += (num4 - this.heightOld[num3]) * this.dempfFactor1;
				this.vertSpeed[num3] *= this.dempfFactor2;
				this.heightCur[num3] += this.vertSpeed[num3];
				this.ripplesHeightMap[num3].a = (byte)(WaterRipples.Clamp(this.vertSpeed[num3]) * 127f + 128f);
			}
		}
		for (int n = 0; n < this.ripplesGridSize; n++)
		{
			for (int num5 = 0; num5 < this.ripplesGridSize; num5++)
			{
				int num6 = num5 + n * this.ripplesGridSize;
				this.ripplesHeightMap[num6].g = (byte)(WaterRipples.Clamp(this.heightCur[num6] * this.vertScale) * 127f + 127f);
				this.ripplesHeightMap[num6].r = (byte)(WaterRipples.Clamp(this.heightCur[(num6 + 1) % (this.ripplesGridSize * this.ripplesGridSize)] - this.heightCur[num6]) * 127f + 127f);
				this.ripplesHeightMap[num6].b = (byte)(WaterRipples.Clamp(this.heightCur[(num6 + this.ripplesGridSize) % (this.ripplesGridSize * this.ripplesGridSize)] - this.heightCur[num6]) * 127f + 127f);
			}
		}
		if (this.ripplesHeightMapTex != null)
		{
			this.ripplesHeightMapTex.SetPixels32(this.ripplesHeightMap);
			this.ripplesHeightMapTex.Apply();
			base.GetComponent<Renderer>().sharedMaterial.SetTexture("_Ripples", this.ripplesHeightMapTex);
		}
	}

	private int ripplesGridSize = 512;

	public Texture2D ripplesHeightMapTex;

	private Color32[] ripplesHeightMap;

	private float[] heightCur;

	private float[] heightOld;

	private float[] vertSpeed;

	public float dempfFactor1 = 0.5f;

	public float dempfFactor2 = 0.995f;

	public float vertScale = 0.05f;
}
