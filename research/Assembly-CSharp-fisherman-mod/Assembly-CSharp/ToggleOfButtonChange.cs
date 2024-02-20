using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleOfButtonChange : MonoBehaviour
{
	private void Start()
	{
		this.Button.onClick.AddListener(delegate
		{
			this.Toggle.isOn = !this.Toggle.isOn;
		});
	}

	public Toggle Toggle;

	public Button Button;
}
