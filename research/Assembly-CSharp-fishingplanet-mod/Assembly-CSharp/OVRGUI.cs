using System;
using UnityEngine;

public class OVRGUI
{
	public void GetFontReplace(ref Font fontReplace)
	{
		fontReplace = this.FontReplace;
	}

	public void SetFontReplace(Font fontReplace)
	{
		this.FontReplace = fontReplace;
	}

	public void GetPixelResolution(ref float pixelWidth, ref float pixelHeight)
	{
		pixelWidth = this.PixelWidth;
		pixelHeight = this.PixelHeight;
	}

	public void SetPixelResolution(float pixelWidth, float pixelHeight)
	{
		this.PixelWidth = pixelWidth;
		this.PixelHeight = pixelHeight;
	}

	public void GetDisplayResolution(ref float Width, ref float Height)
	{
		Width = this.DisplayWidth;
		Height = this.DisplayHeight;
	}

	public void SetDisplayResolution(float Width, float Height)
	{
		this.DisplayWidth = Width;
		this.DisplayHeight = Height;
	}

	public void StereoBox(int X, int Y, int wX, int hY, ref string text, Color color)
	{
		Font font = GUI.skin.font;
		GUI.color = color;
		if (GUI.skin.font != this.FontReplace)
		{
			GUI.skin.font = this.FontReplace;
		}
		float num = this.PixelWidth / this.DisplayWidth;
		this.CalcPositionAndSize((float)X * num, (float)Y * num, (float)wX * num, (float)hY * num, ref this.DrawRect);
		GUI.Box(this.DrawRect, text);
		GUI.skin.font = font;
	}

	public void StereoBox(float X, float Y, float wX, float hY, ref string text, Color color)
	{
		this.StereoBox((int)(X * this.PixelWidth), (int)(Y * this.PixelHeight), (int)(wX * this.PixelWidth), (int)(hY * this.PixelHeight), ref text, color);
	}

	public void StereoDrawTexture(int X, int Y, int wX, int hY, ref Texture image, Color color)
	{
		GUI.color = color;
		if (GUI.skin.font != this.FontReplace)
		{
			GUI.skin.font = this.FontReplace;
		}
		float num = this.PixelWidth / this.DisplayWidth;
		this.CalcPositionAndSize((float)X * num, (float)Y * num, (float)wX * num, (float)hY * num, ref this.DrawRect);
		GUI.DrawTexture(this.DrawRect, image);
	}

	public void StereoDrawTexture(float X, float Y, float wX, float hY, ref Texture image, Color color)
	{
		this.StereoDrawTexture((int)(X * this.PixelWidth), (int)(Y * this.PixelHeight), (int)(wX * this.PixelWidth), (int)(hY * this.PixelHeight), ref image, color);
	}

	private void CalcPositionAndSize(float X, float Y, float wX, float hY, ref Rect calcPosSize)
	{
		float num = (float)Screen.width / this.PixelWidth;
		float num2 = (float)Screen.height / this.PixelHeight;
		calcPosSize.x = X * num;
		calcPosSize.width = wX * num;
		calcPosSize.y = Y * num2;
		calcPosSize.height = hY * num2;
	}

	private Font FontReplace;

	private float PixelWidth = 1280f;

	private float PixelHeight = 800f;

	private float DisplayWidth = 1280f;

	private float DisplayHeight = 800f;

	private Rect DrawRect;
}
