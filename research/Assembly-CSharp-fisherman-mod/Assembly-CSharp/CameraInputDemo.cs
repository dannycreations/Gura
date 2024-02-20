﻿using System;
using DG.Tweening;
using SWS;
using UnityEngine;

public class CameraInputDemo : MonoBehaviour
{
	private void Start()
	{
		this.myMove = base.gameObject.GetComponent<splineMove>();
		this.myMove.StartMove();
		this.myMove.Pause(0f);
	}

	private void Update()
	{
		if (this.myMove.tween == null || TweenExtensions.IsPlaying(this.myMove.tween))
		{
			return;
		}
		if (Input.GetKeyDown(273))
		{
			this.myMove.Resume();
		}
	}

	private void OnGUI()
	{
		if (this.myMove.tween != null && TweenExtensions.IsPlaying(this.myMove.tween))
		{
			return;
		}
		GUI.Box(new Rect((float)(Screen.width - 150), (float)(Screen.height / 2), 150f, 100f), string.Empty);
		Rect rect;
		rect..ctor((float)(Screen.width - 130), (float)(Screen.height / 2 + 10), 110f, 90f);
		GUI.Label(rect, this.infoText);
	}

	public void ShowInformation(string text)
	{
		this.infoText = text;
	}

	public string infoText = "Welcome to this customized input example";

	private splineMove myMove;
}
