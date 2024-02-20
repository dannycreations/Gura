using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class BeautyShot : MonoBehaviour
{
	public string generateFilename()
	{
		int frameCount = Time.frameCount;
		return string.Format("/{0}.png", frameCount + this.frameOffset);
	}

	private IEnumerator GrabCoroutine()
	{
		yield return new WaitForEndOfFrame();
		string filename = this._folder + this.generateFilename();
		ScreenCapture.CaptureScreenshot(filename, (int)this.supersampleScreenshot);
		Debug.Log("File written");
		yield return null;
		yield break;
	}

	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Time.captureFramerate = this.frameRate;
		this.numFrames = this.duration * (float)this.frameRate;
		for (int i = 0; i < 640000; i++)
		{
			if (!Directory.Exists(this._folder))
			{
				break;
			}
		}
		Directory.CreateDirectory(this._folder);
		this._result = new Texture2D(Screen.width, Screen.height, 3, false);
	}

	private byte[] captureCam(Camera cam, int w, int h)
	{
		int cullingMask = cam.cullingMask;
		cam.cullingMask = this.layerMask;
		RenderTexture temporary = RenderTexture.GetTemporary(w, h);
		RenderTexture targetTexture = cam.targetTexture;
		cam.targetTexture = temporary;
		cam.Render();
		RenderTexture.active = temporary;
		this._result.ReadPixels(new Rect(0f, 0f, (float)w, (float)h), 0, 0, false);
		cam.targetTexture = targetTexture;
		cam.cullingMask = cullingMask;
		return ImageConversion.EncodeToPNG(this._result);
	}

	private void Update()
	{
		if (Input.GetKeyDown(this.GrabKey))
		{
			this.grabFlag = true;
		}
	}

	private void OnPostRender()
	{
		string text = this._folder + this.generateFilename();
		if (!this.captureUsingScreenshot)
		{
			Camera current = Camera.current;
			if (current != null)
			{
				File.WriteAllBytes(text, this.captureCam(current, Screen.width, Screen.height));
			}
			else
			{
				Debug.LogError("Cam is null?");
			}
		}
		else if (this.grabFlag)
		{
			this.grabFlag = false;
			base.StartCoroutine(this.GrabCoroutine());
		}
		if (Time.frameCount % this.frameRate != 0 || (float)Time.frameCount > this.numFrames)
		{
		}
	}

	private string _folder = "C:/ScreenShots/";

	private Texture2D _result;

	public LayerMask layerMask;

	public int frameRate = 60;

	public float duration = 10f;

	public int frameOffset;

	public KeyCode GrabKey = 293;

	private float numFrames;

	private bool grabFlag;

	public bool captureUsingScreenshot = true;

	public BeautyShot.Supersample supersampleScreenshot = BeautyShot.Supersample.Four;

	public enum Supersample
	{
		None = 1,
		Two,
		Four = 4,
		Eight = 8,
		Sixteen = 16,
		Wtf = 32
	}
}
