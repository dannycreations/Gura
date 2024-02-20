using System;
using UnityEngine;

public class UnluckFPS : MonoBehaviour
{
	public void Start()
	{
		this.timeleft = this.updateInterval;
		this._textMesh = base.transform.GetComponent<TextMesh>();
	}

	public void Update()
	{
		this.timeleft -= Time.deltaTime;
		this.accum += Time.timeScale / Time.deltaTime;
		this.frames++;
		if (this.timeleft <= 0f)
		{
			this._textMesh.text = "FPS " + (this.accum / (float)this.frames).ToString("f2");
			this.timeleft = this.updateInterval;
			this.accum = 0f;
			this.frames = 0;
		}
	}

	public TextMesh _textMesh;

	public float updateInterval = 0.5f;

	private float accum;

	private int frames;

	private float timeleft;
}
