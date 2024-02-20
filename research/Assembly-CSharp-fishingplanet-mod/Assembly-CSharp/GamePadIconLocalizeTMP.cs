using System;
using InControl;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GamePadIconLocalizeTMP : MonoBehaviour
{
	private void OnEnable()
	{
		string text;
		if (HotkeyIcons.KeyMappings.TryGetValue(this._localizationTerm, out text))
		{
			base.GetComponent<TextMeshProUGUI>().text = text;
		}
	}

	public void SetTerm(InputControlType term)
	{
		this._localizationTerm = term;
		this.Localize();
	}

	public void Localize()
	{
		this.OnEnable();
	}

	[SerializeField]
	private InputControlType _localizationTerm;
}
