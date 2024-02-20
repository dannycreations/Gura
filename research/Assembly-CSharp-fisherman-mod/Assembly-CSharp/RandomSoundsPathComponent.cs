using System;
using UnityEngine;

public class RandomSoundsPathComponent : MonoBehaviour
{
	public AudioClip getRandomSound()
	{
		return this.sounds.getRandomSound();
	}

	private void Start()
	{
		this.sounds = new RandomSounds(this.folderPath, this);
	}

	private void Update()
	{
	}

	public string folderPath = "Sounds/...";

	internal RandomSounds sounds;
}
