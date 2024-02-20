using System;
using UnityEngine;

public class MovieTexturePlayer : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<AudioSource>().pitch = 0.5f;
		((MovieTexture)base.GetComponent<Renderer>().material.mainTexture).loop = true;
		((MovieTexture)base.GetComponent<Renderer>().material.mainTexture).Play();
		base.GetComponent<AudioSource>().Play();
	}

	public MovieTexture myMovieTex;

	public AudioSource myAudioSource;
}
