using System;
using UnityEngine;

public class UIAudioSourceListener : MonoBehaviour
{
	public static UIAudioSourceListener Instance { get; private set; }

	public AudioSource Audio { get; private set; }

	private void Awake()
	{
		if (UIAudioSourceListener.Instance != null)
		{
			Object.Destroy(UIAudioSourceListener.Instance.gameObject);
		}
		UIAudioSourceListener.Instance = this;
		Object.DontDestroyOnLoad(this);
		this.Audio = base.GetComponent<AudioSource>();
	}

	public void Fail()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
	}

	public void Successfull()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.RegistrationComplete, SettingsManager.InterfaceVolume);
	}

	public void ChatMessage()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.ChatMessageClip, SettingsManager.InterfaceVolume);
	}

	public void SportHorn()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.SportHornClip, SettingsManager.InterfaceVolume);
	}

	public void Coundown()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.CoundownClip, SettingsManager.InterfaceVolume);
	}

	public void LineCut()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.LineCutClip, SettingsManager.InterfaceVolume);
	}

	public void Slide()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.SlideClip, SettingsManager.InterfaceVolume);
	}

	public void Purcahse()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.PurcahseClip, SettingsManager.InterfaceVolume);
	}

	public AudioClip PopupClip;

	public AudioClip DragClip;

	public AudioClip DropClip;

	public AudioClip PurcahseClip;

	public AudioClip RegistrationComplete;

	public AudioClip ChatMessageClip;

	public AudioClip AccurateCastClip;

	public AudioClip InnacurateCastClip;

	public AudioClip ReelSpeedChangeClip;

	public AudioClip LineCutClip;

	public AudioClip CoundownClip;

	public AudioClip SportHornClip;

	public AudioClip WindowOpen;

	public AudioClip WindowClose;

	public AudioClip FailClip;

	public AudioClip SlideClip;
}
