﻿using System;
using UnityEngine;

public class SearchTexture
{
	public SearchTexture()
	{
		this.alphaTex = new Texture2D(64, 16, 1, false);
		this.alphaTex.wrapMode = 0;
		this.alphaTex.anisoLevel = 0;
		this.alphaTex.filterMode = 0;
		for (int i = 0; i < 1024; i++)
		{
			int num = i % 64;
			int num2 = i / 64;
			float num3 = (float)SearchTexture.searchTexBytes[i] / 255f;
			this.alphaTex.SetPixel(num, num2, new Color(0f, 0f, 0f, num3));
		}
		this.alphaTex.Apply();
	}

	public Texture2D alphaTex;

	public static byte[] searchTexBytes = new byte[]
	{
		254, 254, 0, 127, 127, 0, 0, 254, 254, 0,
		127, 127, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 127, 127, 0, 127, 127, 0, 0, 127, 127,
		0, 127, 127, 254, 127, 0, 0, 0, 0, 0,
		127, 127, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 254, 254, 0, 127, 127, 0,
		0, 254, 254, 0, 127, 127, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 127, 127, 0, 127, 127,
		0, 0, 127, 127, 0, 127, 127, 254, 127, 0,
		0, 0, 0, 0, 127, 127, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 254, 254, 0, 127, 127, 0, 0, 254,
		254, 0, 127, 127, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 127, 127, 0, 127, 127, 0, 0,
		127, 127, 0, 127, 127, 254, 127, 0, 0, 0,
		0, 0, 127, 127, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 254, 254, 0, 127,
		127, 0, 0, 254, 254, 0, 127, 127, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 127, 127, 0,
		127, 127, 0, 0, 127, 127, 0, 127, 127, 254,
		127, 0, 0, 0, 0, 0, 127, 127, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 127, 127,
		0, 127, 127, 0, 0, 127, 127, 0, 127, 127,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 127,
		127, 0, 127, 127, 0, 0, 127, 127, 0, 127,
		127, 127, 127, 0, 0, 0, 0, 0, 127, 127,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 127, 127, 0, 127, 127, 0, 0, 127,
		127, 0, 127, 127, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 127, 127, 0, 127, 127, 0, 0,
		127, 127, 0, 127, 127, 127, 127, 0, 0, 0,
		0, 0, 127, 127, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		127, 127, 0, 127, 127, 0, 0, 127, 127, 0,
		127, 127, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 127, 127, 0, 127, 127, 0, 0, 127, 127,
		0, 127, 127, 127, 127, 0, 0, 0, 0, 0,
		127, 127, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 127, 127, 0, 127, 127, 0,
		0, 127, 127, 0, 127, 127, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 127, 127, 0, 127, 127,
		0, 0, 127, 127, 0, 127, 127, 127, 127, 0,
		0, 0, 0, 0, 127, 127, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0
	};
}