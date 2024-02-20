using System;
using UnityEngine;

public class LanguageSelectionToggle : MonoBehaviour
{
	private void OnClick()
	{
		if (base.enabled)
		{
			Localization.language = this.LanguageName;
		}
	}

	public string LanguageName;
}
