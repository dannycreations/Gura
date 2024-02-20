using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB_TextureCombinerRenderTexture
{
	public Texture2D DoRenderAtlas(GameObject gameObject, int width, int height, int padding, Rect[] rss, List<MB3_TextureCombiner.MB_TexSet> textureSetss, int indexOfTexSetToRenders, ShaderTextureProperty texPropertyname, TextureBlender resultMaterialTextureBlender, bool isNormalMap, bool fixOutOfBoundsUVs, bool considerNonTextureProperties, MB3_TextureCombiner texCombiner, MB2_LogLevel LOG_LEV)
	{
		this.LOG_LEVEL = LOG_LEV;
		this.textureSets = textureSetss;
		this.indexOfTexSetToRender = indexOfTexSetToRenders;
		this._texPropertyName = texPropertyname;
		this._padding = padding;
		this._isNormalMap = isNormalMap;
		this._fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		this._resultMaterialTextureBlender = resultMaterialTextureBlender;
		this.combiner = texCombiner;
		this.rs = rss;
		Shader shader;
		if (this._isNormalMap)
		{
			shader = Shader.Find("MeshBaker/NormalMapShader");
		}
		else
		{
			shader = Shader.Find("MeshBaker/AlbedoShader");
		}
		if (shader == null)
		{
			Debug.LogError("Could not find shader for RenderTexture. Try reimporting mesh baker");
			return null;
		}
		this.mat = new Material(shader);
		this._destinationTexture = new RenderTexture(width, height, 24, 0);
		this._destinationTexture.filterMode = 0;
		this.myCamera = gameObject.GetComponent<Camera>();
		this.myCamera.orthographic = true;
		this.myCamera.orthographicSize = (float)(height >> 1);
		this.myCamera.aspect = (float)(width / height);
		this.myCamera.targetTexture = this._destinationTexture;
		this.myCamera.clearFlags = 2;
		Transform component = this.myCamera.GetComponent<Transform>();
		component.localPosition = new Vector3((float)width / 2f, (float)height / 2f, 3f);
		component.localRotation = Quaternion.Euler(0f, 180f, 180f);
		this._doRenderAtlas = true;
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log(string.Format("Begin Camera.Render destTex w={0} h={1} camPos={2}", width, height, component.localPosition));
		}
		this.myCamera.Render();
		this._doRenderAtlas = false;
		MB_Utility.Destroy(this.mat);
		MB_Utility.Destroy(this._destinationTexture);
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Finished Camera.Render ");
		}
		Texture2D texture2D = this.targTex;
		this.targTex = null;
		return texture2D;
	}

	public void OnRenderObject()
	{
		if (this._doRenderAtlas)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < this.rs.Length; i++)
			{
				MB3_TextureCombiner.MeshBakerMaterialTexture meshBakerMaterialTexture = this.textureSets[i].ts[this.indexOfTexSetToRender];
				if (this.LOG_LEVEL >= MB2_LogLevel.trace && meshBakerMaterialTexture.t != null)
				{
					Debug.Log(string.Concat(new object[]
					{
						"Added ",
						meshBakerMaterialTexture.t,
						" to atlas w=",
						meshBakerMaterialTexture.t.width,
						" h=",
						meshBakerMaterialTexture.t.height,
						" offset=",
						meshBakerMaterialTexture.matTilingRect.min,
						" scale=",
						meshBakerMaterialTexture.matTilingRect.size,
						" rect=",
						this.rs[i],
						" padding=",
						this._padding
					}));
					this._printTexture(meshBakerMaterialTexture.t);
				}
				this.CopyScaledAndTiledToAtlas(this.textureSets[i], meshBakerMaterialTexture, this.textureSets[i].obUVoffset, this.textureSets[i].obUVscale, this.rs[i], this._texPropertyName, this._resultMaterialTextureBlender);
			}
			stopwatch.Stop();
			stopwatch.Start();
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Total time for Graphics.DrawTexture calls " + stopwatch.ElapsedMilliseconds.ToString("f5"));
			}
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Copying RenderTexture to Texture2D. destW",
					this._destinationTexture.width,
					" destH",
					this._destinationTexture.height
				}));
			}
			Texture2D texture2D = new Texture2D(this._destinationTexture.width, this._destinationTexture.height, 5, true);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = this._destinationTexture;
			int num = this._destinationTexture.width / 512;
			int num2 = this._destinationTexture.height / 512;
			if (num == 0 || num2 == 0)
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					Debug.Log("Copying all in one shot");
				}
				texture2D.ReadPixels(new Rect(0f, 0f, (float)this._destinationTexture.width, (float)this._destinationTexture.height), 0, 0, true);
			}
			else if (this.IsOpenGL())
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					Debug.Log("OpenGL copying blocks");
				}
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						texture2D.ReadPixels(new Rect((float)(j * 512), (float)(k * 512), 512f, 512f), j * 512, k * 512, true);
					}
				}
			}
			else
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					Debug.Log("Not OpenGL copying blocks");
				}
				for (int l = 0; l < num; l++)
				{
					for (int m = 0; m < num2; m++)
					{
						texture2D.ReadPixels(new Rect((float)(l * 512), (float)(this._destinationTexture.height - 512 - m * 512), 512f, 512f), l * 512, m * 512, true);
					}
				}
			}
			RenderTexture.active = active;
			texture2D.Apply();
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				Debug.Log("TempTexture ");
				this._printTexture(texture2D);
			}
			this.myCamera.targetTexture = null;
			RenderTexture.active = null;
			this.targTex = texture2D;
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Total time to copy RenderTexture to Texture2D " + stopwatch.ElapsedMilliseconds.ToString("f5"));
			}
		}
	}

	private Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (float)c.a * 2f - 1f;
		zero.y = (float)c.g * 2f - 1f;
		zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
		Color32 color = default(Color32);
		color.a = 1;
		color.r = (byte)((zero.x + 1f) * 0.5f);
		color.g = (byte)((zero.y + 1f) * 0.5f);
		color.b = (byte)((zero.z + 1f) * 0.5f);
		return color;
	}

	private bool IsOpenGL()
	{
		string graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
		return graphicsDeviceVersion.StartsWith("OpenGL");
	}

	private void CopyScaledAndTiledToAtlas(MB3_TextureCombiner.MB_TexSet texSet, MB3_TextureCombiner.MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, Rect rec, ShaderTextureProperty texturePropertyName, TextureBlender resultMatTexBlender)
	{
		Rect rect = rec;
		if (resultMatTexBlender != null)
		{
			this.myCamera.backgroundColor = resultMatTexBlender.GetColorIfNoTexture(texSet.matsAndGOs.mats[0].mat, texturePropertyName);
		}
		else
		{
			this.myCamera.backgroundColor = MB3_TextureCombiner.GetColorIfNoTexture(texturePropertyName);
		}
		if (source.t == null)
		{
			source.t = this.combiner._createTemporaryTexture(16, 16, 5, true);
		}
		rect.y = 1f - (rect.y + rect.height);
		rect.x *= (float)this._destinationTexture.width;
		rect.y *= (float)this._destinationTexture.height;
		rect.width *= (float)this._destinationTexture.width;
		rect.height *= (float)this._destinationTexture.height;
		Rect rect2 = rect;
		rect2.x -= (float)this._padding;
		rect2.y -= (float)this._padding;
		rect2.width += (float)(this._padding * 2);
		rect2.height += (float)(this._padding * 2);
		Rect rect3 = source.matTilingRect.GetRect();
		Rect rect4 = default(Rect);
		if (this._fixOutOfBoundsUVs)
		{
			Rect rect5;
			rect5..ctor(obUVoffset.x, obUVoffset.y, obUVscale.x, obUVscale.y);
			rect3 = MB3_UVTransformUtility.CombineTransforms(ref rect3, ref rect5);
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				Debug.Log("Fixing out of bounds UVs for tex " + source.t);
			}
		}
		Texture2D t = source.t;
		TextureWrapMode wrapMode = t.wrapMode;
		if (rect3.width == 1f && rect3.height == 1f && rect3.x == 0f && rect3.y == 0f)
		{
			t.wrapMode = 1;
		}
		else
		{
			t.wrapMode = 0;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			Debug.Log(string.Concat(new object[] { "DrawTexture tex=", t.name, " destRect=", rect, " srcRect=", rect3, " Mat=", this.mat }));
		}
		Rect rect6 = default(Rect);
		rect6.x = rect3.x;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = rect3.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x;
		rect4.y = rect2.y;
		rect4.width = rect.width;
		rect4.height = (float)this._padding;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = this._destinationTexture;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = rect3.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x;
		rect4.y = rect.y + rect.height;
		rect4.width = rect.width;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = rect3.height;
		rect4.x = rect2.x;
		rect4.y = rect.y;
		rect4.width = (float)this._padding;
		rect4.height = rect.height;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = rect3.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect.y;
		rect4.width = (float)this._padding;
		rect4.height = rect.height;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect2.x;
		rect4.y = rect2.y;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect2.y;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect2.x;
		rect4.y = rect.y + rect.height;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect.y + rect.height;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		Graphics.DrawTexture(rect, t, rect3, 0, 0, 0, 0, this.mat);
		RenderTexture.active = active;
		t.wrapMode = wrapMode;
	}

	private void _printTexture(Texture2D t)
	{
		if (t.width * t.height > 100)
		{
			Debug.Log("Not printing texture too large.");
		}
		try
		{
			Color32[] pixels = t.GetPixels32();
			string text = string.Empty;
			for (int i = 0; i < t.height; i++)
			{
				for (int j = 0; j < t.width; j++)
				{
					text = text + pixels[i * t.width + j] + ", ";
				}
				text += "\n";
			}
			Debug.Log(text);
		}
		catch (Exception ex)
		{
			Debug.Log("Could not print texture. texture may not be readable." + ex.ToString());
		}
	}

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private Material mat;

	private RenderTexture _destinationTexture;

	private Camera myCamera;

	private int _padding;

	private bool _isNormalMap;

	private bool _fixOutOfBoundsUVs;

	private bool _doRenderAtlas;

	private Rect[] rs;

	private List<MB3_TextureCombiner.MB_TexSet> textureSets;

	private int indexOfTexSetToRender;

	private ShaderTextureProperty _texPropertyName;

	private TextureBlender _resultMaterialTextureBlender;

	private Texture2D targTex;

	private MB3_TextureCombiner combiner;
}
