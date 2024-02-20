using System;
using UnityEngine;

[Serializable]
public class FootstepsSounds
{
	public SurfaceMaterial Material
	{
		get
		{
			return this._material;
		}
	}

	public AudioClip[] Sounds
	{
		get
		{
			return this._sounds;
		}
	}

	public void PlayRandom(AudioSource audio)
	{
		int num = Random.Range(1, this._sounds.Length);
		audio.clip = this._sounds[num];
		audio.PlayOneShot(this._sounds[num]);
		this._sounds[num] = this._sounds[0];
		this._sounds[0] = audio.clip;
	}

	[SerializeField]
	private SurfaceMaterial _material;

	[SerializeField]
	private AudioClip[] _sounds;
}
