﻿using System;
using System.Collections.Generic;
using System.IO;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TestRenderTextureTestHarness : MonoBehaviour
{
	public Texture2D Create3x3Tex()
	{
		Texture2D texture2D = new Texture2D(3, 3, 5, false);
		Color32[] array = new Color32[texture2D.width * texture2D.height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = this.color;
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		return texture2D;
	}

	public Texture2D Create3x3Clone()
	{
		Texture2D texture2D = new Texture2D(3, 3, 5, false);
		Color32[] array = new Color32[]
		{
			new Color32(54, 54, 201, byte.MaxValue),
			new Color32(128, 37, 218, byte.MaxValue),
			new Color32(201, 54, 201, byte.MaxValue),
			new Color32(37, 128, 218, byte.MaxValue),
			new Color32(128, 128, byte.MaxValue, byte.MaxValue),
			new Color32(218, 128, 218, byte.MaxValue),
			new Color32(54, 201, 201, byte.MaxValue),
			new Color32(128, 218, 218, byte.MaxValue),
			new Color32(201, 201, 201, byte.MaxValue)
		};
		texture2D.SetPixels32(array);
		texture2D.Apply();
		return texture2D;
	}

	public static void TestRender(Texture2D input, Texture2D output)
	{
		int num = 1;
		ShaderTextureProperty[] array = new ShaderTextureProperty[]
		{
			new ShaderTextureProperty("_BumpMap", false)
		};
		int width = input.width;
		int height = input.height;
		int num2 = 0;
		Rect[] array2 = new Rect[]
		{
			new Rect(0f, 0f, 1f, 1f)
		};
		List<MB3_TextureCombiner.MB_TexSet> list = new List<MB3_TextureCombiner.MB_TexSet>();
		MB3_TextureCombiner.MeshBakerMaterialTexture[] array3 = new MB3_TextureCombiner.MeshBakerMaterialTexture[]
		{
			new MB3_TextureCombiner.MeshBakerMaterialTexture(input)
		};
		MB3_TextureCombiner.MB_TexSet mb_TexSet = new MB3_TextureCombiner.MB_TexSet(array3, Vector2.zero, Vector2.one);
		list.Add(mb_TexSet);
		GameObject gameObject = new GameObject("MBrenderAtlasesGO");
		MB3_AtlasPackerRenderTexture mb3_AtlasPackerRenderTexture = gameObject.AddComponent<MB3_AtlasPackerRenderTexture>();
		gameObject.AddComponent<Camera>();
		for (int i = 0; i < num; i++)
		{
			Debug.Log(string.Concat(new object[]
			{
				"About to render ",
				array[i].name,
				" isNormal=",
				array[i].isNormalMap
			}));
			mb3_AtlasPackerRenderTexture.LOG_LEVEL = MB2_LogLevel.trace;
			mb3_AtlasPackerRenderTexture.width = width;
			mb3_AtlasPackerRenderTexture.height = height;
			mb3_AtlasPackerRenderTexture.padding = num2;
			mb3_AtlasPackerRenderTexture.rects = array2;
			mb3_AtlasPackerRenderTexture.textureSets = list;
			mb3_AtlasPackerRenderTexture.indexOfTexSetToRender = i;
			mb3_AtlasPackerRenderTexture.isNormalMap = array[i].isNormalMap;
			Texture2D texture2D = mb3_AtlasPackerRenderTexture.OnRenderAtlas(null);
			Debug.Log(string.Concat(new object[]
			{
				"Created atlas ",
				array[i].name,
				" w=",
				texture2D.width,
				" h=",
				texture2D.height,
				" id=",
				texture2D.GetInstanceID()
			}));
			Debug.Log(string.Concat(new object[]
			{
				"Color ",
				texture2D.GetPixel(5, 5),
				" ",
				Color.red
			}));
			byte[] array4 = ImageConversion.EncodeToPNG(texture2D);
			File.WriteAllBytes(Application.dataPath + "/_Experiment/red.png", array4);
		}
	}

	public Texture2D input;

	public bool doColor;

	public Color32 color;
}
