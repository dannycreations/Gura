using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MoviePlayer : MonoBehaviour
{
	private void Start()
	{
		if (this.IsVideoBackground)
		{
			base.GetComponent<AudioSource>().Play();
			base.GetComponent<RawImage>().enabled = true;
			RawImage component = base.GetComponent<RawImage>();
			MovieTexture movieTexture = component.mainTexture as MovieTexture;
			if (movieTexture != null)
			{
				movieTexture.loop = true;
				movieTexture.Play();
			}
		}
	}

	private void OnDestroy()
	{
		if (this.IsVideoBackground)
		{
			RawImage component = base.GetComponent<RawImage>();
			Object.Destroy(component);
		}
	}

	public string psMoviePath;

	public Renderer psMovieRenderer;

	public Renderer xbMovieRenderer;

	public bool IsVideoBackground;
}
