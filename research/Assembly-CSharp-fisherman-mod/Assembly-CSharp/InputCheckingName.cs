using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class InputCheckingName : InputCheckingSimpleText
{
	protected override void Start()
	{
		base.Start();
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique += this.OnCheckUsernameIsUnique;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique -= this.OnCheckUsernameIsUnique;
	}

	protected override bool Checks(string text)
	{
		if (this.Tooltip != null)
		{
			this.Tooltip.SetActive(text.Length < 3);
		}
		if (InputCheckingName.rgx.IsMatch(text) && !AbusiveWords.HasAbusiveWords(text))
		{
			PhotonConnectionFactory.Instance.CheckUsernameIsUnique(text);
		}
		return false;
	}

	internal void OnCheckUsernameIsUnique(bool unique)
	{
		if (unique)
		{
			this.SetCorrectName();
		}
		else
		{
			this.SetIncorrectName();
		}
	}

	private void SetIncorrectName()
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

	private void SetCorrectName()
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

	public GameObject Tooltip;

	private static string pattern = "^(?=.{3,30}$)(?![_.-])(?!.*[_.-]{2})[a-zA-Z0-9._-]+(?<![_.-])$";

	internal static Regex rgx = new Regex(InputCheckingName.pattern, RegexOptions.IgnoreCase);
}
