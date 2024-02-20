using System;
using UnityEngine;

public class HelpWindowHandlerBase : MonoBehaviour
{
	protected virtual void Start()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected virtual void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	protected virtual void Update()
	{
		if (Input.anyKeyDown)
		{
			this.DestroyWindow();
		}
	}

	protected virtual void DestroyWindow()
	{
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.WindowClose, SettingsManager.InterfaceVolume);
		Object.Destroy(base.gameObject);
	}

	protected virtual void OnInputTypeChanged(InputModuleManager.InputType obj)
	{
		this.DestroyWindow();
	}
}
