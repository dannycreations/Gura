using System;
using UnityEngine;

public class LightCookieAnimator : MonoBehaviour
{
	private void Start()
	{
		if (this.Lights.Length == 0)
		{
			Light component = base.GetComponent<Light>();
			if (component != null)
			{
				this.Lights = new Light[] { component };
			}
		}
		if (this.CookieFrames.Length == 0)
		{
			this.CookieFrames = this.LoadFrames(this.FramesResourcesPath);
		}
		this.NextFrame();
		base.InvokeRepeating("NextFrame", 1f / this.FPS, 1f / this.FPS);
	}

	private void NextFrame()
	{
		foreach (Light light in this.Lights)
		{
			light.cookie = this.CookieFrames[this.frameIndex];
		}
		this.frameIndex = (this.frameIndex + 1) % this.CookieFrames.Length;
	}

	private Texture2D[] LoadFrames(string texName)
	{
		return Resources.LoadAll<Texture2D>(texName);
	}

	public Light[] Lights;

	public Texture2D[] CookieFrames;

	public string FramesResourcesPath;

	public float FPS = 30f;

	private int frameIndex;
}
