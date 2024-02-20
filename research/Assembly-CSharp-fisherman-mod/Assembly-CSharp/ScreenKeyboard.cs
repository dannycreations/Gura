using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenKeyboard : MonoBehaviour, IDeselectHandler, IEventSystemHandler
{
	private void Update()
	{
		if (this._waitTime > 0f)
		{
			this._waitTime -= Time.deltaTime;
			if (this._waitTime <= 0f)
			{
				this._waitTime = 0f;
			}
		}
	}

	public void OnInputFieldSelect(string name, bool textIsEmpty = false, ScreenKeyboard.VirtualKeyboardScope scope = ScreenKeyboard.VirtualKeyboardScope.Default)
	{
		string text = ((!(this.caption != null)) ? name : this.caption.text);
		this.VirtualKeyboard((!textIsEmpty) ? this.Text : string.Empty, text, string.Empty, scope);
	}

	public void OnInputFieldSelect()
	{
		string text = ((!(this.caption != null)) ? string.Empty : this.caption.text);
		this.VirtualKeyboard(this.Text, text, string.Empty, ScreenKeyboard.VirtualKeyboardScope.Default);
	}

	private void VirtualKeyboard(string DefaultInput, string Title = "", string Desc = "", ScreenKeyboard.VirtualKeyboardScope scope = ScreenKeyboard.VirtualKeyboardScope.Default)
	{
		ScreenKeyboard.IsOpened = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		ScreenKeyboard.IsOpened = false;
	}

	private void SetText(string t)
	{
		if (this.inputFieldPlayer != null)
		{
			this.inputFieldPlayer.text = t;
		}
		else if (this._inputFieldTMP != null)
		{
			this._inputFieldTMP.text = t;
		}
	}

	private string Text
	{
		get
		{
			if (this.inputFieldPlayer != null)
			{
				return this.inputFieldPlayer.text;
			}
			if (this._inputFieldTMP != null)
			{
				return this._inputFieldTMP.text;
			}
			return string.Empty;
		}
	}

	internal void OnDestroy()
	{
		ScreenKeyboard.IsOpened = false;
	}

	[SerializeField]
	private TMP_InputField _inputFieldTMP;

	public InputField inputFieldPlayer;

	public Text caption;

	public UnityEvent eventOnEnter;

	public UnityEvent closeKeyboard;

	public uint MaxTextLength = 1000U;

	[HideInInspector]
	public static bool IsOpened;

	private float _waitTime;

	public enum VirtualKeyboardScope
	{
		Default,
		EmailSmtpAddress,
		Number,
		Password,
		Search,
		TelephoneNumber,
		Url
	}
}
