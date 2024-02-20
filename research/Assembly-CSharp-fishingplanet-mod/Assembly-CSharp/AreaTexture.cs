﻿using System;
using UnityEngine;

public class AreaTexture
{
	public AreaTexture()
	{
		this.alphaTex = new Texture2D(160, 560, 1, false);
		this.alphaTex.wrapMode = 0;
		this.alphaTex.anisoLevel = 0;
		this.alphaTex.filterMode = 0;
		this.luminTex = new Texture2D(160, 560, 1, false);
		this.luminTex.wrapMode = 0;
		this.luminTex.anisoLevel = 0;
		this.luminTex.filterMode = 0;
		for (int i = 0; i < 89600; i++)
		{
			int num = i % 160;
			int num2 = i / 160;
			float num3 = (float)AreaTexture.areaTexBytes[i * 2] / 255f;
			float num4 = (float)AreaTexture.areaTexBytes[i * 2 + 1] / 255f;
			this.alphaTex.SetPixel(num, num2, new Color(0f, 0f, 0f, num3));
			this.luminTex.SetPixel(num, num2, new Color(0f, 0f, 0f, num4));
		}
		this.alphaTex.Apply();
		this.luminTex.Apply();
	}

	public Texture2D alphaTex;

	public Texture2D luminTex;

	private static readonly byte[] areaTexBytes = new byte[]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 31, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 31, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		64, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		68, 123, 65, 93, 66, 84, 65, 79, 66, 75,
		66, 73, 66, 72, 65, 71, 66, 70, 66, 69,
		66, 69, 66, 68, 66, 68, 66, 68, 66, 67,
		65, 67, 66, 67, 66, 67, 66, 67, 66, 67,
		4, 127, 34, 61, 43, 61, 48, 61, 51, 61,
		53, 61, 55, 61, 56, 61, 57, 61, 57, 61,
		58, 61, 58, 61, 59, 61, 59, 61, 59, 61,
		60, 61, 60, 61, 60, 61, 60, 61, 60, 61,
		64, 127, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 63, 0, 10, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 63, 0, 10, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		98, 32, 77, 42, 72, 48, 68, 51, 68, 53,
		68, 54, 67, 55, 66, 56, 67, 57, 66, 57,
		66, 58, 66, 58, 66, 59, 66, 59, 66, 59,
		65, 59, 66, 60, 66, 60, 66, 60, 66, 60,
		0, 93, 12, 73, 22, 67, 29, 65, 34, 64,
		38, 63, 41, 63, 44, 63, 46, 62, 47, 62,
		49, 62, 50, 62, 51, 62, 51, 62, 52, 62,
		53, 62, 53, 62, 54, 62, 55, 62, 55, 62,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 102, 0, 63, 0, 3, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 102, 0, 63, 0,
		3, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		109, 11, 87, 22, 79, 29, 74, 34, 72, 38,
		71, 41, 70, 43, 68, 45, 68, 47, 68, 48,
		68, 49, 68, 50, 67, 51, 67, 52, 67, 52,
		66, 53, 67, 54, 66, 54, 66, 55, 66, 55,
		0, 104, 6, 84, 13, 75, 20, 71, 25, 68,
		29, 67, 32, 65, 35, 65, 38, 64, 39, 64,
		41, 63, 43, 63, 44, 63, 45, 63, 46, 63,
		47, 63, 48, 63, 48, 62, 49, 62, 50, 62,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 114, 0, 92, 0, 45, 0, 1,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 114, 0, 92, 0,
		45, 0, 1, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		66, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		114, 6, 95, 13, 86, 20, 79, 25, 77, 29,
		74, 32, 73, 35, 71, 37, 70, 39, 70, 41,
		69, 42, 69, 44, 68, 45, 68, 46, 68, 47,
		67, 48, 68, 48, 68, 49, 67, 50, 67, 51,
		0, 109, 4, 91, 9, 81, 14, 76, 19, 72,
		23, 70, 26, 68, 29, 67, 31, 66, 34, 66,
		36, 65, 37, 65, 39, 64, 40, 64, 41, 64,
		42, 63, 43, 63, 44, 63, 45, 63, 46, 63,
		66, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 120, 0, 106, 0, 72, 0, 34,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 120, 0, 106, 0,
		72, 0, 34, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		117, 3, 100, 9, 90, 14, 84, 19, 81, 23,
		78, 26, 76, 29, 73, 31, 73, 34, 72, 35,
		71, 37, 70, 38, 70, 40, 69, 41, 69, 42,
		68, 43, 68, 44, 68, 45, 68, 46, 68, 46,
		0, 112, 2, 96, 7, 86, 11, 80, 15, 76,
		18, 73, 22, 71, 24, 70, 27, 69, 29, 68,
		31, 67, 33, 66, 34, 66, 36, 65, 37, 65,
		38, 64, 39, 64, 40, 64, 41, 64, 42, 63,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 122, 0, 113, 0, 89, 0, 58,
		0, 27, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 122, 0, 113, 0,
		89, 0, 58, 0, 27, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		119, 2, 104, 6, 95, 11, 88, 15, 84, 18,
		81, 21, 79, 24, 76, 27, 75, 29, 74, 31,
		73, 33, 72, 34, 72, 35, 71, 37, 70, 38,
		69, 39, 69, 40, 69, 41, 69, 42, 68, 43,
		0, 114, 2, 100, 5, 91, 8, 84, 12, 80,
		15, 77, 18, 74, 20, 72, 23, 71, 25, 70,
		27, 69, 29, 68, 31, 68, 32, 66, 33, 66,
		34, 66, 36, 65, 37, 65, 38, 65, 39, 64,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 124, 0, 117, 0, 99, 0, 74,
		0, 48, 0, 22, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 124, 0, 117, 0,
		99, 0, 74, 0, 48, 0, 22, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		120, 2, 107, 5, 98, 8, 91, 12, 87, 15,
		84, 18, 81, 20, 78, 23, 77, 25, 76, 27,
		75, 29, 74, 30, 73, 32, 72, 33, 72, 34,
		70, 36, 70, 37, 70, 38, 70, 39, 69, 39,
		0, 116, 1, 102, 4, 94, 7, 87, 10, 83,
		13, 79, 16, 77, 18, 75, 20, 73, 22, 72,
		24, 70, 26, 69, 27, 69, 29, 68, 30, 68,
		32, 67, 33, 66, 34, 66, 35, 66, 36, 65,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 124, 0, 120, 0, 106, 0, 86,
		0, 63, 0, 40, 0, 19, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 124, 0, 120, 0,
		106, 0, 86, 0, 63, 0, 40, 0, 19, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		121, 1, 109, 4, 101, 7, 94, 10, 90, 13,
		86, 15, 84, 18, 81, 20, 79, 22, 78, 24,
		76, 26, 75, 27, 75, 29, 74, 30, 73, 31,
		72, 33, 72, 34, 72, 35, 71, 35, 70, 37,
		0, 117, 1, 105, 3, 97, 6, 90, 8, 86,
		11, 82, 13, 79, 16, 77, 18, 75, 20, 74,
		22, 72, 23, 71, 25, 70, 26, 69, 28, 69,
		29, 68, 30, 68, 31, 68, 32, 67, 33, 66,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 125, 0, 121, 0, 110, 0, 94,
		0, 75, 0, 55, 0, 35, 0, 16, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 125, 0, 121, 0,
		110, 0, 94, 0, 75, 0, 55, 0, 35, 0,
		16, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		122, 1, 111, 3, 103, 6, 96, 8, 93, 11,
		89, 13, 86, 16, 83, 17, 81, 20, 80, 21,
		78, 23, 77, 25, 76, 26, 75, 27, 75, 29,
		72, 30, 73, 31, 73, 32, 72, 33, 72, 34,
		0, 118, 1, 107, 3, 99, 5, 93, 7, 88,
		9, 84, 12, 82, 14, 79, 16, 77, 17, 76,
		19, 74, 21, 73, 23, 72, 24, 71, 25, 70,
		27, 69, 28, 69, 29, 68, 30, 68, 31, 68,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 125, 0, 122, 0, 114, 0, 100,
		0, 84, 0, 66, 0, 48, 0, 31, 0, 14,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 125, 0, 122, 0,
		114, 0, 100, 0, 84, 0, 66, 0, 48, 0,
		31, 0, 14, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		123, 1, 113, 2, 105, 5, 99, 7, 95, 9,
		91, 12, 88, 14, 85, 16, 83, 17, 82, 19,
		80, 21, 78, 22, 78, 24, 76, 25, 76, 26,
		74, 28, 75, 29, 73, 30, 73, 31, 73, 32,
		0, 119, 0, 108, 2, 101, 4, 95, 6, 90,
		8, 87, 10, 84, 12, 81, 14, 79, 16, 77,
		17, 76, 19, 74, 21, 73, 22, 72, 24, 72,
		24, 70, 26, 70, 27, 69, 28, 69, 29, 69,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 123, 0, 116, 0, 105,
		0, 91, 0, 75, 0, 59, 0, 43, 0, 27,
		0, 13, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 123, 0,
		116, 0, 105, 0, 91, 0, 75, 0, 59, 0,
		43, 0, 27, 0, 13, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		123, 0, 114, 2, 107, 4, 100, 6, 97, 8,
		93, 10, 90, 12, 86, 14, 85, 16, 83, 17,
		82, 19, 80, 21, 79, 22, 78, 23, 77, 24,
		75, 26, 75, 26, 75, 28, 75, 29, 73, 29,
		0, 119, 0, 110, 2, 102, 4, 97, 5, 92,
		7, 89, 9, 86, 11, 83, 13, 81, 15, 79,
		16, 78, 17, 76, 19, 75, 20, 74, 21, 72,
		23, 72, 24, 71, 25, 70, 27, 70, 27, 69,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 124, 0, 118, 0, 109,
		0, 97, 0, 83, 0, 68, 0, 53, 0, 39,
		0, 25, 0, 12, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 124, 0,
		118, 0, 109, 0, 97, 0, 83, 0, 68, 0,
		53, 0, 39, 0, 25, 0, 12, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		124, 0, 115, 2, 108, 3, 102, 5, 99, 7,
		94, 9, 92, 11, 88, 12, 87, 14, 85, 16,
		83, 17, 82, 19, 80, 20, 79, 21, 78, 23,
		76, 24, 76, 25, 76, 26, 75, 26, 75, 28,
		0, 119, 0, 111, 2, 104, 3, 99, 5, 94,
		6, 90, 8, 87, 10, 85, 11, 83, 13, 80,
		15, 79, 16, 78, 17, 76, 19, 75, 20, 74,
		21, 73, 22, 72, 24, 72, 24, 70, 25, 70,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 124, 0, 119, 0, 111,
		0, 101, 0, 89, 0, 76, 0, 62, 0, 48,
		0, 35, 0, 22, 0, 11, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 124, 0,
		119, 0, 111, 0, 101, 0, 89, 0, 76, 0,
		62, 0, 48, 0, 35, 0, 22, 0, 11, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		124, 0, 116, 2, 110, 3, 104, 5, 100, 6,
		96, 8, 93, 10, 90, 11, 88, 13, 86, 14,
		85, 16, 83, 17, 82, 19, 80, 20, 80, 21,
		77, 22, 78, 24, 76, 24, 76, 25, 76, 26,
		0, 120, 0, 112, 1, 105, 3, 100, 4, 96,
		6, 92, 7, 89, 9, 86, 11, 84, 12, 82,
		13, 80, 15, 79, 16, 78, 17, 76, 19, 75,
		19, 74, 21, 74, 21, 72, 23, 72, 24, 71,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 125, 0, 120, 0, 113,
		0, 104, 0, 94, 0, 82, 0, 69, 0, 57,
		0, 44, 0, 32, 0, 21, 0, 10, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 125, 0,
		120, 0, 113, 0, 104, 0, 94, 0, 82, 0,
		69, 0, 57, 0, 44, 0, 32, 0, 21, 0,
		10, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		125, 0, 117, 1, 111, 2, 105, 4, 101, 6,
		98, 7, 95, 9, 91, 11, 90, 12, 88, 13,
		86, 14, 85, 16, 83, 17, 82, 19, 80, 19,
		79, 21, 78, 21, 78, 23, 77, 24, 76, 24,
		0, 120, 0, 113, 1, 106, 3, 102, 4, 97,
		5, 93, 6, 90, 8, 88, 9, 85, 11, 83,
		13, 82, 14, 80, 15, 78, 16, 78, 17, 76,
		19, 76, 19, 74, 21, 74, 21, 73, 22, 72,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 125, 0, 121, 0, 115,
		0, 107, 0, 98, 0, 87, 0, 76, 0, 64,
		0, 52, 0, 41, 0, 30, 0, 19, 0, 9,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 125, 0,
		121, 0, 115, 0, 107, 0, 98, 0, 87, 0,
		76, 0, 64, 0, 52, 0, 41, 0, 30, 0,
		19, 0, 9, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		125, 0, 118, 1, 112, 2, 106, 3, 103, 5,
		99, 6, 96, 8, 93, 9, 91, 11, 89, 12,
		87, 13, 85, 14, 85, 16, 83, 17, 82, 19,
		79, 19, 80, 21, 79, 21, 78, 22, 78, 24,
		0, 121, 0, 113, 1, 108, 2, 102, 4, 98,
		5, 95, 6, 92, 7, 89, 9, 87, 10, 85,
		11, 83, 13, 81, 14, 80, 15, 78, 17, 78,
		17, 76, 19, 76, 19, 75, 20, 74, 21, 74,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 125, 0, 122, 0, 117,
		0, 110, 0, 101, 0, 91, 0, 81, 0, 70,
		0, 59, 0, 48, 0, 38, 0, 27, 0, 18,
		0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 125, 0,
		122, 0, 117, 0, 110, 0, 101, 0, 91, 0,
		81, 0, 70, 0, 59, 0, 48, 0, 38, 0,
		27, 0, 18, 0, 8, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		125, 0, 118, 1, 113, 2, 108, 3, 104, 5,
		100, 6, 97, 7, 94, 9, 92, 10, 90, 11,
		89, 12, 87, 14, 85, 14, 85, 17, 82, 17,
		81, 18, 81, 19, 80, 20, 79, 21, 78, 21,
		0, 121, 0, 114, 1, 109, 2, 104, 3, 99,
		4, 96, 6, 93, 7, 91, 8, 88, 9, 86,
		11, 84, 12, 83, 13, 81, 14, 80, 15, 78,
		17, 78, 17, 76, 18, 76, 19, 75, 20, 74,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 31, 0, 63, 0, 102, 0, 114, 0, 120,
		0, 122, 0, 124, 0, 124, 0, 125, 0, 125,
		0, 126, 0, 126, 0, 126, 0, 126, 0, 126,
		0, 126, 0, 125, 0, 88, 0, 112, 0, 119,
		0, 121, 0, 123, 0, 124, 0, 124, 0, 125,
		0, 125, 0, 126, 0, 126, 0, 126, 0, 126,
		0, 126, 0, 126, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 31, 31, 0, 63,
		0, 102, 0, 114, 0, 120, 0, 122, 0, 124,
		0, 124, 0, 125, 0, 125, 0, 126, 0, 126,
		0, 126, 0, 126, 0, 126, 0, 126, 31, 31,
		0, 63, 0, 102, 0, 114, 0, 120, 0, 122,
		0, 124, 0, 124, 0, 125, 0, 125, 0, 126,
		0, 126, 0, 126, 0, 126, 0, 126, 0, 126,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		125, 0, 119, 1, 114, 2, 109, 3, 105, 4,
		102, 6, 99, 7, 95, 8, 94, 9, 92, 11,
		90, 12, 88, 12, 87, 14, 85, 15, 84, 17,
		81, 17, 82, 18, 81, 19, 80, 20, 80, 21,
		0, 121, 0, 115, 1, 109, 2, 105, 3, 101,
		4, 97, 5, 94, 6, 92, 7, 89, 9, 87,
		10, 85, 11, 84, 13, 83, 13, 81, 15, 80,
		15, 78, 17, 78, 17, 76, 18, 76, 19, 75,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 10, 0, 63, 0, 92, 0, 106,
		0, 113, 0, 117, 0, 120, 0, 121, 0, 122,
		0, 123, 0, 124, 0, 124, 0, 125, 0, 125,
		0, 125, 0, 88, 0, 68, 0, 85, 0, 103,
		0, 110, 0, 114, 0, 117, 0, 120, 0, 121,
		0, 122, 0, 123, 0, 124, 0, 124, 0, 125,
		0, 125, 0, 125, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 63, 0, 10, 10,
		0, 63, 0, 92, 0, 106, 0, 113, 0, 117,
		0, 120, 0, 121, 0, 122, 0, 123, 0, 124,
		0, 124, 0, 125, 0, 125, 0, 125, 63, 0,
		10, 10, 0, 63, 0, 92, 0, 106, 0, 113,
		0, 117, 0, 120, 0, 121, 0, 122, 0, 123,
		0, 124, 0, 124, 0, 125, 0, 125, 0, 125,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		125, 0, 120, 1, 114, 2, 110, 2, 106, 3,
		103, 5, 100, 6, 96, 7, 95, 9, 92, 10,
		91, 11, 90, 12, 87, 12, 86, 14, 85, 15,
		83, 17, 82, 17, 82, 18, 81, 19, 80, 19,
		0, 121, 0, 115, 1, 110, 2, 105, 3, 102,
		4, 98, 5, 95, 6, 93, 7, 91, 8, 88,
		9, 86, 11, 85, 11, 83, 13, 82, 13, 80,
		15, 80, 15, 78, 17, 78, 17, 77, 18, 76,
		65, 61, 66, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 3, 0, 45, 0, 72,
		0, 89, 0, 99, 0, 106, 0, 110, 0, 114,
		0, 116, 0, 118, 0, 119, 0, 120, 0, 121,
		0, 122, 0, 112, 0, 85, 0, 32, 0, 62,
		0, 80, 0, 90, 0, 99, 0, 106, 0, 110,
		0, 114, 0, 116, 0, 118, 0, 119, 0, 120,
		0, 121, 0, 122, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 102, 0, 63, 0,
		3, 3, 0, 45, 0, 72, 0, 89, 0, 99,
		0, 106, 0, 110, 0, 114, 0, 116, 0, 118,
		0, 119, 0, 120, 0, 121, 0, 122, 102, 0,
		63, 0, 3, 3, 0, 45, 0, 72, 0, 89,
		0, 99, 0, 106, 0, 110, 0, 114, 0, 116,
		0, 118, 0, 119, 0, 120, 0, 121, 0, 122,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		126, 0, 120, 1, 115, 2, 110, 2, 107, 3,
		104, 5, 101, 6, 97, 7, 95, 7, 94, 9,
		92, 11, 90, 11, 89, 12, 87, 13, 86, 14,
		84, 15, 84, 17, 82, 17, 82, 18, 81, 19,
		0, 121, 0, 116, 0, 111, 2, 106, 3, 102,
		4, 99, 5, 96, 5, 94, 6, 91, 7, 89,
		9, 88, 9, 85, 11, 85, 12, 83, 13, 82,
		13, 80, 15, 80, 15, 78, 17, 78, 17, 77,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 1, 0, 34,
		0, 58, 0, 74, 0, 86, 0, 94, 0, 100,
		0, 105, 0, 109, 0, 111, 0, 113, 0, 115,
		0, 117, 0, 119, 0, 103, 0, 62, 0, 13,
		0, 40, 0, 58, 0, 74, 0, 86, 0, 94,
		0, 100, 0, 105, 0, 109, 0, 111, 0, 113,
		0, 115, 0, 117, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 114, 0, 92, 0,
		45, 0, 1, 1, 0, 34, 0, 58, 0, 74,
		0, 86, 0, 94, 0, 100, 0, 105, 0, 109,
		0, 111, 0, 113, 0, 115, 0, 117, 114, 0,
		92, 0, 45, 0, 1, 1, 0, 34, 0, 58,
		0, 74, 0, 86, 0, 94, 0, 100, 0, 105,
		0, 109, 0, 111, 0, 113, 0, 115, 0, 117,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		126, 0, 120, 0, 116, 1, 111, 2, 108, 3,
		104, 4, 101, 5, 98, 6, 97, 7, 95, 9,
		92, 9, 91, 11, 90, 11, 88, 12, 87, 13,
		85, 14, 85, 15, 84, 17, 82, 17, 82, 18,
		0, 121, 0, 116, 0, 112, 1, 107, 2, 103,
		3, 100, 4, 97, 5, 95, 6, 93, 7, 91,
		8, 88, 9, 87, 10, 85, 11, 84, 12, 83,
		13, 81, 14, 80, 15, 80, 15, 78, 17, 78,
		65, 61, 65, 61, 66, 61, 65, 61, 66, 61,
		66, 61, 66, 61, 65, 61, 66, 61, 66, 61,
		66, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		65, 61, 66, 61, 66, 61, 66, 61, 66, 61,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 27, 0, 48, 0, 63, 0, 75, 0, 84,
		0, 91, 0, 97, 0, 101, 0, 104, 0, 107,
		0, 110, 0, 121, 0, 110, 0, 80, 0, 40,
		0, 1, 0, 27, 0, 48, 0, 63, 0, 75,
		0, 84, 0, 91, 0, 97, 0, 101, 0, 104,
		0, 107, 0, 110, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 120, 0, 106, 0,
		72, 0, 34, 0, 0, 0, 0, 27, 0, 48,
		0, 63, 0, 75, 0, 84, 0, 91, 0, 97,
		0, 101, 0, 104, 0, 107, 0, 110, 120, 0,
		106, 0, 72, 0, 34, 0, 0, 0, 0, 27,
		0, 48, 0, 63, 0, 75, 0, 84, 0, 91,
		0, 97, 0, 101, 0, 104, 0, 107, 0, 110,
		33, 93, 12, 104, 6, 109, 4, 113, 2, 114,
		2, 116, 1, 117, 1, 118, 1, 119, 0, 119,
		0, 119, 0, 120, 0, 120, 0, 121, 0, 121,
		0, 121, 0, 121, 0, 121, 0, 121, 0, 122,
		66, 64, 23, 85, 12, 96, 8, 102, 5, 106,
		4, 109, 3, 111, 2, 113, 2, 115, 1, 115,
		1, 116, 1, 117, 1, 118, 1, 119, 0, 119,
		0, 119, 0, 120, 0, 120, 0, 120, 0, 120,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		33, 93, 12, 104, 6, 109, 4, 113, 2, 114,
		2, 116, 1, 117, 1, 118, 1, 119, 0, 119,
		0, 119, 0, 120, 0, 120, 0, 121, 0, 121,
		0, 121, 0, 121, 0, 121, 0, 121, 0, 122,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 22, 0, 40, 0, 55, 0, 66,
		0, 75, 0, 83, 0, 89, 0, 94, 0, 98,
		0, 101, 0, 123, 0, 114, 0, 90, 0, 58,
		0, 27, 0, 1, 0, 22, 0, 40, 0, 55,
		0, 66, 0, 75, 0, 83, 0, 89, 0, 94,
		0, 98, 0, 101, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 122, 0, 113, 0,
		89, 0, 58, 0, 27, 0, 0, 0, 0, 22,
		0, 40, 0, 55, 0, 66, 0, 75, 0, 83,
		0, 89, 0, 94, 0, 98, 0, 101, 122, 0,
		113, 0, 89, 0, 58, 0, 27, 0, 0, 0,
		0, 22, 0, 40, 0, 55, 0, 66, 0, 75,
		0, 83, 0, 89, 0, 94, 0, 98, 0, 101,
		43, 73, 22, 83, 13, 91, 9, 96, 7, 100,
		5, 103, 4, 105, 3, 107, 3, 108, 2, 110,
		2, 111, 2, 112, 1, 113, 1, 113, 1, 114,
		1, 115, 1, 115, 1, 116, 0, 116, 0, 117,
		87, 22, 45, 44, 27, 59, 18, 69, 13, 77,
		11, 83, 8, 87, 7, 91, 5, 94, 5, 97,
		4, 99, 4, 101, 3, 103, 2, 104, 2, 105,
		2, 107, 2, 108, 1, 109, 1, 110, 1, 111,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		43, 73, 22, 83, 13, 91, 9, 96, 7, 100,
		5, 103, 4, 105, 3, 107, 3, 108, 2, 110,
		2, 111, 2, 112, 1, 113, 1, 113, 1, 114,
		1, 115, 1, 115, 1, 116, 0, 116, 0, 117,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 19, 0, 35, 0, 48,
		0, 59, 0, 68, 0, 76, 0, 82, 0, 87,
		0, 91, 0, 124, 0, 117, 0, 99, 0, 74,
		0, 48, 0, 22, 0, 0, 0, 19, 0, 35,
		0, 48, 0, 59, 0, 68, 0, 76, 0, 82,
		0, 87, 0, 91, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 124, 0, 117, 0,
		99, 0, 74, 0, 48, 0, 22, 0, 0, 0,
		0, 19, 0, 35, 0, 48, 0, 59, 0, 68,
		0, 76, 0, 82, 0, 87, 0, 91, 124, 0,
		117, 0, 99, 0, 74, 0, 48, 0, 22, 0,
		0, 0, 0, 19, 0, 35, 0, 48, 0, 59,
		0, 68, 0, 76, 0, 82, 0, 87, 0, 91,
		49, 67, 29, 75, 20, 81, 14, 86, 11, 91,
		8, 94, 7, 97, 6, 99, 5, 101, 4, 103,
		4, 104, 3, 105, 3, 106, 3, 108, 2, 109,
		2, 109, 2, 110, 2, 111, 2, 112, 1, 112,
		97, 12, 59, 27, 40, 40, 29, 50, 22, 58,
		17, 65, 14, 71, 12, 75, 10, 79, 9, 83,
		7, 85, 6, 88, 5, 90, 5, 92, 5, 94,
		4, 96, 4, 97, 4, 99, 3, 100, 2, 102,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		49, 67, 29, 75, 20, 81, 14, 86, 11, 91,
		8, 94, 7, 97, 6, 99, 5, 101, 4, 103,
		4, 104, 3, 105, 3, 106, 3, 108, 2, 109,
		2, 109, 2, 110, 2, 111, 2, 112, 1, 112,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 16, 0, 31,
		0, 43, 0, 53, 0, 62, 0, 69, 0, 76,
		0, 81, 0, 124, 0, 120, 0, 106, 0, 86,
		0, 63, 0, 40, 0, 19, 0, 0, 0, 16,
		0, 31, 0, 43, 0, 53, 0, 62, 0, 69,
		0, 76, 0, 81, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 124, 0, 120, 0,
		106, 0, 86, 0, 63, 0, 40, 0, 19, 0,
		0, 0, 0, 16, 0, 31, 0, 43, 0, 53,
		0, 62, 0, 69, 0, 76, 0, 81, 124, 0,
		120, 0, 106, 0, 86, 0, 63, 0, 40, 0,
		19, 0, 0, 0, 0, 16, 0, 31, 0, 43,
		0, 53, 0, 62, 0, 69, 0, 76, 0, 81,
		51, 65, 34, 70, 25, 76, 19, 80, 15, 84,
		12, 87, 10, 90, 8, 93, 7, 95, 6, 97,
		5, 99, 5, 100, 4, 102, 4, 102, 4, 104,
		3, 105, 3, 105, 3, 106, 3, 107, 2, 108,
		103, 7, 69, 18, 50, 29, 38, 38, 30, 46,
		24, 52, 20, 58, 17, 63, 15, 68, 12, 71,
		11, 75, 10, 77, 9, 81, 7, 82, 7, 85,
		6, 87, 5, 88, 5, 90, 5, 92, 4, 93,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		51, 65, 34, 70, 25, 76, 19, 80, 15, 84,
		12, 87, 10, 90, 8, 93, 7, 95, 6, 97,
		5, 99, 5, 100, 4, 102, 4, 102, 4, 104,
		3, 105, 3, 105, 3, 106, 3, 107, 2, 108,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 14,
		0, 27, 0, 39, 0, 48, 0, 57, 0, 64,
		0, 70, 0, 125, 0, 121, 0, 110, 0, 94,
		0, 75, 0, 55, 0, 35, 0, 16, 0, 0,
		0, 14, 0, 27, 0, 39, 0, 48, 0, 57,
		0, 64, 0, 70, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 125, 0, 121, 0,
		110, 0, 94, 0, 75, 0, 55, 0, 35, 0,
		16, 0, 0, 0, 0, 14, 0, 27, 0, 39,
		0, 48, 0, 57, 0, 64, 0, 70, 125, 0,
		121, 0, 110, 0, 94, 0, 75, 0, 55, 0,
		35, 0, 16, 0, 0, 0, 0, 14, 0, 27,
		0, 39, 0, 48, 0, 57, 0, 64, 0, 70,
		53, 64, 39, 68, 29, 72, 23, 76, 18, 80,
		15, 83, 13, 86, 11, 88, 9, 90, 8, 92,
		7, 94, 6, 96, 6, 97, 5, 98, 5, 99,
		4, 101, 4, 102, 4, 102, 4, 103, 3, 104,
		107, 5, 77, 13, 59, 22, 46, 30, 37, 37,
		31, 43, 26, 49, 22, 54, 19, 58, 16, 62,
		15, 66, 13, 69, 12, 71, 10, 74, 10, 76,
		9, 79, 7, 81, 7, 82, 7, 84, 6, 86,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		53, 64, 39, 68, 29, 72, 23, 76, 18, 80,
		15, 83, 13, 86, 11, 88, 9, 90, 8, 92,
		7, 94, 6, 96, 6, 97, 5, 98, 5, 99,
		4, 101, 4, 102, 4, 102, 4, 103, 3, 104,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 13, 0, 25, 0, 35, 0, 44, 0, 52,
		0, 59, 0, 125, 0, 122, 0, 114, 0, 100,
		0, 84, 0, 66, 0, 48, 0, 31, 0, 14,
		0, 0, 0, 13, 0, 25, 0, 35, 0, 44,
		0, 52, 0, 59, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 125, 0, 122, 0,
		114, 0, 100, 0, 84, 0, 66, 0, 48, 0,
		31, 0, 14, 0, 0, 0, 0, 13, 0, 25,
		0, 35, 0, 44, 0, 52, 0, 59, 125, 0,
		122, 0, 114, 0, 100, 0, 84, 0, 66, 0,
		48, 0, 31, 0, 14, 0, 0, 0, 0, 13,
		0, 25, 0, 35, 0, 44, 0, 52, 0, 59,
		55, 63, 41, 67, 33, 70, 26, 73, 22, 77,
		18, 80, 16, 82, 13, 84, 12, 87, 10, 89,
		9, 90, 8, 92, 7, 93, 6, 95, 6, 96,
		6, 97, 5, 98, 5, 99, 4, 100, 4, 101,
		110, 4, 83, 10, 65, 17, 52, 24, 43, 31,
		36, 36, 31, 41, 27, 46, 23, 51, 21, 55,
		18, 58, 16, 61, 15, 64, 13, 67, 12, 69,
		12, 72, 10, 74, 10, 76, 9, 78, 7, 79,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		55, 63, 41, 67, 33, 70, 26, 73, 22, 77,
		18, 80, 16, 82, 13, 84, 12, 87, 10, 89,
		9, 90, 8, 92, 7, 93, 6, 95, 6, 96,
		6, 97, 5, 98, 5, 99, 4, 100, 4, 101,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 12, 0, 22, 0, 32, 0, 41,
		0, 48, 0, 126, 0, 123, 0, 116, 0, 105,
		0, 91, 0, 75, 0, 59, 0, 43, 0, 27,
		0, 13, 0, 0, 0, 12, 0, 22, 0, 32,
		0, 41, 0, 48, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 123, 0,
		116, 0, 105, 0, 91, 0, 75, 0, 59, 0,
		43, 0, 27, 0, 13, 0, 0, 0, 0, 12,
		0, 22, 0, 32, 0, 41, 0, 48, 126, 0,
		123, 0, 116, 0, 105, 0, 91, 0, 75, 0,
		59, 0, 43, 0, 27, 0, 13, 0, 0, 0,
		0, 12, 0, 22, 0, 32, 0, 41, 0, 48,
		56, 63, 44, 65, 35, 68, 29, 71, 25, 74,
		20, 77, 18, 79, 16, 82, 14, 84, 12, 86,
		11, 87, 10, 89, 9, 90, 8, 92, 7, 93,
		7, 94, 6, 95, 6, 97, 5, 97, 5, 98,
		112, 3, 88, 8, 71, 14, 58, 20, 49, 26,
		41, 31, 36, 36, 32, 40, 28, 45, 25, 49,
		22, 52, 20, 55, 18, 58, 16, 61, 15, 63,
		14, 66, 12, 68, 12, 70, 11, 71, 10, 74,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		56, 63, 44, 65, 35, 68, 29, 71, 25, 74,
		20, 77, 18, 79, 16, 82, 14, 84, 12, 86,
		11, 87, 10, 89, 9, 90, 8, 92, 7, 93,
		7, 94, 6, 95, 6, 97, 5, 97, 5, 98,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 11, 0, 21, 0, 30,
		0, 38, 0, 126, 0, 124, 0, 118, 0, 109,
		0, 97, 0, 83, 0, 68, 0, 53, 0, 39,
		0, 25, 0, 12, 0, 0, 0, 11, 0, 21,
		0, 30, 0, 38, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 124, 0,
		118, 0, 109, 0, 97, 0, 83, 0, 68, 0,
		53, 0, 39, 0, 25, 0, 12, 0, 0, 0,
		0, 11, 0, 21, 0, 30, 0, 38, 126, 0,
		124, 0, 118, 0, 109, 0, 97, 0, 83, 0,
		68, 0, 53, 0, 39, 0, 25, 0, 12, 0,
		0, 0, 0, 11, 0, 21, 0, 30, 0, 38,
		57, 63, 46, 65, 38, 67, 32, 70, 27, 72,
		23, 75, 20, 77, 18, 79, 16, 81, 14, 83,
		13, 85, 11, 86, 11, 88, 9, 89, 9, 91,
		8, 91, 7, 93, 7, 94, 6, 95, 6, 96,
		113, 2, 91, 7, 76, 12, 63, 17, 54, 22,
		46, 27, 41, 32, 36, 35, 32, 40, 29, 43,
		25, 47, 23, 50, 22, 53, 18, 55, 18, 58,
		16, 60, 15, 63, 14, 65, 12, 66, 12, 69,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		57, 63, 46, 65, 38, 67, 32, 70, 27, 72,
		23, 75, 20, 77, 18, 79, 16, 81, 14, 83,
		13, 85, 11, 86, 11, 88, 9, 89, 9, 91,
		8, 91, 7, 93, 7, 94, 6, 95, 6, 96,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 10, 0, 19,
		0, 27, 0, 126, 0, 124, 0, 119, 0, 111,
		0, 101, 0, 89, 0, 76, 0, 62, 0, 48,
		0, 35, 0, 22, 0, 11, 0, 0, 0, 10,
		0, 19, 0, 27, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 124, 0,
		119, 0, 111, 0, 101, 0, 89, 0, 76, 0,
		62, 0, 48, 0, 35, 0, 22, 0, 11, 0,
		0, 0, 0, 10, 0, 19, 0, 27, 126, 0,
		124, 0, 119, 0, 111, 0, 101, 0, 89, 0,
		76, 0, 62, 0, 48, 0, 35, 0, 22, 0,
		11, 0, 0, 0, 0, 10, 0, 19, 0, 27,
		57, 62, 47, 64, 40, 66, 34, 69, 29, 71,
		25, 73, 22, 75, 20, 77, 17, 79, 16, 81,
		15, 83, 13, 84, 12, 85, 11, 87, 10, 88,
		9, 89, 9, 91, 8, 91, 7, 92, 7, 94,
		115, 2, 94, 5, 79, 10, 68, 15, 58, 19,
		51, 24, 45, 28, 39, 32, 35, 35, 32, 39,
		29, 42, 26, 45, 24, 48, 22, 51, 20, 53,
		18, 56, 18, 58, 16, 60, 15, 62, 15, 65,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		57, 62, 47, 64, 40, 66, 34, 69, 29, 71,
		25, 73, 22, 75, 20, 77, 17, 79, 16, 81,
		15, 83, 13, 84, 12, 85, 11, 87, 10, 88,
		9, 89, 9, 91, 8, 91, 7, 92, 7, 94,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 9,
		0, 18, 0, 126, 0, 125, 0, 120, 0, 113,
		0, 104, 0, 94, 0, 82, 0, 69, 0, 57,
		0, 44, 0, 32, 0, 21, 0, 10, 0, 0,
		0, 9, 0, 18, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 125, 0,
		120, 0, 113, 0, 104, 0, 94, 0, 82, 0,
		69, 0, 57, 0, 44, 0, 32, 0, 21, 0,
		10, 0, 0, 0, 0, 9, 0, 18, 126, 0,
		125, 0, 120, 0, 113, 0, 104, 0, 94, 0,
		82, 0, 69, 0, 57, 0, 44, 0, 32, 0,
		21, 0, 10, 0, 0, 0, 0, 9, 0, 18,
		58, 62, 49, 64, 41, 66, 35, 68, 31, 70,
		27, 72, 24, 74, 21, 76, 19, 77, 17, 79,
		16, 80, 15, 82, 13, 83, 13, 85, 11, 86,
		11, 87, 10, 88, 9, 89, 9, 91, 8, 91,
		116, 1, 98, 5, 83, 8, 71, 12, 62, 17,
		55, 21, 49, 25, 43, 29, 39, 32, 35, 35,
		32, 38, 29, 42, 27, 44, 25, 47, 22, 49,
		22, 52, 19, 53, 18, 56, 18, 59, 15, 59,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		58, 62, 49, 64, 41, 66, 35, 68, 31, 70,
		27, 72, 24, 74, 21, 76, 19, 77, 17, 79,
		16, 80, 15, 82, 13, 83, 13, 85, 11, 86,
		11, 87, 10, 88, 9, 89, 9, 91, 8, 91,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 8, 0, 126, 0, 125, 0, 121, 0, 115,
		0, 107, 0, 98, 0, 87, 0, 76, 0, 64,
		0, 52, 0, 41, 0, 30, 0, 19, 0, 9,
		0, 0, 0, 8, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 126, 0, 125, 0,
		121, 0, 115, 0, 107, 0, 98, 0, 87, 0,
		76, 0, 64, 0, 52, 0, 41, 0, 30, 0,
		19, 0, 9, 0, 0, 0, 0, 8, 126, 0,
		125, 0, 121, 0, 115, 0, 107, 0, 98, 0,
		87, 0, 76, 0, 64, 0, 52, 0, 41, 0,
		30, 0, 19, 0, 9, 0, 0, 0, 0, 8,
		58, 62, 50, 63, 43, 65, 37, 67, 33, 69,
		29, 70, 26, 72, 23, 74, 21, 76, 19, 78,
		17, 79, 16, 80, 15, 82, 14, 83, 13, 84,
		12, 85, 11, 86, 11, 88, 9, 88, 9, 89,
		117, 1, 100, 4, 86, 7, 75, 11, 66, 15,
		58, 18, 52, 22, 47, 25, 42, 29, 38, 32,
		35, 35, 33, 38, 29, 41, 27, 43, 25, 46,
		24, 48, 22, 50, 21, 53, 18, 53, 18, 56,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		0, 123, 0, 123, 0, 123, 0, 123, 0, 123,
		58, 62, 50, 63, 43, 65, 37, 67, 33, 69,
		29, 70, 26, 72, 23, 74, 21, 76, 19, 78,
		17, 79, 16, 80, 15, 82, 14, 83, 13, 84,
		12, 85, 11, 86, 11, 88, 9, 88, 9, 89,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 126, 0, 125, 0, 122, 0, 117,
		0, 110, 0, 101, 0, 91, 0, 81, 0, 70,
		0, 59, 0, 48, 0, 38, 0, 27, 0, 18,
		0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, "Not showing all elements because this array is too big (179200 elements)"
	};
}