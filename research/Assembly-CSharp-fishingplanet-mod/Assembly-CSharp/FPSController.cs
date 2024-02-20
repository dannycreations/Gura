using System;
using UnityEngine;

public class FPSController : MonoBehaviour
{
	private void Start()
	{
		this.lastInterval = (double)Time.realtimeSinceStartup;
		this.frames = 0;
		this.position.x = 10f;
		this.position.y = this.position.height - 10f;
	}

	private void Update()
	{
		this.frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if ((double)realtimeSinceStartup > this.lastInterval + (double)this.updateInterval)
		{
			FPSController.fps = (float)((double)this.frames / ((double)realtimeSinceStartup - this.lastInterval));
			this.frames = 0;
			this.lastInterval = (double)realtimeSinceStartup;
		}
	}

	public static float GetFps()
	{
		return FPSController.fps;
	}

	public float updateInterval = 0.5f;

	private double lastInterval;

	private int frames;

	private static float fps;

	private Rect position = new Rect(0f, 0f, 100f, 20f);
}
