using System;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
	private void Start()
	{
		this.mTimer = Random.Range(0f, this.MaxWaitTime);
	}

	private void Update()
	{
		if (this.sounds != null)
		{
			this.mTimer -= Time.deltaTime;
			if (this.mTimer < 0f)
			{
				AudioClip randomSound = this.sounds.getRandomSound();
				if (randomSound != null)
				{
					if (Time.timeScale != 0f)
					{
						base.GetComponent<AudioSource>().pitch = Time.timeScale;
					}
					base.GetComponent<AudioSource>().PlayOneShot(randomSound);
				}
				this.generateWaitTime();
			}
		}
	}

	private void generateWaitTime()
	{
		this.mTimer = Random.Range(this.MinWaitTime, this.MaxWaitTime);
	}

	public float MinWaitTime = 1f;

	public float MaxWaitTime = 5f;

	public RandomSoundsPathComponent sounds;

	private float mTimer;
}
