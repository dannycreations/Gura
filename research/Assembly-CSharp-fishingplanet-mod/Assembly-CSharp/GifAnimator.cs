using System;
using UnityEngine;

public class GifAnimator : MonoBehaviour
{
	private void Awake()
	{
		this._renderer = base.GetComponent<Renderer>();
	}

	private void Update()
	{
		int num = (int)(Time.time * this._framesPerSecond % (float)this._frames.Length);
		this._renderer.material.mainTexture = this._frames[num];
	}

	[SerializeField]
	private Texture2D[] _frames;

	[SerializeField]
	private float _framesPerSecond = 10f;

	private Renderer _renderer;
}
