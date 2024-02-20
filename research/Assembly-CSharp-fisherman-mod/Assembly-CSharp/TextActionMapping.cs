using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TextActionMapping
{
	[SerializeField]
	public bool IsLongPressed;

	[SerializeField]
	public InputControlType Hotkey;

	[SerializeField]
	public Text text;

	[HideInInspector]
	public LinkedList<string> keysDisplayed = new LinkedList<string>();
}
