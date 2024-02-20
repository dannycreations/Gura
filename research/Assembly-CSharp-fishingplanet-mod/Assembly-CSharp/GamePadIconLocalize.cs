using System;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePadIconLocalize : MonoBehaviour
{
	private void OnEnable()
	{
		string text;
		if (HotkeyIcons.KeyMappings.TryGetValue(this._localizationTerm, out text))
		{
			TextMeshProUGUI component = base.GetComponent<TextMeshProUGUI>();
			Text component2 = base.GetComponent<Text>();
			if (component != null)
			{
				component.text = text;
			}
			else if (component2 != null)
			{
				component2.text = text;
			}
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
