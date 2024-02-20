using System;
using UnityEngine;

public static class RTUtility
{
	public static void Blit(RenderTexture src, RenderTexture des, Material mat, int pass = 0, bool clear = true)
	{
		mat.SetTexture("_MainTex", src);
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(des);
		if (clear)
		{
			GL.Clear(true, true, Color.clear);
		}
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = active;
	}

	public static void Blit(RenderTexture src, RenderTexture des, Material mat, Rect rect, int pass = 0, bool clear = true)
	{
		mat.SetTexture("_MainTex", src);
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(des);
		if (clear)
		{
			GL.Clear(true, true, Color.clear);
		}
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(rect.x, rect.y);
		GL.Vertex3(rect.x, rect.y, 0.1f);
		GL.TexCoord2(rect.x + rect.width, rect.y);
		GL.Vertex3(rect.x + rect.width, rect.y, 0.1f);
		GL.TexCoord2(rect.x + rect.width, rect.y + rect.height);
		GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0.1f);
		GL.TexCoord2(rect.x, rect.y + rect.height);
		GL.Vertex3(rect.x, rect.y + rect.height, 0.1f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = active;
	}

	public static void MultiTargetBlit(RenderTexture[] des, Material mat, int pass = 0)
	{
		RenderBuffer[] array = new RenderBuffer[des.Length];
		for (int i = 0; i < des.Length; i++)
		{
			array[i] = des[i].colorBuffer;
		}
		Graphics.SetRenderTarget(array, des[0].depthBuffer);
		GL.Clear(true, true, Color.clear);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass = 0)
	{
		Graphics.SetRenderTarget(des_rb, des_db);
		GL.Clear(true, true, Color.clear);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static RenderTexture CreaTextureByTemplate(RenderTexture template)
	{
		return new RenderTexture(template.width, template.height, 0, template.format)
		{
			wrapMode = template.wrapMode,
			filterMode = template.filterMode
		};
	}

	public static RenderTexture[] CreateTexturesBufer(int widthResolution, int heightResolution, RenderTextureFormat format = 11, TextureWrapMode wrapMode = 1, FilterMode filterMode = 1)
	{
		RenderTexture[] array = new RenderTexture[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new RenderTexture(widthResolution, heightResolution, 0, format);
			array[i].wrapMode = wrapMode;
			array[i].filterMode = filterMode;
			RTUtility.ClearColor(array[i]);
		}
		return array;
	}

	public static void BlitBuffered(RenderTexture[] buffer, Material material)
	{
		Graphics.Blit(buffer[0], buffer[1], material);
		RTUtility.Swap(buffer);
	}

	public static void Swap(RenderTexture[] texs)
	{
		RenderTexture renderTexture = texs[0];
		texs[0] = texs[1];
		texs[1] = renderTexture;
	}

	public static void Swap(ref RenderTexture tex0, ref RenderTexture tex1)
	{
		RenderTexture renderTexture = tex0;
		tex0 = tex1;
		tex1 = renderTexture;
	}

	public static void ClearColor(RenderTexture tex)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = tex;
		GL.Clear(false, true, Color.clear);
		RenderTexture.active = active;
	}

	public static void ClearColor(RenderTexture[] texs)
	{
		RenderTexture active = RenderTexture.active;
		for (int i = 0; i < texs.Length; i++)
		{
			RenderTexture.active = texs[i];
			GL.Clear(false, true, Color.clear);
		}
		RenderTexture.active = active;
	}

	public static void SetToPoint(RenderTexture[] texs)
	{
		for (int i = 0; i < texs.Length; i++)
		{
			texs[i].filterMode = 0;
		}
	}

	public static void SetToBilinear(RenderTexture[] texs)
	{
		for (int i = 0; i < texs.Length; i++)
		{
			texs[i].filterMode = 1;
		}
	}

	public static void SetToTrilinear(RenderTexture[] texs)
	{
		for (int i = 0; i < texs.Length; i++)
		{
			texs[i].filterMode = 2;
		}
	}

	public static void SetToAA(RenderTexture[] texs)
	{
		for (int i = 0; i < texs.Length; i++)
		{
			texs[i].antiAliasing = 2;
		}
	}

	public static void SetToNoAA(RenderTexture[] texs)
	{
		for (int i = 0; i < texs.Length; i++)
		{
			texs[i].antiAliasing = 0;
		}
	}
}
