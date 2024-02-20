using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayButtonEffect : ActivityStateControlled, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler, IEventSystemHandler
{
	private void PlaySound(AudioClip clip, bool frameDependent = true)
	{
		if (UIAudioSourceListener.Instance == null || UIAudioSourceListener.Instance.Audio == null || !base.ShouldUpdate())
		{
			return;
		}
		if (frameDependent)
		{
			if (Math.Abs(Time.frameCount - PlayButtonEffect.frame) > 3)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(clip, SettingsManager.InterfaceVolume);
				PlayButtonEffect.frame = Time.frameCount;
			}
			return;
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(clip, SettingsManager.InterfaceVolume);
	}

	public void OnSelect(BaseEventData eventData = null)
	{
		if (this._hoverClip != null && !this._hovered && !PlayButtonEffect.Mute)
		{
			this._hovered = true;
			this.PlaySound(this._hoverClip, true);
		}
	}

	public void OnPointerEnter(PointerEventData eventData = null)
	{
		if (this._hoverClip != null && !this._hovered && !PlayButtonEffect.Mute)
		{
			this._hovered = true;
			this.PlaySound(this._hoverClip, true);
		}
	}

	public void OnSubmit()
	{
		if (this._submitClip != null && !PlayButtonEffect.Mute)
		{
			this.PlaySound(this._submitClip, true);
		}
	}

	public void OnSetToogle(bool isOn)
	{
		if (isOn && this._foldClip != null && !PlayButtonEffect.Mute)
		{
			this.PlaySound(this._foldClip, true);
		}
	}

	public void OnUnSetToogle(bool isOn)
	{
		if (!isOn && this._foldClip != null && !PlayButtonEffect.Mute)
		{
			this.PlaySound(this._foldClip, true);
		}
	}

	public void OnPointerExit(PointerEventData eventData = null)
	{
		this._hovered = false;
	}

	public void OnDeselect(BaseEventData eventData = null)
	{
		this._hovered = false;
	}

	public static void SetToogleOn(bool isOn, Toggle toggle)
	{
		bool mute = PlayButtonEffect.Mute;
		PlayButtonEffect.Mute = true;
		toggle.isOn = isOn;
		PlayButtonEffect.Mute = mute;
	}

	[SerializeField]
	private AudioClip _hoverClip;

	[SerializeField]
	private AudioClip _submitClip;

	[SerializeField]
	private AudioClip _foldClip;

	private bool _hovered;

	private static int frame = -1;

	public static bool Mute;
}
