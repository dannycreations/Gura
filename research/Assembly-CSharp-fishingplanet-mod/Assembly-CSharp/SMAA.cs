using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SMAA : MonoBehaviour
{
	private Material material
	{
		get
		{
			if (this.mat == null)
			{
				this.mat = new Material(this.Shader);
				this.mat.hideFlags = 61;
			}
			return this.mat;
		}
	}

	private void OnEnable()
	{
		if (this.areaTexture == null)
		{
			this.areaTexture = new AreaTexture();
		}
		if (this.searchTexture == null)
		{
			this.searchTexture = new SearchTexture();
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (!this.Shader || !this.Shader.isSupported)
		{
			base.enabled = false;
		}
		this.black = new Texture2D(1, 1);
		this.black.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
		this.black.Apply();
	}

	private void OnDisable()
	{
		if (this.mat)
		{
			Object.DestroyImmediate(this.mat);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Vector4 vector;
		vector..ctor(1f / (float)Screen.width, 1f / (float)Screen.height, (float)Screen.width, (float)Screen.height);
		if (this.RenderState == 1)
		{
			Graphics.Blit(source, destination, this.material, 0);
		}
		else if (this.RenderState == 2)
		{
			this.material.SetTexture("areaTex", this.areaTexture.alphaTex);
			this.material.SetTexture("luminTex", this.areaTexture.luminTex);
			this.material.SetTexture("searchTex", this.searchTexture.alphaTex);
			this.material.SetVector("SMAA_RT_METRICS", vector);
			RenderTexture temporary = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			Graphics.Blit(source, temporary, this.material, 0);
			Graphics.Blit(temporary, destination, this.material, 1);
			temporary.Release();
		}
		else
		{
			this.material.SetTexture("areaTex", this.areaTexture.alphaTex);
			this.material.SetTexture("luminTex", this.areaTexture.luminTex);
			this.material.SetTexture("searchTex", this.searchTexture.alphaTex);
			this.material.SetTexture("_SrcTex", source);
			this.material.SetVector("SMAA_RT_METRICS", vector);
			RenderTexture temporary2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			RenderTexture temporary3 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			RenderTexture temporary4 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			Graphics.Blit(source, temporary4);
			for (int i = 0; i < this.Passes; i++)
			{
				Graphics.Blit(this.black, temporary2);
				Graphics.Blit(this.black, temporary3);
				Graphics.Blit(temporary4, temporary2, this.material, 0);
				Graphics.Blit(temporary2, temporary3, this.material, 1);
				Graphics.Blit(temporary3, temporary4, this.material, 2);
			}
			Graphics.Blit(temporary4, destination);
			temporary2.Release();
			temporary3.Release();
			temporary4.Release();
		}
	}

	[Range(1f, 3f)]
	public int RenderState = 3;

	public int Passes = 1;

	public Shader Shader;

	private Texture2D black;

	private Material mat;

	private AreaTexture areaTexture;

	private SearchTexture searchTexture;
}
