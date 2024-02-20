using System;
using UnityEngine;

internal class BokehRenderer
{
	public void RebuildMeshIfNeeded(int width, int height, float spriteRelativeScaleX, float spriteRelativeScaleY, ref Mesh[] meshes)
	{
		if (this.m_CurrentWidth == width && this.m_CurrentHeight == height && this.m_CurrentRelativeScaleX == spriteRelativeScaleX && this.m_CurrentRelativeScaleY == spriteRelativeScaleY && meshes != null)
		{
			return;
		}
		if (meshes != null)
		{
			foreach (Mesh mesh in meshes)
			{
				Object.DestroyImmediate(mesh, true);
			}
		}
		meshes = null;
		this.BuildMeshes(width, height, spriteRelativeScaleX, spriteRelativeScaleY, ref meshes);
	}

	public void BuildMeshes(int width, int height, float spriteRelativeScaleX, float spriteRelativeScaleY, ref Mesh[] meshes)
	{
		int num = 10833;
		int num2 = width * height;
		int num3 = Mathf.CeilToInt(1f * (float)num2 / (1f * (float)num));
		meshes = new Mesh[num3];
		int num4 = num2;
		this.m_CurrentWidth = width;
		this.m_CurrentHeight = height;
		this.m_CurrentRelativeScaleX = spriteRelativeScaleX;
		this.m_CurrentRelativeScaleY = spriteRelativeScaleY;
		int num5 = 0;
		for (int i = 0; i < num3; i++)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = 61;
			int num6 = num4;
			if (num4 > num)
			{
				num6 = num;
			}
			num4 -= num6;
			Vector3[] array = new Vector3[num6 * 4];
			int[] array2 = new int[num6 * 6];
			Vector2[] array3 = new Vector2[num6 * 4];
			Vector2[] array4 = new Vector2[num6 * 4];
			Vector3[] array5 = new Vector3[num6 * 4];
			Color[] array6 = new Color[num6 * 4];
			float num7 = this.m_CurrentRelativeScaleX * (float)width;
			float num8 = this.m_CurrentRelativeScaleY * (float)height;
			for (int j = 0; j < num6; j++)
			{
				int num9 = num5 % width;
				int num10 = (num5 - num9) / width;
				this.SetupSprite(j, num9, num10, array, array2, array3, array4, array5, array6, new Vector2((float)num9 / (float)width, 1f - (float)num10 / (float)height), num7 * 0.5f, num8 * 0.5f);
				num5++;
			}
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.colors = array6;
			mesh.uv = array3;
			mesh.uv2 = array4;
			mesh.normals = array5;
			mesh.RecalculateBounds();
			mesh.UploadMeshData(true);
			meshes[i] = mesh;
		}
	}

	public void Clear(ref Mesh[] meshes)
	{
		if (meshes != null)
		{
			foreach (Mesh mesh in meshes)
			{
				Object.DestroyImmediate(mesh, true);
			}
		}
		meshes = null;
	}

	public void SetTexture(Texture2D texture)
	{
		this.m_CurrentTexture = texture;
		this.m_FlareMaterial.SetTexture("_MainTex", this.m_CurrentTexture);
	}

	public void SetMaterial(Material flareMaterial)
	{
		this.m_FlareMaterial = flareMaterial;
	}

	public void RenderFlare(RenderTexture brightPixels, RenderTexture destination, float intensity, ref Mesh[] meshes)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = destination;
		GL.Clear(true, true, Color.black);
		Matrix4x4 matrix4x = Matrix4x4.Ortho(0f, (float)this.m_CurrentWidth, 0f, (float)this.m_CurrentHeight, -1f, 1f);
		this.m_FlareMaterial.SetMatrix("_FlareProj", matrix4x);
		this.m_FlareMaterial.SetTexture("_BrightTexture", brightPixels);
		this.m_FlareMaterial.SetFloat("_Intensity", intensity);
		if (this.m_FlareMaterial.SetPass(0))
		{
			for (int i = 0; i < meshes.Length; i++)
			{
				Graphics.DrawMeshNow(meshes[i], Matrix4x4.identity);
			}
		}
		else
		{
			Debug.LogError("Can't render flare mesh");
		}
		RenderTexture.active = active;
	}

	public void SetupSprite(int idx, int x, int y, Vector3[] vertices, int[] triangles, Vector2[] uv0, Vector2[] uv1, Vector3[] normals, Color[] colors, Vector2 targetPixelUV, float halfWidth, float halfHeight)
	{
		int num = idx * 4;
		int num2 = idx * 6;
		triangles[num2] = num;
		triangles[num2 + 1] = num + 2;
		triangles[num2 + 2] = num + 1;
		triangles[num2 + 3] = num + 2;
		triangles[num2 + 4] = num + 3;
		triangles[num2 + 5] = num + 1;
		vertices[num] = new Vector3(-halfWidth + (float)x, -halfHeight + (float)y, 0f);
		vertices[num + 1] = new Vector3(halfWidth + (float)x, -halfHeight + (float)y, 0f);
		vertices[num + 2] = new Vector3(-halfWidth + (float)x, halfHeight + (float)y, 0f);
		vertices[num + 3] = new Vector3(halfWidth + (float)x, halfHeight + (float)y, 0f);
		Vector2 vector = targetPixelUV;
		colors[num] = new Color(-halfWidth / (float)this.m_CurrentWidth + vector.x, -halfHeight * -1f / (float)this.m_CurrentHeight + vector.y, 0f, 0f);
		colors[num + 1] = new Color(halfWidth / (float)this.m_CurrentWidth + vector.x, -halfHeight * -1f / (float)this.m_CurrentHeight + vector.y, 0f, 0f);
		colors[num + 2] = new Color(-halfWidth / (float)this.m_CurrentWidth + vector.x, halfHeight * -1f / (float)this.m_CurrentHeight + vector.y, 0f, 0f);
		colors[num + 3] = new Color(halfWidth / (float)this.m_CurrentWidth + vector.x, halfHeight * -1f / (float)this.m_CurrentHeight + vector.y, 0f, 0f);
		normals[num] = -Vector3.forward;
		normals[num + 1] = -Vector3.forward;
		normals[num + 2] = -Vector3.forward;
		normals[num + 3] = -Vector3.forward;
		uv0[num] = new Vector2(0f, 0f);
		uv0[num + 1] = new Vector2(1f, 0f);
		uv0[num + 2] = new Vector2(0f, 1f);
		uv0[num + 3] = new Vector2(1f, 1f);
		uv1[num] = targetPixelUV;
		uv1[num + 1] = targetPixelUV;
		uv1[num + 2] = targetPixelUV;
		uv1[num + 3] = targetPixelUV;
	}

	private Texture2D m_CurrentTexture;

	private Material m_FlareMaterial;

	private int m_CurrentWidth;

	private int m_CurrentHeight;

	private float m_CurrentRelativeScaleX;

	private float m_CurrentRelativeScaleY;
}
