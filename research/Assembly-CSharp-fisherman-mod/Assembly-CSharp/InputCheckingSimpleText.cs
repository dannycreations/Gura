using System;

public class InputCheckingSimpleText : InputChecking
{
	protected override bool Checks(string text)
	{
		return !string.IsNullOrEmpty(text) && text != this.DefaultText && text != Localization.Get(this.DefaultText);
	}

	public string DefaultText;
}
