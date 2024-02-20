using System;
using UnityEngine;
using UnityEngine.UI;

public class InputCheckingPassword : InputCheckingSimpleText
{
	protected override bool Checks(string text)
	{
		if (!string.IsNullOrEmpty(text) && this.oldPassword != null && text == this.oldPassword.text)
		{
			if (this.oldPasswordMatchMessage != null)
			{
				this.oldPasswordMatchMessage.SetActive(true);
			}
			return false;
		}
		if (!string.IsNullOrEmpty(text) && text != this.DefaultText && text != Localization.Get(this.DefaultText) && text.Length >= 6)
		{
			if (this.oldPasswordMatchMessage != null)
			{
				this.oldPasswordMatchMessage.SetActive(false);
			}
			return true;
		}
		return false;
	}

	public override void OnChange()
	{
		string text = this.Label.GetComponent<InputField>().text;
		if (this.Checks(text))
		{
			if (this.CheckIn != null)
			{
				GUITools.SetActive(this.CheckIn, true);
			}
			if (this.CheckOut != null)
			{
				GUITools.SetActive(this.CheckOut, false);
			}
			if (this.ImageObject != null)
			{
				this.ImageObject.color = this.CorrectColor;
			}
			if (this.Tooltip != null)
			{
				this.HideForm();
			}
			this.isCorrect = true;
		}
		else
		{
			if (this.CheckIn != null)
			{
				GUITools.SetActive(this.CheckIn, false);
			}
			if (this.CheckOut != null)
			{
				GUITools.SetActive(this.CheckOut, true);
			}
			if (this.ImageObject != null)
			{
				this.ImageObject.color = this.IncorrectColor;
			}
			this.isCorrect = false;
			if (this.Tooltip != null)
			{
				this.ShowForm();
			}
		}
	}

	private void ShowForm()
	{
		this.Tooltip.SetActive(true);
	}

	private void HideForm()
	{
		this.Tooltip.SetActive(false);
	}

	public GameObject Tooltip;

	public InputField oldPassword;

	public GameObject oldPasswordMatchMessage;
}
