using System;
using UnityEngine;

public class RandomSoundsComponent : MonoBehaviour
{
	public AudioClip getRandomSound()
	{
		if (this.AudioClips.Length == 0)
		{
			return null;
		}
		int num = Mathf.RoundToInt(Random.value * (float)(this.AudioClips.Length - 1));
		return this.AudioClips[num];
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public AudioClip[] AudioClips;
}
