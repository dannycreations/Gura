using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TestTexturePacker : MonoBehaviour
{
	[ContextMenu("Generate List Of Images To Add")]
	public void GenerateListOfImagesToAdd()
	{
		this.imgsToAdd = new List<Vector2>();
		for (int i = 0; i < this.numTex; i++)
		{
			Vector2 vector;
			vector..ctor((float)Mathf.RoundToInt((float)Random.Range(this.min, this.max) * this.xMult), (float)Mathf.RoundToInt((float)Random.Range(this.min, this.max) * this.yMult));
			if (this.imgsMustBePowerOfTwo)
			{
				vector.x = (float)MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)vector.x);
				vector.y = (float)MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)vector.y);
			}
			this.imgsToAdd.Add(vector);
		}
	}

	[ContextMenu("Run")]
	public void RunTestHarness()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = this.doPowerOfTwoTextures;
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(this.imgsToAdd, this.maxDim, this.padding, this.doMultiAtlas);
		if (this.rs != null)
		{
			Debug.Log("NumAtlas= " + this.rs.Length);
			for (int i = 0; i < this.rs.Length; i++)
			{
				for (int j = 0; j < this.rs[i].rects.Length; j++)
				{
					Rect rect = this.rs[i].rects[j];
					rect.x *= (float)this.rs[i].atlasX;
					rect.y *= (float)this.rs[i].atlasY;
					rect.width *= (float)this.rs[i].atlasX;
					rect.height *= (float)this.rs[i].atlasY;
					Debug.Log(rect.ToString("f5"));
				}
				Debug.Log("===============");
			}
			this.res = string.Concat(new object[]
			{
				"mxX= ",
				this.rs[0].atlasX,
				" mxY= ",
				this.rs[0].atlasY
			});
		}
		else
		{
			this.res = "ERROR: PACKING FAILED";
		}
	}

	private void OnDrawGizmos()
	{
		if (this.rs != null)
		{
			for (int i = 0; i < this.rs.Length; i++)
			{
				Vector2 vector;
				vector..ctor((float)i * 1.5f * (float)this.maxDim, 0f);
				AtlasPackingResult atlasPackingResult = this.rs[i];
				Vector2 vector2;
				vector2..ctor(vector.x + (float)(atlasPackingResult.atlasX / 2), vector.y + (float)(atlasPackingResult.atlasY / 2));
				Vector2 vector3;
				vector3..ctor((float)atlasPackingResult.atlasX, (float)atlasPackingResult.atlasY);
				Gizmos.DrawWireCube(vector2, vector3);
				for (int j = 0; j < this.rs[i].rects.Length; j++)
				{
					Rect rect = this.rs[i].rects[j];
					Gizmos.color = new Color(Random.value, Random.value, Random.value);
					vector2..ctor(vector.x + (rect.x + rect.width / 2f) * (float)this.rs[i].atlasX, vector.y + (rect.y + rect.height / 2f) * (float)this.rs[i].atlasY);
					Vector2 vector4;
					vector4..ctor(rect.width * (float)this.rs[i].atlasX, rect.height * (float)this.rs[i].atlasY);
					Gizmos.DrawCube(vector2, vector4);
				}
			}
		}
	}

	[ContextMenu("Test1")]
	private void Test1()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = true;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 80f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	[ContextMenu("Test2")]
	private void Test2()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = true;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(80f, 450f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	[ContextMenu("Test3")]
	private void Test3()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = false;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 80f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	[ContextMenu("Test4")]
	private void Test4()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = false;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(80f, 450f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	private MB2_TexturePacker texturePacker;

	public int numTex = 32;

	public int min = 126;

	public int max = 2046;

	public float xMult = 1f;

	public float yMult = 1f;

	public bool imgsMustBePowerOfTwo;

	public List<Vector2> imgsToAdd = new List<Vector2>();

	public int padding = 1;

	public int maxDim = 4096;

	public bool doPowerOfTwoTextures = true;

	public bool doMultiAtlas;

	public MB2_LogLevel logLevel;

	public string res;

	public AtlasPackingResult[] rs;
}
