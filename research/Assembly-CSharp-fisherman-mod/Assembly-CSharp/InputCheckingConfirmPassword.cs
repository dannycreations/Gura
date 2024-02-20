using System;
using UnityEngine.UI;

public class InputCheckingConfirmPassword : InputCheckingPassword
{
	protected override bool Checks(string text)
	{
		return !string.IsNullOrEmpty(text) && text != this.DefaultText && text != Localization.Get(this.DefaultText) && text == this.PasswordLabel.text;
	}

	public InputField PasswordLabel;
}
