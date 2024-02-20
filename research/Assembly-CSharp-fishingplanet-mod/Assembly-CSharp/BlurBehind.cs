using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Blur Behind")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurBehind : MonoBehaviour
{
	public static void SetViewport()
	{
		Rect rect = Camera.current.rect;
		if (rect != new Rect(0f, 0f, 1f, 1f))
		{
			Vector2 vector;
			if (Camera.current.targetTexture == null)
			{
				vector..ctor((float)Screen.width, (float)Screen.height);
			}
			else
			{
				vector..ctor((float)Camera.current.targetTexture.width, (float)Camera.current.targetTexture.height);
			}
			rect.width = Mathf.Round(Mathf.Clamp01(rect.width + rect.x) * vector.x) / vector.x;
			rect.height = Mathf.Round(Mathf.Clamp01(rect.height + rect.y) * vector.y) / vector.y;
			rect.x = Mathf.Round(Mathf.Clamp01(rect.x) * vector.x) / vector.x;
			rect.y = Mathf.Round(Mathf.Clamp01(rect.y) * vector.y) / vector.y;
			rect.width -= rect.x;
			rect.height -= rect.y;
			Shader.SetGlobalVector("_BlurBehindRect", new Vector4((BlurBehind.storedRect.x - rect.x) / rect.width, BlurBehind.storedRect.y / rect.height + rect.y, BlurBehind.storedRect.width / rect.width, BlurBehind.storedRect.height / rect.height));
		}
	}

	public static void ResetViewport()
	{
		Shader.SetGlobalVector("_BlurBehindRect", new Vector4(BlurBehind.storedRect.x, BlurBehind.storedRect.y, BlurBehind.storedRect.width, BlurBehind.storedRect.height));
	}

	private void CheckSettings(int sourceSize)
	{
		if (this.radius < 0f)
		{
			this.radius = 0f;
		}
		if (this.downsample < 1f)
		{
			this.downsample = 1f;
		}
		if (this.iterations < 0)
		{
			this.iterations = 0;
		}
		if (this.settings != BlurBehind.Settings.Manual)
		{
			float num = ((this.settings != BlurBehind.Settings.Standard) ? 6f : 36f);
			if (this.mode == BlurBehind.Mode.Absolute)
			{
				if (this.radius > 0f)
				{
					if (this.radius < num)
					{
						this.iterations = 1;
					}
					else
					{
						this.iterations = Mathf.FloorToInt(Mathf.Log(this.radius, num)) + 1;
					}
					this.downsample = this.radius / Mathf.Pow(3f, (float)this.iterations);
					if (this.downsample < 1f)
					{
						this.downsample = 1f;
					}
				}
				else
				{
					this.downsample = 1f;
					this.iterations = 0;
				}
			}
			else if (this.radius > 0f)
			{
				float num2 = this.radius / 100f * (float)sourceSize;
				if (num2 < num)
				{
					this.iterations = 1;
				}
				else
				{
					this.iterations = Mathf.FloorToInt(Mathf.Log(num2, num)) + 1;
				}
				this.downsample = (float)sourceSize / (num2 / Mathf.Pow(3f, (float)this.iterations));
			}
			else
			{
				this.downsample = float.PositiveInfinity;
				this.iterations = 0;
			}
		}
	}

	private void CheckOutput(int rtW, int rtH, RenderTextureFormat format)
	{
		if (BlurBehind.storedTexture == null)
		{
			this.CreateOutput(rtW, rtH, format);
		}
		else if (BlurBehind.storedTexture.width != rtW || BlurBehind.storedTexture.height != rtH || BlurBehind.storedTexture.format != format)
		{
			BlurBehind.storedTexture.Release();
			Object.DestroyImmediate(BlurBehind.storedTexture);
			this.CreateOutput(rtW, rtH, format);
		}
		else
		{
			BlurBehind.storedTexture.DiscardContents();
		}
	}

	private bool CheckResources()
	{
		if (this.blurMaterial == null)
		{
			if (!(this.blurShader != null))
			{
				Debug.Log("Blur Behind UI: Shader reference missing");
				return false;
			}
			if (!this.blurShader.isSupported)
			{
				Debug.Log("Blur Behind UI: Shader not supported");
				return false;
			}
			this.blurMaterial = new Material(this.blurShader);
			this.blurMaterial.hideFlags = 52;
		}
		return true;
	}

	private bool CheckSupport()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.Log("Blur Behind UI: Image effects not supported");
			return false;
		}
		if (!SystemInfo.supportsRenderTextures)
		{
			Debug.Log("Blur Behind UI: Render textures not supported");
			return false;
		}
		return true;
	}

	private void CreateOutput(int width, int height, RenderTextureFormat format)
	{
		BlurBehind.storedTexture = new RenderTexture(width, height, 0, format);
		BlurBehind.storedTexture.filterMode = 1;
		BlurBehind.storedTexture.hideFlags = 52;
		Shader.SetGlobalTexture("_BlurBehindTex", BlurBehind.storedTexture);
		Shader.EnableKeyword("BLUR_BEHIND_SET");
	}

	private RenderTexture CropSource(RenderTexture source)
	{
		Rect rect;
		rect..ctor(this.cropRect.x * (float)source.width + this.pixelOffset.x, this.cropRect.y * (float)source.height + this.pixelOffset.y, this.cropRect.width * (float)source.width + this.pixelOffset.width, this.cropRect.height * (float)source.height + this.pixelOffset.height);
		rect.width = Mathf.Clamp01(Mathf.Round(rect.width + rect.x) / (float)source.width);
		rect.height = Mathf.Clamp01(Mathf.Round(rect.height + rect.y) / (float)source.height);
		rect.x = Mathf.Clamp01(Mathf.Round(rect.x) / (float)source.width);
		rect.y = Mathf.Clamp01(Mathf.Round(rect.y) / (float)source.height);
		rect.width -= rect.x;
		rect.height -= rect.y;
		RenderTexture renderTexture;
		if (rect != new Rect(0f, 0f, 1f, 1f))
		{
			renderTexture = RenderTexture.GetTemporary(Mathf.RoundToInt(rect.width * (float)source.width), Mathf.RoundToInt(rect.height * (float)source.height), 0, source.format);
			this.blurMaterial.SetVector("_Parameter", new Vector4(rect.x, rect.y, rect.width, rect.height));
			Graphics.Blit(source, renderTexture, this.blurMaterial, 2);
			BlurBehind.storedRect = rect;
		}
		else
		{
			renderTexture = source;
			BlurBehind.storedRect = new Rect(0f, 0f, 1f, 1f);
		}
		Rect rect2 = Camera.current.rect;
		if (rect2 != new Rect(0f, 0f, 1f, 1f))
		{
			Vector2 vector;
			if (Camera.current.targetTexture == null)
			{
				vector..ctor((float)Screen.width, (float)Screen.height);
			}
			else
			{
				vector..ctor((float)Camera.current.targetTexture.width, (float)Camera.current.targetTexture.height);
			}
			rect2.width = Mathf.Round(Mathf.Clamp01(rect2.width + rect2.x) * vector.x) / vector.x;
			rect2.height = Mathf.Round(Mathf.Clamp01(rect2.height + rect2.y) * vector.y) / vector.y;
			rect2.x = Mathf.Round(Mathf.Clamp01(rect2.x) * vector.x) / vector.x;
			rect2.y = Mathf.Round(Mathf.Clamp01(rect2.y) * vector.y) / vector.y;
			rect2.width -= rect2.x;
			rect2.height -= rect2.y;
			BlurBehind.storedRect = new Rect(rect2.x + BlurBehind.storedRect.x * rect2.width, rect2.y + BlurBehind.storedRect.y * rect2.height, rect2.width * BlurBehind.storedRect.width, rect2.height * BlurBehind.storedRect.height);
		}
		Shader.SetGlobalVector("_BlurBehindRect", new Vector4(BlurBehind.storedRect.x, BlurBehind.storedRect.y, BlurBehind.storedRect.width, BlurBehind.storedRect.height));
		return renderTexture;
	}

	private void Downsample(RenderTexture source, RenderTexture dest)
	{
		int i = 0;
		if (source.width > source.height)
		{
			for (int j = source.width; j > dest.width; j >>= 1)
			{
				i++;
			}
		}
		else
		{
			for (int k = source.height; k > dest.height; k >>= 1)
			{
				i++;
			}
		}
		if (i > 1)
		{
			RenderTexture renderTexture = source;
			while (i > 2)
			{
				int num = renderTexture.width >> 2;
				if (num < 1)
				{
					num = 1;
				}
				int num2 = renderTexture.height >> 2;
				if (num2 < 1)
				{
					num2 = 1;
				}
				renderTexture.filterMode = 1;
				RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, renderTexture.format);
				this.blurMaterial.SetVector("_Parameter", new Vector4(renderTexture.texelSize.x, renderTexture.texelSize.y, -renderTexture.texelSize.x, -renderTexture.texelSize.y));
				Graphics.Blit(renderTexture, temporary, this.blurMaterial, 1);
				if (renderTexture != source)
				{
					RenderTexture.ReleaseTemporary(renderTexture);
				}
				renderTexture = temporary;
				i -= 2;
			}
			if (i > 1)
			{
				this.blurMaterial.SetVector("_Parameter", new Vector4(renderTexture.texelSize.x, renderTexture.texelSize.y, -renderTexture.texelSize.x, -renderTexture.texelSize.y));
				Graphics.Blit(renderTexture, dest, this.blurMaterial, 1);
			}
			else
			{
				Graphics.Blit(renderTexture, dest);
			}
			if (renderTexture != source)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
		}
		else
		{
			Graphics.Blit(source, dest);
		}
	}

	private void OnDisable()
	{
		if (this.blurMaterial)
		{
			Object.Destroy(this.blurMaterial);
			this.blurMaterial = null;
		}
		BlurBehind.count--;
		if (BlurBehind.count == 0 && BlurBehind.storedTexture)
		{
			BlurBehind.storedTexture.Release();
			Object.Destroy(BlurBehind.storedTexture);
			BlurBehind.storedTexture = null;
			Shader.SetGlobalTexture("_BlurBehindTex", null);
			Shader.DisableKeyword("BLUR_BEHIND_SET");
		}
	}

	private void OnEnable()
	{
		BlurBehind.count++;
	}

	private void OnPreRender()
	{
		BlurBehind.SetViewport();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!this.CheckSupport() || !this.CheckResources())
		{
			base.enabled = false;
			Graphics.Blit(source, destination);
			return;
		}
		RenderTexture renderTexture = this.CropSource(source);
		int num = ((renderTexture.width <= renderTexture.height) ? renderTexture.height : renderTexture.width);
		this.CheckSettings(num);
		int num2;
		int num3;
		this.SetOutputSize(source, renderTexture, out num2, out num3);
		this.CheckOutput(num2, num3, renderTexture.format);
		this.Downsample(renderTexture, BlurBehind.storedTexture);
		if (renderTexture != source)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		if (this.iterations > 0 && this.radius > 0f)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, renderTexture.format);
			temporary.filterMode = 1;
			for (int i = 0; i < this.iterations; i++)
			{
				float num4 = this.radius / 300f * Mathf.Pow(3f, (float)i) / Mathf.Pow(3f, (float)(this.iterations - 1));
				if (this.mode == BlurBehind.Mode.Absolute)
				{
					num4 *= 100f / (float)num;
				}
				else if (renderTexture != source)
				{
					num4 *= (float)((source.width <= source.height) ? source.height : source.width) / (float)num;
				}
				float num5 = (float)i * 0.7853982f / (float)this.iterations;
				Vector2 vector = new Vector2(Mathf.Sin(num5), Mathf.Cos(num5)) * num4;
				Vector2 vector2 = ((num2 <= num3) ? new Vector2(1f / (float)num2 * (float)num3, 1f) : new Vector2(1f, 1f / (float)num3 * (float)num2));
				Vector4 vector3;
				vector3..ctor(vector.x * vector2.x, vector.y * vector2.y, 0f, 0f);
				vector3.z = -vector3.x;
				vector3.w = -vector3.y;
				this.blurMaterial.SetVector("_Parameter", vector3);
				Graphics.Blit(BlurBehind.storedTexture, temporary, this.blurMaterial, 0);
				BlurBehind.storedTexture.DiscardContents();
				vector3..ctor(vector.y * vector2.x, -vector.x * vector2.y, 0f, 0f);
				vector3.z = -vector3.x;
				vector3.w = -vector3.y;
				this.blurMaterial.SetVector("_Parameter", vector3);
				Graphics.Blit(temporary, BlurBehind.storedTexture, this.blurMaterial, 0);
				temporary.DiscardContents();
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
		Graphics.Blit(source, destination);
	}

	private void SetOutputSize(RenderTexture source, RenderTexture croppedSource, out int width, out int height)
	{
		float num;
		if (this.mode == BlurBehind.Mode.Absolute)
		{
			num = 1f / this.downsample;
		}
		else if (source.width > source.height)
		{
			if ((float)source.width > this.downsample)
			{
				num = this.downsample / (float)source.width;
			}
			else
			{
				num = 1f;
			}
		}
		else if ((float)source.height > this.downsample)
		{
			num = this.downsample / (float)source.height;
		}
		else
		{
			num = 1f;
		}
		width = Mathf.RoundToInt((float)croppedSource.width * num);
		height = Mathf.RoundToInt((float)croppedSource.height * num);
		if (width < 1)
		{
			width = 1;
		}
		else if (width > croppedSource.width)
		{
			width = croppedSource.width;
		}
		if (height < 1)
		{
			height = 1;
		}
		else if (height > croppedSource.height)
		{
			height = croppedSource.height;
		}
	}

	private static RenderTexture storedTexture = null;

	private static int count = 0;

	private static Rect storedRect = new Rect(0f, 0f, 1f, 1f);

	public Shader blurShader;

	private Material blurMaterial;

	public BlurBehind.Mode mode = BlurBehind.Mode.Relative;

	public float radius = 2.5f;

	public BlurBehind.Settings settings;

	public float downsample = 1f;

	public int iterations = 1;

	public Rect cropRect = new Rect(0f, 0f, 1f, 1f);

	public Rect pixelOffset = new Rect(0f, 0f, 0f, 0f);

	public enum Mode
	{
		Absolute,
		Relative
	}

	public enum Settings
	{
		Standard,
		Smooth,
		Manual
	}
}
