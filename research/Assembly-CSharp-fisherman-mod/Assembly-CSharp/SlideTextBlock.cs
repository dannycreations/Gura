using System;
using InControl;
using UnityEngine;

[Serializable]
public class SlideTextBlock
{
	public string text;

	public Vector2 position;

	public Vector2 size;

	public InputControlType[] inputType;

	public Mouse[] MouseInputType;

	public Key[] KeyInputType;
}
