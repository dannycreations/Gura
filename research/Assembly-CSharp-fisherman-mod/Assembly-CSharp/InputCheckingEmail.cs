using System;
using System.Text.RegularExpressions;

public class InputCheckingEmail : InputChecking
{
	protected override void Start()
	{
		base.Start();
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique += this.OnCheckEmailIsUnique;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique -= this.OnCheckEmailIsUnique;
	}

	protected override bool Checks(string text)
	{
		if (!string.IsNullOrEmpty(text) && text != "Name" && text != Localization.Get("Name") && InputCheckingEmail.rgx.IsMatch(text))
		{
			PhotonConnectionFactory.Instance.CheckEmailIsUnique(text);
		}
		return false;
	}

	private void OnCheckEmailIsUnique(bool unique)
	{
		if (unique)
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
		}
	}

	private static string pattern = "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z";

	protected static Regex rgx = new Regex(InputCheckingEmail.pattern, RegexOptions.IgnoreCase);
}
