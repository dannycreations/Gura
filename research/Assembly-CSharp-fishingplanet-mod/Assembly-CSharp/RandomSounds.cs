using System;
using System.Collections;
using UnityEngine;

public class RandomSounds
{
	public RandomSounds(string folderName, MonoBehaviour mb)
	{
		this.audioClips = new ArrayList();
		for (int i = 1; i <= 20; i++)
		{
			string text = folderName + i;
			mb.StartCoroutine(this.LoadSounds(text));
		}
	}

	public static void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume)
	{
		if (clip != null)
		{
			LogHelper.Log("___kocha PlaySoundAtPoint name:{0} position:{1} volume:{2}", new object[] { clip.name, position, volume });
			RandomSounds.PlaySound(clip, position, volume);
		}
	}

	public static void PlaySoundAtPoint(string fullSoundName, Vector3 position, float volume, bool supressException = false)
	{
		AudioClip audioClip = Resources.Load(fullSoundName, typeof(AudioClip)) as AudioClip;
		if (!(audioClip == null))
		{
			RandomSounds.PlaySound(audioClip, position, volume);
			return;
		}
		if (supressException)
		{
			return;
		}
		throw new ArgumentException("Sound clip " + fullSoundName + " not found!");
	}

	private static void PlaySound(AudioClip clip, Vector3 position, float volume)
	{
		if (!GlobalConsts.InGameVolume)
		{
			return;
		}
		GameObject gameObject = new GameObject("One shot audio");
		gameObject.transform.position = position;
		gameObject.hideFlags = 61;
		AudioSource audioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
		if (Time.timeScale != 0f)
		{
			audioSource.pitch = Time.timeScale;
		}
		audioSource.priority = 1;
		audioSource.clip = clip;
		audioSource.spatialBlend = 1f;
		audioSource.minDistance = 0.1f;
		audioSource.volume = volume;
		audioSource.Play();
		Object.Destroy(gameObject, clip.length);
	}

	public AudioSource Source { get; private set; }

	private void PlaySound(AudioClip clip, float volume)
	{
		if (this.Source == null)
		{
			GameObject gameObject = new GameObject("One shot audio");
			this.Source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
		}
		if (Time.timeScale != 0f)
		{
			this.Source.pitch = Time.timeScale;
		}
		this.Source.priority = 1;
		this.Source.clip = clip;
		this.Source.volume = volume;
		this.Source.Play();
	}

	public IEnumerator LoadSounds(string path)
	{
		ResourceRequest request = Resources.LoadAsync<AudioClip>(path);
		yield return request;
		AudioClip clip = request.asset as AudioClip;
		if (clip)
		{
			this.audioClips.Add(clip);
		}
		yield break;
	}

	public AudioClip getRandomSound()
	{
		if (this.audioClips.Count == 0)
		{
			return null;
		}
		int num = Mathf.RoundToInt(Random.value * (float)(this.audioClips.Count - 1));
		return this.audioClips[num] as AudioClip;
	}

	public void playRandomSoundAtPoint(Vector3 position, float volume)
	{
		RandomSounds.PlaySound(this.getRandomSound(), position, volume);
	}

	public void playRandomSound(float volume)
	{
		this.PlaySound(this.getRandomSound(), volume);
	}

	public void StopSound()
	{
		if (this.Source != null)
		{
			this.Source.Stop();
			this.Source = null;
		}
	}

	public void SetMuteSound(bool mute)
	{
		if (this.Source != null)
		{
			this.Source.mute = mute;
		}
	}

	private ArrayList audioClips;
}
